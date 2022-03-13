using AspectSharp.Abstractions;
using System;

namespace AspectSharp.DynamicProxy.Factories
{
    internal sealed class AspectContextFactory : IAspectContextFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public AspectContextFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public AspectContext CreateContext(AspectContextActivator activator, object target, object proxy, object[] parameters)
            => new ConcreteAspectContext(activator, target, proxy, parameters, _serviceProvider);
    }
}
