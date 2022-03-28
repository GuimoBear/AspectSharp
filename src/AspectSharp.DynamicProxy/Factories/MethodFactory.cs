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
        private static readonly MethodInfo _createContextMethodInfo = typeof(IAspectContextFactory).GetMethod(nameof(IAspectContextFactory.CreateContext));
        private static readonly MethodInfo _executePipelineMethodInfo = typeof(ProxyFactoryUtils).GetMethod(nameof(ProxyFactoryUtils.ExecutePipeline));
        private static readonly MethodInfo _getReturnValueMethodInfo = typeof(AspectContext).GetProperty(nameof(AspectContext.ReturnValue)).GetGetMethod();

        private static readonly MethodInfo _getCompletedTaskMethodInfo = typeof(Task).GetProperty(nameof(Task.CompletedTask)).GetGetMethod();

        public static IEnumerable<MethodBuilder> CreateMethods(ModuleBuilder moduleBuilder, Type targetType, Type serviceType, TypeBuilder typeBuilder, FieldInfo[] fields, IReadOnlyDictionary<MethodInfo, PropertyInfo> pipelineProperties, IReadOnlyDictionary<MethodInfo, FieldInfo> contextActivatorFields)
        {
            var targetField = fields[0];
            var contextFactoryField = fields[1];

            var methods = serviceType.GetMethods();

            foreach (var interfaceMethodInfo in methods)
            {
                var methodInfo = targetType.GetMethod(interfaceMethodInfo.Name, interfaceMethodInfo.GetParameters().Select(pi => pi.ParameterType).ToArray());

                var methodName = methodInfo.Name;

                var returnInfo = methodInfo.GetReturnInfo();

                var parameters = methodInfo.GetParameters();
                var attrs = methodInfo.Attributes | MethodAttributes.Final;
                if (attrs.HasFlag(MethodAttributes.Abstract))
                    attrs ^= MethodAttributes.Abstract;
                if (attrs.HasFlag(MethodAttributes.Public))
                    attrs ^= MethodAttributes.Public;
                attrs |= MethodAttributes.Private;
                var methodBuilder = typeBuilder.DefineMethod(string.Format("{0}.{1}", serviceType.Name, methodInfo.Name), attrs, methodInfo.CallingConvention, methodInfo.ReturnType, parameters.Select(p => p.ParameterType).ToArray());

                var hasParameters = parameters.Length > 0;
                foreach (var tuple in parameters.Zip(Enumerable.Range(1, parameters.Length), (first, second) => new Tuple<ParameterInfo, int>(first, second)))
                {
                    var parameter = tuple.Item1;
                    var idx = tuple.Item2;
                    methodBuilder.DefineParameter(idx, ParameterAttributes.None, parameter.Name);
                }

                ILGenerator cil;
                List<LocalBuilder> localVariables;
                LocalBuilder valueTaskLocal = default;

                if (pipelineProperties.TryGetValue(interfaceMethodInfo, out var pipelineProperty) &&
                    contextActivatorFields.TryGetValue(interfaceMethodInfo, out var aspectContextActivatorField))
                {
                    if (returnInfo.IsAsync)
                    {
                        var ret = ProxyMethodAsyncStateMachineFactory.GenerateAsyncStateMachine(moduleBuilder, typeBuilder, targetType, pipelineProperty, aspectContextActivatorField, methodInfo, methodBuilder);
                        ret.WriteCallerMethod(methodBuilder, targetField, contextFactoryField);

                        typeBuilder.DefineMethodOverride(methodBuilder, interfaceMethodInfo);
                        yield return methodBuilder;
                    }
                    else
                    {
                        cil = methodBuilder.GetILGenerator();
                        localVariables = new List<LocalBuilder>();
#if NETCOREAPP3_1_OR_GREATER
                        if (returnInfo.IsValueTask && returnInfo.IsVoid)
                            localVariables.Add(valueTaskLocal = cil.DeclareLocal(typeof(ValueTask)));
#endif
                        if (hasParameters)
                        {
                            localVariables.Add(cil.DeclareLocal(typeof(object[])));
                            cil.Emit(OpCodes.Ldc_I4, parameters.Length);
                            cil.Emit(OpCodes.Newarr, typeof(object));
                            foreach (var tuple in parameters.Zip(Enumerable.Range(1, parameters.Length), (first, second) => new Tuple<ParameterInfo, int>(first, second)))
                            {
                                var parameter = tuple.Item1;
                                var index = tuple.Item2;
                                cil.Emit(OpCodes.Dup);
                                cil.Emit(OpCodes.Ldc_I4, index - 1);
                                cil.Emit(OpCodes.Ldarg, index);
                                if (parameter.ParameterType.IsValueType)
                                    cil.Emit(OpCodes.Box, parameter.ParameterType);
                                cil.Emit(OpCodes.Stelem_Ref);
                            }
                            cil.Emit(OpCodes.Stloc, localVariables.Count - 1);
                        }
                        cil.Emit(OpCodes.Ldarg_0);
                        cil.Emit(OpCodes.Ldfld, contextFactoryField);
                        cil.Emit(OpCodes.Ldsfld, aspectContextActivatorField);
                        cil.Emit(OpCodes.Ldarg_0);
                        cil.Emit(OpCodes.Ldfld, targetField);
                        cil.Emit(OpCodes.Ldarg_0);
                        if (hasParameters)
                            cil.Emit(OpCodes.Ldloc, localVariables.Count - 1);
                        else
                            cil.Emit(OpCodes.Ldsfld, _emptyParameterArrayFieldInfo);
                        cil.Emit(OpCodes.Callvirt, _createContextMethodInfo);
                        if (!returnInfo.IsVoid)
                            cil.Emit(OpCodes.Dup);
                        cil.Emit(OpCodes.Call, pipelineProperty.GetMethod);
                        cil.Emit(OpCodes.Call, _executePipelineMethodInfo);
                        cil.Emit(OpCodes.Callvirt, _waitTaskMethodInfo);
                        if (!returnInfo.IsVoid)
                        {
                            cil.Emit(OpCodes.Callvirt, _getReturnValueMethodInfo);
                            if (returnInfo.Type.IsValueType)
                                cil.Emit(OpCodes.Unbox_Any, returnInfo.Type);
                            else
                                cil.Emit(OpCodes.Castclass, returnInfo.Type);
                            if (returnInfo.IsAsync)
                            {
#if NETCOREAPP3_1_OR_GREATER
                            if (returnInfo.IsValueTask)
                                cil.Emit(OpCodes.Newobj, typeof(ValueTask<>).MakeGenericType(returnInfo.Type).GetConstructor(new Type[] { returnInfo.Type }));
                            else
#endif
                                cil.Emit(OpCodes.Call, typeof(Task).GetMethod(nameof(Task<int>.FromResult)).MakeGenericMethod(returnInfo.Type));
                            }
                        }
                        else if (returnInfo.IsAsync
#if NETCOREAPP3_1_OR_GREATER
                        && !returnInfo.IsValueTask
#endif
                        )
                            cil.Emit(OpCodes.Call, _getCompletedTaskMethodInfo);
#if NETCOREAPP3_1_OR_GREATER
                    else if (returnInfo.IsValueTask)
                    {
                        cil.Emit(OpCodes.Ldloca_S, 0);
                        cil.Emit(OpCodes.Initobj, typeof(ValueTask));
                        cil.Emit(OpCodes.Ldloc_0);
                    }
#endif
                        cil.Emit(OpCodes.Ret);
                    }
                    typeBuilder.DefineMethodOverride(methodBuilder, interfaceMethodInfo);
                    yield return methodBuilder;
                    continue;
                }

                cil = methodBuilder.GetILGenerator();
                localVariables = new List<LocalBuilder>();
#if NETCOREAPP3_1_OR_GREATER
                if (returnInfo.IsValueTask && returnInfo.IsVoid)
                    localVariables.Add(valueTaskLocal = cil.DeclareLocal(typeof(ValueTask)));
#endif
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
