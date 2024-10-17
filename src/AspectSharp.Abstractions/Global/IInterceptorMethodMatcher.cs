using System.Reflection;

namespace AspectSharp.Abstractions.Global
{
    public interface IInterceptorMethodMatcher
    {
        bool Interceptable(MethodInfo methodInfo);
    }
}
