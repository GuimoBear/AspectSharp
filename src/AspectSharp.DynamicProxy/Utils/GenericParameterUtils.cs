using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectSharp.DynamicProxy.Utils
{
    internal static class GenericParameterUtils
    {
        internal static void DefineGenericParameter(Type targetType, TypeBuilder typeBuilder)
        {
            if (!targetType.GetTypeInfo().IsGenericTypeDefinition)
            {
                return;
            }
            var genericArguments = targetType.GetTypeInfo().GetGenericArguments().Select(t => t.GetTypeInfo()).ToArray();
            var genericArgumentsBuilders = typeBuilder.DefineGenericParameters(genericArguments.Select(a => a.Name).ToArray());
            for (var index = 0; index < genericArguments.Length; index++)
            {
                genericArgumentsBuilders[index].SetGenericParameterAttributes(ToClassGenericParameterAttributes(genericArguments[index].GenericParameterAttributes));
                foreach (var constraint in genericArguments[index].GetGenericParameterConstraints().Select(t => t.GetTypeInfo()))
                {
                    if (constraint.IsClass) genericArgumentsBuilders[index].SetBaseTypeConstraint(constraint.AsType());
                    if (constraint.IsInterface) genericArgumentsBuilders[index].SetInterfaceConstraints(constraint.AsType());
                }
            }
        }

        internal static GenericTypeParameterBuilder[] DefineGenericParameter(MethodInfo tergetMethod, MethodBuilder methodBuilder)
        {
            if (!tergetMethod.IsGenericMethod)
            {
                return Array.Empty<GenericTypeParameterBuilder>();
            }
            var genericArguments = tergetMethod.GetGenericArguments().Select(t => t.GetTypeInfo()).ToArray();
            var genericArgumentsBuilders = methodBuilder.DefineGenericParameters(genericArguments.Select(a => a.Name).ToArray());
            for (var index = 0; index < genericArguments.Length; index++)
            {
                genericArgumentsBuilders[index].SetGenericParameterAttributes(genericArguments[index].GenericParameterAttributes);
                foreach (var constraint in genericArguments[index].GetGenericParameterConstraints().Select(t => t.GetTypeInfo()))
                {
                    if (constraint.IsClass) genericArgumentsBuilders[index].SetBaseTypeConstraint(constraint.AsType());
                    if (constraint.IsInterface) genericArgumentsBuilders[index].SetInterfaceConstraints(constraint.AsType());
                }
            }
            return genericArgumentsBuilders;
        }

        internal static GenericParameterAttributes ToClassGenericParameterAttributes(GenericParameterAttributes attributes)
        {
            if (attributes == GenericParameterAttributes.None)
            {
                return GenericParameterAttributes.None;
            }
            if (attributes.HasFlag(GenericParameterAttributes.SpecialConstraintMask))
            {
                return GenericParameterAttributes.SpecialConstraintMask;
            }
            if (attributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
            {
                return GenericParameterAttributes.NotNullableValueTypeConstraint;
            }
            if (attributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint) && attributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
            {
                return GenericParameterAttributes.ReferenceTypeConstraint | GenericParameterAttributes.DefaultConstructorConstraint;
            }
            if (attributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
            {
                return GenericParameterAttributes.ReferenceTypeConstraint;
            }
            if (attributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
            {
                return GenericParameterAttributes.DefaultConstructorConstraint;
            }
            return GenericParameterAttributes.None;
        }

        internal static Type ReplaceTypeUsingGenericParameters(Type type, GenericTypeParameterBuilder[] genericParameters)
        {
            if (type.ContainsGenericParameters)
            {
                if (type.IsAutoLayout && type.Name.EndsWith('&'))
                {
                    return ReplaceTypeUsingGenericParameters(type.GetElementType(), genericParameters).MakeByRefType();
                }
                else if (type.IsGenericType)
                {
                    var replacedGenericParameters = type.GetGenericArguments().Select(g => ReplaceTypeUsingGenericParameters(genericParameters.FirstOrDefault(g1 => g1.Name == g.Name) ?? g, genericParameters)).ToArray();
                    if (!type.IsGenericTypeDefinition)
                        type = type.GetGenericTypeDefinition();
                    return type.MakeGenericType(replacedGenericParameters);
                }
                else
                {
                    return genericParameters.First(g => g.Name == type.Name);
                }
            }
            return type;
        }
    }
}
