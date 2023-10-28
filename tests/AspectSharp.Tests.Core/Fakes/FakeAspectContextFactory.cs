using AspectSharp.Abstractions;
using AspectSharp.DynamicProxy;
using System;

namespace AspectSharp.Tests.Core.Fakes
{
    public class FakeAspectContextFactory : IAspectContextFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public AspectContext CurrentContext { get; set; }

        public IServiceProvider Services => _serviceProvider;

        public FakeAspectContextFactory(IServiceProvider serviceProvider)
            => _serviceProvider = serviceProvider;

        public AspectContext CreateContext(AspectContextActivator activator, object target, object proxy, object[] parameters)
        {
            CurrentContext = new ConcreteAspectContext(activator, target, proxy, parameters, _serviceProvider);
            return CurrentContext;
        }
    }
}
