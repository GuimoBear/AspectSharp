using AspectSharp.Abstractions;
using System;

namespace AspectSharp.DynamicProxy
{
    public interface IAspectContextFactory
    {
        IServiceProvider Services {  get; }
        AspectContext CurrentContext { get; set; }

        AspectContext CreateContext(AspectContextActivator activator, object target, object proxy, object[] parameters);
    }
}
