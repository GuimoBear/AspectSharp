using AspectSharp.Abstractions;
using AspectSharp.Abstractions.Attributes;
using AspectSharp.Abstractions.Enums;
using AspectSharp.DynamicProxy.Utils;
using AspectSharp.Tests.Core.Aspects;
using AspectSharp.Tests.Core.Services;
using AspectSharp.Tests.Core.Services.Interfaces;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace AspectSharp.Tests.Net5.DynamicProxy.Utils
{
    public class InterceptorTypeCacheTests
    {
        [Theory]
        [MemberData(nameof(GetInterceptorTypeDataTheoryData))]
        internal void GetInterceptorTypeDataTheory(Type serviceType, DynamicProxyFactoryConfigurations configs, bool expectedIsIntercepted, IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>> expectedInterceptors)
        {
            InterceptorTypeCache.TryGetInterceptedTypeData(serviceType, configs, out var interceptedTypeData)
                .Should().Be(expectedIsIntercepted);

            if (expectedIsIntercepted)
            {

                interceptedTypeData.IsIntercepted
                    .Should().Be(expectedIsIntercepted);

                interceptedTypeData.Interceptors.Keys
                    .Should().BeEquivalentTo(expectedInterceptors.Keys);

                foreach (var (values, expectedValues) in interceptedTypeData.Interceptors.Values.Zip(expectedInterceptors.Values))
                {
                    foreach (var (interceptor, expectedInterceptor) in values.Zip(expectedValues))
                    {
                        interceptor.AttributeType.Should().Be(expectedInterceptor.AttributeType);
                        interceptor.Constructor.Should().BeSameAs(expectedInterceptor.Constructor);
                        interceptor.ConstructorArguments.Should().BeEquivalentTo(expectedInterceptor.ConstructorArguments);
                        interceptor.NamedArguments.Should().BeEquivalentTo(expectedInterceptor.NamedArguments);
                    }
                }
            }
            else
                interceptedTypeData.Should().BeNull();
        }

        public static IEnumerable<object[]> GetInterceptorTypeDataTheoryData()
        {
            yield return GetInterceptorTypeDataUsingDefaultConfigs();
            yield return GetInterceptorTypeDataWithInterceptOnAddEvent();
            yield return GetInterceptorTypeDataWithInterceptOnRemoveEvent();
            yield return GetInterceptorTypeDataWithInterceptOnAnyEventMethod();
            yield return GetInterceptorTypeDataWithInterceptOnGetProperty();
            yield return GetInterceptorTypeDataWithInterceptOnSetProperty();
            yield return GetInterceptorTypeDataWithInterceptOnAnyPropertyMethod();
            yield return GetInterceptorTypeDataExcludingTypeDefinitionAspectsForMethods();
            yield return GetInterceptorTypeDataWithIninspectedInterface();
        }

        private static object[] GetInterceptorTypeDataUsingDefaultConfigs()
        {
            var serviceType = typeof(ISimpleFakeService);
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
                    typeDefinitionInterceptors
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
            return new object[]
            {
                serviceType,
                new DynamicProxyFactoryConfigurations(default, default, default),
                true,
                dict
            };
        }

        private static object[] GetInterceptorTypeDataWithInterceptOnAddEvent()
        {
            var serviceType = typeof(ISimpleFakeService);
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
                    typeDefinitionInterceptors
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
            return new object[]
            {
                serviceType,
                new DynamicProxyFactoryConfigurations(InterceptedEventMethod.Add, default, default),
                true,
                dict
            };
        }

        private static object[] GetInterceptorTypeDataWithInterceptOnRemoveEvent()
        {
            var serviceType = typeof(ISimpleFakeService);
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
                    typeDefinitionInterceptors
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
            return new object[]
            {
                serviceType,
                new DynamicProxyFactoryConfigurations(InterceptedEventMethod.Remove, default, default),
                true,
                dict
            };
        }

        private static object[] GetInterceptorTypeDataWithInterceptOnAnyEventMethod()
        {
            var serviceType = typeof(ISimpleFakeService);
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
                    typeDefinitionInterceptors
                },
                {
                    serviceType.GetMethod(nameof(ISimpleFakeService.InterceptedMethod)),
                    typeDefinitionInterceptors.Concat(serviceType.GetMethod(nameof(ISimpleFakeService.InterceptedMethod)).CustomAttributes.Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.AttributeType))).ToList()
                },
                {
                    serviceType.GetEvent(nameof(ISimpleFakeService.OnEvent)).GetAddMethod(),
                    typeDefinitionInterceptors.ToList()
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
            return new object[]
            {
                serviceType,
                new DynamicProxyFactoryConfigurations(InterceptedEventMethod.All, default, default),
                true,
                dict
            };
        }

        private static object[] GetInterceptorTypeDataWithInterceptOnGetProperty()
        {
            var serviceType = typeof(ISimpleFakeService);
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
                    typeDefinitionInterceptors
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
                    typeDefinitionInterceptors.ToList()
                }
            };
            return new object[]
            {
                serviceType,
                new DynamicProxyFactoryConfigurations(default, InterceptedPropertyMethod.Get, default),
                true,
                dict
            };
        }

        private static object[] GetInterceptorTypeDataWithInterceptOnSetProperty()
        {
            var serviceType = typeof(ISimpleFakeService);
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
                    typeDefinitionInterceptors
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
            return new object[]
            {
                serviceType,
                new DynamicProxyFactoryConfigurations(default, InterceptedPropertyMethod.Set, default),
                true,
                dict
            };
        }

        private static object[] GetInterceptorTypeDataWithInterceptOnAnyPropertyMethod()
        {
            var serviceType = typeof(ISimpleFakeService);
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
                    typeDefinitionInterceptors
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
                    typeDefinitionInterceptors.ToList()
                },
                {
                    serviceType.GetProperty(nameof(ISimpleFakeService.Property)).GetSetMethod(),
                    typeDefinitionInterceptors.ToList()
                }
            };
            return new object[]
            {
                serviceType,
                new DynamicProxyFactoryConfigurations(default, InterceptedPropertyMethod.All, default),
                true,
                dict
            };
        }

        private static object[] GetInterceptorTypeDataExcludingTypeDefinitionAspectsForMethods()
        {
            var serviceType = typeof(ISimpleFakeService);
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
            return new object[]
            {
                serviceType,
                new DynamicProxyFactoryConfigurations(default, default, true),
                true,
                dict
            };
        }

        private static object[] GetInterceptorTypeDataWithIninspectedInterface()
        {
            var serviceType = typeof(IUninspectedFakeService);
            return new object[]
            {
                serviceType,
                new DynamicProxyFactoryConfigurations(default, default, default),
                false,
                null
            };
        }

        private static object[] GetInterceptorTypeDataWithConcreteType()
        {
            var serviceType = typeof(SimpleFakeService);
            return new object[]
            {
                serviceType,
                new DynamicProxyFactoryConfigurations(default, default, default),
                false,
                null
            };
        }
    }
}
