using System;
using System.Reflection;

namespace AspectSharp.DynamicProxy
{
    public sealed class AspectContextActivator
    {
        public Type ServiceType { get; }

        public MethodInfo ServiceMethod { get; }

        public Type ProxyType { get; }

        public MethodInfo ProxyMethod { get; }

        public Type TargetType { get; }

        public MethodInfo TargetMethod { get; }

        public AspectContextActivator(
            Type serviceType,
            MethodInfo serviceMethod,
            Type proxyType,
            MethodInfo proxyMethod,
            Type targetType,
            MethodInfo TtrgetMethod)
        {
            ServiceType = serviceType;
            ServiceMethod = serviceMethod;
            ProxyType = proxyType;
            ProxyMethod = proxyMethod;
            TargetType = targetType;
            TargetMethod = TtrgetMethod;
        }
    }
}
