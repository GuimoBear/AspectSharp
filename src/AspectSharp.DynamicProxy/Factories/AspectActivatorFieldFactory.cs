using AspectSharp.DynamicProxy.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static AspectSharp.DynamicProxy.Utils.InterceptorTypeCache;

namespace AspectSharp.DynamicProxy.Factories
{
    internal static class AspectActivatorFieldFactory
    {
        private static readonly MethodInfo _getTypeFromHandleMethodInfo = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), new Type[] { typeof(RuntimeTypeHandle) });
        private static readonly MethodInfo _newContextActivatorMethodInfo = typeof(ProxyFactoryUtils).GetMethod(nameof(ProxyFactoryUtils.NewContextActivator), new Type[] { typeof(Type), typeof(Type), typeof(Type), typeof(string), typeof(Type[]) });

        public static IReadOnlyDictionary<MethodInfo, FieldInfo> CreateStaticFields(TypeBuilder typeBuilder, Type serviceType, Type targetType, InterceptedTypeData interceptedTypeData)
        {
            var attrs = MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
            var staticConstructor = typeBuilder.DefineConstructor(attrs, CallingConventions.Standard, null);
            var cil = staticConstructor.GetILGenerator();

            cil.DeclareLocal(typeof(Type));
            cil.DeclareLocal(typeof(Type));
            cil.DeclareLocal(typeof(Type));
            cil.DeclareLocal(typeof(Type[]));

            cil.Emit(OpCodes.Ldtoken, serviceType);
            cil.Emit(OpCodes.Call, _getTypeFromHandleMethodInfo);
            cil.Emit(OpCodes.Stloc_0);

            cil.Emit(OpCodes.Ldtoken, typeBuilder.GetType());
            cil.Emit(OpCodes.Call, _getTypeFromHandleMethodInfo);
            cil.Emit(OpCodes.Stloc_1);

            cil.Emit(OpCodes.Ldtoken, targetType);
            cil.Emit(OpCodes.Call, _getTypeFromHandleMethodInfo);
            cil.Emit(OpCodes.Stloc_2);

            var methods = serviceType.GetMethods();
            var index = 1;

            var ret = new Dictionary<MethodInfo, FieldInfo>();
            foreach (var methodInfo in methods)
            {
                if (interceptedTypeData.TryGetMethodInterceptors(methodInfo, out _))
                {
                    var field = typeBuilder.DefineField(string.Format("_aspectContextAtivator{0}", index), typeof(AspectContextActivator), FieldAttributes.Private | FieldAttributes.Static | FieldAttributes.InitOnly);

                    var parameters = methodInfo.GetParameters();
                    var hasParameters = parameters.Length > 0;
                    if (hasParameters)
                    {
                        cil.Emit(OpCodes.Ldc_I4, parameters.Length);
                        cil.Emit(OpCodes.Newarr, typeof(Type));
                        foreach (var tuple in parameters.Zip(Enumerable.Range(0, parameters.Length), (first, second) => new Tuple<ParameterInfo, int>(first, second)))
                        {
                            var parameter = tuple.Item1;
                            var idx = tuple.Item2;
                            cil.Emit(OpCodes.Dup);
                            cil.Emit(OpCodes.Ldc_I4, idx);
                            cil.Emit(OpCodes.Ldtoken, parameter.ParameterType);
                            cil.Emit(OpCodes.Call, _getTypeFromHandleMethodInfo);
                            cil.Emit(OpCodes.Stelem_Ref);
                        }
                        cil.Emit(OpCodes.Stloc_3);
                    }

                    cil.Emit(OpCodes.Ldloc_0);
                    cil.Emit(OpCodes.Ldloc_1);
                    cil.Emit(OpCodes.Ldloc_2);
                    cil.Emit(OpCodes.Ldstr, methodInfo.Name);
                    if (hasParameters)
                        cil.Emit(OpCodes.Ldloc_3);
                    else
                        cil.Emit(OpCodes.Ldnull);
                    cil.Emit(OpCodes.Call, _newContextActivatorMethodInfo);
                    cil.Emit(OpCodes.Stsfld, field);

                    ret.Add(methodInfo, field);

                    index++;
                }
            }
            cil.Emit(OpCodes.Ret);
            return ret;
        }
    }
}
