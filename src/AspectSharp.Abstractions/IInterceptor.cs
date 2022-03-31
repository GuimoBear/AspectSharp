using System.Threading.Tasks;

namespace AspectSharp.Abstractions
{
    public interface IInterceptor
    {
        Task Invoke(AspectContext context, AspectDelegate next);
    }
}
