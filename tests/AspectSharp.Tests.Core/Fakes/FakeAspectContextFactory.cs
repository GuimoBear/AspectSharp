using AspectSharp.Abstractions;
using AspectSharp.DynamicProxy;
using System;

namespace AspectSharp.Tests.Core.Fakes
{
    public class FakeAspectContextFactory : IAspectContextFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public AspectContext Context { get; private set; }

        public FakeAspectContextFactory(IServiceProvider serviceProvider)
            => _serviceProvider = serviceProvider;

        public AspectContext CreateContext(AspectContextActivator activator, object target, object proxy, object[] parameters)
        {
            Context = new ConcreteAspectContext(activator, target, proxy, parameters, _serviceProvider);
            return Context;
        }
    }
}
