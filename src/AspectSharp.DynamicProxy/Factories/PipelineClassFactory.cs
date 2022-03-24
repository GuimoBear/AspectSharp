using AspectSharp.Abstractions;
using AspectSharp.Abstractions.Attributes;
using AspectSharp.DynamicProxy.StateMachines;
using AspectSharp.DynamicProxy.StateMachines.Interfaces;
using AspectSharp.DynamicProxy.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using static AspectSharp.DynamicProxy.Utils.InterceptorTypeCache;

namespace AspectSharp.DynamicProxy.Factories
{
    internal static class PipelineClassFactory
    {
        private static readonly ConstructorInfo _objectConstructorMethodInfo = typeof(object).GetConstructor(Array.Empty<Type>());

        private static readonly MethodInfo _getTypeFromHandleMethodInfo = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), new Type[] { typeof(RuntimeTypeHandle) });

        private static readonly MethodInfo _getTargetMethodInfo = typeof(AspectContext).GetProperty(nameof(AspectContext.Target)).GetGetMethod();
        private static readonly MethodInfo _getParametersMethodInfo = typeof(AspectContext).GetProperty(nameof(AspectContext.Parameters)).GetGetMethod();
        private static readonly MethodInfo _waitTaskMethodInfo = typeof(Task).GetMethod(nameof(Task.Wait), Array.Empty<Type>());
        private static readonly MethodInfo _setReturnValueMethodInfo = typeof(AspectContext).GetProperty(nameof(AspectContext.ReturnValue)).GetSetMethod();
#if NETCOREAPP3_1_OR_GREATER
        private static readonly MethodInfo _asTaskValueTaskMethodInfo = typeof(ValueTask).GetMethod(nameof(ValueTask.AsTask));
#endif
        private static readonly MethodInfo _getCompletedTaskMethodInfo = typeof(Task).GetProperty(nameof(Task.CompletedTask)).GetGetMethod();
        private static readonly Type _exceptionType = typeof(Exception);

        private static readonly Type _interceptDelegateType = typeof(InterceptDelegate);
        private static readonly Type _abstractInterceptorAttributeType = typeof(AbstractInterceptorAttribute);

        private static readonly MethodInfo _createPipelineMethodInfo = typeof(ProxyFactoryUtils).GetMethod(nameof(ProxyFactoryUtils.CreatePipeline), new Type[] { typeof(AspectDelegate), typeof(AbstractInterceptorAttribute[]) });
        private static readonly MethodInfo _getInterceptorsMethodInfo = typeof(ProxyFactoryUtils).GetMethod(nameof(ProxyFactoryUtils.GetInterceptors), new Type[] { typeof(Type), typeof(int), typeof(int) });

        private static readonly ConstructorInfo _aspectDelegateConstructorInfo = typeof(AspectDelegate).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) });

        private static readonly ParameterInfo[] _aspectDelegateParameters = typeof(AspectDelegate).GetMethod(nameof(AspectDelegate.Invoke)).GetParameters();
        private static readonly Type _asyncStateMachineType = typeof(IAsyncStateMachine);

        public static IReadOnlyDictionary<MethodInfo, PropertyInfo> CreatePipelineClass(Type serviceType, TypeBuilder typeBuilder, ModuleBuilder moduleBuilder, InterceptedTypeData interceptedTypeData, DynamicProxyFactoryConfigurations configs)
        {
            var attrs = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
            var staticAttrs = MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
            var constructorBuilder = typeBuilder.DefineConstructor(attrs, CallingConventions.Standard | CallingConventions.HasThis, Type.EmptyTypes);
            var staticConstructorBuilder = typeBuilder.DefineConstructor(staticAttrs, CallingConventions.Standard, Type.EmptyTypes);
            var cil = constructorBuilder.GetILGenerator();
            cil.DeclareLocal(typeof(AbstractInterceptorAttribute[]));

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Call, _objectConstructorMethodInfo);

            var staticCil = staticConstructorBuilder.GetILGenerator();
            var localServiceType = staticCil.DeclareLocal(typeof(Type));

            staticCil.Emit(OpCodes.Ldtoken, serviceType);
            staticCil.Emit(OpCodes.Call, _getTypeFromHandleMethodInfo);
            staticCil.Emit(OpCodes.Stloc_0, localServiceType);

            var methods = serviceType.GetMethods();
            var index = 1;

            var ret = new Dictionary<MethodInfo, PropertyInfo>();
            foreach (var methodInfo in methods)
            {
                if (interceptedTypeData.TryGetMethodInterceptors(methodInfo, out _))
                {
                    var propertyBuilder = CreateAspectDelegateAndPipelineProperty(typeBuilder, moduleBuilder, staticCil, methodInfo, configs, index);

                    ret.Add(methodInfo, propertyBuilder);

                    index++;
                }
            }
            cil.Emit(OpCodes.Ret);
            staticCil.Emit(OpCodes.Ret);

            return ret;
        }

        private static PropertyBuilder CreateAspectDelegateAndPipelineProperty(TypeBuilder typeBuilder, ModuleBuilder moduleBuilder, ILGenerator staticCil, MethodInfo methodInfo, DynamicProxyFactoryConfigurations configs, int index)
        {
            var returnInfo = methodInfo.GetReturnInfo();

            var pipelineField = typeBuilder.DefineField(string.Format("_pipeline{0}", index), _interceptDelegateType, FieldAttributes.Private | FieldAttributes.Static | FieldAttributes.InitOnly);

            var methodBuilder = typeBuilder.DefineMethod(string.Format("get_Pipeline{0}", index), MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.SpecialName, CallingConventions.Standard, _interceptDelegateType, null);
            var cil = methodBuilder.GetILGenerator();

            cil.Emit(OpCodes.Ldsfld, pipelineField);
            cil.Emit(OpCodes.Ret);

            var pipelineProperty = typeBuilder.DefineProperty(string.Format("Pipeline{0}", index), PropertyAttributes.None, CallingConventions.Standard, _interceptDelegateType, Array.Empty<Type>());
            pipelineProperty.SetGetMethod(methodBuilder);

            if (returnInfo.IsAsync)
            {
                methodBuilder = typeBuilder.DefineMethod(string.Format("AspectDelegate{0}", index), MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Static, CallingConventions.Standard, typeof(Task), new Type[] { typeof(AspectContext) });
                methodBuilder.DefineParameter(1, ParameterAttributes.None, "context");

                var methodParameters = methodInfo.GetParameters();

                var attrs = TypeAttributes.Public | TypeAttributes.SequentialLayout | TypeAttributes.AnsiClass | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit;

                var aspectContextAsyncStateMachineType = GetAspectDelegateStateMachineType(methodInfo.ReturnType);

                var interfaces = aspectContextAsyncStateMachineType.GetInterfaces().Reverse().ToList();
                interfaces.Add(aspectContextAsyncStateMachineType);
                interfaces.Reverse();
                var awaiterTypeBuilder = moduleBuilder.DefineType(string.Format("{0}_AspectDelegate{1}AsyncStateMachine", typeBuilder.Name, index), attrs, typeof(ValueType), interfaces.ToArray());
                var moveNextMethodInfo = GetMoveNextMethod(methodInfo.ReturnType, aspectContextAsyncStateMachineType, awaiterTypeBuilder);
                var fields = DefineFields(methodInfo, awaiterTypeBuilder);

                var prepareAwaiterMethodBuilder = DefinePrepareAwaiterMethod(methodInfo, awaiterTypeBuilder, fields[2]);

                var awaiterAttrs = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
                var awaiterMethodBuilder = awaiterTypeBuilder.DefineMethod("MoveNext", awaiterAttrs, CallingConventions.Standard | CallingConventions.HasThis);
                var awaiterCil = awaiterMethodBuilder.GetILGenerator();

                awaiterCil.Emit(OpCodes.Ldarg_0);
                awaiterCil.Emit(OpCodes.Call, moveNextMethodInfo);
                awaiterCil.Emit(OpCodes.Ret);

                DefineSetStateMachine(methodInfo, awaiterTypeBuilder, fields);
                DefineAspectDelegateStateMachineMethods(methodInfo, awaiterTypeBuilder, fields);

                var awaiterType = awaiterTypeBuilder.CreateType();

                var stateField = fields[0];
                var builderField = fields[1];
                var contextField = fields[2];

                var attributeConstructor = typeof(AsyncStateMachineAttribute).GetConstructor(new Type[] { typeof(Type) });
                var customAttribute = new CustomAttributeBuilder(attributeConstructor, new object[] { awaiterType });
                methodBuilder.SetCustomAttribute(customAttribute);

                cil = methodBuilder.GetILGenerator();
                var localAwaiterStateMachine = cil.DeclareLocal(awaiterTypeBuilder);
                cil.Emit(OpCodes.Ldloca_S, localAwaiterStateMachine.LocalIndex);
                cil.Emit(OpCodes.Initobj, awaiterType);

                cil.Emit(OpCodes.Ldloca_S, localAwaiterStateMachine.LocalIndex);
                cil.Emit(OpCodes.Call, builderField.FieldType.GetMethod(nameof(AsyncTaskMethodBuilder.Create), Type.EmptyTypes));
                cil.Emit(OpCodes.Stfld, builderField);

                cil.Emit(OpCodes.Ldloca_S, localAwaiterStateMachine.LocalIndex);
                cil.Emit(OpCodes.Ldarg_0);
                cil.Emit(OpCodes.Stfld, contextField);

                cil.Emit(OpCodes.Ldloca_S, localAwaiterStateMachine.LocalIndex);
                cil.Emit(OpCodes.Ldc_I4_M1);
                cil.Emit(OpCodes.Stfld, stateField);

                var startStateMachineMethodInfo = builderField.FieldType.GetMethod(nameof(AsyncTaskMethodBuilder.Start)).MakeGenericMethod(awaiterType);

                cil.Emit(OpCodes.Ldloca_S, localAwaiterStateMachine.LocalIndex);
                cil.Emit(OpCodes.Ldflda, builderField);
                cil.Emit(OpCodes.Ldloca_S, localAwaiterStateMachine.LocalIndex);
                cil.Emit(OpCodes.Call, startStateMachineMethodInfo);

                cil.Emit(OpCodes.Ldloca_S, localAwaiterStateMachine.LocalIndex);
                cil.Emit(OpCodes.Ldflda, builderField);
                cil.Emit(OpCodes.Call, builderField.FieldType.GetProperty(nameof(AsyncTaskMethodBuilder.Task)).GetGetMethod());
                cil.Emit(OpCodes.Ret);
            }
            else
            {
                methodBuilder = typeBuilder.DefineMethod(string.Format("AspectDelegate{0}", index), MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Static, CallingConventions.Standard, typeof(Task), new Type[] { typeof(AspectContext) });
                methodBuilder.DefineParameter(1, ParameterAttributes.None, "context");

                cil = methodBuilder.GetILGenerator();

                var localVariables = new List<LocalBuilder>();
                if (!returnInfo.IsVoid)
                    localVariables.Add(cil.DeclareLocal(methodInfo.DeclaringType));
                var parameters = methodInfo.GetParameters();
                foreach (var param in parameters)
                    localVariables.Add(cil.DeclareLocal(param.ParameterType));
#if NETCOREAPP3_1_OR_GREATER
            if (returnInfo.IsValueTask)
                localVariables.Add(cil.DeclareLocal(methodInfo.ReturnType));
#endif

                cil.Emit(OpCodes.Ldarg_0);
                cil.Emit(OpCodes.Callvirt, _getTargetMethodInfo);
                cil.Emit(OpCodes.Isinst, methodInfo.DeclaringType);
                if (!returnInfo.IsVoid)
                    cil.Emit(OpCodes.Stloc_0);

                foreach (var i in Enumerable.Range(returnInfo.IsVoid ? 0 : 1, parameters.Length))
                {
                    cil.Emit(OpCodes.Ldarg_0);
                    cil.Emit(OpCodes.Callvirt, _getParametersMethodInfo);
                    cil.Emit(OpCodes.Ldc_I4, returnInfo.IsVoid ? i : i - 1);
                    cil.Emit(OpCodes.Ldelem_Ref);
                    var param = parameters[returnInfo.IsVoid ? i : i - 1];
                    if (param.ParameterType.IsValueType)
                        cil.Emit(OpCodes.Unbox_Any, param.ParameterType);
                    else
                        cil.Emit(OpCodes.Castclass, param.ParameterType);
                    cil.Emit(OpCodes.Stloc, i);
                }

                if (!returnInfo.IsVoid)
                {
                    cil.Emit(OpCodes.Ldarg_0);
                    cil.Emit(OpCodes.Ldloc_0);
                }
                foreach (var i in Enumerable.Range(returnInfo.IsVoid ? 0 : 1, parameters.Length))
                    cil.Emit(OpCodes.Ldloc, i);
                cil.Emit(OpCodes.Callvirt, methodInfo);

#if NETCOREAPP3_1_OR_GREATER
            if (returnInfo.IsValueTask)
            {
                cil.Emit(OpCodes.Stloc_S, localVariables.Count - 1);
                cil.Emit(OpCodes.Ldloca_S, localVariables.Count - 1);
                if (returnInfo.IsVoid)
                    cil.Emit(OpCodes.Call, _asTaskValueTaskMethodInfo);
                else
                    cil.Emit(OpCodes.Call, typeof(ValueTask<>).MakeGenericType(returnInfo.Type).GetProperty(nameof(ValueTask<int>.Result)).GetGetMethod());
            }
            else
#endif
                if (returnInfo.IsAsync && !returnInfo.IsVoid)
                    cil.Emit(OpCodes.Callvirt, typeof(Task<>).MakeGenericType(returnInfo.Type).GetProperty(nameof(Task<int>.Result)).GetGetMethod());
                if (!returnInfo.IsVoid)
                {
                    if (returnInfo.Type.IsValueType)
                        cil.Emit(OpCodes.Box, returnInfo.Type);
                    cil.Emit(OpCodes.Callvirt, _setReturnValueMethodInfo);
                    cil.Emit(OpCodes.Call, _getCompletedTaskMethodInfo);
                }
                else if (!returnInfo.IsAsync)
                    cil.Emit(OpCodes.Call, _getCompletedTaskMethodInfo);
                cil.Emit(OpCodes.Ret);
            }

            staticCil.Emit(OpCodes.Ldnull);
            staticCil.Emit(OpCodes.Ldftn, methodBuilder);
            staticCil.Emit(OpCodes.Newobj, _aspectDelegateConstructorInfo);

            staticCil.Emit(OpCodes.Ldloc_0);
            staticCil.Emit(OpCodes.Ldc_I4, configs.GetHashCode());
            staticCil.Emit(OpCodes.Ldc_I4, methodInfo.GetHashCode());
            staticCil.Emit(OpCodes.Call, _getInterceptorsMethodInfo);
            staticCil.Emit(OpCodes.Call, _createPipelineMethodInfo);
            staticCil.Emit(OpCodes.Stsfld, pipelineField);

            return pipelineProperty;
        }

        private static FieldBuilder[] DefineFields(MethodInfo methodInfo, TypeBuilder typeBuilder)
        {
            var asyncMethodBuilderType = GetAsyncMethodBuilderType(methodInfo.ReturnType);
            var taskAwaiterType = GetTaskAwaiterType(methodInfo.ReturnType);

            var fields = new List<FieldBuilder>();
            var stateField = typeBuilder.DefineField("<>__state", typeof(int), FieldAttributes.Public);
            var builderField = typeBuilder.DefineField("<>___builder", asyncMethodBuilderType, FieldAttributes.Public);
            var contextField = typeBuilder.DefineField("<>___context", typeof(AspectContext), FieldAttributes.Public);

            fields.Add(stateField);
            fields.Add(builderField);
            fields.Add(contextField);
            if (methodInfo.ReturnType.IsGenericType)
                fields.Add(typeBuilder.DefineField("_contextWrap", typeof(AspectContext), FieldAttributes.Private));
            var awaiterField = typeBuilder.DefineField("<>__awaiter", taskAwaiterType, FieldAttributes.Private);
            fields.Add(awaiterField);

            DefineProperty(typeBuilder, "State", stateField);
            DefineProperty(typeBuilder, "Builder", builderField);
            DefineProperty(typeBuilder, "Context", contextField);
            DefineProperty(typeBuilder, "Awaiter", awaiterField);

            return fields.ToArray();
        }

        private static MethodBuilder DefinePrepareAwaiterMethod(MethodInfo methodInfo, TypeBuilder typeBuilder, FieldBuilder contextField)
        {
            var returnInfo = methodInfo.GetReturnInfo();
            var methodParameters = methodInfo.GetParameters();

            var taskAwaiterType = GetTaskAwaiterType(methodInfo.ReturnType);

            var attrs = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;

            var methodBuilder = typeBuilder.DefineMethod(nameof(IAspectDelegateTaskAsyncStateMachine.PrepareAwaiter), attrs, CallingConventions.Standard | CallingConventions.HasThis, taskAwaiterType, Type.EmptyTypes);
            var cil = methodBuilder.GetILGenerator();
            var localTarget = cil.DeclareLocal(methodInfo.DeclaringType);
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
            cil.Emit(OpCodes.Stloc, localTarget.LocalIndex);
            foreach (var i in Enumerable.Range(0, parameters.Count))
            {
                var param = parameters[i];

                cil.Emit(OpCodes.Ldarg_0);
                cil.Emit(OpCodes.Callvirt, _getParametersMethodInfo);
                cil.Emit(OpCodes.Ldc_I4, i);
                cil.Emit(OpCodes.Ldelem_Ref);
                if (param.LocalType.IsValueType)
                    cil.Emit(OpCodes.Unbox_Any, param.LocalType);
                else
                    cil.Emit(OpCodes.Castclass, param.LocalType);
                cil.Emit(OpCodes.Stloc, param.LocalIndex);
            }
            cil.Emit(OpCodes.Ldloc, localTarget.LocalIndex);
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

        private static void DefineSetStateMachine(MethodInfo methodInfo, TypeBuilder typeBuilder, FieldBuilder[] fields)
        {
            var asyncMethodBuilderType = GetAsyncMethodBuilderType(methodInfo.ReturnType);

            var builderField = fields[1];

            var parameterTypes = new Type[] { typeof(IAsyncStateMachine) };

            var attrs = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
            var methodBuilder = typeBuilder.DefineMethod("SetStateMachine", attrs, CallingConventions.Standard | CallingConventions.HasThis, null, parameterTypes);
            methodBuilder.DefineParameter(1, ParameterAttributes.None, "stateMachine");

            var cil = methodBuilder.GetILGenerator();

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldflda, builderField);
            cil.Emit(OpCodes.Ldarg_1);
            cil.Emit(OpCodes.Call, asyncMethodBuilderType.GetMethod(nameof(AsyncTaskMethodBuilder.SetStateMachine), parameterTypes));
            cil.Emit(OpCodes.Ret);
        }

        private static void DefineAspectDelegateStateMachineMethods(MethodInfo methodInfo, TypeBuilder typeBuilder, FieldBuilder[] fields)
        {
            var asyncMethodBuilderType = GetAsyncMethodBuilderType(methodInfo.ReturnType);
            var taskAwaiterType = GetTaskAwaiterType(methodInfo.ReturnType);

            var builderField = fields[1];
            var awaiterField = fields.Last();

            var attrs = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
            var methodBuilder = typeBuilder.DefineMethod("SetResult", attrs, CallingConventions.Standard | CallingConventions.HasThis);

            var cil = methodBuilder.GetILGenerator();

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldflda, builderField);
            cil.Emit(OpCodes.Call, asyncMethodBuilderType.GetMethod(nameof(AsyncTaskMethodBuilder.SetResult), Type.EmptyTypes));
            cil.Emit(OpCodes.Ret);

            methodBuilder = typeBuilder.DefineMethod("SetException", attrs, CallingConventions.Standard | CallingConventions.HasThis, null, new Type[] { typeof(Exception) });
            methodBuilder.DefineParameter(1, ParameterAttributes.None, "ex");

            cil = methodBuilder.GetILGenerator();

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldflda, builderField);
            cil.Emit(OpCodes.Ldarg_1);
            cil.Emit(OpCodes.Call, asyncMethodBuilderType.GetMethod(nameof(AsyncTaskMethodBuilder.SetException), new Type[] { typeof(Exception) }));
            cil.Emit(OpCodes.Ret);

            methodBuilder = typeBuilder.DefineMethod("AwaitUnsafeOnCompleted", attrs, CallingConventions.Standard | CallingConventions.HasThis);
            cil = methodBuilder.GetILGenerator();
            var localAwaiter = cil.DeclareLocal(taskAwaiterType);

            var method = asyncMethodBuilderType.GetMethod(nameof(AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted)).MakeGenericMethod(taskAwaiterType, typeBuilder);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldflda, builderField);
            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldflda, awaiterField);
            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Call, method);
            cil.Emit(OpCodes.Ret);
        }

        private static void DefineProperty(TypeBuilder typeBuilder, string propertyName, FieldBuilder field)
        {
            var attrs = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.SpecialName; 
            var getter = typeBuilder.DefineMethod(string.Format("get_{0}", propertyName), attrs, CallingConventions.Standard | CallingConventions.HasThis, field.FieldType, Type.EmptyTypes);
            var cil = getter.GetILGenerator();
            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldfld, field);
            cil.Emit(OpCodes.Ret);

            var setter = typeBuilder.DefineMethod(string.Format("set_{0}", propertyName), attrs, CallingConventions.Standard | CallingConventions.HasThis, null, new Type[] { field.FieldType });
            setter.DefineParameter(1, ParameterAttributes.None, "value");
            cil = setter.GetILGenerator();
            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldarg_1);
            cil.Emit(OpCodes.Stfld, field);
            cil.Emit(OpCodes.Ret);

            var propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.None, field.FieldType, null);

            propertyBuilder.SetGetMethod(getter);
            propertyBuilder.SetSetMethod(setter);
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
                if (returnType == typeof(Task))
                    return typeof(AsyncTaskMethodBuilder);
                else if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
                    return typeof(AsyncTaskMethodBuilder);
                    //return typeof(AsyncTaskMethodBuilder<>).MakeGenericType(returnType.GetGenericArguments());
#if NETCOREAPP3_1_OR_GREATER
                else if (returnType == typeof(ValueTask))
                    return typeof(AsyncValueTaskMethodBuilder);
                else if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
                    return typeof(AsyncValueTaskMethodBuilder);
                    //return typeof(AsyncValueTaskMethodBuilder<>).MakeGenericType(returnType.GetGenericArguments());
#endif
            }
            return default;
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
                if (returnType == typeof(Task))
                    return typeof(TaskAwaiter);
                else if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
                    return typeof(TaskAwaiter<>).MakeGenericType(returnType.GetGenericArguments());
#if NETCOREAPP3_1_OR_GREATER
                else if (returnType == typeof(ValueTask))
                    return typeof(ValueTaskAwaiter);
                else if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
                    return typeof(ValueTaskAwaiter<>).MakeGenericType(returnType.GetGenericArguments());
#endif
            }
            return default;
        }

        private static Type GetAspectDelegateStateMachineType(Type returnType)
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
                if (returnType == typeof(Task))
                    return typeof(IAspectDelegateTaskAsyncStateMachine);
                else if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
                    return typeof(IAspectDelegateTaskAsyncStateMachine<>).MakeGenericType(returnType.GetGenericArguments());
#if NETCOREAPP3_1_OR_GREATER
                else if (returnType == typeof(ValueTask))
                    return typeof(IAspectDelegateValueTaskAsyncStateMachine);
                else if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
                    return typeof(IAspectDelegateValueTaskAsyncStateMachine<>).MakeGenericType(returnType.GetGenericArguments());
#endif
            }
            return default;
        }

        private static MethodInfo GetMoveNextMethod(Type returnType, Type aspectDelegateAsyncStateMachineType, Type awaiterType)
        {
            if (returnType.IsGenericType)
            {
                var retType = returnType.GetGenericArguments()[0];
                if (returnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    var methodInfo = typeof(AspectDelegateTaskAsyncStateMachineManager)
                        .GetMethods()
                        .First(mi => mi.Name == "MoveNext" &&
                                     mi.IsGenericMethodDefinition &&
                                     mi.GetGenericArguments().Length == 2 &&
                                     mi.GetGenericArguments()[0].GetInterfaces()
                                        .Any(intf => intf.IsGenericType && intf.GetGenericTypeDefinition() == typeof(IAspectDelegateTaskAsyncStateMachine<>)));
                    var genericMethod = methodInfo.MakeGenericMethod(awaiterType, retType);
                    return genericMethod;
                }
#if NETCOREAPP3_1_OR_GREATER
                else if (returnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
                {
                    var methodInfo = typeof(AspectDelegateValueTaskAsyncStateMachineManager)
                        .GetMethods()
                        .First(mi => mi.Name == "MoveNext" &&
                                     mi.IsGenericMethodDefinition &&
                                     mi.GetGenericArguments().Length == 2 &&
                                     mi.GetGenericArguments()[0].GetInterfaces()
                                        .Any(intf => intf.IsGenericType && intf.GetGenericTypeDefinition() == typeof(IAspectDelegateValueTaskAsyncStateMachine<>)));
                    var genericMethod = methodInfo.MakeGenericMethod(awaiterType, retType);
                    return genericMethod;
                }
#endif
            }
            else
            {
                if (returnType == typeof(Task))
                {
                    var methodInfo = typeof(AspectDelegateTaskAsyncStateMachineManager).GetMethods()
                        .First(mi => mi.Name == "MoveNext" &&
                                     mi.IsGenericMethodDefinition &&
                                     mi.GetGenericArguments().Length == 1 &&
                                     typeof(IAspectDelegateTaskAsyncStateMachine).IsAssignableFrom(mi.GetGenericArguments()[0]));
                    var genericMethod = methodInfo.MakeGenericMethod(awaiterType);
                    return genericMethod;
                }
#if NETCOREAPP3_1_OR_GREATER
                else if (returnType == typeof(ValueTask))
                {
                    var methodInfo = typeof(AspectDelegateValueTaskAsyncStateMachineManager)
                        .GetMethods()
                        .First(mi => mi.Name == "MoveNext" &&
                                     mi.IsGenericMethodDefinition &&
                                     mi.GetGenericArguments().Length == 1 &&
                                     typeof(IAspectDelegateValueTaskAsyncStateMachine).IsAssignableFrom(mi.GetGenericArguments()[0]));
                    var genericMethod = methodInfo.MakeGenericMethod(awaiterType);
                    return genericMethod;
                }
#endif
            }
            return default;
        }
    }
}
