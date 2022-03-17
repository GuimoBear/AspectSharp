using AspectSharp.Benchmarks.Services.Interfaces;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Threading.Tasks;

namespace AspectSharp.Benchmarks
{
    [Description("Control")]
    public class ControlBenchmark : BenchmarkBase
    {
        [GlobalSetup]
        public override void GlobalSetup()
        {
            var services = NewServiceCollection();
            SetProvider(services.BuildServiceProvider(true));
        }

        [Benchmark]
        public override string CallFakeService()
        {
            using var scope = NewScope();
            var service = scope.ServiceProvider.GetRequiredService<IMetrifiedFakeService>();
            return service.SayHello("Peter");
        }

        [Benchmark]
        public override string CallFakeServiceWithoutMetrics()
        {
            using var scope = NewScope();
            var service = scope.ServiceProvider.GetRequiredService<IMetrifiedFakeService>();
            return service.SayHelloWithoutAspects("Peter");
        }

        [Benchmark]
        public override string CallUnmetrifiedFakeService()
        {
            using var scope = NewScope();
            var service = scope.ServiceProvider.GetRequiredService<IAnotherFakeService>();
            return service.SayHello("Peter");
        }

        [GlobalCleanup]
        public override void GlobalCleanup()
        {
            DisposeProvider();
        }
    }
}
