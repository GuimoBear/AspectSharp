using System;

namespace AspectSharp.Tests.Core.Services.Interfaces
{
    public interface IUninspectedFakeService
    {
        event EventHandler OnEvent;

        event EventHandler OnInterceptedEvent;

        int Property { get; set; }

        int InterceptedProperty { get; set; }

        void Method();

        void InterceptedMethod();
    }
}
