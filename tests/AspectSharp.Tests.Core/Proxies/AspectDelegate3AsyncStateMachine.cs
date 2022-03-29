using AspectSharp.Abstractions;
using AspectSharp.Tests.Core.Services.Interfaces;
using System;
using System.Runtime.CompilerServices;

namespace AspectSharp.Tests.Core.Proxies
{
    public struct AspectDelegate3AsyncStateMachine : IAsyncStateMachine
    {
        public int state;
        public AsyncTaskMethodBuilder builder;
        public AspectContext context;
        private TaskAwaiter _awaiter;

        public void MoveNext()
        {
            var num = state;
            if (num == -2)
                return;
            try
            {
                TaskAwaiter awaiter;
                if (num != 0)
                {
                    awaiter = PrepareAwaiter();

                    if (!awaiter.IsCompleted)
                    {
                        AwaitOnCompleted(ref awaiter);
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

        private TaskAwaiter PrepareAwaiter()
        {
            IFakeService target;
            target = context.Target as IFakeService;
            return target.InterceptedDoSomethingAsyncWithoutParameterAndWithoutReturn().GetAwaiter();
        }

        private void AwaitOnCompleted(ref TaskAwaiter awaiter)
        {
            state = 0;
            _awaiter = awaiter;
            builder.AwaitUnsafeOnCompleted(ref awaiter, ref this);
        }

        private void AfterCompletion(ref TaskAwaiter awaiter)
            => awaiter.GetResult();

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

        public void SetStateMachine(IAsyncStateMachine stateMachine)
            => builder.SetStateMachine(stateMachine);
    }
}
