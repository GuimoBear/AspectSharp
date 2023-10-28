using AspectSharp.Abstractions;
using AspectSharp.Tests.Core.Aspects;
using System.Collections.Generic;

namespace AspectSharp.Tests.Core.Services.Interfaces
{
    public interface IServiceWithGenericMethod
    {
        [Aspect3]
        T Call<T>();

        //[Aspect3]
        //void Call<T>(T container);

        //[Aspect3]
        //void Call<T>(IContainer<T> container);

        //[Aspect3]
        //void Call<T>(Dictionary<string, IContainer<T>> container) where T: notnull, AspectContext;
    }

    public interface IContainer<T>
    {
        T Body { get; }
    }
}
