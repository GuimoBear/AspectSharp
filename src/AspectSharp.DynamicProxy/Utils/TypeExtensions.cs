using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace AspectSharp.DynamicProxy.Utils
{
    internal static class TypeExtensions
    {
        private static readonly IReadOnlyDictionary<Type, OpCode> _numericTypesLoadOpCode = new Dictionary<Type, OpCode>
        {
#if NET5_0_OR_GREATER
            { typeof(nint), OpCodes.Ldind_I },
            { typeof(nuint), OpCodes.Ldind_I },
#endif
            { typeof(sbyte), OpCodes.Ldind_I1 },
            { typeof(byte), OpCodes.Ldind_U1 },
            { typeof(short), OpCodes.Ldind_I2 },
            { typeof(ushort), OpCodes.Ldind_U2 },
            { typeof(int), OpCodes.Ldind_I4 },
            { typeof(uint), OpCodes.Ldind_U4 },
            { typeof(long), OpCodes.Ldind_I8 },
            { typeof(ulong), OpCodes.Ldind_I8 },
            { typeof(float), OpCodes.Ldind_R4 },
            { typeof(double), OpCodes.Ldind_R8 }
        }.ToImmutableDictionary();

        private static readonly IReadOnlyDictionary<Type, OpCode> _numericTypesSetOpCode = new Dictionary<Type, OpCode>
        {
#if NET5_0_OR_GREATER
            { typeof(nint), OpCodes.Stind_I },
            { typeof(nuint), OpCodes.Stind_I },
#endif
            { typeof(sbyte), OpCodes.Stind_I1 },
            { typeof(byte), OpCodes.Stind_I1 },
            { typeof(short), OpCodes.Stind_I2 },
            { typeof(ushort), OpCodes.Stind_I2 },
            { typeof(int), OpCodes.Stind_I4 },
            { typeof(uint), OpCodes.Stind_I4 },
            { typeof(long), OpCodes.Stind_I8 },
            { typeof(ulong), OpCodes.Stind_I8 },
            { typeof(float), OpCodes.Stind_R4 },
            { typeof(double), OpCodes.Stind_R8 }
        }.ToImmutableDictionary();

        public static void EmitOptionalLoadOpCode(this Type parameterType, ILGenerator cil)
        {
            if (_numericTypesLoadOpCode.TryGetValue(parameterType, out var opCode))
                cil.Emit(opCode);
            else if (parameterType.IsValueType || parameterType.ContainsGenericParameters)
                cil.Emit(OpCodes.Ldobj, parameterType);
            else
                cil.Emit(OpCodes.Ldind_Ref);
        }

        public static void EmitOptionalSetOpCode(this Type parameterType, ILGenerator cil)
        {
            if (_numericTypesSetOpCode.TryGetValue(parameterType, out var opCode))
                cil.Emit(opCode);
            else if (parameterType.IsValueType)
                cil.Emit(OpCodes.Stobj, parameterType);
            else
                cil.Emit(OpCodes.Stind_Ref);
        }

        public static MethodInfo? GetMethod(this Type type, MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters();
            var parameterTypes = new Type[parameters.Length];
            var parameterNames = new string[parameters.Length];
            var parametersIsRefOrOut = new bool[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                parametersIsRefOrOut[i] = parameter.ParameterType.IsByRef && parameter.ParameterType.IsAutoLayout && parameter.ParameterType.Name.EndsWith("&");
                parameterNames[i] = parameter.Name;
#if NET7_0_OR_GREATER
                parameterTypes[i] = parameter.ParameterType;
#else
                parameterTypes[i] = parametersIsRefOrOut[i] ? parameter.ParameterType.GetElementType() : parameter.ParameterType;
#endif
            }
            return type.GetMethodExactly(methodInfo.Name, parameterTypes, parameterNames, parametersIsRefOrOut);
        }

        public static MethodInfo? GetMethodExactly(this Type type, string name, Type[] parameterTypes, string[] parameterNames, bool[] parametersIsRefOrOut)
        {
            var parameters = Array.Empty<Tuple<Type, string, bool>>();
            if (parameterTypes != null && parameterTypes.Length > 0)
                parameters = parameterTypes.Select((type, idx) => new Tuple<Type, string, bool>(type, parameterNames[idx], parametersIsRefOrOut[idx])).ToArray();
            var method = type.GetMethodsRecursively()
                .FirstOrDefault(mi => mi.Name == name &&
                             mi.GetParameters().Length == parameters.Length &&
                             mi.GetParameters()
                               .Select((pi, idx) => new Tuple<ParameterInfo, int>(pi, idx))
                               .All(tuple =>
                               {
                                   var parameterIsOutOrRef = tuple.Item1.ParameterType.IsByRef && tuple.Item1.ParameterType.IsAutoLayout && tuple.Item1.ParameterType.Name.EndsWith("&");
#if NET7_0_OR_GREATER
                                   var parameterType = tuple.Item1.ParameterType;
#else
                                   var parameterType = parameterIsOutOrRef ? tuple.Item1.ParameterType.GetElementType() : tuple.Item1.ParameterType;
#endif
                                   var expectedParameter = parameters[tuple.Item2];
                                   return expectedParameter.Equals(new Tuple<Type, string, bool>(parameterType, tuple.Item1.Name, parameterIsOutOrRef));
                               }));
            return method;
        }

        public static MethodInfo? GetMethodByStringRepresentation(this Type type, string methodStringRepresentation)
        {
            var method = type.GetMethodsRecursively()
                .FirstOrDefault(mi => MethodInfoUtils.StringRepresentation(mi).Equals(methodStringRepresentation));
            return method;
        }

        public static IEnumerable<MethodInfo> GetMethodsRecursively(this Type type, BindingFlags? bindingFlags = null)
        => GetMethodsRecursively(type, new HashSet<int>(), bindingFlags);
        public static IEnumerable<PropertyInfo> GetPropertiesRecursively(this Type type, BindingFlags? bindingFlags = null)
        => GetPropertiesRecursively(type, new HashSet<int>(), bindingFlags);
        public static IEnumerable<EventInfo> GetEventsRecursively(this Type type, BindingFlags? bindingFlags = null)
        => GetEventsRecursively(type, new HashSet<int>(), bindingFlags);

        private static IEnumerable<MethodInfo> GetMethodsRecursively(this Type type, HashSet<int> hashes, BindingFlags? bindingFlags = null)
        {
            var methods = bindingFlags is null ? type.GetMethods() : type.GetMethods(bindingFlags.Value);
            foreach (var method in methods)
            {
                if (hashes.Add(method.GetHashCode()))
                {
                    yield return method;
                }
            }
            var nestedTypes = bindingFlags is null ? type.GetNestedTypes() : type.GetNestedTypes(bindingFlags.Value);
            foreach (var nestedType in nestedTypes)
            {
                foreach (var method in GetMethodsRecursively(nestedType, hashes, bindingFlags))
                {
                    yield return method;
                }
            }
            var interfaces = type.GetInterfaces();
            foreach (var @interface in interfaces)
            {
                foreach (var method in GetMethodsRecursively(@interface, hashes, bindingFlags))
                {
                    yield return method;
                }
            }
        }

        private static IEnumerable<PropertyInfo> GetPropertiesRecursively(this Type type, HashSet<int> hashes, BindingFlags? bindingFlags = null)
        {
            var properties = bindingFlags is null ? type.GetProperties() : type.GetProperties(bindingFlags.Value);
            foreach (var property in properties)
            {
                if (hashes.Add(property.GetHashCode()))
                {
                    yield return property;
                }
            }
            var nestedTypes = bindingFlags is null ? type.GetNestedTypes() : type.GetNestedTypes(bindingFlags.Value);
            foreach (var nestedType in nestedTypes)
            {
                foreach (var property in GetPropertiesRecursively(nestedType, hashes, bindingFlags))
                {
                    yield return property;
                }
            }
            var interfaces = type.GetInterfaces();
            foreach (var @interface in interfaces)
            {
                foreach (var property in GetPropertiesRecursively(@interface, hashes, bindingFlags))
                {
                    yield return property;
                }
            }
        }

        private static IEnumerable<EventInfo> GetEventsRecursively(this Type type, HashSet<int> hashes, BindingFlags? bindingFlags = null)
        {
            var events = bindingFlags is null ? type.GetEvents() : type.GetEvents(bindingFlags.Value);
            foreach (var @event in events)
            {
                if (hashes.Add(@event.GetHashCode()))
                {
                    yield return @event;
                }
            }
            var nestedTypes = bindingFlags is null ? type.GetNestedTypes() : type.GetNestedTypes(bindingFlags.Value);
            foreach (var nestedType in nestedTypes)
            {
                foreach (var @event in GetEventsRecursively(nestedType, hashes, bindingFlags))
                {
                    yield return @event;
                }
            }
            var interfaces = type.GetInterfaces();
            foreach (var @interface in interfaces)
            {
                foreach (var @event in GetEventsRecursively(@interface, hashes, bindingFlags))
                {
                    yield return @event;
                }
            }
        }
    }
}
