using AspectSharp.Tests.Core.Aspects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspectSharp.Tests.Core.Services.Interfaces
{
    [Aspect3]
    public interface IFakeService
    {
        event EventHandler OnChanged;

        int ValueTypeProperty { get; set; }
        IEnumerable<string> ReferenceTypeProperty { get; set; }

        void DoSomethingWithoutParameterAndWithoutReturn();

        [Aspect1]
        [Aspect2]
        void InterceptedDoSomethingWithoutParameterAndWithoutReturn();

        void DoSomethingWithParameterAndWithoutReturn(int param1, string param2, IEnumerable<string> param3);

        [Aspect1]
        [Aspect2]
        void InterceptedDoSomethingWithParameterAndWithoutReturn(int param1, string param2, IEnumerable<string> param3);

        Task DoSomethingAsyncWithoutParameterAndWithoutReturn();

        [Aspect1]
        [Aspect2]
        Task InterceptedDoSomethingAsyncWithoutParameterAndWithoutReturn();

        ValueTask DoSomethingValueAsyncWithoutParameterAndWithoutReturn();

        [Aspect1]
        [Aspect2]
        ValueTask InterceptedDoSomethingValueAsyncWithoutParameterAndWithoutReturn();

        ValueTask DoSomethingValueAsyncWithParameterAndWithoutReturn(int param1, string param2, IEnumerable<string> param3);

        [Aspect1]
        [Aspect2]
        ValueTask InterceptedDoSomethingValueAsyncWithParameterAndWithoutReturn(int param1, string param2, IEnumerable<string> param3);

        int DoSomethingWithoutParameterAndValueTypeReturn();

        [Aspect1]
        [Aspect2]
        int InterceptedDoSomethingWithoutParameterAndValueTypeReturn();

        int DoSomethingWithParameterAndValueTypeReturn(int param1, string param2, IEnumerable<string> param3);

        [Aspect1]
        [Aspect2]
        int InterceptedDoSomethingWithParameterAndValueTypeReturn(int param1, string param2, IEnumerable<string> param3);

        IEnumerable<string> DoSomethingWithoutParameterAndReferenceTypeReturn();

        [Aspect1]
        [Aspect2]
        IEnumerable<string> InterceptedDoSomethingWithoutParameterAndReferenceTypeReturn();

        IEnumerable<string> DoSomethingWithParameterAndReferenceTypeReturn(int param1, string param2, IEnumerable<string> param3);

        [Aspect1]
        [Aspect2]
        IEnumerable<string> InterceptedDoSomethingWithParameterAndReferenceTypeReturn(int param1, string param2, IEnumerable<string> param3);

        Task<int> DoSomethingAsyncWithoutParameterAndValueTypeReturn();

        [Aspect1]
        [Aspect2]
        Task<int> InterceptedDoSomethingAsyncWithoutParameterAndValueTypeReturn();

        Task<int> DoSomethingAsyncWithParameterAndValueTypeReturn(int param1, string param2, IEnumerable<string> param3);

        [Aspect1]
        [Aspect2]
        Task<int> InterceptedDoSomethingAsyncWithParameterAndValueTypeReturn(int param1, string param2, IEnumerable<string> param3);

        ValueTask<int> DoSomethingValueAsyncWithoutParameterAndValueTypeReturn();

        [Aspect1]
        [Aspect2]
        ValueTask<int> InterceptedDoSomethingValueAsyncWithoutParameterAndValueTypeReturn();

        ValueTask<int> DoSomethingValueAsyncWithParameterAndValueTypeReturn(int param1, string param2, IEnumerable<string> param3);

        [Aspect1]
        ValueTask<int> InterceptedDoSomethingValueAsyncWithParameterAndValueTypeReturn(int param1, string param2, IEnumerable<string> param3);

        Task<IEnumerable<string>> DoSomethingAsyncWithoutParameterAndReferenceypeReturn();

        [Aspect1]
        [Aspect2]
        Task<IEnumerable<string>> InterceptedDoSomethingAsyncWithoutParameterAndReferenceypeReturn();

        Task<IEnumerable<string>> DoSomethingAsyncWithParameterAndReferenceTypeReturn(int param1, string param2, IEnumerable<string> param3);

        [Aspect1]
        [Aspect2]
        Task<IEnumerable<string>> InterceptedDoSomethingAsyncWithParameterAndReferenceTypeReturn(int param1, string param2, IEnumerable<string> param3);

        ValueTask<IEnumerable<string>> DoSomethingValueAsyncWithoutParameterAndReferenceTypeReturn();

        [Aspect1]
        [Aspect2]
        ValueTask<IEnumerable<string>> InterceptedDoSomethingValueAsyncWithoutParameterAndReferenceTypeReturn();

        ValueTask<IEnumerable<string>> DoSomethingValueAsyncWithParameterAndReferenceTypeReturn(int param1, string param2, IEnumerable<string> param3);

        [Aspect1]
        [Aspect2]
        ValueTask<IEnumerable<string>> InterceptedDoSomethingValueAsyncWithParameterAndReferenceTypeReturn(int param1, string param2, IEnumerable<string> param3);
    }
}
