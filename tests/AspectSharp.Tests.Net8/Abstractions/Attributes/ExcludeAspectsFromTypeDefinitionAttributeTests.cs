using AspectSharp.Abstractions.Attributes;
using FluentAssertions;
using System;
using Xunit;

namespace AspectSharp.Tests.Net8.Abstractions.Attributes
{
    public class ExcludeAspectsFromTypeDefinitionAttributeTests
    {
        [Fact]
        public void Given_NoTypesInConstructor_When_CreateAttribute_Then_ExcludeAllShouldBeTrue()
        {
            var sut = new ExcludeAspectsFromTypeDefinitionAttribute();

            sut.Aspects.Should().BeEmpty();

            sut.ExcludeAll
                .Should().BeTrue();
        }

        [Theory]
        [InlineData(typeof(ExcludeAspectsFromTypeDefinitionAttributeTests))]
        public void Given_AnyTypeInConstructor_When_CreateAttribute_Then_ExcludeAllShouldBeFalse(Type aspectType)
        {
            var sut = new ExcludeAspectsFromTypeDefinitionAttribute(aspectType);

            sut.Aspects.Should().HaveCount(1);

            sut.Aspects.Should().Contain(aspectType);

            sut.ExcludeAll
                .Should().BeFalse();
        }
    }
}
