using AspectSharp.Abstractions.Attributes;
using FluentAssertions;
using System;
using Xunit;

namespace AspectSharp.Tests.Net8.Abstractions.Attributes
{
    public class IncludeAspectsFromTypeDefinitionAttributeTests
    {
        [Fact]
        public void Given_NoTypesInConstructor_When_CreateAttribute_Then_ExcludeAllShouldBeTrue()
        {
            var sut = new IncludeAspectsFromTypeDefinitionAttribute();

            sut.Aspects.Should().BeEmpty();

            sut.IncludeAll
                .Should().BeTrue();
        }

        [Theory]
        [InlineData(typeof(IncludeAspectsFromTypeDefinitionAttributeTests))]
        public void Given_AnyTypeInConstructor_When_CreateAttribute_Then_IncludeAllShouldBeFalse(Type aspectType)
        {
            var sut = new IncludeAspectsFromTypeDefinitionAttribute(aspectType);

            sut.Aspects.Should().HaveCount(1);

            sut.Aspects.Should().Contain(aspectType);

            sut.IncludeAll
                .Should().BeFalse();
        }
    }
}
