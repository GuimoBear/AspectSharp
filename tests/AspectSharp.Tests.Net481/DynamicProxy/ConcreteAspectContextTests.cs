using AspectSharp.Abstractions;
using AspectSharp.DynamicProxy;
using AspectSharp.Tests.Core.Aspects;
using AspectSharp.Tests.Core.Proxies;
using AspectSharp.Tests.Core.Services;
using AspectSharp.Tests.Core.Services.Interfaces;
using FluentAssertions;
using Moq;
using System;
using System.Linq;
using Xunit;

namespace AspectSharp.Tests.Net481.DynamicProxy
{
    public class ConcreteAspectContextTests
    {
        [Fact]
        public void ConcreteAspectContextConstructorFacts()
        {

            var serviceType = typeof(IFakeService);
            var serviceMethod = serviceType.GetMethod(nameof(IFakeService.DoSomethingAsyncWithParameterAndReferenceTypeReturn));
            var proxyService = typeof(FakeServiceProxy);
            var proxyMethod = proxyService.GetMethod(nameof(FakeServiceProxy.DoSomethingAsyncWithParameterAndReferenceTypeReturn));
            var targetService = typeof(FakeService);
            var targetMethod = targetService.GetMethod(nameof(FakeService.DoSomethingAsyncWithParameterAndReferenceTypeReturn));

            var activator = new AspectContextActivator(serviceType, serviceMethod, proxyService, proxyMethod, targetService, targetMethod);

            var configs = new DynamicProxyFactoryConfigurations();
            AspectSharp.DynamicProxy.Utils.InterceptorTypeCache.TryGetInterceptedTypeData(serviceType, configs, out _);

            var target = new FakeService();
            object proxy = new FakeServiceProxy(target, Mock.Of<IAspectContextFactory>());
            var parameters = new object[] { 30, "param", Enumerable.Empty<string>() };
            var provider = Mock.Of<IServiceProvider>();

            var sut = new ConcreteAspectContext(activator, target, proxy, parameters, provider);

            sut.ServiceType
                .Should().Be(serviceType);
            sut.ServiceMethod
                .Should().BeSameAs(serviceMethod);
            sut.ProxyType
                .Should().Be(proxyService);
            sut.ProxyMethod
                .Should().BeSameAs(proxyMethod);
            sut.TargetType
                .Should().Be(targetService);
            sut.TargetMethod
                .Should().BeSameAs(targetMethod);

            sut.Target
                .Should().BeSameAs(target);
            sut.Proxy
                .Should().BeSameAs(proxy);
            sut.Parameters
                .Should().BeSameAs(parameters);
            sut.ServiceProvider
                .Should().BeSameAs(provider);

            sut.AdditionalData
                .Should().NotBeNull().And.BeEmpty();

            sut.ReturnValue
                .Should().BeNull();
        }
    }
}
