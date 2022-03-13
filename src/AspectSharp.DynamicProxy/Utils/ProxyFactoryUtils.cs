using AspectSharp.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectSharp.DynamicProxy.Utils
{
    public static class ProxyFactoryUtils
    {
        public static AspectDelegate EmptyAspectDelegate = _ => Task.CompletedTask;

        public static object[] EmptyParameters = Array.Empty<object>();

        public static AspectDelegate DelegateFromAction(Action action)
        {
            return _ =>
            {
                action();
                return Task.CompletedTask;
            };
        }

        public static AspectDelegate DelegateFromAsyncAction(Func<Task> asyncAction)
            => async _ => await asyncAction();

        public static AspectDelegate DelegateFromAsyncAction(Func<ValueTask> asyncAction)
            => async _ => await asyncAction();

        public static AspectDelegate DelegateFromFunction<TReturn>(Func<TReturn> function)
        {
            return ctx =>
            {
                ctx.ReturnValue = function();
                return Task.CompletedTask;
            };
        }

        public static AspectDelegate DelegateFromAsyncFunction<TReturn>(Func<Task<TReturn>> asyncFunction)
            => async ctx => ctx.ReturnValue = await asyncFunction();

        public static AspectDelegate DelegateFromAsyncFunction<TReturn>(Func<ValueTask<TReturn>> asyncFunction)
            => async ctx => ctx.ReturnValue = await asyncFunction();

        public static InterceptDelegate FirstDelegate(InterceptDelegate current, AspectDelegate next)
            => (ctx, _) => current.Invoke(ctx, next);

        public static InterceptDelegate Encapsulate(InterceptDelegate current, InterceptDelegate inner)
            => (ctx, next) => current.Invoke(ctx, _ctx => inner(_ctx, next));

        public static InterceptDelegate CreatePipeline(AspectDelegate root, AbstractInterceptorAttribute[] interceptorAttributes)
        {
            var stack = new Stack<InterceptDelegate>(interceptorAttributes.Select(x => (InterceptDelegate)x.Invoke).Reverse());
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
    }
}
