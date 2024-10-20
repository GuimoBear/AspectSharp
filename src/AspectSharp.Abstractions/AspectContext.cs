﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectSharp.Abstractions
{
    public abstract class AspectContext
    {
        public abstract MemberTypes MemberType { get; }

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

        public bool TargetMethodCalled { get; internal set; }

        internal abstract Task Run();
    }
}
