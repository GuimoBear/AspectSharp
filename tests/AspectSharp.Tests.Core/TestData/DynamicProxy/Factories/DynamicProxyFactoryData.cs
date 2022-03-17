using AspectSharp.Abstractions;
using AspectSharp.DynamicProxy;
using AspectSharp.DynamicProxy.Utils;
using AspectSharp.Tests.Core.Enums;
using AspectSharp.Tests.Core.TestData.DynamicProxy.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AspectSharp.Tests.Core.TestData.DynamicProxy.Factories
{
    internal static class DynamicProxyFactoryData
    {
        public static IEnumerable<Tuple<Type, Type, DynamicProxyFactoryConfigurations, IDictionary<MethodInfo, Tuple<Action<object>, IEnumerable<string>>>>> ProxyClassAspectsPipelineTheoryData()
        {
            foreach(var tuple in InterceptorTypeCacheData.GetInterceptorTypeDataTheoryData())
            {
                var serviceType = tuple.Item1;
                var targetType = tuple.Item2;
                var configs = tuple.Item3;
                var interceptorDictionary = tuple.Item5;

                IDictionary<MethodInfo, Tuple<Action<object>, IEnumerable<string>>> methodCallData = new Dictionary<MethodInfo, Tuple<Action<object>, IEnumerable<string>>>();
                foreach(var methodInfo in serviceType.GetMethods())
                {
                    var parameters = methodInfo.GetParameters().Select(p => p.ParameterType.GetDefault()).ToArray();

                    Action<object> action = proxyInstance =>
                    {
                        var ret = methodInfo.Invoke(proxyInstance, parameters);
                        if (!(ret is null))
                        {
                            var retInfo = ret.GetType().GetReturnInfo();
                            if (retInfo.IsAsync)
                            {
#if NETCOREAPP3_1_OR_GREATER
                                if (retInfo.IsValueTask)
                                {
                                    if (!retInfo.IsVoid)
                                    {
                                        var getResultMethod = typeof(ValueTask<>).MakeGenericType(retInfo.Type).GetProperty(nameof(ValueTask<int>.Result)).GetGetMethod();
                                        getResultMethod.Invoke(ret, Array.Empty<object>());
                                    }
                                    else
                                        ((ValueTask)ret).AsTask().Wait();
                                }
                                else
#endif
                                    (ret as Task).Wait();
                            }
                        }
                    };

                    var aspectContextAditionalInfo = new List<string>();
                    if (!(interceptorDictionary is null) && interceptorDictionary.TryGetValue(methodInfo, out var interceptors))
                    {
                        //interceptors = interceptors.Reverse();
                        aspectContextAditionalInfo.AddRange(
                            interceptors
                                .Select(attr => string.Format("{0}: {1} {2}", attr.AttributeType.Name.PadRight(9), InterceptMoment.Before.ToString().ToLower(), methodInfo.Name))
                                .Concat(interceptors.Reverse()
                                    .Select(attr => string.Format("{0}: {1} {2}", attr.AttributeType.Name.PadRight(9), InterceptMoment.After.ToString().ToLower(), methodInfo.Name))));
                    }
                    methodCallData.Add(methodInfo, new Tuple<Action<object>, IEnumerable<string>>(action, aspectContextAditionalInfo));
                }

                yield return new Tuple<Type, Type, DynamicProxyFactoryConfigurations, IDictionary<MethodInfo, Tuple<Action<object>, IEnumerable<string>>>>
                (
                    serviceType,
                    targetType,
                    configs,
                    methodCallData
                );
            }
        }
        private static object GetDefault(this Type type)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);
            else if (type == typeof(string))
                return "";
            else if (type.IsGenericType && typeof(IEnumerable<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
                return Activator.CreateInstance(typeof(List<>).MakeGenericType(type.GetGenericArguments()[0]));
            return null;
        }
    }
}
