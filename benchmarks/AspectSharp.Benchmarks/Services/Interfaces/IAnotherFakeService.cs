using System.Threading.Tasks;

namespace AspectSharp.Benchmarks.Services.Interfaces
{
    public interface IAnotherFakeService
    {
        Task<string> SayHello(string param);
    }
}
