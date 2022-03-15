using AspectSharp.Abstractions;
using AspectSharp.Abstractions.Attributes;
using AspectSharp.Abstractions.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AspectSharp.DynamicProxy.Utils
{
    internal static class InterceptorTypeCache
    {
        private static readonly Type _abstractInterceptorType = typeof(AbstractInterceptorAttribute);

        private static readonly ConcurrentDictionary<Tuple<Type, int>, InterceptedTypeData> _cachedTypes = new ConcurrentDictionary<Tuple<Type, int>, InterceptedTypeData>();

        internal static ConcurrentDictionary<int, DynamicProxyFactoryConfigurations> _previouslyUsedConfigurations { get; private set; } = new ConcurrentDictionary<int, DynamicProxyFactoryConfigurations>();

        internal static DynamicProxyFactoryConfigurations Configurations { get; private set; } = new DynamicProxyFactoryConfigurations();

        public static void SetConfigurations(DynamicProxyFactoryConfigurations configurations)
            => Configurations = configurations ?? Configurations;

        internal static bool TryGetInterceptedTypeData(Type type, out InterceptedTypeData interceptedTypeData)
            => TryGetInterceptedTypeData(type, Configurations, out interceptedTypeData);

        internal static bool TryGetInterceptedTypeData(Type type, int previouslyUsedConfigurationsHashCode, out InterceptedTypeData interceptedTypeData)
        {
            _ = _previouslyUsedConfigurations.TryGetValue(previouslyUsedConfigurationsHashCode, out var configs);
            return TryGetInterceptedTypeData(type, configs, out interceptedTypeData);
        }

        internal static bool TryGetInterceptedTypeData(Type type, DynamicProxyFactoryConfigurations configs, out InterceptedTypeData interceptedTypeData)
        {
            configs = configs ?? Configurations;
            if (_cachedTypes.TryGetValue(new Tuple<Type, int>(type, configs.GetHashCode()), out interceptedTypeData))
                return true;

            _ = _previouslyUsedConfigurations.TryAdd(configs.GetHashCode(), configs);

            var typeDefinitionAttributes = type
                .CustomAttributes
                    .Where(attr => _abstractInterceptorType.IsAssignableFrom(attr.AttributeType))
                    .ToList();

            var methodInterceptorDictionary = type
                .GetMethods()
                    .Where(mi => typeDefinitionAttributes.Any() || mi.CustomAttributes.Any(attr => _abstractInterceptorType.IsAssignableFrom(attr.AttributeType)))
                    .ToDictionary(mi => mi, mi => typeDefinitionAttributes.Concat(mi.CustomAttributes.Where(attr => _abstractInterceptorType.IsAssignableFrom(attr.AttributeType)).ToList()));

            if (methodInterceptorDictionary.Any())
            {
                var properties = type.GetProperties();
                var events = type.GetEvents();

                var interceptors = new Dictionary<MethodInfo, IEnumerable<CustomAttributeData>>();
                foreach(var tuple in methodInterceptorDictionary)
                {
                    var methodInfo = tuple.Key;
                    var allMethodInterceptors = tuple.Value;
                    var typeDefinitionInterceptors = allMethodInterceptors.Take(typeDefinitionAttributes.Count).ToList();
                    var methodInterceptors = allMethodInterceptors.Skip(typeDefinitionAttributes.Count).ToList();

                    var propertyFromMethod = properties.FirstOrDefault(prop => prop.GetGetMethod() == methodInfo || prop.GetSetMethod() == methodInfo);
                    var eventFromMethod = events.FirstOrDefault(evt => evt.GetAddMethod() == methodInfo || evt.GetRemoveMethod() == methodInfo || evt.GetRaiseMethod() == methodInfo);

                    if (!(propertyFromMethod is null))
                        typeDefinitionAttributes = propertyFromMethod.FilterTypeDefinitionInterceptorsFromProperty(methodInfo, configs, typeDefinitionAttributes).ToList();
                    else if (!(eventFromMethod is null))
                        typeDefinitionAttributes = eventFromMethod.FilterTypeDefinitionInterceptorsFromEvent(methodInfo, configs, typeDefinitionAttributes).ToList();
                    else
                    {
                        if (configs.ExcludeTypeDefinitionAspectsForMethods)
                        {
                            typeDefinitionInterceptors = Enumerable.Empty<CustomAttributeData>().ToList();
                            var attribute = methodInfo.GetCustomAttributes(true).FirstOrDefault(attr => attr.GetType() == typeof(IncludeAspectsFromTypeDefinitionAttribute));
                            if (attribute is IncludeAspectsFromTypeDefinitionAttribute includeAspects)
                            {
                                typeDefinitionInterceptors = includeAspects.IncludeAspectsFromTypeDefinition(allMethodInterceptors.Take(typeDefinitionAttributes.Count).ToList()).ToList();
                                if (includeAspects.IncludeAll)
                                    break;
                            }
                        }
                        else
                        {
                            var attribute = methodInfo.GetCustomAttributes(true).FirstOrDefault(attr => attr.GetType() == typeof(ExcludeAspectsFromTypeDefinitionAttribute));
                            if (attribute is ExcludeAspectsFromTypeDefinitionAttribute excludeAspects)
                            {
                                typeDefinitionInterceptors = excludeAspects.ExcludeAspectsFromTypeDefinition(typeDefinitionInterceptors).ToList();
                                if (excludeAspects.ExcludeAll)
                                    break;
                            }
                        }
                    }
                    var acceptedInterceptors = typeDefinitionInterceptors.Concat(methodInterceptors).ToList();
                    if (acceptedInterceptors.Any())
                        interceptors.Add(methodInfo, acceptedInterceptors);
                }
                if (interceptors.Any())
                {
                    interceptedTypeData = new InterceptedTypeData(interceptors);
                    _cachedTypes.TryAdd(new Tuple<Type, int>(type, configs.GetHashCode()), interceptedTypeData);
                    return true;
                }
            }
            interceptedTypeData = default;
            return false;
        }

        private static IEnumerable<CustomAttributeData> FilterTypeDefinitionInterceptorsFromProperty(this PropertyInfo propertyInfo, MethodInfo methodInfo, DynamicProxyFactoryConfigurations configs, IEnumerable<CustomAttributeData> typeDefinitionInterceptors)
        {
            var propertyMethodType = propertyInfo.GetPropertyMethodType(methodInfo);
            if (configs.IncludeTypeDefinitionAspectsToProperties.HasFlag(propertyMethodType))
            {
                typeDefinitionInterceptors = propertyInfo.ExcludeTypeDefinitionAspects<ExcludeAspectsFromTypeDefinitionToThisPropertyAttribute>(
                    typeDefinitionInterceptors,
                    excludeAspectsFromThisProperty => excludeAspectsFromThisProperty.Methods.HasFlag(propertyMethodType),
                    methodInfo).ToList();
            }
            else
            {
                typeDefinitionInterceptors = propertyInfo.IncludeTypeDefinitionAspects<IncludeAspectsFromTypeDefinitionToThisPropertyAttribute>(
                    typeDefinitionInterceptors,
                    includeAspectsFromThisProperty => includeAspectsFromThisProperty.Methods.HasFlag(propertyMethodType),
                    methodInfo).ToList();
            }
            return typeDefinitionInterceptors;
        }

        private static IEnumerable<CustomAttributeData> FilterTypeDefinitionInterceptorsFromEvent(this EventInfo eventInfo, MethodInfo methodInfo, DynamicProxyFactoryConfigurations configs, IEnumerable<CustomAttributeData> typeDefinitionInterceptors)
        {
            var eventMethodType = eventInfo.GetEventMethodType(methodInfo);
            if (configs.IncludeTypeDefinitionAspectsToEvents.HasFlag(eventMethodType))
            {
                typeDefinitionInterceptors = eventInfo.ExcludeTypeDefinitionAspects<ExcludeAspectsFromTypeDefinitionToThisEventAttribute>(
                    typeDefinitionInterceptors,
                    excludeAspectsFromThisEvent => excludeAspectsFromThisEvent.Methods.HasFlag(eventMethodType),
                    methodInfo).ToList();
            }
            else
            {
                typeDefinitionInterceptors = eventInfo.IncludeTypeDefinitionAspects<IncludeAspectsFromTypeDefinitionToThisEventAttribute>(
                    typeDefinitionInterceptors,
                    includeAspectsFromThisEvent => includeAspectsFromThisEvent.Methods.HasFlag(eventMethodType),
                    methodInfo).ToList();
            }
            return typeDefinitionInterceptors;
        }

        private static InterceptedPropertyMethod GetPropertyMethodType(this PropertyInfo propertyInfo, MethodInfo methodInfo)
        {
            if (propertyInfo.GetGetMethod() == methodInfo)
                return InterceptedPropertyMethod.Get;
            return InterceptedPropertyMethod.Set;
        }

        private static InterceptedEventMethod GetEventMethodType(this EventInfo eventInfo, MethodInfo methodInfo)
        {
            if (eventInfo.GetAddMethod() == methodInfo)
                return InterceptedEventMethod.Add;
            if (eventInfo.GetRemoveMethod() == methodInfo)
                return InterceptedEventMethod.Remove;
            return InterceptedEventMethod.Raise;
        }

        internal sealed class InterceptedTypeData
        {
            public IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>> Interceptors { get; }

            public bool IsIntercepted { get; }

            public InterceptedTypeData(IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>> interceptors)
            {
                Interceptors = interceptors;
                IsIntercepted = !(interceptors is null) && interceptors.Any();
            }

            public bool TryGetMethodInterceptors(MethodInfo methodInfo, out IEnumerable<CustomAttributeData> interceptors)
            {
                var interceptorList = new List<CustomAttributeData>();
                if (IsIntercepted &&
                    Interceptors.TryGetValue(methodInfo, out var methodInterceptors))
                    interceptorList.AddRange(methodInterceptors);
                interceptors = interceptorList;
                return interceptorList.Count > 0;
            }
        }
    }
}
