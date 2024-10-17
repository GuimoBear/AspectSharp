using AspectSharp.DynamicProxy.Factories;
using System;
using System.IO;
using Xunit;

namespace AspectSharp.Tests.Net481.Fixtures
{
    public sealed class SaveAssemblyFixture : IDisposable
    {
        public void Dispose()
        {
            if (File.Exists(DynamicProxyFactory._proxiedClassesAssemblyBuilder.GetName().Name + ".dll"))
                File.Delete(DynamicProxyFactory._proxiedClassesAssemblyBuilder.GetName().Name + ".dll");
            if (File.Exists(DynamicProxyFactory._proxiedClassesAssemblyBuilder.GetName().Name + ".mod"))
                File.Delete(DynamicProxyFactory._proxiedClassesAssemblyBuilder.GetName().Name + ".mod");
            DynamicProxyFactory._proxiedClassesAssemblyBuilder.Save(DynamicProxyFactory._proxiedClassesAssemblyBuilder.GetName().Name + ".dll");
        }
    }

    [CollectionDefinition(NAME)]
    public class AspectSharpCollection : ICollectionFixture<SaveAssemblyFixture>
    {
        public const string NAME = "AspectrSharpCollection";

        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
