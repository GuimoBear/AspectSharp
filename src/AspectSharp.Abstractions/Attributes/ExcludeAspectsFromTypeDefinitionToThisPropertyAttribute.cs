using AspectSharp.Abstractions.Enums;
using System;

namespace AspectSharp.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class IncludeAspectsFromTypeDefinitionToThisPropertyAttribute : IncludeAspectsFromTypeDefinitionAttribute
    {
        public InterceptedPropertyMethod Methods { get; }

        public IncludeAspectsFromTypeDefinitionToThisPropertyAttribute(params Type[] aspects) : this(InterceptedPropertyMethod.None, aspects) { }

        public IncludeAspectsFromTypeDefinitionToThisPropertyAttribute(InterceptedPropertyMethod methods, params Type[] aspects) : base(aspects) 
        {
            Methods = methods;
        }
    }
}
