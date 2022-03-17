namespace AspectSharp.Benchmarks.Services.Interfaces
{
    public interface IMetrifiedFakeService
    {
        string SayHello(string param);
        string SayHelloWithoutAspects(string param);
    }
}
