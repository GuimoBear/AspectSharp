using AspectSharp.Abstractions;
using AspectSharp.Tests.Core.Aspects;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspectSharp.Tests.Core.Services.Interfaces
{
    public interface IServiceWithGenericTypeAndGenericMethod<TClass>
    {
        [Aspect3]
        T Call<T>(TClass value);

        [Aspect3]
        void Call<T>(TClass value, T container);

        [Aspect3]
        void Call<T>(TClass value, IContainer<T> container);

        [Aspect3]
        void Call<T>(TClass value, Dictionary<string, IContainer<T>> container) where T : AspectContext;


        [Aspect3]
        Task<T> TaskCall<T>(TClass value);

        [Aspect3]
        Task TaskCall<T>(TClass value, T container);

        [Aspect3]
        Task TaskCall<T>(TClass value, IContainer<T> container);

        [Aspect3]
        Task TaskCall<T>(TClass value, Dictionary<string, IContainer<T>> container) where T : AspectContext;

#if NETCOREAPP3_1_OR_GREATER
        [Aspect3]
        ValueTask<T> ValueTaskCall<T>(TClass value);

        [Aspect3]
        ValueTask ValueTaskCall<T>(TClass value, T container);

        [Aspect3]
        ValueTask ValueTaskCall<T>(TClass value, IContainer<T> container);

        [Aspect3]
        ValueTask ValueTaskCall<T>(TClass value, Dictionary<string, IContainer<T>> container) where T : AspectContext;
#endif


        [Aspect3]
        Task<T> AsyncTaskCall<T>(TClass value);

        [Aspect3]
        Task AsyncTaskCall<T>(TClass value, T container);

        [Aspect3]
        Task AsyncTaskCall<T>(TClass value, IContainer<T> container);

        [Aspect3]
        Task AsyncTaskCall<T>(TClass value, Dictionary<string, IContainer<T>> container) where T : AspectContext;

#if NETCOREAPP3_1_OR_GREATER
        [Aspect3]
        ValueTask<T> AsyncValueTaskCall<T>(TClass value);

        [Aspect3]
        ValueTask AsyncValueTaskCall<T>(TClass value, T container);

        [Aspect3]
        ValueTask AsyncValueTaskCall<T>(TClass value, IContainer<T> container);

        [Aspect3]
        ValueTask AsyncValueTaskCall<T>(TClass value, Dictionary<string, IContainer<T>> container) where T : AspectContext;
#endif




        [Aspect3]
        void CallWithRefParameter<T>(TClass value, ref T container);

        [Aspect3]
        void CallWithRefParameter<T>(TClass value, ref IContainer<T> container);

        [Aspect3]
        void CallWithRefParameter<T>(TClass value, ref Dictionary<string, IContainer<T>> container) where T : AspectContext;

        [Aspect3]
        void CallWithOutParameter<T>(TClass value, T parameter, out T container);

        [Aspect3]
        void CallWithOutParameter<T>(TClass value, IContainer<T> parameter, out IContainer<T> container);

        [Aspect3]
        void CallWithOutParameter<T>(TClass value, Dictionary<string, IContainer<T>> parameter, out Dictionary<string, IContainer<T>> container) where T : AspectContext;
    }
}
