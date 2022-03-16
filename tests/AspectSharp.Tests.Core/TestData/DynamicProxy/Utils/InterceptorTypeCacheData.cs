using AspectSharp.Abstractions;
using AspectSharp.Abstractions.Attributes;
using AspectSharp.Abstractions.Enums;
using AspectSharp.Tests.Core.Aspects;
using AspectSharp.Tests.Core.Services;
using AspectSharp.Tests.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AspectSharp.Tests.Core.TestData.DynamicProxy.Utils
{
    internal static class InterceptorTypeCacheData
    {
        public static IEnumerable<Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>>> GetInterceptorTypeDataTheoryData()
        {
            //yield return GetInterceptorTypeDataUsingDefaultConfigs();
            //yield return GetInterceptorTypeDataWithInterceptOnAddEvent();
            //yield return GetInterceptorTypeDataWithInterceptOnRemoveEvent();
            //yield return GetInterceptorTypeDataWithInterceptOnAnyEventMethod();
            //yield return GetInterceptorTypeDataWithInterceptOnGetProperty();
            //yield return GetInterceptorTypeDataWithInterceptOnSetProperty();
            yield return GetInterceptorTypeDataWithInterceptOnAnyPropertyMethod();
            //yield return GetInterceptorTypeDataExcludingTypeDefinitionAspectsForMethods();
            //yield return GetInterceptorTypeDataWithIninspectedInterface();
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
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetGetMethod(),
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetGetMethod().CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)).ToList()
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetSetMethod(),
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetSetMethod().CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)).ToList()
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
                }
            };
            return new Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>>
            (
                serviceType,
                targetType,
                new DynamicProxyFactoryConfigurations(default, default, default),
                true,
                dict
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
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetGetMethod(),
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetGetMethod().CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)).ToList()
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetSetMethod(),
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetSetMethod().CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)).ToList()
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
                }
            };
            return new Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>>
            (
                serviceType,
                targetType,
                new DynamicProxyFactoryConfigurations(InterceptedEventMethod.Add, default, default),
                true,
                dict
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
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetGetMethod(),
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetGetMethod().CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)).ToList()
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetSetMethod(),
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetSetMethod().CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)).ToList()
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
                }
            };
            return new Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>>
            (
                serviceType,
                targetType,
                new DynamicProxyFactoryConfigurations(InterceptedEventMethod.Remove, default, default),
                true,
                dict
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
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetGetMethod(),
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetGetMethod().CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)).ToList()
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetSetMethod(),
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetSetMethod().CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)).ToList()
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
                }
            };
            return new Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>>
            (
                serviceType,
                targetType,
                new DynamicProxyFactoryConfigurations(InterceptedEventMethod.All, default, default),
                true,
                dict
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
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetGetMethod(),
                    typeDefinitionInterceptors.Concat(
                        serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty))
                            .GetGetMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetSetMethod(),
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetSetMethod().CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)).ToList()
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
                }
            };
            return new Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>>
            (
                serviceType,
                targetType,
                new DynamicProxyFactoryConfigurations(default, InterceptedPropertyMethod.Get, default),
                true,
                dict
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
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetGetMethod(),
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetGetMethod().CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)).ToList()
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetSetMethod(),
                    typeDefinitionInterceptors.Concat(
                        serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty))
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
                    serviceType.GetProperty(nameof(ISimpleFakeService.Property)).GetSetMethod(),
                    typeDefinitionInterceptors.ToList()
                }
            };
            return new Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>>
            (
                serviceType,
                targetType,
                new DynamicProxyFactoryConfigurations(default, InterceptedPropertyMethod.Set, default),
                true,
                dict
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
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetGetMethod(),
                    typeDefinitionInterceptors.Concat(
                        serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty))
                            .GetGetMethod()
                            .CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)))
                        .ToList()
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetSetMethod(),
                    typeDefinitionInterceptors.Concat(
                        serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty))
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
                    serviceType.GetProperty(nameof(ISimpleFakeService.Property)).GetSetMethod(),
                    typeDefinitionInterceptors.ToList()
                }
            };
            return new Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>>
            (
                serviceType,
                targetType,
                new DynamicProxyFactoryConfigurations(default, InterceptedPropertyMethod.All, default),
                true,
                dict
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
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetGetMethod(),
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetGetMethod().CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)).ToList()
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetSetMethod(),
                    serviceType.GetProperty(nameof(ISimpleFakeService.InterceptedProperty)).GetSetMethod().CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType)).ToList()
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
                }
            };
            return new Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>>
            (
                serviceType,
                targetType,
                new DynamicProxyFactoryConfigurations(default, default, true),
                true,
                dict
            );
        }

        private static Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>> GetInterceptorTypeDataWithIninspectedInterface()
        {
            var serviceType = typeof(IUninspectedFakeService);
            var targetType = typeof(UninspectedFakeService);
            return new Tuple<Type, Type, DynamicProxyFactoryConfigurations, bool, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>>>
            (
                serviceType,
                targetType,
                new DynamicProxyFactoryConfigurations(default, default, default),
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
                new DynamicProxyFactoryConfigurations(default, default, default),
                false,
                null
            );
        }
    }
}
