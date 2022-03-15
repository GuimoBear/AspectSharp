using AspectSharp.Abstractions.Enums;
using System;
using System.Linq;

namespace AspectSharp.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Event, AllowMultiple = false, Inherited = true)]
    public sealed class IncludeAspectsFromTypeDefinitionToThisPropertyAttribute : IncludeAspectsFromTypeDefinitionAttribute
    {
        public InterceptedPropertyMethod Methods { get; init; }

        public IncludeAspectsFromTypeDefinitionToThisPropertyAttribute(params Type[] aspects) : base(aspects) { }
    }
}
