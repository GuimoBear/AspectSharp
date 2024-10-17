using System;
using System.Reflection;

namespace AspectSharp.Abstractions.Global
{
    public interface IInterceptorConfig
    {
        IInterceptorMethodMatcherBuilder AllMethods { get; }

        void WithMatcher<TInterceptorMethodMatcher>()
            where TInterceptorMethodMatcher : IInterceptorMethodMatcher, new();

        void WithMatcher<TInterceptorMethodMatcher>(TInterceptorMethodMatcher interceptorMatcher)
            where TInterceptorMethodMatcher : class, IInterceptorMethodMatcher;
    }

    public interface IInterceptorMethodMatcherBuilder
    {
        IInterceptorMethodAssemblyMatcherBuilder WithAssembly();
        IInterceptorMethodNamespaceMatcherBuilder WithNamespace();
        IInterceptorMethodInterfaceMatcherBuilder WithInterface();
        IInterceptorMethodNameMatcherBuilder WithName();
    }

    public interface IInterceptorMethodMatcherLogicalOperatorsBuilder
    {
        IInterceptorMethodMatcherBuilder And();
    }

    public interface IInterceptorMethodAssemblyMatcherBuilder
    {
        IInterceptorMethodMatcherLogicalOperatorsBuilder EqualTo(Assembly assembly);
        IInterceptorMethodMatcherLogicalOperatorsBuilder Like(Predicate<Assembly> predicate);

        IInterceptorMethodMatcherLogicalOperatorsBuilder NotEqualsTo(Assembly assembly);
        IInterceptorMethodMatcherLogicalOperatorsBuilder NotLike(Predicate<Assembly> predicate);
    }

    public interface IInterceptorMethodNamespaceMatcherBuilder
    {
        IInterceptorMethodMatcherLogicalOperatorsBuilder EqualTo(string @namespace);
        IInterceptorMethodMatcherLogicalOperatorsBuilder Like(Predicate<string> predicate);

        IInterceptorMethodMatcherLogicalOperatorsBuilder NotEqualsTo(string @namespace);
        IInterceptorMethodMatcherLogicalOperatorsBuilder NotLike(Predicate<string> predicate);
    }

    public interface IInterceptorMethodInterfaceMatcherBuilder
    {
        IInterceptorMethodMatcherLogicalOperatorsBuilder EqualTo(Type @interface);
        IInterceptorMethodMatcherLogicalOperatorsBuilder Like(Predicate<Type> predicate);

        IInterceptorMethodMatcherLogicalOperatorsBuilder NotEqualsTo(Type @interface);
        IInterceptorMethodMatcherLogicalOperatorsBuilder NotLike(Predicate<Type> predicate);
    }

    public interface IInterceptorMethodNameMatcherBuilder
    {
        IInterceptorMethodMatcherLogicalOperatorsBuilder EqualTo(string name);
        IInterceptorMethodMatcherLogicalOperatorsBuilder Like(Predicate<string> predicate);

        IInterceptorMethodMatcherLogicalOperatorsBuilder NotEqualsTo(string name);
        IInterceptorMethodMatcherLogicalOperatorsBuilder NotLike(Predicate<string> predicate);
    }
}
