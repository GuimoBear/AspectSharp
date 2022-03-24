using System;
using System.Runtime.CompilerServices;

namespace AspectSharp.DynamicProxy.StateMachines.Interfaces
{
    internal interface IAspectDelegateTaskAsyncStateMachine : IAspectDelegateAsyncStateMachine
    {
        TaskAwaiter Awaiter { get; set; }
        TaskAwaiter PrepareAwaiter();
    }

    internal interface IAspectDelegateTaskAsyncStateMachine<TResult> : IAspectDelegateAsyncStateMachine
    {
        TaskAwaiter<TResult> Awaiter { get; set; }
        TaskAwaiter<TResult> PrepareAwaiter();
    }
}
