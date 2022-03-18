using AspectSharp.Abstractions;
using AspectSharp.Abstractions.Attributes;
using AspectSharp.Tests.Core.Enums;
using System;
using System.Threading.Tasks;

namespace AspectSharp.Tests.Core.Aspects
{
    [AttributeUsage(AttributeTargets.Event, AllowMultiple = false)]
    public sealed class EventAspectAttribute : AbstractInterceptorAttribute
    {
        private static readonly string _aspectName = nameof(EventAspectAttribute).PadRight(9);

        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            var key = string.Format("{0}: {1} {2}", _aspectName, InterceptMoment.Before.ToString().ToLower(), context.ServiceMethod.Name);
            int count = 1;
            if (context.AdditionalData.TryGetValue(key, out var value))
            {
                count += (int)value;
                context.AdditionalData[key] = count;
            }
            context.AdditionalData.Add(string.Format("{0}{1}: {2} {3}", _aspectName, (count == 1 ? string.Empty : string.Format(" {0}", count)), InterceptMoment.Before.ToString().ToLower(), context.ServiceMethod.Name), count);
            await next(context);
            context.AdditionalData.Add(string.Format("{0}{1}: {2} {3}", _aspectName, (count == 1 ? string.Empty : string.Format(" {0}", count)), InterceptMoment.After.ToString().ToLower(), context.ServiceMethod.Name), count);
        }
    }
}
