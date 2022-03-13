using AspectSharp.Abstractions;
using System.Threading.Tasks;

namespace AspectSharp.DynamicProxy
{
    public delegate Task InterceptDelegate(AspectContext context, AspectDelegate next);
}
