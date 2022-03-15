using System;
using System.Linq;

namespace AspectSharp.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event, AllowMultiple = false, Inherited = true)]
    public class ExcludeAspectsFromTypeDefinitionAttribute : Attribute
    {
        public ExcludeAspectsFromTypeDefinitionAttribute(params Type[] aspects)
        {
            Aspects = aspects;
            ExcludeAll = Aspects is null || !Aspects.Any();
        }

        public Type[] Aspects { get; }

        public bool ExcludeAll { get; }
    }
}
