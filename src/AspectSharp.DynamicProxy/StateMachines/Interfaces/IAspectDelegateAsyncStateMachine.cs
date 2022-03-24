using AspectSharp.Abstractions;
using System;
using System.Runtime.CompilerServices;

namespace AspectSharp.DynamicProxy.StateMachines.Interfaces
{
    internal interface IAspectDelegateAsyncStateMachine : IAsyncStateMachine
    {
        int State { get; set; }
        AspectContext Context { get; set; }
        void SetResult();
        void SetException(Exception ex);
        void AwaitUnsafeOnCompleted();
    }
}
