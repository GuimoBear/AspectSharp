using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Filters;
using BenchmarkDotNet.Running;
using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspectSharp.Benchmarks
{
    internal class Program
    {
        static void Main(string[] args)
        {
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
                        Run(new AspectCoreMetricsBenchmark());
                        Run(new AspectSharpMetricsBenchmark());
                        Run(new ControlMetricsBenchmark());
                        Run(new NoMetricsBenchmark());

                        BenchmarkRunner.Run(typeof(MetricsBenchmarkBase).Assembly, new Config(filter), Array.Empty<string>());
                    });
            }
        }

        private static void Run(MetricsBenchmarkBase benchmark)
        {
            benchmark.GlobalSetup();
            benchmark.CallFakeService();
            benchmark.CallFakeServiceWithoutMetrics();
            benchmark.CallUnmetrifiedFakeService();
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
            { BenchmarkRuntime.Net6, CoreRuntime.Core60 }
        };

        private readonly BenchmarkRuntime _runtime;
        private readonly IEnumerable<Runtime> _runtimes;
        private readonly BenchmarkCategory _category;
        private readonly IEnumerable<string> _categories;

        public BenchmarkOptions(BenchmarkRuntime runtime, BenchmarkCategory category)
        {
            _runtime = runtime;
            var runtimeList = new List<Runtime>();
            foreach(var _runtime in Enum.GetValues(runtime.GetType()).Cast<Enum>().Where(runtime.HasFlag).Cast<BenchmarkRuntime>())
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
                    new Example("Run metrified methods in a single runtime", new BenchmarkOptions(BenchmarkRuntime.Net47, BenchmarkCategory.Metrified)),
                    new Example("Run metrified and unmetrified methods in a multiple runtimes", new BenchmarkOptions(BenchmarkRuntime.Net48 | BenchmarkRuntime.Net5 | BenchmarkRuntime.Net6, BenchmarkCategory.Metrified | BenchmarkCategory.Unmetrified)),
                    new Example("Run all methods in all supported runtimes(maybe take a lot of time)", new BenchmarkOptions(BenchmarkRuntime.All, BenchmarkCategory.All))
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
        All = Net461 | Net462 | Net47 | Net48 | NetCoreApp31 | Net5 | Net6
    }

    [Flags]
    internal enum BenchmarkCategory
    {
        Metrified = 1, 
        Unmetrified = 2,
        UnmetrifiedClass = 4,
        All = Metrified | Unmetrified | UnmetrifiedClass
    }
}
