using AspectSharp.Abstractions;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AspectSharp.DynamicProxy
{
    internal sealed class ConcreteAspectContext : AspectContext
    {
        public override MemberTypes MemberType { get; }

        public override Type ServiceType { get; }

        public override MethodInfo ServiceMethod { get; }

        public override Type ProxyType { get; }

        public override MethodInfo ProxyMethod { get; }

        public override Type TargetType { get; }

        public override MethodInfo TargetMethod { get; }

        public override object Proxy { get; }

        public override object Target { get; }

        public override object[] Parameters { get; }

        public override IServiceProvider ServiceProvider { get; }

        public override IDictionary<string, object> AdditionalData { get; }

        public override object ReturnValue { get; set; }

        public ConcreteAspectContext(
            AspectContextActivator activator,
            object target,
            object proxy,
            object[] parameters,
            IServiceProvider serviceProvider)
        {
            MemberType = activator.MemberType;
            ServiceType = activator.ServiceType;
            ServiceMethod = activator.ServiceMethod;
            ProxyType = activator.ProxyType;
            ProxyMethod = activator.ProxyMethod;
            TargetType = activator.TargetType;
            TargetMethod = activator.TargetMethod;

            Proxy = proxy;
            Target = target;
            Parameters = parameters;
            ServiceProvider = serviceProvider;

            AdditionalData = new Dictionary<string, object>();
        }
    }
}
