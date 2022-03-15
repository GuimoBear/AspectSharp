using AspectSharp.Abstractions.Enums;
using System;
using System.Linq;

namespace AspectSharp.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Event, AllowMultiple = false, Inherited = true)]
    public sealed class IncludeAspectsFromTypeDefinitionToThisPropertyAttribute : IncludeAspectsFromTypeDefinitionAttribute
    {
        public InterceptedPropertyMethod Methods { get; }

        public IncludeAspectsFromTypeDefinitionToThisPropertyAttribute(InterceptedPropertyMethod methods, params Type[] aspects) : base(aspects) 
        {
            Methods = methods;
        }
    }
}
