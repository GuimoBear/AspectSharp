using AspectSharp.Benchmarks.Aspects;
using System.Threading.Tasks;

namespace AspectSharp.Benchmarks.Services.Interfaces
{
    //[AspectCoreMetrifyType]
    //[AspectSharpMetrifyType]
    public interface IFakeService
    {
        [AspectCoreMetrifyMethod]
        [AspectSharpMetrifyMethod]
        string SayHello(string param);
        [AspectCoreMetrifyMethod]
        [AspectSharpMetrifyMethod]
        Task<string> SayHelloAsync(string param);

        string SayHelloWithoutMetrics(string param);
        Task<string> SayHelloWithoutMetricsAsync(string param);
    }
}
