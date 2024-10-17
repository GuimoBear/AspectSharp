using AspectCore.Api.Trace.ValueObjects;
using AspectSharp.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

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
            try
            {
                using (traceContext.Data.Start())
                {
                    await next(context);
                }
                _logger.LogWarning(traceContext.Data.ToString(0));
                //Console.WriteLine(traceContext.Data.ToString(0));
                //JsonSerializer.Serialize(traceContext.Data, new JsonSerializerOptions { WriteIndented = true }))
                //Task.Factory.StartNew(() => Console.WriteLine(traceContext.Data.ToString(0))/* _logger.LogInformation(traceContext.Data.ToString(0))*/);
            }
            catch
            {
                _logger.LogWarning(traceContext.Data.ToString(0));
                //Console.WriteLine(traceContext.Data.ToString(0));
                //Task.Factory.StartNew(() => Console.WriteLine(traceContext.Data.ToString(0)));
                throw;
            }
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
