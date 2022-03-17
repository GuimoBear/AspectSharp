using BenchmarkDotNet.Running;

namespace AspectSharp.Benchmarks
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Run(new AspectCoreBenchmark());
            Run(new AspectSharpBenchmark());
            Run(new ControlBenchmark());

            new BenchmarkSwitcher(typeof(BenchmarkBase).Assembly).Run(args, new Config());
        }

        private static void Run(BenchmarkBase benchmark)
        {
            benchmark.GlobalSetup();
            benchmark.CallFakeService();
            benchmark.CallFakeServiceWithoutMetrics();
            benchmark.CallUnmetrifiedFakeService();
            benchmark.GlobalCleanup();
        }
    }
}
