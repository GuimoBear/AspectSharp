using System.Threading.Tasks;

namespace AspectSharp.DynamicProxy
{
    internal interface IMethodInvoker
    {
        Task InvokeAsync();
    }
}
