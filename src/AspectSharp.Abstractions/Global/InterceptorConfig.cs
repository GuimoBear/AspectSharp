using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AspectSharp.Abstractions.Global
{
    internal sealed class InterceptorConfig<TInterceptor> 
        : IInterceptorConfig,
          IInterceptorMethodMatcher,
          IInterceptorMethodMatcherBuilder, 
          IInterceptorMethodMatcherLogicalOperatorsBuilder
        where TInterceptor : class, IInterceptor
    {
        public static readonly IInterceptorMethodMatcher AllowAllMathods = new AllMethodsInterceptorMethodMatcher();

        private readonly Lazy<ICollection<Predicate<MethodInfo>>> _predicates
            = new Lazy<ICollection<Predicate<MethodInfo>>>(() => new List<Predicate<MethodInfo>>());

        private IInterceptorMethodMatcher _matcher;

        private readonly TInterceptor _interceptor;

        public IInterceptorMethodMatcherBuilder AllMethods => this;

        public TInterceptor Interceptor => _interceptor;

        public InterceptorConfig(TInterceptor interceptor)
        {
            if (interceptor is null)
                throw new ArgumentNullException(nameof(interceptor));
            _interceptor = interceptor;
        }

        public void WithMatcher<TInterceptorMethodMatcher>() where TInterceptorMethodMatcher : IInterceptorMethodMatcher, new()
            => _matcher = new TInterceptorMethodMatcher();

        public void WithMatcher<TInterceptorMethodMatcher>(TInterceptorMethodMatcher interceptorMatcher) 
            where TInterceptorMethodMatcher : class, IInterceptorMethodMatcher
        {
            if (interceptorMatcher is null)
                throw new ArgumentNullException(nameof(interceptorMatcher));
            _matcher = interceptorMatcher;
        }

        public IInterceptorMethodAssemblyMatcherBuilder WithAssembly()
            => new InterceptorMethodPropertyMatcher(this);

        public IInterceptorMethodNamespaceMatcherBuilder WithNamespace()
            => new InterceptorMethodPropertyMatcher(this);

        public IInterceptorMethodInterfaceMatcherBuilder WithInterface()
            => new InterceptorMethodPropertyMatcher(this);

        public IInterceptorMethodNameMatcherBuilder WithName()
            => new InterceptorMethodPropertyMatcher(this);

        public IInterceptorMethodMatcherBuilder And()
            => this;

        public bool Interceptable(MethodInfo methodInfo)
        {
            if (!(_matcher is null))
                return _matcher.Interceptable(methodInfo);
            if (_predicates.IsValueCreated)
                return _predicates.Value.All(predicate => predicate(methodInfo));
            return true;
        }

        public override int GetHashCode()
        {
            if (!(_matcher is null))
                return _matcher.GetType().GetHashCode();
            if (_predicates.IsValueCreated)
                return _predicates.Value.Aggregate(default(int), (left, right) => left ^ right.GetHashCode());
            return base.GetHashCode();
        }

        internal sealed class AllMethodsInterceptorMethodMatcher : IInterceptorMethodMatcher
        {
            public bool Interceptable(MethodInfo methodInfo)
                => true;
        }

        internal sealed class InterceptorMethodPropertyMatcher 
            : IInterceptorMethodAssemblyMatcherBuilder,
              IInterceptorMethodNamespaceMatcherBuilder,
              IInterceptorMethodInterfaceMatcherBuilder,
              IInterceptorMethodNameMatcherBuilder
        {
            private readonly InterceptorConfig<TInterceptor> _parent;

            internal InterceptorMethodPropertyMatcher(InterceptorConfig<TInterceptor> parent)
                => _parent = parent;

            IInterceptorMethodMatcherLogicalOperatorsBuilder IInterceptorMethodAssemblyMatcherBuilder.EqualTo(Assembly assembly)
            {
                if (assembly is null)
                    throw new ArgumentNullException(nameof(assembly));
                _parent._predicates.Value.Add(mi => mi.DeclaringType.Assembly.Equals(assembly));
                return _parent;
            }

            IInterceptorMethodMatcherLogicalOperatorsBuilder IInterceptorMethodAssemblyMatcherBuilder.Like(Predicate<Assembly> predicate)
            {
                if (predicate is null)
                    throw new ArgumentNullException(nameof(predicate));
                _parent._predicates.Value.Add(mi => predicate(mi.DeclaringType.Assembly));
                return _parent;
            }

            IInterceptorMethodMatcherLogicalOperatorsBuilder IInterceptorMethodAssemblyMatcherBuilder.NotEqualsTo(Assembly assembly)
            {
                if (assembly is null)
                    throw new ArgumentNullException(nameof(assembly));
                _parent._predicates.Value.Add(mi => !mi.DeclaringType.Assembly.Equals(assembly));
                return _parent;
            }

            IInterceptorMethodMatcherLogicalOperatorsBuilder IInterceptorMethodAssemblyMatcherBuilder.NotLike(Predicate<Assembly> predicate)
            {
                if (predicate is null)
                    throw new ArgumentNullException(nameof(predicate));
                _parent._predicates.Value.Add(mi => !predicate(mi.DeclaringType.Assembly));
                return _parent;
            }

            IInterceptorMethodMatcherLogicalOperatorsBuilder IInterceptorMethodNamespaceMatcherBuilder.EqualTo(string @namespace)
            {
                if (@namespace is null)
                    throw new ArgumentNullException(nameof(@namespace));
                _parent._predicates.Value.Add(mi => mi.DeclaringType.Namespace.Equals(@namespace));
                return _parent;
            }

            IInterceptorMethodMatcherLogicalOperatorsBuilder IInterceptorMethodNamespaceMatcherBuilder.Like(Predicate<string> predicate)
            {
                if (predicate is null)
                    throw new ArgumentNullException(nameof(predicate));
                _parent._predicates.Value.Add(mi => predicate(mi.DeclaringType.Namespace));
                return _parent;
            }

            IInterceptorMethodMatcherLogicalOperatorsBuilder IInterceptorMethodNamespaceMatcherBuilder.NotEqualsTo(string @namespace)
            {
                if (@namespace is null)
                    throw new ArgumentNullException(nameof(@namespace));
                _parent._predicates.Value.Add(mi => !mi.DeclaringType.Namespace.Equals(@namespace));
                return _parent;
            }

            IInterceptorMethodMatcherLogicalOperatorsBuilder IInterceptorMethodNamespaceMatcherBuilder.NotLike(Predicate<string> predicate)
            {
                if (predicate is null)
                    throw new ArgumentNullException(nameof(predicate));
                _parent._predicates.Value.Add(mi => !predicate(mi.DeclaringType.Namespace));
                return _parent;
            }

            IInterceptorMethodMatcherLogicalOperatorsBuilder IInterceptorMethodInterfaceMatcherBuilder.EqualTo(Type @interface)
            {
                if (@interface is null)
                    throw new ArgumentNullException(nameof(@interface));
                if (!@interface.IsInterface)
                    throw new ArgumentException(string.Format("'{0}' is not an interface", @interface.FullName), nameof(@interface));
                _parent._predicates.Value.Add(mi => mi.DeclaringType.Equals(@interface));
                return _parent;
            }

            IInterceptorMethodMatcherLogicalOperatorsBuilder IInterceptorMethodInterfaceMatcherBuilder.Like(Predicate<Type> predicate)
            {
                if (predicate is null)
                    throw new ArgumentNullException(nameof(predicate));
                _parent._predicates.Value.Add(mi => predicate(mi.DeclaringType));
                return _parent;
            }

            IInterceptorMethodMatcherLogicalOperatorsBuilder IInterceptorMethodInterfaceMatcherBuilder.NotEqualsTo(Type @interface)
            {
                if (@interface is null)
                    throw new ArgumentNullException(nameof(@interface));
                if (!@interface.IsInterface)
                    throw new ArgumentException(string.Format("'{0}' is not an interface", @interface.FullName), nameof(@interface));
                _parent._predicates.Value.Add(mi => !mi.DeclaringType.Equals(@interface));
                return _parent;
            }

            IInterceptorMethodMatcherLogicalOperatorsBuilder IInterceptorMethodInterfaceMatcherBuilder.NotLike(Predicate<Type> predicate)
            {
                if (predicate is null)
                    throw new ArgumentNullException(nameof(predicate));
                _parent._predicates.Value.Add(mi => !predicate(mi.DeclaringType));
                return _parent;
            }



            IInterceptorMethodMatcherLogicalOperatorsBuilder IInterceptorMethodNameMatcherBuilder.EqualTo(string name)
            {
                if (name is null)
                    throw new ArgumentNullException(nameof(name));
                _parent._predicates.Value.Add(mi => mi.Name.Equals(name));
                return _parent;
            }

            IInterceptorMethodMatcherLogicalOperatorsBuilder IInterceptorMethodNameMatcherBuilder.Like(Predicate<string> predicate)
            {
                if (predicate is null)
                    throw new ArgumentNullException(nameof(predicate));
                _parent._predicates.Value.Add(mi => predicate(mi.Name));
                return _parent;
            }

            IInterceptorMethodMatcherLogicalOperatorsBuilder IInterceptorMethodNameMatcherBuilder.NotEqualsTo(string name)
            {
                if (name is null)
                    throw new ArgumentNullException(nameof(name));
                _parent._predicates.Value.Add(mi => !mi.Name.Equals(name));
                return _parent;
            }

            IInterceptorMethodMatcherLogicalOperatorsBuilder IInterceptorMethodNameMatcherBuilder.NotLike(Predicate<string> predicate)
            {
                if (predicate is null)
                    throw new ArgumentNullException(nameof(predicate));
                _parent._predicates.Value.Add(mi => !predicate(mi.Name));
                return _parent;
            }
        }
    }
}
