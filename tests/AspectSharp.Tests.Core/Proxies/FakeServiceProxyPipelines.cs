﻿using AspectSharp.Abstractions;
using AspectSharp.Abstractions.Attributes;
using AspectSharp.DynamicProxy;
using AspectSharp.DynamicProxy.Utils;
using AspectSharp.Tests.Core.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspectSharp.Tests.Core.Proxies
{
    public static class FakeServiceProxyPipelines
    {
        public static InterceptDelegate Pipeline1 { get; }
        public static InterceptDelegate Pipeline2 { get; }
        public static InterceptDelegate Pipeline3 { get; }
#if NETCOREAPP3_1_OR_GREATER
        public static InterceptDelegate Pipeline4 { get; }
        public static InterceptDelegate Pipeline5 { get; }
#endif
        public static InterceptDelegate Pipeline6 { get; }
        public static InterceptDelegate Pipeline7 { get; }
        public static InterceptDelegate Pipeline8 { get; }
        public static InterceptDelegate Pipeline9 { get; }
        public static InterceptDelegate Pipeline10 { get; }
        public static InterceptDelegate Pipeline11 { get; }
#if NETCOREAPP3_1_OR_GREATER
        public static InterceptDelegate Pipeline12 { get; }
        public static InterceptDelegate Pipeline13 { get; }
#endif
        public static InterceptDelegate Pipeline14 { get; }
        public static InterceptDelegate Pipeline15 { get; }
#if NETCOREAPP3_1_OR_GREATER
        public static InterceptDelegate Pipeline16 { get; }
        public static InterceptDelegate Pipeline17 { get; }
#endif

        static FakeServiceProxyPipelines()
        {
            var serviceType = typeof(IFakeService);
            AspectDelegate aspectDelegate = default;
            AbstractInterceptorAttribute[] aspectsFromDelegate = default;

            aspectDelegate = AspectDelegate1;
            aspectsFromDelegate = ProxyFactoryUtils.GetInterceptors(serviceType, -1349792768, 43942917);
            Pipeline1 = ProxyFactoryUtils.CreatePipeline(aspectDelegate, aspectsFromDelegate);

            aspectDelegate = AspectDelegate2;
            aspectsFromDelegate = ProxyFactoryUtils.GetInterceptors(serviceType, -1349792768, 59941933);
            Pipeline2 = ProxyFactoryUtils.CreatePipeline(aspectDelegate, aspectsFromDelegate);

            aspectDelegate = AspectDelegate3;
            aspectsFromDelegate = ProxyFactoryUtils.GetInterceptors(serviceType, -1349792768, 2606490);
            Pipeline3 = ProxyFactoryUtils.CreatePipeline(aspectDelegate, aspectsFromDelegate);

            aspectDelegate = AspectDelegate3;
            aspectsFromDelegate = ProxyFactoryUtils.GetInterceptors(serviceType, -1349792768, 2606490);
            Pipeline3 = ProxyFactoryUtils.CreatePipeline(aspectDelegate, aspectsFromDelegate);

#if NETCOREAPP3_1_OR_GREATER
            aspectDelegate = AspectDelegate4;
            aspectsFromDelegate = ProxyFactoryUtils.GetInterceptors(serviceType, -1349792768, 23458411);
            Pipeline4 = ProxyFactoryUtils.CreatePipeline(aspectDelegate, aspectsFromDelegate);

            aspectDelegate = AspectDelegate5;
            aspectsFromDelegate = ProxyFactoryUtils.GetInterceptors(serviceType, -1349792768, 9799115);
            Pipeline5 = ProxyFactoryUtils.CreatePipeline(aspectDelegate, aspectsFromDelegate);
#endif
            aspectDelegate = AspectDelegate6;
            aspectsFromDelegate = ProxyFactoryUtils.GetInterceptors(serviceType, -1349792768, 21083178);
            Pipeline6 = ProxyFactoryUtils.CreatePipeline(aspectDelegate, aspectsFromDelegate);

            aspectDelegate = AspectDelegate7;
            aspectsFromDelegate = ProxyFactoryUtils.GetInterceptors(serviceType, -1349792768, 55530882);
            Pipeline7 = ProxyFactoryUtils.CreatePipeline(aspectDelegate, aspectsFromDelegate);

            aspectDelegate = AspectDelegate8;
            aspectsFromDelegate = ProxyFactoryUtils.GetInterceptors(serviceType, -1349792768, 30015890);
            Pipeline8 = ProxyFactoryUtils.CreatePipeline(aspectDelegate, aspectsFromDelegate);

            aspectDelegate = AspectDelegate9;
            aspectsFromDelegate = ProxyFactoryUtils.GetInterceptors(serviceType, -1349792768, 1707556);
            Pipeline9 = ProxyFactoryUtils.CreatePipeline(aspectDelegate, aspectsFromDelegate);

            aspectDelegate = AspectDelegate10;
            aspectsFromDelegate = ProxyFactoryUtils.GetInterceptors(serviceType, -1349792768, 15368010);
            Pipeline10 = ProxyFactoryUtils.CreatePipeline(aspectDelegate, aspectsFromDelegate);

            aspectDelegate = AspectDelegate11;
            aspectsFromDelegate = ProxyFactoryUtils.GetInterceptors(serviceType, -1349792768, 4094363);
            Pipeline11 = ProxyFactoryUtils.CreatePipeline(aspectDelegate, aspectsFromDelegate);

#if NETCOREAPP3_1_OR_GREATER
            aspectDelegate = AspectDelegate12;
            aspectsFromDelegate = ProxyFactoryUtils.GetInterceptors(serviceType, -1349792768, 36849274);
            Pipeline12 = ProxyFactoryUtils.CreatePipeline(aspectDelegate, aspectsFromDelegate);

            aspectDelegate = AspectDelegate13;
            aspectsFromDelegate = ProxyFactoryUtils.GetInterceptors(serviceType, -1349792768, 63208015);
            Pipeline13 = ProxyFactoryUtils.CreatePipeline(aspectDelegate, aspectsFromDelegate);
#endif
            aspectDelegate = AspectDelegate14;
            aspectsFromDelegate = ProxyFactoryUtils.GetInterceptors(serviceType, -1349792768, 63208015);
            Pipeline14 = ProxyFactoryUtils.CreatePipeline(aspectDelegate, aspectsFromDelegate);

            aspectDelegate = AspectDelegate15;
            aspectsFromDelegate = ProxyFactoryUtils.GetInterceptors(serviceType, -1349792768, 19575591);
            Pipeline15 = ProxyFactoryUtils.CreatePipeline(aspectDelegate, aspectsFromDelegate);

#if NETCOREAPP3_1_OR_GREATER
            aspectDelegate = AspectDelegate16;
            aspectsFromDelegate = ProxyFactoryUtils.GetInterceptors(serviceType, -1349792768, 41962596);
            Pipeline16 = ProxyFactoryUtils.CreatePipeline(aspectDelegate, aspectsFromDelegate);

            aspectDelegate = AspectDelegate17;
            aspectsFromDelegate = ProxyFactoryUtils.GetInterceptors(serviceType, -1349792768, 42119052);
            Pipeline17 = ProxyFactoryUtils.CreatePipeline(aspectDelegate, aspectsFromDelegate);
#endif
        }

        private static Task AspectDelegate1(AspectContext aspectContext)
        {
            IFakeService target;
            target = aspectContext.Target as IFakeService;
            target.InterceptedDoSomethingWithoutParameterAndWithoutReturn();
            return Task.CompletedTask;
        }

        private static Task AspectDelegate2(AspectContext aspectContext)
        {
            IFakeService target;
            int param1;
            string param2;
            IEnumerable<string> param3;
            target = aspectContext.Target as IFakeService;
            param1 = (int)aspectContext.Parameters[0];
            param2 = (string)aspectContext.Parameters[1];
            param3 = (IEnumerable<string>)aspectContext.Parameters[2];
            target.InterceptedDoSomethingWithParameterAndWithoutReturn(param1, param2, param3);
            return Task.CompletedTask;
        }

        private static Task AspectDelegate3(AspectContext aspectContext)
        {
            IFakeService target;
            target = aspectContext.Target as IFakeService;
            return target.InterceptedDoSomethingAsyncWithoutParameterAndWithoutReturn();
        }

#if NETCOREAPP3_1_OR_GREATER
        private static Task AspectDelegate4(AspectContext aspectContext)
        {
            IFakeService target;
            target = aspectContext.Target as IFakeService;
            target.InterceptedDoSomethingValueAsyncWithoutParameterAndWithoutReturn().AsTask().Wait();
            return Task.CompletedTask;
        }

        private static Task AspectDelegate5(AspectContext aspectContext)
        {
            IFakeService target;
            int param1;
            string param2;
            IEnumerable<string> param3;
            target = aspectContext.Target as IFakeService;
            param1 = (int)aspectContext.Parameters[0];
            param2 = (string)aspectContext.Parameters[1];
            param3 = (IEnumerable<string>)aspectContext.Parameters[2];
            target.InterceptedDoSomethingValueAsyncWithParameterAndWithoutReturn(param1, param2, param3).AsTask().Wait();
            return Task.CompletedTask;
        }
#endif

        private static Task AspectDelegate6(AspectContext aspectContext)
        {
            IFakeService target;
            target = aspectContext.Target as IFakeService;
            aspectContext.ReturnValue = target.InterceptedDoSomethingWithoutParameterAndValueTypeReturn();
            return Task.CompletedTask;
        }

        private static Task AspectDelegate7(AspectContext aspectContext)
        {
            IFakeService target;
            int param1;
            string param2;
            IEnumerable<string> param3;
            target = aspectContext.Target as IFakeService;
            param1 = (int)aspectContext.Parameters[0];
            param2 = (string)aspectContext.Parameters[1];
            param3 = (IEnumerable<string>)aspectContext.Parameters[2];
            aspectContext.ReturnValue = target.InterceptedDoSomethingWithParameterAndValueTypeReturn(param1, param2, param3);
            return Task.CompletedTask;
        }

        private static Task AspectDelegate8(AspectContext aspectContext)
        {
            IFakeService target;
            target = aspectContext.Target as IFakeService;
            aspectContext.ReturnValue = target.InterceptedDoSomethingWithoutParameterAndReferenceTypeReturn();
            return Task.CompletedTask;
        }

        private static Task AspectDelegate9(AspectContext aspectContext)
        {
            IFakeService target;
            int param1;
            string param2;
            IEnumerable<string> param3;
            target = aspectContext.Target as IFakeService;
            param1 = (int)aspectContext.Parameters[0];
            param2 = (string)aspectContext.Parameters[1];
            param3 = (IEnumerable<string>)aspectContext.Parameters[2];
            aspectContext.ReturnValue = target.InterceptedDoSomethingWithParameterAndReferenceTypeReturn(param1, param2, param3);
            return Task.CompletedTask;
        }

        private static Task AspectDelegate10(AspectContext aspectContext)
        {
            IFakeService target;
            target = aspectContext.Target as IFakeService;
            aspectContext.ReturnValue = target.InterceptedDoSomethingAsyncWithoutParameterAndValueTypeReturn().Result;
            return Task.CompletedTask;
        }

        private static Task AspectDelegate11(AspectContext aspectContext)
        {
            IFakeService target;
            int param1;
            string param2;
            IEnumerable<string> param3;
            target = aspectContext.Target as IFakeService;
            param1 = (int)aspectContext.Parameters[0];
            param2 = (string)aspectContext.Parameters[1];
            param3 = (IEnumerable<string>)aspectContext.Parameters[2];
            aspectContext.ReturnValue = target.InterceptedDoSomethingAsyncWithParameterAndValueTypeReturn(param1, param2, param3).Result;
            return Task.CompletedTask;
        }

#if NETCOREAPP3_1_OR_GREATER
        private static Task AspectDelegate12(AspectContext aspectContext)
        {
            IFakeService target;
            target = aspectContext.Target as IFakeService;
            aspectContext.ReturnValue = target.InterceptedDoSomethingValueAsyncWithoutParameterAndValueTypeReturn().Result;
            return Task.CompletedTask;
        }

        private static Task AspectDelegate13(AspectContext aspectContext)
        {
            IFakeService target;
            int param1;
            string param2;
            IEnumerable<string> param3;
            target = aspectContext.Target as IFakeService;
            param1 = (int)aspectContext.Parameters[0];
            param2 = (string)aspectContext.Parameters[1];
            param3 = (IEnumerable<string>)aspectContext.Parameters[2];
            aspectContext.ReturnValue = target.InterceptedDoSomethingValueAsyncWithParameterAndValueTypeReturn(param1, param2, param3).Result;
            return Task.CompletedTask;
        }
#endif

        private static Task AspectDelegate14(AspectContext aspectContext)
        {
            IFakeService target;
            target = aspectContext.Target as IFakeService;
            aspectContext.ReturnValue = target.InterceptedDoSomethingAsyncWithoutParameterAndReferenceypeReturn().Result;
            return Task.CompletedTask;
        }

        private static Task AspectDelegate15(AspectContext aspectContext)
        {
            IFakeService target;
            int param1;
            string param2;
            IEnumerable<string> param3;
            target = aspectContext.Target as IFakeService;
            param1 = (int)aspectContext.Parameters[0];
            param2 = (string)aspectContext.Parameters[1];
            param3 = (IEnumerable<string>)aspectContext.Parameters[2];
            aspectContext.ReturnValue = target.InterceptedDoSomethingAsyncWithParameterAndReferenceTypeReturn(param1, param2, param3).Result;
            return Task.CompletedTask;
        }

#if NETCOREAPP3_1_OR_GREATER
        private static Task AspectDelegate16(AspectContext aspectContext)
        {
            IFakeService target;
            target = aspectContext.Target as IFakeService;
            aspectContext.ReturnValue = target.InterceptedDoSomethingValueAsyncWithoutParameterAndReferenceTypeReturn().Result;
            return Task.CompletedTask;
        }

        private static Task AspectDelegate17(AspectContext aspectContext)
        {
            IFakeService target;
            int param1;
            string param2;
            IEnumerable<string> param3;
            target = aspectContext.Target as IFakeService;
            param1 = (int)aspectContext.Parameters[0];
            param2 = (string)aspectContext.Parameters[1];
            param3 = (IEnumerable<string>)aspectContext.Parameters[2];
            aspectContext.ReturnValue = target.InterceptedDoSomethingValueAsyncWithParameterAndReferenceTypeReturn(param1, param2, param3).Result;
            return Task.CompletedTask;
        }
#endif
    }
}