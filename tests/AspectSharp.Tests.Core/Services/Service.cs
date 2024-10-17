using AspectSharp.DynamicProxy;
using AspectSharp.Tests.Core.Fakes;
using AspectSharp.Tests.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectSharp.Tests.Core.Services
{
    public sealed class Service : IService
    {
        private readonly FakeAspectContextFactory _factory;

        public Service(IAspectContextFactory contextFactory)
        {
            _factory = (contextFactory as FakeAspectContextFactory);
        }

        public IEnumerable<string> Function()
        {
            _factory.CurrentContext.AdditionalData.Add("Service.Function", null);
            return Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<string>> AsyncFunction()
        {
            _factory.CurrentContext.AdditionalData.Add("Service.AsyncFunction", null);
            await Task.Delay(TimeSpan.FromMilliseconds(0.1));
            return Enumerable.Empty<string>();
        }

#if NETCOREAPP3_1_OR_GREATER
        public async ValueTask<IEnumerable<string>> ValueAsyncFunction()
        {
            _factory.CurrentContext.AdditionalData.Add("Service.ValueAsyncFunction", null);
            await Task.Delay(TimeSpan.FromMilliseconds(0.1));
            return Enumerable.Empty<string>();
        }
#endif
    }
}
