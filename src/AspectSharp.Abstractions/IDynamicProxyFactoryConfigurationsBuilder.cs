using AspectSharp.Abstractions.Attributes;
using AspectSharp.Abstractions.Enums;
using AspectSharp.Abstractions.Global;
using System;

namespace AspectSharp.Abstractions
{
    public interface IDynamicProxyFactoryConfigurationsBuilder
    {
        /// <summary>
        /// Enable interceptions to events. <br /><br />
        /// To enable interceptions to a specific event, use <see cref="IncludeAspectsFromTypeDefinitionToThisEventAttribute"/> attribute<br />
        /// With this configuration enabled, disable interceptions to a specific event using <see cref="ExcludeAspectsFromTypeDefinitionToThisEventAttribute"/> attribute
        /// </summary>
        /// <param name="eventMethodsToIntercept">The methods to be intercepted</param>
        IDynamicProxyFactoryConfigurationsBuilder IncludeAspectsToEvents(InterceptedEventMethod eventMethodsToIntercept = InterceptedEventMethod.All);

        /// <summary>
        /// Enable interceptions to properties. <br /><br />
        /// To enable interceptions to a specific property, use <see cref="IncludeAspectsFromTypeDefinitionToThisPropertyAttribute"/> attribute<br /> 
        /// With this configuration enabled, disable interceptions to a specific property using <see cref="ExcludeAspectsFromTypeDefinitionToThisPropertyAttribute"/> attribute
        /// </summary>
        /// <param name="propertyMethodsToIntercept">The methods to be intercepted</param>
        IDynamicProxyFactoryConfigurationsBuilder IncludeAspectsToProperties(InterceptedPropertyMethod propertyMethodsToIntercept = InterceptedPropertyMethod.All);

        /// <summary>
        /// Disable interceptions from type definition aspects in methods. <br /><br />
        /// By default, method interceptions from type definition aspects is enabled, to disable interceptions to a specific method, use <see cref="ExcludeAspectsFromTypeDefinitionAttribute"/><br />
        /// With this configuration enabled, use <see cref="IncludeAspectsFromTypeDefinitionAttribute"/> to enable interceptions to a expecific method
        /// </summary>
        IDynamicProxyFactoryConfigurationsBuilder ExcludeAspectsForMethods();

        IDynamicProxyFactoryConfigurationsBuilder WithInterceptor<TInterceptor>() 
            where TInterceptor : class, IInterceptor, new();

        IDynamicProxyFactoryConfigurationsBuilder WithInterceptor<TInterceptor>(Action<IInterceptorConfig> configure)
            where TInterceptor : class, IInterceptor, new();

        IDynamicProxyFactoryConfigurationsBuilder WithInterceptor<TInterceptor>(TInterceptor instance)
            where TInterceptor : class, IInterceptor;

        IDynamicProxyFactoryConfigurationsBuilder WithInterceptor<TInterceptor>(TInterceptor instance, Action<IInterceptorConfig> configure)
            where TInterceptor : class, IInterceptor;

        IDynamicProxyFactoryConfigurationsBuilder IgnoreErrorsWhileTryingInjectAspects();
    }
}
