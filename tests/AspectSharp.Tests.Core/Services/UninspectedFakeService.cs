using AspectSharp.Tests.Core.Services.Interfaces;
using System;

namespace AspectSharp.Tests.Core.Services
{
    public sealed class UninspectedFakeService : IUninspectedFakeService
    {
        public event EventHandler OnEvent;
        public event EventHandler OnInterceptedEvent;

        public int Property { get; set; }
        public int InterceptedProperty { get; set; }

        public void InterceptedMethod()
        {

        }

        public void Method()
        {

        }
    }
}
