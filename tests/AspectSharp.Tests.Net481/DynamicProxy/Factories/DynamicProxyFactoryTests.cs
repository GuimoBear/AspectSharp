﻿using AspectSharp.Abstractions;
using AspectSharp.DynamicProxy;
using AspectSharp.DynamicProxy.Factories;
using AspectSharp.DynamicProxy.Utils;
using AspectSharp.Extensions.DependencyInjection;
using AspectSharp.Tests.Core.Fakes;
using AspectSharp.Tests.Core.TestData.DynamicProxy.Factories;
using AspectSharp.Tests.Net481.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace AspectSharp.Tests.Net481.DynamicProxy.Factories
{
    [Collection(AspectSharpCollection.NAME)]
    public class DynamicProxyFactoryTests
    {
        public DynamicProxyFactoryTests(SaveAssemblyFixture _)
        {
            
        }

        [Theory]
        [MemberData(nameof(ProxyClassAspectsPipelineTheoryData))]
        internal void ProxyClassAspectsPipelineExecutionTheory(Type serviceType, Type targetType, DynamicProxyFactoryConfigurations configs, IDictionary<MethodInfo, Tuple<Action<object>, IEnumerable<string>>> methodCallData)
        {
            if (InterceptorTypeCache.TryGetInterceptedTypeData(serviceType, configs, out var interceptedTypeData))
            {
                var proxyType = DynamicProxyFactory.Create(serviceType, targetType, interceptedTypeData, configs);

                var services = new ServiceCollection();

                services
                    .AddSingleton(serviceType, targetType)
                    .AddAspects(_configs =>
                    {
                        _includeAspectsToEventsFieldInfo.SetValue(_configs, configs.IncludeAspectsToEvents);
                        _includeAspectsToPropertiesFieldInfo.SetValue(_configs, configs.IncludeAspectsToProperties);
                        _excludeAspectsForMethodsFieldInfo.SetValue(_configs, configs.ExcludeAspectsForMethods);
                        _globalInterceptorFieldInfo.SetValue(_configs, configs.GlobalInterceptors);
                    });

                services.Remove(services.First(sd => sd.ServiceType == typeof(IAspectContextFactory)));
                services.AddSingleton<IAspectContextFactory, FakeAspectContextFactory>();
                //.AddSingleton(targetType)
                //.AddSingleton(serviceType, proxyType);

                using (var serviceProvider = services.BuildServiceProvider(true))
                {
                    foreach (var kvp in methodCallData)
                    {
                        var method = kvp.Key;
                        var methodData = kvp.Value;

                        var methodDelegate = methodData.Item1;
                        var expectedAditionalDataKeys = methodData.Item2;

                        var contextFactory = serviceProvider.GetService<IAspectContextFactory>() as FakeAspectContextFactory;
                        var proxyInstance = serviceProvider.GetService(serviceType);
                        methodDelegate(proxyInstance);
                        if (expectedAditionalDataKeys.Any())
                        {
                            var aditionalDataKeys = contextFactory.CurrentContext.AdditionalData.Keys;
                            aditionalDataKeys
                                .Should().HaveCount(expectedAditionalDataKeys.Count(), because: string.Format("method '{0}' should be the same number of aspects", method.Name));

                            foreach (var (key, expectedKey) in Enumerable.Zip(aditionalDataKeys, expectedAditionalDataKeys, (left, right) => new Tuple<string, string>(left, right)))
                                key.Should().Be(expectedKey, because: string.Format("method '{0}' should be added '{1}' in aspected context, but founded {2}", method.Name, key, expectedKey));
                        }
                    }
                }
            }
        }

        public static IEnumerable<object[]> ProxyClassAspectsPipelineTheoryData()
            => DynamicProxyFactoryData
                  .ProxyClassAspectsPipelineTheoryData()
                  .Select(tuple => new object[] { tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4 });

        private static readonly FieldInfo _includeAspectsToEventsFieldInfo = typeof(DynamicProxyFactoryConfigurationsBuilder).GetField("_includeAspectsToEvents", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _includeAspectsToPropertiesFieldInfo = typeof(DynamicProxyFactoryConfigurationsBuilder).GetField("_includeAspectsToProperties", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _excludeAspectsForMethodsFieldInfo = typeof(DynamicProxyFactoryConfigurationsBuilder).GetField("_excludeAspectsForMethods", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _globalInterceptorFieldInfo = typeof(DynamicProxyFactoryConfigurationsBuilder).GetField("_globalInterceptor", BindingFlags.NonPublic | BindingFlags.Instance);
    }
}