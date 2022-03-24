using AspectSharp.DynamicProxy.Factories.Builders.Classes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AspectSharp.DynamicProxy.Factories.Builders.Classes
{
    internal sealed class InstanceTaskAwaiterClassBuilder : IInstanceTaskAwaiterClassBuilder
    {
        private static readonly Type _asyncStateMachineType = typeof(IAsyncStateMachine);

        private static readonly Type _exceptionType = typeof(Exception);

        private readonly TypeBuilder _parentType;
        private readonly ModuleBuilder _moduleBuilder;
        private readonly string _nestedTypeName;
        private readonly MethodBuilder _callerMethod;
        private readonly Type _thisType;
        private readonly ParameterInfo[] _callerParameters;
        private Type _asyncMethodBuilderType;
        internal IDictionary<ParameterInfo, InstanceParameterParser> parameterParsers;
        private InstanceAwaiterConfigurer _configurer;
        private Type _awaitableType;
        private Type _taskAwaiterType;

        private bool _hasShortRamification = true;

        private TaskCompleted _taskCompletedMethod;
        private TaskCompletedWithReturn _taskCompletedWithReturnMethod;

        private FieldBuilder _methodBuilderField;
        private FieldBuilder _thisField;
        private FieldBuilder[] _parameterFields;
        private FieldBuilder _stateField;

        public FieldBuilder StateField => _stateField;
        public FieldBuilder MethodBuilderField => _methodBuilderField;
        public FieldBuilder ThisField => _thisField;
        public FieldBuilder[] ParameterFields => _parameterFields;

        public InstanceTaskAwaiterClassBuilder(TypeBuilder parentType, ModuleBuilder moduleBuilder, string nestedTypeName, MethodBuilder callerMethod, ParameterInfo[] callerParameters)
        {
            _parentType = parentType ?? throw new ArgumentNullException(nameof(parentType));
            _moduleBuilder = moduleBuilder ?? throw new ArgumentNullException(nameof(moduleBuilder));
            _nestedTypeName = nestedTypeName ?? throw new ArgumentNullException(nameof(nestedTypeName));
            _callerMethod = callerMethod ?? throw new ArgumentNullException(nameof(callerMethod));
            _thisType = _callerMethod.DeclaringType;
            _callerParameters = callerParameters ?? throw new ArgumentNullException(nameof(callerParameters));
            parameterParsers = new Dictionary<ParameterInfo, InstanceParameterParser>();
        }

        public IInstanceTaskAwaiterParameterPreparerBuilder PrepareParameter(int parameterIndex)
        {
            if (_callerParameters is null || parameterIndex < 0 || parameterIndex >= _callerParameters.Length)
                throw new ArgumentOutOfRangeException(nameof(parameterIndex));
            return new InstanceTaskAwaiterParameterPreparerBuilder(this, _callerParameters[parameterIndex]);
        }

        public IInstanceTaskAwaiterClassBuilder WithAwaiter(Type awaitableType, InstanceAwaiterConfigurer awaiterConfigurerMethod)
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
                    _taskAwaiterType = typeof(TaskAwaiter);
                }
                else if (_awaitableType.IsGenericType && _awaitableType.GetGenericTypeDefinition() == typeof(ValueTask<>))
                {
                    _asyncMethodBuilderType = typeof(AsyncValueTaskMethodBuilder<>).MakeGenericType(_awaitableType.GetGenericArguments());
                    _taskAwaiterType = typeof(TaskAwaiter<>).MakeGenericType(_awaitableType.GetGenericArguments());
                }
#endif
                return this;
            }
            throw new ArgumentException(string.Format("{0} parameter must be an awaitable type", nameof(awaitableType)), nameof(awaitableType));
        }

        public IInstanceTaskAwaiterClassBuilder HasLongAwaiterDefinition()
        {
            _hasShortRamification = false;
            return this;
        }

        public IInstanceTaskAwaiterClassFinalBuilder WithAfterCompletedTask(TaskCompleted taskCompletedMethod)
        {
            _taskCompletedMethod = taskCompletedMethod ?? throw new ArgumentNullException(nameof(taskCompletedMethod));
            return this;
        }

        public IInstanceTaskAwaiterClassFinalBuilder WithAfterCompletedTaskWithResult(TaskCompletedWithReturn taskCompletedMethod)
        {
            _taskCompletedWithReturnMethod = taskCompletedMethod ?? throw new ArgumentNullException(nameof(taskCompletedMethod));
            return this;
        }

        public Type Build()
        {
            var attrs = TypeAttributes.NestedPrivate | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit;
            var awaiterTypeBuilder = _moduleBuilder.DefineType(_nestedTypeName, attrs, typeof(ValueType), new Type[] { typeof(IAsyncStateMachine) });
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
            fields.Add(typeBuilder.DefineField("<>___this", _thisType, FieldAttributes.Public));
            foreach (var parameter in _callerParameters)
                fields.Add(typeBuilder.DefineField(parameter.Name, parameter.ParameterType, FieldAttributes.Public));
            fields.Add(typeBuilder.DefineField("<>__awaiter", _taskAwaiterType, FieldAttributes.Private));
            //if (_taskAwaiterType.IsGenericType)
            //    foreach (var parameter in _callerParameters)
            //        fields.Add(typeBuilder.DefineField(string.Format("{0}_wrap", parameter.Name), parameter.ParameterType, FieldAttributes.Private));
            return fields.ToArray();
        }

        private void DefineMoveNextMethod(TypeBuilder typeBuilder, FieldBuilder[] fields)
        {
            var stateField = _stateField = fields[0];
            var builderField = _methodBuilderField = fields[1];
            var thisField = _thisField = fields[2];
            var parameterFields = new List<FieldBuilder>();
            //var wrappedParameterFields = new List<FieldBuilder>();
            if (_callerParameters.Length > 0)
            {
                parameterFields.AddRange(fields.Skip(3).Take(_callerParameters.Length));
                //if (_taskAwaiterType.IsGenericType)
                //    wrappedParameterFields.AddRange(fields.Skip(3 + _callerParameters.Length).Take(_callerParameters.Length));
            }
            _parameterFields = parameterFields.ToArray();
            var awaiterField = fields.Last();

            var attrs = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
            var methodBuilder = typeBuilder.DefineMethod("MoveNext", attrs, CallingConventions.Standard | CallingConventions.HasThis);

            var cil = methodBuilder.GetILGenerator();

            var localVariables = new List<LocalBuilder>();
            localVariables.Add(cil.DeclareLocal(typeof(int)));
            if (!_taskAwaiterType.IsGenericType)
                localVariables.Add(cil.DeclareLocal(_taskAwaiterType));
            else
            {
                localVariables.Add(cil.DeclareLocal(_taskAwaiterType.GetGenericArguments()[0]));
                localVariables.Add(cil.DeclareLocal(_taskAwaiterType));
            }
            localVariables.Add(cil.DeclareLocal(_exceptionType));
            var hasParameters = parameterParsers.Count > 0;
            var parameters = new List<LocalBuilder>();
            if (hasParameters)
                foreach (var parameter in parameterParsers)
                    parameters.Add(cil.DeclareLocal(parameter.Key.ParameterType));

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldfld, stateField);
            cil.Emit(OpCodes.Stloc_0);

            cil.BeginExceptionBlock();

            var awaiterIsNotInitializedLabel = cil.DefineLabel();
            if (_hasShortRamification)
                cil.Emit(OpCodes.Brfalse_S, awaiterIsNotInitializedLabel);
            else
                cil.Emit(OpCodes.Brfalse, awaiterIsNotInitializedLabel);


            foreach (var tuple in parameters.Zip(parameterParsers, (left, right) => new Tuple<LocalBuilder, KeyValuePair<ParameterInfo, InstanceParameterParser>>(left, right)))
            {
                var localParameter = tuple.Item1;
                //var parameterInfo = tuple.Item2.Key;
                var parameterConfigurer = tuple.Item2.Value;
                parameterConfigurer(cil, typeBuilder, thisField, _parameterFields, localParameter);
            }

            var endOfCatchLabel = cil.DefineLabel();
            var endOfMethodLabel = cil.DefineLabel();

            _configurer(cil, typeBuilder, thisField, parameters.ToArray());
            cil.Emit(OpCodes.Callvirt, _awaitableType.GetMethod("GetAwaiter", Type.EmptyTypes));
            cil.Emit(OpCodes.Stloc_S, _taskAwaiterType.IsGenericType ? 2 : 1);
            cil.Emit(OpCodes.Ldloca_S, _taskAwaiterType.IsGenericType ? 2 : 1);

            cil.Emit(OpCodes.Call, _taskAwaiterType.GetProperty(nameof(Task.IsCompleted)).GetGetMethod());

            var taskCompletedLabel = cil.DefineLabel();
            cil.Emit(OpCodes.Brtrue_S, taskCompletedLabel);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldc_I4_0);
            cil.Emit(OpCodes.Dup);
            cil.Emit(OpCodes.Stloc_0);
            cil.Emit(OpCodes.Stfld, stateField);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldloc_S, _taskAwaiterType.IsGenericType ? 2 : 1);
            cil.Emit(OpCodes.Stfld, awaiterField);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldflda, builderField);
            cil.Emit(OpCodes.Ldloca_S, _taskAwaiterType.IsGenericType ? 2 : 1);
            cil.Emit(OpCodes.Ldarg_0);
            var method = _asyncMethodBuilderType.GetMethod(nameof(AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted)).MakeGenericMethod(_taskAwaiterType, typeBuilder);
            cil.Emit(OpCodes.Call, method);

            cil.Emit(OpCodes.Leave_S, endOfMethodLabel);

            cil.MarkLabel(awaiterIsNotInitializedLabel);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldfld, awaiterField);
            cil.Emit(OpCodes.Stloc_S, _taskAwaiterType.IsGenericType ? 2 : 1);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldflda, awaiterField);
            cil.Emit(OpCodes.Initobj, _taskAwaiterType);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldc_I4_M1);
            cil.Emit(OpCodes.Dup);
            cil.Emit(OpCodes.Stloc_0);
            cil.Emit(OpCodes.Stfld, stateField);

            cil.MarkLabel(taskCompletedLabel);

            if (_taskAwaiterType.IsGenericType)
            {
                cil.Emit(OpCodes.Ldloca_S, 2);
                cil.Emit(OpCodes.Call, _taskAwaiterType.GetMethod(nameof(TaskAwaiter<int>.GetResult), Type.EmptyTypes));
                cil.Emit(OpCodes.Stloc, 1);
            }

            if (!(_taskCompletedMethod is null))
                _taskCompletedMethod(cil, typeBuilder, builderField, parameterFields.ToArray());
            else
                _taskCompletedWithReturnMethod(cil, typeBuilder, builderField, parameterFields.ToArray(), localVariables[1]);
            cil.Emit(OpCodes.Leave_S, endOfCatchLabel);

            cil.BeginCatchBlock(_exceptionType);
            var exceptionLocalIndex = localVariables.Last().LocalIndex;
            cil.Emit(OpCodes.Stloc_S, exceptionLocalIndex);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldc_I4_S, -2);
            cil.Emit(OpCodes.Stfld, stateField);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldflda, builderField);
            cil.Emit(OpCodes.Ldloc, exceptionLocalIndex);
            cil.Emit(OpCodes.Ldloc, _asyncMethodBuilderType.GetMethod(nameof(AsyncTaskMethodBuilder.SetException), new Type[] { typeof(Exception) }));
            cil.Emit(OpCodes.Leave_S, endOfMethodLabel);
            cil.EndExceptionBlock();
            cil.MarkLabel(endOfCatchLabel);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldc_I4_S, -2);
            cil.Emit(OpCodes.Stfld, stateField);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldflda, builderField);
            if (_asyncMethodBuilderType.IsGenericType)
            {
                cil.Emit(OpCodes.Ldloc, 1);
                cil.Emit(OpCodes.Call, _asyncMethodBuilderType.GetMethod(nameof(AsyncTaskMethodBuilder.SetResult), new Type[] { _asyncMethodBuilderType.GetGenericArguments()[0] }));
            }
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

        internal sealed class InstanceTaskAwaiterParameterPreparerBuilder : IInstanceTaskAwaiterParameterPreparerBuilder
        {
            private readonly ParameterInfo _parameter;
            private readonly InstanceTaskAwaiterClassBuilder _parent;

            public InstanceTaskAwaiterParameterPreparerBuilder(InstanceTaskAwaiterClassBuilder parent, ParameterInfo parameter)
            {
                _parent = parent;
                _parameter = parameter;
            }

            public IInstanceTaskAwaiterClassBuilder WithParser(InstanceParameterParser parserMethod)
            {
                _parent.parameterParsers[_parameter] = parserMethod;
                return _parent;
            }
        }

        private struct AsyncStateMachine1 : IAsyncStateMachine
        {
            public int state;
            public AsyncTaskMethodBuilder<string> builder;
            private TaskAwaiter awaiter;

            public void MoveNext()
            {

            }

            public void SetStateMachine(IAsyncStateMachine stateMachine)
            {
                throw new NotImplementedException();
            }
        }
    }
}
