using System;
using System.Collections.Generic;
using System.Reflection;

namespace AspectSharp.Abstractions
{
    public abstract class AspectContext
    {
        public abstract Type ServiceType { get; }

        public abstract MethodInfo ServiceMethod { get; }

        public abstract Type ProxyType { get; }

        public abstract MethodInfo ProxyMethod { get; }

        public abstract Type TargetType { get; }

        public abstract MethodInfo TargetMethod { get; }

        public abstract object Proxy { get; }

        public abstract object Target { get; }

        public abstract object[] Parameters { get; }

        public abstract IServiceProvider ServiceProvider { get; }

        public abstract IDictionary<string, object> AdditionalData { get; }

        public abstract object ReturnValue { get; set; }
    }
}
