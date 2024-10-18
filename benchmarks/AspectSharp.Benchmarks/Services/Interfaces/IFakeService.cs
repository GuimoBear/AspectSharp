using AspectSharp.Benchmarks.Aspects;
using System.Threading.Tasks;

namespace AspectSharp.Benchmarks.Services.Interfaces
{
    public interface IFakeService
    {
        [AspectCoreMetrifyMethod]
        [AspectSharpMetrifyMethod]
        string SayHello(string param);

        [AspectCoreNoOpMethod]
        [AspectCoreNoOpMethod]
        [AspectCoreNoOpMethod]
        [AspectCoreMetrifyMethod]
        [AspectSharpNoOpMethod]
        [AspectSharpNoOpMethod]
        [AspectSharpNoOpMethod]
        [AspectSharpMetrifyMethod]
        string SayHelloWithFourInterceptors(string param);

        [AspectCoreMetrifyMethod]
        [AspectSharpMetrifyMethod]
        Task<string> SayHelloAsync(string param);

        [AspectCoreNoOpMethod]
        [AspectCoreNoOpMethod]
        [AspectCoreNoOpMethod]
        [AspectCoreMetrifyMethod]
        [AspectSharpNoOpMethod]
        [AspectSharpNoOpMethod]
        [AspectSharpNoOpMethod]
        [AspectSharpMetrifyMethod]
        Task<string> SayHelloWithFourInterceptorsAsync(string param);

        string SayHelloWithoutMetrics(string param);
        Task<string> SayHelloWithoutMetricsAsync(string param);
    }
}
