using AspectSharp.Abstractions.Enums;
using AspectSharp.Abstractions.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AspectSharp.Abstractions
{
    internal sealed class DynamicProxyFactoryConfigurations
    {
        internal InterceptedEventMethod IncludeAspectsToEvents { get; }
        internal InterceptedPropertyMethod IncludeAspectsToProperties { get; }
        internal bool ExcludeAspectsForMethods { get; }
        internal IEnumerable<GlobalInterceptorConfig> GlobalInterceptors { get; }

        public DynamicProxyFactoryConfigurations() 
        {
            GlobalInterceptors = new List<GlobalInterceptorConfig>();
        }

        public DynamicProxyFactoryConfigurations(
            InterceptedEventMethod includeAspectsToEvents,
            InterceptedPropertyMethod includeAspectsToProperties,
            bool excludeAspectsForMethods,
            IEnumerable<GlobalInterceptorConfig> globalInterceptors)
        {
            if (globalInterceptors is null)
                globalInterceptors = new List<GlobalInterceptorConfig>();
            IncludeAspectsToEvents = includeAspectsToEvents;
            IncludeAspectsToProperties = includeAspectsToProperties;
            ExcludeAspectsForMethods = excludeAspectsForMethods;
            GlobalInterceptors = globalInterceptors;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var globalConfigurationsHashCode = GlobalInterceptors
                    .Aggregate(default(int), (left, right) => left ^ right.GetHashCode());
                return (IncludeAspectsToEvents.GetHashCode() * 17) ^
                       (IncludeAspectsToProperties.GetHashCode() * 37) ^
                       ExcludeAspectsForMethods.GetHashCode() ^
                       globalConfigurationsHashCode;
            }
        }

        internal sealed class GlobalInterceptorConfig
        {
            private readonly IInterceptor _interceptor;
            private readonly IInterceptorMethodMatcher _interceptorMatcher;

            public GlobalInterceptorConfig(IInterceptor interceptor, IInterceptorMethodMatcher interceptorMatcher)
            {
                if (interceptor is null)
                    throw new ArgumentNullException(nameof(interceptor));
                if (interceptorMatcher is null)
                    throw new ArgumentNullException(nameof(interceptorMatcher));
                _interceptor = interceptor;
                _interceptorMatcher = interceptorMatcher;
            }

            public bool TryGetInterceptor(MethodInfo methodInfo, out IInterceptor interceptor)
            {
                interceptor = default;
                if (!_interceptorMatcher.Interceptable(methodInfo))
                    return false;
                interceptor = _interceptor;
                return true;
            }

            public override int GetHashCode()
                => _interceptor.GetType().GetHashCode() ^ _interceptorMatcher.GetType().GetHashCode();
        }
    }
}
