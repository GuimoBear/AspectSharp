using AspectSharp.Abstractions.Attributes;
using AspectSharp.Abstractions.Enums;
using AspectSharp.Tests.Core.Aspects;
using System;

namespace AspectSharp.Tests.Core.Services.Interfaces
{
    [Aspect1]
    [TypeDefinitionAspect]
    public interface ISimpleFakeService
    {
        [ExcludeAspectsFromTypeDefinitionToThisEvent(InterceptedEventMethod.Add, typeof(TypeDefinitionAspectAttribute))]
        [IncludeAspectsFromTypeDefinitionToThisEvent(InterceptedEventMethod.Add, typeof(Aspect1Attribute))]
        event EventHandler OnEvent;

        [EventAspect]
        event EventHandler OnInterceptedEvent;

        [ExcludeAspectsFromTypeDefinition]
        event EventHandler OnEventWithAnyAspect;

        [IncludeAspectsFromTypeDefinition]
        event EventHandler OnEventWithAllAspect;

        [ExcludeAspectsFromTypeDefinitionToThisProperty(InterceptedPropertyMethod.Get, typeof(TypeDefinitionAspectAttribute))]
        [IncludeAspectsFromTypeDefinitionToThisProperty(InterceptedPropertyMethod.Get, typeof(Aspect1Attribute))]
        int Property { get; set; }

        [PropertyAspect]
        int InterceptedProperty 
        { 
            [Aspect2]
            get;
            [Aspect3]
            set; 
        }

        [ExcludeAspectsFromTypeDefinitionToThisProperty]
        int PropertyWithAnyAspect { get; set; }

        [IncludeAspectsFromTypeDefinitionToThisProperty]
        int PropertyWithAllAspect { get; set; }

        [ExcludeAspectsFromTypeDefinition(typeof(TypeDefinitionAspectAttribute))]
        [IncludeAspectsFromTypeDefinition(typeof(Aspect1Attribute))]
        void Method();

        [Aspect3]
        void InterceptedMethod();

        [ExcludeAspectsFromTypeDefinition]
        void MethodWithAnyAspect();
    }
}
