using AspectSharp.Abstractions;
using AspectSharp.DynamicProxy;
using AspectSharp.DynamicProxy.Utils;
using AspectSharp.Tests.Core.Services.Interfaces;
using System;
using System.Runtime.CompilerServices;

namespace AspectSharp.Tests.Core.Proxies
{
    internal struct InterceptedDoSomethingAsyncWithoutParameterAndValueTypeReturnAsyncStateMachine : IAsyncStateMachine
    {
        public int state;
        public AsyncTaskMethodBuilder<int> builder;
        public IFakeService target;
        public FakeServiceProxy proxy;
        public IAspectContextFactory contextFactory;

        private TaskAwaiter _awaiter;
        private AspectContext _context;

        void IAsyncStateMachine.MoveNext()
        {
            var num = state;
            int result;
            try
            {
                TaskAwaiter awaiter;
                if (num != 0)
                {
                    awaiter = PrepareAwaiter();

                    if (!awaiter.IsCompleted)
                    {
                        _awaiter = awaiter;
                        state = 0;
                        builder.AwaitUnsafeOnCompleted(ref awaiter, ref this);
                        return;
                    }
                }
                else
                {
                    awaiter = _awaiter;
                    _awaiter = default;
                    state = -1;
                }
                result = AfterCompletion(ref awaiter);
            }
            catch (Exception ex)
            {
                OnException(ex);
                return;
            }
            OnCompleted(result);
        }

        private TaskAwaiter PrepareAwaiter()
        {
            _context = contextFactory.CreateContext(FakeServiceProxy._aspectContextAtivator10, target, proxy, ProxyFactoryUtils.EmptyParameters);
            return ProxyFactoryUtils.ExecutePipeline(_context, FakeServiceProxyPipelines.Pipeline10).GetAwaiter();
        }

        private int AfterCompletion(ref TaskAwaiter awaiter)
        {
            awaiter.GetResult();
            var result = (int)_context.ReturnValue;
            _context = null;
            return result;
        }

        private void OnException(Exception ex)
        {
            state = -2;
            builder.SetException(ex);
        }

        private void OnCompleted(int result)
        {
            state = -2;
            builder.SetResult(result);
        }

        void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
            => builder.SetStateMachine(stateMachine);
    }
}
