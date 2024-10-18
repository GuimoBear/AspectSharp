using AspectSharp.Abstractions;
using AspectSharp.DynamicProxy.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectSharp.DynamicProxy
{
    internal class ConcreteAspectContext : AspectContext
    {
        private readonly AspectContextActivator _activator;

        public override MemberTypes MemberType => _activator.MemberType;

        public override Type ServiceType => _activator.ServiceType;

        public override MethodInfo ServiceMethod => _activator.ServiceMethod;

        public override Type ProxyType => _activator.ProxyType;

        public override MethodInfo ProxyMethod => _activator.ProxyMethod;

        public override Type TargetType => _activator.TargetType;

        public override MethodInfo TargetMethod => _activator.TargetMethod;

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
            _activator = activator;

            Proxy = proxy;
            Target = target;
            Parameters = parameters;
            ServiceProvider = serviceProvider;

            AdditionalData = new LazyDictionary();
        }

        private int _index;

        internal override Task Run()
        {
            if (_index == _activator.Interceptors.Length)
                return _activator.Tail(this);
            return _activator.Interceptors[_index++].Invoke(this, _ctx => _ctx.Run());
        }
    }
}
