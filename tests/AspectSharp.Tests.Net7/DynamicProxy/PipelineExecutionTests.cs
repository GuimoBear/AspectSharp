using AspectSharp.Abstractions;
using AspectSharp.Abstractions.Enums;
using AspectSharp.DynamicProxy;
using AspectSharp.DynamicProxy.Factories;
using AspectSharp.DynamicProxy.Utils;
using AspectSharp.Tests.Core.Fakes;
using AspectSharp.Tests.Core.Services;
using AspectSharp.Tests.Core.Services.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AspectSharp.Tests.Net7.DynamicProxy
{
    public class PipelineExecutionTests
    {
        [Fact]
        public void Given_AnService_When_ExecuteFunction_Then_PipelineExecutionShouldBeFullyExecuted()
        {
            var serviceType = typeof(IService);
            var targetType = typeof(Service);
            var configs = new DynamicProxyFactoryConfigurations(InterceptedEventMethod.None, InterceptedPropertyMethod.None, false, default);

            InterceptorTypeCache.TryGetInterceptedTypeData(serviceType, configs, out var interceptedTypeData);

            var proxyType = DynamicProxyFactory.Create(serviceType, targetType, interceptedTypeData, configs);
            var services = new ServiceCollection();

            services
                .AddSingleton<IAspectContextFactory, FakeAspectContextFactory>()
                .AddSingleton(targetType)
                .AddSingleton(serviceType, proxyType);

            using(var serviceProvider = services.BuildServiceProvider(true))
            using(var scope = serviceProvider.CreateScope())
            {
                var service = scope.ServiceProvider.GetService<IService>();

                var ret = service.Function();

                var dict = (scope.ServiceProvider.GetService<IAspectContextFactory>() as FakeAspectContextFactory).Context.AdditionalData;

                dict.Should().HaveCount(9);

                dict.Keys.Skip(4).First().Should().Be("Service.Function");
            }
        }

        [Fact]
        public async Task Given_AnService_When_ExecuteAsyncFunction_Then_PipelineExecutionShouldBeFullyExecuted()
        {
            var serviceType = typeof(IService);
            var targetType = typeof(Service);
            var configs = new DynamicProxyFactoryConfigurations(InterceptedEventMethod.None, InterceptedPropertyMethod.None, false, default);

            InterceptorTypeCache.TryGetInterceptedTypeData(serviceType, configs, out var interceptedTypeData);

            var proxyType = DynamicProxyFactory.Create(serviceType, targetType, interceptedTypeData, configs);
            var services = new ServiceCollection();

            services
                .AddSingleton<IAspectContextFactory, FakeAspectContextFactory>()
                .AddSingleton(targetType)
                .AddSingleton(serviceType, proxyType);

            using (var serviceProvider = services.BuildServiceProvider(true))
            using (var scope = serviceProvider.CreateScope())
            {
                var service = scope.ServiceProvider.GetService<IService>();

                var ret = await service.AsyncFunction();

                var dict = (scope.ServiceProvider.GetService<IAspectContextFactory>() as FakeAspectContextFactory).Context.AdditionalData;

                dict.Should().HaveCount(9);

                dict.Keys.Skip(4).First().Should().Be("Service.AsyncFunction");
            }
        }

        [Fact]
        public async ValueTask Given_AnService_When_ExecuteValueAsyncFunction_Then_PipelineExecutionShouldBeFullyExecuted()
        {
            var serviceType = typeof(IService);
            var targetType = typeof(Service);
            var configs = new DynamicProxyFactoryConfigurations(InterceptedEventMethod.None, InterceptedPropertyMethod.None, false, default);

            InterceptorTypeCache.TryGetInterceptedTypeData(serviceType, configs, out var interceptedTypeData);

            var proxyType = DynamicProxyFactory.Create(serviceType, targetType, interceptedTypeData, configs);
            var services = new ServiceCollection();

            services
                .AddSingleton<IAspectContextFactory, FakeAspectContextFactory>()
                .AddSingleton(targetType)
                .AddSingleton(serviceType, proxyType);

            using (var serviceProvider = services.BuildServiceProvider(true))
            using (var scope = serviceProvider.CreateScope())
            {
                var service = scope.ServiceProvider.GetService<IService>();

                var ret = await service.ValueAsyncFunction();

                var dict = (scope.ServiceProvider.GetService<IAspectContextFactory>() as FakeAspectContextFactory).Context.AdditionalData;

                dict.Should().HaveCount(9);

                dict.Keys.Skip(4).First().Should().Be("Service.ValueAsyncFunction");
            }
        }
    }
}
