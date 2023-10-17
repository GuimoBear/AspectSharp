using AspectSharp.Abstractions;
using AspectSharp.DynamicProxy;
using AspectSharp.DynamicProxy.Factories;
using AspectSharp.DynamicProxy.Utils;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace AspectSharp.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAspects(this IServiceCollection services, Action<IDynamicProxyFactoryConfigurationsBuilder> configure = default)
        {
            var builder = new DynamicProxyFactoryConfigurationsBuilder();
            if (!(configure is null))
                configure(builder);

            var configs = builder.Build();
            InterceptorTypeCache.SetConfigurations(configs);

            var serviceProvider = services.BuildServiceProvider(true);
            foreach (var sd in services.ToList())
            {
                if (InterceptorTypeCache.TryGetInterceptedTypeData(sd.ServiceType, out var interceptorTypeCache))
                {
                    ServiceDescriptor targetServiceDescriptor = default;
                    var targetType = sd.ImplementationType;
                    if (targetType is null && !(sd.ImplementationInstance is null))
                    {
                        var instance = sd.ImplementationInstance;
                        targetType = instance.GetType();
                        targetServiceDescriptor = new ServiceDescriptor(targetType, instance);
                    }
                    else if (!(sd.ImplementationFactory is null))
                    {
                        var instance = sd.ImplementationFactory(serviceProvider);
                        if (!(instance is null))
                        {
                            targetType = instance.GetType();
                            targetServiceDescriptor = new ServiceDescriptor(targetType, sd.ImplementationFactory, sd.Lifetime);
                        }
                        if (instance is IDisposable disposable)
                            try { disposable.Dispose(); } catch { }
                        else if (instance is IAsyncDisposable asyncDisposable)
                            try { asyncDisposable.DisposeAsync().GetAwaiter().GetResult(); } catch { }
                    }
                    else
                        targetServiceDescriptor = new ServiceDescriptor(targetType, targetType, sd.Lifetime);
                    if (!(targetType is null))
                    {
                        try
                        {
                            var proxyType = DynamicProxyFactory.Create(sd.ServiceType, targetType, interceptorTypeCache, configs);
                            services.Remove(sd);
                            services.Add(targetServiceDescriptor);
                            services.Add(new ServiceDescriptor(sd.ServiceType, proxyType, sd.Lifetime));
                        }
                        catch
                        {

                        }
                    }

                }
            };
            services.AddTransient<IAspectContextFactory, AspectContextFactory>();
            return services;
        }
    }
}
