using AspectSharp.Abstractions;
using AspectSharp.DynamicProxy.Utils;
using AspectSharp.Tests.Core.Services;
using AspectSharp.Tests.Core.TestData.DynamicProxy.Utils;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace AspectSharp.Tests.Net5.DynamicProxy.Utils
{
    public class InterceptorTypeCacheTests
    {
        [Fact]
        public void Given_AnNonInterfaceType_When_TryGetInterceptedTypeData_Then_ReturnFalse()
        {
            InterceptorTypeCache.TryGetInterceptedTypeData(typeof(FakeService), out _)
                .Should().BeFalse();
        }

        [Theory]
        [MemberData(nameof(GetInterceptorTypeDataTheoryData))]
        internal void GetInterceptorTypeDataTheory(Type serviceType, DynamicProxyFactoryConfigurations configs, bool expectedIsIntercepted, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>> expectedInterceptors)
        {
            InterceptorTypeCache.TryGetInterceptedTypeData(serviceType, configs, out var interceptedTypeData)
                .Should().Be(expectedIsIntercepted);

            if (expectedIsIntercepted)
            {
                interceptedTypeData.IsIntercepted
                    .Should().Be(expectedIsIntercepted);

                interceptedTypeData.InterceptorAttributes
                    .Should().HaveCount(expectedInterceptors.Count());

                foreach (var (kvp, expectedKvp) in interceptedTypeData.InterceptorAttributes.Zip(expectedInterceptors))
                {
                    var (key, values) = kvp;
                    var (expectedKey, expectedValues) = expectedKvp;

                    key.Should().BeSameAs(expectedKey);

                    values.Should().HaveCount(expectedValues.Count());
                    foreach (var (interceptor, expectedInterceptor) in values.Zip(expectedValues))
                    {
                        interceptor.AttributeType.Should().Be(expectedInterceptor.AttributeType);
                        interceptor.Constructor.Should().BeSameAs(expectedInterceptor.Constructor);
                        interceptor.ConstructorArguments.Should().BeEquivalentTo(expectedInterceptor.ConstructorArguments);
                        interceptor.NamedArguments.Should().BeEquivalentTo(expectedInterceptor.NamedArguments);
                    }
                }
            }
            else
                interceptedTypeData.Should().BeNull();
        }

        public static IEnumerable<object[]> GetInterceptorTypeDataTheoryData()
            => InterceptorTypeCacheData
                  .GetInterceptorTypeDataTheoryData()
                  .Select(tuple => new object[] { tuple.Item1, tuple.Item3, tuple.Item4, tuple.Item5 });
    }
}
