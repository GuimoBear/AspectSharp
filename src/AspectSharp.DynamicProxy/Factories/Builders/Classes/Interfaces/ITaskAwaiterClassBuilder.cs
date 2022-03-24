using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectSharp.DynamicProxy.Factories.Builders.Classes.Interfaces
{
    internal interface ITaskAwaiterClassBuilder : ITaskAwaiterClassFinalBuilder
    {
        ITaskAwaiterClassBuilder CalledFrom(MethodBuilder callerMethod, ParameterInfo[] callerParameters);
        IInstanceTaskAwaiterClassBuilder AsInstanceMethod();
        ITaskAwaiterParameterPreparerBuilder PrepareParameter(int parameterIndex);
        ITaskAwaiterClassBuilder WithAwaiter(Type awaitableType, AwaiterConfigurer awaiterConfigurerMethod);
        ITaskAwaiterClassBuilder HasLongAwaiterDefinition();
        ITaskAwaiterClassFinalBuilder WithAfterCompletedTask(TaskCompleted taskCompletedMethod);
        ITaskAwaiterClassFinalBuilder WithAfterCompletedTaskWithResult(TaskCompletedWithReturn taskCompletedMethod);
    }

    internal interface ITaskAwaiterClassFinalBuilder
    {
        FieldBuilder StateField { get; }
        FieldBuilder MethodBuilderField { get; }
        FieldBuilder[] ParameterFields { get; }

        Type Build();
    }

    internal interface ITaskAwaiterParameterPreparerBuilder
    {
        ITaskAwaiterClassBuilder WithParser(ParameterParser parserMethod);
    }

    internal delegate void ParameterParser(ILGenerator cil, TypeBuilder typeBuilder, FieldBuilder[] parameters, LocalBuilder localParameter);

    internal delegate void AwaiterConfigurer(ILGenerator cil, TypeBuilder typeBuilder, LocalBuilder[] localParameters);

    internal delegate void TaskCompleted(ILGenerator cil, TypeBuilder typeBuilder, FieldBuilder methodBuilderField, FieldBuilder[] callerParameters);
    internal delegate void TaskCompletedWithReturn(ILGenerator cil, TypeBuilder typeBuilder, FieldBuilder methodBuilderField, FieldBuilder[] callerParameters, LocalBuilder taskResultLocal);
}
