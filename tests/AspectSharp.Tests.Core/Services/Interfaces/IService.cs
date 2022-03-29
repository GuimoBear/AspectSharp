using AspectSharp.Tests.Core.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectSharp.Tests.Core.Services.Interfaces
{
    [TypeDefinitionAspect]
    [Aspect3]
    public interface IService
    {
        [Aspect1]
        [Aspect2]
        IEnumerable<string> Function();

        [Aspect1]
        [Aspect2]
        Task<IEnumerable<string>> AsyncFunction();

#if NETCOREAPP3_1_OR_GREATER
        [Aspect1]
        [Aspect2]
        ValueTask<IEnumerable<string>> ValueAsyncFunction();
#endif
    }
}
