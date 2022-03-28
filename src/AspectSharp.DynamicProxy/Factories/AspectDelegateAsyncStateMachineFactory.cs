using AspectSharp.Abstractions;
using AspectSharp.DynamicProxy.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AspectSharp.DynamicProxy.Factories
{
    internal static class AspectDelegateAsyncStateMachineFactory
    {
        internal static GeneratedAsyncStateMachine GenerateAsyncStateMachine(ModuleBuilder moduleBuilder, MethodInfo methodInfo, MethodBuilder callerMethod)
        {
            var typeName = string.Format("AspectSharp.AsyncStateMachines.AspectDelegates.{0}_{1}", callerMethod.DeclaringType.Name, callerMethod.Name);
            var attrs = TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit;

            var typeBuilder = moduleBuilder.DefineType(typeName, attrs, typeof(ValueType), _interfaces);

            var stateField = DefineStateField(typeBuilder);
            var builderField = DefineMethodBuilderField(typeBuilder, callerMethod);
            var contextField = DefineContextField(typeBuilder);
            var awaiterField = DefineAwaiterField(typeBuilder, methodInfo);
            FieldBuilder contextWrapField = default;
            if (methodInfo.ReturnType.IsGenericType)
                contextWrapField = DefineContextWrapField(typeBuilder);

            DefineMoveNext(typeBuilder, methodInfo, stateField, builderField, contextField, contextWrapField, awaiterField);
            DefineSetStateMachine(typeBuilder, builderField);

            var awaiterType = typeBuilder.CreateType();

            return new GeneratedAsyncStateMachine(callerMethod, awaiterType, stateField, builderField, contextField);
        }

        private static FieldBuilder DefineStateField(TypeBuilder typeBuilder)
            => typeBuilder.DefineField("<>__state", _stateType, FieldAttributes.Public);

        private static FieldBuilder DefineMethodBuilderField(TypeBuilder typeBuilder, MethodInfo methodInfo)
            => typeBuilder.DefineField("<>___builder", GetAsyncMethodBuilderType(methodInfo.ReturnType), FieldAttributes.Public);

        private static FieldBuilder DefineContextField(TypeBuilder typeBuilder)
            => typeBuilder.DefineField("<>___context", _contextType, FieldAttributes.Public);

        private static FieldBuilder DefineContextWrapField(TypeBuilder typeBuilder)
            => typeBuilder.DefineField("<>___contextWrap", _contextType, FieldAttributes.Private);

        private static FieldBuilder DefineAwaiterField(TypeBuilder typeBuilder, MethodInfo methodInfo)
            => typeBuilder.DefineField("<>__awaiter", GetTaskAwaiterType(methodInfo.ReturnType), FieldAttributes.Private);

        private static MethodBuilder DefineMoveNext(TypeBuilder typeBuilder, MethodInfo methodInfo, FieldBuilder stateField, FieldBuilder builderField, FieldBuilder contextField, FieldBuilder contextWrapField, FieldBuilder awaiterField)
        {
            var prepareAwaiterMethod = DefinePrepareAwaiter(typeBuilder, methodInfo, contextField, contextWrapField);
            var awaitOnCompletedMethod = DefineAwaitOnCompleted(typeBuilder, methodInfo, stateField, awaiterField, builderField);
            var afterCompletionMethod = DefineAfterCompletionMethod(typeBuilder, methodInfo, contextWrapField);

            var awaiterType = GetTaskAwaiterType(methodInfo.ReturnType);

            var attrs = MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final;
            var callingConventions = CallingConventions.Standard | CallingConventions.HasThis;
            var methodBuilder = typeBuilder.DefineMethod(string.Format("{0}.{1}", nameof(IAsyncStateMachine), nameof(IAsyncStateMachine.MoveNext)), attrs, callingConventions);

            var cil = methodBuilder.GetILGenerator();

            var localState = cil.DeclareLocal(_stateType);
            var localAwaiter = cil.DeclareLocal(awaiterField.FieldType);
            var localException = cil.DeclareLocal(_exceptionType);

            var stateEqualsZeroLabel = cil.DefineLabel();
            var afterElseLabel = cil.DefineLabel();
            var endOfMethodLabel = cil.DefineLabel();
            var afterCatchLabel = cil.DefineLabel();

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldfld, stateField);
            cil.Emit(OpCodes.Stloc, localState.LocalIndex);

            cil.BeginExceptionBlock();

            cil.Emit(OpCodes.Ldloc, localState.LocalIndex);
            cil.Emit(OpCodes.Brfalse_S, stateEqualsZeroLabel);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Call, prepareAwaiterMethod);
            cil.Emit(OpCodes.Stloc_S, localAwaiter.LocalIndex);

            cil.Emit(OpCodes.Ldloca_S, localAwaiter.LocalIndex);
            cil.Emit(OpCodes.Call, localAwaiter.LocalType.GetProperty(nameof(TaskAwaiter.IsCompleted)).GetGetMethod());
            cil.Emit(OpCodes.Brtrue_S, afterElseLabel);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldloca_S, localAwaiter.LocalIndex);
            cil.Emit(OpCodes.Call, awaitOnCompletedMethod);

            cil.Emit(OpCodes.Leave_S, endOfMethodLabel);

            cil.MarkLabel(stateEqualsZeroLabel);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldfld, awaiterField);
            cil.Emit(OpCodes.Stloc, localAwaiter.LocalIndex);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldflda, awaiterField);
            cil.Emit(OpCodes.Initobj, awaiterField.FieldType);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldc_I4_M1);
            cil.Emit(OpCodes.Stfld, stateField);

            cil.MarkLabel(afterElseLabel);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldloca_S, localAwaiter.LocalIndex);
            cil.Emit(OpCodes.Call, afterCompletionMethod);

            cil.Emit(OpCodes.Leave_S, afterCatchLabel);

            cil.BeginCatchBlock(_exceptionType);
            cil.Emit(OpCodes.Stloc, localException.LocalIndex);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldflda, builderField);
            cil.Emit(OpCodes.Ldloc, localException.LocalIndex);
            cil.Emit(OpCodes.Call, GetSetExceptionOnAsyncMethodBuilderMethod(builderField));

            cil.Emit(OpCodes.Leave_S, endOfMethodLabel);
            cil.EndExceptionBlock();
            cil.MarkLabel(afterCatchLabel);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldflda, builderField);
            cil.Emit(OpCodes.Call, GetSetResultOnAsyncMethodBuilderMethod(builderField));

            cil.MarkLabel(endOfMethodLabel);
            cil.Emit(OpCodes.Ret);

            typeBuilder.DefineMethodOverride(methodBuilder, _moveNextStateMachineMethod);

            return methodBuilder;
        }

        private static MethodBuilder DefineSetStateMachine(TypeBuilder typeBuilder, FieldBuilder builderField)
        {
            var parameterTypes = new Type[] { typeof(IAsyncStateMachine) };

            var attrs = MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final;
            var callingConventions = CallingConventions.Standard | CallingConventions.HasThis;
            var methodBuilder = typeBuilder.DefineMethod(string.Format("{0}.{1}", nameof(IAsyncStateMachine), nameof(IAsyncStateMachine.SetStateMachine)), attrs, callingConventions, null, parameterTypes);
            methodBuilder.DefineParameter(1, ParameterAttributes.None, "stateMachine");

            var attributeConstructor = typeof(DebuggerHiddenAttribute).GetConstructor(Type.EmptyTypes);
            var customAttribute = new CustomAttributeBuilder(attributeConstructor, new object[0]);
            methodBuilder.SetCustomAttribute(customAttribute);

            var cil = methodBuilder.GetILGenerator();

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldflda, builderField);
            cil.Emit(OpCodes.Ldarg_1);
            cil.Emit(OpCodes.Call, GetSetStateMachineOnAsyncMethodBuilderMethod(builderField));
            cil.Emit(OpCodes.Ret);

            typeBuilder.DefineMethodOverride(methodBuilder, _setStateMachineMethod);

            var publicMethodBuilder = typeBuilder.DefineMethod(nameof(IAsyncStateMachine.SetStateMachine), attrs, callingConventions, null, parameterTypes);
            publicMethodBuilder.DefineParameter(1, ParameterAttributes.None, "stateMachine");
            var publicCil = publicMethodBuilder.GetILGenerator();

            publicCil.Emit(OpCodes.Ldarg_0);
            publicCil.Emit(OpCodes.Call, methodBuilder);
            publicCil.Emit(OpCodes.Ret);

            return methodBuilder;
        }

        private static MethodBuilder DefinePrepareAwaiter(TypeBuilder typeBuilder, MethodInfo methodInfo, FieldBuilder contextField, FieldBuilder contextWrapField)
        {
            var returnInfo = methodInfo.GetReturnInfo();
            var methodParameters = methodInfo.GetParameters();

            var taskAwaiterType = GetTaskAwaiterType(methodInfo.ReturnType);

            var attrs = MethodAttributes.Private | MethodAttributes.HideBySig;

            var methodBuilder = typeBuilder.DefineMethod("PrepareAwaiter", attrs, CallingConventions.Standard | CallingConventions.HasThis, taskAwaiterType, Type.EmptyTypes);

            var cil = methodBuilder.GetILGenerator();

            var hasParameters = methodParameters.Length > 0;
            var parameters = new List<LocalBuilder>();
            if (hasParameters)
                foreach (var parameter in methodParameters)
                    parameters.Add(cil.DeclareLocal(parameter.ParameterType));
            LocalBuilder localValueTask = default;
#if NETCOREAPP3_1_OR_GREATER
        if (returnInfo.IsValueTask)
            localValueTask = cil.DeclareLocal(methodInfo.ReturnType);
#endif

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldfld, contextField);
            cil.Emit(OpCodes.Callvirt, _getTargetMethodInfo);
            cil.Emit(OpCodes.Isinst, methodInfo.DeclaringType);
            foreach (var i in Enumerable.Range(0, parameters.Count))
            {
                var param = parameters[i];

                cil.Emit(OpCodes.Ldarg_0);
                cil.Emit(OpCodes.Ldfld, contextField);
                cil.Emit(OpCodes.Callvirt, _getParametersMethodInfo);
                cil.Emit(OpCodes.Ldc_I4, i);
                cil.Emit(OpCodes.Ldelem_Ref);
                if (param.LocalType.IsValueType)
                    cil.Emit(OpCodes.Unbox_Any, param.LocalType);
                else
                    cil.Emit(OpCodes.Castclass, param.LocalType);
                cil.Emit(OpCodes.Stloc, param.LocalIndex);
            }

            if (methodInfo.ReturnType.IsGenericType)
            {
                cil.Emit(OpCodes.Ldarg_0);
                cil.Emit(OpCodes.Ldarg_0);
                cil.Emit(OpCodes.Ldfld, contextField);
                cil.Emit(OpCodes.Stfld, contextWrapField);
            }

            foreach (var parameter in parameters)
                cil.Emit(OpCodes.Ldloc, parameter.LocalIndex);
            cil.Emit(OpCodes.Callvirt, methodInfo);

#if NETCOREAPP3_1_OR_GREATER
        if (returnInfo.IsValueTask)
        {
            cil.Emit(OpCodes.Stloc_S, localValueTask.LocalIndex);
            cil.Emit(OpCodes.Ldloca_S, localValueTask.LocalIndex);
            cil.Emit(OpCodes.Call, methodInfo.ReturnType.GetMethod(nameof(Task.GetAwaiter), Type.EmptyTypes));
        }
        else
#endif
            cil.Emit(OpCodes.Callvirt, methodInfo.ReturnType.GetMethod(nameof(Task.GetAwaiter), Type.EmptyTypes));
            cil.Emit(OpCodes.Ret);
            return methodBuilder;
        }

        private static MethodBuilder DefineAwaitOnCompleted(TypeBuilder typeBuilder, MethodInfo methodInfo, FieldBuilder stateField, FieldBuilder awaiterField, FieldBuilder builderField)
        {
            var taskAwaiterType = GetTaskAwaiterType(methodInfo.ReturnType);
            var taskAwaiterByRefType = taskAwaiterType.MakeByRefType();

            var attrs = MethodAttributes.Private | MethodAttributes.HideBySig;

            var methodBuilder = typeBuilder.DefineMethod("AwaitOnCompleted", attrs, CallingConventions.Standard | CallingConventions.HasThis, null, new Type[] { taskAwaiterByRefType });
            methodBuilder.DefineParameter(1, ParameterAttributes.None, "awaiter");
            var cil = methodBuilder.GetILGenerator();

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldc_I4_0);
            cil.Emit(OpCodes.Stfld, stateField);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldarg_1);
            cil.Emit(OpCodes.Ldobj, awaiterField.FieldType);
            cil.Emit(OpCodes.Stfld, awaiterField);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldflda, builderField);
            cil.Emit(OpCodes.Ldarg_1);
            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Call, GetAwaitUnsafeOnCompletedOnAsyncMethodBuilderMethod(typeBuilder, builderField, awaiterField));

            cil.Emit(OpCodes.Ret);
            return methodBuilder;
        }

        private static MethodBuilder DefineAfterCompletionMethod(TypeBuilder typeBuilder, MethodInfo methodInfo, FieldBuilder contextWrapField)
        {
            var taskAwaiterType = GetTaskAwaiterType(methodInfo.ReturnType);
            var taskAwaiterByRefType = taskAwaiterType.MakeByRefType();

            var attrs = MethodAttributes.Private | MethodAttributes.Virtual;

            var methodBuilder = typeBuilder.DefineMethod("AfterCompletion", attrs, CallingConventions.Standard | CallingConventions.HasThis, null, new Type[] { taskAwaiterByRefType });
            var awaiterParameter = methodBuilder.DefineParameter(1, ParameterAttributes.None, "awaiter");

            var cil = methodBuilder.GetILGenerator();

            LocalBuilder localResult = default;
            if (methodInfo.ReturnType.IsGenericType)
                localResult = cil.DeclareLocal(methodInfo.ReturnType.GetGenericArguments()[0]);

            cil.Emit(OpCodes.Ldarg, awaiterParameter.Position);
            cil.Emit(OpCodes.Call, taskAwaiterType.GetMethod(nameof(TaskAwaiter.GetResult), Type.EmptyTypes));

            if (methodInfo.ReturnType.IsGenericType)
            {
                var parameterType = methodInfo.ReturnType.GetGenericArguments()[0];
                cil.Emit(OpCodes.Stloc, localResult.LocalIndex);
                cil.Emit(OpCodes.Ldarg_0);
                cil.Emit(OpCodes.Ldfld, contextWrapField);
                cil.Emit(OpCodes.Ldloc, localResult.LocalIndex);
                if (parameterType.IsValueType)
                    cil.Emit(OpCodes.Box, parameterType);
                cil.Emit(OpCodes.Callvirt, _setResultInContextMethodInfo);

                cil.Emit(OpCodes.Ldarg_0);
                cil.Emit(OpCodes.Ldnull);
                cil.Emit(OpCodes.Stfld, contextWrapField);
            }
            cil.Emit(OpCodes.Ret);

            return methodBuilder;
        }

        private static Type GetTaskAwaiterType(Type returnType)
        {
            if ((returnType.IsGenericType && (returnType.GetGenericTypeDefinition() == typeof(Task<>)
#if NETCOREAPP3_1_OR_GREATER
                || returnType.GetGenericTypeDefinition() == typeof(ValueTask<>)
#endif
            )) || returnType == typeof(Task)
#if NETCOREAPP3_1_OR_GREATER
            || returnType == typeof(ValueTask)
#endif
            )
            {
                if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
                    return typeof(TaskAwaiter<>).MakeGenericType(returnType.GetGenericArguments());
#if NETCOREAPP3_1_OR_GREATER
                else if (returnType == typeof(ValueTask))
                    return typeof(ValueTaskAwaiter);
                else if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
                    return typeof(ValueTaskAwaiter<>).MakeGenericType(returnType.GetGenericArguments());
#endif
            }
            return typeof(TaskAwaiter);
        }

        private static Type GetAsyncMethodBuilderType(Type returnType)
        {
            if ((returnType.IsGenericType && (returnType.GetGenericTypeDefinition() == typeof(Task<>)
#if NETCOREAPP3_1_OR_GREATER
                || returnType.GetGenericTypeDefinition() == typeof(ValueTask<>)
#endif
            )) || returnType == typeof(Task)
#if NETCOREAPP3_1_OR_GREATER
            || returnType == typeof(ValueTask)
#endif
            )
            {
                if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
                    return typeof(AsyncTaskMethodBuilder<>).MakeGenericType(returnType.GetGenericArguments());
#if NETCOREAPP3_1_OR_GREATER
                else if (returnType == typeof(ValueTask))
                    return typeof(AsyncValueTaskMethodBuilder);
                else if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
                    return typeof(AsyncValueTaskMethodBuilder<>).MakeGenericType(returnType.GetGenericArguments());
#endif
            }
            return typeof(AsyncTaskMethodBuilder);
        }

        private static MethodInfo GetAwaitUnsafeOnCompletedOnAsyncMethodBuilderMethod(TypeBuilder typeBuilder, FieldBuilder builderField, FieldBuilder awaiterField)
            => builderField.FieldType.GetMethod(nameof(AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted)).MakeGenericMethod(awaiterField.FieldType, typeBuilder);

        private static MethodInfo GetSetResultOnAsyncMethodBuilderMethod(FieldBuilder builderField)
        {
            if (builderField.FieldType.IsGenericType)
                return builderField.FieldType.GetMethod(nameof(AsyncTaskMethodBuilder.SetResult), builderField.FieldType.GetGenericArguments());
            return builderField.FieldType.GetMethod(nameof(AsyncTaskMethodBuilder.SetResult), Type.EmptyTypes);
        }

        private static MethodInfo GetSetExceptionOnAsyncMethodBuilderMethod(FieldBuilder builderField)
            => builderField.FieldType.GetMethod(nameof(AsyncTaskMethodBuilder.SetException));

        private static MethodInfo GetCreateMethodBuilderMethod(FieldBuilder builderField)
            => builderField.FieldType.GetMethod(nameof(AsyncTaskMethodBuilder.Create));

        private static MethodInfo GetStartMethodBuilderGenericMethod(FieldBuilder builderField, Type stateMachineType)
            => builderField.FieldType.GetMethod(nameof(AsyncTaskMethodBuilder.Start)).MakeGenericMethod(stateMachineType);

        private static MethodInfo GetTaskGetterOnAsyncMethodBuilderMethod(FieldBuilder builderField)
            => builderField.FieldType.GetProperty(nameof(AsyncTaskMethodBuilder.Task)).GetGetMethod();

        private static MethodInfo GetSetStateMachineOnAsyncMethodBuilderMethod(FieldBuilder builderField)
            => builderField.FieldType.GetMethod(nameof(AsyncTaskMethodBuilder.SetStateMachine));

        internal class GeneratedAsyncStateMachine
        {
            private readonly MethodBuilder _callerMethod;

            public Type Type { get; }
            public FieldBuilder StateField { get; }
            public FieldBuilder BuilderField { get; }
            public FieldBuilder ContextField { get; }

            public GeneratedAsyncStateMachine(MethodBuilder callerMethod, Type type, FieldBuilder stateField, FieldBuilder builderField, FieldBuilder contextField)
            {
                _callerMethod = callerMethod;
                Type = type;
                StateField = stateField;
                BuilderField = builderField;
                ContextField = contextField;
            }

            public void WriteCallerMethod()
            {
                var attributeConstructor = typeof(AsyncStateMachineAttribute).GetConstructor(new Type[] { typeof(Type) });
                var customAttribute = new CustomAttributeBuilder(attributeConstructor, new object[] { Type });
                _callerMethod.SetCustomAttribute(customAttribute);

                var cil = _callerMethod.GetILGenerator();
                var localAwaiterStateMachine = cil.DeclareLocal(Type);

                cil.Emit(OpCodes.Ldloca_S, localAwaiterStateMachine.LocalIndex);
                cil.Emit(OpCodes.Call, GetCreateMethodBuilderMethod(BuilderField));
                cil.Emit(OpCodes.Stfld, BuilderField);

                cil.Emit(OpCodes.Ldloca_S, localAwaiterStateMachine.LocalIndex);
                cil.Emit(OpCodes.Ldarg_0);
                cil.Emit(OpCodes.Stfld, ContextField);

                cil.Emit(OpCodes.Ldloca_S, localAwaiterStateMachine.LocalIndex);
                cil.Emit(OpCodes.Ldc_I4_M1);
                cil.Emit(OpCodes.Stfld, StateField);

                cil.Emit(OpCodes.Ldloca_S, localAwaiterStateMachine.LocalIndex);
                cil.Emit(OpCodes.Ldflda, BuilderField);
                cil.Emit(OpCodes.Ldloca_S, localAwaiterStateMachine.LocalIndex);
                cil.Emit(OpCodes.Call, GetStartMethodBuilderGenericMethod(BuilderField, Type));

                cil.Emit(OpCodes.Ldloca_S, localAwaiterStateMachine.LocalIndex);
                cil.Emit(OpCodes.Ldflda, BuilderField);
                cil.Emit(OpCodes.Call, GetTaskGetterOnAsyncMethodBuilderMethod(BuilderField));
                cil.Emit(OpCodes.Ret);
            }
        }

        private static readonly Type _valueType = typeof(ValueType);

        private static readonly Type[] _interfaces = new Type[] { typeof(IAsyncStateMachine) };
        private static readonly MethodInfo _moveNextStateMachineMethod = typeof(IAsyncStateMachine).GetMethod(nameof(IAsyncStateMachine.MoveNext));
        private static readonly MethodInfo _setStateMachineMethod = typeof(IAsyncStateMachine).GetMethod(nameof(IAsyncStateMachine.SetStateMachine));

        private static readonly Type _stateType = typeof(int);

        private static readonly Type _exceptionType = typeof(Exception);
        private static readonly Type _contextType = typeof(AspectContext);

        private static readonly Type _taskAwaiterType = typeof(TaskAwaiter);
#if NETCOREAPP3_1_OR_GREATER
        private static readonly Type _valueTaskAwaiterType = typeof(ValueTaskAwaiter);
#endif

        private static readonly Type _taskAwaiterGenericType = typeof(TaskAwaiter<>);
#if NETCOREAPP3_1_OR_GREATER
        private static readonly Type _valueTaskAwaiterGenericType = typeof(ValueTaskAwaiter<>);
#endif

        private static readonly MethodInfo _getTargetMethodInfo = typeof(AspectContext).GetProperty(nameof(AspectContext.Target)).GetGetMethod();
        private static readonly MethodInfo _getParametersMethodInfo = typeof(AspectContext).GetProperty(nameof(AspectContext.Parameters)).GetGetMethod();

        //private static readonly MethodInfo _setStateMachineOnBuilderMethod = _taskMethodBuilderType.GetMethod(nameof(AsyncTaskMethodBuilder.SetStateMachine));
        //private static readonly MethodInfo _setExceptionOnBuilderMethod = _taskMethodBuilderType.GetMethod(nameof(AsyncTaskMethodBuilder.SetException));

        //private static readonly MethodInfo _createBuilderMethod = _taskMethodBuilderType.GetMethod(nameof(AsyncTaskMethodBuilder.Create), Type.EmptyTypes);
        //private static readonly MethodInfo _startOnBuilderGenericMethod = _taskMethodBuilderType.GetMethod(nameof(AsyncTaskMethodBuilder.Start));
        //private static readonly MethodInfo _taskGetterOnBuilderGenericMethod = _taskMethodBuilderType.GetProperty(nameof(AsyncTaskMethodBuilder.Task)).GetGetMethod();

        private static readonly MethodInfo _setResultInContextMethodInfo = _contextType.GetProperty(nameof(AspectContext.ReturnValue)).GetSetMethod();
    }
}
