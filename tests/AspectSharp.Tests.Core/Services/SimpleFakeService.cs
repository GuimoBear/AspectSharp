using AspectSharp.Tests.Core.Services.Interfaces;
using System;
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
    }
}
