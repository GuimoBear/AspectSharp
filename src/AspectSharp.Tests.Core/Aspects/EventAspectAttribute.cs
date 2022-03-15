using AspectSharp.Abstractions;
using AspectSharp.Abstractions.Attributes;
using System;
using System.Threading.Tasks;

namespace AspectSharp.Tests.Core.Aspects
{
    [AttributeUsage(AttributeTargets.Event, AllowMultiple = false)]
    public sealed class EventAspectAttribute : AbstractInterceptorAttribute
    {
        private static readonly Random _random = new Random();

        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            Console.WriteLine("{0}: {1}.{2}", nameof(EventAspectAttribute), context.TargetType.Name, context.TargetMethod.Name);
            context.AdditionalData.Add(nameof(EventAspectAttribute), _random.Next());
            await next(context);
        }
    }
}
