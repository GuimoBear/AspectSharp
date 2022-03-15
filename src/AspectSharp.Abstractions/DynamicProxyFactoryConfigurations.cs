using AspectSharp.Abstractions.Enums;
using System;

namespace AspectSharp.Abstractions
{
    internal sealed class DynamicProxyFactoryConfigurations
    {
        internal InterceptedEventMethod IncludeTypeDefinitionAspectsToEvents { get; init; }
        internal InterceptedPropertyMethod IncludeTypeDefinitionAspectsToProperties { get; init; }
        internal bool ExcludeTypeDefinitionAspectsForMethods { get; init; }

        public override int GetHashCode()
            => HashCode.Combine(IncludeTypeDefinitionAspectsToEvents, IncludeTypeDefinitionAspectsToProperties, ExcludeTypeDefinitionAspectsForMethods);
    }
}
