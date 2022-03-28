using System.Threading.Tasks;

namespace AspectSharp.Benchmarks.Services.Interfaces
{
    public interface IAnotherFakeService
    {
        string SayHello(string param);
        Task<string> SayHelloAsync(string param);

        string SayHelloWithoutMetrics(string param);
        Task<string> SayHelloWithoutMetricsAsync(string param);
    }
}
