using AspectCore.Extensions.DependencyInjection;
using AspectSharp.Benchmarks.Extensions;
using AspectSharp.Benchmarks.Services.Interfaces;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace AspectSharp.Benchmarks
{
    [Description("AspectCore")]
    public class AspectCoreBenchmark : BenchmarkBase
    {
        [GlobalSetup]
        public override void GlobalSetup()
        {
            var services = NewServiceCollection();
            services.AddAspectCore();
            SetProvider(services.BuildServiceContextProvider());
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
