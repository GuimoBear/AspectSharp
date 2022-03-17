using AspectCore.Configuration;
using AspectCore.Extensions.DependencyInjection;
using AspectSharp.Benchmarks.Aspects;
using AspectSharp.Benchmarks.Clients;
using AspectSharp.Benchmarks.Clients.Interfaces;
using AspectSharp.Benchmarks.Services;
using AspectSharp.Benchmarks.Services.Interfaces;
using AspectSharp.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace AspectSharp.Benchmarks.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services
                .AddLogging(logging =>
                {
                    logging
                        .SetMinimumLevel(LogLevel.Warning)
                        .AddConsole();
                })
                .AddScoped<IMetricsClient, MetricsClient>()
                .AddScoped<IFakeService, FakeService>()
                .AddScoped<IAnotherFakeService, AnotherFakeService>()
                .AddScoped<IMetrifiedFakeService, MetrifiedFakeService>();
            return services;
        }

        public static IServiceCollection AddAspectCore(this IServiceCollection services)
        {
            services
                .AddSingleton<AspectCoreMetrifyMethodAttribute>()
                .ConfigureDynamicProxy();
            return services;
        }

        public static IServiceCollection AddAspectSharp(this IServiceCollection services)
        {
            services
                .AddAspects();
            return services;
        }
    }
}
