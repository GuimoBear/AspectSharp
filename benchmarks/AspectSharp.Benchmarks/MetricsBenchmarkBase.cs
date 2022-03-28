using AspectSharp.Benchmarks.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace AspectSharp.Benchmarks
{
    public abstract class MetricsBenchmarkBase
    {
        public const string CREATE_SCOPE_DURING_BENCHMARK_ENV_NAME = "CREATE_SCOPE_DURING_BENCHMARK";

        protected bool CreateScopeDuringBenchmark { get; }

        public MetricsBenchmarkBase()
        {
            var env = Environment.GetEnvironmentVariable(CREATE_SCOPE_DURING_BENCHMARK_ENV_NAME);
            if (!string.IsNullOrEmpty(env) && env.Equals("true", StringComparison.OrdinalIgnoreCase))
                CreateScopeDuringBenchmark = true;
        }

        private IServiceProvider ServiceProvider { get; set; }

        protected IServiceScope Scope { get; set; }

        protected void SetProvider(IServiceProvider serviceProvider)
            => ServiceProvider = serviceProvider;

        protected void DisposeProvider()
            => (ServiceProvider as ServiceProvider)?.Dispose();

        protected IServiceScope NewScope()
            => ServiceProvider.CreateScope();

        protected IServiceCollection NewServiceCollection()
            => new ServiceCollection().AddServices();

        public abstract void GlobalSetup();
        public virtual void IterationSetup()
        {
            if (!CreateScopeDuringBenchmark)
                Scope = NewScope();
        }

        public abstract string CallFakeService();
        public abstract string CallFakeServiceWithoutMetrics();
        public abstract string CallUnmetrifiedFakeService();

        public abstract Task<string> CallFakeServiceAsync();
        public abstract Task<string> CallFakeServiceWithoutMetricsAsync();
        public abstract Task<string> CallUnmetrifiedFakeServiceAsync();

        public virtual void IterationCleanup()
        {
            if (!CreateScopeDuringBenchmark)
                Scope?.Dispose();
        }

        public abstract void GlobalCleanup();
    }
}
