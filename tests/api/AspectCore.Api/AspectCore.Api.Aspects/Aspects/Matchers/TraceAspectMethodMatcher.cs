//using AspectCore.Api.Domain.Responses;
//using AspectCore.Api.Infrastructure.Repositories;
//using AspectCore.Api.Utils;
using AspectSharp.Abstractions.Global;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AspectCore.Api.Trace.Aspects.Matchers
{
    public sealed class TraceAspectMethodMatcher : IInterceptorMethodMatcher
    {
        public static readonly TraceAspectMethodMatcher Instance = new TraceAspectMethodMatcher();

        private TraceAspectMethodMatcher() { }

        public bool Interceptable(MethodInfo methodInfo)
            => methodInfo.DeclaringType.Namespace.StartsWith("Lifecycle.Core");
        //{
        //    if (methodInfo.DeclaringType.Namespace.StartsWith("Lifecycle.Core.Workflows") ||
        //        methodInfo.DeclaringType.Namespace.StartsWith("Lifecycle.Core.Services"))
        //    {
        //        return true;
        //    }
        //    return false;
        //}

        private static readonly IEnumerable<Assembly> _assemblesToIntercept = new HashSet<Assembly>(new List<Assembly>
        {
            //typeof(WeatherForecast).Assembly,
            //typeof(CityRepository).Assembly,
            //typeof(Delay).Assembly

        });
    }
}
