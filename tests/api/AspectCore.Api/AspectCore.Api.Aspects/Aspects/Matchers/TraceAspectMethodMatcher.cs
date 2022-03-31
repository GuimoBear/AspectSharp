using AspectCore.Api.Domain.Responses;
using AspectCore.Api.Infrastructure.Repositories;
using AspectCore.Api.Utils;
using AspectSharp.Abstractions.Global;
using System.Reflection;

namespace AspectCore.Api.Trace.Aspects.Matchers
{
    public sealed class TraceAspectMethodMatcher : IInterceptorMethodMatcher
    {
        public static readonly TraceAspectMethodMatcher Instance = new TraceAspectMethodMatcher();

        private TraceAspectMethodMatcher() { }

        public bool Interceptable(MethodInfo methodInfo)
            => _assemblesToIntercept.Contains(methodInfo.DeclaringType?.Assembly);

        private static readonly IEnumerable<Assembly> _assemblesToIntercept = new List<Assembly>
        {
            typeof(WeatherForecast).Assembly,
            typeof(CityRepository).Assembly,
            typeof(Delay).Assembly
        };
    }
}
