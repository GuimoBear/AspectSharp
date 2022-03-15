using AspectSharp.Abstractions.Enums;
using System;

namespace AspectSharp.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ExcludeAspectsFromTypeDefinitionToThisPropertyAttribute : ExcludeAspectsFromTypeDefinitionAttribute
    {
        public InterceptedPropertyMethod Methods { get; }

        public ExcludeAspectsFromTypeDefinitionToThisPropertyAttribute(params Type[] aspects) : this(InterceptedPropertyMethod.None, aspects) { }

        public ExcludeAspectsFromTypeDefinitionToThisPropertyAttribute(InterceptedPropertyMethod methods, params Type[] aspects) : base(aspects) 
        {
            Methods = methods;
        }
    }
}
