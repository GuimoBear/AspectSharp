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
        public static AspectDelegate EmptyAspectDelegate = _ => Task.CompletedTask;

        public static object[] EmptyParameters = Array.Empty<object>();

        public static InterceptDelegate FirstDelegate(InterceptDelegate current, AspectDelegate next)
            => (ctx, _) => current.Invoke(ctx, next);

        public static InterceptDelegate Encapsulate(InterceptDelegate current, InterceptDelegate inner)
            => (ctx, next) => current.Invoke(ctx, _ctx => inner(_ctx, next));

        public static InterceptDelegate CreatePipeline(AspectDelegate root, AbstractInterceptorAttribute[] interceptorAttributes)
        {
            if (interceptorAttributes.Length == 0)
                return (ctx, _) => root(ctx);
            var stack = new Stack<InterceptDelegate>(interceptorAttributes.Select(x => (InterceptDelegate)x.Invoke));
            var ret = FirstDelegate(stack.Pop(), root);
            while (stack.Any())
                ret = Encapsulate(stack.Pop(), ret);
            return ret;
        }

        public static async Task ExecutePipeline(AspectContext context, InterceptDelegate pipeline)
            => await pipeline(context, EmptyAspectDelegate);

        public static AspectContextActivator NewContextActivator(Type serviceType, Type proxyType, Type targetType, string methodName, Type[] parameterTypes = default)
        {
            var serviceMethod = serviceType.GetMethod(methodName, parameterTypes ?? Array.Empty<Type>());
            var proxyMethod = proxyType.GetMethod(methodName, parameterTypes ?? Array.Empty<Type>());
            var targetMethod = targetType.GetMethod(methodName, parameterTypes ?? Array.Empty<Type>());

            return new AspectContextActivator(serviceType, serviceMethod, proxyType, proxyMethod, targetType, targetMethod);
        }

        public static AbstractInterceptorAttribute[] GetInterceptors(Type serviceType, int previouslyUsedConfigurationsHashCode, int methodHashCode)
        {
            if (!InterceptorTypeCache.TryGetInterceptedTypeData(serviceType, previouslyUsedConfigurationsHashCode, out var interceptedTypeData))
                return Array.Empty<AbstractInterceptorAttribute>();

            var methodInfo = serviceType.GetMethods().FirstOrDefault(mi => mi.GetHashCode() == methodHashCode);
            if (methodInfo is null || 
                !interceptedTypeData.TryGetMethodInterceptors(methodInfo, out var interceptors))
                return Array.Empty<AbstractInterceptorAttribute>();

            var typeDefinitionAttributes = new List<Tuple<CustomAttributeData, AbstractInterceptorAttribute>>();
            foreach(var interceptorInstance in serviceType.GetCustomAttributes(true).Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.GetType())))
            {
                var interceptorData = serviceType.CustomAttributes.FirstOrDefault(attr => attr.AttributeType == interceptorInstance.GetType());
                if (!(interceptorData is null))
                    typeDefinitionAttributes.Add(new Tuple<CustomAttributeData, AbstractInterceptorAttribute>(interceptorData, interceptorInstance as AbstractInterceptorAttribute));
            }
            var eventInfo = serviceType
                .GetEvents()
                .FirstOrDefault(evt => evt.GetAddMethod() == methodInfo ||
                                       evt.GetRemoveMethod() == methodInfo || 
                                       evt.GetRaiseMethod() == methodInfo);
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
            foreach (var interceptorInstance in methodInfo.GetCustomAttributes(true).Where(attr => typeof(AbstractInterceptorAttribute).IsAssignableFrom(attr.GetType())))
            {
                var interceptorData = methodInfo.CustomAttributes.FirstOrDefault(attr => attr.AttributeType == interceptorInstance.GetType());
                if (!(interceptorData is null))
                    methodAttributes.Add(new Tuple<CustomAttributeData, AbstractInterceptorAttribute>(interceptorData, interceptorInstance as AbstractInterceptorAttribute));
            }
            var ret = new List<AbstractInterceptorAttribute>();
            foreach(var tuple in typeDefinitionAttributes.Concat(methodAttributes))
            {
                var data = tuple.Item1;
                var instance = tuple.Item2;

                if (interceptors.Any(attr => IsEquals(attr, data)))
                    ret.Add(instance);
            }
            return ret.ToArray();

            //return interceptors.Select(attributeData => attributeData.CreateInstance() as AbstractInterceptorAttribute).ToArray();
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
