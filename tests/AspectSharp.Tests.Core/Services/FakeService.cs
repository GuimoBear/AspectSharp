using AspectSharp.Tests.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspectSharp.Tests.Core.Services
{
    public class FakeService : IFakeService
    {
        private static readonly Random _random = new Random();

        public event EventHandler OnChanged;

        public int ValueTypeProperty { get; set; }

        public IEnumerable<string> ReferenceTypeProperty { get; set; }

        public Task<IEnumerable<string>> DoSomethingAsyncWithoutParameterAndReferenceypeReturn()
            => Task.FromResult<IEnumerable<string>>(new List<string> { "DoSomethingAsyncWithoutParameterAndReferenceypeReturn" });

        public Task<IEnumerable<string>> InterceptedDoSomethingAsyncWithoutParameterAndReferenceypeReturn()
            => DoSomethingAsyncWithoutParameterAndReferenceypeReturn();

        public Task<int> DoSomethingAsyncWithoutParameterAndValueTypeReturn()
            => Task.FromResult(_random.Next());

        public Task<int> InterceptedDoSomethingAsyncWithoutParameterAndValueTypeReturn()
            => DoSomethingAsyncWithoutParameterAndValueTypeReturn();

        public async Task DoSomethingAsyncWithoutParameterAndWithoutReturn()
            => await Task.Delay(1);

        public Task InterceptedDoSomethingAsyncWithoutParameterAndWithoutReturn()
            => DoSomethingAsyncWithoutParameterAndWithoutReturn();

#if NETCOREAPP3_1_OR_GREATER
        public async ValueTask DoSomethingValueAsyncWithoutParameterAndWithoutReturn()
            => await Task.Delay(1);

        public ValueTask InterceptedDoSomethingValueAsyncWithoutParameterAndWithoutReturn()
            => DoSomethingValueAsyncWithoutParameterAndWithoutReturn();
#endif


        public Task<IEnumerable<string>> DoSomethingAsyncWithParameterAndReferenceTypeReturn(int param1, string param2, IEnumerable<string> param3)
            => Task.FromResult<IEnumerable<string>>(new List<string> { string.Format("DoSomethingAsyncWithParameterAndReferenceTypeReturn {0} {1} {2}", param1, param2, param3) });

        public Task<IEnumerable<string>> InterceptedDoSomethingAsyncWithParameterAndReferenceTypeReturn(int param1, string param2, IEnumerable<string> param3)
            => DoSomethingAsyncWithParameterAndReferenceTypeReturn(param1, param2, param3);

        public Task<int> DoSomethingAsyncWithParameterAndValueTypeReturn(int param1, string param2, IEnumerable<string> param3)
            => Task.FromResult(_random.Next() ^ param1 ^ (param2?.GetHashCode() ?? 0));

        public Task<int> InterceptedDoSomethingAsyncWithParameterAndValueTypeReturn(int param1, string param2, IEnumerable<string> param3)
            => DoSomethingAsyncWithParameterAndValueTypeReturn(param1, param2, param3);

#if NETCOREAPP3_1_OR_GREATER
        public ValueTask<IEnumerable<string>> DoSomethingValueAsyncWithoutParameterAndReferenceTypeReturn()
            => new ValueTask<IEnumerable<string>>(new List<string> { "DoSomethingValueAsyncWithoutParameterAndReferenceTypeReturn" });

        public ValueTask<IEnumerable<string>> InterceptedDoSomethingValueAsyncWithoutParameterAndReferenceTypeReturn()
            => DoSomethingValueAsyncWithoutParameterAndReferenceTypeReturn();

        public ValueTask<int> DoSomethingValueAsyncWithoutParameterAndValueTypeReturn()
            => new ValueTask<int>(_random.Next());

        public ValueTask<int> InterceptedDoSomethingValueAsyncWithoutParameterAndValueTypeReturn()
            => DoSomethingValueAsyncWithoutParameterAndValueTypeReturn();

        public ValueTask<IEnumerable<string>> DoSomethingValueAsyncWithParameterAndReferenceTypeReturn(int param1, string param2, IEnumerable<string> param3)
            => new ValueTask<IEnumerable<string>>(new List<string> { string.Format("DoSomethingValueAsyncWithParameterAndReferenceTypeReturn {0} {1} {2}", param1, param2, param3) });

        public ValueTask<IEnumerable<string>> InterceptedDoSomethingValueAsyncWithParameterAndReferenceTypeReturn(int param1, string param2, IEnumerable<string> param3)
            => DoSomethingValueAsyncWithParameterAndReferenceTypeReturn(param1, param2, param3);

        public ValueTask<int> DoSomethingValueAsyncWithParameterAndValueTypeReturn(int param1, string param2, IEnumerable<string> param3)
            => new ValueTask<int>(_random.Next() ^ param1 ^ (param2?.GetHashCode() ?? 0));

        public ValueTask<int> InterceptedDoSomethingValueAsyncWithParameterAndValueTypeReturn(int param1, string param2, IEnumerable<string> param3)
            => DoSomethingValueAsyncWithParameterAndValueTypeReturn(param1, param2, param3);

        public ValueTask DoSomethingValueAsyncWithParameterAndWithoutReturn(int param1, string param2, IEnumerable<string> param3)
        {
            ValueTypeProperty = param1 ^ (param2?.GetHashCode() ?? default);
            return new ValueTask();
        }

        public ValueTask InterceptedDoSomethingValueAsyncWithParameterAndWithoutReturn(int param1, string param2, IEnumerable<string> param3)
            => DoSomethingValueAsyncWithParameterAndWithoutReturn(param1, param2, param3);
#endif

        public IEnumerable<string> DoSomethingWithoutParameterAndReferenceTypeReturn()
            => new List<string> { "DoSomethingWithoutParameterAndReferenceTypeReturn" };

        public IEnumerable<string> InterceptedDoSomethingWithoutParameterAndReferenceTypeReturn()
            => DoSomethingWithoutParameterAndReferenceTypeReturn();

        public int DoSomethingWithoutParameterAndValueTypeReturn()
            => _random.Next();

        public int InterceptedDoSomethingWithoutParameterAndValueTypeReturn()
            => DoSomethingWithoutParameterAndValueTypeReturn();

        public void DoSomethingWithoutParameterAndWithoutReturn()
            => ValueTypeProperty ^= _random.Next();

        public void InterceptedDoSomethingWithoutParameterAndWithoutReturn()
            => DoSomethingWithoutParameterAndWithoutReturn();

        public IEnumerable<string> DoSomethingWithParameterAndReferenceTypeReturn(int param1, string param2, IEnumerable<string> param3)
            => new List<string> { string.Format("DoSomethingWithParameterAndReferenceTypeReturn {0} {1} {2}", param1, param2, param3) };

        public IEnumerable<string> InterceptedDoSomethingWithParameterAndReferenceTypeReturn(int param1, string param2, IEnumerable<string> param3)
            => DoSomethingWithParameterAndReferenceTypeReturn(param1, param2, param3);

        public int DoSomethingWithParameterAndValueTypeReturn(int param1, string param2, IEnumerable<string> param3)
            => _random.Next() ^ param1 ^ (param2?.GetHashCode() ?? 0);

        public int InterceptedDoSomethingWithParameterAndValueTypeReturn(int param1, string param2, IEnumerable<string> param3)
            => DoSomethingWithParameterAndValueTypeReturn(param1, param2, param3);

        public void DoSomethingWithParameterAndWithoutReturn(int param1, string param2, IEnumerable<string> param3)
            => ValueTypeProperty ^= _random.Next() ^ param1 ^ (param2?.GetHashCode() ?? 0);

        public void InterceptedDoSomethingWithParameterAndWithoutReturn(int param1, string param2, IEnumerable<string> param3)
            => DoSomethingWithParameterAndWithoutReturn(param1, param2, param3);
    }
}
