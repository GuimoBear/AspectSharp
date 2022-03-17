namespace AspectSharp.Benchmarks.Services.Interfaces
{
    public interface IMetrifiedFakeService
    {
        string SayHello(string param);
        string SayHelloWithoutMetrics(string param);
    }
}
