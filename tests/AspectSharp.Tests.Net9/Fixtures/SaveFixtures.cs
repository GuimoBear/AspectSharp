using AspectSharp.DynamicProxy.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AspectSharp.Tests.Net9.Fixtures
{
    public sealed class SaveFixtures : IDisposable
    {
        public void Dispose()
        {
            var type = DynamicProxyFactory._proxiedClassesAssemblyBuilder.GetType();
        }
    }

    [CollectionDefinition(NAME)]
    public sealed class SaveFixtureCollection : ICollectionFixture<SaveFixtures>
    {
        public const string NAME = "Save assembly";
    }
}
