using AspectSharp.Abstractions.Attributes;
using AspectSharp.Abstractions.Enums;
using FluentAssertions;
using Xunit;

namespace AspectSharp.Tests.Net5.Abstractions.Attributes
{
    public class IncludeAspectsFromTypeDefinitionToThisEventAttributeTests
    {
        [Theory]
        [InlineData(InterceptedEventMethod.None)]
        [InlineData(InterceptedEventMethod.Add)]
        [InlineData(InterceptedEventMethod.Remove)]
        [InlineData(InterceptedEventMethod.Raise)]
        [InlineData(InterceptedEventMethod.All)]
        public void Given_AnEventMethodMethod_When_CreateAttribute_Then_MethodsShouldBeSetted(InterceptedEventMethod expectedMethod)
        {
            var sut = new IncludeAspectsFromTypeDefinitionToThisEventAttribute()
            {
                Methods = expectedMethod
            };

            sut.Methods.Should().Be(expectedMethod);
        }
    }
}
