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

        private static IEnumerable<Tuple<MethodInfo, FieldBuilder, FieldBuilder, FieldBuilder>> DefineStaticConstructor(TypeBuilder typeBuilder, Type serviceType, Type targetType, InterceptedTypeData interceptedTypeData, IEnumerable<MethodBuilder> fieldBuilders)
        {
            var staticConstructor = typeBuilder.DefineConstructor(MethodAttributes.Static, CallingConventions.Standard, null);
            var staticCil = staticConstructor.GetILGenerator();

            var getTypeFromHandlerMethodInfo = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), BindingFlags.Public | BindingFlags.Static);
            var getMethodMethodInfo = typeof(Type).GetMethod(nameof(Type.GetMethod), new Type[] { typeof(string), typeof(Type[]) });
            var addParamListMethodInfo = typeof(List<string>).GetMethod("Add", new Type[1] { typeof(string) });
            var clearParamListMethodInfo = typeof(List<string>).GetMethod("Clear", new Type[0]);

            Func<MethodInfo, int> orderMethod = mi =>
            {
                var hashCode = mi.Name.GetHashCode();
                foreach (var parameter in mi.GetParameters())
                    hashCode ^= parameter.Name.GetHashCode() ^
                                parameter.ParameterType.GetHashCode();
                return hashCode;
            };

            var builders = fieldBuilders.OrderBy(kvp => orderMethod(kvp)).ToList();
            var serviceMethodInfos = serviceType.GetMethods().OrderBy(orderMethod).Where(mi => mi.IsPublic && builders.Any(mi2 => mi2.Name == mi.Name && mi2.GetParameters().All(pi => mi.GetParameters().Any(pi2 => pi.Name == pi2.Name && pi.ParameterType == pi2.ParameterType)))).ToArray();
            var targetMethodInfos = targetType.GetMethods().OrderBy(orderMethod).Where(mi => mi.IsPublic && serviceMethodInfos.Any(mi2 => mi2.Name == mi.Name && mi2.GetParameters().All(pi => mi.GetParameters().Any(pi2 => pi.Name == pi2.Name && pi.ParameterType == pi2.ParameterType)))).ToArray();

            staticCil.Emit(OpCodes.Newobj, typeof(List<Type>).GetConstructor(new Type[0]));
            staticCil.Emit(OpCodes.Stloc_0);



            foreach (var tuple in builders
                        .Zip(serviceMethodInfos, (first, second) => new Tuple<MethodInfo, MethodInfo>(first, second))
                        .Zip(targetMethodInfos, (first, second) => new Tuple<MethodInfo, MethodInfo, MethodInfo>(first.Item1, first.Item2, second)))
            {
                var proxyMi = tuple.Item1;
                var serviceMi = tuple.Item2;
                var targetMi = tuple.Item3;

                var serviceMethodInfoFieldBuilder = typeBuilder.DefineField(string.Format("_{0}ServiceMethodInfo", serviceMi.Name.Substring(0, 1).ToLower() + serviceMi.Name.Substring(1)), typeof(MethodInfo), FieldAttributes.Private | FieldAttributes.Static | FieldAttributes.InitOnly);
                var targetMethodInfoFieldBuilder = typeBuilder.DefineField(string.Format("_{0}TargetMethodInfo", targetMi.Name.Substring(0, 1).ToLower() + targetMi.Name.Substring(1)), typeof(MethodInfo), FieldAttributes.Private | FieldAttributes.Static | FieldAttributes.InitOnly);
                var proxyMethodInfoFieldBuilder = typeBuilder.DefineField(string.Format("_{0}ProxyMethodInfo", proxyMi.Name.Substring(0, 1).ToLower() + proxyMi.Name.Substring(1)), typeof(MethodInfo), FieldAttributes.Private | FieldAttributes.Static | FieldAttributes.InitOnly);

                foreach (var pi in proxyMi.GetParameters())
                {
                    staticCil.Emit(OpCodes.Ldloc_0);
                    staticCil.Emit(OpCodes.Ldtoken, pi.ParameterType);
                    staticCil.Emit(OpCodes.Call, getTypeFromHandlerMethodInfo);
                    staticCil.Emit(OpCodes.Callvirt, addParamListMethodInfo);
                }

                staticCil.Emit(OpCodes.Ldtoken, serviceType);
                staticCil.Emit(OpCodes.Call, getTypeFromHandlerMethodInfo);
                staticCil.Emit(OpCodes.Ldstr, serviceMi.Name);
                staticCil.Emit(OpCodes.Ldloc_0);
                staticCil.Emit(OpCodes.Callvirt, addParamListMethodInfo);
                staticCil.Emit(OpCodes.Callvirt, getMethodMethodInfo);
                staticCil.Emit(OpCodes.Stsfld, serviceMethodInfoFieldBuilder);

                staticCil.Emit(OpCodes.Ldtoken, targetType);
                staticCil.Emit(OpCodes.Call, getTypeFromHandlerMethodInfo);
                staticCil.Emit(OpCodes.Ldstr, targetMi.Name);
                staticCil.Emit(OpCodes.Ldloc_0);
                staticCil.Emit(OpCodes.Callvirt, addParamListMethodInfo);
                staticCil.Emit(OpCodes.Callvirt, getMethodMethodInfo);
                staticCil.Emit(OpCodes.Stsfld, targetMethodInfoFieldBuilder);

                staticCil.Emit(OpCodes.Ldtoken, typeBuilder);
                staticCil.Emit(OpCodes.Call, getTypeFromHandlerMethodInfo);
                staticCil.Emit(OpCodes.Ldstr, serviceMi.Name);
                staticCil.Emit(OpCodes.Ldloc_0);
                staticCil.Emit(OpCodes.Callvirt, addParamListMethodInfo);
                staticCil.Emit(OpCodes.Callvirt, getMethodMethodInfo);
                staticCil.Emit(OpCodes.Stsfld, targetMethodInfoFieldBuilder);

                staticCil.Emit(OpCodes.Ldloc_0);
                staticCil.Emit(OpCodes.Callvirt, clearParamListMethodInfo);

                yield return new Tuple<MethodInfo, FieldBuilder, FieldBuilder, FieldBuilder>(proxyMi, serviceMethodInfoFieldBuilder, targetMethodInfoFieldBuilder, proxyMethodInfoFieldBuilder);
            }
            staticCil.Emit(OpCodes.Ret);
        }
    }
}
