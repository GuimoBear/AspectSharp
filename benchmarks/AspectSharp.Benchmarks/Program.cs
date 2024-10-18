using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Filters;
using BenchmarkDotNet.Running;
using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectSharp.Benchmarks
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await Run(new AspectCoreMetricsBenchmark());
            await Run(new AspectSharpMetricsBenchmark());
            await Run(new ControlMetricsBenchmark());
            await Run(new NoMetricsBenchmark());
            using (var parser = new Parser(with =>
            {
                with.CaseSensitive = Parser.Default.Settings.CaseSensitive;
                with.CaseInsensitiveEnumValues = true;
                with.ParsingCulture = Parser.Default.Settings.ParsingCulture;
                with.HelpWriter = Parser.Default.Settings.HelpWriter;
                with.IgnoreUnknownArguments = Parser.Default.Settings.IgnoreUnknownArguments;
                with.AutoHelp = Parser.Default.Settings.AutoHelp;
                with.AutoVersion = Parser.Default.Settings.AutoVersion;
                with.EnableDashDash = Parser.Default.Settings.EnableDashDash;
                with.MaximumDisplayWidth = Parser.Default.Settings.MaximumDisplayWidth;
            }))
            {
                parser.ParseArguments<BenchmarkOptions>(args)
                    .WithParsed(filter =>
                    {
                        BenchmarkRunner.Run(typeof(MetricsBenchmarkBase).Assembly, new Config(filter, filter.CreateScopeDuringBenchmark), Array.Empty<string>());
                    });
            }
        }

        private static async Task Run(MetricsBenchmarkBase benchmark)
        {
            benchmark.GlobalSetup();
            Console.WriteLine("{0} benchmarks started", benchmark.GetType().Name);
#if DEBUG
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var actionSw = System.Diagnostics.Stopwatch.StartNew();
#endif
            benchmark.IterationSetup();
            benchmark.CallFakeService();
            benchmark.IterationCleanup();
#if DEBUG
            actionSw.Stop();
            var elapsed = actionSw.Elapsed;
            Console.WriteLine("\t{0}.{1}: Elapsed time {2}", benchmark.GetType().Name, nameof(MetricsBenchmarkBase.CallFakeService), elapsed.ToString());
            actionSw = System.Diagnostics.Stopwatch.StartNew();
#endif
            benchmark.IterationSetup();
            benchmark.CallFakeServiceWithoutMetrics();
            benchmark.IterationCleanup();
#if DEBUG
            actionSw.Stop();
            elapsed = actionSw.Elapsed;
            Console.WriteLine("\t{0}.{1}: Elapsed time {2}", benchmark.GetType().Name, nameof(MetricsBenchmarkBase.CallFakeServiceWithoutMetrics), elapsed.ToString());
            actionSw = System.Diagnostics.Stopwatch.StartNew();
#endif
            benchmark.IterationSetup();
            benchmark.CallUnmetrifiedFakeService();
            benchmark.IterationCleanup();
#if DEBUG
            actionSw.Stop();
            elapsed = actionSw.Elapsed;
            Console.WriteLine("\t{0}.{1}: Elapsed time {2}", benchmark.GetType().Name, nameof(MetricsBenchmarkBase.CallUnmetrifiedFakeService), elapsed.ToString());
            actionSw = System.Diagnostics.Stopwatch.StartNew();
#endif
            benchmark.IterationSetup();
            await benchmark.CallFakeServiceAsync();
            benchmark.IterationCleanup();
#if DEBUG
            actionSw.Stop();
            elapsed = actionSw.Elapsed;
            Console.WriteLine("\t{0}.{1}: Elapsed time {2}", benchmark.GetType().Name, nameof(MetricsBenchmarkBase.CallFakeServiceAsync), elapsed.ToString());
            actionSw = System.Diagnostics.Stopwatch.StartNew();
#endif
            benchmark.IterationSetup();
            await benchmark.CallFakeServiceWithoutMetricsAsync();
            benchmark.IterationCleanup();
#if DEBUG
            actionSw.Stop();
            elapsed = actionSw.Elapsed;
            Console.WriteLine("\t{0}.{1}: Elapsed time {2}", benchmark.GetType().Name, nameof(MetricsBenchmarkBase.CallFakeServiceWithoutMetricsAsync), elapsed.ToString());
            actionSw = System.Diagnostics.Stopwatch.StartNew();
#endif
            benchmark.IterationSetup();
            await benchmark.CallUnmetrifiedFakeServiceAsync();
            benchmark.IterationCleanup();
#if DEBUG
            actionSw.Stop();
            elapsed = actionSw.Elapsed;
            Console.WriteLine("\t{0}.{1}: Elapsed time {2}", benchmark.GetType().Name, nameof(MetricsBenchmarkBase.CallUnmetrifiedFakeServiceAsync), elapsed.ToString());
            sw.Stop();
            elapsed = sw.Elapsed;
            Console.WriteLine("{0}: Elapsed time {1}", benchmark.GetType().Name, elapsed.ToString());
#endif
            benchmark.GlobalCleanup();
        }
    }

    internal sealed class BenchmarkOptions : IFilter
    {
        private static readonly IDictionary<BenchmarkRuntime, Runtime> _benchmarkRuntimes = new Dictionary<BenchmarkRuntime, Runtime>
        {
            { BenchmarkRuntime.Net461, ClrRuntime.Net461 },
            { BenchmarkRuntime.Net462, ClrRuntime.Net462 },
            { BenchmarkRuntime.Net47, ClrRuntime.Net47 },
            { BenchmarkRuntime.Net472, ClrRuntime.Net472 },
            { BenchmarkRuntime.Net48, ClrRuntime.Net48 },
            { BenchmarkRuntime.NetCoreApp31, CoreRuntime.Core31 },
            { BenchmarkRuntime.Net5, CoreRuntime.Core50 },
            { BenchmarkRuntime.Net6, CoreRuntime.Core60 },
            { BenchmarkRuntime.Net7, CoreRuntime.Core70 },
            { BenchmarkRuntime.Net8, CoreRuntime.Core80 },
            { BenchmarkRuntime.Net9, CoreRuntime.Core90 }
        };

        private readonly BenchmarkRuntime _runtime;
        private readonly IEnumerable<Runtime> _runtimes;
        private readonly bool _createScopeDuringBenchmark;
        private readonly BenchmarkCategory _category;
        private readonly IEnumerable<string> _categories;

        public BenchmarkOptions(bool createScopeDuringBenchmark, BenchmarkRuntime runtime, BenchmarkCategory category)
        {
            _createScopeDuringBenchmark = createScopeDuringBenchmark;
            _runtime = runtime;
            var runtimeList = new List<Runtime>();
            foreach (var _runtime in Enum.GetValues(runtime.GetType()).Cast<Enum>().Where(runtime.HasFlag).Cast<BenchmarkRuntime>())
            {
                if (_benchmarkRuntimes.TryGetValue(_runtime, out var benchmarkRuntime))
                    runtimeList.Add(benchmarkRuntime);
            }
            _runtimes = runtimeList;
            _category = category;
            _categories = Enum.GetValues(category.GetType())
                .Cast<Enum>()
                .Where(category.HasFlag)
                .Cast<BenchmarkCategory>()
                .Select(e => e.ToString());
        }

        [Option('s', "create-scope", Required = false, HelpText = "Create scope during benchmark method")]
        public bool CreateScopeDuringBenchmark { get => _createScopeDuringBenchmark; }

        [Option('r', "runtime", Required = true, HelpText = "Benchmark runtime")]
        public BenchmarkRuntime Runtime { get => _runtime; }

        [Option('c', "category", Required = true, HelpText = "Benchmark category")]
        public BenchmarkCategory Category { get => _category; }

        [Usage(ApplicationAlias = "benchmark")]
        public static IEnumerable<Example> RuntimeExamples
        {
            get
            {
                return new List<Example>()
                {
                    new Example("Run metrified methods in a single runtime", new BenchmarkOptions(false, BenchmarkRuntime.Net47, BenchmarkCategory.Metrified)),
                    new Example("Creating scope during benchmark method, run metrified and unmetrified methods in a multiple runtimes", new BenchmarkOptions(true, BenchmarkRuntime.Net48 | BenchmarkRuntime.Net5 | BenchmarkRuntime.Net6, BenchmarkCategory.Metrified | BenchmarkCategory.Unmetrified)),
                    new Example("Run all methods in all supported runtimes(maybe take a lot of time)", new BenchmarkOptions(false, BenchmarkRuntime.All, BenchmarkCategory.All))
                };
            }
        }

        public bool Predicate(BenchmarkCase benchmarkCase)
            => _runtimes.Contains(benchmarkCase.GetRuntime()) &&
               _categories.Any(benchmarkCase.Descriptor.HasCategory);
    }

    [Flags]
    internal enum BenchmarkRuntime
    {
        Net461 = 1,
        Net462 = 2,
        Net47 = 4,
        Net472 = 8,
        Net48 = 16,
        NetCoreApp31 = 32,
        Net5 = 64,
        Net6 = 128,
        Net7 = 256,
        Net8 = 512,
        Net9 = 1024,
        All = Net461 | Net462 | Net47 | Net48 | NetCoreApp31 | Net5 | Net6 | Net7 | Net8 | Net9
    }

    [Flags]
    internal enum BenchmarkCategory
    {
        Metrified = 1,
        Unmetrified = 2,
        UnmetrifiedClass = 4,
        AsyncMetrified = 8,
        AsyncUnmetrified = 16,
        AsyncUnmetrifiedClass = 32,
        AllSync = Metrified | Unmetrified | UnmetrifiedClass,
        AllAsync = AsyncMetrified | AsyncUnmetrified | AsyncUnmetrifiedClass,
        All = AllSync | AllAsync
    }
}
