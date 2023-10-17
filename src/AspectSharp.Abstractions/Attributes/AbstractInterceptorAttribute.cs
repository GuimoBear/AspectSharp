using System;
using System.Threading.Tasks;

namespace AspectSharp.Abstractions.Attributes
{
    public abstract class AbstractInterceptorAttribute : Attribute, IInterceptor
    {
        public abstract Task Invoke(AspectContext context, AspectDelegate next);
    }
}
