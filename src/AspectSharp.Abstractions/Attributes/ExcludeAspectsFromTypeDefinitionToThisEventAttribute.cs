using AspectSharp.Abstractions.Enums;
using System;
using System.Linq;

namespace AspectSharp.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Event, AllowMultiple = false, Inherited = true)]
    public sealed class ExcludeAspectsFromTypeDefinitionToThisEventAttribute : ExcludeAspectsFromTypeDefinitionAttribute
    {
        public InterceptedEventMethod Methods { get; init; }

        public ExcludeAspectsFromTypeDefinitionToThisEventAttribute(params Type[] aspects) : base(aspects) { }
    }
}
