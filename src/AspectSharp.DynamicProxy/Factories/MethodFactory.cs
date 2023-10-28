using AspectSharp.Abstractions;
using AspectSharp.DynamicProxy.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace AspectSharp.DynamicProxy.Factories
{
    internal static class MethodFactory
    {
        private static readonly MethodInfo _waitTaskMethodInfo = typeof(Task).GetMethod(nameof(Task.Wait), Type.EmptyTypes);
        private static readonly FieldInfo _emptyParameterArrayFieldInfo = typeof(ProxyFactoryUtils).GetField(nameof(ProxyFactoryUtils.EmptyParameters));
        private static readonly MethodInfo _setLastContextMethodInfo = typeof(IAspectContextFactory).GetProperty(nameof(IAspectContextFactory.CurrentContext)).GetSetMethod();
        
        private static readonly MethodInfo _getServicesMethodInfo = typeof(IAspectContextFactory).GetProperty(nameof(IAspectContextFactory.Services)).GetGetMethod();

        private static readonly MethodInfo _executePipelineMethodInfo = typeof(ProxyFactoryUtils).GetMethod(nameof(ProxyFactoryUtils.ExecutePipeline));
        private static readonly MethodInfo _getParametersMethodInfo = typeof(AspectContext).GetProperty(nameof(AspectContext.Parameters)).GetGetMethod();
        private static readonly MethodInfo _getReturnValueMethodInfo = typeof(AspectContext).GetProperty(nameof(AspectContext.ReturnValue)).GetGetMethod();

        private static readonly MethodInfo _getCompletedTaskMethodInfo = typeof(Task).GetProperty(nameof(Task.CompletedTask)).GetGetMethod();

        public static IEnumerable<MethodBuilder> CreateMethods(ModuleBuilder moduleBuilder, Type targetType, Type serviceType, TypeBuilder typeBuilder, FieldInfo[] fields, IReadOnlyDictionary<MethodInfo, PropertyInfo> pipelineProperties, IReadOnlyDictionary<MethodInfo, FieldInfo> contextActivatorFields)
        {
            var targetField = fields[0];
            var contextFactoryField = fields[1];

            var methods = serviceType.GetMethodsRecursively();

            foreach (var interfaceMethodInfo in methods)
            {
                var methodInfo = targetType.GetMethod(interfaceMethodInfo);

                var customAspectTuple = CustomAspectContextFactory.CreateCustomAspectContext(methodInfo, moduleBuilder);
                var customAspectContext = customAspectTuple.Item1;
                var contextConstructor = customAspectTuple.Item2;

                var methodName = methodInfo.Name;

                var returnInfo = methodInfo.GetReturnInfo();

                var parameters = methodInfo.GetParameters();
                var attrs = methodInfo.Attributes | MethodAttributes.Final;
                if (attrs.HasFlag(MethodAttributes.Abstract))
                    attrs ^= MethodAttributes.Abstract;
                if (attrs.HasFlag(MethodAttributes.Public))
                    attrs ^= MethodAttributes.Public;
                attrs |= MethodAttributes.Private;
                var methodBuilder = typeBuilder.DefineMethod(string.Format("{0}.{1}", serviceType.Name, methodInfo.Name), attrs, methodInfo.CallingConvention);
                var methodGenericParameters = GenericParameterUtils.DefineGenericParameter(methodInfo, methodBuilder);
                methodBuilder.SetParameters(methodInfo.GetParameters().Select(pi => GenericParameterUtils.ReplaceTypeUsingGenericParameters(pi.ParameterType, methodGenericParameters)).ToArray());
                methodBuilder.SetReturnType(GenericParameterUtils.ReplaceTypeUsingGenericParameters(methodInfo.ReturnType, methodGenericParameters));

                if (methodInfo.ReturnType.ContainsGenericParameters)
                    returnInfo = methodBuilder.GetReturnInfo();

                var hasParameters = parameters.Length > 0;
                foreach (var tuple in parameters.Zip(Enumerable.Range(1, parameters.Length), (first, second) => new Tuple<ParameterInfo, int>(first, second)))
                {
                    var parameter = tuple.Item1;
                    var idx = tuple.Item2;
                    methodBuilder.DefineParameter(parameter.Position, parameter.Attributes, parameter.Name);
                }

                ILGenerator cil;
                List<LocalBuilder> localVariables;

                if (pipelineProperties.TryGetValue(interfaceMethodInfo, out var pipelineProperty) &&
                    contextActivatorFields.TryGetValue(interfaceMethodInfo, out var aspectContextActivatorField))
                {
                    if (returnInfo.IsAsync)
                    {
                        var ret = ProxyMethodAsyncStateMachineFactory.GenerateAsyncStateMachine(moduleBuilder, typeBuilder, targetType, pipelineProperty, aspectContextActivatorField, methodInfo, methodBuilder, customAspectContext, contextConstructor);
                        ret.WriteCallerMethod(methodBuilder, targetField, contextFactoryField);

                        typeBuilder.DefineMethodOverride(methodBuilder, interfaceMethodInfo);
                        yield return methodBuilder;
                    }
                    else
                    {
                        bool hasRefOrOutParameters = false;
                        cil = methodBuilder.GetILGenerator();
                        if (hasParameters)
                        {
                            cil.DeclareLocal(typeof(object[]));
                            cil.Emit(OpCodes.Ldc_I4, parameters.Length);
                            cil.Emit(OpCodes.Newarr, typeof(object));
                            foreach (var tuple in parameters.Zip(Enumerable.Range(1, parameters.Length), (first, second) => new Tuple<ParameterInfo, int>(first, second)))
                            {
                                var parameter = tuple.Item1;
                                if (parameter.IsOut)
                                {
                                    hasRefOrOutParameters = true;
                                    continue;
                                }
                                var index = tuple.Item2;
                                cil.Emit(OpCodes.Dup);
                                cil.Emit(OpCodes.Ldc_I4, index - 1);
                                cil.Emit(OpCodes.Ldarg, index);
                                if (parameter.ParameterType.IsAutoLayout && parameter.ParameterType.Name.EndsWith("&"))
                                {
                                    hasRefOrOutParameters = true;
                                    parameter.ParameterType.GetElementType().EmitOptionalLoadOpCode(cil);
                                    cil.Emit(OpCodes.Box, parameter.ParameterType.GetElementType());
                                }
                                else if (parameter.ParameterType.IsValueType || parameter.ParameterType.ContainsGenericParameters)
                                    cil.Emit(OpCodes.Box, parameter.ParameterType);
                                cil.Emit(OpCodes.Stelem_Ref);
                            }
                            cil.Emit(OpCodes.Stloc_0);
                        }
                        if (customAspectContext.IsGenericType)
                        {
                            customAspectContext = customAspectContext.MakeGenericType(methodGenericParameters);
                            contextConstructor = TypeBuilder.GetConstructor(customAspectContext, contextConstructor);
                        }

                        var contextVariable = cil.DeclareLocal(customAspectContext);
                        //cil.Emit(OpCodes.Ldarg_0);
                        //cil.Emit(OpCodes.Ldfld, contextFactoryField);
                        cil.Emit(OpCodes.Ldsfld, aspectContextActivatorField);
                        cil.Emit(OpCodes.Ldarg_0);
                        cil.Emit(OpCodes.Ldfld, targetField);
                        cil.Emit(OpCodes.Ldarg_0);
                        if (hasParameters)
                            cil.Emit(OpCodes.Ldloc_0);
                        else
                            cil.Emit(OpCodes.Ldsfld, _emptyParameterArrayFieldInfo);

                        cil.Emit(OpCodes.Ldarg_0);
                        cil.Emit(OpCodes.Ldfld, contextFactoryField);
                        cil.Emit(OpCodes.Callvirt, _getServicesMethodInfo);
                        cil.Emit(OpCodes.Newobj, contextConstructor);
                        cil.Emit(OpCodes.Stloc, contextVariable.LocalIndex);





                        //cil.Emit(OpCodes.Callvirt, _createContextMethodInfo);

                        cil.Emit(OpCodes.Ldarg_0);
                        cil.Emit(OpCodes.Ldfld, contextFactoryField);
                        cil.Emit(OpCodes.Ldloc, contextVariable.LocalIndex);
                        cil.Emit(OpCodes.Callvirt, _setLastContextMethodInfo);


                        cil.Emit(OpCodes.Ldloc, contextVariable.LocalIndex);
                        cil.Emit(OpCodes.Call, pipelineProperty.GetMethod);
                        cil.Emit(OpCodes.Call, _executePipelineMethodInfo);
                        cil.Emit(OpCodes.Callvirt, _waitTaskMethodInfo);

                        if (hasRefOrOutParameters)
                        {
                            //foreach (var tuple in parameters.Zip(Enumerable.Range(1, parameters.Length), (first, second) => new Tuple<ParameterInfo, int>(first, second)))
                            foreach (var tuple in parameters.Select((parameter, idx) => new Tuple<ParameterInfo, int>(parameter, idx + 1)))
                            {
                                var parameter = tuple.Item1;
                                if (parameter.ParameterType.IsAutoLayout && parameter.ParameterType.Name.EndsWith("&"))
                                {
                                    var index = tuple.Item2;
                                    cil.Emit(OpCodes.Ldarg, index);
                                    cil.Emit(OpCodes.Ldloc, contextVariable.LocalIndex);
                                    cil.Emit(OpCodes.Callvirt, _getParametersMethodInfo);
                                    cil.Emit(OpCodes.Ldc_I4, index - 1);
                                    cil.Emit(OpCodes.Ldelem_Ref);
                                    var innerType = parameter.ParameterType.GetElementType();
                                    if (innerType.IsValueType || innerType.ContainsGenericParameters)
                                        cil.Emit(OpCodes.Unbox_Any, innerType);
                                    else
                                        cil.Emit(OpCodes.Castclass, innerType);
                                    innerType.EmitOptionalSetOpCode(cil);
                                }
                            }
                        }

                        if (!returnInfo.IsVoid)
                        {
                            cil.Emit(OpCodes.Ldloc, contextVariable.LocalIndex);
                            cil.Emit(OpCodes.Callvirt, _getReturnValueMethodInfo);
                            if (returnInfo.Type.IsValueType || returnInfo.Type.ContainsGenericParameters)
                                cil.Emit(OpCodes.Unbox_Any, returnInfo.Type);
                            else
                                cil.Emit(OpCodes.Castclass, returnInfo.Type);
                        }
                        cil.Emit(OpCodes.Ret);
                    }
                    typeBuilder.DefineMethodOverride(methodBuilder, interfaceMethodInfo);
                    yield return methodBuilder;
                    continue;
                }

                cil = methodBuilder.GetILGenerator();
                localVariables = new List<LocalBuilder>();
                cil.Emit(OpCodes.Ldarg_0);
                cil.Emit(OpCodes.Ldfld, targetField);
                if (hasParameters)
                {
                    var idx = 1;
                    foreach (var parameter in parameters)
                        cil.Emit(OpCodes.Ldarg, idx++);
                }
                cil.Emit(OpCodes.Callvirt, methodInfo);
                cil.Emit(OpCodes.Ret);
                typeBuilder.DefineMethodOverride(methodBuilder, interfaceMethodInfo);
                yield return methodBuilder;
            }
        }
    }
}
