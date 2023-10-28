using AspectSharp.Abstractions;
using AspectSharp.Tests.Core.Services.Interfaces;
using System.Collections.Generic;

namespace AspectSharp.Tests.Core.Services
{
    internal class ServiceWithGenericMethod : IServiceWithGenericMethod
    {
        public T Call<T>()
        {
            DoNothing();
            return default;
        }

        public void Call<T>(T container)
            => DoNothing();

        public void Call<T>(IContainer<T> container)
            => DoNothing();

        public void Call<T>(Dictionary<string, IContainer<T>> container) where T : notnull, AspectContext
            => DoNothing();

        public void CallWithRefParameter<T>(ref T container)
            => DoNothing();

        public void CallWithRefParameter<T>(ref IContainer<T> container)
            => DoNothing();

        public void CallWithRefParameter<T>(ref Dictionary<string, IContainer<T>> container) where T : notnull, AspectContext
            => DoNothing();

        public void CallWithOutParameter<T>(T parameter, out T container)
        {
            container = parameter;
            DoNothing();
        }

        public void CallWithOutParameter<T>(IContainer<T> parameter, out IContainer<T> container)
        {
            container = parameter;
            DoNothing();
        }

        public void CallWithOutParameter<T>(Dictionary<string, IContainer<T>> parameter, out Dictionary<string, IContainer<T>> container) where T : notnull, AspectContext
        {
            container = parameter;
            DoNothing();
        }

        private void DoNothing() { }
    }
}
