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

        //public void Call<T>(T container)
        //    => DoNothing();

        //public void Call<T>(IContainer<T> container)
        //    => DoNothing();

        //public void Call<T>(Dictionary<string, IContainer<T>> container) where T : notnull, AspectContext
        //    => DoNothing();

        private void DoNothing() { }
    }
}
