using AspectSharp.Abstractions.Enums;
using System;

namespace AspectSharp.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Event, AllowMultiple = false, Inherited = true)]
    public sealed class ExcludeAspectsFromTypeDefinitionToThisEventAttribute : ExcludeAspectsFromTypeDefinitionAttribute
    {
        public InterceptedEventMethod Methods { get; }

        public ExcludeAspectsFromTypeDefinitionToThisEventAttribute(InterceptedEventMethod methods, params Type[] aspects) : base(aspects) 
        {
            Methods = methods;
        }
    }
}
