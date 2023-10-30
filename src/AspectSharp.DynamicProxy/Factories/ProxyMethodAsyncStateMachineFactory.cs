using AspectSharp.Abstractions;
using AspectSharp.Abstractions.Attributes;
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
    internal static class ProxyMethodAsyncStateMachineFactory
    {
        private static readonly IDictionary<Tuple<string, string, int>, Type> _cachedProxyTypes
            = new Dictionary<Tuple<string, string, int>, Type>();

        internal static GeneratedAsyncStateMachine GenerateAsyncStateMachine(
            ModuleBuilder moduleBuilder,
            TypeBuilder proxyType,
            Type targetType,
            PropertyInfo pipelineProperty,
            FieldInfo originContextActivatorField, 
            MethodInfo methodInfo,
            MethodBuilder callerMethod,
            Type customAspectContext, 
            ConstructorInfo contextConstructor)
        {
            var attrs = TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit;

            var previouslyDefinedProxyClassFromThisTargetCount = _cachedProxyTypes.Count(kvp => kvp.Key.Item1 == callerMethod.DeclaringType.Name && kvp.Key.Item2 == callerMethod.Name);
            TypeBuilder typeBuilder;
            if (previouslyDefinedProxyClassFromThisTargetCount == 0)
                typeBuilder = moduleBuilder.DefineType(string.Format("AspectSharp.AsyncStateMachines.ProxyMethods.{0}_{1}", callerMethod.DeclaringType.Name, callerMethod.Name), attrs, typeof(ValueType), _interfaces);
            else
                typeBuilder = moduleBuilder.DefineType(string.Format("AspectSharp.AsyncStateMachines.ProxyMethods.{0}_{1}{2}", callerMethod.DeclaringType.Name, callerMethod.Name, previouslyDefinedProxyClassFromThisTargetCount), attrs, typeof(ValueType), _interfaces);

            var genericParameterBuilders = Array.Empty<GenericTypeParameterBuilder>();

            var methodToCall = methodInfo;
            var methodParameters = methodToCall.GetParameters();

            if (methodToCall.IsGenericMethod)
            {
                var genericParameters = methodInfo.GetGenericArguments().Select(a => a.GetTypeInfo()).ToArray();

                genericParameterBuilders = typeBuilder.DefineGenericParameters(genericParameters.Select(ti => ti.Name).ToArray());

                for (int i = 0; i < genericParameters.Length; i++)
                {
                    var genericParameter = genericParameters[i];
                    var genericParameterBuilder = genericParameterBuilders[i];
                    genericParameterBuilder.SetGenericParameterAttributes(GenericParameterUtils.ToClassGenericParameterAttributes(genericParameter.GenericParameterAttributes));
                    foreach (var constraint in genericParameter.GetGenericParameterConstraints().Select(t => t.GetTypeInfo()))
                    {
                        if (constraint.IsClass) genericParameterBuilder.SetBaseTypeConstraint(constraint.AsType());
                        if (constraint.IsInterface) genericParameterBuilder.SetInterfaceConstraints(constraint.AsType());
                    }
                }

                methodToCall = methodInfo.IsGenericMethodDefinition
                    ? methodInfo.MakeGenericMethod(genericParameterBuilders)
                    : methodInfo.GetGenericMethodDefinition().MakeGenericMethod(genericParameterBuilders);

                customAspectContext = customAspectContext.MakeGenericType(genericParameterBuilders);
                contextConstructor = TypeBuilder.GetConstructor(customAspectContext, contextConstructor);
            }

            var stateField = DefineStateField(typeBuilder);
            var builderField = DefineMethodBuilderField(typeBuilder, methodToCall, genericParameterBuilders);
            var targetField = DefineTargetField(typeBuilder, targetType);
            var proxyField = DefineProxyField(typeBuilder, proxyType);
            var contextFactoryField = DefineContextFactoryField(typeBuilder);
            var parameterFields = DefineParametersField(typeBuilder, methodToCall, methodParameters, genericParameterBuilders);
            var contextField = DefineContextField(typeBuilder, customAspectContext);
            var awaiterField = DefineAwaiterField(typeBuilder);

            DefineMoveNext(typeBuilder, methodToCall, stateField, builderField, targetField, proxyField, originContextActivatorField, contextConstructor, contextFactoryField, contextField, parameterFields, pipelineProperty, awaiterField, genericParameterBuilders);
            DefineSetStateMachine(typeBuilder, builderField);

            var awaiterType = typeBuilder.CreateTypeInfo().AsType();

            _cachedProxyTypes.Add(new Tuple<string, string, int>(callerMethod.DeclaringType.Name, callerMethod.Name, awaiterType.GetHashCode()), awaiterType);

            return new GeneratedAsyncStateMachine(awaiterType, stateField, builderField, targetField, proxyField, contextFactoryField, parameterFields);
        }

        private static FieldBuilder DefineStateField(TypeBuilder typeBuilder)
            => typeBuilder.DefineField("<>__state", _stateType, FieldAttributes.Public);

        private static FieldBuilder DefineMethodBuilderField(TypeBuilder typeBuilder, MethodInfo methodInfo, GenericTypeParameterBuilder[] genericParameters)
            => typeBuilder.DefineField("<>___builder", GetAsyncMethodBuilderType(GenericParameterUtils.ReplaceTypeUsingGenericParameters(methodInfo.ReturnType, genericParameters)), FieldAttributes.Public);

        private static FieldBuilder DefineTargetField(TypeBuilder typeBuilder, Type serviceType)
            => typeBuilder.DefineField("<>__target", serviceType, FieldAttributes.Public);

        private static FieldBuilder DefineProxyField(TypeBuilder typeBuilder, TypeBuilder proxyType)
            => typeBuilder.DefineField("<>__proxy", proxyType, FieldAttributes.Public);

        private static FieldBuilder DefineContextFactoryField(TypeBuilder typeBuilder)
            => typeBuilder.DefineField("<>__contextFactory", typeof(IAspectContextFactory), FieldAttributes.Public);

        private static FieldBuilder[] DefineParametersField(TypeBuilder typeBuilder, MethodInfo methodInfo, ParameterInfo[] methodParameters, GenericTypeParameterBuilder[] genericParameters)
        {
            if (methodParameters.Length == 0)
                return Array.Empty<FieldBuilder>();
            var ret = new List<FieldBuilder>();
            foreach (var parameter in methodParameters)
                ret.Add(typeBuilder.DefineField(string.Format("<>__{0}", parameter.Name), GenericParameterUtils.ReplaceTypeUsingGenericParameters(parameter.ParameterType, genericParameters), FieldAttributes.Public));
            return ret.ToArray();
        }

        private static FieldBuilder DefineContextField(TypeBuilder typeBuilder, Type customAspectContext)
            => typeBuilder.DefineField("<>__context", customAspectContext, FieldAttributes.Private);

        private static FieldBuilder DefineAwaiterField(TypeBuilder typeBuilder)
            => typeBuilder.DefineField("<>__awaiter", _taskAwaiterType, FieldAttributes.Private);

        private static MethodBuilder DefineMoveNext(TypeBuilder typeBuilder, MethodInfo methodInfo, FieldBuilder stateField, FieldBuilder builderField, FieldBuilder targetField, FieldBuilder proxyField, FieldInfo originContextActivatorField, ConstructorInfo contextConstructor, FieldBuilder contextFactoryField, FieldBuilder contextField, FieldBuilder[] parameterFields, PropertyInfo pipelineProperty, FieldBuilder awaiterField, GenericTypeParameterBuilder[] genericParameters)
        {
            var returnInfo = methodInfo.GetReturnInfo();

            var prepareAwaiterMethod = DefinePrepareAwaiter(typeBuilder, targetField, proxyField, originContextActivatorField, contextConstructor, contextFactoryField, contextField, parameterFields, pipelineProperty, genericParameters);
            var awaitOnCompletedMethod = DefineAwaitOnCompleted(typeBuilder, stateField, awaiterField, builderField);
            var afterCompletionMethod = DefineAfterCompletionMethod(typeBuilder, methodInfo, contextField, genericParameters);

            var awaiterType = _taskAwaiterType;

            var attrs = MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final;
            var callingConventions = CallingConventions.Standard | CallingConventions.HasThis;
            var methodBuilder = typeBuilder.DefineMethod(string.Format("{0}.{1}", nameof(IAsyncStateMachine), nameof(IAsyncStateMachine.MoveNext)), attrs, callingConventions);

            var cil = methodBuilder.GetILGenerator();

            var localState = cil.DeclareLocal(_stateType);
            var localAwaiter = cil.DeclareLocal(awaiterField.FieldType);
            var localException = cil.DeclareLocal(_exceptionType);
            LocalBuilder localReturn = default;
            if (!returnInfo.IsVoid)
                localReturn = cil.DeclareLocal(GenericParameterUtils.ReplaceTypeUsingGenericParameters(returnInfo.Type, genericParameters));

            var stateEqualsZeroLabel = cil.DefineLabel();
            var afterElseLabel = cil.DefineLabel();
            var endOfMethodLabel = cil.DefineLabel();
            var afterCatchLabel = cil.DefineLabel();

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldfld, stateField);
            cil.Emit(OpCodes.Stloc, localState.LocalIndex);

            cil.BeginExceptionBlock();

            cil.Emit(OpCodes.Ldloc, localState.LocalIndex);
            cil.Emit(OpCodes.Brfalse, stateEqualsZeroLabel);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Call, prepareAwaiterMethod);
            cil.Emit(OpCodes.Stloc, localAwaiter.LocalIndex);

            cil.Emit(OpCodes.Ldloca_S, localAwaiter.LocalIndex);
            cil.Emit(OpCodes.Call, localAwaiter.LocalType.GetProperty(nameof(TaskAwaiter.IsCompleted)).GetGetMethod());
            cil.Emit(OpCodes.Brtrue_S, afterElseLabel);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldloca_S, localAwaiter.LocalIndex);
            cil.Emit(OpCodes.Call, awaitOnCompletedMethod);

            cil.Emit(OpCodes.Leave, endOfMethodLabel);

            cil.MarkLabel(stateEqualsZeroLabel);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldfld, awaiterField);
            cil.Emit(OpCodes.Stloc, localAwaiter.LocalIndex);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldflda, awaiterField);
            cil.Emit(OpCodes.Initobj, awaiterField.FieldType);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldc_I4_M1);
            cil.Emit(OpCodes.Dup);
            cil.Emit(OpCodes.Stloc, localState.LocalIndex);
            cil.Emit(OpCodes.Stfld, stateField);

            cil.MarkLabel(afterElseLabel);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldloca_S, localAwaiter.LocalIndex);
            cil.Emit(OpCodes.Call, afterCompletionMethod);
            if (!returnInfo.IsVoid)
                cil.Emit(OpCodes.Stloc, localReturn.LocalIndex);

            cil.Emit(OpCodes.Leave_S, afterCatchLabel);

            cil.BeginCatchBlock(_exceptionType);
            cil.Emit(OpCodes.Stloc, localException.LocalIndex);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldnull);
            cil.Emit(OpCodes.Stfld, contextField);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldflda, builderField);
            cil.Emit(OpCodes.Ldloc_S, localException.LocalIndex);
            cil.Emit(OpCodes.Call, GetSetExceptionOnAsyncMethodBuilderMethod(builderField));

            cil.Emit(OpCodes.Leave_S, endOfMethodLabel);
            cil.EndExceptionBlock();
            cil.MarkLabel(afterCatchLabel);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldnull);
            cil.Emit(OpCodes.Stfld, contextField);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldflda, builderField);
            if (!returnInfo.IsVoid)
                cil.Emit(OpCodes.Ldloc, localReturn.LocalIndex);
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

        private static MethodBuilder DefinePrepareAwaiter(TypeBuilder typeBuilder, FieldBuilder targetField, FieldBuilder proxyField, FieldInfo originContextActivatorField, ConstructorInfo contextConstructor, FieldBuilder contextFactoryField, FieldBuilder contextField, FieldBuilder[] parameterFields, PropertyInfo pipelineProperty, GenericTypeParameterBuilder[] genericParameters)
        {
            var attrs = MethodAttributes.Private | MethodAttributes.HideBySig;

            var methodBuilder = typeBuilder.DefineMethod("PrepareAwaiter", attrs, CallingConventions.Standard | CallingConventions.HasThis, _taskAwaiterType, Type.EmptyTypes);

            var cil = methodBuilder.GetILGenerator();

            LocalBuilder localParameterArray = default;
            if (parameterFields.Length > 0)
            {
                localParameterArray = cil.DeclareLocal(typeof(object[]));
                cil.Emit(OpCodes.Ldc_I4, parameterFields.Length);
                cil.Emit(OpCodes.Newarr, typeof(object));
                foreach (var tuple in parameterFields.Zip(Enumerable.Range(0, parameterFields.Length), (first, second) => new Tuple<FieldBuilder, int>(first, second)))
                {
                    var parameter = tuple.Item1;
                    var index = tuple.Item2;
                    cil.Emit(OpCodes.Dup, localParameterArray.LocalIndex);
                    cil.Emit(OpCodes.Ldc_I4, index);
                    cil.Emit(OpCodes.Ldarg_0);
                    cil.Emit(OpCodes.Ldfld, parameter);
                    if (parameter.FieldType.IsValueType || parameter.FieldType.ContainsGenericParameters)
                        cil.Emit(OpCodes.Box, parameter.FieldType);
                    cil.Emit(OpCodes.Stelem_Ref);
                }
                cil.Emit(OpCodes.Stloc, localParameterArray.LocalIndex);
            }

            cil.Emit(OpCodes.Ldarg_0);
            //cil.Emit(OpCodes.Ldarg_0);
            //cil.Emit(OpCodes.Ldfld, contextFactoryField);
            cil.Emit(OpCodes.Ldsfld, originContextActivatorField);
            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldfld, targetField);
            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldfld, proxyField);
            if (!(localParameterArray is null))
                cil.Emit(OpCodes.Ldloc, localParameterArray.LocalIndex);
            else
                cil.Emit(OpCodes.Ldsfld, _emptyParameterArrayFieldInfo);
            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldfld, contextFactoryField);
            cil.Emit(OpCodes.Callvirt, _getServicesValueMethodInfo);
            cil.Emit(OpCodes.Newobj, contextConstructor);
            //cil.Emit(OpCodes.Callvirt, _createContextMethodInfo);
            cil.Emit(OpCodes.Stfld, contextField);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldfld, contextFactoryField);
            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldfld, contextField);
            cil.Emit(OpCodes.Callvirt, _setLastContextMethodInfo);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldfld, contextField);
            cil.Emit(OpCodes.Call, pipelineProperty.GetMethod);
            cil.Emit(OpCodes.Call, _executePipelineMethodInfo);
            cil.Emit(OpCodes.Callvirt, _getAwaiterMethodInfo);
            cil.Emit(OpCodes.Ret);

            return methodBuilder;
        }

        private static MethodBuilder DefineAwaitOnCompleted(TypeBuilder typeBuilder, FieldBuilder stateField, FieldBuilder awaiterField, FieldBuilder builderField)
        {
            var taskAwaiterByRefType = _taskAwaiterType.MakeByRefType();

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

        private static MethodBuilder DefineAfterCompletionMethod(TypeBuilder typeBuilder, MethodInfo methodInfo, FieldBuilder contextField, GenericTypeParameterBuilder[] genericParameters)
        {
            var retInfo = methodInfo.GetReturnInfo();
            var taskAwaiterByRefType = _taskAwaiterType.MakeByRefType();

            var attrs = MethodAttributes.Private | MethodAttributes.Virtual;
            Type returnType = default;
            if (!retInfo.IsVoid)
                returnType = GenericParameterUtils.ReplaceTypeUsingGenericParameters(retInfo.Type, genericParameters);
            var methodBuilder = typeBuilder.DefineMethod("AfterCompletion", attrs, CallingConventions.Standard | CallingConventions.HasThis, returnType, new Type[] { taskAwaiterByRefType });
            var awaiterParameter = methodBuilder.DefineParameter(1, ParameterAttributes.None, "awaiter");

            var cil = methodBuilder.GetILGenerator();

            cil.Emit(OpCodes.Ldarg, awaiterParameter.Position);
            cil.Emit(OpCodes.Call, _taskAwaiterType.GetMethod(nameof(TaskAwaiter.GetResult), Type.EmptyTypes));

            if (!retInfo.IsVoid)
            {
                cil.Emit(OpCodes.Ldarg_0);
                cil.Emit(OpCodes.Ldfld, contextField);
                cil.Emit(OpCodes.Callvirt, _getReturnValueMethodInfo);
                if (retInfo.Type.IsValueType || retInfo.Type.ContainsGenericParameters)
                    cil.Emit(OpCodes.Unbox_Any, returnType);
                else
                    cil.Emit(OpCodes.Castclass, returnType);
            }

            cil.Emit(OpCodes.Ret);

            return methodBuilder;
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
            => builderField.FieldType.ContainsGenericParameters 
                ? TypeBuilder.GetMethod(builderField.FieldType, typeof(AsyncTaskMethodBuilder<>).GetMethod(nameof(AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted))) 
                : builderField.FieldType.GetMethod(nameof(AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted)).MakeGenericMethod(awaiterField.FieldType, typeBuilder);

        private static MethodInfo GetSetResultOnAsyncMethodBuilderMethod(FieldBuilder builderField)
        {
            if (builderField.FieldType.IsGenericType)
            {
                return builderField.FieldType.ContainsGenericParameters 
                    ? TypeBuilder.GetMethod(builderField.FieldType, typeof(AsyncTaskMethodBuilder<>).GetMethods().First(mi => mi.Name == nameof(AsyncTaskMethodBuilder.SetResult) && mi.GetParameters().Count() == 1))
                    : builderField.FieldType.GetMethod(nameof(AsyncTaskMethodBuilder.SetResult), builderField.FieldType.GetGenericArguments());
            }
            return builderField.FieldType.ContainsGenericParameters
                ? TypeBuilder.GetMethod(builderField.FieldType, typeof(AsyncTaskMethodBuilder<>).GetMethods().First(mi => mi.Name == nameof(AsyncTaskMethodBuilder.SetResult) && mi.GetParameters().Count() == 0))
                : builderField.FieldType.GetMethod(nameof(AsyncTaskMethodBuilder.SetResult), Type.EmptyTypes);
        }

        private static MethodInfo GetSetExceptionOnAsyncMethodBuilderMethod(FieldBuilder builderField)
            => builderField.FieldType.ContainsGenericParameters 
                ? TypeBuilder.GetMethod(builderField.FieldType, typeof(AsyncTaskMethodBuilder<>).GetMethod(nameof(AsyncTaskMethodBuilder.SetException)))
                : builderField.FieldType.GetMethod(nameof(AsyncTaskMethodBuilder.SetException));

        private static MethodInfo GetCreateMethodBuilderMethod(FieldBuilder builderField)
            => builderField.FieldType.ContainsGenericParameters
                ? TypeBuilder.GetMethod(builderField.FieldType, typeof(AsyncTaskMethodBuilder<>).GetMethod(nameof(AsyncTaskMethodBuilder.Create)))
                : builderField.FieldType.GetMethod(nameof(AsyncTaskMethodBuilder.Create));

        private static MethodInfo GetStartMethodBuilderGenericMethod(FieldBuilder builderField, Type stateMachineType)
            => builderField.FieldType.ContainsGenericParameters
                ? TypeBuilder.GetMethod(builderField.FieldType, typeof(AsyncTaskMethodBuilder<>).GetMethod(nameof(AsyncTaskMethodBuilder.Start))).MakeGenericMethod(stateMachineType)
                : builderField.FieldType.GetMethod(nameof(AsyncTaskMethodBuilder.Start)).MakeGenericMethod(stateMachineType);

        private static MethodInfo GetTaskGetterOnAsyncMethodBuilderMethod(FieldBuilder builderField)
            => builderField.FieldType.ContainsGenericParameters
                ? TypeBuilder.GetMethod(builderField.FieldType, typeof(AsyncTaskMethodBuilder<>).GetProperty(nameof(AsyncTaskMethodBuilder.Task)).GetGetMethod())
                : builderField.FieldType.GetProperty(nameof(AsyncTaskMethodBuilder.Task)).GetGetMethod();

        private static MethodInfo GetSetStateMachineOnAsyncMethodBuilderMethod(FieldBuilder builderField)
            => builderField.FieldType.ContainsGenericParameters
                ? TypeBuilder.GetMethod(builderField.FieldType, typeof(AsyncTaskMethodBuilder<>).GetMethod(nameof(AsyncTaskMethodBuilder.SetStateMachine)))
                : builderField.FieldType.GetMethod(nameof(AsyncTaskMethodBuilder.SetStateMachine));

        internal class GeneratedAsyncStateMachine
        {
            public Type Type { get; }
            public FieldBuilder StateField { get; }
            public FieldBuilder BuilderField { get; }
            public FieldBuilder TargetField { get; }
            public FieldBuilder ProxyField { get; }
            public FieldBuilder ContextFactoryField { get; }
            public FieldBuilder[] ParameterFields { get; }

            public GeneratedAsyncStateMachine(
                Type type, 
                FieldBuilder stateField, 
                FieldBuilder builderField, 
                FieldBuilder targetField,
                FieldBuilder proxyField,
                FieldBuilder contextFactoryField,
                FieldBuilder[] parameterFields)
            {
                Type = type;
                StateField = stateField;
                BuilderField = builderField;
                TargetField = targetField;
                ProxyField = proxyField;
                ContextFactoryField = contextFactoryField;
                ParameterFields = parameterFields;
            }

            public void WriteCallerMethod(MethodBuilder callerMethod, GenericTypeParameterBuilder[] callerMethodGenericParameters, FieldInfo targetField, FieldInfo contextFactoryField)
            {
                var attributeConstructor = typeof(AsyncStateMachineAttribute).GetConstructor(new Type[] { typeof(Type) });
                var customAttribute = new CustomAttributeBuilder(attributeConstructor, new object[] { Type.IsGenericType && !Type.IsGenericTypeDefinition ? Type.GetGenericTypeDefinition() : Type });
                callerMethod.SetCustomAttribute(customAttribute);

                var cil = callerMethod.GetILGenerator();
                var localAwaiterStateMachine = cil.DeclareLocal(GenericParameterUtils.ReplaceTypeUsingGenericParameters(Type, callerMethodGenericParameters));

                cil.Emit(OpCodes.Ldloca_S, localAwaiterStateMachine.LocalIndex);
                cil.Emit(OpCodes.Ldc_I4_M1);
                cil.Emit(OpCodes.Stfld, StateField);

                cil.Emit(OpCodes.Ldloca_S, localAwaiterStateMachine.LocalIndex);
                cil.Emit(OpCodes.Call, GetCreateMethodBuilderMethod(BuilderField));
                cil.Emit(OpCodes.Stfld, BuilderField);

                cil.Emit(OpCodes.Ldloca_S, localAwaiterStateMachine.LocalIndex);
                cil.Emit(OpCodes.Ldarg_0);
                cil.Emit(OpCodes.Ldfld, contextFactoryField);
                cil.Emit(OpCodes.Stfld, ContextFactoryField);

                cil.Emit(OpCodes.Ldloca_S, localAwaiterStateMachine.LocalIndex);
                cil.Emit(OpCodes.Ldarg_0);
                cil.Emit(OpCodes.Ldfld, targetField);
                cil.Emit(OpCodes.Stfld, TargetField);

                cil.Emit(OpCodes.Ldloca_S, localAwaiterStateMachine.LocalIndex);
                cil.Emit(OpCodes.Ldarg_0);
                cil.Emit(OpCodes.Stfld, ProxyField);

                foreach(var tuple in ParameterFields.Zip(Enumerable.Range(1, ParameterFields.Length), (left, right) => new Tuple<FieldBuilder, int>(left, right)))
                {
                    cil.Emit(OpCodes.Ldloca_S, localAwaiterStateMachine.LocalIndex);
                    cil.Emit(OpCodes.Ldarg, tuple.Item2);
                    cil.Emit(OpCodes.Stfld, tuple.Item1);
                }

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

        private static readonly MethodInfo _setResultInContextMethodInfo = _contextType.GetProperty(nameof(AspectContext.ReturnValue)).GetSetMethod();

        private static readonly FieldInfo _emptyParameterArrayFieldInfo = typeof(ProxyFactoryUtils).GetField(nameof(ProxyFactoryUtils.EmptyParameters));
        private static readonly MethodInfo _executePipelineMethodInfo = typeof(ProxyFactoryUtils).GetMethod(nameof(ProxyFactoryUtils.ExecutePipeline));
        private static readonly MethodInfo _getAwaiterMethodInfo = typeof(Task).GetMethod(nameof(Task.GetAwaiter), Type.EmptyTypes);
        //private static readonly MethodInfo _createContextMethodInfo = typeof(IAspectContextFactory).GetMethod(nameof(IAspectContextFactory.CreateContext));
        private static readonly MethodInfo _getServicesValueMethodInfo = typeof(IAspectContextFactory).GetProperty(nameof(IAspectContextFactory.Services)).GetGetMethod();
        private static readonly MethodInfo _setLastContextMethodInfo = typeof(IAspectContextFactory).GetProperty(nameof(IAspectContextFactory.CurrentContext)).GetSetMethod();
        private static readonly MethodInfo _getReturnValueMethodInfo = typeof(AspectContext).GetProperty(nameof(AspectContext.ReturnValue)).GetGetMethod();
    }
}
