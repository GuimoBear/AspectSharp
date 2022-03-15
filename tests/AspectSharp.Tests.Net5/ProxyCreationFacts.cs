﻿using AspectSharp.Abstractions;
using AspectSharp.DynamicProxy.Factories;
using AspectSharp.DynamicProxy.Utils;
using AspectSharp.Tests.Core.Services;
using AspectSharp.Tests.Core.Services.Interfaces;
using FluentAssertions;
using System;
using System.Linq;
using Xunit;

namespace AspectSharp.Tests.Net5
{
    public class ProxyCreationFacts
    {
        [Fact]
        public void When_CreateProxy_Then_TypesShouldBeNotNull()
        {
            var serviceClass = typeof(IFakeService);
            var targetClass = typeof(FakeService);

            var configs = new DynamicProxyFactoryConfigurations
            {
                ExcludeTypeDefinitionAspectsForMethods = true
            };

            InterceptorTypeCache.TryGetInterceptedTypeData(serviceClass, configs, out var interceptedTypeData)
                .Should().BeTrue();

            var (proxyClass, pipelineClass) = DynamicProxyFactory.Create(serviceClass, targetClass, interceptedTypeData, configs);

            var pipelineInstance = Activator.CreateInstance(pipelineClass);

            proxyClass
                .Should().NotBeNull();

            proxyClass.IsAssignableTo(serviceClass);

            pipelineClass
                .Should().NotBeNull();

            pipelineClass.GetProperties()
                .Length.Should().Be(interceptedTypeData.Interceptors.Keys.Count());
        }
    }
}