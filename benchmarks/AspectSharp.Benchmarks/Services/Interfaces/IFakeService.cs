using AspectSharp.Benchmarks.Aspects;

namespace AspectSharp.Benchmarks.Services.Interfaces
{
    //[AspectCoreMetrifyType]
    //[AspectSharpMetrifyType]
    public interface IFakeService
    {
        [AspectCoreMetrifyMethod]
        [AspectSharpMetrifyMethod]
        string SayHello(string param);

        string SayHelloWithoutAspects(string param);
    }
}
