using AspectCore.Api.Trace.ValueObjects;
using AspectSharp.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.Json;

namespace AspectCore.Api.Trace.Middlewares
{
    public sealed class TraceInitializerMiddleware : IMiddleware
    {
        private readonly ILogger<TraceInitializerMiddleware> _logger;

        public TraceInitializerMiddleware(ILogger<TraceInitializerMiddleware> logger)
            => _logger = logger;

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var traceContext = context.RequestServices.GetRequiredService<TraceContext>();
            traceContext.Data = new TraceItem(new TraceAspectContext(this, context, next));
            using (traceContext.Data.Start())
            {
                await next(context);
            }
            //JsonSerializer.Serialize(traceContext.Data, new JsonSerializerOptions { WriteIndented = true }))
            Task.Factory.StartNew(() => _logger.LogInformation(traceContext.Data.ToString(0)));
        }

        private sealed class TraceAspectContext : AspectContext
        {
            public override MemberTypes MemberType { get; } = MemberTypes.Method;

            public override Type ServiceType { get; } = typeof(IMiddleware);

            public override MethodInfo ServiceMethod { get; } = typeof(IMiddleware).GetMethod(nameof(IMiddleware.InvokeAsync));

            public override Type ProxyType { get; } = typeof(TraceInitializerMiddleware);

            public override MethodInfo ProxyMethod { get; } = typeof(TraceInitializerMiddleware).GetMethod(nameof(TraceInitializerMiddleware.InvokeAsync));

            public override Type TargetType { get; } = typeof(TraceInitializerMiddleware);

            public override MethodInfo TargetMethod { get; } = typeof(TraceInitializerMiddleware).GetMethod(nameof(TraceInitializerMiddleware.InvokeAsync));

            public override object Proxy { get; }

            public override object Target { get; }

            public override object[] Parameters { get; }

            public override IServiceProvider ServiceProvider { get; }

            public override IDictionary<string, object> AdditionalData { get; }

            public override object ReturnValue { get; set; }

            public TraceAspectContext(TraceInitializerMiddleware instance, HttpContext context, RequestDelegate next)
            {
                Proxy = instance;
                Target = context;
                Parameters = new object[]
                {
                    context, 
                    next
                };
                ServiceProvider = context.RequestServices;
            }
        }
    }
}
