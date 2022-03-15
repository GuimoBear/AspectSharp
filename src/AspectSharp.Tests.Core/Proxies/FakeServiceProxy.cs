using AspectSharp.DynamicProxy;
using AspectSharp.DynamicProxy.Utils;
using AspectSharp.Tests.Core.Services;
using AspectSharp.Tests.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspectSharp.Tests.Core.Proxies
{
    public class FakeServiceProxy : IFakeService
    {
        private readonly IFakeService _target;
        private readonly IAspectContextFactory _contextFactory;

        public FakeServiceProxy(FakeService target, IAspectContextFactory contextFactory)
        {
            _target = target;
            _contextFactory = contextFactory;
        }

        public event EventHandler OnChanged
        {
            add => _target.OnChanged += value;
            remove => _target.OnChanged -= value;
        }

        public int ValueTypeProperty
        {
            get => _target.ValueTypeProperty;
            set => _target.ValueTypeProperty = value;
        }

        public IEnumerable<string> ReferenceTypeProperty
        {
            get => _target.ReferenceTypeProperty;
            set => _target.ReferenceTypeProperty = value;
        }

        public void DoSomethingWithoutParameterAndWithoutReturn()
            => _target.DoSomethingWithoutParameterAndWithoutReturn();

        public void InterceptedDoSomethingWithoutParameterAndWithoutReturn()
        {
            var context = _contextFactory.CreateContext(_aspectContextAtivator1, _target, this, ProxyFactoryUtils.EmptyParameters);
            ProxyFactoryUtils.ExecutePipeline(context, _pipelines.Pipeline1).Wait();
        }

        public void DoSomethingWithParameterAndWithoutReturn(int param1, string param2, IEnumerable<string> param3)
            => _target.DoSomethingWithParameterAndWithoutReturn(param1, param2, param3);

        public void InterceptedDoSomethingWithParameterAndWithoutReturn(int param1, string param2, IEnumerable<string> param3)
        {
            var parameters = new object[] { param1, param2, param3 };
            var context = _contextFactory.CreateContext(_aspectContextAtivator2, _target, this, parameters);
            ProxyFactoryUtils.ExecutePipeline(context, _pipelines.Pipeline2).Wait();
        }

        public Task DoSomethingAsyncWithoutParameterAndWithoutReturn()
            => _target.DoSomethingAsyncWithoutParameterAndWithoutReturn();

        public Task InterceptedDoSomethingAsyncWithoutParameterAndWithoutReturn()
        {
            var context = _contextFactory.CreateContext(_aspectContextAtivator3, _target, this, ProxyFactoryUtils.EmptyParameters);
            return ProxyFactoryUtils.ExecutePipeline(context, _pipelines.Pipeline3);
        }

        public ValueTask DoSomethingValueAsyncWithoutParameterAndWithoutReturn()
            => _target.DoSomethingValueAsyncWithoutParameterAndWithoutReturn();

        public ValueTask InterceptedDoSomethingValueAsyncWithoutParameterAndWithoutReturn()
        {
            var context = _contextFactory.CreateContext(_aspectContextAtivator4, _target, this, ProxyFactoryUtils.EmptyParameters);
            ProxyFactoryUtils.ExecutePipeline(context, _pipelines.Pipeline4).Wait();
            return ValueTask.CompletedTask;
        }

        public ValueTask DoSomethingValueAsyncWithParameterAndWithoutReturn(int param1, string param2, IEnumerable<string> param3)
            => _target.DoSomethingValueAsyncWithParameterAndWithoutReturn(param1, param2, param3);

        public ValueTask InterceptedDoSomethingValueAsyncWithParameterAndWithoutReturn(int param1, string param2, IEnumerable<string> param3)
        {
            var parameters = new object[] { param1, param2, param3 };
            var context = _contextFactory.CreateContext(_aspectContextAtivator5, _target, this, parameters);
            ProxyFactoryUtils.ExecutePipeline(context, _pipelines.Pipeline5).Wait();
            return ValueTask.CompletedTask;
        }

        public int DoSomethingWithoutParameterAndValueTypeReturn()
            => _target.DoSomethingWithoutParameterAndValueTypeReturn();

        public int InterceptedDoSomethingWithoutParameterAndValueTypeReturn()
        {
            var context = _contextFactory.CreateContext(_aspectContextAtivator6, _target, this, ProxyFactoryUtils.EmptyParameters);
            ProxyFactoryUtils.ExecutePipeline(context, _pipelines.Pipeline6).Wait();
            return (int)context.ReturnValue;
        }

        public int DoSomethingWithParameterAndValueTypeReturn(int param1, string param2, IEnumerable<string> param3)
            => _target.DoSomethingWithParameterAndValueTypeReturn(param1, param2, param3);

        public int InterceptedDoSomethingWithParameterAndValueTypeReturn(int param1, string param2, IEnumerable<string> param3)
        {
            var parameters = new object[] { param1, param2, param3 };
            var context = _contextFactory.CreateContext(_aspectContextAtivator7, _target, this, parameters);
            ProxyFactoryUtils.ExecutePipeline(context, _pipelines.Pipeline7).Wait();
            return (int)context.ReturnValue;
        }

        public IEnumerable<string> DoSomethingWithoutParameterAndReferenceTypeReturn()
            => _target.DoSomethingWithoutParameterAndReferenceTypeReturn();

        public IEnumerable<string> InterceptedDoSomethingWithoutParameterAndReferenceTypeReturn()
        {
            var context = _contextFactory.CreateContext(_aspectContextAtivator8, _target, this, ProxyFactoryUtils.EmptyParameters);
            ProxyFactoryUtils.ExecutePipeline(context, _pipelines.Pipeline8).Wait();
            return (IEnumerable<string>)context.ReturnValue;
        }

        public IEnumerable<string> DoSomethingWithParameterAndReferenceTypeReturn(int param1, string param2, IEnumerable<string> param3)
            => _target.DoSomethingWithParameterAndReferenceTypeReturn(param1, param2, param3);

        public IEnumerable<string> InterceptedDoSomethingWithParameterAndReferenceTypeReturn(int param1, string param2, IEnumerable<string> param3)
        {
            var parameters = new object[] { param1, param2, param3 };
            var context = _contextFactory.CreateContext(_aspectContextAtivator9, _target, this, parameters);
            ProxyFactoryUtils.ExecutePipeline(context, _pipelines.Pipeline9).Wait();
            return (IEnumerable<string>)context.ReturnValue;
        }

        public Task<int> DoSomethingAsyncWithoutParameterAndValueTypeReturn()
            => _target.DoSomethingAsyncWithoutParameterAndValueTypeReturn();

        public Task<int> InterceptedDoSomethingAsyncWithoutParameterAndValueTypeReturn()
        {
            var context = _contextFactory.CreateContext(_aspectContextAtivator10, _target, this, ProxyFactoryUtils.EmptyParameters);
            ProxyFactoryUtils.ExecutePipeline(context, _pipelines.Pipeline10).Wait();
            return Task.FromResult((int)context.ReturnValue);
        }

        public Task<int> DoSomethingAsyncWithParameterAndValueTypeReturn(int param1, string param2, IEnumerable<string> param3)
            => _target.DoSomethingAsyncWithParameterAndValueTypeReturn(param1, param2, param3);

        public Task<int> InterceptedDoSomethingAsyncWithParameterAndValueTypeReturn(int param1, string param2, IEnumerable<string> param3)
        {
            var parameters = new object[] { param1, param2, param3 };
            var context = _contextFactory.CreateContext(_aspectContextAtivator11, _target, this, parameters);
            ProxyFactoryUtils.ExecutePipeline(context, _pipelines.Pipeline11).Wait();
            return Task.FromResult((int)context.ReturnValue);
        }

        public ValueTask<int> DoSomethingValueAsyncWithoutParameterAndValueTypeReturn()
            => _target.DoSomethingValueAsyncWithoutParameterAndValueTypeReturn();

        public ValueTask<int> InterceptedDoSomethingValueAsyncWithoutParameterAndValueTypeReturn()
        {
            var context = _contextFactory.CreateContext(_aspectContextAtivator12, _target, this, ProxyFactoryUtils.EmptyParameters);
            ProxyFactoryUtils.ExecutePipeline(context, _pipelines.Pipeline12).Wait();
            return ValueTask.FromResult((int)context.ReturnValue);
        }

        public ValueTask<int> DoSomethingValueAsyncWithParameterAndValueTypeReturn(int param1, string param2, IEnumerable<string> param3)
            => _target.DoSomethingValueAsyncWithParameterAndValueTypeReturn(param1, param2, param3);

        public ValueTask<int> InterceptedDoSomethingValueAsyncWithParameterAndValueTypeReturn(int param1, string param2, IEnumerable<string> param3)
        {
            var parameters = new object[] { param1, param2, param3 };
            var context = _contextFactory.CreateContext(_aspectContextAtivator13, _target, this, parameters);
            ProxyFactoryUtils.ExecutePipeline(context, _pipelines.Pipeline13).Wait();
            return ValueTask.FromResult((int)context.ReturnValue);
        }

        public Task<IEnumerable<string>> DoSomethingAsyncWithoutParameterAndReferenceypeReturn()
            => _target.DoSomethingAsyncWithoutParameterAndReferenceypeReturn();

        public Task<IEnumerable<string>> InterceptedDoSomethingAsyncWithoutParameterAndReferenceypeReturn()
        {
            var context = _contextFactory.CreateContext(_aspectContextAtivator14, _target, this, ProxyFactoryUtils.EmptyParameters);
            ProxyFactoryUtils.ExecutePipeline(context, _pipelines.Pipeline14).Wait();
            return Task.FromResult((IEnumerable<string>)context.ReturnValue);
        }

        public Task<IEnumerable<string>> DoSomethingAsyncWithParameterAndReferenceTypeReturn(int param1, string param2, IEnumerable<string> param3)
            => _target.DoSomethingAsyncWithParameterAndReferenceTypeReturn(param1, param2, param3);

        public Task<IEnumerable<string>> InterceptedDoSomethingAsyncWithParameterAndReferenceTypeReturn(int param1, string param2, IEnumerable<string> param3)
        {
            var parameters = new object[] { param1, param2, param3 };
            var context = _contextFactory.CreateContext(_aspectContextAtivator15, _target, this, parameters);
            ProxyFactoryUtils.ExecutePipeline(context, _pipelines.Pipeline15).Wait();
            return Task.FromResult((IEnumerable<string>)context.ReturnValue);
        }

        public ValueTask<IEnumerable<string>> DoSomethingValueAsyncWithoutParameterAndReferenceTypeReturn()
            => _target.DoSomethingValueAsyncWithoutParameterAndReferenceTypeReturn();

        public ValueTask<IEnumerable<string>> InterceptedDoSomethingValueAsyncWithoutParameterAndReferenceTypeReturn()
        {
            var context = _contextFactory.CreateContext(_aspectContextAtivator16, _target, this, ProxyFactoryUtils.EmptyParameters);
            ProxyFactoryUtils.ExecutePipeline(context, _pipelines.Pipeline16).Wait();
            return ValueTask.FromResult((IEnumerable<string>)context.ReturnValue);
        }

        public ValueTask<IEnumerable<string>> DoSomethingValueAsyncWithParameterAndReferenceTypeReturn(int param1, string param2, IEnumerable<string> param3)
            => _target.DoSomethingValueAsyncWithParameterAndReferenceTypeReturn(param1, param2, param3);

        public ValueTask<IEnumerable<string>> InterceptedDoSomethingValueAsyncWithParameterAndReferenceTypeReturn(int param1, string param2, IEnumerable<string> param3)
        {
            var parameters = new object[] { param1, param2, param3 };
            var context = _contextFactory.CreateContext(_aspectContextAtivator17, _target, this, parameters);
            ProxyFactoryUtils.ExecutePipeline(context, _pipelines.Pipeline17).Wait();
            return ValueTask.FromResult((IEnumerable<string>)context.ReturnValue);
        }

        private static readonly FakeServiceProxy_Pipelines _pipelines;

        private static readonly AspectContextActivator _aspectContextAtivator1;
        private static readonly AspectContextActivator _aspectContextAtivator2;
        private static readonly AspectContextActivator _aspectContextAtivator3;
        private static readonly AspectContextActivator _aspectContextAtivator4;
        private static readonly AspectContextActivator _aspectContextAtivator5;
        private static readonly AspectContextActivator _aspectContextAtivator6;
        private static readonly AspectContextActivator _aspectContextAtivator7;
        private static readonly AspectContextActivator _aspectContextAtivator8;
        private static readonly AspectContextActivator _aspectContextAtivator9;
        private static readonly AspectContextActivator _aspectContextAtivator10;
        private static readonly AspectContextActivator _aspectContextAtivator11;
        private static readonly AspectContextActivator _aspectContextAtivator12;
        private static readonly AspectContextActivator _aspectContextAtivator13;
        private static readonly AspectContextActivator _aspectContextAtivator14;
        private static readonly AspectContextActivator _aspectContextAtivator15;
        private static readonly AspectContextActivator _aspectContextAtivator16;
        private static readonly AspectContextActivator _aspectContextAtivator17;

        static FakeServiceProxy()
        {
            _pipelines = new FakeServiceProxy_Pipelines();

            Type serviceType = typeof(IFakeService);
            Type proxyType = typeof(FakeServiceProxy);
            Type targetType = typeof(FakeService);
            Type[] parameterTypes;

            _aspectContextAtivator1 = ProxyFactoryUtils.NewContextActivator(serviceType, proxyType, targetType, "InterceptedDoSomethingWithoutParameterAndWithoutReturn");

            parameterTypes = new Type[] { typeof(int), typeof(string), typeof(IEnumerable<string>) };
            _aspectContextAtivator2 = ProxyFactoryUtils.NewContextActivator(serviceType, proxyType, targetType, "InterceptedDoSomethingWithParameterAndWithoutReturn", parameterTypes);

            _aspectContextAtivator3 = ProxyFactoryUtils.NewContextActivator(serviceType, proxyType, targetType, "InterceptedDoSomethingAsyncWithoutParameterAndWithoutReturn");

            _aspectContextAtivator4 = ProxyFactoryUtils.NewContextActivator(serviceType, proxyType, targetType, "InterceptedDoSomethingValueAsyncWithoutParameterAndWithoutReturn");

            parameterTypes = new Type[] { typeof(int), typeof(string), typeof(IEnumerable<string>) };
            _aspectContextAtivator5 = ProxyFactoryUtils.NewContextActivator(serviceType, proxyType, targetType, "InterceptedDoSomethingValueAsyncWithParameterAndWithoutReturn", parameterTypes);

            _aspectContextAtivator6 = ProxyFactoryUtils.NewContextActivator(serviceType, proxyType, targetType, "InterceptedDoSomethingWithoutParameterAndValueTypeReturn");

            parameterTypes = new Type[] { typeof(int), typeof(string), typeof(IEnumerable<string>) };
            _aspectContextAtivator7 = ProxyFactoryUtils.NewContextActivator(serviceType, proxyType, targetType, "InterceptedDoSomethingWithParameterAndValueTypeReturn", parameterTypes);

            _aspectContextAtivator8 = ProxyFactoryUtils.NewContextActivator(serviceType, proxyType, targetType, "InterceptedDoSomethingWithoutParameterAndReferenceTypeReturn");

            parameterTypes = new Type[] { typeof(int), typeof(string), typeof(IEnumerable<string>) };
            _aspectContextAtivator9 = ProxyFactoryUtils.NewContextActivator(serviceType, proxyType, targetType, "InterceptedDoSomethingWithParameterAndReferenceTypeReturn", parameterTypes);

            _aspectContextAtivator10 = ProxyFactoryUtils.NewContextActivator(serviceType, proxyType, targetType, "InterceptedDoSomethingAsyncWithoutParameterAndValueTypeReturn");

            parameterTypes = new Type[] { typeof(int), typeof(string), typeof(IEnumerable<string>) };
            _aspectContextAtivator11 = ProxyFactoryUtils.NewContextActivator(serviceType, proxyType, targetType, "InterceptedDoSomethingAsyncWithParameterAndValueTypeReturn", parameterTypes);

            _aspectContextAtivator12 = ProxyFactoryUtils.NewContextActivator(serviceType, proxyType, targetType, "InterceptedDoSomethingValueAsyncWithoutParameterAndValueTypeReturn");

            parameterTypes = new Type[] { typeof(int), typeof(string), typeof(IEnumerable<string>) };
            _aspectContextAtivator13 = ProxyFactoryUtils.NewContextActivator(serviceType, proxyType, targetType, "InterceptedDoSomethingValueAsyncWithParameterAndValueTypeReturn", parameterTypes);

            _aspectContextAtivator14 = ProxyFactoryUtils.NewContextActivator(serviceType, proxyType, targetType, "InterceptedDoSomethingAsyncWithoutParameterAndReferenceypeReturn");

            parameterTypes = new Type[] { typeof(int), typeof(string), typeof(IEnumerable<string>) };
            _aspectContextAtivator15 = ProxyFactoryUtils.NewContextActivator(serviceType, proxyType, targetType, "InterceptedDoSomethingAsyncWithParameterAndReferenceTypeReturn", parameterTypes);

            _aspectContextAtivator16 = ProxyFactoryUtils.NewContextActivator(serviceType, proxyType, targetType, "InterceptedDoSomethingValueAsyncWithoutParameterAndReferenceTypeReturn");

            parameterTypes = new Type[] { typeof(int), typeof(string), typeof(IEnumerable<string>) };
            _aspectContextAtivator17 = ProxyFactoryUtils.NewContextActivator(serviceType, proxyType, targetType, "InterceptedDoSomethingValueAsyncWithParameterAndReferenceTypeReturn", parameterTypes);
        }
    }
}
