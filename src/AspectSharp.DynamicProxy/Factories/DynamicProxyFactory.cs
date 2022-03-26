using AspectSharp.Abstractions;
using AspectSharp.DynamicProxy.Exceptions;
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

        private static readonly IDictionary<Tuple<Type, Type, int>, Tuple<Type , Type>> _cachedProxyTypes
            = new Dictionary<Tuple<Type, Type, int>, Tuple<Type, Type>>();

        public static Type Create(Type serviceType, Type targetType, InterceptedTypeData interceptedTypeData, DynamicProxyFactoryConfigurations configs)
        {
            if (!serviceType.IsInterface)
                throw new NotInterfaceTypeException(serviceType);

            configs = configs ?? new DynamicProxyFactoryConfigurations(
                Configurations.IncludeTypeDefinitionAspectsToEvents,
                Configurations.IncludeTypeDefinitionAspectsToProperties,
                Configurations.ExcludeTypeDefinitionAspectsForMethods);
            if (_cachedProxyTypes.TryGetValue(new Tuple<Type, Type, int>(serviceType, targetType, configs.GetHashCode()), out var type))
                return type.Item1;

            lock (_lock)
            {
                var previouslyDefinedProxyClassFromThisTargetCount = _cachedProxyTypes.Count(kvp => kvp.Key.Item2 == targetType);
                TypeBuilder typeBuilder;
                if (previouslyDefinedProxyClassFromThisTargetCount == 0)
                    typeBuilder = _proxiedClassesModuleBuilder.DefineType(string.Format("AspectSharp.Proxies.{0}Proxy", targetType.Name), TypeAttributes.Public | TypeAttributes.Sealed);
                else
                    typeBuilder = _proxiedClassesModuleBuilder.DefineType(string.Format("AspectSharp.Proxies.{0}Proxy{1}", targetType.Name, previouslyDefinedProxyClassFromThisTargetCount), TypeAttributes.Public | TypeAttributes.Sealed);
                var pipelineDefinitionsTypeBuilder = _proxiedClassesModuleBuilder.DefineType(string.Format("AspectSharp.Pipelines.{0}", typeBuilder.Name), TypeAttributes.Public | TypeAttributes.Sealed);
                var pipelineProperties = PipelineClassFactory.CreatePipelineClass(serviceType, pipelineDefinitionsTypeBuilder, _proxiedClassesModuleBuilder, interceptedTypeData, configs);

                var concretePipelineType = pipelineDefinitionsTypeBuilder.CreateType();

                var contextActivatorFields = AspectActivatorFieldFactory.CreateStaticFields(typeBuilder, serviceType, targetType, interceptedTypeData);

                var readonlyFields = DefineReadonlyFields(serviceType, typeBuilder).ToArray();

                DefineConstructor(targetType, typeBuilder, readonlyFields);


                var methods = MethodFactory.CreateMethods(serviceType, typeBuilder, readonlyFields, pipelineProperties, contextActivatorFields).ToList();

                PropertyFactory.CreateProperties(serviceType, typeBuilder, methods, readonlyFields[0]);

                EventFactory.CreateEvents(serviceType, typeBuilder, methods, readonlyFields[0]);

                typeBuilder.AddInterfaceImplementation(serviceType);

                var concreteType = typeBuilder.CreateType();
                _cachedProxyTypes.Add(new Tuple<Type, Type, int>(serviceType, targetType, configs.GetHashCode()), new Tuple<Type, Type>(concreteType, concretePipelineType));
                return concreteType;
            }
        }

        private static IEnumerable<FieldBuilder> DefineReadonlyFields(Type serviceType, TypeBuilder typeBuilder)
        {
            yield return typeBuilder.DefineField("_target", serviceType, FieldAttributes.Private | FieldAttributes.InitOnly);
            yield return typeBuilder.DefineField("_contextFactory", typeof(IAspectContextFactory), FieldAttributes.Private | FieldAttributes.InitOnly);
        }

        private static void DefineConstructor(Type targetType, TypeBuilder typeBuilder, FieldBuilder[] readonlyFields)
        {
            var targetField = readonlyFields[0];
            var contextFactoryField = readonlyFields[1];

            var attrs = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
            var coonstructorParameters = new Type[] { targetType, contextFactoryField.FieldType };
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
            return AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        }

        private static ModuleBuilder NewModuleBuilder(AssemblyBuilder assemblyBuilder)
        {
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("Impl");
            return moduleBuilder;
        }
    }
}
