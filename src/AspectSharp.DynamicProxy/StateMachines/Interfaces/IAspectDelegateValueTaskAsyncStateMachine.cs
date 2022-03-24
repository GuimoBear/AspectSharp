#if NETCOREAPP3_1_OR_GREATER
using System.Runtime.CompilerServices;

namespace AspectSharp.DynamicProxy.StateMachines.Interfaces
{
    internal interface IAspectDelegateValueTaskAsyncStateMachine : IAspectDelegateAsyncStateMachine
    {
        ValueTaskAwaiter Awaiter { get; set; }
        ValueTaskAwaiter PrepareAwaiter();
    }

    internal interface IAspectDelegateValueTaskAsyncStateMachine<TResult> : IAspectDelegateAsyncStateMachine
    {
        ValueTaskAwaiter<TResult> Awaiter { get; set; }
        ValueTaskAwaiter<TResult> PrepareAwaiter();
    }
}
#endif