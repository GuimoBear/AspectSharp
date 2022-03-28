using AspectSharp.Abstractions;
using AspectSharp.DynamicProxy;
using AspectSharp.DynamicProxy.Utils;
using AspectSharp.Tests.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AspectSharp.Tests.Core.Proxies
{
    internal struct InterceptedDoSomethingAsyncWithParameterAndReferenceTypeReturnAsyncStateMachine : IAsyncStateMachine
    {
        public int state;
        public AsyncTaskMethodBuilder<IEnumerable<string>> builder;
        public IFakeService target;
        public FakeServiceProxy proxy;
        public IAspectContextFactory contextFactory;

        public int param1;
        public string param2;
        public IEnumerable<string> param3;

        private TaskAwaiter _awaiter;
        private AspectContext _context;

        void IAsyncStateMachine.MoveNext()
        {
            var num = state;
            IEnumerable<string> result;
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
                state = -2;
                builder.SetException(ex);
                return;
            }
            state = -2;
            builder.SetResult(result);
        }

        private TaskAwaiter PrepareAwaiter()
        {
            var parameters = new object[3];
            parameters[0] = param1;
            parameters[1] = param2;
            parameters[2] = param3;
            _context = contextFactory.CreateContext(FakeServiceProxy._aspectContextAtivator15, target, proxy, parameters);
            return ProxyFactoryUtils.ExecutePipeline(_context, FakeServiceProxyPipelines.Pipeline15).GetAwaiter();
        }

        private IEnumerable<string> AfterCompletion(ref TaskAwaiter awaiter)
        {
            awaiter.GetResult();
            var result = (IEnumerable<string>)_context.ReturnValue;
            _context = null;
            return result;
        }

        void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
            => builder.SetStateMachine(stateMachine);
    }
}
