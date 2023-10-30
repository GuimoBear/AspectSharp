using AspectSharp.Abstractions;
using AspectSharp.Abstractions.Attributes;
using AspectSharp.DynamicProxy.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AspectSharp.DynamicProxy.Factories
{
    internal static class CustomAspectContextFactory
    {
        private static readonly Type PARENT_TYPE = typeof(ConcreteAspectContext);
        private const MethodAttributes CONSTRUCTOR_ATTRIBUTES = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
        public static readonly Type[] CONSTRUCTOR_PARAMETERS = new Type[] { typeof(AspectContextActivator), typeof(object), typeof(object), typeof(object[]), typeof(IServiceProvider) };
        private static readonly ConstructorInfo PARENT_CONSTRUCTOR = PARENT_TYPE.GetConstructor(CONSTRUCTOR_PARAMETERS);

        private const string METHOD_NAME = nameof(IMethodInvoker.InvokeAsync);
        private const MethodAttributes METHOD_ATTRIBUTES = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;

        private static readonly Type METHOD_INVOKER_INTERFACE_TYPE = typeof(IMethodInvoker);

        private static readonly IDictionary<Tuple<string, string, int>, Type> _cachedProxyTypes
            = new Dictionary<Tuple<string, string, int>, Type>();

        public static Tuple<Type, ConstructorInfo> CreateCustomAspectContext(MethodInfo interfaceMethodInfo, MethodInfo targetMethodInfo, ModuleBuilder moduleBuilder) 
        {
            lock (_cachedProxyTypes)
            {
                var methodToCall = interfaceMethodInfo;

                var previouslyDefinedProxyClassFromThisTargetCount = _cachedProxyTypes.Count(kvp => kvp.Key.Item1 == interfaceMethodInfo.DeclaringType.Name && kvp.Key.Item2 == interfaceMethodInfo.Name);
                TypeBuilder typeBuilder;
                if (previouslyDefinedProxyClassFromThisTargetCount == 0)
                    typeBuilder = moduleBuilder.DefineType(string.Format("AspectSharp.CustomContexts.{0}.{1}Context", interfaceMethodInfo.DeclaringType.Name, interfaceMethodInfo.Name), TypeAttributes.Public | TypeAttributes.Sealed, PARENT_TYPE);
                else
                    typeBuilder = moduleBuilder.DefineType(string.Format("AspectSharp.CustomContexts.{0}.{1}Context{2}", interfaceMethodInfo.DeclaringType.Name, interfaceMethodInfo.Name, previouslyDefinedProxyClassFromThisTargetCount), TypeAttributes.Public | TypeAttributes.Sealed, PARENT_TYPE);
                var genericParameterBuilders = Array.Empty<GenericTypeParameterBuilder>();

                if (interfaceMethodInfo.IsGenericMethod)
                {
                    var genericParameters = interfaceMethodInfo.GetGenericArguments().Select(a => a.GetTypeInfo()).ToArray();

                    genericParameterBuilders = typeBuilder.DefineGenericParameters(genericParameters.Select(ti => ti.Name).ToArray());

                    for (int i = 0; i < genericParameters.Length; i++)
                    {
                        var genericParameter = genericParameters[i];
                        var genericParameterBuilder = genericParameterBuilders[i];
                        genericParameterBuilder.SetGenericParameterAttributes(GenericParameterUtils.ToClassGenericParameterAttributes(genericParameter.GenericParameterAttributes));
                        foreach (var constraint in genericParameter.GetGenericParameterConstraints().Select(t => t.GetTypeInfo()))
                        {
                            if (constraint.IsClass) genericParameterBuilder.SetBaseTypeConstraint(constraint.AsType());
                            if (constraint.IsInterface) genericParameterBuilder.SetInterfaceConstraints(constraint.AsType());
                        }
                    }

                    methodToCall = interfaceMethodInfo.IsGenericMethodDefinition
                        ? interfaceMethodInfo.MakeGenericMethod(genericParameterBuilders)
                        : interfaceMethodInfo.GetGenericMethodDefinition().MakeGenericMethod(genericParameterBuilders);
                }
                var constructorBuilder = typeBuilder.DefineConstructor(CONSTRUCTOR_ATTRIBUTES, CallingConventions.HasThis, CONSTRUCTOR_PARAMETERS);

                var cil = constructorBuilder.GetILGenerator();

                cil.Emit(OpCodes.Ldarg_0);
                cil.Emit(OpCodes.Ldarg_1);
                cil.Emit(OpCodes.Ldarg_2);
                cil.Emit(OpCodes.Ldarg_3);
                cil.Emit(OpCodes.Ldarg, 4);
                cil.Emit(OpCodes.Ldarg, 5);
                cil.Emit(OpCodes.Call, PARENT_CONSTRUCTOR);
                cil.Emit(OpCodes.Ret);

                var methodBuilder = typeBuilder.DefineMethod(METHOD_NAME, METHOD_ATTRIBUTES, CallingConventions.HasThis, typeof(Task), Array.Empty<Type>());

                var returnInfo = methodToCall.GetReturnInfo();

                if (targetMethodInfo.GetReturnInfo().IsAsync)
                {
                    var ret = AspectDelegateAsyncStateMachineFactory.GenerateAsyncStateMachine(moduleBuilder, interfaceMethodInfo, methodBuilder, genericParameterBuilders);
                    ret.WriteCallerMethod();
                }
                else
                {
                    cil = methodBuilder.GetILGenerator();

                    var parameters = interfaceMethodInfo.GetParameters();
                    var parameterTypes = new List<Type>(parameters.Length);

                    foreach (var param in parameters)
                    {
                        var parameterType = GenericParameterUtils.ReplaceTypeUsingGenericParameters(param.ParameterType, genericParameterBuilders);
                        parameterTypes.Add(parameterType);
                        cil.DeclareLocal(parameterType.IsAutoLayout && parameterType.Name.EndsWith("&") ? parameterType.GetElementType() : parameterType);
                    }

                    foreach (var i in Enumerable.Range(0, parameters.Length))
                    {
                        var param = parameters[i];
                        var parameterType = parameterTypes[i];
                        if (param.IsOut)
                            continue;
                        cil.Emit(OpCodes.Ldarg_0);
                        cil.Emit(OpCodes.Callvirt, _getParametersMethodInfo);
                        cil.Emit(OpCodes.Ldc_I4, i);
                        cil.Emit(OpCodes.Ldelem_Ref); 
                        if (parameterType.IsAutoLayout && parameterType.Name.EndsWith("&"))
                        {
                            var innerType = parameterType.GetElementType();
                            if (innerType.IsValueType || innerType.ContainsGenericParameters)
                                cil.Emit(OpCodes.Unbox_Any, innerType);
                            else
                                cil.Emit(OpCodes.Castclass, innerType);
                        } 
                        else if (parameterType.IsValueType || parameterType.ContainsGenericParameters)
                            cil.Emit(OpCodes.Unbox_Any, parameterType);
                        else
                            cil.Emit(OpCodes.Castclass, parameterType);
                        cil.Emit(OpCodes.Stloc, i);
                    }

                    if (!returnInfo.IsVoid)
                        cil.Emit(OpCodes.Ldarg_0);

                    cil.Emit(OpCodes.Ldarg_0);
                    cil.Emit(OpCodes.Callvirt, _getTargetMethodInfo);
                    cil.Emit(OpCodes.Isinst, methodToCall.DeclaringType);

                    foreach (var i in Enumerable.Range(0, parameters.Length))
                    {
                        var param = parameters[i];
                        var parameterType = parameterTypes[i];
                        if (parameterType.IsAutoLayout && parameterType.Name.EndsWith("&"))
                            cil.Emit(OpCodes.Ldloca_S, i);
                        else
                            cil.Emit(OpCodes.Ldloc, i);
                    }

                    //if (!methodToCall.ContainsGenericParameters)
                        cil.Emit(OpCodes.Callvirt, methodToCall);
                    //else
                    //    cil.Emit(OpCodes.Call, methodToCall);

                    if (!returnInfo.IsVoid)
                    {
                        if (returnInfo.Type.IsValueType || returnInfo.Type.ContainsGenericParameters)
                            cil.Emit(OpCodes.Box, GenericParameterUtils.ReplaceTypeUsingGenericParameters(returnInfo.Type, genericParameterBuilders));
                        cil.Emit(OpCodes.Callvirt, _setReturnValueMethodInfo);
                    }

                    cil.Emit(OpCodes.Ldarg_0);
                    cil.Emit(OpCodes.Ldc_I4_1);
                    cil.Emit(OpCodes.Callvirt, _setTargetMethodCalledMethodInfo);

                    foreach (var i in Enumerable.Range(0, parameters.Length))
                    {
                        var param = parameters[i];
                        var parameterType = parameterTypes[i];
                        if (parameterType.IsAutoLayout && parameterType.Name.EndsWith("&"))
                        {
                            cil.Emit(OpCodes.Ldarg_0);
                            cil.Emit(OpCodes.Callvirt, _getParametersMethodInfo);
                            cil.Emit(OpCodes.Ldc_I4, i);
                            cil.Emit(OpCodes.Ldloc, i);
                            var innerType = parameterType.GetElementType();
                            if (innerType.IsValueType || innerType.ContainsGenericParameters)
                                cil.Emit(OpCodes.Box, innerType);
                            cil.Emit(OpCodes.Stelem_Ref);
                        }
                    }
                    cil.Emit(OpCodes.Call, _getCompletedTaskMethodInfo);
                    cil.Emit(OpCodes.Ret);
                }

                typeBuilder.AddInterfaceImplementation(METHOD_INVOKER_INTERFACE_TYPE);

                var concreteType = typeBuilder.CreateTypeInfo().AsType();
                var constructor = concreteType.GetConstructor(CONSTRUCTOR_PARAMETERS);

                _cachedProxyTypes.Add(new Tuple<string, string, int>(interfaceMethodInfo.DeclaringType.Name, interfaceMethodInfo.Name, concreteType.GetHashCode()), concreteType);

                return new Tuple<Type, ConstructorInfo>(concreteType, constructor);
            }
        }

        private static readonly ConstructorInfo _objectConstructorMethodInfo = typeof(object).GetConstructor(Array.Empty<Type>());

        private static readonly MethodInfo _getTypeFromHandleMethodInfo = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), new Type[] { typeof(RuntimeTypeHandle) });

        private static readonly MethodInfo _getTargetMethodInfo = typeof(AspectContext).GetProperty(nameof(AspectContext.Target)).GetGetMethod();
        private static readonly MethodInfo _getParametersMethodInfo = typeof(AspectContext).GetProperty(nameof(AspectContext.Parameters)).GetGetMethod();
        private static readonly MethodInfo _waitTaskMethodInfo = typeof(Task).GetMethod(nameof(Task.Wait), Array.Empty<Type>());
        private static readonly MethodInfo _setReturnValueMethodInfo = typeof(AspectContext).GetProperty(nameof(AspectContext.ReturnValue)).GetSetMethod();

        private static readonly MethodInfo _setTargetMethodCalledMethodInfo = typeof(AspectContext).GetProperty(nameof(AspectContext.TargetMethodCalled)).GetSetMethod(true);
#if NETCOREAPP3_1_OR_GREATER
        private static readonly MethodInfo _asTaskValueTaskMethodInfo = typeof(ValueTask).GetMethod(nameof(ValueTask.AsTask));
#endif
        private static readonly MethodInfo _getCompletedTaskMethodInfo = typeof(Task).GetProperty(nameof(Task.CompletedTask)).GetGetMethod();
        private static readonly Type _exceptionType = typeof(Exception);

        private static readonly Type _interceptDelegateType = typeof(InterceptDelegate);

        private static readonly MethodInfo _createPipelineMethodInfo = typeof(ProxyFactoryUtils).GetMethod(nameof(ProxyFactoryUtils.CreatePipeline), new Type[] { typeof(AspectDelegate), typeof(AbstractInterceptorAttribute[]) });
        private static readonly MethodInfo _getInterceptorsMethodInfo = typeof(ProxyFactoryUtils).GetMethod(nameof(ProxyFactoryUtils.GetInterceptors), new Type[] { typeof(Type), typeof(int), typeof(int) });

        private static readonly ConstructorInfo _aspectDelegateConstructorInfo = typeof(AspectDelegate).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) });

        private static readonly ParameterInfo[] _aspectDelegateParameters = typeof(AspectDelegate).GetMethod(nameof(AspectDelegate.Invoke)).GetParameters();
        private static readonly Type _asyncStateMachineType = typeof(IAsyncStateMachine);

    }
}
