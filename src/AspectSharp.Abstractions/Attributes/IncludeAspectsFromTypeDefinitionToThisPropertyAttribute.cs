using AspectSharp.Abstractions.Enums;
using System;

namespace AspectSharp.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Event, AllowMultiple = false, Inherited = true)]
    public sealed class ExcludeAspectsFromTypeDefinitionToThisPropertyAttribute : ExcludeAspectsFromTypeDefinitionAttribute
    {
        public InterceptedPropertyMethod Methods { get; }

        public ExcludeAspectsFromTypeDefinitionToThisPropertyAttribute(InterceptedPropertyMethod methods, params Type[] aspects) : base(aspects) 
        {
            Methods = methods;
        }
    }
}
