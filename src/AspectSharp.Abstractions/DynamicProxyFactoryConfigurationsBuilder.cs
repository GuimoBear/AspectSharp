using AspectSharp.Abstractions.Enums;
using AspectSharp.Abstractions.Global;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AspectSharp.Abstractions
{
    internal sealed class DynamicProxyFactoryConfigurationsBuilder : IDynamicProxyFactoryConfigurationsBuilder
    {
        private InterceptedEventMethod _includeAspectsToEvents;
        private InterceptedPropertyMethod _includeAspectsToProperties;
        private bool _excludeAspectsForMethods;
        private ICollection<DynamicProxyFactoryConfigurations.GlobalInterceptorConfig> _globalInterceptor = new List<DynamicProxyFactoryConfigurations.GlobalInterceptorConfig>();

        public IDynamicProxyFactoryConfigurationsBuilder IncludeAspectsToEvents(InterceptedEventMethod eventMethodsToIntercept = InterceptedEventMethod.All)
        {
            _includeAspectsToEvents = eventMethodsToIntercept;
            return this;
        }

        public IDynamicProxyFactoryConfigurationsBuilder IncludeAspectsToProperties(InterceptedPropertyMethod propertyMethodsToIntercept = InterceptedPropertyMethod.All)
        {
            _includeAspectsToProperties = propertyMethodsToIntercept;
            return this;
        }

        public IDynamicProxyFactoryConfigurationsBuilder ExcludeAspectsForMethods()
        {
            _excludeAspectsForMethods = true;
            return this;
        }

        internal DynamicProxyFactoryConfigurations Build()
        {
            return new DynamicProxyFactoryConfigurations(
                _includeAspectsToEvents, 
                _includeAspectsToProperties, 
                _excludeAspectsForMethods, 
                _globalInterceptor);
        }

        public IDynamicProxyFactoryConfigurationsBuilder WithInterceptor<TInterceptor>() where TInterceptor : class, IInterceptor, new()
        {
            _globalInterceptor.Add(new DynamicProxyFactoryConfigurations.GlobalInterceptorConfig(new TInterceptor(), InterceptorConfig<TInterceptor>.AllowAllMathods));
            return this;
        }

        public IDynamicProxyFactoryConfigurationsBuilder WithInterceptor<TInterceptor>(Action<IInterceptorConfig> configure) where TInterceptor : class, IInterceptor, new()
        {
            var configurations = new InterceptorConfig<TInterceptor>(new TInterceptor());
            configure(configurations);
            _globalInterceptor.Add(new DynamicProxyFactoryConfigurations.GlobalInterceptorConfig(configurations.Interceptor, configurations));
            return this;
        }

        public IDynamicProxyFactoryConfigurationsBuilder WithInterceptor<TInterceptor>(TInterceptor instance) where TInterceptor : class, IInterceptor
        {
            _globalInterceptor.Add(new DynamicProxyFactoryConfigurations.GlobalInterceptorConfig(instance, InterceptorConfig<TInterceptor>.AllowAllMathods));
            return this;
        }

        public IDynamicProxyFactoryConfigurationsBuilder WithInterceptor<TInterceptor>(TInterceptor instance, Action<IInterceptorConfig> configure) where TInterceptor : class, IInterceptor
        {
            var configurations = new InterceptorConfig<TInterceptor>(instance);
            configure(configurations);
            _globalInterceptor.Add(new DynamicProxyFactoryConfigurations.GlobalInterceptorConfig(configurations.Interceptor, configurations));
            return this;
        }
    }
}
