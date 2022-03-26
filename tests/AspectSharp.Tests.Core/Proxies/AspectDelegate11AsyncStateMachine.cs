using AspectSharp.Abstractions;
using AspectSharp.Tests.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AspectSharp.Tests.Core.Proxies
{
    public struct AspectDelegate11AsyncStateMachine : IAsyncStateMachine
    {
        public int state;
        public AsyncTaskMethodBuilder builder;
        public AspectContext context;
        private TaskAwaiter<int> _awaiter;
        private AspectContext _contextWrap;

        void IAsyncStateMachine.MoveNext()
        {
            var num = state;
            try
            {
                TaskAwaiter<int> awaiter;
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
                AfterCompletion(ref awaiter);
            }
            catch (Exception ex)
            {
                OnException(ex);
                return;
            }
            OnCompleted();
        }

        private TaskAwaiter<int> PrepareAwaiter()
        {
            IFakeService target;
            int param1;
            string param2;
            IEnumerable<string> param3;
            target = context.Target as IFakeService;
            param1 = (int)context.Parameters[0];
            param2 = (string)context.Parameters[1];
            param3 = (IEnumerable<string>)context.Parameters[2];
            _contextWrap = context;
            return target.InterceptedDoSomethingAsyncWithParameterAndValueTypeReturn(param1, param2, param3).GetAwaiter();
        }

        private void AfterCompletion(ref TaskAwaiter<int> awaiter)
        {
            var result = awaiter.GetResult();
            _contextWrap.ReturnValue = result;
            _contextWrap = null;
        }

        private void OnException(Exception ex)
        {
            state = -2;
            builder.SetException(ex);
        }

        private void OnCompleted()
        {
            state = -2;
            builder.SetResult();
        }

        void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
            => builder.SetStateMachine(stateMachine);
    }
}
