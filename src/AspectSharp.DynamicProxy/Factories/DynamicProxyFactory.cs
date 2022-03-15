using AspectSharp.Abstractions;
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
        private static readonly ConstructorInfo _objectConstructorMethodInfo = typeof(object).GetConstructor(Array.Empty<Type>());

        internal static readonly AssemblyBuilder _proxiedClassesAssemblyBuilder = NewAssemblyBuilder();
        internal static readonly ModuleBuilder _proxiedClassesModuleBuilder = NewModuleBuilder(_proxiedClassesAssemblyBuilder);

        private static readonly IDictionary<(Type Service, Type Target), (Type Proxy, Type Pipeline)> _cachedProxyTypes
            = new Dictionary<(Type Service, Type Target), (Type, Type)>();

        public static (Type Proxy, Type Pipelines) Create(Type serviceType, Type targetType, InterceptedTypeData interceptedTypeData, DynamicProxyFactoryConfigurations configs)
        {
            configs = configs ?? Configurations;
            if (_cachedProxyTypes.TryGetValue((serviceType, targetType), out var type))
                return type;

            var previouslyDefinedProxyClassFromThisTargetCount = _cachedProxyTypes.Count(kvp => kvp.Key.Target == targetType);
            TypeBuilder typeBuilder;
            if (previouslyDefinedProxyClassFromThisTargetCount == 0)
                typeBuilder = _proxiedClassesModuleBuilder.DefineType(string.Format("{0}Proxy", targetType.Name), TypeAttributes.Public | TypeAttributes.Sealed);
            else
                typeBuilder = _proxiedClassesModuleBuilder.DefineType(string.Format("{0}Proxy_{1}", targetType.Name, previouslyDefinedProxyClassFromThisTargetCount), TypeAttributes.Public | TypeAttributes.Sealed);

            var pipelineDefinitionsTypeBuilder = _proxiedClassesModuleBuilder.DefineType(string.Format("{0}_Pipelines", typeBuilder.Name), TypeAttributes.Public | TypeAttributes.Sealed);
            var pipelineProperties = PipelineClassFactory.CreatePipelineClass(serviceType, pipelineDefinitionsTypeBuilder, interceptedTypeData, configs);

            var concretePipelineType = pipelineDefinitionsTypeBuilder.CreateType();

            var (pipelineField, contextActivatorFields) = AspectActivatorFieldFactory.CreateStaticFields(typeBuilder, serviceType, targetType, concretePipelineType, interceptedTypeData);

            var readonlyFields = DefineReadonlyFields(serviceType, pipelineField, typeBuilder).ToArray();

            DefineConstructor(targetType, typeBuilder, readonlyFields);


            var methods = MethodFactory.CreateMethods(serviceType, typeBuilder, readonlyFields, pipelineProperties, contextActivatorFields).ToList();

            PropertyFactory.CreateProperties(serviceType, typeBuilder, methods, readonlyFields[0]);

            EventFactory.CreateEvents(serviceType, typeBuilder, methods, readonlyFields[0]);

            typeBuilder.AddInterfaceImplementation(serviceType);

            var concreteType = typeBuilder.CreateType();
            _cachedProxyTypes.Add((serviceType, targetType), (concreteType, concretePipelineType));
            return (concreteType, concretePipelineType);
        }

        private static IEnumerable<FieldBuilder> DefineReadonlyFields(Type serviceType, FieldBuilder pipelineField, TypeBuilder typeBuilder)
        {
            yield return typeBuilder.DefineField("_target", serviceType, FieldAttributes.Private | FieldAttributes.InitOnly);
            yield return pipelineField;
            yield return typeBuilder.DefineField("_contextFactory", typeof(IAspectContextFactory), FieldAttributes.Private | FieldAttributes.InitOnly);
        }

        private static void DefineConstructor(Type targetType, TypeBuilder typeBuilder, FieldBuilder[] readonlyFields)
        {
            var targetField = readonlyFields[0];
            var pipelinesField = readonlyFields[1];
            var contextFactoryField = readonlyFields[2];

            var attrs = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
            var coonstructorParameters = new Type[] { targetType, contextFactoryField.FieldType };
            var constructorBuilder = typeBuilder.DefineConstructor(attrs, CallingConventions.Standard | CallingConventions.HasThis, coonstructorParameters);

            var parameters = new List<ParameterBuilder>();
            foreach (var (readonlyFieldBuilder, idx) in readonlyFields.Where(rf => rf != pipelinesField).Zip(Enumerable.Range(1, readonlyFields.Where(rf => rf != pipelinesField).Count()), (first, second) => new Tuple<FieldBuilder, int>(first, second)))
            {
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
            var assemblyName = new AssemblyName(typeof(DynamicProxyFactory).Assembly.GetName().Name + ".Proxies");
            return AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        }

        private static ModuleBuilder NewModuleBuilder(AssemblyBuilder assemblyBuilder)
        {
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("Impl");

            return moduleBuilder;
        }
    }
}
