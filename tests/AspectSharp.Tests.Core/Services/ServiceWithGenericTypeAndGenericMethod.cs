using AspectSharp.Abstractions;
using AspectSharp.Tests.Core.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspectSharp.Tests.Core.Services
{
    internal class ServiceWithGenericTypeAndGenericMethod<TClass> : IServiceWithGenericTypeAndGenericMethod<TClass>
    {
        public T Call<T>(TClass value)
        {
            DoNothing();
            return default;
        }

        public void Call<T>(TClass value, T container)
            => DoNothing();

        public void Call<T>(TClass value, IContainer<T> container)
            => DoNothing();

        public void Call<T>(TClass value, Dictionary<string, IContainer<T>> container) where T : AspectContext
            => DoNothing();



        public Task<T> TaskCall<T>(TClass value)
            => Task.FromResult(default(T));

        public Task TaskCall<T>(TClass value, T container)
        {
            DoNothing();
            return Task.CompletedTask;
        }

        public Task TaskCall<T>(TClass value, IContainer<T> container)
        {
            DoNothing();
            return Task.CompletedTask;
        }

        public Task TaskCall<T>(TClass value, Dictionary<string, IContainer<T>> container) where T : AspectContext
        {
            DoNothing();
            return Task.CompletedTask;
        }

#if NETCOREAPP3_1_OR_GREATER
        public ValueTask<T> ValueTaskCall<T>(TClass value)
            => new ValueTask<T>(Task.FromResult(default(T)));

        public ValueTask ValueTaskCall<T>(TClass value, T container)
        {
            DoNothing();
            return new ValueTask(Task.CompletedTask);
        }

        public ValueTask ValueTaskCall<T>(TClass value, IContainer<T> container)
        {
            DoNothing();
            return new ValueTask(Task.CompletedTask);
        }

        public ValueTask ValueTaskCall<T>(TClass value, Dictionary<string, IContainer<T>> container) where T : AspectContext
        {
            DoNothing();
            return new ValueTask(Task.CompletedTask);
        }
#endif






        public async Task<T> AsyncTaskCall<T>(TClass value)
        {
            await Task.Delay(1);
            return default(T);
        }

        public async Task AsyncTaskCall<T>(TClass value, T container)
        {
            await Task.Delay(1);
        }

        public async Task AsyncTaskCall<T>(TClass value, IContainer<T> container)
        {
            await Task.Delay(1);
        }

        public async Task AsyncTaskCall<T>(TClass value, Dictionary<string, IContainer<T>> container) where T : AspectContext
        {
            await Task.Delay(1);
        }


#if NETCOREAPP3_1_OR_GREATER
        public async ValueTask<T> AsyncValueTaskCall<T>(TClass value)
        {
            await Task.Delay(1);
            return default(T);
        }

        public async ValueTask AsyncValueTaskCall<T>(TClass value, T container)
        {
            await Task.Delay(1);
        }

        public async ValueTask AsyncValueTaskCall<T>(TClass value, IContainer<T> container)
        {
            await Task.Delay(1);
        }

        public async ValueTask AsyncValueTaskCall<T>(TClass value, Dictionary<string, IContainer<T>> container) where T : AspectContext
        {
            await Task.Delay(1);
        }
#endif





        public void CallWithRefParameter<T>(TClass value, ref T container)
            => DoNothing();

        public void CallWithRefParameter<T>(TClass value, ref IContainer<T> container)
            => DoNothing();

        public void CallWithRefParameter<T>(TClass value, ref Dictionary<string, IContainer<T>> container) where T : AspectContext
            => DoNothing();

        public void CallWithOutParameter<T>(TClass value, T parameter, out T container)
        {
            container = parameter;
            DoNothing();
        }

        public void CallWithOutParameter<T>(TClass value, IContainer<T> parameter, out IContainer<T> container)
        {
            container = parameter;
            DoNothing();
        }

        public void CallWithOutParameter<T>(TClass value, Dictionary<string, IContainer<T>> parameter, out Dictionary<string, IContainer<T>> container) where T : AspectContext
        {
            container = parameter;
            DoNothing();
        }

        private void DoNothing() { }

        private Task DoNothingAsync()
            => Task.CompletedTask;

#if NETCOREAPP3_1_OR_GREATER
        private ValueTask DoNothingValueAsync()
            => new ValueTask(Task.CompletedTask);
#endif
    }
}
