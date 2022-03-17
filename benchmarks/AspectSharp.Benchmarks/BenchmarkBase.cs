using AspectSharp.Benchmarks.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace AspectSharp.Benchmarks
{
    public abstract class BenchmarkBase
    {
        private IServiceProvider ServiceProvider { get; set; }

        protected void SetProvider(IServiceProvider serviceProvider)
            => ServiceProvider = serviceProvider;

        protected void DisposeProvider()
            => (ServiceProvider as ServiceProvider)?.Dispose();

        protected IServiceScope NewScope()
            => ServiceProvider.CreateScope();

        protected IServiceCollection NewServiceCollection()
            => new ServiceCollection().AddServices();

        public abstract void GlobalSetup();
        public abstract string CallFakeService();
        public abstract string CallFakeServiceWithoutMetrics();
        public abstract string CallUnmetrifiedFakeService();
        public abstract void GlobalCleanup();
    }
}
