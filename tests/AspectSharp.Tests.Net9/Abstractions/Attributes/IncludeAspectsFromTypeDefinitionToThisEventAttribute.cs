using AspectSharp.Abstractions.Attributes;
using AspectSharp.Abstractions.Enums;
using AspectSharp.Tests.Net9.Fixtures;
using FluentAssertions;
using Xunit;

namespace AspectSharp.Tests.Net9.Abstractions.Attributes
{
    [Collection(SaveFixtureCollection.NAME)]
    public class IncludeAspectsFromTypeDefinitionToThisEventAttributeTests
    {
        public IncludeAspectsFromTypeDefinitionToThisEventAttributeTests(SaveFixtures _) { }

        [Theory]
        [InlineData(InterceptedEventMethod.None)]
        [InlineData(InterceptedEventMethod.Add)]
        [InlineData(InterceptedEventMethod.Remove)]
        [InlineData(InterceptedEventMethod.All)]
        public void Given_AnEventMethodMethod_When_CreateAttribute_Then_MethodsShouldBeSetted(InterceptedEventMethod expectedMethod)
        {
            var sut = new IncludeAspectsFromTypeDefinitionToThisEventAttribute(expectedMethod);

            sut.Methods.Should().Be(expectedMethod);
        }
    }
}
