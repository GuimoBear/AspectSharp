using AspectSharp.Abstractions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AspectSharp.DynamicProxy.Utils
{
    internal static class GenericTypesExtensions
    {
        public static IEnumerable<CustomAttributeData> ExcludeTypeDefinitionAspects<TCustomExcludeAttribute>(this MemberInfo memberInfo, IEnumerable<CustomAttributeData> typeDefinitionInterceptors, Predicate<TCustomExcludeAttribute> customAttributePredicate, MethodInfo currentMethodInfo)
            where TCustomExcludeAttribute : ExcludeAspectsFromTypeDefinitionAttribute
        {
            var excludingAttributes = memberInfo
                                .GetCustomAttributes(true)
                                .Where(attr => attr.GetType() == typeof(ExcludeAspectsFromTypeDefinitionAttribute) ||
                                               attr.GetType() == typeof(TCustomExcludeAttribute));
            if (!(currentMethodInfo is null))
                excludingAttributes = excludingAttributes.Concat(currentMethodInfo.GetCustomAttributes(true).Where(attr => attr.GetType() == typeof(ExcludeAspectsFromTypeDefinitionAttribute)));
            foreach (var attribute in excludingAttributes)
            {
                if (attribute is TCustomExcludeAttribute excludeAspectsFromThisProperty)
                {
                    if (excludeAspectsFromThisProperty.ExcludeAll)
                    {
                        typeDefinitionInterceptors = Enumerable.Empty<CustomAttributeData>().ToList();
                        break;
                    }
                    else if (customAttributePredicate(excludeAspectsFromThisProperty))
                        typeDefinitionInterceptors = typeDefinitionInterceptors.Where(attr => !excludeAspectsFromThisProperty.Aspects.Any(attributeType => attributeType == attr.AttributeType)).ToList();
                }
                else if (attribute is ExcludeAspectsFromTypeDefinitionAttribute excludeAspects)
                {
                    typeDefinitionInterceptors = excludeAspects.ExcludeAspectsFromTypeDefinition(typeDefinitionInterceptors);
                    if (excludeAspects.ExcludeAll)
                        break;
                }
            }
            return typeDefinitionInterceptors;
        }

        public static IEnumerable<CustomAttributeData> IncludeTypeDefinitionAspects<TCustomIncludeAttribute>(this MemberInfo memberInfo, IEnumerable<CustomAttributeData> typeDefinitionInterceptors, Predicate<TCustomIncludeAttribute> customAttributePredicate, MethodInfo currentMethodInfo)
            where TCustomIncludeAttribute : IncludeAspectsFromTypeDefinitionAttribute
        {
            var receivedTypeDefinitionInterceptors = typeDefinitionInterceptors;

            typeDefinitionInterceptors = Enumerable.Empty<CustomAttributeData>().ToList();
            var excludingAttributes = memberInfo
                                .GetCustomAttributes(true)
                                .Where(attr => attr.GetType() == typeof(IncludeAspectsFromTypeDefinitionAttribute) ||
                                               attr.GetType() == typeof(TCustomIncludeAttribute));
            if (!(currentMethodInfo is null))
                excludingAttributes = excludingAttributes.Concat(currentMethodInfo.GetCustomAttributes(true).Where(attr => attr.GetType() == typeof(IncludeAspectsFromTypeDefinitionAttribute)));
            foreach (var attribute in excludingAttributes)
            {
                if (attribute is TCustomIncludeAttribute includeAspectsFromThisProperty)
                {
                    if (includeAspectsFromThisProperty.IncludeAll)
                    {
                        typeDefinitionInterceptors = receivedTypeDefinitionInterceptors.ToList();
                        break;
                    }
                    else if (customAttributePredicate(includeAspectsFromThisProperty))
                        typeDefinitionInterceptors = receivedTypeDefinitionInterceptors.Where(attr => includeAspectsFromThisProperty.Aspects.Any(attributeType => attributeType == attr.AttributeType)).ToList();
                }
                else if (attribute is IncludeAspectsFromTypeDefinitionAttribute includeAspects)
                {
                    typeDefinitionInterceptors = includeAspects.IncludeAspectsFromTypeDefinition(receivedTypeDefinitionInterceptors);
                    if (includeAspects.IncludeAll)
                        break;
                }
            }
            return typeDefinitionInterceptors;
        }



        internal static IEnumerable<CustomAttributeData> ExcludeAspectsFromTypeDefinition(this ExcludeAspectsFromTypeDefinitionAttribute excludeAspects, IEnumerable<CustomAttributeData> typeDefinitionInterceptors)
        {
            if (excludeAspects.ExcludeAll)
                typeDefinitionInterceptors = Enumerable.Empty<CustomAttributeData>().ToList();
            else
                typeDefinitionInterceptors = typeDefinitionInterceptors.Where(attr => !excludeAspects.Aspects.Any(attributeType => attributeType == attr.AttributeType)).ToList();
            return typeDefinitionInterceptors;
        }

        internal static IEnumerable<CustomAttributeData> IncludeAspectsFromTypeDefinition(this IncludeAspectsFromTypeDefinitionAttribute includeAspects, IEnumerable<CustomAttributeData> typeDefinitionInterceptors)
        {
            var receivedTypeDefinitionInterceptors = typeDefinitionInterceptors;

            if (includeAspects.IncludeAll)
                typeDefinitionInterceptors = receivedTypeDefinitionInterceptors.ToList();
            else
                typeDefinitionInterceptors = receivedTypeDefinitionInterceptors.Where(attr => includeAspects.Aspects.Any(attributeType => attributeType == attr.AttributeType)).ToList();
            return typeDefinitionInterceptors;
        }
    }
}
