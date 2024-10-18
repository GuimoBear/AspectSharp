using AspectSharp.Abstractions;
using AspectSharp.DynamicProxy.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static AspectSharp.DynamicProxy.Utils.InterceptorTypeCache;

namespace AspectSharp.DynamicProxy.Factories
{
    internal static class AspectActivatorFieldFactory
    {
        private static readonly MethodInfo _getTypeFromHandleMethodInfo = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), new Type[] { typeof(RuntimeTypeHandle) });
        private static readonly MethodInfo _newContextActivatorMethodInfo = typeof(ProxyFactoryUtils).GetMethod(nameof(ProxyFactoryUtils.NewContextActivator), new Type[] { typeof(Type), typeof(Type), typeof(Type), typeof(string), typeof(Type[]), typeof(string[]), typeof(bool[]) });
        private static readonly ConstructorInfo _aspectDelegateConstructorInfo = typeof(AspectDelegate).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) });
        private static readonly MethodInfo _newContextActivatorUsingStringRepresentationMethodInfo = typeof(ProxyFactoryUtils).GetMethod(nameof(ProxyFactoryUtils.NewContextActivatorUsingStringRepresentationMethodInfo), new Type[] { typeof(Type), typeof(Type), typeof(Type), typeof(string), typeof(int), typeof(AspectDelegate) });

        public static IReadOnlyDictionary<MethodInfo, FieldInfo> CreateStaticFields(TypeBuilder typeBuilder, Type serviceType, Type targetType, InterceptedTypeData interceptedTypeData, IReadOnlyDictionary<MethodInfo, MethodInfo> pipelineProperties, DynamicProxyFactoryConfigurations configs)
        {
            var attrs = MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
            var staticConstructor = typeBuilder.DefineConstructor(attrs, CallingConventions.Standard, null);
            var cil = staticConstructor.GetILGenerator();

            cil.DeclareLocal(typeof(Type));
            cil.DeclareLocal(typeof(Type));
            cil.DeclareLocal(typeof(Type));

            cil.Emit(OpCodes.Ldtoken, serviceType);
            cil.Emit(OpCodes.Call, _getTypeFromHandleMethodInfo);
            cil.Emit(OpCodes.Stloc_0);

            cil.Emit(OpCodes.Ldtoken, typeBuilder.GetType());
            cil.Emit(OpCodes.Call, _getTypeFromHandleMethodInfo);
            cil.Emit(OpCodes.Stloc_1);

            cil.Emit(OpCodes.Ldtoken, targetType);
            cil.Emit(OpCodes.Call, _getTypeFromHandleMethodInfo);
            cil.Emit(OpCodes.Stloc_2);

            var methods = serviceType.GetMethodsRecursively();
            var index = 1;

            var ret = new Dictionary<MethodInfo, FieldInfo>();
            foreach (var interfaceMethodInfo in methods)
            {
                var methodInfo = targetType.GetMethod(interfaceMethodInfo);

                if (interceptedTypeData.TryGetMethodInterceptorAttributes(interfaceMethodInfo, out _) ||
                    interceptedTypeData.TryGetMethodGlobalInterceptors(interfaceMethodInfo, out _))
                {
                    var aspectDelegate = pipelineProperties[interfaceMethodInfo];

                    var methodStringRepresentation = MethodInfoUtils.StringRepresentation(interfaceMethodInfo);
                    var field = typeBuilder.DefineField(string.Format("_aspectContextAtivator{0}", index), typeof(AspectContextActivator), FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.InitOnly);

                    cil.Emit(OpCodes.Ldloc_0);
                    cil.Emit(OpCodes.Ldloc_1);
                    cil.Emit(OpCodes.Ldloc_2);
                    cil.Emit(OpCodes.Ldstr, methodStringRepresentation);
                    cil.Emit(OpCodes.Ldc_I4, configs.GetHashCode());
                    cil.Emit(OpCodes.Ldnull);
                    cil.Emit(OpCodes.Ldftn, aspectDelegate);
                    cil.Emit(OpCodes.Newobj, _aspectDelegateConstructorInfo);
                    cil.Emit(OpCodes.Call, _newContextActivatorUsingStringRepresentationMethodInfo);
                    cil.Emit(OpCodes.Stsfld, field);

                    ret.Add(interfaceMethodInfo, field);

                    index++;
                }
            }
            cil.Emit(OpCodes.Ret);
            return ret;
        }
    }
}
