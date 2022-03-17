using AspectCore.DynamicProxy;
using System;

namespace AspectSharp.Benchmarks.Clients.Interfaces
{
    public interface IMetricsClient
    {
        [NonAspect]
        IDisposable NewScope(string name);
    }
}
