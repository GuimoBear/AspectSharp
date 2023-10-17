using AspectSharp.Abstractions;
using AspectSharp.Abstractions.Global;
using AspectSharp.Tests.Core.Enums;
using AspectSharp.Tests.Core.Services.Interfaces;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectSharp.Tests.Core.Aspects
{
    public class GlobalAspect : IInterceptor
    {
        private static readonly string _aspectName = nameof(GlobalAspect);

        public async Task Invoke(AspectContext context, AspectDelegate next)
        {
            var methodName = context.ServiceMethod.Name;
            var key = string.Format("{0}: {1} {2}", _aspectName, InterceptMoment.Before.ToString().ToLower(), methodName);
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

    public class GlobalAspectMethodMatcher : IInterceptorMethodMatcher
    {
        public bool Interceptable(MethodInfo methodInfo)
            => methodInfo.DeclaringType == typeof(IFakeService);
    }
}
