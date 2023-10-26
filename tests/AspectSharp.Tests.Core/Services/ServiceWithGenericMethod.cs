using AspectSharp.Tests.Core.Services.Interfaces;

namespace AspectSharp.Tests.Core.Services
{
    internal class ServiceWithGenericMethod : IServiceWithGenericMethod
    {
        public void Call<T>(IContainer<T> container)
            => DoNothing();

        private void DoNothing() { }
    }
}
