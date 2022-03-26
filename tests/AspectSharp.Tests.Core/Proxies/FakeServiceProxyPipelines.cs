using AspectSharp.Abstractions;
using AspectSharp.Abstractions.Attributes;
using AspectSharp.DynamicProxy;
using AspectSharp.DynamicProxy.Utils;
using AspectSharp.Tests.Core.Services.Interfaces;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

        [AsyncStateMachine(typeof(AspectDelegate3AsyncStateMachine))]
        private static Task AspectDelegate3(AspectContext aspectContext)
        {
            var stateMachine = default(AspectDelegate3AsyncStateMachine);
            stateMachine.builder = AsyncTaskMethodBuilder.Create();
            stateMachine.context = aspectContext;
            stateMachine.state = -1;
            stateMachine.builder.Start(ref stateMachine);
            return stateMachine.builder.Task;
        }

#if NETCOREAPP3_1_OR_GREATER
        private static Task AspectDelegate4(AspectContext aspectContext)
        {
            IFakeService target;
            target = aspectContext.Target as IFakeService;
            return target.InterceptedDoSomethingValueAsyncWithoutParameterAndWithoutReturn().AsTask();
        }

        private static async Task AspectDelegate5(AspectContext aspectContext)
        {
            IFakeService target;
            int param1;
            string param2;
            IEnumerable<string> param3;
            target = aspectContext.Target as IFakeService;
            param1 = (int)aspectContext.Parameters[0];
            param2 = (string)aspectContext.Parameters[1];
            param3 = (IEnumerable<string>)aspectContext.Parameters[2];
            await target.InterceptedDoSomethingValueAsyncWithParameterAndWithoutReturn(param1, param2, param3);
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

        private static async Task AspectDelegate10(AspectContext aspectContext)
        {
            IFakeService target;
            target = aspectContext.Target as IFakeService;
            aspectContext.ReturnValue = await target.InterceptedDoSomethingAsyncWithoutParameterAndValueTypeReturn();
            //return Task.CompletedTask;
        }

        /*
        private static FieldBuilder DefineStateField(TypeBuilder typeBuilder)
            => typeBuilder.DefineField("<>__state", _stateType, FieldAttributes.Public);

        private static FieldBuilder DefineMethodBuilderField(TypeBuilder typeBuilder)
            => typeBuilder.DefineField("<>___builder", _taskMethodBuilderType, FieldAttributes.Public);

        private static FieldBuilder DefineContextField(TypeBuilder typeBuilder)
            => typeBuilder.DefineField("<>___context", _contextType, FieldAttributes.Public);
         */

        [AsyncStateMachine(typeof(AspectDelegate11AsyncStateMachine))]
        private static Task AspectDelegate11(AspectContext aspectContext)
        {
            /*
            var type = DynamicProxyFactory
                ._proxiedClassesAssemblyBuilder
                .GetTypes()
                .FirstOrDefault(t => t.Name == "FakeServiceProxy_AspectDelegate11AsyncStateMachine");
            var inst = Activator.CreateInstance(type) as IAsyncStateMachine;

            var stateField = type.GetField("<>__state");
            var builderField = type.GetField("<>___builder");
            var contextField = type.GetField("<>___context");
            FieldInfo loggerField = default;
            try
            {
                loggerField = type.GetField("<>___logger");
            }
            catch
            {

            }

            var logger = new StringBuilder();

            stateField.SetValue(inst, -1);
            contextField.SetValue(inst, aspectContext);
            builderField.SetValue(inst, AsyncTaskMethodBuilder.Create());
            if (!(loggerField is null))
                loggerField.SetValue(inst, logger);

            
            var moveNextMethod = typeof(IAsyncStateMachine).GetMethod(nameof(IAsyncStateMachine.MoveNext));

            try
            {
                moveNextMethod.Invoke(inst, new object[] { });
            }
            catch
            {

            }

            var prepareAwaiterMethod = type.GetMethod("PrepareAwaiter", BindingFlags.NonPublic | BindingFlags.Instance);

            var awaiter = (TaskAwaiter)prepareAwaiterMethod.Invoke(inst, new object[] { });

            var builder = (AsyncTaskMethodBuilder)builderField.GetValue(inst);
            builder.Start(ref inst);
            */
            var stateMachine = new AspectDelegate11AsyncStateMachine
            {
                builder = AsyncTaskMethodBuilder.Create(),
                context = aspectContext,
                state = -1
            };
            stateMachine.builder.Start(ref stateMachine);
            return stateMachine.builder.Task;
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

        private static async Task AspectDelegate15(AspectContext aspectContext)
        {
            IFakeService target;
            int param1;
            string param2;
            IEnumerable<string> param3;
            target = aspectContext.Target as IFakeService;
            param1 = (int)aspectContext.Parameters[0];
            param2 = (string)aspectContext.Parameters[1];
            param3 = (IEnumerable<string>)aspectContext.Parameters[2];
            aspectContext.ReturnValue = await target.InterceptedDoSomethingAsyncWithParameterAndReferenceTypeReturn(param1, param2, param3);
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

        private static async Task AspectDelegate18(AspectContext aspectContext)
        {
            var target = aspectContext.Target as IFakeService;
            await target.InterceptedDoSomethingAsyncWithoutParameterAndWithoutReturn();
        }
    }
}
