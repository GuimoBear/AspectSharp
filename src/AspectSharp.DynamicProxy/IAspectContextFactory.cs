using AspectSharp.Abstractions;

namespace AspectSharp.DynamicProxy
{
    public interface IAspectContextFactory
    {
        AspectContext CreateContext(AspectContextActivator activator, object target, object proxy, object[] parameters);
    }
}
