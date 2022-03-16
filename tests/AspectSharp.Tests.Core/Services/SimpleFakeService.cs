using AspectSharp.Tests.Core.Services.Interfaces;
using System;

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
    }
}
