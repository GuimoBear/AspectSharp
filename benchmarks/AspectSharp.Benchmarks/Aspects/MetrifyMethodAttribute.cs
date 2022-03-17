using AspectCore.DynamicProxy;
using AspectSharp.Benchmarks.Clients.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace AspectSharp.Benchmarks.Aspects
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class AspectCoreMetrifyMethodAttribute : AbstractInterceptorAttribute
    {
        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            var metricName = string.Format("Method {0}.{1}", context.ServiceMethod.DeclaringType?.FullName, context.ServiceMethod.Name);
            using (var scope = context.ServiceProvider.GetRequiredService<IMetricsClient>().NewScope(metricName))
            {
                await next(context);
                return;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class AspectSharpMetrifyMethodAttribute : Abstractions.Attributes.AbstractInterceptorAttribute
    {
        public override async Task Invoke(Abstractions.AspectContext context, Abstractions.AspectDelegate next)
        {
            var metricName = string.Format("Method {0}.{1}", context.ServiceMethod.DeclaringType?.FullName, context.ServiceMethod.Name);
            using (var scope = context.ServiceProvider.GetRequiredService<IMetricsClient>().NewScope(metricName))
            {
                await next(context);
                return;
            }
        }
    }
}
