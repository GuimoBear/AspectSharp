using AspectSharp.Abstractions.Enums;
using System;
using System.Linq;

namespace AspectSharp.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Event, AllowMultiple = false, Inherited = true)]
    public sealed class IncludeAspectsFromTypeDefinitionToThisEventAttribute : IncludeAspectsFromTypeDefinitionAttribute
    {
        public InterceptedEventMethod Methods { get; }

        public IncludeAspectsFromTypeDefinitionToThisEventAttribute(params Type[] aspects) : this(InterceptedEventMethod.None, aspects) { }

        public IncludeAspectsFromTypeDefinitionToThisEventAttribute(InterceptedEventMethod methods, params Type[] aspects) : base(aspects) 
        { 
            Methods = methods;
        }
    }
}
