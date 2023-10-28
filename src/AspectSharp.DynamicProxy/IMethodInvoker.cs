using System;
using System.Threading.Tasks;

namespace AspectSharp.DynamicProxy
{
    internal interface IMethodInvoker
    {
        Task InvokeAsync();
    }

    internal sealed class T<TService> : ConcreteAspectContext, IMethodInvoker
    {
        public T(AspectContextActivator activator, object target, object proxy, object[] parameters, IServiceProvider serviceProvider) : base(activator, target, proxy, parameters, serviceProvider)
        {

        }

        public Task InvokeAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}
