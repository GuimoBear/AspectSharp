using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectSharp.DynamicProxy.Factories
{
    internal static class PropertyFactory
    {
        public static void CreateProperties(Type serviceType, TypeBuilder typeBuilder, IEnumerable<MethodBuilder> methods, FieldInfo targetField)
        {
            var properties = serviceType.GetProperties();
            foreach (var property in properties)
            {
                var propertyBuilder = typeBuilder.DefineProperty(property.Name, PropertyAttributes.None, property.PropertyType, null);

                if (property.CanRead)
                {
                    var propertyMethod = property.GetGetMethod();
                    var attrs = propertyMethod.Attributes ^ MethodAttributes.Abstract;

                    var previouslyCreatedMethod = methods.FirstOrDefault(mi => mi.Name == propertyMethod.Name && mi.ReturnType == propertyMethod.ReturnType && mi.Attributes == attrs && mi.CallingConvention == propertyMethod.CallingConvention);

                    if (previouslyCreatedMethod is not null)
                        propertyBuilder.SetGetMethod(previouslyCreatedMethod);
                    else
                    {
                        var methodBuilder = typeBuilder.DefineMethod(string.Format("get_{0}", property.Name), attrs, propertyMethod.ReturnType, propertyMethod.GetParameters().Select(p => p.ParameterType).ToArray());
                        var cil = methodBuilder.GetILGenerator();
                        cil.Emit(OpCodes.Ldarg_0);
                        cil.Emit(OpCodes.Ldfld, targetField);
                        cil.Emit(OpCodes.Callvirt, propertyMethod);
                        cil.Emit(OpCodes.Ret);
                        propertyBuilder.SetGetMethod(methodBuilder);
                    }
                }
                if (property.CanWrite)
                {
                    var propertyMethod = property.GetSetMethod();
                    var attrs = propertyMethod.Attributes ^ MethodAttributes.Abstract;

                    var previouslyCreatedMethod = methods.FirstOrDefault(mi => mi.Name == propertyMethod.Name && mi.ReturnType == propertyMethod.ReturnType && mi.Attributes == attrs && mi.CallingConvention == propertyMethod.CallingConvention);

                    if (previouslyCreatedMethod is not null)
                        propertyBuilder.SetSetMethod(previouslyCreatedMethod);
                    else
                    {
                        var methodBuilder = typeBuilder.DefineMethod(string.Format("set_{0}", property.Name), attrs, propertyMethod.ReturnType, propertyMethod.GetParameters().Select(p => p.ParameterType).ToArray());
                        var cil = methodBuilder.GetILGenerator();
                        cil.Emit(OpCodes.Ldarg_0);
                        cil.Emit(OpCodes.Ldfld, targetField);
                        cil.Emit(OpCodes.Ldarg_1);
                        cil.Emit(OpCodes.Callvirt, propertyMethod);
                        cil.Emit(OpCodes.Ret);
                        propertyBuilder.SetSetMethod(methodBuilder);
                    }
                }
            }
        }
    }
}
