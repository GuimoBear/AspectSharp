using AspectSharp.Abstractions;
using AspectSharp.DynamicProxy.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using static AspectSharp.DynamicProxy.Utils.InterceptorTypeCache;

namespace AspectSharp.DynamicProxy.Factories
{
    internal static class AspectDelegateFactory
    {
        public static IReadOnlyDictionary<MethodInfo, MethodInfo> Create(Type targetType, Type serviceType, TypeBuilder typeBuilder, InterceptedTypeData interceptedTypeData, DynamicProxyFactoryConfigurations configs)
        {
            var methods = serviceType.GetMethodsRecursively();
            var index = 1;
            var ret = new Dictionary<MethodInfo, MethodInfo>();
            foreach (var interfaceMethodInfo in methods)
            {
                var methodInfo = targetType.GetMethod(interfaceMethodInfo);
                if (interceptedTypeData.TryGetMethodInterceptorAttributes(interfaceMethodInfo, out _) ||
                    interceptedTypeData.TryGetMethodGlobalInterceptors(interfaceMethodInfo, out _))
                {
                    var aspectDelegate = CreateAspectDelegate(typeBuilder, methodInfo, index);
                    ret.Add(interfaceMethodInfo, aspectDelegate);
                    index++;
                }
            }
            return ret;
        }

        private static MethodInfo CreateAspectDelegate(TypeBuilder typeBuilder, MethodInfo methodInfo, int index)
        {
            var returnInfo = methodInfo.GetReturnInfo();

            var methodBuilder = typeBuilder.DefineMethod(string.Format("AspectDelegate{0}", index), MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Static, CallingConventions.Standard, typeof(Task), new Type[] { typeof(AspectContext) });
            methodBuilder.DefineParameter(1, ParameterAttributes.None, "context");

            var cil = methodBuilder.GetILGenerator();
            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Castclass, typeof(IMethodInvoker));
            cil.Emit(OpCodes.Callvirt, typeof(IMethodInvoker).GetMethod(nameof(IMethodInvoker.InvokeAsync)));
            cil.Emit(OpCodes.Ret);

            return methodBuilder;
        }
    }
}
