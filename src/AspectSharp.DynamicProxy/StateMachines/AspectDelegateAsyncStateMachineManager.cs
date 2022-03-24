using AspectSharp.DynamicProxy.StateMachines.Interfaces;
using System;
using System.Runtime.CompilerServices;

namespace AspectSharp.DynamicProxy.StateMachines
{
    internal static class AspectDelegateTaskAsyncStateMachineManager
    {
        public static void MoveNext<TAsyncStateMachine>(ref TAsyncStateMachine stateMachine)
            where TAsyncStateMachine : IAspectDelegateTaskAsyncStateMachine
        {
            var state = stateMachine.State;
            try
            {
                TaskAwaiter awaiter;
                if (state != 0)
                {
                    awaiter = stateMachine.PrepareAwaiter();

                    if (!awaiter.IsCompleted)
                    {
                        state = stateMachine.State = 0;
                        stateMachine.Awaiter = awaiter;
                        stateMachine.AwaitUnsafeOnCompleted();
                        return;
                    }
                }
                else
                {
                    awaiter = stateMachine.Awaiter;
                    stateMachine.Awaiter = default;
                    state = stateMachine.State = -1;
                }
                awaiter.GetResult();
            }
            catch (Exception ex)
            {
                stateMachine.State = -2;
                stateMachine.SetException(ex);
                return;
            }
            stateMachine.State = -2;
            stateMachine.SetResult();
        }

        public static void MoveNext<TAsyncStateMachine, TResult>(ref TAsyncStateMachine stateMachine)
            where TAsyncStateMachine : IAspectDelegateTaskAsyncStateMachine<TResult>
        {
            var state = stateMachine.State;
            TResult result;
            try
            {
                TaskAwaiter<TResult> awaiter;
                if (state != 0)
                {
                    awaiter = stateMachine.PrepareAwaiter();
                    if (!awaiter.IsCompleted)
                    {
                        state = stateMachine.State = 0;
                        stateMachine.Awaiter = awaiter;
                        stateMachine.AwaitUnsafeOnCompleted();
                        return;
                    }
                }
                else
                {
                    awaiter = stateMachine.Awaiter;
                    stateMachine.Awaiter = default;
                    state = stateMachine.State = -1;
                }
                result = awaiter.GetResult();
                stateMachine.Context.ReturnValue = result;
                stateMachine.Context = null;
            }
            catch (Exception ex)
            {
                stateMachine.State = -2;
                stateMachine.SetException(ex);
                return;
            }
            stateMachine.State = -2;
            stateMachine.SetResult();
        }
    }
}
