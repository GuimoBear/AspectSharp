using AspectSharp.Abstractions;
using AspectSharp.DynamicProxy.Exceptions;
using AspectSharp.DynamicProxy.Factories;
using AspectSharp.DynamicProxy.Utils;
using AspectSharp.Tests.Core.Services;
using AspectSharp.Tests.Core.Services.Interfaces;
using FluentAssertions;
using Xunit;

namespace AspectSharp.Tests.Net5
{
    public class ProxyCreationFacts
    {
        [Fact]
        public void Given_AnNotInterfaceType_When_CreateProxy_Then_ThrowNotInterfaceTypeException()
        {
            var configs = new DynamicProxyFactoryConfigurations(default, default, true, default);

            var targetClass = typeof(FakeService);
            Assert.Throws<NotInterfaceTypeException>(() => DynamicProxyFactory.Create(targetClass, targetClass, new InterceptorTypeCache.InterceptedTypeData(default, default), configs));
        }

        [Fact]
        public void When_CreateProxy_Then_TypesShouldBeNotNull()
        {
            var serviceClass = typeof(IFakeService);
            var targetClass = typeof(FakeService);

            var configs = new DynamicProxyFactoryConfigurations(default, default, true, default);

            InterceptorTypeCache.TryGetInterceptedTypeData(serviceClass, configs, out var interceptedTypeData)
                .Should().BeTrue();

            var proxyClass = DynamicProxyFactory.Create(serviceClass, targetClass, interceptedTypeData, configs);

            proxyClass
                .Should().NotBeNull();

            serviceClass.IsAssignableFrom(proxyClass)
                .Should().BeTrue();
        }
    }
}
