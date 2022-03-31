using AspectSharp.Abstractions;
using AspectSharp.Abstractions.Attributes;
using AspectSharp.Abstractions.Enums;
using AspectSharp.DynamicProxy.Factories;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AspectSharp.DynamicProxy.Utils
{
    internal static class InterceptorTypeCache
    {
        private static readonly IEnumerable<Assembly> _assembliesToIgnore = new List<Assembly>
        {
            typeof(AspectContext).Assembly,
            typeof(ConcreteAspectContext).Assembly,
            DynamicProxyFactory._proxiedClassesAssemblyBuilder
        };

        private static readonly object _lock = new object();

        private static readonly Type _abstractInterceptorType = typeof(AbstractInterceptorAttribute);

        private static readonly Type _uninterceptableType = typeof(UninterceptableAttribute);

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
            if (!type.IsInterface || _assembliesToIgnore.Contains(type.Assembly))
            {
                interceptedTypeData = default;
                return false;
            }
            lock (_lock)
            {
                configs = configs ?? new DynamicProxyFactoryConfigurations(
                    Configurations.IncludeAspectsToEvents,
                    Configurations.IncludeAspectsToProperties,
                    Configurations.ExcludeAspectsForMethods,
                    Configurations.GlobalInterceptors.ToList());
                if (_cachedTypes.TryGetValue(new Tuple<Type, int>(type, configs.GetHashCode()), out interceptedTypeData))
                    return true;

                _ = _previouslyUsedConfigurations.TryAdd(configs.GetHashCode(), configs);

                if (type.CustomAttributes.Any(attr => attr.AttributeType == _uninterceptableType))
                    return false;

                var typeDefinitionAttributes = type
                    .CustomAttributes
                        .Where(attr => _abstractInterceptorType.IsAssignableFrom(attr.AttributeType))
                        .ToList();

                var methodInterceptorDictionary = type
                    .GetMethods()
                        .Where(mi => configs.GlobalInterceptors.Any() ||
                                     typeDefinitionAttributes.Any() || 
                                     mi.CustomAttributes.Any(attr => _abstractInterceptorType.IsAssignableFrom(attr.AttributeType)))
                        .ToDictionary(mi => mi, mi => typeDefinitionAttributes.Concat(mi.CustomAttributes.Where(attr => _abstractInterceptorType.IsAssignableFrom(attr.AttributeType)).ToList()));

                if (methodInterceptorDictionary.Any())
                {
                    var properties = type.GetProperties();
                    var events = type.GetEvents();

                    var globalInterceptors = new Dictionary<MethodInfo, IEnumerable<IInterceptor>>();
                    var interceptors = new Dictionary<MethodInfo, IEnumerable<CustomAttributeData>>();
                    foreach (var tuple in methodInterceptorDictionary)
                    {
                        var methodInfo = tuple.Key;
                        var allMethodInterceptors = tuple.Value;

                        if (methodInfo.CustomAttributes.Any(attr => attr.AttributeType == _uninterceptableType))
                            continue;

                        var typeDefinitionInterceptors = allMethodInterceptors.Take(typeDefinitionAttributes.Count).ToList();
                        var methodInterceptors = allMethodInterceptors.Skip(typeDefinitionAttributes.Count).ToList();

                        var propertyFromMethod = properties.FirstOrDefault(prop => prop.GetGetMethod() == methodInfo || prop.GetSetMethod() == methodInfo);
                        var eventFromMethod = events.FirstOrDefault(evt => evt.GetAddMethod() == methodInfo || evt.GetRemoveMethod() == methodInfo);


                        var globalInterceptorList = new List<IInterceptor>();
                        var ignoreProperty = !(propertyFromMethod is null || configs.IncludeAspectsToProperties.HasFlag(propertyFromMethod.GetPropertyMethodType(methodInfo)));
                        var ignoreEvent = !(eventFromMethod is null || configs.IncludeAspectsToEvents.HasFlag(eventFromMethod.GetEventMethodType(methodInfo)));
                        if (!ignoreProperty && !ignoreEvent)
                        {
                            foreach (var globalInterceptorConfig in configs.GlobalInterceptors)
                            {
                                if (globalInterceptorConfig.TryGetInterceptor(methodInfo, out var globalInterceptor))
                                    globalInterceptorList.Add(globalInterceptor);
                            }
                        }


                        if (!(propertyFromMethod is null))
                        {
                            methodInterceptors = propertyFromMethod.CustomAttributes.Where(attr => _abstractInterceptorType.IsAssignableFrom(attr.AttributeType)).Concat(methodInterceptors).ToList();
                            typeDefinitionInterceptors = propertyFromMethod.FilterTypeDefinitionInterceptorsFromProperty(methodInfo, configs, typeDefinitionInterceptors).ToList();
                        }
                        else if (!(eventFromMethod is null))
                        {
                            methodInterceptors = eventFromMethod.CustomAttributes.Where(attr => _abstractInterceptorType.IsAssignableFrom(attr.AttributeType)).Concat(methodInterceptors).ToList();
                            typeDefinitionInterceptors = eventFromMethod.FilterTypeDefinitionInterceptorsFromEvent(methodInfo, configs, typeDefinitionInterceptors).ToList();
                        }
                        else
                        {
                            if (configs.ExcludeAspectsForMethods)
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
                        if (globalInterceptorList.Count > 0)
                            globalInterceptors.Add(methodInfo, globalInterceptorList);
                        if (acceptedInterceptors.Any())
                            interceptors.Add(methodInfo, acceptedInterceptors);
                    }
                    if (interceptors.Any() || globalInterceptors.Any())
                    {
                        interceptedTypeData = new InterceptedTypeData(globalInterceptors, interceptors);
                        _cachedTypes.TryAdd(new Tuple<Type, int>(type, configs.GetHashCode()), interceptedTypeData);
                        return true;
                    }
                }
                interceptedTypeData = default;
                return false;
            }
        }

        private static IEnumerable<CustomAttributeData> FilterTypeDefinitionInterceptorsFromProperty(this PropertyInfo propertyInfo, MethodInfo methodInfo, DynamicProxyFactoryConfigurations configs, IEnumerable<CustomAttributeData> typeDefinitionInterceptors)
        {
            var propertyMethodType = propertyInfo.GetPropertyMethodType(methodInfo);
            if (configs.IncludeAspectsToProperties.HasFlag(propertyMethodType))
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
            if (configs.IncludeAspectsToEvents.HasFlag(eventMethodType))
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
            return InterceptedEventMethod.Remove;
        }

        internal sealed class InterceptedTypeData
        {
            public IReadOnlyDictionary<MethodInfo, IEnumerable<IInterceptor>> GlobalInterceptors { get; }
            public IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>> InterceptorAttributes { get; }

            public bool IsIntercepted { get; }

            public InterceptedTypeData(IReadOnlyDictionary<MethodInfo, IEnumerable<IInterceptor>> globalInterceptors, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>> interceptorAttributes)
            {
                if (globalInterceptors is null)
                    globalInterceptors = new Dictionary<MethodInfo, IEnumerable<IInterceptor>>();
                InterceptorAttributes = interceptorAttributes;
                GlobalInterceptors = globalInterceptors;
                if (!(interceptorAttributes is null))
                {
                    InterceptorAttributes = InterceptorAttributes
                        .OrderBy(kvp => string.Format("{0}{1}", kvp.Key.Name, kvp.Key.GetParameters().Length))
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                    IsIntercepted = GlobalInterceptors.Any() || interceptorAttributes.Any();
                }
            }

            public bool TryGetMethodInterceptorAttributes(MethodInfo methodInfo, out IEnumerable<CustomAttributeData> interceptors)
            {
                var interceptorList = new List<CustomAttributeData>();
                if (IsIntercepted &&
                    InterceptorAttributes.TryGetValue(methodInfo, out var methodInterceptors))
                    interceptorList.AddRange(methodInterceptors);
                interceptors = interceptorList;
                return interceptorList.Count > 0;
            }

            public bool TryGetMethodGlobalInterceptors(MethodInfo methodInfo, out IEnumerable<IInterceptor> interceptors)
            {
                var interceptorList = new List<IInterceptor>();
                if (IsIntercepted &&
                    GlobalInterceptors.TryGetValue(methodInfo, out var globalInterceptors))
                    interceptorList.AddRange(globalInterceptors);
                interceptors = interceptorList;
                return interceptorList.Count > 0;
            }
        }
    }
}
