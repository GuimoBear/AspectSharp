using AspectSharp.Tests.Core.Aspects;

namespace AspectSharp.Tests.Core.Services.Interfaces
{
    public interface IServiceWithGenericMethod
    {
        [Aspect3]
        void Call<T>(IContainer<T> container);
    }

    public interface IContainer<T>
    {
        T Body { get; }
    }
}
