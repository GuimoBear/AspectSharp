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

        private static readonly ConcurrentDictionary<(Type, int), InterceptedTypeData> _cachedTypes = new ConcurrentDictionary<(Type, int), InterceptedTypeData>();

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
            if (_cachedTypes.TryGetValue((type, configs.GetHashCode()), out interceptedTypeData))
                return true;

            _ = _previouslyUsedConfigurations.TryAdd(configs.GetHashCode(), configs);

            var typeDefinitionAttributes = type
                .CustomAttributes
                    .Where(attr => attr.AttributeType.IsAssignableTo(_abstractInterceptorType))
                    .ToList();

            var methodInterceptorDictionary = type
                .GetMethods()
                    .Where(mi => typeDefinitionAttributes.Any() || mi.CustomAttributes.Any(attr => attr.AttributeType.IsAssignableTo(_abstractInterceptorType)))
                    .ToDictionary(mi => mi, mi => typeDefinitionAttributes.Concat(mi.CustomAttributes.Where(attr => attr.AttributeType.IsAssignableTo(_abstractInterceptorType)).ToList()));

            if (methodInterceptorDictionary.Any())
            {
                var properties = type.GetProperties();
                var events = type.GetEvents();

                var interceptors = new Dictionary<MethodInfo, IEnumerable<CustomAttributeData>>();
                foreach(var (methodInfo, allMethodInterceptors) in methodInterceptorDictionary)
                {
                    var typeDefinitionInterceptors = allMethodInterceptors.Take(typeDefinitionAttributes.Count).ToList();
                    var methodInterceptors = allMethodInterceptors.Skip(typeDefinitionAttributes.Count).ToList();

                    var propertyFromMethod = properties.FirstOrDefault(prop => prop.GetGetMethod() == methodInfo || prop.GetSetMethod() == methodInfo);
                    var eventFromMethod = events.FirstOrDefault(evt => evt.GetAddMethod() == methodInfo || evt.GetRemoveMethod() == methodInfo || evt.GetRaiseMethod() == methodInfo);

                    if (propertyFromMethod is not null)
                    {
                        var propertyMethodType = propertyFromMethod.GetPropertyMethodType(methodInfo);
                        if (configs.IncludeTypeDefinitionAspectsToProperties.HasFlag(propertyMethodType))
                        {
                            var excludingAttributes = propertyFromMethod
                                .GetCustomAttributes(true)
                                .Where(attr => attr.GetType() == typeof(ExcludeAspectsFromTypeDefinitionAttribute) ||
                                               attr.GetType() == typeof(ExcludeAspectsFromTypeDefinitionToThisPropertyAttribute))
                                .Concat(methodInfo.GetCustomAttributes(true)
                                    .Where(attr => attr.GetType() == typeof(ExcludeAspectsFromTypeDefinitionAttribute)));
                            foreach (var attribute in excludingAttributes)
                            {
                                if (attribute is ExcludeAspectsFromTypeDefinitionAttribute excludeAspects)
                                {
                                    if (excludeAspects.ExcludeAll)
                                    {
                                        typeDefinitionInterceptors = Enumerable.Empty<CustomAttributeData>().ToList();
                                        break;
                                    }
                                    else
                                        typeDefinitionInterceptors = typeDefinitionInterceptors.Where(attr => !excludeAspects.Aspects.Any(attributeType => attributeType == attr.AttributeType)).ToList();
                                }
                                else if (attribute is ExcludeAspectsFromTypeDefinitionToThisPropertyAttribute excludeAspectsFromThisProperty)
                                {
                                    if (excludeAspectsFromThisProperty.ExcludeAll)
                                    {
                                        typeDefinitionInterceptors = Enumerable.Empty<CustomAttributeData>().ToList();
                                        break;
                                    }
                                    else if (excludeAspectsFromThisProperty.Methods.HasFlag(propertyMethodType))
                                        typeDefinitionInterceptors = typeDefinitionInterceptors.Where(attr => !excludeAspectsFromThisProperty.Aspects.Any(attributeType => attributeType == attr.AttributeType)).ToList();
                                }
                            }
                        }
                        else
                        {
                            typeDefinitionInterceptors = Enumerable.Empty<CustomAttributeData>().ToList();
                            var includingAttributes = propertyFromMethod
                                .GetCustomAttributes(true)
                                .Where(attr => attr.GetType() == typeof(IncludeAspectsFromTypeDefinitionAttribute) ||
                                               attr.GetType() == typeof(IncludeAspectsFromTypeDefinitionToThisPropertyAttribute))
                                .Concat(methodInfo.GetCustomAttributes(true)
                                    .Where(attr => attr.GetType() == typeof(IncludeAspectsFromTypeDefinitionAttribute)));
                            foreach (var attribute in includingAttributes)
                            {
                                if (attribute is IncludeAspectsFromTypeDefinitionAttribute includeAspects)
                                {
                                    if (includeAspects.IncludeAll)
                                    {
                                        typeDefinitionInterceptors = allMethodInterceptors.Take(typeDefinitionAttributes.Count).ToList();
                                        break;
                                    }
                                    else
                                        typeDefinitionInterceptors = allMethodInterceptors.Take(typeDefinitionAttributes.Count).Where(attr => includeAspects.Aspects.Any(attributeType => attributeType == attr.AttributeType)).ToList();
                                }
                                else if (attribute is IncludeAspectsFromTypeDefinitionToThisPropertyAttribute includeAspectsFromThisProperty)
                                {
                                    if (includeAspectsFromThisProperty.IncludeAll)
                                    {
                                        typeDefinitionInterceptors = Enumerable.Empty<CustomAttributeData>().ToList();
                                        break;
                                    }
                                    else if (includeAspectsFromThisProperty.Methods.HasFlag(propertyMethodType))
                                        typeDefinitionInterceptors = allMethodInterceptors.Take(typeDefinitionAttributes.Count).Where(attr => includeAspectsFromThisProperty.Aspects.Any(attributeType => attributeType == attr.AttributeType)).ToList();
                                }
                            }
                        }
                    }
                    else if (eventFromMethod is not null)
                    {
                        var eventMethodType = eventFromMethod.GetEventMethodType(methodInfo);
                        if (configs.IncludeTypeDefinitionAspectsToEvents.HasFlag(eventMethodType))
                        {
                            var excludingAttributes = eventFromMethod
                                .GetCustomAttributes(true)
                                .Where(attr => attr.GetType() == typeof(ExcludeAspectsFromTypeDefinitionAttribute) ||
                                               attr.GetType() == typeof(ExcludeAspectsFromTypeDefinitionToThisEventAttribute))
                                .Concat(methodInfo.GetCustomAttributes(true)
                                    .Where(attr => attr.GetType() == typeof(ExcludeAspectsFromTypeDefinitionAttribute)));
                            foreach (var attribute in excludingAttributes)
                            {
                                if (attribute is ExcludeAspectsFromTypeDefinitionAttribute excludeAspects)
                                {
                                    if (excludeAspects.ExcludeAll)
                                    {
                                        typeDefinitionInterceptors = Enumerable.Empty<CustomAttributeData>().ToList();
                                        break;
                                    }
                                    else
                                        typeDefinitionInterceptors = typeDefinitionInterceptors.Where(attr => !excludeAspects.Aspects.Any(attributeType => attributeType == attr.AttributeType)).ToList();
                                }
                                else if (attribute is ExcludeAspectsFromTypeDefinitionToThisEventAttribute excludeAspectsFromThisEvent)
                                {
                                    if (excludeAspectsFromThisEvent.ExcludeAll)
                                    {
                                        typeDefinitionInterceptors = Enumerable.Empty<CustomAttributeData>().ToList();
                                        break;
                                    }
                                    else if (excludeAspectsFromThisEvent.Methods.HasFlag(eventMethodType))
                                        typeDefinitionInterceptors = typeDefinitionInterceptors.Where(attr => !excludeAspectsFromThisEvent.Aspects.Any(attributeType => attributeType == attr.AttributeType)).ToList();
                                }
                            }
                        }
                        else
                        {
                            typeDefinitionInterceptors = Enumerable.Empty<CustomAttributeData>().ToList();
                            var includingAttributes = eventFromMethod
                                .GetCustomAttributes(true)
                                .Where(attr => attr.GetType() == typeof(IncludeAspectsFromTypeDefinitionAttribute) ||
                                               attr.GetType() == typeof(IncludeAspectsFromTypeDefinitionToThisEventAttribute))
                                .Concat(methodInfo.GetCustomAttributes(true)
                                    .Where(attr => attr.GetType() == typeof(IncludeAspectsFromTypeDefinitionAttribute)));
                            foreach (var attribute in includingAttributes)
                            {
                                if (attribute is IncludeAspectsFromTypeDefinitionAttribute includeAspects)
                                {
                                    if (includeAspects.IncludeAll)
                                    {
                                        typeDefinitionInterceptors = allMethodInterceptors.Take(typeDefinitionAttributes.Count).ToList();
                                        break;
                                    }
                                    else
                                        typeDefinitionInterceptors = allMethodInterceptors.Take(typeDefinitionAttributes.Count).Where(attr => includeAspects.Aspects.Any(attributeType => attributeType == attr.AttributeType)).ToList();
                                }
                                else if (attribute is IncludeAspectsFromTypeDefinitionToThisEventAttribute includeAspectsFromThisEvent)
                                {
                                    if (includeAspectsFromThisEvent.IncludeAll)
                                    {
                                        typeDefinitionInterceptors = Enumerable.Empty<CustomAttributeData>().ToList();
                                        break;
                                    }
                                    else if (includeAspectsFromThisEvent.Methods.HasFlag(eventMethodType))
                                        typeDefinitionInterceptors = allMethodInterceptors.Take(typeDefinitionAttributes.Count).Where(attr => includeAspectsFromThisEvent.Aspects.Any(attributeType => attributeType == attr.AttributeType)).ToList();
                                }
                            }
                        }
                    }
                    else
                    {
                        if (configs.ExcludeTypeDefinitionAspectsForMethods)
                        {
                            typeDefinitionInterceptors = Enumerable.Empty<CustomAttributeData>().ToList();
                            var attribute = methodInfo.GetCustomAttributes(true).FirstOrDefault(attr => attr.GetType() == typeof(IncludeAspectsFromTypeDefinitionAttribute));
                            if (attribute is IncludeAspectsFromTypeDefinitionAttribute includeAspects)
                            {
                                if (includeAspects.IncludeAll)
                                {
                                    typeDefinitionInterceptors = allMethodInterceptors.Take(typeDefinitionAttributes.Count).ToList();
                                    break;
                                }
                                else
                                    typeDefinitionInterceptors = allMethodInterceptors.Take(typeDefinitionAttributes.Count).Where(attr => includeAspects.Aspects.Any(attributeType => attributeType == attr.AttributeType)).ToList();
                            }
                        }
                        else
                        {
                            var attribute = methodInfo.GetCustomAttributes(true).FirstOrDefault(attr => attr.GetType() == typeof(ExcludeAspectsFromTypeDefinitionAttribute));
                            if (attribute is ExcludeAspectsFromTypeDefinitionAttribute excludeAspects)
                            { 
                                if (excludeAspects.ExcludeAll)
                                {
                                    typeDefinitionInterceptors = Enumerable.Empty<CustomAttributeData>().ToList();
                                    break;
                                }
                                else
                                    typeDefinitionInterceptors = typeDefinitionInterceptors.Where(attr => !excludeAspects.Aspects.Any(attributeType => attributeType == attr.AttributeType)).ToList();
                            }
                        }
                    }
                    var acceptedInterceptors = typeDefinitionInterceptors.Concat(methodInterceptors).ToList();
                    if (acceptedInterceptors.Any())
                        interceptors.Add(methodInfo, acceptedInterceptors);
                }
                if (interceptors.Any())
                {
                    interceptedTypeData = new InterceptedTypeData
                    {
                        Interceptors = interceptors,
                        IsIntercepted = interceptors.Any()
                    };
                    _cachedTypes.TryAdd((type, configs.GetHashCode()), interceptedTypeData);
                    return true;
                }
            }
            interceptedTypeData = default;
            return false;
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
            public IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>> Interceptors { get; init; }

            public bool IsIntercepted { get; init; }

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
