using AspectSharp.DynamicProxy;
using AspectSharp.Tests.Core.Proxies;
using AspectSharp.Tests.Core.Services;
using AspectSharp.Tests.Core.Services.Interfaces;
using FluentAssertions;
using Xunit;

namespace AspectSharp.Tests.Net7.DynamicProxy
{
    public class AspectContextActivatorTests
    {
        [Fact]
        public void ConstructAspectContextActivatorConstructorFacts()
        {
            var serviceType = typeof(IFakeService);
            var serviceMethod = serviceType.GetMethod(nameof(IFakeService.DoSomethingAsyncWithParameterAndReferenceTypeReturn));
            var proxyService = typeof(FakeServiceProxy);
            var proxyMethod = proxyService.GetMethod(nameof(FakeServiceProxy.DoSomethingAsyncWithParameterAndReferenceTypeReturn));
            var targetService = typeof(FakeService);
            var targetMethod = targetService.GetMethod(nameof(FakeService.DoSomethingAsyncWithParameterAndReferenceTypeReturn));

            var sut = new AspectContextActivator(serviceType, serviceMethod, proxyService, proxyMethod, targetService, targetMethod);

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
        }
    }
}
