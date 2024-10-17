using AspectSharp.Abstractions.Attributes;
using AspectSharp.Tests.Core.Aspects;
using System.Threading.Tasks;

namespace AspectSharp.Tests.Core.Services.Interfaces
{
    [Aspect1]
    [TypeDefinitionAspect]
    public interface ISimpleGenericFakeService<TValue>
    {
        [PropertyAspect]
        [Aspect2]
        TValue Value
        {
            [Aspect2]
            get;
            [Aspect3]
            set;
        }

        [ExcludeAspectsFromTypeDefinition(typeof(TypeDefinitionAspectAttribute))]
        [IncludeAspectsFromTypeDefinition(typeof(Aspect1Attribute))]
        void SetValue(TValue value);

        [Aspect3]
        TValue GetValue();

        [Aspect1]
        [Aspect2]
        Task<TValue> WaitMethod(string param);

#if NETCOREAPP3_1_OR_GREATER
        [Aspect1]
        [Aspect2]
        ValueTask<TValue> ValueWaitMethod(string param);
#endif
    }
}
