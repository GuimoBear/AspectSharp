using AspectSharp.Abstractions;
using AspectSharp.Abstractions.Attributes;
using AspectSharp.Abstractions.Enums;
using AspectSharp.Tests.Core.Aspects;
using AspectSharp.Tests.Core.Services;
using AspectSharp.Tests.Core.Services.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AspectSharp.Tests.Core.TestData.DynamicProxy.Utils
{
    internal static class InterceptorTypeCacheData
    {
        public static IEnumerable<Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>>> GetInterceptorTypeDataTheoryData()
        {
            yield return GetInterceptorTypeDataWithInterfaceContainingAllScenarios();
            yield return GetInterceptorTypeDataUsingDefaultConfigs();
            yield return GetInterceptorTypeDataWithInterceptOnAddEvent();
            yield return GetInterceptorTypeDataWithInterceptOnRemoveEvent();
            yield return GetInterceptorTypeDataWithInterceptOnAnyEventMethod();
            yield return GetInterceptorTypeDataWithInterceptOnGetProperty();
            yield return GetInterceptorTypeDataWithInterceptOnSetProperty();
            yield return GetInterceptorTypeDataWithInterceptOnAnyPropertyMethod();
            yield return GetInterceptorTypeDataExcludingTypeDefinitionAspectsForMethods();
            yield return GetInterceptorTypeDataWithUninspectedInterface();
            yield return GetInterceptorTypeDataUsingDefaultConfigsAndGenericReferenceType();
            yield return GetInterceptorTypeDataUsingDefaultConfigsAndGenericValueType();
            yield return GetInterceptorTypeDataUsingDefaultConfigsAndInheritedGenericReferenceType();
            yield return GetInterceptorTypeDataUsingDefaultConfigsAndInheritedGenericValueType();
            yield return GetInterceptorTypeDataUsingDefaultConfigsAndServiceWithGenericMethod();
        }

        private static Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>> GetInterceptorTypeDataUsingDefaultConfigs()
        {
            var serviceType = typeof(ISimpleFakeService);
            var targetType = typeof(SimpleFakeService);
            var typeDefinitionInterceptors = serviceType.CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)).ToList();
            var aspect1InterceptorList = typeDefinitionInterceptors.Where(attr => typeof(Aspect1Attribute).IsAssignableFrom(attr.AttributeType)).ToList();
            var dict = new Dictionary<MethodInfo, IEnumerable<CustomAttributeData>>
            {
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent)).GetAddMethod(),
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent))
                        .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                        .Concat(serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent))
                            .GetAddMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent)).GetRemoveMethod(),
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent))
                        .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                        .Concat(serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent))
                            .GetRemoveMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnEventWithAllAspect)).GetAddMethod(),
                    typeDefinitionInterceptors
                },
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnEventWithAllAspect)).GetRemoveMethod(),
                    typeDefinitionInterceptors
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetGetMethod(),
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty))
                        .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                        .Concat(serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty))
                            .GetGetMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetSetMethod(),
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty))
                        .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                        .Concat(serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty))
                            .GetSetMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(ISimpleFakeService.Method)),
                    aspect1InterceptorList
                },
                {
                    serviceType.GetMethod(nameof(ISimpleFakeService.InterceptedMethod)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(ISimpleFakeService.InterceptedMethod)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))).ToList()
                },
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnEvent)).GetAddMethod(),
                    aspect1InterceptorList
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.Property)).GetGetMethod(),
                    aspect1InterceptorList
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.PropertyWithAllAspect)).GetGetMethod(),
                    typeDefinitionInterceptors
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.PropertyWithAllAspect)).GetSetMethod(),
                    typeDefinitionInterceptors
                }
            };
            return new Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>>
            (
                serviceType,
                targetType,
                new DynamicProxyFactoryConfigurations(default, default, default, default),
                true,
                dict.OrderBy(kvp => string.Format("{0}{1}", kvp.Key.Name, kvp.Key.GetParameters().Length)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            );
        }

        private static Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>> GetInterceptorTypeDataWithInterceptOnAddEvent()
        {
            var serviceType = typeof(ISimpleFakeService);
            var targetType = typeof(SimpleFakeService);
            var typeDefinitionInterceptors = serviceType.CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)).ToList();
            var aspect1InterceptorList = typeDefinitionInterceptors.Where(attr => typeof(Aspect1Attribute).IsAssignableFrom(attr.AttributeType)).ToList();
            var dict = new Dictionary<MethodInfo, IEnumerable<CustomAttributeData>>
            {
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent)).GetAddMethod(),
                    typeDefinitionInterceptors.Concat(
                        serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent))
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                            .Concat(serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent))
                                .GetAddMethod()
                                .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))))
                        .ToList()
                },
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent)).GetRemoveMethod(),
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent))
                        .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                        .Concat(serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent))
                            .GetRemoveMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnEventWithAllAspect)).GetAddMethod(),
                    typeDefinitionInterceptors
                },
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnEventWithAllAspect)).GetRemoveMethod(),
                    typeDefinitionInterceptors
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetGetMethod(),
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty))
                        .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                        .Concat(serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty))
                            .GetGetMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetSetMethod(),
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty))
                        .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                        .Concat(serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty))
                            .GetSetMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(ISimpleFakeService.Method)),
                    aspect1InterceptorList
                },
                {
                    serviceType.GetMethod(nameof(ISimpleFakeService.InterceptedMethod)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(ISimpleFakeService.InterceptedMethod)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))).ToList()
                },
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnEvent)).GetAddMethod(),
                    aspect1InterceptorList
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.Property)).GetGetMethod(),
                    aspect1InterceptorList
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.PropertyWithAllAspect)).GetGetMethod(),
                    typeDefinitionInterceptors
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.PropertyWithAllAspect)).GetSetMethod(),
                    typeDefinitionInterceptors
                }
            };
            return new Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>>
            (
                serviceType,
                targetType,
                new DynamicProxyFactoryConfigurations(InterceptedEventMethod.Add, default, default, default),
                true,
                dict.OrderBy(kvp => string.Format("{0}{1}", kvp.Key.Name, kvp.Key.GetParameters().Length)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            );
        }

        private static Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>> GetInterceptorTypeDataWithInterceptOnRemoveEvent()
        {
            var serviceType = typeof(ISimpleFakeService);
            var targetType = typeof(SimpleFakeService);
            var typeDefinitionInterceptors = serviceType.CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)).ToList();
            var aspect1InterceptorList = typeDefinitionInterceptors.Where(attr => typeof(Aspect1Attribute).IsAssignableFrom(attr.AttributeType)).ToList();
            var dict = new Dictionary<MethodInfo, IEnumerable<CustomAttributeData>>
            {
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent)).GetAddMethod(),
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent))
                        .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                        .Concat(serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent))
                            .GetAddMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent)).GetRemoveMethod(),
                    typeDefinitionInterceptors.Concat(
                        serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent))
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                            .Concat(serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent))
                                .GetRemoveMethod()
                                .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))))
                        .ToList()
                },
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnEventWithAllAspect)).GetAddMethod(),
                    typeDefinitionInterceptors
                },
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnEventWithAllAspect)).GetRemoveMethod(),
                    typeDefinitionInterceptors
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetGetMethod(),
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty))
                        .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                        .Concat(serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty))
                            .GetGetMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetSetMethod(),
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty))
                        .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                        .Concat(serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty))
                            .GetSetMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(ISimpleFakeService.Method)),
                    aspect1InterceptorList
                },
                {
                    serviceType.GetMethod(nameof(ISimpleFakeService.InterceptedMethod)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(ISimpleFakeService.InterceptedMethod)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))).ToList()
                },
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnEvent)).GetAddMethod(),
                    aspect1InterceptorList
                },
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnEvent)).GetRemoveMethod(),
                    typeDefinitionInterceptors.ToList()
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.Property)).GetGetMethod(),
                    aspect1InterceptorList
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.PropertyWithAllAspect)).GetGetMethod(),
                    typeDefinitionInterceptors
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.PropertyWithAllAspect)).GetSetMethod(),
                    typeDefinitionInterceptors
                }
            };
            return new Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>>
            (
                serviceType,
                targetType,
                new DynamicProxyFactoryConfigurations(InterceptedEventMethod.Remove, default, default, default),
                true,
                dict.OrderBy(kvp => string.Format("{0}{1}", kvp.Key.Name, kvp.Key.GetParameters().Length)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            );
        }

        private static Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>> GetInterceptorTypeDataWithInterceptOnAnyEventMethod()
        {
            var serviceType = typeof(ISimpleFakeService);
            var targetType = typeof(SimpleFakeService);
            var typeDefinitionInterceptors = serviceType.CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)).ToList();
            var aspect1InterceptorList = typeDefinitionInterceptors.Where(attr => typeof(Aspect1Attribute).IsAssignableFrom(attr.AttributeType)).ToList();
            var dict = new Dictionary<MethodInfo, IEnumerable<CustomAttributeData>>
            {
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent)).GetAddMethod(),
                    typeDefinitionInterceptors.Concat(
                        serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent))
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                            .Concat(serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent))
                                .GetAddMethod()
                                .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))))
                        .ToList()
                },
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent)).GetRemoveMethod(),
                    typeDefinitionInterceptors.Concat(
                        serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent))
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                            .Concat(serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent))
                                .GetRemoveMethod()
                                .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))))
                        .ToList()
                },
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnEventWithAllAspect)).GetAddMethod(),
                    typeDefinitionInterceptors
                },
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnEventWithAllAspect)).GetRemoveMethod(),
                    typeDefinitionInterceptors
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetGetMethod(),
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty))
                        .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                        .Concat(serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty))
                            .GetGetMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetSetMethod(),
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty))
                        .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                        .Concat(serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty))
                            .GetSetMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(ISimpleFakeService.Method)),
                    aspect1InterceptorList
                },
                {
                    serviceType.GetMethod(nameof(ISimpleFakeService.InterceptedMethod)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(ISimpleFakeService.InterceptedMethod)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))).ToList()
                },
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnEvent)).GetAddMethod(),
                    aspect1InterceptorList.ToList()
                },
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnEvent)).GetRemoveMethod(),
                    typeDefinitionInterceptors.ToList()
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.Property)).GetGetMethod(),
                    aspect1InterceptorList
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.PropertyWithAllAspect)).GetGetMethod(),
                    typeDefinitionInterceptors
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.PropertyWithAllAspect)).GetSetMethod(),
                    typeDefinitionInterceptors
                }
            };
            return new Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>>
            (
                serviceType,
                targetType,
                new DynamicProxyFactoryConfigurations(InterceptedEventMethod.All, default, default, default),
                true,
                dict.OrderBy(kvp => string.Format("{0}{1}", kvp.Key.Name, kvp.Key.GetParameters().Length)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            );
        }

        private static Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>> GetInterceptorTypeDataWithInterceptOnGetProperty()
        {
            var serviceType = typeof(ISimpleFakeService);
            var targetType = typeof(SimpleFakeService);
            var typeDefinitionInterceptors = serviceType.CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)).ToList();
            var aspect1InterceptorList = typeDefinitionInterceptors.Where(attr => typeof(Aspect1Attribute).IsAssignableFrom(attr.AttributeType)).ToList();
            var dict = new Dictionary<MethodInfo, IEnumerable<CustomAttributeData>>
            {
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent)).GetAddMethod(),
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent))
                        .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                        .Concat(serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent))
                            .GetAddMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent)).GetRemoveMethod(),
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent))
                        .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                        .Concat(serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent))
                            .GetRemoveMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnEventWithAllAspect)).GetAddMethod(),
                    typeDefinitionInterceptors
                },
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnEventWithAllAspect)).GetRemoveMethod(),
                    typeDefinitionInterceptors
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetGetMethod(),
                    typeDefinitionInterceptors.Concat(
                        serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty))
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                            .Concat(serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty))
                                .GetGetMethod()
                                .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))))
                        .ToList()
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetSetMethod(),
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty))
                        .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                        .Concat(serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty))
                            .GetSetMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(ISimpleFakeService.Method)),
                    aspect1InterceptorList
                },
                {
                    serviceType.GetMethod(nameof(ISimpleFakeService.InterceptedMethod)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(ISimpleFakeService.InterceptedMethod)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))).ToList()
                },
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnEvent)).GetAddMethod(),
                    aspect1InterceptorList
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.Property)).GetGetMethod(),
                    aspect1InterceptorList.ToList()
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.PropertyWithAllAspect)).GetGetMethod(),
                    typeDefinitionInterceptors
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.PropertyWithAllAspect)).GetSetMethod(),
                    typeDefinitionInterceptors
                }
            };
            return new Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>>
            (
                serviceType,
                targetType,
                new DynamicProxyFactoryConfigurations(default, InterceptedPropertyMethod.Get, default, default),
                true,
                dict.OrderBy(kvp => string.Format("{0}{1}", kvp.Key.Name, kvp.Key.GetParameters().Length)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            );
        }

        private static Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>> GetInterceptorTypeDataWithInterceptOnSetProperty()
        {
            var serviceType = typeof(ISimpleFakeService);
            var targetType = typeof(SimpleFakeService);
            var typeDefinitionInterceptors = serviceType.CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)).ToList();
            var aspect1InterceptorList = typeDefinitionInterceptors.Where(attr => typeof(Aspect1Attribute).IsAssignableFrom(attr.AttributeType)).ToList();
            var dict = new Dictionary<MethodInfo, IEnumerable<CustomAttributeData>>
            {
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent)).GetAddMethod(),
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent))
                        .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                        .Concat(serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent))
                            .GetAddMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent)).GetRemoveMethod(),
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent))
                        .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                        .Concat(serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent))
                            .GetRemoveMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnEventWithAllAspect)).GetAddMethod(),
                    typeDefinitionInterceptors
                },
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnEventWithAllAspect)).GetRemoveMethod(),
                    typeDefinitionInterceptors
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetGetMethod(),
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty))
                        .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                        .Concat(serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty))
                            .GetGetMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetSetMethod(),
                    typeDefinitionInterceptors.Concat(
                        serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty))
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                            .Concat(serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty))
                                .GetSetMethod()
                                .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(ISimpleFakeService.Method)),
                    aspect1InterceptorList
                },
                {
                    serviceType.GetMethod(nameof(ISimpleFakeService.InterceptedMethod)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(ISimpleFakeService.InterceptedMethod)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))).ToList()
                },
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnEvent)).GetAddMethod(),
                    aspect1InterceptorList
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.Property)).GetGetMethod(),
                    aspect1InterceptorList
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.Property)).GetSetMethod(),
                    typeDefinitionInterceptors.ToList()
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.PropertyWithAllAspect)).GetGetMethod(),
                    typeDefinitionInterceptors
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.PropertyWithAllAspect)).GetSetMethod(),
                    typeDefinitionInterceptors
                }
            };
            return new Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>>
            (
                serviceType,
                targetType,
                new DynamicProxyFactoryConfigurations(default, InterceptedPropertyMethod.Set, default, default),
                true,
                dict.OrderBy(kvp => string.Format("{0}{1}", kvp.Key.Name, kvp.Key.GetParameters().Length)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            );
        }

        private static Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>> GetInterceptorTypeDataWithInterceptOnAnyPropertyMethod()
        {
            var serviceType = typeof(ISimpleFakeService);
            var targetType = typeof(SimpleFakeService);
            var typeDefinitionInterceptors = serviceType.CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)).ToList();
            var aspect1InterceptorList = typeDefinitionInterceptors.Where(attr => typeof(Aspect1Attribute).IsAssignableFrom(attr.AttributeType)).ToList();
            var dict = new Dictionary<MethodInfo, IEnumerable<CustomAttributeData>>
            {
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent)).GetAddMethod(),
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent))
                        .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                        .Concat(serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent))
                            .GetAddMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent)).GetRemoveMethod(),
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent))
                        .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                        .Concat(serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent))
                            .GetRemoveMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnEventWithAllAspect)).GetAddMethod(),
                    typeDefinitionInterceptors
                },
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnEventWithAllAspect)).GetRemoveMethod(),
                    typeDefinitionInterceptors
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetGetMethod(),
                    typeDefinitionInterceptors.Concat(
                        serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty))
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                            .Concat(serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty))
                                .GetGetMethod()
                                .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))))
                        .ToList()
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetSetMethod(),
                    typeDefinitionInterceptors.Concat(
                        serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty))
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                            .Concat(serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty))
                                .GetSetMethod()
                                .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(ISimpleFakeService.Method)),
                    aspect1InterceptorList
                },
                {
                    serviceType.GetMethod(nameof(ISimpleFakeService.InterceptedMethod)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(ISimpleFakeService.InterceptedMethod)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))).ToList()
                },
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnEvent)).GetAddMethod(),
                    aspect1InterceptorList
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.Property)).GetGetMethod(),
                    aspect1InterceptorList.ToList()
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.Property)).GetSetMethod(),
                    typeDefinitionInterceptors.ToList()
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.PropertyWithAllAspect)).GetGetMethod(),
                    typeDefinitionInterceptors
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.PropertyWithAllAspect)).GetSetMethod(),
                    typeDefinitionInterceptors
                }
            };
            return new Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>>
            (
                serviceType,
                targetType,
                new DynamicProxyFactoryConfigurations(default, InterceptedPropertyMethod.All, default, default),
                true,
                dict.OrderBy(kvp => string.Format("{0}{1}", kvp.Key.Name, kvp.Key.GetParameters().Length)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            );
        }

        private static Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>> GetInterceptorTypeDataExcludingTypeDefinitionAspectsForMethods()
        {
            var serviceType = typeof(ISimpleFakeService);
            var targetType = typeof(SimpleFakeService);
            var typeDefinitionInterceptors = serviceType.CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)).ToList();
            var aspect1InterceptorList = typeDefinitionInterceptors.Where(attr => typeof(Aspect1Attribute).IsAssignableFrom(attr.AttributeType)).ToList();
            var dict = new Dictionary<MethodInfo, IEnumerable<CustomAttributeData>>
            {
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent)).GetAddMethod(),
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent))
                        .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                        .Concat(serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent))
                            .GetAddMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent)).GetRemoveMethod(),
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent))
                        .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                        .Concat(serviceType.GetEvent(nameof(ISimpleFakeService.OnInterceptedEvent))
                            .GetRemoveMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnEventWithAllAspect)).GetAddMethod(),
                    typeDefinitionInterceptors
                },
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnEventWithAllAspect)).GetRemoveMethod(),
                    typeDefinitionInterceptors
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetGetMethod(),
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty))
                        .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                        .Concat(serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty))
                            .GetGetMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetSetMethod(),
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty))
                        .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                        .Concat(serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty))
                            .GetSetMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(ISimpleFakeService.Method)),
                    aspect1InterceptorList
                },
                {
                    serviceType.GetMethod(nameof(ISimpleFakeService.InterceptedMethod)),
                    serviceType.GetMethod(nameof(ISimpleFakeService.InterceptedMethod)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)).ToList()
                },
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnEvent)).GetAddMethod(),
                    aspect1InterceptorList
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.Property)).GetGetMethod(),
                    aspect1InterceptorList
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.PropertyWithAllAspect)).GetGetMethod(),
                    typeDefinitionInterceptors
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.PropertyWithAllAspect)).GetSetMethod(),
                    typeDefinitionInterceptors
                },
                {
                    serviceType.GetMethod(nameof(ISimpleFakeService.MethodWithOutValueTypeParameter)),
                    serviceType.GetMethod(nameof(ISimpleFakeService.MethodWithOutValueTypeParameter)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)).ToList()
                },
                {
                    serviceType.GetMethod(nameof(ISimpleFakeService.MethodWithRefValueTypeParameter)),
                    serviceType.GetMethod(nameof(ISimpleFakeService.MethodWithRefValueTypeParameter)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)).ToList()
                },
                {
                    serviceType.GetMethod(nameof(ISimpleFakeService.MethodWithOutReferenceTypeParameter)),
                    serviceType.GetMethod(nameof(ISimpleFakeService.MethodWithOutReferenceTypeParameter)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)).ToList()
                },
                {
                    serviceType.GetMethod(nameof(ISimpleFakeService.MethodWithRefReferenceTypeParameter)),
                    serviceType.GetMethod(nameof(ISimpleFakeService.MethodWithRefReferenceTypeParameter)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)).ToList()
                }
            };
            var globalAspects = new List<DynamicProxyFactoryConfigurations.GlobalInterceptorConfig>
            {
                new DynamicProxyFactoryConfigurations.GlobalInterceptorConfig(new GlobalAspect(), new GlobalAspectMethodMatcher())
            };
            return new Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>>
            (
                serviceType,
                targetType,
                new DynamicProxyFactoryConfigurations(default, default, true, globalAspects),
                true,
                dict.OrderBy(kvp => string.Format("{0}{1}", kvp.Key.Name, kvp.Key.GetParameters().Length)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            );
        }

        private static Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>> GetInterceptorTypeDataWithUninspectedInterface()
        {
            var serviceType = typeof(IUninspectedFakeService);
            var targetType = typeof(UninspectedFakeService);
            return new Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>>
            (
                serviceType,
                targetType,
                new DynamicProxyFactoryConfigurations(default, default, default, default),
                false,
                null
            );
        }

        private static Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>> GetInterceptorTypeDataWithConcreteType()
        {
            var serviceType = typeof(SimpleFakeService);
            var targetType = typeof(SimpleFakeService);
            return new Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>>
            (
                serviceType,
                targetType,
                new DynamicProxyFactoryConfigurations(default, default, default, default),
                false,
                null
            );
        }

        private static Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>> GetInterceptorTypeDataWithInterfaceContainingAllScenarios()
        {
            var serviceType = typeof(IFakeService);
            var targetType = typeof(FakeService);
            var typeDefinitionInterceptors = serviceType.CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)).ToList();
            var aspect1InterceptorList = typeDefinitionInterceptors.Where(attr => typeof(Aspect1Attribute).IsAssignableFrom(attr.AttributeType)).ToList();
            var dict = new Dictionary<MethodInfo, IEnumerable<CustomAttributeData>>
            {
                {
                    serviceType.GetEvent(nameof(IFakeService.OnChanged)).GetAddMethod(),
                    typeDefinitionInterceptors.Concat(
                        serviceType.GetEvent(nameof(IFakeService.OnChanged)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                            .Concat(serviceType.GetEvent(nameof(IFakeService.OnChanged))
                                .GetAddMethod()
                                .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))))
                            .ToList()
                },
                {
                    serviceType.GetEvent(nameof(IFakeService.OnChanged)).GetRemoveMethod(),
                    typeDefinitionInterceptors.Concat(
                        serviceType.GetEvent(nameof(IFakeService.OnChanged)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                            .Concat(serviceType.GetEvent(nameof(IFakeService.OnChanged))
                                .GetRemoveMethod()
                                .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))))
                            .ToList()
                },
                {
                    serviceType.GetProperty(nameof(IFakeService.ValueTypeProperty)).GetGetMethod(),
                    typeDefinitionInterceptors.Concat(serviceType.GetProperty(nameof(IFakeService.ValueTypeProperty))
                            .GetGetMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetProperty(nameof(IFakeService.ValueTypeProperty)).GetSetMethod(),
                    typeDefinitionInterceptors.Concat(serviceType.GetProperty(nameof(IFakeService.ValueTypeProperty))
                            .GetSetMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetProperty(nameof(IFakeService.ReferenceTypeProperty)).GetGetMethod(),
                    typeDefinitionInterceptors.Concat(serviceType.GetProperty(nameof(IFakeService.ReferenceTypeProperty))
                            .GetGetMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetProperty(nameof(IFakeService.ReferenceTypeProperty)).GetSetMethod(),
                    typeDefinitionInterceptors.Concat(serviceType.GetProperty(nameof(IFakeService.ReferenceTypeProperty))
                            .GetSetMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(IFakeService.DoSomethingWithoutParameterAndWithoutReturn)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(IFakeService.DoSomethingWithoutParameterAndWithoutReturn)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(IFakeService.InterceptedDoSomethingWithoutParameterAndWithoutReturn)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(IFakeService.InterceptedDoSomethingWithoutParameterAndWithoutReturn)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(IFakeService.DoSomethingWithParameterAndWithoutReturn)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(IFakeService.DoSomethingWithParameterAndWithoutReturn)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(IFakeService.InterceptedDoSomethingWithParameterAndWithoutReturn)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(IFakeService.InterceptedDoSomethingWithParameterAndWithoutReturn)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(IFakeService.DoSomethingAsyncWithoutParameterAndWithoutReturn)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(IFakeService.DoSomethingAsyncWithoutParameterAndWithoutReturn)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(IFakeService.InterceptedDoSomethingAsyncWithoutParameterAndWithoutReturn)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(IFakeService.InterceptedDoSomethingAsyncWithoutParameterAndWithoutReturn)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
#if NETCOREAPP3_1_OR_GREATER
                {
                    serviceType.GetMethod(nameof(IFakeService.DoSomethingValueAsyncWithoutParameterAndWithoutReturn)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(IFakeService.DoSomethingValueAsyncWithoutParameterAndWithoutReturn)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(IFakeService.InterceptedDoSomethingValueAsyncWithoutParameterAndWithoutReturn)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(IFakeService.InterceptedDoSomethingValueAsyncWithoutParameterAndWithoutReturn)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(IFakeService.DoSomethingValueAsyncWithParameterAndWithoutReturn)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(IFakeService.DoSomethingValueAsyncWithParameterAndWithoutReturn)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(IFakeService.InterceptedDoSomethingValueAsyncWithParameterAndWithoutReturn)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(IFakeService.InterceptedDoSomethingValueAsyncWithParameterAndWithoutReturn)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
#endif
                {
                    serviceType.GetMethod(nameof(IFakeService.DoSomethingWithoutParameterAndValueTypeReturn)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(IFakeService.DoSomethingWithoutParameterAndValueTypeReturn)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(IFakeService.InterceptedDoSomethingWithoutParameterAndValueTypeReturn)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(IFakeService.InterceptedDoSomethingWithoutParameterAndValueTypeReturn)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(IFakeService.DoSomethingWithParameterAndValueTypeReturn)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(IFakeService.DoSomethingWithParameterAndValueTypeReturn)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(IFakeService.InterceptedDoSomethingWithParameterAndValueTypeReturn)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(IFakeService.InterceptedDoSomethingWithParameterAndValueTypeReturn)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(IFakeService.DoSomethingWithoutParameterAndReferenceTypeReturn)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(IFakeService.DoSomethingWithoutParameterAndReferenceTypeReturn)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(IFakeService.InterceptedDoSomethingWithoutParameterAndReferenceTypeReturn)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(IFakeService.InterceptedDoSomethingWithoutParameterAndReferenceTypeReturn)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(IFakeService.DoSomethingWithParameterAndReferenceTypeReturn)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(IFakeService.DoSomethingWithParameterAndReferenceTypeReturn)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(IFakeService.InterceptedDoSomethingWithParameterAndReferenceTypeReturn)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(IFakeService.InterceptedDoSomethingWithParameterAndReferenceTypeReturn)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(IFakeService.DoSomethingAsyncWithoutParameterAndValueTypeReturn)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(IFakeService.DoSomethingAsyncWithoutParameterAndValueTypeReturn)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(IFakeService.InterceptedDoSomethingAsyncWithoutParameterAndValueTypeReturn)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(IFakeService.InterceptedDoSomethingAsyncWithoutParameterAndValueTypeReturn)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(IFakeService.DoSomethingAsyncWithParameterAndValueTypeReturn)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(IFakeService.DoSomethingAsyncWithParameterAndValueTypeReturn)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(IFakeService.InterceptedDoSomethingAsyncWithParameterAndValueTypeReturn)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(IFakeService.InterceptedDoSomethingAsyncWithParameterAndValueTypeReturn)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
#if NETCOREAPP3_1_OR_GREATER
                {
                    serviceType.GetMethod(nameof(IFakeService.DoSomethingValueAsyncWithoutParameterAndValueTypeReturn)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(IFakeService.DoSomethingValueAsyncWithoutParameterAndValueTypeReturn)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(IFakeService.InterceptedDoSomethingValueAsyncWithoutParameterAndValueTypeReturn)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(IFakeService.InterceptedDoSomethingValueAsyncWithoutParameterAndValueTypeReturn)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(IFakeService.DoSomethingValueAsyncWithParameterAndValueTypeReturn)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(IFakeService.DoSomethingValueAsyncWithParameterAndValueTypeReturn)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(IFakeService.InterceptedDoSomethingValueAsyncWithParameterAndValueTypeReturn)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(IFakeService.InterceptedDoSomethingValueAsyncWithParameterAndValueTypeReturn)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
#endif
                {
                    serviceType.GetMethod(nameof(IFakeService.DoSomethingAsyncWithoutParameterAndReferenceypeReturn)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(IFakeService.DoSomethingAsyncWithoutParameterAndReferenceypeReturn)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(IFakeService.InterceptedDoSomethingAsyncWithoutParameterAndReferenceypeReturn)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(IFakeService.InterceptedDoSomethingAsyncWithoutParameterAndReferenceypeReturn)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(IFakeService.DoSomethingAsyncWithParameterAndReferenceTypeReturn)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(IFakeService.DoSomethingAsyncWithParameterAndReferenceTypeReturn)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(IFakeService.InterceptedDoSomethingAsyncWithParameterAndReferenceTypeReturn)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(IFakeService.InterceptedDoSomethingAsyncWithParameterAndReferenceTypeReturn)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
#if NETCOREAPP3_1_OR_GREATER
                {
                    serviceType.GetMethod(nameof(IFakeService.DoSomethingValueAsyncWithoutParameterAndReferenceTypeReturn)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(IFakeService.DoSomethingValueAsyncWithoutParameterAndReferenceTypeReturn)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(IFakeService.InterceptedDoSomethingValueAsyncWithoutParameterAndReferenceTypeReturn)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(IFakeService.InterceptedDoSomethingValueAsyncWithoutParameterAndReferenceTypeReturn)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(IFakeService.DoSomethingValueAsyncWithParameterAndReferenceTypeReturn)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(IFakeService.DoSomethingValueAsyncWithParameterAndReferenceTypeReturn)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(IFakeService.InterceptedDoSomethingValueAsyncWithParameterAndReferenceTypeReturn)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(IFakeService.InterceptedDoSomethingValueAsyncWithParameterAndReferenceTypeReturn)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                }
#endif
            };

            var globalAspects = new List<DynamicProxyFactoryConfigurations.GlobalInterceptorConfig>
            {
                new DynamicProxyFactoryConfigurations.GlobalInterceptorConfig(new GlobalAspect(), new GlobalAspectMethodMatcher())
            };
            return new Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>>
            (
                serviceType,
                targetType,
                new DynamicProxyFactoryConfigurations(InterceptedEventMethod.All, InterceptedPropertyMethod.All, false, globalAspects),
                true,
                dict.OrderBy(kvp => string.Format("{0}{1}", kvp.Key.Name, kvp.Key.GetParameters().Length)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            );
        }

        private static Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>> GetInterceptorTypeDataUsingDefaultConfigsAndGenericReferenceType()
        {
            var serviceType = typeof(ISimpleGenericFakeService<Dictionary<string, int>>);
            var targetType = typeof(SimpleGenericReferenceTypeFakeService);
            var typeDefinitionInterceptors = serviceType.CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)).ToList();
            var aspect1InterceptorList = typeDefinitionInterceptors.Where(attr => typeof(Aspect1Attribute).IsAssignableFrom(attr.AttributeType)).ToList();
            var dict = new Dictionary<MethodInfo, IEnumerable<CustomAttributeData>>
            {
                {
                    serviceType.GetProperty(nameof(ISimpleGenericFakeService<int>.Value)).GetGetMethod(),
                    typeDefinitionInterceptors.Concat(serviceType.GetProperty(nameof(ISimpleGenericFakeService<int>.Value))
                        .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                        .Concat(serviceType.GetProperty(nameof(ISimpleGenericFakeService<int>.Value))
                            .GetGetMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))))
                        .ToList()
                },
                {
                    serviceType.GetProperty(nameof(ISimpleGenericFakeService<int>.Value)).GetSetMethod(),
                    typeDefinitionInterceptors.Concat(serviceType.GetProperty(nameof(ISimpleGenericFakeService<int>.Value))
                        .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                        .Concat(serviceType.GetProperty(nameof(ISimpleGenericFakeService<int>.Value))
                            .GetSetMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(ISimpleGenericFakeService<int>.GetValue)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(ISimpleGenericFakeService<int>.GetValue)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))).ToList()
                },
                {
                    serviceType.GetMethod(nameof(ISimpleGenericFakeService<int>.SetValue)),
                    aspect1InterceptorList.Concat(serviceType.GetMethod(nameof(ISimpleGenericFakeService<int>.SetValue)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))).ToList()
                },
                {
                    serviceType.GetMethod(nameof(ISimpleGenericFakeService<int>.WaitMethod)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(ISimpleGenericFakeService<int>.WaitMethod)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))).ToList()
                },
                {
                    serviceType.GetMethod(nameof(ISimpleGenericFakeService<int>.ValueWaitMethod)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(ISimpleGenericFakeService<int>.ValueWaitMethod)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))).ToList()
                }
            };
            var globalAspects = new List<DynamicProxyFactoryConfigurations.GlobalInterceptorConfig>
            {
                new DynamicProxyFactoryConfigurations.GlobalInterceptorConfig(new GlobalAspect(), new GlobalAspectMethodMatcher())
            };
            return new Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>>
            (
                serviceType,
                targetType,
                new DynamicProxyFactoryConfigurations(InterceptedEventMethod.All, InterceptedPropertyMethod.All, false, globalAspects),
                true,
                dict.OrderBy(kvp => string.Format("{0}{1}", kvp.Key.Name, kvp.Key.GetParameters().Length)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            );
        }
        
        private static Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>> GetInterceptorTypeDataUsingDefaultConfigsAndGenericValueType()
        {
            var serviceType = typeof(ISimpleGenericFakeService<int>);
            var targetType = typeof(SimpleGenericValueTypeFakeService);
            var typeDefinitionInterceptors = serviceType.CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)).ToList();
            var aspect1InterceptorList = typeDefinitionInterceptors.Where(attr => typeof(Aspect1Attribute).IsAssignableFrom(attr.AttributeType)).ToList();
            var dict = new Dictionary<MethodInfo, IEnumerable<CustomAttributeData>>
            {
                {
                    serviceType.GetProperty(nameof(ISimpleGenericFakeService<int>.Value)).GetGetMethod(),
                    typeDefinitionInterceptors.Concat(serviceType.GetProperty(nameof(ISimpleGenericFakeService<int>.Value))
                        .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                        .Concat(serviceType.GetProperty(nameof(ISimpleGenericFakeService<int>.Value))
                            .GetGetMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))))
                        .ToList()
                },
                {
                    serviceType.GetProperty(nameof(ISimpleGenericFakeService<int>.Value)).GetSetMethod(),
                    typeDefinitionInterceptors.Concat(serviceType.GetProperty(nameof(ISimpleGenericFakeService<int>.Value))
                        .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                        .Concat(serviceType.GetProperty(nameof(ISimpleGenericFakeService<int>.Value))
                            .GetSetMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(ISimpleGenericFakeService<int>.GetValue)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(ISimpleGenericFakeService<int>.GetValue)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))).ToList()
                },
                {
                    serviceType.GetMethod(nameof(ISimpleGenericFakeService<int>.SetValue)),
                    aspect1InterceptorList.Concat(serviceType.GetMethod(nameof(ISimpleGenericFakeService<int>.SetValue)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))).ToList()
                },
                {
                    serviceType.GetMethod(nameof(ISimpleGenericFakeService<int>.WaitMethod)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(ISimpleGenericFakeService<int>.WaitMethod)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))).ToList()
                },
                {
                    serviceType.GetMethod(nameof(ISimpleGenericFakeService<int>.ValueWaitMethod)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(ISimpleGenericFakeService<int>.ValueWaitMethod)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))).ToList()
                }
            };
            var globalAspects = new List<DynamicProxyFactoryConfigurations.GlobalInterceptorConfig>
            {
                new DynamicProxyFactoryConfigurations.GlobalInterceptorConfig(new GlobalAspect(), new GlobalAspectMethodMatcher())
            };
            return new Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>>
            (
                serviceType,
                targetType,
                new DynamicProxyFactoryConfigurations(InterceptedEventMethod.All, InterceptedPropertyMethod.All, false, globalAspects),
                true,
                dict.OrderBy(kvp => string.Format("{0}{1}", kvp.Key.Name, kvp.Key.GetParameters().Length)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            );
        }

        private static Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>> GetInterceptorTypeDataUsingDefaultConfigsAndInheritedGenericReferenceType()
        {
            var serviceType = typeof(ISimpleGenericFakeService<List<int>>);
            var targetType = typeof(SimpleInheritedGenericReferenceTypeFakeService<List<int>, int>);
            var typeDefinitionInterceptors = serviceType.CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)).ToList();
            var aspect1InterceptorList = typeDefinitionInterceptors.Where(attr => typeof(Aspect1Attribute).IsAssignableFrom(attr.AttributeType)).ToList();
            var dict = new Dictionary<MethodInfo, IEnumerable<CustomAttributeData>>
            {
                {
                    serviceType.GetProperty(nameof(ISimpleGenericFakeService<int>.Value)).GetGetMethod(),
                    typeDefinitionInterceptors.Concat(serviceType.GetProperty(nameof(ISimpleGenericFakeService<int>.Value))
                        .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                        .Concat(serviceType.GetProperty(nameof(ISimpleGenericFakeService<int>.Value))
                            .GetGetMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))))
                        .ToList()
                },
                {
                    serviceType.GetProperty(nameof(ISimpleGenericFakeService<int>.Value)).GetSetMethod(),
                    typeDefinitionInterceptors.Concat(serviceType.GetProperty(nameof(ISimpleGenericFakeService<int>.Value))
                        .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                        .Concat(serviceType.GetProperty(nameof(ISimpleGenericFakeService<int>.Value))
                            .GetSetMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(ISimpleGenericFakeService<int>.GetValue)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(ISimpleGenericFakeService<int>.GetValue)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))).ToList()
                },
                {
                    serviceType.GetMethod(nameof(ISimpleGenericFakeService<int>.SetValue)),
                    aspect1InterceptorList.Concat(serviceType.GetMethod(nameof(ISimpleGenericFakeService<int>.SetValue)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))).ToList()
                },
                {
                    serviceType.GetMethod(nameof(ISimpleGenericFakeService<int>.WaitMethod)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(ISimpleGenericFakeService<int>.WaitMethod)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))).ToList()
                },
                {
                    serviceType.GetMethod(nameof(ISimpleGenericFakeService<int>.ValueWaitMethod)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(ISimpleGenericFakeService<int>.ValueWaitMethod)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))).ToList()
                }
            };
            var globalAspects = new List<DynamicProxyFactoryConfigurations.GlobalInterceptorConfig>
            {
                new DynamicProxyFactoryConfigurations.GlobalInterceptorConfig(new GlobalAspect(), new GlobalAspectMethodMatcher())
            };
            return new Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>>
            (
                serviceType,
                targetType,
                new DynamicProxyFactoryConfigurations(InterceptedEventMethod.All, InterceptedPropertyMethod.All, false, globalAspects),
                true,
                dict.OrderBy(kvp => string.Format("{0}{1}", kvp.Key.Name, kvp.Key.GetParameters().Length)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            );
        }

        private static Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>> GetInterceptorTypeDataUsingDefaultConfigsAndInheritedGenericValueType()
        {
            var serviceType = typeof(ISimpleGenericFakeService<int>);
            var targetType = typeof(SimpleInheritedGenericValueTypeFakeService<int>);
            var typeDefinitionInterceptors = serviceType.CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)).ToList();
            var aspect1InterceptorList = typeDefinitionInterceptors.Where(attr => typeof(Aspect1Attribute).IsAssignableFrom(attr.AttributeType)).ToList();
            var dict = new Dictionary<MethodInfo, IEnumerable<CustomAttributeData>>
            {
                {
                    serviceType.GetProperty(nameof(ISimpleGenericFakeService<int>.Value)).GetGetMethod(),
                    typeDefinitionInterceptors.Concat(serviceType.GetProperty(nameof(ISimpleGenericFakeService<int>.Value))
                        .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                        .Concat(serviceType.GetProperty(nameof(ISimpleGenericFakeService<int>.Value))
                            .GetGetMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))))
                        .ToList()
                },
                {
                    serviceType.GetProperty(nameof(ISimpleGenericFakeService<int>.Value)).GetSetMethod(),
                    typeDefinitionInterceptors.Concat(serviceType.GetProperty(nameof(ISimpleGenericFakeService<int>.Value))
                        .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))
                        .Concat(serviceType.GetProperty(nameof(ISimpleGenericFakeService<int>.Value))
                            .GetSetMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))))
                        .ToList()
                },
                {
                    serviceType.GetMethod(nameof(ISimpleGenericFakeService<int>.GetValue)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(ISimpleGenericFakeService<int>.GetValue)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))).ToList()
                },
                {
                    serviceType.GetMethod(nameof(ISimpleGenericFakeService<int>.SetValue)),
                    aspect1InterceptorList.Concat(serviceType.GetMethod(nameof(ISimpleGenericFakeService<int>.SetValue)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))).ToList()
                },
                {
                    serviceType.GetMethod(nameof(ISimpleGenericFakeService<int>.WaitMethod)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(ISimpleGenericFakeService<int>.WaitMethod)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))).ToList()
                },
                {
                    serviceType.GetMethod(nameof(ISimpleGenericFakeService<int>.ValueWaitMethod)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(ISimpleGenericFakeService<int>.ValueWaitMethod)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))).ToList()
                }
            };
            var globalAspects = new List<DynamicProxyFactoryConfigurations.GlobalInterceptorConfig>
            {
                new DynamicProxyFactoryConfigurations.GlobalInterceptorConfig(new GlobalAspect(), new GlobalAspectMethodMatcher())
            };
            return new Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>>
            (
                serviceType,
                targetType,
                new DynamicProxyFactoryConfigurations(InterceptedEventMethod.All, InterceptedPropertyMethod.All, false, globalAspects),
                true,
                dict.OrderBy(kvp => string.Format("{0}{1}", kvp.Key.Name, kvp.Key.GetParameters().Length)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            );
        }

        private static Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>> GetInterceptorTypeDataUsingDefaultConfigsAndServiceWithGenericMethod()
        {
            var serviceType = typeof(IServiceWithGenericMethod);
            var targetType = typeof(ServiceWithGenericMethod);
            var typeDefinitionInterceptors = serviceType.CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)).ToList();
            var aspect1InterceptorList = typeDefinitionInterceptors.Where(attr => typeof(Aspect1Attribute).IsAssignableFrom(attr.AttributeType)).ToList();
            var dict = new Dictionary<MethodInfo, IEnumerable<CustomAttributeData>>
            {
                {
                    serviceType.GetMethods().Single(mi => mi.Name == nameof(IServiceWithGenericMethod.Call) && mi.GetParameters().Length == 0),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethods().Single(mi => mi.Name == nameof(IServiceWithGenericMethod.Call) && mi.GetParameters().Length == 0).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))).ToList()
                },
                {
                    serviceType.GetMethods().Single(mi => mi.Name == nameof(IServiceWithGenericMethod.Call) && mi.GetParameters().Length == 1 && !mi.GetParameters().First().ParameterType.IsGenericType),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethods().Single(mi => mi.Name == nameof(IServiceWithGenericMethod.Call) && mi.GetParameters().Length == 1 && !mi.GetParameters().First().ParameterType.IsGenericType).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))).ToList()
                },
                {
                    serviceType.GetMethods().Single(mi => mi.Name == nameof(IServiceWithGenericMethod.Call) && mi.GetParameters().Length == 1 && mi.GetParameters().First().ParameterType.IsGenericType && mi.GetParameters().First().ParameterType.GetGenericTypeDefinition().Equals(typeof(IContainer<>))),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethods().Single(mi => mi.Name == nameof(IServiceWithGenericMethod.Call) && mi.GetParameters().Length == 1 && mi.GetParameters().First().ParameterType.IsGenericType && mi.GetParameters().First().ParameterType.GetGenericTypeDefinition().Equals(typeof(IContainer<>))).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))).ToList()
                },
                {
                    serviceType.GetMethods().Single(mi => mi.Name == nameof(IServiceWithGenericMethod.Call) && mi.GetParameters().Length == 1 && mi.GetParameters().First().ParameterType.IsGenericType && mi.GetParameters().First().ParameterType.GetGenericTypeDefinition().Equals(typeof(Dictionary<,>))),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethods().Single(mi => mi.Name == nameof(IServiceWithGenericMethod.Call) && mi.GetParameters().Length == 1 && mi.GetParameters().First().ParameterType.IsGenericType && mi.GetParameters().First().ParameterType.GetGenericTypeDefinition().Equals(typeof(Dictionary<,>))).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))).ToList()
                },
                {
                    serviceType.GetMethods().Single(mi => mi.Name == nameof(IServiceWithGenericMethod.CallWithRefParameter) && mi.GetParameters().Length == 1 && !mi.GetParameters().First().ParameterType.GetElementType().IsGenericType),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethods().Single(mi => mi.Name == nameof(IServiceWithGenericMethod.CallWithRefParameter) && mi.GetParameters().Length == 1 && !mi.GetParameters().First().ParameterType.GetElementType().IsGenericType).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))).ToList()
                },
                {
                    serviceType.GetMethods().Single(mi => mi.Name == nameof(IServiceWithGenericMethod.CallWithRefParameter) && mi.GetParameters().Length == 1 && mi.GetParameters().First().ParameterType.GetElementType().IsGenericType && mi.GetParameters().First().ParameterType.GetElementType().GetGenericTypeDefinition().Equals(typeof(IContainer<>))),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethods().Single(mi => mi.Name == nameof(IServiceWithGenericMethod.CallWithRefParameter) && mi.GetParameters().Length == 1 && mi.GetParameters().First().ParameterType.GetElementType().IsGenericType && mi.GetParameters().First().ParameterType.GetElementType().GetGenericTypeDefinition().Equals(typeof(IContainer<>))).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))).ToList()
                },
                {
                    serviceType.GetMethods().Single(mi => mi.Name == nameof(IServiceWithGenericMethod.CallWithRefParameter) && mi.GetParameters().Length == 1 && mi.GetParameters().First().ParameterType.GetElementType().IsGenericType && mi.GetParameters().First().ParameterType.GetElementType().GetGenericTypeDefinition().Equals(typeof(Dictionary<,>))),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethods().Single(mi => mi.Name == nameof(IServiceWithGenericMethod.CallWithRefParameter) && mi.GetParameters().Length == 1 && mi.GetParameters().First().ParameterType.GetElementType().IsGenericType && mi.GetParameters().First().ParameterType.GetElementType().GetGenericTypeDefinition().Equals(typeof(Dictionary<,>))).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))).ToList()
                },
                {
                    serviceType.GetMethods().Single(mi => mi.Name == nameof(IServiceWithGenericMethod.CallWithOutParameter) && mi.GetParameters().Length == 2 && !mi.GetParameters().Last().ParameterType.GetElementType().IsGenericType),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethods().Single(mi => mi.Name == nameof(IServiceWithGenericMethod.CallWithOutParameter) && mi.GetParameters().Length == 2 && !mi.GetParameters().Last().ParameterType.GetElementType().IsGenericType).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))).ToList()
                },
                {
                    serviceType.GetMethods().Single(mi => mi.Name == nameof(IServiceWithGenericMethod.CallWithOutParameter) && mi.GetParameters().Length == 2 && mi.GetParameters().Last().ParameterType.GetElementType().IsGenericType && mi.GetParameters().Last().ParameterType.GetElementType().GetGenericTypeDefinition().Equals(typeof(IContainer<>))),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethods().Single(mi => mi.Name == nameof(IServiceWithGenericMethod.CallWithOutParameter) && mi.GetParameters().Length == 2 && mi.GetParameters().Last().ParameterType.GetElementType().IsGenericType && mi.GetParameters().Last().ParameterType.GetElementType().GetGenericTypeDefinition().Equals(typeof(IContainer<>))).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))).ToList()
                },
                {
                    serviceType.GetMethods().Single(mi => mi.Name == nameof(IServiceWithGenericMethod.CallWithOutParameter) && mi.GetParameters().Length == 2 && mi.GetParameters().Last().ParameterType.GetElementType().IsGenericType && mi.GetParameters().Last().ParameterType.GetElementType().GetGenericTypeDefinition().Equals(typeof(Dictionary<,>))),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethods().Single(mi => mi.Name == nameof(IServiceWithGenericMethod.CallWithOutParameter) && mi.GetParameters().Length == 2 && mi.GetParameters().Last().ParameterType.GetElementType().IsGenericType && mi.GetParameters().Last().ParameterType.GetElementType().GetGenericTypeDefinition().Equals(typeof(Dictionary<,>))).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))).ToList()
                }
            };
            var globalAspects = new List<DynamicProxyFactoryConfigurations.GlobalInterceptorConfig>
            {
                new DynamicProxyFactoryConfigurations.GlobalInterceptorConfig(new GlobalAspect(), new GlobalAspectMethodMatcher())
            };
            return new Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>>
            (
                serviceType,
                targetType,
                new DynamicProxyFactoryConfigurations(InterceptedEventMethod.All, InterceptedPropertyMethod.All, false, globalAspects),
                true,
                dict.OrderBy(kvp => string.Format("{0}{1}", kvp.Key.Name, kvp.Key.GetParameters().Length)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            );
        }
    }
}
