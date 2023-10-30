using AspectSharp.Abstractions;
using AspectSharp.DynamicProxy;
using AspectSharp.DynamicProxy.Utils;
using AspectSharp.Tests.Core.Enums;
using AspectSharp.Tests.Core.TestData.DynamicProxy.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
                foreach(var mi in serviceType.GetMethods())
                {
                    var methodInfo = mi;
                    var targetMethodInfo = targetType.GetMethod(methodInfo);
                    var globalInterceptors = new List<IInterceptor>();
                    foreach (var globalInterceptorConfig in configs.GlobalInterceptors)
                    {
                        if (globalInterceptorConfig.TryGetInterceptor(methodInfo, out var interceptor))
                            globalInterceptors.Add(interceptor);
                    }

                    if (methodInfo.IsGenericMethodDefinition)
                    {
                        try
                        {
                            methodInfo = methodInfo.MakeGenericMethod(new Type[] { typeof(string) });
                        }
                        catch
                        {
                            methodInfo = methodInfo.MakeGenericMethod(new Type[] { typeof(ConcreteAspectContext) });
                        }
                    }
                    var methodName = methodInfo.Name;

                    var parameters = methodInfo.GetParameters().Select(p => p.ParameterType.GetDefault()).ToArray();

                    Action<object> action = proxyInstance =>
                    {
                        try
                        {
                            var ret = methodInfo.Invoke(proxyInstance, parameters);
                            if (!(ret is null))
                            {
                                var retInfo = targetMethodInfo.GetReturnInfo();
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
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                    };

                    var aspectContextAditionalInfo = new List<string>();
                    IEnumerable<CustomAttributeData> interceptors = default;
                    interceptorDictionary?.TryGetValue(methodInfo, out interceptors);
                    //if (!)
                    //{
                    var beforeInterceptorKey = new Dictionary<string, int>();
                        var afterInterceptorKey = new Dictionary<string, int>();
                        foreach (var interceptor in (globalInterceptors.Select(interceptor => interceptor.GetType()) ?? Enumerable.Empty<Type>()).Concat(( interceptors?.Select(attr => attr.AttributeType) ?? Enumerable.Empty<Type>())))
                        {
                            var key = string.Format("{0}: {1} {2}", interceptor.Name, InterceptMoment.Before.ToString().ToLower(), methodInfo.Name);
                            int count = 1;
                            if (beforeInterceptorKey.TryGetValue(key, out var value))
                            {
                                count += value;
                                beforeInterceptorKey[key] = count;
                            }
                            beforeInterceptorKey.Add(string.Format("{0}{1}: {2} {3}", interceptor.Name, (count == 1 ? string.Empty : string.Format(" {0}", count)), InterceptMoment.Before.ToString().ToLower(), methodInfo.Name), count);

                            key = string.Format("{0}: {1} {2}", interceptor.Name, InterceptMoment.After.ToString().ToLower(), methodInfo.Name);
                            count = 1;
                            if (afterInterceptorKey.TryGetValue(key, out value))
                            {
                                count += value;
                                afterInterceptorKey[key] = count;
                            }
                            afterInterceptorKey.Add(string.Format("{0}{1}: {2} {3}", interceptor.Name, (count == 1 ? string.Empty : string.Format(" {0}", count)), InterceptMoment.After.ToString().ToLower(), methodInfo.Name), count);
                        }
                        aspectContextAditionalInfo.AddRange(beforeInterceptorKey.Keys.Concat(afterInterceptorKey.Keys.Reverse()));
                    //}
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
            if (type.IsByRef && type.IsAutoLayout && type.Name.EndsWith("&"))
                return GetDefault(type.GetElementType());
            else if (type.IsValueType)
                return Activator.CreateInstance(type);
            else if (type == typeof(string))
                return "";
            else if (type.IsGenericType && typeof(IEnumerable<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
                return Activator.CreateInstance(typeof(List<>).MakeGenericType(type.GetGenericArguments()[0]));
            return null;
        }
    }
}
