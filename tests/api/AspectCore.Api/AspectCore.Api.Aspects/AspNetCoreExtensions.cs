using AspectCore.Api.Trace.Aspects;
using AspectCore.Api.Trace.Aspects.Matchers;
using AspectCore.Api.Trace.Middlewares;
using AspectCore.Api.Trace.ValueObjects;
using AspectSharp.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Api.Trace
{
    public static class AspNetCoreExtensions
    {
        public static IServiceCollection AddTrace(this IServiceCollection services)
        {
            services
                .AddSingleton<TraceInitializerMiddleware>()
                .AddScoped<TraceContext>()
                .AddAspects(configs =>
                {
                    configs.WithInterceptor(TraceAspect.Instance, interceptorConfig => interceptorConfig.WithMatcher(TraceAspectMethodMatcher.Instance));
                });
            return services;
        }

        public static IApplicationBuilder UseTrace(this IApplicationBuilder applicationBuilder)
            => applicationBuilder.UseMiddleware<TraceInitializerMiddleware>();
    }
}
