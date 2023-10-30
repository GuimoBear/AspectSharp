using AspectSharp.Abstractions;
using AspectSharp.Abstractions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectSharp.DynamicProxy.Utils
{
    internal static class ProxyFactoryUtils
    {
        public static AspectDelegate NoOp = _ => Task.CompletedTask;

        public static readonly object[] EmptyParameters = Array.Empty<object>();

        public static InterceptDelegate FirstDelegate(InterceptDelegate current, AspectDelegate next)
            => (ctx, _) => current.Invoke(ctx, next);

        public static InterceptDelegate Encapsulate(InterceptDelegate current, InterceptDelegate inner)
            => (ctx, next) => current.Invoke(ctx, _ctx => inner(_ctx, next));

        public static InterceptDelegate CreatePipeline(AspectDelegate root, IInterceptor[] interceptorAttributes)
        {
            if (interceptorAttributes.Length == 0)
                return (ctx, _) => root(ctx);
            var stack = new Stack<InterceptDelegate>(interceptorAttributes.Select(x => (InterceptDelegate)x.Invoke));
            var ret = FirstDelegate(stack.Pop(), root);
            while (stack.Any())
                ret = Encapsulate(stack.Pop(), ret);
            return ret;
        }

        public static Task ExecutePipeline(AspectContext context, InterceptDelegate pipeline)
            => pipeline(context, NoOp);

        public static async Task<TResult> ExecutePipelineAndReturnResult<TResult>(AspectContext context, InterceptDelegate pipeline)
        {
            await ExecutePipeline(context, pipeline);
            return (TResult)context.ReturnValue;
        }

        public static AspectContextActivator NewContextActivator(Type serviceType, Type proxyType, Type targetType, string methodName, Type[] parameterTypes = default, string[] parameterNames = default, bool[] parametersIsRefOrOut = default)
        {
            var serviceMethod = serviceType.GetMethodExactly(methodName, parameterTypes ?? Array.Empty<Type>(), parameterNames ?? Array.Empty<string>(), parametersIsRefOrOut ?? Array.Empty<bool>());
            var proxyMethod = proxyType.GetMethodExactly(methodName, parameterTypes ?? Array.Empty<Type>(), parameterNames ?? Array.Empty<string>(), parametersIsRefOrOut ?? Array.Empty<bool>());
            var targetMethod = targetType.GetMethodExactly(methodName, parameterTypes ?? Array.Empty<Type>(), parameterNames ?? Array.Empty<string>(), parametersIsRefOrOut ?? Array.Empty<bool>());

            return new AspectContextActivator(serviceType, serviceMethod, proxyType, proxyMethod, targetType, targetMethod);
        }

        public static AspectContextActivator NewContextActivatorUsingStringRepresentationMethodInfo(Type serviceType, Type proxyType, Type targetType, string methodStringRepresentation)
        {
            var serviceMethod = serviceType.GetMethodByStringRepresentation(methodStringRepresentation);
            var proxyMethod = proxyType.GetMethodByStringRepresentation(methodStringRepresentation);
            var targetMethod = targetType.GetMethodByStringRepresentation(methodStringRepresentation);

            return new AspectContextActivator(serviceType, serviceMethod, proxyType, proxyMethod, targetType, targetMethod);
        }

        public static IInterceptor[] GetInterceptors(Type serviceType, int previouslyUsedConfigurationsHashCode, int methodHashCode)
        {
            var serviceName = serviceType.Name;

            if (!InterceptorTypeCache.TryGetInterceptedTypeData(serviceType, previouslyUsedConfigurationsHashCode, out var interceptedTypeData))
                return Array.Empty<AbstractInterceptorAttribute>();

            var methodInfo = serviceType.GetMethodsRecursively().FirstOrDefault(mi => mi.GetHashCode() == methodHashCode);
            if (methodInfo is null)
                return Array.Empty<AbstractInterceptorAttribute>();

            var hasInterceptors = interceptedTypeData.TryGetMethodInterceptorAttributes(methodInfo, out var interceptors);
            var hasGlobalInterceptors = interceptedTypeData.TryGetMethodGlobalInterceptors(methodInfo, out var globalInterceptors);
            if (!hasInterceptors && !hasGlobalInterceptors)
                return Array.Empty<AbstractInterceptorAttribute>();

            var typeDefinitionAttributes = new List<Tuple<CustomAttributeData, AbstractInterceptorAttribute>>();
            foreach(var interceptorInstance in serviceType.GetCustomAttributes(true).Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.GetType())))
            {
                var interceptorData = serviceType.CustomAttributes.FirstOrDefault(attr => attr.AttributeType == interceptorInstance.GetType());
                if (!(interceptorData is null))
                    typeDefinitionAttributes.Add(new Tuple<CustomAttributeData, AbstractInterceptorAttribute>(interceptorData, interceptorInstance as AbstractInterceptorAttribute));
            }
            var eventInfo = serviceType
                .GetEventsRecursively()
                .FirstOrDefault(evt => evt.GetAddMethod() == methodInfo ||
                                       evt.GetRemoveMethod() == methodInfo);
            var propertyInfo = serviceType
                .GetPropertiesRecursively()
                .FirstOrDefault(pi => pi.GetGetMethod() == methodInfo ||
                                      pi.GetSetMethod() == methodInfo);
            var methodAttributes = new List<Tuple<CustomAttributeData, AbstractInterceptorAttribute>>();
            if (!(eventInfo is null))
            {
                foreach (var interceptorInstance in eventInfo.GetCustomAttributes(true).Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.GetType())))
                {
                    var interceptorData = eventInfo.CustomAttributes.FirstOrDefault(attr => attr.AttributeType == interceptorInstance.GetType());
                    if (!(interceptorData is null))
                        methodAttributes.Add(new Tuple<CustomAttributeData, AbstractInterceptorAttribute>(interceptorData, interceptorInstance as AbstractInterceptorAttribute));
                }
            }
            if (!(propertyInfo is null))
            {
                foreach (var interceptorInstance in propertyInfo.GetCustomAttributes(true).Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.GetType())))
                {
                    var interceptorData = propertyInfo.CustomAttributes.FirstOrDefault(attr => attr.AttributeType == interceptorInstance.GetType());
                    if (!(interceptorData is null))
                        methodAttributes.Add(new Tuple<CustomAttributeData, AbstractInterceptorAttribute>(interceptorData, interceptorInstance as AbstractInterceptorAttribute));
                }
            }
            foreach (var interceptorInstance in methodInfo.GetCustomAttributes(true).Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.GetType())))
            {
                var interceptorData = methodInfo.CustomAttributes.FirstOrDefault(attr => attr.AttributeType == interceptorInstance.GetType());
                if (!(interceptorData is null))
                    methodAttributes.Add(new Tuple<CustomAttributeData, AbstractInterceptorAttribute>(interceptorData, interceptorInstance as AbstractInterceptorAttribute));
            }
            var ret = new List<IInterceptor>();

            if (!(globalInterceptors is null))
                ret.AddRange(globalInterceptors);

            foreach (var tuple in typeDefinitionAttributes.Concat(methodAttributes))
            {
                var data = tuple.Item1;
                var instance = tuple.Item2;

                if (interceptors?.Any(attr => IsEquals(attr, data)) ?? false)
                    ret.Add(instance);
            }
            return ret.ToArray();
        }

        private static bool IsEquals(CustomAttributeData left, CustomAttributeData right)
        {
            if (left.Constructor != right.Constructor)
                return false;

            foreach(var tuple in left.ConstructorArguments.Zip(right.ConstructorArguments, (l, r) => new Tuple<CustomAttributeTypedArgument, CustomAttributeTypedArgument>(l, r)))
            {
                if (tuple.Item1 != tuple.Item2)
                    return false;
            }

            foreach (var tuple in left.NamedArguments.Zip(right.NamedArguments, (l, r) => new Tuple<CustomAttributeNamedArgument, CustomAttributeNamedArgument>(l, r)))
            {
                if (tuple.Item1 != tuple.Item2)
                    return false;
            }

            return true;
        }
    }
}
