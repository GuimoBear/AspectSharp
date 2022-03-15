using AspectSharp.Abstractions;
using AspectSharp.DynamicProxy;
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

        private static readonly MethodInfo _getCompletedValueTaskMethodInfo = typeof(ValueTask).GetProperty(nameof(ValueTask.CompletedTask)).GetGetMethod();
        private static readonly MethodInfo _getCompletedTaskMethodInfo = typeof(Task).GetProperty(nameof(Task.CompletedTask)).GetGetMethod();

        public static IEnumerable<MethodBuilder> CreateMethods(Type serviceType, TypeBuilder typeBuilder, FieldInfo[] fields, IReadOnlyDictionary<MethodInfo, PropertyInfo> pipelineProperties, IReadOnlyDictionary<MethodInfo, FieldInfo> contextActivatorFields)
        {
            var targetField = fields[0];
            var pipelinesField = fields[1];
            var contextFactoryField = fields[2];

            var methods = serviceType.GetMethods();
            foreach (var methodInfo in methods)
            {
                var methodName = methodInfo.Name;

                var retType = methodInfo.ReturnType;
                var isValueTask = retType == typeof(ValueTask);
                var isAsync = retType == typeof(Task) || isValueTask;

                var isVoid = retType == typeof(void) || isValueTask || isAsync;

                if (retType.IsGenericType)
                {
                    var genericTypeDefinition = retType.GetGenericTypeDefinition();
                    isValueTask = genericTypeDefinition == typeof(ValueTask<>);
                    isAsync = genericTypeDefinition == typeof(Task<>) || isValueTask;
                    if (isAsync)
                        retType = retType.GetGenericArguments()[0];
                }

                var parameters = methodInfo.GetParameters();
                var methodBuilder = typeBuilder.DefineMethod(methodInfo.Name, methodInfo.Attributes ^ MethodAttributes.Abstract, methodInfo.CallingConvention, methodInfo.ReturnType, parameters.Select(p => p.ParameterType).ToArray());
                var hasParameters = parameters.Length > 0;
                foreach (var tuple in parameters.Zip(Enumerable.Range(1, parameters.Length), (first, second) => new Tuple<ParameterInfo, int>(first, second)))
                {
                    var parameter = tuple.Item1;
                    var idx = tuple.Item2;
                    methodBuilder.DefineParameter(idx, ParameterAttributes.None, parameter.Name);
                }

                var cil = methodBuilder.GetILGenerator();

                if (pipelineProperties.TryGetValue(methodInfo, out var pipelineProperty) &&
                    contextActivatorFields.TryGetValue(methodInfo, out var aspectContextActivatorField))
                {
                    if (hasParameters)
                    {
                        cil.DeclareLocal(typeof(object[]));
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
                        cil.Emit(OpCodes.Stloc_0);
                    }
                    cil.Emit(OpCodes.Ldarg_0);
                    cil.Emit(OpCodes.Ldfld, contextFactoryField);
                    cil.Emit(OpCodes.Ldsfld, aspectContextActivatorField);
                    cil.Emit(OpCodes.Ldarg_0);
                    cil.Emit(OpCodes.Ldfld, targetField);
                    cil.Emit(OpCodes.Ldarg_0);
                    if (hasParameters)
                        cil.Emit(OpCodes.Ldloc_0);
                    else
                        cil.Emit(OpCodes.Ldsfld, _emptyParameterArrayFieldInfo);
                    cil.Emit(OpCodes.Callvirt, _createContextMethodInfo);
                    if (!isVoid)
                        cil.Emit(OpCodes.Dup);
                    cil.Emit(OpCodes.Ldsfld, pipelinesField);
                    cil.Emit(OpCodes.Callvirt, pipelineProperty.GetMethod);
                    cil.Emit(OpCodes.Call, _executePipelineMethodInfo);
                    cil.Emit(OpCodes.Callvirt, _waitTaskMethodInfo);
                    if (!isVoid)
                    {
                        cil.Emit(OpCodes.Callvirt, _getReturnValueMethodInfo);
                        if (retType.IsValueType)
                            cil.Emit(OpCodes.Unbox_Any, retType);
                        else
                            cil.Emit(OpCodes.Castclass, retType);
                        if (isAsync)
                        {
                            if (isValueTask)
                                cil.Emit(OpCodes.Call, typeof(ValueTask).GetMethod(nameof(ValueTask.FromResult)).MakeGenericMethod(retType));
                            else
                                cil.Emit(OpCodes.Call, typeof(Task).GetMethod(nameof(Task<int>.FromResult)).MakeGenericMethod(retType));
                        }
                    }
                    else if (isAsync)
                        cil.Emit(OpCodes.Call, _getCompletedTaskMethodInfo);
                    cil.Emit(OpCodes.Ret);
                    yield return methodBuilder;
                    continue;
                }
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
                yield return methodBuilder;
            }
        }
    }
}
