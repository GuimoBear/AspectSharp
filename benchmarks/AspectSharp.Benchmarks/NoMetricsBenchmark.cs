﻿using AspectSharp.Benchmarks.Services.Interfaces;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Threading.Tasks;

namespace AspectSharp.Benchmarks
{
    [Description("Without metrics")]
    public class NoMetricsBenchmark : MetricsBenchmarkBase
    {
        public NoMetricsBenchmark()
            : base() { }

        [GlobalSetup]
        public override void GlobalSetup()
        {
            var services = NewServiceCollection();
            SetProvider(services.BuildServiceProvider(true));
        }

        [IterationSetup]
        public override void IterationSetup()
            => base.IterationSetup();

        [Benchmark]
        [BenchmarkCategory(nameof(BenchmarkCategory.Metrified))]
        public override string CallFakeService()
        {
            if (CreateScopeDuringBenchmark)
                Scope = NewScope();
            try
            {
                var service = Scope.ServiceProvider.GetRequiredService<IAnotherFakeService>();
                return service.SayHello("Peter");
            }
            finally
            {
                if (CreateScopeDuringBenchmark)
                    Scope.Dispose();
            }
        }

        [Benchmark]
        [BenchmarkCategory(nameof(BenchmarkCategory.Metrified))]
        public override string CallFakeServiceWithFourInterceptors()
        {
            if (CreateScopeDuringBenchmark)
                Scope = NewScope();
            try
            {
                var service = Scope.ServiceProvider.GetRequiredService<IAnotherFakeService>();
                return service.SayHello("Peter");
            }
            finally
            {
                if (CreateScopeDuringBenchmark)
                    Scope.Dispose();
            }
        }

        [Benchmark]
        [BenchmarkCategory(nameof(BenchmarkCategory.Unmetrified))]
        public override string CallFakeServiceWithoutMetrics()
        {
            if (CreateScopeDuringBenchmark)
                Scope = NewScope();
            try
            {
                var service = Scope.ServiceProvider.GetRequiredService<IAnotherFakeService>();
                return service.SayHelloWithoutMetrics("Peter");
            }
            finally
            {
                if (CreateScopeDuringBenchmark)
                    Scope.Dispose();
            }
        }

        [Benchmark]
        [BenchmarkCategory(nameof(BenchmarkCategory.UnmetrifiedClass))]
        public override string CallUnmetrifiedFakeService()
        {
            if (CreateScopeDuringBenchmark)
                Scope = NewScope();
            try
            {
                var service = Scope.ServiceProvider.GetRequiredService<IAnotherFakeService>();
                return service.SayHello("Peter");
            }
            finally
            {
                if (CreateScopeDuringBenchmark)
                    Scope.Dispose();
            }
        }

        [Benchmark]
        [BenchmarkCategory(nameof(BenchmarkCategory.AsyncMetrified))]
        public override async Task<string> CallFakeServiceAsync()
        {
            if (CreateScopeDuringBenchmark)
                Scope = NewScope();
            try
            {
                var service = Scope.ServiceProvider.GetRequiredService<IAnotherFakeService>();
                return await service.SayHelloAsync("Peter");
            }
            finally
            {
                if (CreateScopeDuringBenchmark)
                    Scope.Dispose();
            }
        }

        [Benchmark]
        [BenchmarkCategory(nameof(BenchmarkCategory.AsyncMetrified))]
        public override async Task<string> CallFakeServiceWithFourInterceptorsAsync()
        {
            if (CreateScopeDuringBenchmark)
                Scope = NewScope();
            try
            {
                var service = Scope.ServiceProvider.GetRequiredService<IAnotherFakeService>();
                return await service.SayHelloAsync("Peter");
            }
            finally
            {
                if (CreateScopeDuringBenchmark)
                    Scope.Dispose();
            }
        }

        [Benchmark]
        [BenchmarkCategory(nameof(BenchmarkCategory.AsyncUnmetrified))]
        public override async Task<string> CallFakeServiceWithoutMetricsAsync()
        {
            if (CreateScopeDuringBenchmark)
                Scope = NewScope();
            try
            {
                var service = Scope.ServiceProvider.GetRequiredService<IAnotherFakeService>();
                return await service.SayHelloWithoutMetricsAsync("Peter");
            }
            finally
            {
                if (CreateScopeDuringBenchmark)
                    Scope.Dispose();
            }
        }

        [Benchmark]
        [BenchmarkCategory(nameof(BenchmarkCategory.AsyncUnmetrifiedClass))]
        public override async Task<string> CallUnmetrifiedFakeServiceAsync()
        {
            if (CreateScopeDuringBenchmark)
                Scope = NewScope();
            try
            {
                var service = Scope.ServiceProvider.GetRequiredService<IAnotherFakeService>();
                return await service.SayHelloAsync("Peter");
            }
            finally
            {
                if (CreateScopeDuringBenchmark)
                    Scope.Dispose();
            }
        }

        [IterationCleanup]
        public override void IterationCleanup()
            => base.IterationCleanup();

        [GlobalCleanup]
        public override void GlobalCleanup()
        {
            DisposeProvider();
        }
    }
}
