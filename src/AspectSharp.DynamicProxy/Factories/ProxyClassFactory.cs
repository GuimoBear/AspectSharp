﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static AspectSharp.DynamicProxy.Utils.InterceptorTypeCache;

namespace AspectSharp.DynamicProxy.Factories
{
    internal static class ProxyClassFactory
    {
        private static readonly ConstructorInfo _objectConstructorMethodInfo = typeof(object).GetConstructor(Array.Empty<Type>());

        internal static readonly AssemblyBuilder _proxiedClassesAssemblyBuilder = NewAssemblyBuilder();
        internal static readonly ModuleBuilder _proxiedClassesModuleBuilder = NewModuleBuilder(_proxiedClassesAssemblyBuilder);

        private static readonly IDictionary<(Type Service, Type Target), (Type Proxy, Type Pipeline)> _cachedProxyTypes
            = new Dictionary<(Type Service, Type Target), (Type, Type)>();

        public static (Type Proxy, Type Pipelines) Create(Type serviceType, Type targetType, InterceptedTypeData interceptedTypeData)
        {
            if (_cachedProxyTypes.TryGetValue((serviceType, targetType), out var type))
                return type;

            var previouslyDefinedProxyClassFromThisTargetCount = _cachedProxyTypes.Count(kvp => kvp.Key.Target == targetType);
            var typeBuilder = _proxiedClassesModuleBuilder.DefineType(string.Format("{0}Proxy_{1}", targetType.Name, ++previouslyDefinedProxyClassFromThisTargetCount), TypeAttributes.Public | TypeAttributes.Sealed);

            var pipelineDefinitionsTypeBuilder = _proxiedClassesModuleBuilder.DefineType(string.Format("{0}_Pipelines", typeBuilder.Name), TypeAttributes.Public | TypeAttributes.Sealed);
            var pipelineProperties = PipelineClassFactory.CreatePipelineClass(serviceType, pipelineDefinitionsTypeBuilder, interceptedTypeData);


            var readonlyFields = DefineReadonlyFields(serviceType, pipelineDefinitionsTypeBuilder, typeBuilder).ToArray();

            DefineConstructor(targetType, typeBuilder, readonlyFields);

            PropertyFactory.CreateProperties(serviceType, typeBuilder, readonlyFields[0]);

            EventFactory.CreateEvents(serviceType, typeBuilder, readonlyFields[0]);

            var contextActivatorFields = AspectActivatorFieldFactory.CreateStaticFields(typeBuilder, serviceType, targetType, interceptedTypeData);

            MethodFactory.CreateMethods(serviceType, typeBuilder, readonlyFields, pipelineProperties, contextActivatorFields);

            typeBuilder.AddInterfaceImplementation(serviceType);

            var concreteType = typeBuilder.CreateType();
            var concretePipelineType = pipelineDefinitionsTypeBuilder.CreateType();
            _cachedProxyTypes.Add((serviceType, targetType), (concreteType, concretePipelineType));
            return (concreteType, concretePipelineType);
        }

        private static IEnumerable<FieldBuilder> DefineReadonlyFields(Type serviceType, Type pipelineDefinitionType, TypeBuilder typeBuilder)
        {
            yield return typeBuilder.DefineField("_target", serviceType, FieldAttributes.Private | FieldAttributes.InitOnly);
            yield return typeBuilder.DefineField("_pipelines", pipelineDefinitionType, FieldAttributes.Private | FieldAttributes.InitOnly);
            yield return typeBuilder.DefineField("_contextFactory", typeof(IAspectContextFactory), FieldAttributes.Private | FieldAttributes.InitOnly);
        }

        private static void DefineConstructor(Type targetType, TypeBuilder typeBuilder, FieldBuilder[] readonlyFields)
        {
            var targetField = readonlyFields[0];
            var pipelinesField = readonlyFields[1];
            var contextFactoryField = readonlyFields[2];

            var attrs = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
            var coonstructorParameters = new Type[] { targetType, pipelinesField.FieldType, contextFactoryField.FieldType };
            var constructorBuilder = typeBuilder.DefineConstructor(attrs, CallingConventions.Standard | CallingConventions.HasThis, coonstructorParameters);

            var parameters = new List<ParameterBuilder>();
            foreach (var (readonlyFieldBuilder, idx) in readonlyFields.Zip(Enumerable.Range(1, readonlyFields.Count()), (first, second) => new Tuple<FieldBuilder, int>(first, second)))
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
            cil.Emit(OpCodes.Stfld, pipelinesField);
            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldarg_3);
            cil.Emit(OpCodes.Stfld, contextFactoryField);
            cil.Emit(OpCodes.Ret);
        }

        private static Tuple<OpCode, bool> GetLdargFromIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return new Tuple<OpCode, bool>(OpCodes.Ldarg_0, false);
                case 1:
                    return new Tuple<OpCode, bool>(OpCodes.Ldarg_1, false);
                case 2:
                    return new Tuple<OpCode, bool>(OpCodes.Ldarg_2, false);
                case 3:
                    return new Tuple<OpCode, bool>(OpCodes.Ldarg_3, false);
            }
            return new Tuple<OpCode, bool>(OpCodes.Ldarg_S, true);
        }

        private static AssemblyBuilder NewAssemblyBuilder()
        {
            var assemblyName = new AssemblyName(typeof(ProxyClassFactory).Assembly.GetName().Name + ".Proxies");
            return AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        }

        private static ModuleBuilder NewModuleBuilder(AssemblyBuilder assemblyBuilder)
        {
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("Impl");

            return moduleBuilder;
        }
    }
}
