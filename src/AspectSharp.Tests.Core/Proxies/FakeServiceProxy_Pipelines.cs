using AspectSharp.Abstractions;
using AspectSharp.Abstractions.Attributes;
using AspectSharp.DynamicProxy;
using AspectSharp.DynamicProxy.Utils;
using AspectSharp.Tests.Core.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspectSharp.Tests.Core.Proxies
{
    public sealed class FakeServiceProxy_Pipelines
    {
        public InterceptDelegate Pipeline1 { get; }
        public InterceptDelegate Pipeline2 { get; }
        public InterceptDelegate Pipeline3 { get; }
#if NETCOREAPP3_1_OR_GREATER
        public InterceptDelegate Pipeline4 { get; }
        public InterceptDelegate Pipeline5 { get; }
#endif
        public InterceptDelegate Pipeline6 { get; }
        public InterceptDelegate Pipeline7 { get; }
        public InterceptDelegate Pipeline8 { get; }
        public InterceptDelegate Pipeline9 { get; }
        public InterceptDelegate Pipeline10 { get; }
        public InterceptDelegate Pipeline11 { get; }
#if NETCOREAPP3_1_OR_GREATER
        public InterceptDelegate Pipeline12 { get; }
        public InterceptDelegate Pipeline13 { get; }
#endif
        public InterceptDelegate Pipeline14 { get; }
        public InterceptDelegate Pipeline15 { get; }
#if NETCOREAPP3_1_OR_GREATER
        public InterceptDelegate Pipeline16 { get; }
        public InterceptDelegate Pipeline17 { get; }
#endif

        public FakeServiceProxy_Pipelines()
        {
            Pipeline1 = ProxyFactoryUtils.CreatePipeline(_aspectDelegate1, _aspectsFromDelegate1);
            Pipeline2 = ProxyFactoryUtils.CreatePipeline(_aspectDelegate2, _aspectsFromDelegate2);
            Pipeline3 = ProxyFactoryUtils.CreatePipeline(_aspectDelegate3, _aspectsFromDelegate3);
#if NETCOREAPP3_1_OR_GREATER
            Pipeline4 = ProxyFactoryUtils.CreatePipeline(_aspectDelegate4, _aspectsFromDelegate4);
            Pipeline5 = ProxyFactoryUtils.CreatePipeline(_aspectDelegate5, _aspectsFromDelegate5);
#endif
            Pipeline6 = ProxyFactoryUtils.CreatePipeline(_aspectDelegate6, _aspectsFromDelegate6);
            Pipeline7 = ProxyFactoryUtils.CreatePipeline(_aspectDelegate7, _aspectsFromDelegate7);
            Pipeline8 = ProxyFactoryUtils.CreatePipeline(_aspectDelegate8, _aspectsFromDelegate8);
            Pipeline9 = ProxyFactoryUtils.CreatePipeline(_aspectDelegate9, _aspectsFromDelegate9);
            Pipeline10 = ProxyFactoryUtils.CreatePipeline(_aspectDelegate10, _aspectsFromDelegate10);
            Pipeline11 = ProxyFactoryUtils.CreatePipeline(_aspectDelegate11, _aspectsFromDelegate11);
#if NETCOREAPP3_1_OR_GREATER
            Pipeline12 = ProxyFactoryUtils.CreatePipeline(_aspectDelegate12, _aspectsFromDelegate12);
            Pipeline13 = ProxyFactoryUtils.CreatePipeline(_aspectDelegate13, _aspectsFromDelegate13);
#endif
            Pipeline14 = ProxyFactoryUtils.CreatePipeline(_aspectDelegate14, _aspectsFromDelegate14);
            Pipeline15 = ProxyFactoryUtils.CreatePipeline(_aspectDelegate15, _aspectsFromDelegate15);
#if NETCOREAPP3_1_OR_GREATER
            Pipeline16 = ProxyFactoryUtils.CreatePipeline(_aspectDelegate16, _aspectsFromDelegate16);
            Pipeline17 = ProxyFactoryUtils.CreatePipeline(_aspectDelegate17, _aspectsFromDelegate17);
#endif
        }

        private static readonly AbstractInterceptorAttribute[] _aspectsFromDelegate1;
        private static readonly AbstractInterceptorAttribute[] _aspectsFromDelegate2;
        private static readonly AbstractInterceptorAttribute[] _aspectsFromDelegate3;
#if NETCOREAPP3_1_OR_GREATER
        private static readonly AbstractInterceptorAttribute[] _aspectsFromDelegate4;
        private static readonly AbstractInterceptorAttribute[] _aspectsFromDelegate5;
#endif
        private static readonly AbstractInterceptorAttribute[] _aspectsFromDelegate6;
        private static readonly AbstractInterceptorAttribute[] _aspectsFromDelegate7;
        private static readonly AbstractInterceptorAttribute[] _aspectsFromDelegate8;
        private static readonly AbstractInterceptorAttribute[] _aspectsFromDelegate9;
        private static readonly AbstractInterceptorAttribute[] _aspectsFromDelegate10;
        private static readonly AbstractInterceptorAttribute[] _aspectsFromDelegate11;
#if NETCOREAPP3_1_OR_GREATER
        private static readonly AbstractInterceptorAttribute[] _aspectsFromDelegate12;
        private static readonly AbstractInterceptorAttribute[] _aspectsFromDelegate13;
#endif
        private static readonly AbstractInterceptorAttribute[] _aspectsFromDelegate14;
        private static readonly AbstractInterceptorAttribute[] _aspectsFromDelegate15;
#if NETCOREAPP3_1_OR_GREATER
        private static readonly AbstractInterceptorAttribute[] _aspectsFromDelegate16;
        private static readonly AbstractInterceptorAttribute[] _aspectsFromDelegate17;
#endif

        private static readonly AspectDelegate _aspectDelegate1;
        private static readonly AspectDelegate _aspectDelegate2;
        private static readonly AspectDelegate _aspectDelegate3;
#if NETCOREAPP3_1_OR_GREATER
        private static readonly AspectDelegate _aspectDelegate4;
        private static readonly AspectDelegate _aspectDelegate5;
#endif
        private static readonly AspectDelegate _aspectDelegate6;
        private static readonly AspectDelegate _aspectDelegate7;
        private static readonly AspectDelegate _aspectDelegate8;
        private static readonly AspectDelegate _aspectDelegate9;
        private static readonly AspectDelegate _aspectDelegate10;
        private static readonly AspectDelegate _aspectDelegate11;
#if NETCOREAPP3_1_OR_GREATER
        private static readonly AspectDelegate _aspectDelegate12;
        private static readonly AspectDelegate _aspectDelegate13;
#endif
        private static readonly AspectDelegate _aspectDelegate14;
        private static readonly AspectDelegate _aspectDelegate15;
#if NETCOREAPP3_1_OR_GREATER
        private static readonly AspectDelegate _aspectDelegate16;
        private static readonly AspectDelegate _aspectDelegate17;
#endif

        static FakeServiceProxy_Pipelines()
        {
            var serviceType = typeof(IFakeService);

            _aspectDelegate1 = AspectDelegate1;
            _aspectsFromDelegate1 = ProxyFactoryUtils.GetInterceptors(serviceType, -1349792768, 43942917);

            _aspectDelegate2 = AspectDelegate2;
            _aspectsFromDelegate2 = ProxyFactoryUtils.GetInterceptors(serviceType, -1349792768, 59941933);

            _aspectDelegate3 = AspectDelegate3;
            _aspectsFromDelegate3 = ProxyFactoryUtils.GetInterceptors(serviceType, -1349792768, 2606490);

#if NETCOREAPP3_1_OR_GREATER
            _aspectDelegate4 = AspectDelegate4;
            _aspectsFromDelegate4 = ProxyFactoryUtils.GetInterceptors(serviceType, -1349792768, 23458411);

            _aspectDelegate5 = AspectDelegate5;
            _aspectsFromDelegate5 = ProxyFactoryUtils.GetInterceptors(serviceType, -1349792768, 9799115);
#endif
            _aspectDelegate6 = AspectDelegate6;
            _aspectsFromDelegate6 = ProxyFactoryUtils.GetInterceptors(serviceType, -1349792768, 21083178);

            _aspectDelegate7 = AspectDelegate7;
            _aspectsFromDelegate7 = ProxyFactoryUtils.GetInterceptors(serviceType, -1349792768, 55530882);

            _aspectDelegate8 = AspectDelegate8;
            _aspectsFromDelegate8 = ProxyFactoryUtils.GetInterceptors(serviceType, -1349792768, 30015890);

            _aspectDelegate9 = AspectDelegate9;
            _aspectsFromDelegate9 = ProxyFactoryUtils.GetInterceptors(serviceType, -1349792768, 1707556);

            _aspectDelegate10 = AspectDelegate10;
            _aspectsFromDelegate10 = ProxyFactoryUtils.GetInterceptors(serviceType, -1349792768, 15368010);

            _aspectDelegate11 = AspectDelegate11;
            _aspectsFromDelegate11 = ProxyFactoryUtils.GetInterceptors(serviceType, -1349792768, 4094363);

#if NETCOREAPP3_1_OR_GREATER
            _aspectDelegate12 = AspectDelegate12;
            _aspectsFromDelegate12 = ProxyFactoryUtils.GetInterceptors(serviceType, -1349792768, 36849274);

            _aspectDelegate13 = AspectDelegate13;
            _aspectsFromDelegate13 = ProxyFactoryUtils.GetInterceptors(serviceType, -1349792768, 63208015);
#endif
            _aspectDelegate14 = AspectDelegate14;
            _aspectsFromDelegate14 = ProxyFactoryUtils.GetInterceptors(serviceType, -1349792768, 63208015);

            _aspectDelegate15 = AspectDelegate15;
            _aspectsFromDelegate15 = ProxyFactoryUtils.GetInterceptors(serviceType, -1349792768, 19575591);

#if NETCOREAPP3_1_OR_GREATER
            _aspectDelegate16 = AspectDelegate16;
            _aspectsFromDelegate16 = ProxyFactoryUtils.GetInterceptors(serviceType, -1349792768, 41962596);

            _aspectDelegate17 = AspectDelegate17;
            _aspectsFromDelegate17 = ProxyFactoryUtils.GetInterceptors(serviceType, -1349792768, 42119052);
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
