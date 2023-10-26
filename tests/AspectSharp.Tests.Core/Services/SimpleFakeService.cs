using AspectSharp.Tests.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AspectSharp.Tests.Core.Services
{
    public sealed class SimpleFakeService : ISimpleFakeService
    {
        public event EventHandler OnEvent;
        public event EventHandler OnInterceptedEvent;
        public event EventHandler OnEventWithAnyAspect;
        public event EventHandler OnEventWithAllAspect;

        public int Property { get; set; }
        public int InterceptedProperty { get; set; }
        public int PropertyWithAnyAspect { get; set; }
        public int PropertyWithAllAspect { get; set; }

        public void InterceptedMethod()
        {

        }

        public void Method()
        {

        }

        public void MethodWithAnyAspect()
        {

        }

        public async Task<string> WaitMethod(string param)
        {
            await Task.Delay(10);
            return string.Format("Hello {0}: the random number is {1}", param, new Random().Next(0, 100));
        }

#if NETCOREAPP3_1_OR_GREATER
        public async ValueTask<string> ValueWaitMethod(string param)
        {
            await Task.Delay(10);
            return string.Format("Hello {0}: the random number is {1}", param, new Random().Next(0, 100));
        }
#endif

        static readonly Random random = new Random();

        public bool MethodWithOutValueTypeParameter(int inValue, out int outValue)
        {
            outValue = random.Next();
            return true;
        }

        public bool MethodWithOutReferenceTypeParameter(Dictionary<string, int> inValue, out Dictionary<string, int> outValue)
        {
            outValue = new Dictionary<string, int>()
            {
                { Guid.NewGuid().ToString(), random.Next() }
            };
            return true;
        }

        public bool MethodWithRefValueTypeParameter(int inValue, ref int outValue)
        {
            outValue = random.Next();
            return true;
        }

        public bool MethodWithRefReferenceTypeParameter(Dictionary<string, int> inValue, ref Dictionary<string, int> outValue)
        {
            outValue = new Dictionary<string, int>()
            {
                { Guid.NewGuid().ToString(), random.Next() }
            };
            return true;
        }
    }
}
