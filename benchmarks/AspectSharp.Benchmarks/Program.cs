using BenchmarkDotNet.Running;

namespace AspectSharp.Benchmarks
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Run(new AspectCoreMetricsBenchmark());
            Run(new AspectSharpMetricsBenchmark());
            Run(new ControlMetricsBenchmark());
            Run(new NoMetricsBenchmark());

            new BenchmarkSwitcher(typeof(MetricsBenchmarkBase).Assembly).Run(args, new Config());
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
}
