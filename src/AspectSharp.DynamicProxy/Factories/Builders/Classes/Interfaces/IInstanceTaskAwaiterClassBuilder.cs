using System;
using System.Reflection.Emit;

namespace AspectSharp.DynamicProxy.Factories.Builders.Classes.Interfaces
{
    internal interface IInstanceTaskAwaiterClassBuilder : IInstanceTaskAwaiterClassFinalBuilder
    {
        IInstanceTaskAwaiterParameterPreparerBuilder PrepareParameter(int parameterIndex);
        IInstanceTaskAwaiterClassBuilder WithAwaiter(Type awaitableType, InstanceAwaiterConfigurer awaiterConfigurerMethod);
        IInstanceTaskAwaiterClassBuilder HasLongAwaiterDefinition();
        IInstanceTaskAwaiterClassFinalBuilder WithAfterCompletedTask(TaskCompleted taskCompletedMethod);
        IInstanceTaskAwaiterClassFinalBuilder WithAfterCompletedTaskWithResult(TaskCompletedWithReturn taskCompletedMethod);
    }

    internal interface IInstanceTaskAwaiterClassFinalBuilder
    {
        FieldBuilder StateField { get; }
        FieldBuilder MethodBuilderField { get; }
        FieldBuilder ThisField { get; }
        FieldBuilder[] ParameterFields { get; }

        Type Build();
    }

    internal interface IInstanceTaskAwaiterParameterPreparerBuilder
    {
        IInstanceTaskAwaiterClassBuilder WithParser(InstanceParameterParser parserMethod);
    }

    internal delegate void InstanceParameterParser(ILGenerator cil, TypeBuilder typeBuilder, FieldBuilder thisField, FieldBuilder[] parameterFields, LocalBuilder localParameter);

    internal delegate void InstanceAwaiterConfigurer(ILGenerator cil, TypeBuilder typeBuilder, FieldBuilder thisField, LocalBuilder[] localParameters);
}
