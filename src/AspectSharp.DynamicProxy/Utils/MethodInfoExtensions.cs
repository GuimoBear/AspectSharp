using System;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectSharp.DynamicProxy.Utils
{
    internal static class MethodInfoExtensions
    {
        public static ReturnData GetReturnInfo(this MethodInfo methodInfo)
            => GetReturnInfo(methodInfo.ReturnType);

        public static ReturnData GetReturnInfo(this Type retType)
        {
#if NETCOREAPP3_1_OR_GREATER
            var isValueTask = retType == typeof(ValueTask);
#endif
            var isAsync = retType == typeof(Task)
#if NETCOREAPP3_1_OR_GREATER
                || isValueTask;
#else
                ;
#endif

            var isVoid = retType == typeof(void)
#if NETCOREAPP3_1_OR_GREATER
                || isValueTask
#endif
                || isAsync;

            if (retType.IsGenericType)
            {
                var genericTypeDefinition = retType.GetGenericTypeDefinition();
#if NETCOREAPP3_1_OR_GREATER
                    isValueTask = genericTypeDefinition == typeof(ValueTask<>);
#endif
                isAsync = genericTypeDefinition == typeof(Task<>)
#if NETCOREAPP3_1_OR_GREATER
                || isValueTask;
#else
                ;
#endif
                if (isAsync)
                    retType = retType.GetGenericArguments()[0];
            }

#if NETCOREAPP3_1_OR_GREATER
            return new ReturnData(retType, isVoid, isAsync, isValueTask);
#else
            return new ReturnData(retType, isVoid, isAsync);
#endif
        }

        internal sealed class ReturnData
        {
            public Type Type { get; }
            public bool IsVoid { get; }
            public bool IsAsync { get; }

#if NETCOREAPP3_1_OR_GREATER
            public bool IsValueTask { get; }

            public ReturnData(Type type, bool isVoid, bool isAsync, bool isValueTask)
                : this(type, isVoid, isAsync)
            {
                IsValueTask = isValueTask;
            }
#endif

            public ReturnData(Type type, bool isVoid, bool isAsync)
            {
                Type = type;
                IsVoid = isVoid;
                IsAsync = isAsync;
            }
        }
    }
}
