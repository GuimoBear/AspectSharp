using AspectSharp.Abstractions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AspectSharp.DynamicProxy.Utils
{
    internal static class InterceptorTypeCache
    {
        private static readonly Type _abstractInterceptorType = typeof(AbstractInterceptorAttribute);

        private static readonly IDictionary<Type, InterceptedTypeData> _cachedTypes = new Dictionary<Type, InterceptedTypeData>();

        internal static bool TryGetInterceptedTypeData(Type type, out InterceptedTypeData interceptedTypeData)
        {
            if (_cachedTypes.TryGetValue(type, out interceptedTypeData))
                return true;

            var typeDefinitionAttributes = type
                       .CustomAttributes
                       .Where(attr => attr.AttributeType.IsAssignableTo(_abstractInterceptorType))
                       .ToList();

            var methodInterceptors = type
                .GetMethods()
                .Where(mi => mi.CustomAttributes.Any(attr => attr.AttributeType.IsAssignableTo(_abstractInterceptorType)))
                .ToDictionary(mi => mi, mi => mi.CustomAttributes.Where(attr => attr.AttributeType.IsAssignableTo(_abstractInterceptorType)).ToList() as IEnumerable<CustomAttributeData>);

            if (typeDefinitionAttributes.Any() || methodInterceptors.Any())
            {
                interceptedTypeData = new InterceptedTypeData
                {
                    HasInterceptorInTypeDefinition = typeDefinitionAttributes.Any(),
                    TypeDefinitionInterceptors = typeDefinitionAttributes,
                    HasInterceptorInSomeMethod = methodInterceptors.Any(),
                    MethodInterceptors = methodInterceptors
                };
                _cachedTypes.Add(type, interceptedTypeData);
                return true;
            }
            interceptedTypeData = default;
            return false;
        }

        internal sealed class InterceptedTypeData
        {
            public bool HasInterceptorInTypeDefinition { get; init; }
            public IEnumerable<CustomAttributeData> TypeDefinitionInterceptors { get; init; }
            public bool HasInterceptorInSomeMethod { get; init; }
            public IReadOnlyDictionary<MethodInfo, IEnumerable<CustomAttributeData>> MethodInterceptors { get; init; }

            public bool IsIntercepted => HasInterceptorInTypeDefinition || HasInterceptorInSomeMethod;

            public IEnumerable<CustomAttributeData> AllInterceptors => TypeDefinitionInterceptors?.Concat(MethodInterceptors?.Values?.SelectMany(x => x))?.Distinct() ?? Enumerable.Empty<CustomAttributeData>();

            public bool TryGetMethodInterceptors(MethodInfo methodInfo, out IEnumerable<CustomAttributeData> interceptors)
            {
                var interceptorList = new List<CustomAttributeData>();
                if (HasInterceptorInTypeDefinition)
                    interceptorList.AddRange(TypeDefinitionInterceptors);
                if (HasInterceptorInSomeMethod &&
                    MethodInterceptors.TryGetValue(methodInfo, out var methodInterceptors))
                    interceptorList.AddRange(methodInterceptors);
                interceptors = interceptorList;
                return interceptorList.Count > 0;
            }
        }
    }
}
