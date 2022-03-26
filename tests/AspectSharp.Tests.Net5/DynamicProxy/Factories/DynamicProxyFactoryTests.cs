using AspectSharp.Abstractions;
using AspectSharp.DynamicProxy;
using AspectSharp.DynamicProxy.Factories;
using AspectSharp.DynamicProxy.Utils;
using AspectSharp.Tests.Core.Proxies;
using AspectSharp.Tests.Core.Services;
using AspectSharp.Tests.Core.Services.Interfaces;
using AspectSharp.Tests.Core.TestData.DynamicProxy.Factories;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace AspectSharp.Tests.Net5.DynamicProxy.Factories
{
    public class DynamicProxyFactoryTests
    {
        [Theory]
        [MemberData(nameof(ProxyClassAspectsPipelineTheoryData))]
        internal async Task ProxyClassAspectsPipelineExecutionTheory(Type serviceType, Type targetType, DynamicProxyFactoryConfigurations configs, IDictionary<MethodInfo, Tuple<Action<object>, IEnumerable<string>>> methodCallData)
        {

            if (InterceptorTypeCache.TryGetInterceptedTypeData(serviceType, configs, out var interceptedTypeData))
            {
                var proxyType = DynamicProxyFactory.Create(serviceType, targetType, interceptedTypeData, configs);
                try
                {
                    var generator = new Lokad.ILPack.AssemblyGenerator();
                    var bytes = generator.GenerateAssemblyBytes(DynamicProxyFactory._proxiedClassesAssemblyBuilder);
                    if (File.Exists(@"C:\projetos\teste.dll"))
                        File.Delete(@"C:\projetos\teste.dll");
                    using (var file = File.Create(@"C:\projetos\teste.dll"))
                    {
                        file.Write(bytes);
                        file.Flush();
                        file.Close();
                    }
                }
                catch (Exception ex)
                {

                }
                var services = new ServiceCollection();
                /*
                if (targetType == typeof(FakeService))
                {
                    services
                        .AddSingleton<IAspectContextFactory, FakeAspectContextFactory>()
                        .AddSingleton(targetType)
                        .AddSingleton(serviceType, typeof(FakeServiceProxy));

                    using (var serviceProvider = services.BuildServiceProvider(true))
                    {
                        var contextFactory = serviceProvider.GetService<IAspectContextFactory>() as FakeAspectContextFactory;
                        var proxyInstance = serviceProvider.GetService<IFakeService>();

                        try
                        {
                            var asyncStateMachine = typeof(FakeServiceProxyPipelines).GetMembers(BindingFlags.NonPublic)[0];
                            await proxyInstance.InterceptedDoSomethingAsyncWithoutParameterAndWithoutReturn();
                            var vlr = await proxyInstance.InterceptedDoSomethingAsyncWithParameterAndValueTypeReturn(5, "", Enumerable.Empty<string>());
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                    services = new ServiceCollection();
                }
                */
                services
                    .AddSingleton<IAspectContextFactory, FakeAspectContextFactory>()
                    .AddSingleton(targetType)
                    .AddSingleton(serviceType, proxyType);

                using (var serviceProvider = services.BuildServiceProvider(true))
                {
                    foreach (var (method, methodData) in methodCallData)
                    {
                        var (methodDelegate, expectedAditionalDataKeys) = methodData;

                        var contextFactory = serviceProvider.GetService<IAspectContextFactory>() as FakeAspectContextFactory;
                        var proxyInstance = serviceProvider.GetService(serviceType);
                        methodDelegate(proxyInstance);
                        if (expectedAditionalDataKeys.Any())
                        {
                            var aditionalDataKeys = contextFactory.Context.AdditionalData.Keys;
                            aditionalDataKeys
                                .Should().HaveCount(expectedAditionalDataKeys.Count());

                            foreach (var (key, expectedKey) in aditionalDataKeys.Zip(expectedAditionalDataKeys))
                                key.Should().Be(expectedKey);
                        }
                    }
                }
            }
        }

        private class FakeAspectContextFactory : IAspectContextFactory
        {
            private readonly IServiceProvider _serviceProvider;

            public AspectContext Context { get; private set; }

            public FakeAspectContextFactory(IServiceProvider serviceProvider)
                => _serviceProvider = serviceProvider;

            public AspectContext CreateContext(AspectContextActivator activator, object target, object proxy, object[] parameters)
                => Context = new ConcreteAspectContext(activator, target, proxy, parameters, _serviceProvider);
        }

        public static IEnumerable<object[]> ProxyClassAspectsPipelineTheoryData()
            => DynamicProxyFactoryData
                  .ProxyClassAspectsPipelineTheoryData()
                  .Select(tuple => new object[] { tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4 });
    }
}
