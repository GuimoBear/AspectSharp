using AspectSharp.Abstractions.Enums;
using System;

namespace AspectSharp.Abstractions
{
    internal sealed class DynamicProxyFactoryConfigurations
    {
        internal InterceptedEventMethod IncludeTypeDefinitionAspectsToEvents { get; }
        internal InterceptedPropertyMethod IncludeTypeDefinitionAspectsToProperties { get; }
        internal bool ExcludeTypeDefinitionAspectsForMethods { get; }

        public DynamicProxyFactoryConfigurations() { }

        public DynamicProxyFactoryConfigurations(
            InterceptedEventMethod includeTypeDefinitionAspectsToEvents,
            InterceptedPropertyMethod includeTypeDefinitionAspectsToProperties,
            bool excludeTypeDefinitionAspectsForMethods)
        {
            IncludeTypeDefinitionAspectsToEvents = includeTypeDefinitionAspectsToEvents;
            IncludeTypeDefinitionAspectsToProperties = includeTypeDefinitionAspectsToProperties;
            ExcludeTypeDefinitionAspectsForMethods = excludeTypeDefinitionAspectsForMethods;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (IncludeTypeDefinitionAspectsToEvents.GetHashCode() * 17) ^
                       (IncludeTypeDefinitionAspectsToProperties.GetHashCode() * 37) ^
                       ExcludeTypeDefinitionAspectsForMethods.GetHashCode();
            }
        }
    }
}
