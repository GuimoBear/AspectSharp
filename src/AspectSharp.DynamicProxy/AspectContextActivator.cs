using System;
using System.Linq;
using System.Reflection;

namespace AspectSharp.DynamicProxy
{
    public sealed class AspectContextActivator
    {
        public MemberTypes MemberType { get; }

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

            MemberType = MemberTypes.Method;
            if (ServiceMethod.IsSpecialName)
            {
                if (!(ServiceType.GetProperties().FirstOrDefault(prop => prop.GetGetMethod() == ServiceMethod || prop.GetSetMethod() == ServiceMethod) is null))
                    MemberType = MemberTypes.Property;
                else if (!(ServiceType.GetEvents().FirstOrDefault(evt => evt.GetAddMethod() == ServiceMethod || evt.GetRemoveMethod() == ServiceMethod) is null))
                    MemberType = MemberTypes.Event;
            }
        }
    }
}
