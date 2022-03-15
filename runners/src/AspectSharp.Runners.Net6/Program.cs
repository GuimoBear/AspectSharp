using AspectSharp.Abstractions;
using AspectSharp.Abstractions.Enums;
using AspectSharp.DynamicProxy;
using AspectSharp.DynamicProxy.Factories;
using AspectSharp.DynamicProxy.Utils;
using AspectSharp.Tests.Core.Aspects;
using AspectSharp.Tests.Core.Services;
using AspectSharp.Tests.Core.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

Type proxyType = default;
if (InterceptorTypeCache.TryGetInterceptedTypeData(typeof(IFakeService), out var interceptedTypeData))
{
    var configs = new DynamicProxyFactoryConfigurations(InterceptedEventMethod.None, InterceptedPropertyMethod.Set, false);

    proxyType = DynamicProxyFactory.Create(typeof(IFakeService), typeof(FakeService), interceptedTypeData, configs);
}

IServiceCollection services = new ServiceCollection();
services
    .AddLogging(logging => logging.AddConsole().SetMinimumLevel(LogLevel.Debug))
    .AddTransient<IAspectContextFactory, AspectContextFactory>()
    .AddSingleton<Aspect1Attribute>()
    .AddSingleton<Aspect2Attribute>()
    .AddSingleton<Aspect3Attribute>()
    .AddScoped<FakeService>()
    .AddScoped(typeof(IFakeService), proxyType);

using var provider = services.BuildServiceProvider();
using var scope = provider.CreateScope();

var target = scope.ServiceProvider.GetService<FakeService>();
var service = scope.ServiceProvider.GetRequiredService<IFakeService>();

var y = service.DoSomethingWithoutParameterAndReferenceTypeReturn();
var x = service.DoSomethingWithoutParameterAndValueTypeReturn();
var t2 = await service.DoSomethingAsyncWithParameterAndReferenceTypeReturn(30, "225", service.ReferenceTypeProperty);

Console.WriteLine(service.ValueTypeProperty);
service.ValueTypeProperty = 30;
service.ReferenceTypeProperty = Enumerable.Empty<string>();
Console.WriteLine(service.ReferenceTypeProperty);

service.InterceptedDoSomethingWithoutParameterAndWithoutReturn();
service.InterceptedDoSomethingWithParameterAndWithoutReturn(30, "225", service.ReferenceTypeProperty);

var n1 = service.InterceptedDoSomethingWithoutParameterAndValueTypeReturn();
var n2 = service.InterceptedDoSomethingWithParameterAndValueTypeReturn(30, "225", service.ReferenceTypeProperty);

await service.InterceptedDoSomethingAsyncWithoutParameterAndWithoutReturn();

await service.InterceptedDoSomethingValueAsyncWithoutParameterAndWithoutReturn();
var fft = await service.InterceptedDoSomethingAsyncWithoutParameterAndReferenceypeReturn();
fft = await service.InterceptedDoSomethingValueAsyncWithoutParameterAndReferenceTypeReturn();

var t = service.InterceptedDoSomethingWithoutParameterAndReferenceTypeReturn();
t2 = await service.InterceptedDoSomethingAsyncWithParameterAndReferenceTypeReturn(30, "225", service.ReferenceTypeProperty);
var t3 = await service.InterceptedDoSomethingValueAsyncWithParameterAndReferenceTypeReturn(30, "225", service.ReferenceTypeProperty);

var v = service.InterceptedDoSomethingWithoutParameterAndValueTypeReturn();
var v2 = await service.InterceptedDoSomethingAsyncWithParameterAndValueTypeReturn(30, "225", service.ReferenceTypeProperty);
var v3 = await service.InterceptedDoSomethingValueAsyncWithParameterAndValueTypeReturn(30, "225", service.ReferenceTypeProperty);





service.DoSomethingWithoutParameterAndWithoutReturn();
service.DoSomethingWithParameterAndWithoutReturn(30, "225", service.ReferenceTypeProperty);

n1 = service.DoSomethingWithoutParameterAndValueTypeReturn();
n2 = service.DoSomethingWithParameterAndValueTypeReturn(30, "225", service.ReferenceTypeProperty);

await service.DoSomethingAsyncWithoutParameterAndWithoutReturn();

await service.DoSomethingValueAsyncWithoutParameterAndWithoutReturn();
fft = await service.DoSomethingAsyncWithoutParameterAndReferenceypeReturn();
fft = await service.DoSomethingValueAsyncWithoutParameterAndReferenceTypeReturn();

t = service.DoSomethingWithoutParameterAndReferenceTypeReturn();
t2 = await service.DoSomethingAsyncWithParameterAndReferenceTypeReturn(30, "225", service.ReferenceTypeProperty);
t3 = await service.DoSomethingValueAsyncWithParameterAndReferenceTypeReturn(30, "225", service.ReferenceTypeProperty);

v = service.DoSomethingWithoutParameterAndValueTypeReturn();
v2 = await service.DoSomethingAsyncWithParameterAndValueTypeReturn(30, "225", service.ReferenceTypeProperty);
v3 = await service.DoSomethingValueAsyncWithParameterAndValueTypeReturn(30, "225", service.ReferenceTypeProperty);









Console.ReadKey();