using AspectSharp.Abstractions;
using AspectSharp.Abstractions.Attributes;
using AspectSharp.DynamicProxy;
using AspectSharp.DynamicProxy.Utils;
using AspectSharp.Tests.Core.Aspects;
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
        public InterceptDelegate Pipeline4 { get; }
        public InterceptDelegate Pipeline5 { get; }
        public InterceptDelegate Pipeline6 { get; }
        public InterceptDelegate Pipeline7 { get; }
        public InterceptDelegate Pipeline8 { get; }
        public InterceptDelegate Pipeline9 { get; }
        public InterceptDelegate Pipeline10 { get; }
        public InterceptDelegate Pipeline11 { get; }
        public InterceptDelegate Pipeline12 { get; }
        public InterceptDelegate Pipeline13 { get; }
        public InterceptDelegate Pipeline14 { get; }
        public InterceptDelegate Pipeline15 { get; }
        public InterceptDelegate Pipeline16 { get; }
        public InterceptDelegate Pipeline17 { get; }

        public FakeServiceProxy_Pipelines(Aspect1Attribute aspect1, Aspect2Attribute aspect2)
        {
            var aspects = new AbstractInterceptorAttribute[] { aspect1, aspect2 };
            Pipeline1 = ProxyFactoryUtils.CreatePipeline(_aspectDelegate1, aspects);
            aspects = new AbstractInterceptorAttribute[] { aspect1, aspect2 };
            Pipeline2 = ProxyFactoryUtils.CreatePipeline(_aspectDelegate2, aspects);
            aspects = new AbstractInterceptorAttribute[] { aspect1, aspect2 };
            Pipeline3 = ProxyFactoryUtils.CreatePipeline(_aspectDelegate3, aspects);
            aspects = new AbstractInterceptorAttribute[] { aspect1, aspect2 };
            Pipeline4 = ProxyFactoryUtils.CreatePipeline(_aspectDelegate4, aspects);
            aspects = new AbstractInterceptorAttribute[] { aspect1, aspect2 };
            Pipeline5 = ProxyFactoryUtils.CreatePipeline(_aspectDelegate5, aspects);
            aspects = new AbstractInterceptorAttribute[] { aspect1, aspect2 };
            Pipeline6 = ProxyFactoryUtils.CreatePipeline(_aspectDelegate6, aspects);
            aspects = new AbstractInterceptorAttribute[] { aspect1, aspect2 };
            Pipeline7 = ProxyFactoryUtils.CreatePipeline(_aspectDelegate7, aspects);
            aspects = new AbstractInterceptorAttribute[] { aspect1, aspect2 };
            Pipeline8 = ProxyFactoryUtils.CreatePipeline(_aspectDelegate8, aspects);
            aspects = new AbstractInterceptorAttribute[] { aspect1, aspect2 };
            Pipeline9 = ProxyFactoryUtils.CreatePipeline(_aspectDelegate9, aspects);
            aspects = new AbstractInterceptorAttribute[] { aspect1, aspect2 };
            Pipeline10 = ProxyFactoryUtils.CreatePipeline(_aspectDelegate10, aspects);
            aspects = new AbstractInterceptorAttribute[] { aspect1, aspect2 };
            Pipeline11 = ProxyFactoryUtils.CreatePipeline(_aspectDelegate11, aspects);
            aspects = new AbstractInterceptorAttribute[] { aspect1, aspect2 };
            Pipeline12 = ProxyFactoryUtils.CreatePipeline(_aspectDelegate12, aspects);
            aspects = new AbstractInterceptorAttribute[] { aspect1, aspect2 };
            Pipeline13 = ProxyFactoryUtils.CreatePipeline(_aspectDelegate13, aspects);
            aspects = new AbstractInterceptorAttribute[] { aspect1, aspect2 };
            Pipeline14 = ProxyFactoryUtils.CreatePipeline(_aspectDelegate14, aspects);
            aspects = new AbstractInterceptorAttribute[] { aspect1, aspect2 };
            Pipeline15 = ProxyFactoryUtils.CreatePipeline(_aspectDelegate15, aspects);
            aspects = new AbstractInterceptorAttribute[] { aspect1, aspect2 };
            Pipeline16 = ProxyFactoryUtils.CreatePipeline(_aspectDelegate16, aspects);
            aspects = new AbstractInterceptorAttribute[] { aspect1, aspect2 };
            Pipeline17 = ProxyFactoryUtils.CreatePipeline(_aspectDelegate17, aspects);
        }

        private static readonly AspectDelegate _aspectDelegate1;
        private static readonly AspectDelegate _aspectDelegate2;
        private static readonly AspectDelegate _aspectDelegate3;
        private static readonly AspectDelegate _aspectDelegate4;
        private static readonly AspectDelegate _aspectDelegate5;
        private static readonly AspectDelegate _aspectDelegate6;
        private static readonly AspectDelegate _aspectDelegate7;
        private static readonly AspectDelegate _aspectDelegate8;
        private static readonly AspectDelegate _aspectDelegate9;
        private static readonly AspectDelegate _aspectDelegate10;
        private static readonly AspectDelegate _aspectDelegate11;
        private static readonly AspectDelegate _aspectDelegate12;
        private static readonly AspectDelegate _aspectDelegate13;
        private static readonly AspectDelegate _aspectDelegate14;
        private static readonly AspectDelegate _aspectDelegate15;
        private static readonly AspectDelegate _aspectDelegate16;
        private static readonly AspectDelegate _aspectDelegate17;

        static FakeServiceProxy_Pipelines()
        {
            _aspectDelegate1 = AspectDelegate1;
            _aspectDelegate2 = AspectDelegate2;
            _aspectDelegate3 = AspectDelegate3;
            _aspectDelegate4 = AspectDelegate4;
            _aspectDelegate5 = AspectDelegate5;
            _aspectDelegate6 = AspectDelegate6;
            _aspectDelegate7 = AspectDelegate7;
            _aspectDelegate8 = AspectDelegate8;
            _aspectDelegate9 = AspectDelegate9;
            _aspectDelegate10 = AspectDelegate10;
            _aspectDelegate11 = AspectDelegate11;
            _aspectDelegate12 = AspectDelegate12;
            _aspectDelegate13 = AspectDelegate13;
            _aspectDelegate14 = AspectDelegate14;
            _aspectDelegate15 = AspectDelegate15;
            _aspectDelegate16 = AspectDelegate16;
            _aspectDelegate17 = AspectDelegate17;
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
    }
}
