using AspectSharp.Abstractions;
using AspectSharp.Tests.Core.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspectSharp.Tests.Core.Services
{
    internal class ServiceWithGenericMethod : IServiceWithGenericMethod
    {
        //public T Call<T>()
        //{
        //    DoNothing();
        //    return default;
        //}

        //public void Call<T>(T container)
        //    => DoNothing();

        //public void Call<T>(IContainer<T> container)
        //    => DoNothing();

        //public void Call<T>(Dictionary<string, IContainer<T>> container) where T : notnull, AspectContext
        //    => DoNothing();



        public Task<T> TaskCall<T>()
            => Task.FromResult(default(T));

        public Task TaskCall<T>(T container)
        {
            DoNothing();
            return Task.CompletedTask;
        }

        public Task TaskCall<T>(IContainer<T> container)
        {
            DoNothing();
            return Task.CompletedTask;
        }

        public Task TaskCall<T>(Dictionary<string, IContainer<T>> container) where T : notnull, AspectContext
        {
            DoNothing();
            return Task.CompletedTask;
        }






        public async Task<T> AsyncTaskCall<T>()
        {
            await Task.Delay(1);
            return default(T);
        }

        public async Task AsyncTaskCall<T>(T container)
        {
            await Task.Delay(1);
        }

        public async Task AsyncTaskCall<T>(IContainer<T> container)
        {
            await Task.Delay(1);
        }

        public async Task AsyncTaskCall<T>(Dictionary<string, IContainer<T>> container) where T : notnull, AspectContext
        {
            await Task.Delay(1);
        }





        //public void CallWithRefParameter<T>(ref T container)
        //    => DoNothing();

        //public void CallWithRefParameter<T>(ref IContainer<T> container)
        //    => DoNothing();

        //public void CallWithRefParameter<T>(ref Dictionary<string, IContainer<T>> container) where T : notnull, AspectContext
        //    => DoNothing();

        //public void CallWithOutParameter<T>(T parameter, out T container)
        //{
        //    container = parameter;
        //    DoNothing();
        //}

        //public void CallWithOutParameter<T>(IContainer<T> parameter, out IContainer<T> container)
        //{
        //    container = parameter;
        //    DoNothing();
        //}

        //public void CallWithOutParameter<T>(Dictionary<string, IContainer<T>> parameter, out Dictionary<string, IContainer<T>> container) where T : notnull, AspectContext
        //{
        //    container = parameter;
        //    DoNothing();
        //}

        private void DoNothing() { }

        private Task DoNothingAsync()
            => Task.CompletedTask;

#if NETCOREAPP3_1_OR_GREATER
        private ValueTask DoNothingValueAsync()
            => new ValueTask(Task.CompletedTask);
#endif
    }
}
