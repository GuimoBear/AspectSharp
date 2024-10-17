using AspectSharp.Abstractions.Attributes;
using AspectSharp.Abstractions.Enums;
using FluentAssertions;
using Xunit;

namespace AspectSharp.Tests.Net8.Abstractions.Attributes
{
    public class ExcludeAspectsFromTypeDefinitionToThisEventAttributeTests
    {
        [Theory]
        [InlineData(InterceptedEventMethod.None)]
        [InlineData(InterceptedEventMethod.Add)]
        [InlineData(InterceptedEventMethod.Remove)]
        [InlineData(InterceptedEventMethod.All)]
        public void Given_AnEventMethodMethod_When_CreateAttribute_Then_MethodsShouldBeSetted(InterceptedEventMethod expectedMethod)
        {
            var sut = new ExcludeAspectsFromTypeDefinitionToThisEventAttribute(expectedMethod);

            sut.Methods.Should().Be(expectedMethod);
        }
    }
}
