using AspectSharp.Abstractions.Enums;

namespace AspectSharp.Abstractions
{
    internal sealed class DynamicProxyFactoryConfigurationsBuilder : IDynamicProxyFactoryConfigurationsBuilder
    {
        private InterceptedEventMethod _includeTypeDefinitionAspectsToEvents;
        private InterceptedPropertyMethod _includeTypeDefinitionAspectsToProperties;
        private bool _excludeTypeDefinitionAspectsForMethods;

        public IDynamicProxyFactoryConfigurationsBuilder IncludeTypeDefinitionAspectsToEvents(InterceptedEventMethod eventMethodsToIntercept = InterceptedEventMethod.All)
        {
            _includeTypeDefinitionAspectsToEvents = eventMethodsToIntercept;
            return this;
        }

        public IDynamicProxyFactoryConfigurationsBuilder IncludeTypeDefinitionAspectsToProperties(InterceptedPropertyMethod propertyMethodsToIntercept = InterceptedPropertyMethod.All)
        {
            _includeTypeDefinitionAspectsToProperties = propertyMethodsToIntercept;
            return this;
        }

        public IDynamicProxyFactoryConfigurationsBuilder ExcludeTypeDefinitionAspectsForMethods()
        {
            _excludeTypeDefinitionAspectsForMethods = true;
            return this;
        }

        internal DynamicProxyFactoryConfigurations Build()
        {
            return new DynamicProxyFactoryConfigurations(
                _includeTypeDefinitionAspectsToEvents, 
                _includeTypeDefinitionAspectsToProperties, 
                _excludeTypeDefinitionAspectsForMethods);
        }
    }
}
