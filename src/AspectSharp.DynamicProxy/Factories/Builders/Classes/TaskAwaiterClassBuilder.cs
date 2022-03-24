using AspectSharp.DynamicProxy.Factories.Builders.Classes.Interfaces;
using AspectSharp.DynamicProxy.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AspectSharp.DynamicProxy.Factories.Builders.Classes
{
    internal sealed class TaskAwaiterClassBuilder : ITaskAwaiterClassBuilder
    {
        private static readonly Type _asyncStateMachineType = typeof(IAsyncStateMachine);

        private static readonly Type _exceptionType = typeof(Exception);

        private readonly TypeBuilder _parentType;
        private readonly ModuleBuilder _moduleBuilder;
        private readonly string _nestedTypeName;
        private MethodBuilder _callerMethod;
        private ParameterInfo[] _callerParameters;
        private Type _asyncMethodBuilderType;
        internal IDictionary<ParameterInfo, ParameterParser> parameterParsers;
        private AwaiterConfigurer _configurer;
        private Type _awaitableType;
        private Type _taskAwaiterType;

        private bool _hasShortRamification = true;

        private TaskCompleted _taskCompletedMethod;
        private TaskCompletedWithReturn _taskCompletedWithReturnMethod;

        private FieldBuilder _stateField;
        private FieldBuilder _methodBuildeField;
        private FieldBuilder[] _parameterFields;

        public FieldBuilder StateField => _stateField;
        public FieldBuilder MethodBuilderField => _methodBuildeField;
        public FieldBuilder[] ParameterFields => _parameterFields;

        public TaskAwaiterClassBuilder(TypeBuilder parentType, ModuleBuilder moduleBuilder, string nestedTypeName)
        {
            _parentType = parentType ?? throw new ArgumentNullException(nameof(parentType));
            _moduleBuilder = moduleBuilder ?? throw new ArgumentNullException(nameof(moduleBuilder));
            _nestedTypeName = nestedTypeName ?? throw new ArgumentNullException(nameof(nestedTypeName));
        }

        public ITaskAwaiterClassBuilder CalledFrom(MethodBuilder callerMethod, ParameterInfo[] callerParameters)
        {
            var retInfo = callerMethod.GetReturnInfo();
            if (!retInfo.IsAsync)
                throw new ArgumentException(string.Format("The method {0} must be async", callerMethod.Name), nameof(callerMethod));
            _callerMethod = callerMethod;
            _callerParameters = callerParameters;
            parameterParsers = new Dictionary<ParameterInfo, ParameterParser>();

            return this;
        }

        public ITaskAwaiterParameterPreparerBuilder PrepareParameter(int parameterIndex)
        {
            if (_callerParameters is null || parameterIndex < 0 || parameterIndex >= _callerParameters.Length)
                throw new ArgumentOutOfRangeException(nameof(parameterIndex));
            return new TaskAwaiterParameterPreparerBuilder(this, _callerParameters[parameterIndex]);
        }

        public ITaskAwaiterClassBuilder WithAwaiter(Type awaitableType, AwaiterConfigurer awaiterConfigurerMethod)
        {
            if (awaiterConfigurerMethod is null)
                throw new ArgumentNullException(nameof(awaiterConfigurerMethod));
            if (awaitableType is null)
                throw new ArgumentNullException(nameof(awaitableType));
            if ((awaitableType.IsGenericType && (awaitableType.GetGenericTypeDefinition() == typeof(Task<>)
#if NETCOREAPP3_1_OR_GREATER
                || awaitableType.GetGenericTypeDefinition() == typeof(ValueTask<>)
#endif
            )) || awaitableType == typeof(Task)
#if NETCOREAPP3_1_OR_GREATER
            || awaitableType == typeof(ValueTask)
#endif
            )
            {
                _configurer = awaiterConfigurerMethod;
                _awaitableType = awaitableType;
                if (_awaitableType == typeof(Task))
                {
                    _asyncMethodBuilderType = typeof(AsyncTaskMethodBuilder);
                    _taskAwaiterType = typeof(TaskAwaiter);
                }
                else if (_awaitableType.IsGenericType && _awaitableType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    _asyncMethodBuilderType = typeof(AsyncTaskMethodBuilder<>).MakeGenericType(_awaitableType.GetGenericArguments());
                    _taskAwaiterType = typeof(TaskAwaiter<>).MakeGenericType(_awaitableType.GetGenericArguments());
                }
#if NETCOREAPP3_1_OR_GREATER
                else if (_awaitableType == typeof(ValueTask))
                {
                    _asyncMethodBuilderType = typeof(AsyncValueTaskMethodBuilder);
                    _taskAwaiterType = typeof(ValueTaskAwaiter);
                }
                else if (_awaitableType.IsGenericType && _awaitableType.GetGenericTypeDefinition() == typeof(ValueTask<>))
                {
                    _asyncMethodBuilderType = typeof(AsyncValueTaskMethodBuilder<>).MakeGenericType(_awaitableType.GetGenericArguments());
                    _taskAwaiterType = typeof(ValueTaskAwaiter<>).MakeGenericType(_awaitableType.GetGenericArguments());
                }
#endif
                return this;
            }
            throw new ArgumentException(string.Format("{0} parameter must be an awaitable type", nameof(awaitableType)), nameof(awaitableType));
        }

        public ITaskAwaiterClassBuilder HasLongAwaiterDefinition()
        {
            _hasShortRamification = false;
            return this;
        }

        public IInstanceTaskAwaiterClassBuilder AsInstanceMethod()
        {
            var builder = new InstanceTaskAwaiterClassBuilder(_parentType, _moduleBuilder, _nestedTypeName, _callerMethod, _callerParameters);
            return builder;
        }


        public ITaskAwaiterClassFinalBuilder WithAfterCompletedTask(TaskCompleted taskCompletedMethod)
        {
            _taskCompletedMethod = taskCompletedMethod ?? throw new ArgumentNullException(nameof(taskCompletedMethod));
            return this;
        }

        public ITaskAwaiterClassFinalBuilder WithAfterCompletedTaskWithResult(TaskCompletedWithReturn taskCompletedMethod)
        {
            _taskCompletedWithReturnMethod = taskCompletedMethod ?? throw new ArgumentNullException(nameof(taskCompletedMethod));
            return this;
        }

        public Type Build()
        {
            var attrs = TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit;
            var awaiterTypeBuilder = _moduleBuilder.DefineType(_nestedTypeName, attrs, typeof(ValueType), new Type[] { _asyncStateMachineType });
            var fields = DeclareFields(awaiterTypeBuilder);
            DefineMoveNextMethod(awaiterTypeBuilder, fields);
            DefineSetStateMachine(awaiterTypeBuilder, fields);

            var concreteType = awaiterTypeBuilder.CreateType();

            return concreteType;
        }

        private FieldBuilder[] DeclareFields(TypeBuilder typeBuilder)
        {
            var fields = new List<FieldBuilder>();
            fields.Add(typeBuilder.DefineField("<>__state", typeof(int), FieldAttributes.Public));
            fields.Add(typeBuilder.DefineField("<>___builder", _asyncMethodBuilderType, FieldAttributes.Public));
            foreach (var parameter in _callerParameters)
                fields.Add(typeBuilder.DefineField(parameter.Name, parameter.ParameterType, FieldAttributes.Public));
            fields.Add(typeBuilder.DefineField("<>__awaiter", _taskAwaiterType, FieldAttributes.Private));
            return fields.ToArray();
        }

        private void DefineMoveNextMethod(TypeBuilder typeBuilder, FieldBuilder[] fields)
        {
            var stateField = _stateField = fields[0];
            var builderField = _methodBuildeField = fields[1];
            var parameterFields = new List<FieldBuilder>();
            if (_callerParameters.Length > 0)
                parameterFields.AddRange(fields.Skip(2).Take(_callerParameters.Length));
            _parameterFields = parameterFields.ToArray();
            var awaiterField = fields.Last();

            var attrs = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
            var methodBuilder = typeBuilder.DefineMethod("MoveNext", attrs, CallingConventions.Standard | CallingConventions.HasThis);

            var cil = methodBuilder.GetILGenerator();

            //var localVariables = new List<LocalBuilder>();
            var localException = cil.DeclareLocal(_exceptionType);
            var localState = cil.DeclareLocal(typeof(int));
            LocalBuilder localReturn = default;
            LocalBuilder localAwaiter = default;
            if (!_taskAwaiterType.IsGenericType)
                localAwaiter = cil.DeclareLocal(_taskAwaiterType);
            else
            {
                localReturn = cil.DeclareLocal(_taskAwaiterType.GetGenericArguments()[0]);
                localAwaiter = cil.DeclareLocal(_taskAwaiterType);
            }
            var hasParameters = parameterParsers.Count > 0;
            var parameters = new List<LocalBuilder>();
            if (hasParameters)
                foreach (var parameter in parameterParsers)
                    parameters.Add(cil.DeclareLocal(parameter.Key.ParameterType));

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldfld, stateField);
            cil.Emit(OpCodes.Stloc, localState.LocalIndex);

            cil.BeginExceptionBlock();

            cil.Emit(OpCodes.Ldloc, localState.LocalIndex);
            var awaiterIsNotInitializedLabel = cil.DefineLabel();
            //if (_hasShortRamification)
            //    cil.Emit(OpCodes.Brfalse_S, awaiterIsNotInitializedLabel);
            //else
                cil.Emit(OpCodes.Brfalse, awaiterIsNotInitializedLabel);

            foreach (var tuple in parameters.Zip(parameterParsers, (left, right) => new Tuple<LocalBuilder, KeyValuePair<ParameterInfo, ParameterParser>>(left, right)))
            {
                var localParameter = tuple.Item1;
                //var parameterInfo = tuple.Item2.Key;
                var parameterConfigurer = tuple.Item2.Value;
                parameterConfigurer(cil, typeBuilder, _parameterFields, localParameter);
            }

            var endOfCatchLabel = cil.DefineLabel();
            var endOfMethodLabel = cil.DefineLabel();

            _configurer(cil, typeBuilder, parameters.ToArray());
            cil.Emit(OpCodes.Callvirt, _awaitableType.GetMethod("GetAwaiter", Type.EmptyTypes));
            cil.Emit(OpCodes.Stloc_S, localAwaiter.LocalIndex);
            cil.Emit(OpCodes.Ldloca_S, localAwaiter.LocalIndex);

            cil.Emit(OpCodes.Call, _taskAwaiterType.GetProperty(nameof(Task.IsCompleted)).GetGetMethod());

            var taskCompletedLabel = cil.DefineLabel();
            cil.Emit(OpCodes.Brtrue_S, taskCompletedLabel);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldc_I4_0);
            cil.Emit(OpCodes.Dup);
            cil.Emit(OpCodes.Stloc, localState.LocalIndex);
            cil.Emit(OpCodes.Stfld, stateField);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldloc, localAwaiter.LocalIndex);
            cil.Emit(OpCodes.Stfld, awaiterField);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldflda, builderField);
            cil.Emit(OpCodes.Ldloca_S, localAwaiter.LocalIndex);
            cil.Emit(OpCodes.Ldarg_0);
            var method = _asyncMethodBuilderType.GetMethod(nameof(AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted)).MakeGenericMethod(_taskAwaiterType, typeBuilder);
            cil.Emit(OpCodes.Call, method);

            cil.Emit(OpCodes.Leave, endOfMethodLabel);

            cil.MarkLabel(awaiterIsNotInitializedLabel);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldfld, awaiterField);
            cil.Emit(OpCodes.Stloc, localAwaiter.LocalIndex);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldflda, awaiterField);
            cil.Emit(OpCodes.Initobj, _taskAwaiterType);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldc_I4_M1);
            cil.Emit(OpCodes.Dup);
            cil.Emit(OpCodes.Stloc, localState.LocalIndex);
            cil.Emit(OpCodes.Stfld, stateField);

            cil.MarkLabel(taskCompletedLabel);

            cil.Emit(OpCodes.Ldloca_S, localAwaiter.LocalIndex);
            cil.Emit(OpCodes.Call, _taskAwaiterType.GetMethod(nameof(TaskAwaiter<int>.GetResult), Type.EmptyTypes));
            if (_taskAwaiterType.IsGenericType)
                cil.Emit(OpCodes.Stloc, localReturn.LocalIndex);

            if (!(_taskCompletedMethod is null))
                _taskCompletedMethod(cil, typeBuilder, builderField, parameterFields.ToArray());
            else if (!(_taskCompletedWithReturnMethod is null))
                _taskCompletedWithReturnMethod(cil, typeBuilder, builderField, parameterFields.ToArray(), localReturn);
            cil.Emit(OpCodes.Leave_S, endOfCatchLabel);

            cil.BeginCatchBlock(_exceptionType);
            var exceptionLocalIndex = localException.LocalIndex;
            cil.Emit(OpCodes.Stloc_S, exceptionLocalIndex);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldc_I4_S, -2);
            cil.Emit(OpCodes.Stfld, stateField);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldflda, builderField);
            cil.Emit(OpCodes.Ldloc, exceptionLocalIndex);
            cil.Emit(OpCodes.Call, _asyncMethodBuilderType.GetMethod(nameof(AsyncTaskMethodBuilder.SetException), new Type[] { typeof(Exception) }));
            cil.Emit(OpCodes.Leave, endOfMethodLabel);
            cil.EndExceptionBlock();
            cil.MarkLabel(endOfCatchLabel);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldc_I4_S, -2);
            cil.Emit(OpCodes.Stfld, stateField);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldflda, builderField);
            if (_asyncMethodBuilderType.IsGenericType)
            {
                cil.Emit(OpCodes.Ldloc, localReturn.LocalIndex);
                cil.Emit(OpCodes.Call, _asyncMethodBuilderType.GetMethod(nameof(AsyncTaskMethodBuilder.SetResult), new Type[] { _asyncMethodBuilderType.GetGenericArguments()[0] }));
            }
            else
                cil.Emit(OpCodes.Call, _asyncMethodBuilderType.GetMethod(nameof(AsyncTaskMethodBuilder.SetResult), Type.EmptyTypes));

            cil.MarkLabel(endOfMethodLabel);
            cil.Emit(OpCodes.Ret);
        }

        private void DefineSetStateMachine(TypeBuilder typeBuilder, FieldBuilder[] fields)
        {
            var builderField = fields[1];

            var parameterTypes = new Type[] { typeof(IAsyncStateMachine) };

            var attrs = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
            var methodBuilder = typeBuilder.DefineMethod("SetStateMachine", attrs, CallingConventions.Standard | CallingConventions.HasThis, null, parameterTypes);
            methodBuilder.DefineParameter(1, ParameterAttributes.None, "stateMachine");

            var cil = methodBuilder.GetILGenerator();

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldflda, builderField);
            cil.Emit(OpCodes.Ldarg_1);
            cil.Emit(OpCodes.Call, _asyncMethodBuilderType.GetMethod(nameof(AsyncTaskMethodBuilder.SetStateMachine), parameterTypes));
            cil.Emit(OpCodes.Ret);
        }

        internal class TaskAwaiterParameterPreparerBuilder : ITaskAwaiterParameterPreparerBuilder
        {
            private readonly ParameterInfo _parameter;
            private readonly TaskAwaiterClassBuilder _parent;

            public TaskAwaiterParameterPreparerBuilder(TaskAwaiterClassBuilder parent, ParameterInfo parameter)
            {
                _parent = parent;
                _parameter = parameter;
            }

            public ITaskAwaiterClassBuilder WithParser(ParameterParser parserMethod)
            {
                _parent.parameterParsers[_parameter] = parserMethod;
                return _parent;
            }
        }
    }
}
