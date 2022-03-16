using AspectSharp.Abstractions;
using AspectSharp.Abstractions.Attributes;
using AspectSharp.DynamicProxy.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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
#if NETCOREAPP3_1_OR_GREATER
        private static readonly MethodInfo _asTaskValueTaskMethodInfo = typeof(ValueTask).GetMethod(nameof(ValueTask.AsTask));
#endif
        private static readonly MethodInfo _getCompletedTaskMethodInfo = typeof(Task).GetProperty(nameof(Task.CompletedTask)).GetGetMethod();

        private static readonly Type _interceptDelegateType = typeof(InterceptDelegate);
        private static readonly Type _abstractInterceptorAttributeType = typeof(AbstractInterceptorAttribute);

        private static readonly MethodInfo _createPipelineMethodInfo = typeof(ProxyFactoryUtils).GetMethod(nameof(ProxyFactoryUtils.CreatePipeline), new Type[] { typeof(AspectDelegate), typeof(AbstractInterceptorAttribute[]) });
        private static readonly MethodInfo _getInterceptorsMethodInfo = typeof(ProxyFactoryUtils).GetMethod(nameof(ProxyFactoryUtils.GetInterceptors), new Type[] { typeof(Type), typeof(int), typeof(int) });

        private static readonly ConstructorInfo _aspectDelegateConstructorInfo = typeof(AspectDelegate).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) });

        public static IReadOnlyDictionary<MethodInfo, PropertyInfo> CreatePipelineClass(Type serviceType, TypeBuilder typeBuilder, InterceptedTypeData interceptedTypeData, DynamicProxyFactoryConfigurations configs)
        {
            var attrs = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
            var staticAttrs = MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
            var constructorBuilder = typeBuilder.DefineConstructor(attrs, CallingConventions.Standard | CallingConventions.HasThis, Type.EmptyTypes);
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

            var methods = serviceType.GetMethods();
            var index = 1;

            var ret = new Dictionary<MethodInfo, PropertyInfo>();
            foreach (var methodInfo in methods)
            {
                if (interceptedTypeData.TryGetMethodInterceptors(methodInfo, out _))
                {
                    var propertyBuilder = CreateAspectDelegateAndPipelineProperty(typeBuilder, staticCil, methodInfo, configs, index);

                    ret.Add(methodInfo, propertyBuilder);

                    index++;
                }
            }
            cil.Emit(OpCodes.Ret);
            staticCil.Emit(OpCodes.Ret);

            return ret;
        }

        private static PropertyBuilder CreateAspectDelegateAndPipelineProperty(TypeBuilder typeBuilder, ILGenerator staticCil, MethodInfo methodInfo, DynamicProxyFactoryConfigurations configs, int index)
        {
            var returnInfo = methodInfo.GetReturnInfo();

            var pipelineField = typeBuilder.DefineField(string.Format("_pipeline{0}", index), _interceptDelegateType, FieldAttributes.Private | FieldAttributes.Static | FieldAttributes.InitOnly);

            var methodBuilder = typeBuilder.DefineMethod(string.Format("get_Pipeline{0}", index), MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.SpecialName, CallingConventions.Standard, _interceptDelegateType, null);
            var cil = methodBuilder.GetILGenerator();

            cil.Emit(OpCodes.Ldsfld, pipelineField);
            cil.Emit(OpCodes.Ret);

            var pipelineProperty = typeBuilder.DefineProperty(string.Format("Pipeline{0}", index), PropertyAttributes.None, CallingConventions.Standard, _interceptDelegateType, Array.Empty<Type>());
            pipelineProperty.SetGetMethod(methodBuilder);

            methodBuilder = typeBuilder.DefineMethod(string.Format("AspectDelegate{0}", index), MethodAttributes.Private | MethodAttributes.Static, CallingConventions.Standard, typeof(Task), new Type[] { typeof(AspectContext) });
            methodBuilder.DefineParameter(1, ParameterAttributes.None, "context");

            cil = methodBuilder.GetILGenerator();

            if (!returnInfo.IsVoid)
                cil.DeclareLocal(methodInfo.DeclaringType);
            var parameters = methodInfo.GetParameters();
            foreach (var param in parameters)
                cil.DeclareLocal(param.ParameterType);
#if NETCOREAPP3_1_OR_GREATER
            if (returnInfo.IsValueTask)
                cil.DeclareLocal(methodInfo.ReturnType);
#endif

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Callvirt, _getTargetMethodInfo);
            cil.Emit(OpCodes.Isinst, methodInfo.DeclaringType);
            if (!returnInfo.IsVoid)
                cil.Emit(OpCodes.Stloc_0);

            foreach (var i in Enumerable.Range(returnInfo.IsVoid ? 0 : 1, parameters.Length))
            {
                cil.Emit(OpCodes.Ldarg_0);
                cil.Emit(OpCodes.Callvirt, _getParametersMethodInfo);
                cil.Emit(OpCodes.Ldc_I4, returnInfo.IsVoid ? i : i - 1);
                cil.Emit(OpCodes.Ldelem_Ref);
                var param = parameters[returnInfo.IsVoid ? i : i - 1];
                if (param.ParameterType.IsValueType)
                    cil.Emit(OpCodes.Unbox_Any, param.ParameterType);
                else
                    cil.Emit(OpCodes.Castclass, param.ParameterType);
                cil.Emit(OpCodes.Stloc, i);
            }

            if (!returnInfo.IsVoid)
            {
                cil.Emit(OpCodes.Ldarg_0);
                cil.Emit(OpCodes.Ldloc_0);
            }
            foreach (var i in Enumerable.Range(returnInfo.IsVoid ? 0 : 1, parameters.Length))
                cil.Emit(OpCodes.Ldloc, i);
            cil.Emit(OpCodes.Callvirt, methodInfo);

#if NETCOREAPP3_1_OR_GREATER
            if (returnInfo.IsValueTask)
            {
                cil.Emit(OpCodes.Stloc_S, parameters.Length + (returnInfo.IsVoid ? 0 : 1));
                cil.Emit(OpCodes.Ldloca_S, parameters.Length + (returnInfo.IsVoid ? 0 : 1));
                if (returnInfo.IsVoid)
                    cil.Emit(OpCodes.Call, _asTaskValueTaskMethodInfo);
                else
                    cil.Emit(OpCodes.Call, typeof(ValueTask<>).MakeGenericType(returnInfo.Type).GetConstructor(new Type[] { returnInfo.Type }));
            }
            else
#endif
            if (returnInfo.IsAsync && !returnInfo.IsVoid)
                cil.Emit(OpCodes.Callvirt, typeof(Task<>).MakeGenericType(returnInfo.Type).GetProperty(nameof(Task<int>.Result)).GetGetMethod());
            if (!returnInfo.IsVoid)
            {
                if (returnInfo.Type.IsValueType)
                    cil.Emit(OpCodes.Box, returnInfo.Type);
                cil.Emit(OpCodes.Callvirt, _setReturnValueMethodInfo);
                cil.Emit(OpCodes.Call, _getCompletedTaskMethodInfo);
            }
            else if (!returnInfo.IsAsync)
                cil.Emit(OpCodes.Call, _getCompletedTaskMethodInfo);
            cil.Emit(OpCodes.Ret);

            staticCil.Emit(OpCodes.Ldnull);
            staticCil.Emit(OpCodes.Ldftn, methodBuilder);
            staticCil.Emit(OpCodes.Newobj, _aspectDelegateConstructorInfo);

            staticCil.Emit(OpCodes.Ldloc_0);
            staticCil.Emit(OpCodes.Ldc_I4, configs.GetHashCode());
            staticCil.Emit(OpCodes.Ldc_I4, methodInfo.GetHashCode());
            staticCil.Emit(OpCodes.Call, _getInterceptorsMethodInfo);
            staticCil.Emit(OpCodes.Call, _createPipelineMethodInfo);
            staticCil.Emit(OpCodes.Stsfld, pipelineField);

            return pipelineProperty;
        }

        private static PropertyBuilder CreatePipelineProperty(TypeBuilder typeBuilder, ILGenerator constructorIlGenerator, FieldInfo aspectDelegateField, FieldInfo aspectsFromDelegateField, int index)
        {
            var pipelineField = typeBuilder.DefineField(string.Format("_pipeline{0}", index), _interceptDelegateType, FieldAttributes.Private | FieldAttributes.InitOnly);

            var methodBuilder = typeBuilder.DefineMethod(string.Format("get_pipeline{0}", index), MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.SpecialName, CallingConventions.Standard | CallingConventions.HasThis, _interceptDelegateType, null);
            var cil = methodBuilder.GetILGenerator();

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldfld, pipelineField);
            cil.Emit(OpCodes.Ret);

            var property = typeBuilder.DefineProperty(string.Format("Pipeline{0}", index), PropertyAttributes.None, _interceptDelegateType, Array.Empty<Type>());
            property.SetGetMethod(methodBuilder);

            constructorIlGenerator.Emit(OpCodes.Ldarg_0);
            constructorIlGenerator.Emit(OpCodes.Ldsfld, aspectDelegateField);
            constructorIlGenerator.Emit(OpCodes.Ldsfld, aspectsFromDelegateField);
            constructorIlGenerator.Emit(OpCodes.Call, _createPipelineMethodInfo);
            constructorIlGenerator.Emit(OpCodes.Stfld, pipelineField);

            return property;
        }
    }
}
