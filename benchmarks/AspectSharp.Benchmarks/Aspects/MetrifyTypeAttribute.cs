using AspectCore.DynamicProxy;
using AspectSharp.Benchmarks.Clients.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace AspectSharp.Benchmarks.Aspects
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public sealed class AspectCoreMetrifyTypeAttribute : AbstractInterceptorAttribute
    {
        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            var metricName = string.Format("Class {0}.{1}", context.ServiceMethod.DeclaringType?.FullName, context.ServiceMethod.Name);
            using (var scope = context.ServiceProvider.GetRequiredService<IMetricsClient>().NewScope(metricName))
            {
                await next(context);
                return;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public sealed class AspectSharpMetrifyTypeAttribute : Abstractions.Attributes.AbstractInterceptorAttribute
    {
        public override async Task Invoke(Abstractions.AspectContext context, Abstractions.AspectDelegate next)
        {
            var metricName = string.Format("Class {0}.{1}", context.ServiceMethod.DeclaringType?.FullName, context.ServiceMethod.Name);
            using (var scope = context.ServiceProvider.GetRequiredService<IMetricsClient>().NewScope(metricName))
            {
                await next(context);
                return;
            }
        }
    }
}
