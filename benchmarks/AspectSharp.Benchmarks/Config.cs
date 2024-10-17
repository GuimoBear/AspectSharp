using AspectSharp.Benchmarks.Helpers;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Filters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Order;

namespace AspectSharp.Benchmarks
{
    public class Config : ManualConfig
    {
        public const int UnrollFactor = 5000;
        public const int Iterations = 30;

        public Config(IFilter filter, bool createScopeDuringBenchmark)
        {
            AddLogger(ConsoleLogger.Default);

            AddExporter(CsvExporter.Default);
            AddExporter(MarkdownExporter.GitHub);
            AddExporter(RPlotExporter.Default);
            AddExporter(HtmlExporter.Default);

            var md = MemoryDiagnoser.Default;
            AddDiagnoser(md);
            AddColumn(new MetricSchemaColumn());
            AddColumn(new RuntimeColumn());
            AddColumn(TargetMethodColumn.Method);
            AddColumn(StatisticColumn.Mean);
            AddColumn(StatisticColumn.StdDev);
            AddColumn(StatisticColumn.Error);
            AddColumn(BaselineRatioColumn.RatioMean);
            AddColumnProvider(DefaultColumnProviders.Metrics);

            AddJob(Job.Default
                   .WithRuntime(ClrRuntime.Net461)
                   .WithLaunchCount(1)
                   .WithWarmupCount(2)
                   .WithUnrollFactor(UnrollFactor)
                   .WithIterationCount(Iterations)
                   .WithEnvironmentVariable(new EnvironmentVariable(MetricsBenchmarkBase.CREATE_SCOPE_DURING_BENCHMARK_ENV_NAME, createScopeDuringBenchmark.ToString()))
            );
            AddJob(Job.Default
                   .WithRuntime(ClrRuntime.Net462)
                   .WithLaunchCount(1)
                   .WithWarmupCount(2)
                   .WithUnrollFactor(UnrollFactor)
                   .WithIterationCount(Iterations)
                   .WithEnvironmentVariable(new EnvironmentVariable(MetricsBenchmarkBase.CREATE_SCOPE_DURING_BENCHMARK_ENV_NAME, createScopeDuringBenchmark.ToString()))
            );
            AddJob(Job.Default
                   .WithRuntime(ClrRuntime.Net47)
                   .WithLaunchCount(1)
                   .WithWarmupCount(2)
                   .WithUnrollFactor(UnrollFactor)
                   .WithIterationCount(Iterations)
                   .WithEnvironmentVariable(new EnvironmentVariable(MetricsBenchmarkBase.CREATE_SCOPE_DURING_BENCHMARK_ENV_NAME, createScopeDuringBenchmark.ToString()))
            );
            AddJob(Job.Default
                   .WithRuntime(ClrRuntime.Net472)
                   .WithLaunchCount(1)
                   .WithWarmupCount(2)
                   .WithUnrollFactor(UnrollFactor)
                   .WithIterationCount(Iterations)
                   .WithEnvironmentVariable(new EnvironmentVariable(MetricsBenchmarkBase.CREATE_SCOPE_DURING_BENCHMARK_ENV_NAME, createScopeDuringBenchmark.ToString()))
            );
            AddJob(Job.Default
                   .WithRuntime(ClrRuntime.Net48)
                   .WithLaunchCount(1)
                   .WithWarmupCount(2)
                   .WithUnrollFactor(UnrollFactor)
                   .WithIterationCount(Iterations)
                   .WithEnvironmentVariable(new EnvironmentVariable(MetricsBenchmarkBase.CREATE_SCOPE_DURING_BENCHMARK_ENV_NAME, createScopeDuringBenchmark.ToString()))
            );
            AddJob(Job.Default
                   .WithRuntime(CoreRuntime.Core31)
                   .WithLaunchCount(1)
                   .WithWarmupCount(2)
                   .WithUnrollFactor(UnrollFactor)
                   .WithIterationCount(Iterations)
                   .WithEnvironmentVariable(new EnvironmentVariable(MetricsBenchmarkBase.CREATE_SCOPE_DURING_BENCHMARK_ENV_NAME, createScopeDuringBenchmark.ToString()))
            );
            AddJob(Job.Default
                   .WithRuntime(CoreRuntime.Core50)
                   .WithLaunchCount(1)
                   .WithWarmupCount(2)
                   .WithUnrollFactor(UnrollFactor)
                   .WithIterationCount(Iterations)
                   .WithEnvironmentVariable(new EnvironmentVariable(MetricsBenchmarkBase.CREATE_SCOPE_DURING_BENCHMARK_ENV_NAME, createScopeDuringBenchmark.ToString()))
            );
            AddJob(Job.Default
                   .WithRuntime(CoreRuntime.Core60)
                   .WithLaunchCount(1)
                   .WithWarmupCount(2)
                   .WithUnrollFactor(UnrollFactor)
                   .WithIterationCount(Iterations)
                   .WithEnvironmentVariable(new EnvironmentVariable(MetricsBenchmarkBase.CREATE_SCOPE_DURING_BENCHMARK_ENV_NAME, createScopeDuringBenchmark.ToString()))
            );
            AddJob(Job.Default
                   .WithRuntime(CoreRuntime.Core70)
                   .WithLaunchCount(1)
                   .WithWarmupCount(2)
                   .WithUnrollFactor(UnrollFactor)
                   .WithIterationCount(Iterations)
                   .WithEnvironmentVariable(new EnvironmentVariable(MetricsBenchmarkBase.CREATE_SCOPE_DURING_BENCHMARK_ENV_NAME, createScopeDuringBenchmark.ToString()))
            );
            AddJob(Job.Default
                   .WithRuntime(CoreRuntime.Core80)
                   .WithLaunchCount(1)
                   .WithWarmupCount(2)
                   .WithUnrollFactor(UnrollFactor)
                   .WithIterationCount(Iterations)
                   .WithEnvironmentVariable(new EnvironmentVariable(MetricsBenchmarkBase.CREATE_SCOPE_DURING_BENCHMARK_ENV_NAME, createScopeDuringBenchmark.ToString()))
            );

            AddLogicalGroupRules(BenchmarkLogicalGroupRule.ByCategory, BenchmarkLogicalGroupRule.ByJob);
            Orderer = new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest);
            Options |= ConfigOptions.JoinSummary;

            if (!(filter is null))
                AddFilter(filter);
        }
    }
}
