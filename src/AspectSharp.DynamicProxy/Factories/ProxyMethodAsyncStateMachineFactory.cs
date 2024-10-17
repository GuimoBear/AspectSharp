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
            GenericTypeParameterBuilder[] callerMethodGenericParameters, 
            TypeInfo[] typeGnericParameters,
            Type customAspectContext, 
            ConstructorInfo contextConstructor)
        {
            var attrs = TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit;

            var previouslyDefinedProxyClassFromThisTargetCount = _cachedProxyTypes.Count(kvp => kvp.Key.Item1 == callerMethod.DeclaringType.Name && kvp.Key.Item2 == callerMethod.Name);
            TypeBuilder typeBuilder;
            if (previouslyDefinedProxyClassFromThisTargetCount == 0)
                typeBuilder = moduleBuilder.DefineType(string.Format("AspectSharp.AsyncStateMachines.ProxyMethods.{0}.{1}", callerMethod.DeclaringType.Name, callerMethod.Name), attrs, typeof(ValueType), _interfaces);
            else
                typeBuilder = moduleBuilder.DefineType(string.Format("AspectSharp.AsyncStateMachines.ProxyMethods.{0}.{1}{2}", callerMethod.DeclaringType.Name, callerMethod.Name, previouslyDefinedProxyClassFromThisTargetCount), attrs, typeof(ValueType), _interfaces);

            var genericParameterBuilders = Array.Empty<GenericTypeParameterBuilder>();

            var methodToCall = methodInfo;
            var methodParameters = methodToCall.GetParameters();

            if (methodToCall.IsGenericMethod)
            {
                var genericParameters = typeGnericParameters.Concat(methodInfo.GetGenericArguments().Select(a => a.GetTypeInfo())).ToArray();

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

            var stateFieldBuilder = DefineStateField(typeBuilder);
            var builderFieldBuilder = DefineMethodBuilderField(typeBuilder, methodToCall, genericParameterBuilders, callerMethodGenericParameters);
            var targetFieldBuilder = DefineTargetField(typeBuilder, targetType);
            var proxyFieldBuilder = DefineProxyField(typeBuilder, proxyType);
            var contextFactoryFieldBuilder = DefineContextFactoryField(typeBuilder);
            var parameterFieldBuilders = DefineParametersField(typeBuilder, methodParameters, genericParameterBuilders, callerMethodGenericParameters);
            var contextField = DefineContextField(typeBuilder);
            var awaiterField = DefineAwaiterField(typeBuilder, methodToCall, genericParameterBuilders, callerMethodGenericParameters);

            DefineMoveNext(typeBuilder, methodToCall, stateFieldBuilder, builderFieldBuilder, targetFieldBuilder, proxyFieldBuilder, originContextActivatorField, contextConstructor, contextFactoryFieldBuilder, contextField, parameterFieldBuilders, pipelineProperty, awaiterField, genericParameterBuilders, callerMethodGenericParameters);
            DefineSetStateMachine(typeBuilder, builderFieldBuilder);

            var awaiterType = typeBuilder.CreateTypeInfo().AsType();
            _cachedProxyTypes.Add(new Tuple<string, string, int>(callerMethod.DeclaringType.Name, callerMethod.Name, awaiterType.GetHashCode()), awaiterType);


            var stateField = awaiterType.GetField(stateFieldBuilder.Name);
            var builderField = awaiterType.GetField(builderFieldBuilder.Name);
            var targetField = awaiterType.GetField(targetFieldBuilder.Name);
            var proxyField = awaiterType.GetField(proxyFieldBuilder.Name);
            var contextFactoryField = awaiterType.GetField(contextFactoryFieldBuilder.Name);
            var parameterFields = parameterFieldBuilders.Select(fi => awaiterType.GetField(fi.Name)).ToArray();

            return new GeneratedAsyncStateMachine(awaiterType, stateField, builderField, targetField, proxyField, contextFactoryField, parameterFields);
        }

        private static FieldBuilder DefineStateField(TypeBuilder typeBuilder)
            => typeBuilder.DefineField("<>__state", _stateType, FieldAttributes.Public);

        private static FieldBuilder DefineMethodBuilderField(TypeBuilder typeBuilder, MethodInfo methodInfo, GenericTypeParameterBuilder[] genericParameters, GenericTypeParameterBuilder[] typeGenericParameters)
            => typeBuilder.DefineField("<>___builder", GenericParameterUtils.ReplaceTypeUsingGenericParameters(GetAsyncMethodBuilderType(methodInfo.ReturnType), genericParameters, typeGenericParameters), FieldAttributes.Public);

        private static FieldBuilder DefineTargetField(TypeBuilder typeBuilder, Type serviceType)
            => typeBuilder.DefineField("<>__target", serviceType, FieldAttributes.Public);

        private static FieldBuilder DefineProxyField(TypeBuilder typeBuilder, TypeBuilder proxyType)
            => typeBuilder.DefineField("<>__proxy", proxyType, FieldAttributes.Public);

        private static FieldBuilder DefineContextFactoryField(TypeBuilder typeBuilder)
            => typeBuilder.DefineField("<>__contextFactory", typeof(IAspectContextFactory), FieldAttributes.Public);

        private static FieldBuilder[] DefineParametersField(TypeBuilder typeBuilder, ParameterInfo[] methodParameters, GenericTypeParameterBuilder[] genericParameters, GenericTypeParameterBuilder[] typeGenericParameters)
        {
            if (methodParameters.Length == 0)
                return Array.Empty<FieldBuilder>();
            var ret = new List<FieldBuilder>();
            foreach (var parameter in methodParameters)
                ret.Add(typeBuilder.DefineField(string.Format("<>__{0}", parameter.Name), GenericParameterUtils.ReplaceTypeUsingGenericParameters(parameter.ParameterType, genericParameters, typeGenericParameters), FieldAttributes.Public));
            return ret.ToArray();
        }

        private static FieldBuilder DefineContextField(TypeBuilder typeBuilder)
            => typeBuilder.DefineField("<>__context", typeof(AspectContext), FieldAttributes.Private);

        private static FieldBuilder DefineAwaiterField(TypeBuilder typeBuilder, MethodInfo methodInfo, GenericTypeParameterBuilder[] genericParameters, GenericTypeParameterBuilder[] typeGenericParameters)
        {
            var returnInfo = methodInfo.GetReturnInfo();
            if (returnInfo.IsVoid)
            { 
                return typeBuilder.DefineField("<>__awaiter", _taskAwaiterType, FieldAttributes.Private);
            }
            else
            {
                return typeBuilder.DefineField("<>__awaiter", typeof(TaskAwaiter<>).MakeGenericType(GenericParameterUtils.ReplaceTypeUsingGenericParameters(returnInfo.Type, genericParameters, typeGenericParameters)), FieldAttributes.Private);
            }
        }

        private static MethodBuilder DefineMoveNext(TypeBuilder typeBuilder, MethodInfo methodInfo, FieldBuilder stateField, FieldBuilder builderField, FieldBuilder targetField, FieldBuilder proxyField, FieldInfo originContextActivatorField, ConstructorInfo contextConstructor, FieldBuilder contextFactoryField, FieldBuilder contextField, FieldBuilder[] parameterFields, PropertyInfo pipelineProperty, FieldBuilder awaiterField, GenericTypeParameterBuilder[] genericParameters, GenericTypeParameterBuilder[] typeGenericParameters)
        {
            var returnInfo = methodInfo.GetReturnInfo();

            var prepareAwaiterMethod = DefinePrepareAwaiter(typeBuilder, targetField, proxyField, originContextActivatorField, contextConstructor, contextFactoryField, contextField, parameterFields, pipelineProperty, methodInfo, genericParameters, typeGenericParameters);
            var awaitOnCompletedMethod = DefineAwaitOnCompleted(typeBuilder, stateField, awaiterField, builderField);
            var afterCompletionMethod = DefineAfterCompletionMethod(typeBuilder, awaiterField, methodInfo, contextField, genericParameters, typeGenericParameters);

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
                localReturn = cil.DeclareLocal(GenericParameterUtils.ReplaceTypeUsingGenericParameters(returnInfo.Type, genericParameters, typeGenericParameters));

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
            cil.Emit(OpCodes.Call, localAwaiter.LocalType.ContainsGenericParameters 
                ? TypeBuilder.GetMethod(localAwaiter.LocalType, typeof(TaskAwaiter<>).GetProperty(nameof(TaskAwaiter.IsCompleted)).GetGetMethod())
                : localAwaiter.LocalType.GetProperty(nameof(TaskAwaiter.IsCompleted)).GetGetMethod());
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

        private static MethodBuilder DefinePrepareAwaiter(TypeBuilder typeBuilder, FieldBuilder targetField, FieldBuilder proxyField, FieldInfo originContextActivatorField, ConstructorInfo contextConstructor, FieldBuilder contextFactoryField, FieldBuilder contextField, FieldBuilder[] parameterFields, PropertyInfo pipelineProperty, MethodInfo methodInfo, GenericTypeParameterBuilder[] genericParameters, GenericTypeParameterBuilder[] typeGenericParameters)
        {
            var attrs = MethodAttributes.Private | MethodAttributes.HideBySig;
            var returnInfo = methodInfo.GetReturnInfo();
            var returnType = _taskAwaiterType;
            if (!returnInfo.IsVoid)
                returnType = typeof(TaskAwaiter<>).MakeGenericType(GenericParameterUtils.ReplaceTypeUsingGenericParameters(returnInfo.Type, genericParameters, typeGenericParameters));

            var methodBuilder = typeBuilder.DefineMethod("PrepareAwaiter", attrs, CallingConventions.Standard | CallingConventions.HasThis, returnType, Type.EmptyTypes);

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
            if (returnInfo.IsVoid)
            {
                cil.Emit(OpCodes.Call, _executePipelineMethodInfo);
                cil.Emit(OpCodes.Callvirt, _getAwaiterMethodInfo);
            }
            else
            {
                cil.Emit(OpCodes.Call, typeof(ProxyFactoryUtils).GetMethod(nameof(ProxyFactoryUtils.ExecutePipelineAndReturnResult)).MakeGenericMethod(GenericParameterUtils.ReplaceTypeUsingGenericParameters(returnInfo.Type, genericParameters, typeGenericParameters)));
                cil.Emit(OpCodes.Callvirt, returnInfo.Type.ContainsGenericParameters 
                    ? TypeBuilder.GetMethod(typeof(Task<>).MakeGenericType(GenericParameterUtils.ReplaceTypeUsingGenericParameters(returnInfo.Type, genericParameters, typeGenericParameters)), typeof(Task<>).GetMethod(nameof(Task.GetAwaiter), Type.EmptyTypes))
                    : typeof(Task<>).MakeGenericType(GenericParameterUtils.ReplaceTypeUsingGenericParameters(returnInfo.Type, genericParameters, typeGenericParameters)).GetMethod(nameof(Task.GetAwaiter), Type.EmptyTypes));
            }
            cil.Emit(OpCodes.Ret);

            return methodBuilder;
        }

        private static MethodBuilder DefineAwaitOnCompleted(TypeBuilder typeBuilder, FieldBuilder stateField, FieldBuilder awaiterField, FieldBuilder builderField)
        {
            var taskAwaiterByRefType = awaiterField.FieldType.MakeByRefType();

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
            var awaitUnsafeOnCompletedOnAsyncMethodBuilderMethod = GetAwaitUnsafeOnCompletedOnAsyncMethodBuilderMethod(typeBuilder, builderField, awaiterField);
            cil.Emit(OpCodes.Call, awaitUnsafeOnCompletedOnAsyncMethodBuilderMethod);

            cil.Emit(OpCodes.Ret);
            return methodBuilder;
        }

        private static MethodBuilder DefineAfterCompletionMethod(TypeBuilder typeBuilder, FieldBuilder awaiterField, MethodInfo methodInfo, FieldBuilder contextField, GenericTypeParameterBuilder[] genericParameters, GenericTypeParameterBuilder[] typeGenericParameters)
        {
            var retInfo = methodInfo.GetReturnInfo();
            var taskAwaiterByRefType = awaiterField.FieldType.MakeByRefType();

            var attrs = MethodAttributes.Private | MethodAttributes.Virtual;
            Type returnType = default;
            if (!retInfo.IsVoid)
                returnType = GenericParameterUtils.ReplaceTypeUsingGenericParameters(retInfo.Type, genericParameters, typeGenericParameters);
            var methodBuilder = typeBuilder.DefineMethod("AfterCompletion", attrs, CallingConventions.Standard | CallingConventions.HasThis, returnType, new Type[] { taskAwaiterByRefType });
            var awaiterParameter = methodBuilder.DefineParameter(1, ParameterAttributes.None, "awaiter");

            var cil = methodBuilder.GetILGenerator();

            cil.Emit(OpCodes.Ldarg, awaiterParameter.Position);
            if (retInfo.IsVoid)
            {
                cil.Emit(OpCodes.Call, _taskAwaiterType.GetMethod(nameof(TaskAwaiter.GetResult), Type.EmptyTypes));
            }
            else
            {
                cil.Emit(OpCodes.Call, awaiterField.FieldType.ContainsGenericParameters
                    ? TypeBuilder.GetMethod(awaiterField.FieldType, typeof(TaskAwaiter<>).GetMethod(nameof(TaskAwaiter.GetResult), Type.EmptyTypes))
                    : awaiterField.FieldType.GetMethod(nameof(TaskAwaiter.GetResult), Type.EmptyTypes));
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
                ? TypeBuilder.GetMethod(builderField.FieldType, builderField.FieldType.GetGenericTypeDefinition().GetMethod(nameof(AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted))).MakeGenericMethod(awaiterField.FieldType, typeBuilder)
                : builderField.FieldType.GetMethod(nameof(AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted)).MakeGenericMethod(awaiterField.FieldType, typeBuilder);

        private static MethodInfo GetSetResultOnAsyncMethodBuilderMethod(FieldBuilder builderField)
        {
            if (builderField.FieldType.IsGenericType)
            {
                return builderField.FieldType.ContainsGenericParameters 
                    ? TypeBuilder.GetMethod(builderField.FieldType, builderField.FieldType.GetGenericTypeDefinition().GetMethods().First(mi => mi.Name == nameof(AsyncTaskMethodBuilder.SetResult) && mi.GetParameters().Count() == 1))
                    : builderField.FieldType.GetMethod(nameof(AsyncTaskMethodBuilder.SetResult), builderField.FieldType.GetGenericArguments());
            }
            return builderField.FieldType.ContainsGenericParameters
                ? TypeBuilder.GetMethod(builderField.FieldType, builderField.FieldType.GetGenericTypeDefinition().GetMethods().First(mi => mi.Name == nameof(AsyncTaskMethodBuilder.SetResult) && mi.GetParameters().Count() == 0))
                : builderField.FieldType.GetMethod(nameof(AsyncTaskMethodBuilder.SetResult), Type.EmptyTypes);
        }

        private static MethodInfo GetSetExceptionOnAsyncMethodBuilderMethod(FieldBuilder builderField)
            => builderField.FieldType.ContainsGenericParameters 
                ? TypeBuilder.GetMethod(builderField.FieldType, builderField.FieldType.GetGenericTypeDefinition().GetMethod(nameof(AsyncTaskMethodBuilder.SetException)))
                : builderField.FieldType.GetMethod(nameof(AsyncTaskMethodBuilder.SetException));

        private static MethodInfo GetCreateMethodBuilderMethod(Type builderFieldType)
            => builderFieldType.ContainsGenericParameters
                ? TypeBuilder.GetMethod(builderFieldType, builderFieldType.GetGenericTypeDefinition().GetMethod(nameof(AsyncTaskMethodBuilder.Create)))
                : builderFieldType.GetMethod(nameof(AsyncTaskMethodBuilder.Create));

        private static MethodInfo GetStartMethodBuilderGenericMethod(Type builderFieldType, Type stateMachineType)
            => builderFieldType.ContainsGenericParameters
                ? TypeBuilder.GetMethod(builderFieldType, builderFieldType.GetGenericTypeDefinition().GetMethod(nameof(AsyncTaskMethodBuilder.Start))).MakeGenericMethod(stateMachineType)
                : builderFieldType.GetMethod(nameof(AsyncTaskMethodBuilder.Start)).MakeGenericMethod(stateMachineType);

        private static MethodInfo GetTaskGetterOnAsyncMethodBuilderMethod(Type builderFieldType)
            => builderFieldType.ContainsGenericParameters
                ? TypeBuilder.GetMethod(builderFieldType, builderFieldType.GetGenericTypeDefinition().GetProperty(nameof(AsyncTaskMethodBuilder.Task)).GetGetMethod())
                : builderFieldType.GetProperty(nameof(AsyncTaskMethodBuilder.Task)).GetGetMethod();

        private static MethodInfo GetSetStateMachineOnAsyncMethodBuilderMethod(FieldBuilder builderField)
            => builderField.FieldType.ContainsGenericParameters
                ? TypeBuilder.GetMethod(builderField.FieldType, builderField.FieldType.GetGenericTypeDefinition().GetMethod(nameof(AsyncTaskMethodBuilder.SetStateMachine)))
                : builderField.FieldType.GetMethod(nameof(AsyncTaskMethodBuilder.SetStateMachine));

        internal class GeneratedAsyncStateMachine
        {
            public Type Type { get; }
            public FieldInfo StateField { get; }
            public FieldInfo BuilderField { get; }
            public FieldInfo TargetField { get; }
            public FieldInfo ProxyField { get; }
            public FieldInfo ContextFactoryField { get; }
            public FieldInfo[] ParameterFields { get; }

            public GeneratedAsyncStateMachine(
                Type type, 
                FieldInfo stateField,
                FieldInfo builderField,
                FieldInfo targetField,
                FieldInfo proxyField,
                FieldInfo contextFactoryField,
                FieldInfo[] parameterFields)
            {
                Type = type;
                StateField = stateField;
                BuilderField = builderField;
                TargetField = targetField;
                ProxyField = proxyField;
                ContextFactoryField = contextFactoryField;
                ParameterFields = parameterFields;
            }

            public void WriteCallerMethod(MethodBuilder callerMethod, GenericTypeParameterBuilder[] callerMethodGenericParameters, GenericTypeParameterBuilder[] typeGenericParameters, FieldInfo targetField, FieldInfo contextFactoryField)
            {
                var attributeConstructor = typeof(AsyncStateMachineAttribute).GetConstructor(new Type[] { typeof(Type) });
                var customAttribute = new CustomAttributeBuilder(attributeConstructor, new object[] { Type });
                callerMethod.SetCustomAttribute(customAttribute);

                var builderFieldType = BuilderField.FieldType;
                if (builderFieldType.ContainsGenericParameters)
                    builderFieldType = builderFieldType.GetGenericTypeDefinition().MakeGenericType(builderFieldType.GetGenericArguments().Select(g => GenericParameterUtils.ReplaceTypeUsingGenericParameters(g, callerMethodGenericParameters, typeGenericParameters)).ToArray());

                var asyncStateMachineType = Type;

                var stateField = StateField;
                var builderField = BuilderField;
                var asyncStateMachineTargetField = TargetField;
                var proxyField = ProxyField;
                var asyncStateMachineContextFactoryField = ContextFactoryField;
                var parameterFields = ParameterFields;
                if (Type.IsGenericType)
                {
                    asyncStateMachineType = Type.MakeGenericType(typeGenericParameters.Concat(callerMethodGenericParameters).ToArray());

                    stateField = TypeBuilder.GetField(asyncStateMachineType, stateField);
                    builderField = TypeBuilder.GetField(asyncStateMachineType, builderField);
                    asyncStateMachineTargetField = TypeBuilder.GetField(asyncStateMachineType, asyncStateMachineTargetField);
                    proxyField = TypeBuilder.GetField(asyncStateMachineType, proxyField);
                    asyncStateMachineContextFactoryField = TypeBuilder.GetField(asyncStateMachineType, asyncStateMachineContextFactoryField);

                    parameterFields = parameterFields.Select(fi => TypeBuilder.GetField(asyncStateMachineType, fi)).ToArray();
                }


                var cil = callerMethod.GetILGenerator();

                if (asyncStateMachineType.IsGenericTypeDefinition)
                    asyncStateMachineType = Type.MakeGenericType(callerMethodGenericParameters);

                var localAwaiterStateMachine = cil.DeclareLocal(asyncStateMachineType);

                cil.Emit(OpCodes.Ldloca_S, localAwaiterStateMachine);
                cil.Emit(OpCodes.Ldc_I4_M1);
                cil.Emit(OpCodes.Stfld, stateField);

                cil.Emit(OpCodes.Ldloca_S, localAwaiterStateMachine);
                var createMethodBuilderMethod = GetCreateMethodBuilderMethod(builderFieldType);
                cil.Emit(OpCodes.Call, createMethodBuilderMethod);
                cil.Emit(OpCodes.Stfld, builderField);

                cil.Emit(OpCodes.Ldloca_S, localAwaiterStateMachine);
                cil.Emit(OpCodes.Ldarg_0);
                cil.Emit(OpCodes.Ldfld, contextFactoryField);
                cil.Emit(OpCodes.Stfld, asyncStateMachineContextFactoryField);

                cil.Emit(OpCodes.Ldloca_S, localAwaiterStateMachine);
                cil.Emit(OpCodes.Ldarg_0);
                cil.Emit(OpCodes.Ldfld, targetField);
                cil.Emit(OpCodes.Stfld, asyncStateMachineTargetField);

                cil.Emit(OpCodes.Ldloca_S, localAwaiterStateMachine);
                cil.Emit(OpCodes.Ldarg_0);
                cil.Emit(OpCodes.Stfld, proxyField);

                foreach(var tuple in parameterFields.Zip(Enumerable.Range(1, parameterFields.Length), (left, right) => new Tuple<FieldInfo, int>(left, right)))
                {
                    cil.Emit(OpCodes.Ldloca_S, localAwaiterStateMachine);
                    cil.Emit(OpCodes.Ldarg, tuple.Item2);
                    cil.Emit(OpCodes.Stfld, tuple.Item1);
                }

                cil.Emit(OpCodes.Ldloca_S, localAwaiterStateMachine);
                cil.Emit(OpCodes.Ldflda, builderField);
                cil.Emit(OpCodes.Ldloca_S, localAwaiterStateMachine);
                var startMethodBuilderGenericMethod = GetStartMethodBuilderGenericMethod(builderFieldType, localAwaiterStateMachine.LocalType);
                cil.Emit(OpCodes.Call, startMethodBuilderGenericMethod);

                cil.Emit(OpCodes.Ldloca_S, localAwaiterStateMachine);
                cil.Emit(OpCodes.Ldflda, builderField);
                var taskGetterOnAsyncMethodBuilderMethod = GetTaskGetterOnAsyncMethodBuilderMethod(builderFieldType);
                cil.Emit(OpCodes.Call, taskGetterOnAsyncMethodBuilderMethod);

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
