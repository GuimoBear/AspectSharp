using AspectSharp.Tests.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspectSharp.Tests.Core.Services
{
    public class SimpleGenericReferenceTypeFakeService : ISimpleGenericFakeService<Dictionary<string, int>>
    {
        public Dictionary<string, int> Value { get; set; }

        public Dictionary<string, int> GetValue()
            => Value;

        public void SetValue(Dictionary<string, int> value)
        {

        }

        public async Task<Dictionary<string, int>> WaitMethod(string param)
        {
            await Task.Delay(10);
            return Value;
        }

#if NETCOREAPP3_1_OR_GREATER
        public async ValueTask<Dictionary<string, int>> ValueWaitMethod(string param)
        {
            await Task.Delay(10);
            return Value;
        }
#endif
    }

    public class SimpleGenericValueTypeFakeService : ISimpleGenericFakeService<int>
    {
        public int Value { get; set; }

        public int GetValue()
            => Value;

        public void SetValue(int value)
        {

        }

        public async Task<int> WaitMethod(string param)
        {
            await Task.Delay(10);
            return new Random().Next(0, 100);
        }

#if NETCOREAPP3_1_OR_GREATER
        public async ValueTask<int> ValueWaitMethod(string param)
        {
            await Task.Delay(10);
            return new Random().Next(0, 100);
        }
#endif
    }

    public class SimpleInheritedGenericReferenceTypeFakeService<TList, TValue> : ISimpleGenericFakeService<TList>
        where TList : IEnumerable<TValue>
    {
        public TList Value { get; set; }

        public TList GetValue()
            => Value;

        public void SetValue(TList value)
        {

        }

        public async Task<TList> WaitMethod(string param)
        {
            await Task.Delay(10);
            return Value;
        }

#if NETCOREAPP3_1_OR_GREATER
        public async ValueTask<TList> ValueWaitMethod(string param)
        {
            await Task.Delay(10);
            return Value;
        }
#endif
    }

    public class SimpleInheritedGenericValueTypeFakeService<TValue> : ISimpleGenericFakeService<TValue>
        where TValue : struct
    {
        public TValue Value { get; set; }

        public TValue GetValue()
            => Value;

        public void SetValue(TValue value)
        {

        }

        public async Task<TValue> WaitMethod(string param)
        {
            await Task.Delay(10);
            return Value;
        }

#if NETCOREAPP3_1_OR_GREATER
        public async ValueTask<TValue> ValueWaitMethod(string param)
        {
            await Task.Delay(10);
            return Value;
        }
#endif
    }
}
