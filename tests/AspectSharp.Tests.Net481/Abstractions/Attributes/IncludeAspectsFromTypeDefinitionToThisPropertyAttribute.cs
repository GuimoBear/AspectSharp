﻿using AspectSharp.Abstractions.Attributes;
using AspectSharp.Abstractions.Enums;
using FluentAssertions;
using Xunit;

namespace AspectSharp.Tests.Net481.Abstractions.Attributes
{
    public class IncludeAspectsFromTypeDefinitionToThisPropertyAttributeTests
    {
        [Theory]
        [InlineData(InterceptedPropertyMethod.None)]
        [InlineData(InterceptedPropertyMethod.Get)]
        [InlineData(InterceptedPropertyMethod.Set)]
        [InlineData(InterceptedPropertyMethod.All)]
        public void Given_AnPropertyMethodMethod_When_CreateAttribute_Then_MethodsShouldBeSetted(InterceptedPropertyMethod expectedMethod)
        {
            var sut = new IncludeAspectsFromTypeDefinitionToThisPropertyAttribute(expectedMethod);

            sut.Methods.Should().Be(expectedMethod);
        }
    }
}
