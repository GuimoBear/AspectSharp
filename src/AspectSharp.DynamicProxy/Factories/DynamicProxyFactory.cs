using AspectSharp.Abstractions;
using AspectSharp.DynamicProxy.Exceptions;
using AspectSharp.DynamicProxy.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static AspectSharp.DynamicProxy.Utils.InterceptorTypeCache;

namespace AspectSharp.DynamicProxy.Factories
{
    internal static class DynamicProxyFactory
    {
        private static readonly object _lock = new object();

        private static readonly ConstructorInfo _objectConstructorMethodInfo = typeof(object).GetConstructor(Array.Empty<Type>());

        internal static readonly AssemblyBuilder _proxiedClassesAssemblyBuilder = NewAssemblyBuilder();
        internal static readonly ModuleBuilder _proxiedClassesModuleBuilder = NewModuleBuilder(_proxiedClassesAssemblyBuilder);

        private static readonly IDictionary<Tuple<Type, Type, string, string, int>, Tuple<Type, Type>> _cachedProxyTypes
            = new Dictionary<Tuple<Type, Type, string, string, int>, Tuple<Type, Type>>();

        public static Type Create(Type serviceType, Type targetType, InterceptedTypeData interceptedTypeData, DynamicProxyFactoryConfigurations configs)
        {
            if (!serviceType.IsInterface)
                throw new NotInterfaceTypeException(serviceType);

            configs = configs ?? new DynamicProxyFactoryConfigurations(
                Configurations.IncludeAspectsToEvents,
                Configurations.IncludeAspectsToProperties,
                Configurations.ExcludeAspectsForMethods, 
                Configurations.GlobalInterceptors, 
                Configurations.IgnoreErrorsWhileTryingInjectAspects);
            if (_cachedProxyTypes.TryGetValue(new Tuple<Type, Type, string, string, int>(serviceType, targetType, serviceType.Name, targetType.Name, configs.GetHashCode()), out var type))
                return type.Item1;

            lock (_lock)
            {
                var previouslyDefinedProxyClassFromThisTargetCount = _cachedProxyTypes.Count(kvp => kvp.Key.Item3 == serviceType.Name && kvp.Key.Item4 == targetType.Name);
                TypeBuilder typeBuilder;
                if (previouslyDefinedProxyClassFromThisTargetCount == 0)
                    typeBuilder = _proxiedClassesModuleBuilder.DefineType(string.Format("AspectSharp.Proxies.{0}Proxy", targetType.Name), TypeAttributes.Public | TypeAttributes.Sealed);
                else
                    typeBuilder = _proxiedClassesModuleBuilder.DefineType(string.Format("AspectSharp.Proxies.{0}Proxy{1}", targetType.Name, previouslyDefinedProxyClassFromThisTargetCount), TypeAttributes.Public | TypeAttributes.Sealed);
                var (typeGenericParameters, typeGenericParameterBuilders) = GenericParameterUtils.DefineGenericParameter(targetType, typeBuilder);

                var pipelineDefinitionsTypeBuilder = _proxiedClassesModuleBuilder.DefineType(string.Format("AspectSharp.Pipelines.{0}", typeBuilder.Name), TypeAttributes.Public | TypeAttributes.Sealed);
                //GenericParameterUtils.DefineGenericParameter(targetType, pipelineDefinitionsTypeBuilder);

                var pipelineProperties = PipelineClassFactory.CreatePipelineClass(targetType, serviceType, pipelineDefinitionsTypeBuilder, interceptedTypeData, configs);

                var concretePipelineType = pipelineDefinitionsTypeBuilder.CreateTypeInfo().AsType();

                var contextActivatorFields = AspectActivatorFieldFactory.CreateStaticFields(typeBuilder, serviceType, targetType, interceptedTypeData);

                var readonlyFields = DefineReadonlyFields(targetType, typeBuilder, typeGenericParameterBuilders).ToArray();

                DefineConstructor(typeBuilder, readonlyFields);

                var methods = MethodFactory.CreateMethods(_proxiedClassesModuleBuilder, targetType, serviceType, typeBuilder, typeGenericParameters, typeGenericParameterBuilders, readonlyFields, pipelineProperties, contextActivatorFields).ToList();

                PropertyFactory.CreateProperties(serviceType, typeBuilder, methods, readonlyFields[0]);

                EventFactory.CreateEvents(serviceType, typeBuilder, methods, readonlyFields[0]);

                typeBuilder.AddInterfaceImplementation(serviceType);

                var concreteType = typeBuilder.CreateTypeInfo().AsType();
                _cachedProxyTypes.Add(new Tuple<Type, Type, string, string, int>(serviceType, targetType, serviceType.Name, targetType.Name, configs.GetHashCode()), new Tuple<Type, Type>(concreteType, concretePipelineType));
                return concreteType;
            }
        }

        private static IEnumerable<FieldBuilder> DefineReadonlyFields(Type targetType, TypeBuilder typeBuilder, GenericTypeParameterBuilder[] typeGenericParameters)
        {
            yield return typeBuilder.DefineField("_target", targetType.IsGenericTypeDefinition ? targetType.MakeGenericType(typeGenericParameters) : targetType, FieldAttributes.Private | FieldAttributes.InitOnly);
            yield return typeBuilder.DefineField("_contextFactory", typeof(IAspectContextFactory), FieldAttributes.Private | FieldAttributes.InitOnly);
        }

        private static void DefineConstructor(TypeBuilder typeBuilder, FieldBuilder[] readonlyFields)
        {
            var targetField = readonlyFields[0];
            var contextFactoryField = readonlyFields[1];

            var attrs = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
            var coonstructorParameters = new Type[] { targetField.FieldType, contextFactoryField.FieldType };
            var constructorBuilder = typeBuilder.DefineConstructor(attrs, CallingConventions.HasThis, coonstructorParameters);

            var parameters = new List<ParameterBuilder>();
            foreach (var tuple in readonlyFields.Zip(Enumerable.Range(1, readonlyFields.Length), (first, second) => new Tuple<FieldBuilder, int>(first, second)))
            {
                var readonlyFieldBuilder = tuple.Item1;
                var idx = tuple.Item2;
                parameters.Add(constructorBuilder.DefineParameter(idx, ParameterAttributes.None, readonlyFieldBuilder.Name.Substring(1)));
            }

            var cil = constructorBuilder.GetILGenerator();

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Call, _objectConstructorMethodInfo);
            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldarg_1);
            cil.Emit(OpCodes.Stfld, targetField);
            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldarg_2);
            cil.Emit(OpCodes.Stfld, contextFactoryField);
            cil.Emit(OpCodes.Ret);
        }

        private static AssemblyBuilder NewAssemblyBuilder()
        {
            
            var assemblyName = new AssemblyName("AspectSharp.Proxies");
#if NETCOREAPP3_1_OR_GREATER
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
#elif NET46 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48 || NET481
            var assemblyBuilder =  AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
#else
            var assemblyBuilder =  AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
#endif
            return assemblyBuilder;
        }

        private static ModuleBuilder NewModuleBuilder(AssemblyBuilder assemblyBuilder)
        {
#if NETCOREAPP3_1_OR_GREATER
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyBuilder.GetName().Name);
#elif NET461_OR_GREATER
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyBuilder.GetName().Name, assemblyBuilder.GetName().Name + ".mod");
#else
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyBuilder.GetName().Name);
#endif
            return moduleBuilder;
        }
    }
}
