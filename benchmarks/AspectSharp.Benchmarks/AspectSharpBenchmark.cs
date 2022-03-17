using AspectSharp.Benchmarks.Extensions;
using AspectSharp.Benchmarks.Services.Interfaces;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Threading.Tasks;

namespace AspectSharp.Benchmarks
{
    [Description("AspectSharp")]
    public class AspectSharpBenchmark : BenchmarkBase
    {
        [GlobalSetup]
        public override void GlobalSetup()
        {
            var services = NewServiceCollection();
            services.AddAspectSharp();
            SetProvider(services.BuildServiceProvider(true));
        }

        [Benchmark]
        public override string CallFakeService()
        {
            using var scope = NewScope();
            var service = scope.ServiceProvider.GetRequiredService<IFakeService>();
            return service.SayHello("Peter");
        }

        [Benchmark]
        public override string CallFakeServiceWithoutMetrics()
        {
            using var scope = NewScope();
            var service = scope.ServiceProvider.GetRequiredService<IFakeService>();
            return service.SayHelloWithoutAspects("Peter");
        }

        [GlobalCleanup]
        public override void GlobalCleanup()
        {
            DisposeProvider();
        }
    }
}
