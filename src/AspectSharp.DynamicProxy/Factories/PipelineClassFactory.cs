using AspectSharp.Abstractions;
using AspectSharp.Abstractions.Attributes;
using AspectSharp.DynamicProxy.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using static AspectSharp.DynamicProxy.Utils.InterceptorTypeCache;

namespace AspectSharp.DynamicProxy.Factories
{
    internal static class PipelineClassFactory
    {
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

        public static IReadOnlyDictionary<MethodInfo, PropertyInfo> CreatePipelineClass(Type targetType, Type serviceType, TypeBuilder typeBuilder, InterceptedTypeData interceptedTypeData, DynamicProxyFactoryConfigurations configs)
        {
            var attrs = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
            var staticAttrs = MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
            var constructorBuilder = typeBuilder.DefineConstructor(attrs, CallingConventions.HasThis, Type.EmptyTypes);
            var staticConstructorBuilder = typeBuilder.DefineConstructor(staticAttrs, CallingConventions.Standard, Type.EmptyTypes);
            var cil = constructorBuilder.GetILGenerator();
            cil.DeclareLocal(typeof(AbstractInterceptorAttribute[]));

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Call, _objectConstructorMethodInfo);

            var staticCil = staticConstructorBuilder.GetILGenerator();
            var localServiceType = staticCil.DeclareLocal(typeof(Type));

            staticCil.Emit(OpCodes.Ldtoken, serviceType);
            staticCil.Emit(OpCodes.Call, _getTypeFromHandleMethodInfo);
            staticCil.Emit(OpCodes.Stloc_0, localServiceType);

            var methods = serviceType.GetMethodsRecursively();
            var index = 1;

            var ret = new Dictionary<MethodInfo, PropertyInfo>();
            foreach (var interfaceMethodInfo in methods)
            {
                var methodInfo = targetType.GetMethod(interfaceMethodInfo);
                if (interceptedTypeData.TryGetMethodInterceptorAttributes(interfaceMethodInfo, out _) ||
                    interceptedTypeData.TryGetMethodGlobalInterceptors(interfaceMethodInfo, out _))
                {
                    var propertyBuilder = CreateAspectDelegateAndPipelineProperty(typeBuilder, staticCil, interfaceMethodInfo, methodInfo, configs, index);
                    ret.Add(interfaceMethodInfo, propertyBuilder);
                    index++;
                }
            }
            cil.Emit(OpCodes.Ret);
            staticCil.Emit(OpCodes.Ret);
            return ret;
        }

        private static PropertyBuilder CreateAspectDelegateAndPipelineProperty(TypeBuilder typeBuilder, ILGenerator staticCil, MethodInfo interfaceMethodInfo, MethodInfo methodInfo, DynamicProxyFactoryConfigurations configs, int index)
        {
            var returnInfo = methodInfo.GetReturnInfo();

            var pipelineField = typeBuilder.DefineField(string.Format("_pipeline{0}", index), _interceptDelegateType, FieldAttributes.Private | FieldAttributes.Static | FieldAttributes.InitOnly);

            var methodBuilder = typeBuilder.DefineMethod(string.Format("get_Pipeline{0}", index), MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.SpecialName, CallingConventions.Standard, _interceptDelegateType, null);
            var cil = methodBuilder.GetILGenerator();

            cil.Emit(OpCodes.Ldsfld, pipelineField);
            cil.Emit(OpCodes.Ret);

            var pipelineProperty = typeBuilder.DefineProperty(string.Format("Pipeline{0}", index), PropertyAttributes.None, CallingConventions.Standard, _interceptDelegateType, Array.Empty<Type>());
            pipelineProperty.SetGetMethod(methodBuilder);

            methodBuilder = typeBuilder.DefineMethod(string.Format("AspectDelegate{0}", index), MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Static, CallingConventions.Standard, typeof(Task), new Type[] { typeof(AspectContext) });
            methodBuilder.DefineParameter(1, ParameterAttributes.None, "context");

            cil = methodBuilder.GetILGenerator();
            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Castclass, typeof(IMethodInvoker));
            cil.Emit(OpCodes.Callvirt, typeof(IMethodInvoker).GetMethod(nameof(IMethodInvoker.InvokeAsync)));
            cil.Emit(OpCodes.Ret);

            //if (returnInfo.IsAsync)
            //{
            //    var ret = AspectDelegateAsyncStateMachineFactory.GenerateAsyncStateMachine(moduleBuilder, methodInfo, methodBuilder);
            //    ret.WriteCallerMethod();
            //}
            //else
            //{
            //    cil = methodBuilder.GetILGenerator();

            //    var localVariables = new List<LocalBuilder>();
            //    var parameters = methodInfo.GetParameters();
            //    foreach (var param in parameters)
            //        localVariables.Add(cil.DeclareLocal(param.ParameterType.IsByRef && param.ParameterType.IsAutoLayout && param.ParameterType.Name.EndsWith("&") ? param.ParameterType.GetElementType() : param.ParameterType));

            //    foreach (var i in Enumerable.Range(0, parameters.Length))
            //    {
            //        var param = parameters[i];
            //        if (param.IsOut)
            //            continue;
            //        cil.Emit(OpCodes.Ldarg_0);
            //        cil.Emit(OpCodes.Callvirt, _getParametersMethodInfo);
            //        cil.Emit(OpCodes.Ldc_I4, i);
            //        cil.Emit(OpCodes.Ldelem_Ref);
            //        if (param.ParameterType.IsValueType)
            //            cil.Emit(OpCodes.Unbox_Any, param.ParameterType);
            //        else if (param.ParameterType.IsByRef && param.ParameterType.IsAutoLayout && param.ParameterType.Name.EndsWith("&"))
            //        {
            //            var innerType = param.ParameterType.GetElementType();
            //            if (innerType.IsValueType)
            //                cil.Emit(OpCodes.Unbox_Any, innerType);
            //            else
            //                cil.Emit(OpCodes.Castclass, innerType);
            //        }
            //        else
            //            cil.Emit(OpCodes.Castclass, param.ParameterType);
            //        cil.Emit(OpCodes.Stloc, i);
            //    }

            //    if (!returnInfo.IsVoid)
            //        cil.Emit(OpCodes.Ldarg_0);

            //    cil.Emit(OpCodes.Ldarg_0);
            //    cil.Emit(OpCodes.Callvirt, _getTargetMethodInfo);
            //    cil.Emit(OpCodes.Castclass, methodInfo.DeclaringType);

            //    foreach (var i in Enumerable.Range(0, parameters.Length))
            //    {
            //        var param = parameters[i];
            //        if (param.ParameterType.IsByRef && param.ParameterType.IsAutoLayout && param.ParameterType.Name.EndsWith("&"))
            //            cil.Emit(OpCodes.Ldloca_S, i);
            //        else
            //            cil.Emit(OpCodes.Ldloc, i);
            //    }
            //    cil.Emit(OpCodes.Callvirt, methodInfo);

            //    if (!returnInfo.IsVoid)
            //    {
            //        if (returnInfo.Type.IsValueType)
            //            cil.Emit(OpCodes.Box, returnInfo.Type);
            //        cil.Emit(OpCodes.Callvirt, _setReturnValueMethodInfo);
            //    }

            //    cil.Emit(OpCodes.Ldarg_0);
            //    cil.Emit(OpCodes.Ldc_I4_1);
            //    cil.Emit(OpCodes.Callvirt, _setTargetMethodCalledMethodInfo);

            //    foreach (var i in Enumerable.Range(0, parameters.Length))
            //    {
            //        var param = parameters[i];
            //        if (param.ParameterType.IsByRef && param.ParameterType.IsAutoLayout && param.ParameterType.Name.EndsWith("&"))
            //        {
            //            cil.Emit(OpCodes.Ldarg_0);
            //            cil.Emit(OpCodes.Callvirt, _getParametersMethodInfo);
            //            cil.Emit(OpCodes.Ldc_I4, i);
            //            cil.Emit(OpCodes.Ldloc, i);
            //            var innerType = param.ParameterType.GetElementType();
            //            if (innerType.IsValueType)
            //                cil.Emit(OpCodes.Box, innerType);
            //            cil.Emit(OpCodes.Stelem_Ref);
            //        }
            //    }
            //    cil.Emit(OpCodes.Call, _getCompletedTaskMethodInfo);
            //    cil.Emit(OpCodes.Ret);
            //}

            staticCil.Emit(OpCodes.Ldnull);
            staticCil.Emit(OpCodes.Ldftn, methodBuilder);
            staticCil.Emit(OpCodes.Newobj, _aspectDelegateConstructorInfo);

            staticCil.Emit(OpCodes.Ldloc_0);
            staticCil.Emit(OpCodes.Ldc_I4, configs.GetHashCode());
            staticCil.Emit(OpCodes.Ldc_I4, interfaceMethodInfo.GetHashCode());
            staticCil.Emit(OpCodes.Call, _getInterceptorsMethodInfo);
            staticCil.Emit(OpCodes.Call, _createPipelineMethodInfo);
            staticCil.Emit(OpCodes.Stsfld, pipelineField);

            return pipelineProperty;
        }
    }
}
