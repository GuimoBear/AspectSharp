using AspectSharp.Benchmarks.Helpers;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Order;

namespace AspectSharp.Benchmarks
{
    public class Config : ManualConfig
    {
        public const int UnrollFactor = 3000;
        public const int Iterations = 20;

        public Config()
        {
            AddLogger(ConsoleLogger.Default);

            AddExporter(CsvExporter.Default);
            AddExporter(MarkdownExporter.GitHub);
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
            );
            AddJob(Job.Default
                   .WithRuntime(ClrRuntime.Net462)
                   .WithLaunchCount(1)
                   .WithWarmupCount(2)
                   .WithUnrollFactor(UnrollFactor)
                   .WithIterationCount(Iterations)
            );
            AddJob(Job.Default
                   .WithRuntime(ClrRuntime.Net47)
                   .WithLaunchCount(1)
                   .WithWarmupCount(2)
                   .WithUnrollFactor(UnrollFactor)
                   .WithIterationCount(Iterations)
            );
            AddJob(Job.Default
                   .WithRuntime(ClrRuntime.Net472)
                   .WithLaunchCount(1)
                   .WithWarmupCount(2)
                   .WithUnrollFactor(UnrollFactor)
                   .WithIterationCount(Iterations)
            );
            AddJob(Job.Default
                   .WithRuntime(ClrRuntime.Net48)
                   .WithLaunchCount(1)
                   .WithWarmupCount(2)
                   .WithUnrollFactor(UnrollFactor)
                   .WithIterationCount(Iterations)
            );
            AddJob(Job.Default
                   .WithRuntime(CoreRuntime.Core31)
                   .WithLaunchCount(1)
                   .WithWarmupCount(2)
                   .WithUnrollFactor(UnrollFactor)
                   .WithIterationCount(Iterations)
            );
            AddJob(Job.Default
                   .WithRuntime(CoreRuntime.Core50)
                   .WithLaunchCount(1)
                   .WithWarmupCount(2)
                   .WithUnrollFactor(UnrollFactor)
                   .WithIterationCount(Iterations)
            );
            AddJob(Job.Default
                   .WithRuntime(CoreRuntime.Core60)
                   .WithLaunchCount(1)
                   .WithWarmupCount(2)
                   .WithUnrollFactor(UnrollFactor)
                   .WithIterationCount(Iterations)
            );
            Orderer = new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest);
            Options |= ConfigOptions.JoinSummary;
        }
    }
}
