using AspectSharp.Abstractions;
using AspectSharp.Abstractions.Enums;
using FluentAssertions;
using Xunit;

namespace AspectSharp.Tests.Net6.Abstractions
{
    public class DynamicProxyFactoryConfigurationsBuilderTests
    {
        [Fact]
        public void When_BuildWithoutAnyConfigs_Then_CreatedConfigurationsShouldBeDefault()
        {
            var sut = new DynamicProxyFactoryConfigurationsBuilder();

            var configs = sut.Build();

            configs.ExcludeTypeDefinitionAspectsForMethods
                .Should().BeFalse();

            configs.IncludeTypeDefinitionAspectsToEvents
                .Should().Be(InterceptedEventMethod.None);

            configs.IncludeTypeDefinitionAspectsToProperties
                .Should().Be(InterceptedPropertyMethod.None);
        }

        [Fact]
        public void Given_ExcludeTypeDefinitionsAspectsForMethod_When_BuildConfigs_Then_ExcludeTypeDefinitionsAspectsForMethodShouldBeTrue()
        {
            var sut = new DynamicProxyFactoryConfigurationsBuilder();

            sut.ExcludeTypeDefinitionAspectsForMethods();

            var configs = sut.Build();

            configs.ExcludeTypeDefinitionAspectsForMethods
                .Should().BeTrue();
        }

        [Theory]
        [InlineData(InterceptedPropertyMethod.None)]
        [InlineData(InterceptedPropertyMethod.Get)]
        [InlineData(InterceptedPropertyMethod.Set)]
        [InlineData(InterceptedPropertyMethod.All)]
        public void Given_IncludeTypeDefinitionAspectsToProperties_When_BuildConfigs_Then_IncludeTypeDefinitionAspectsToPropertiesShouldBeExpectedResult(InterceptedPropertyMethod expectedMethods)
        {
            var sut = new DynamicProxyFactoryConfigurationsBuilder();

            sut.IncludeTypeDefinitionAspectsToProperties(expectedMethods);

            var configs = sut.Build();

            configs.IncludeTypeDefinitionAspectsToProperties
                .Should().Be(expectedMethods);
        }

        [Theory]
        [InlineData(InterceptedEventMethod.None)]
        [InlineData(InterceptedEventMethod.Add)]
        [InlineData(InterceptedEventMethod.Remove)]
        [InlineData(InterceptedEventMethod.Raise)]
        [InlineData(InterceptedEventMethod.All)]
        public void Given_IncludeTypeDefinitionAspectsToEvents_When_BuildConfigs_Then_IncludeTypeDefinitionAspectsToEventsShouldBeExpectedResult(InterceptedEventMethod expectedMethods)
        {
            var sut = new DynamicProxyFactoryConfigurationsBuilder();

            sut.IncludeTypeDefinitionAspectsToEvents(expectedMethods);

            var configs = sut.Build();

            configs.IncludeTypeDefinitionAspectsToEvents
                .Should().Be(expectedMethods);
        }
    }
}
