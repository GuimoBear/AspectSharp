using AspectSharp.Abstractions;
using AspectSharp.Tests.Core.Aspects;
using System.Collections.Generic;

namespace AspectSharp.Tests.Core.Services.Interfaces
{
    public interface IServiceWithGenericMethod
    {
        [Aspect3]
        T Call<T>();

        [Aspect3]
        void Call<T>(T container);

        [Aspect3]
        void Call<T>(IContainer<T> container);

        [Aspect3]
        void Call<T>(Dictionary<string, IContainer<T>> container) where T : notnull, AspectContext;

        [Aspect3]
        void CallWithRefParameter<T>(ref T container);

        [Aspect3]
        void CallWithRefParameter<T>(ref IContainer<T> container);

        [Aspect3]
        void CallWithRefParameter<T>(ref Dictionary<string, IContainer<T>> container) where T : notnull, AspectContext;

        [Aspect3]
        void CallWithOutParameter<T>(T parameter, out T container);

        [Aspect3]
        void CallWithOutParameter<T>(IContainer<T> parameter, out IContainer<T> container);

        [Aspect3]
        void CallWithOutParameter<T>(Dictionary<string, IContainer<T>> parameter, out Dictionary<string, IContainer<T>> container) where T : notnull, AspectContext;
    }

    public interface IContainer<T>
    {
        T Body { get; }
    }
}
