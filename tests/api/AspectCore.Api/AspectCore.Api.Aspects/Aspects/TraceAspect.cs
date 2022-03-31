using AspectCore.Api.Trace.ValueObjects;
using AspectSharp.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Api.Trace.Aspects
{
    public sealed class TraceAspect : IInterceptor
    {
        public static readonly TraceAspect Instance = new TraceAspect();

        private TraceAspect() { }

        public async Task Invoke(AspectContext context, AspectDelegate next)
        {
            var traceContext = context.ServiceProvider.GetRequiredService<TraceContext>();
            var current = new TraceItem(context);
            traceContext.Data.AddChildren(current);
            var parent = traceContext.Data;
            traceContext.Data = current;

            using (current.Start())
            {
                await next(context);
            }
            traceContext.Data = parent;
        }
    }
}
