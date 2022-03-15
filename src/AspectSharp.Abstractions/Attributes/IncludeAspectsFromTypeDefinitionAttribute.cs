using System;
using System.Linq;

namespace AspectSharp.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event, AllowMultiple = false, Inherited = true)]
    public class IncludeAspectsFromTypeDefinitionAttribute : Attribute
    {
        public IncludeAspectsFromTypeDefinitionAttribute(params Type[] aspects)
        {
            Aspects = aspects;
            IncludeAll = Aspects is null || !Aspects.Any();
        }

        public Type[] Aspects { get; }

        public bool IncludeAll { get; }
    }
}
