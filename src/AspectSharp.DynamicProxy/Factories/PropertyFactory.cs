using AspectSharp.DynamicProxy.Utils;
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
            var properties = serviceType.GetPropertiesRecursively();
            foreach (var property in properties)
            {
                var propertyBuilder = typeBuilder.DefineProperty(property.Name, PropertyAttributes.None, property.PropertyType, null);

                if (property.CanRead)
                {
                    var propertyMethod = property.GetGetMethod();
                    var attrs = (propertyMethod.Attributes ^ MethodAttributes.Abstract) | MethodAttributes.Final;

                    var previouslyCreatedMethod = methods.FirstOrDefault(mi => mi.Name == propertyMethod.Name && mi.ReturnType == propertyMethod.ReturnType && mi.Attributes == attrs && mi.CallingConvention == propertyMethod.CallingConvention);

                    if (!(previouslyCreatedMethod is null))
                        propertyBuilder.SetGetMethod(previouslyCreatedMethod);
                }
                if (property.CanWrite)
                {
                    var propertyMethod = property.GetSetMethod();
                    var attrs = (propertyMethod.Attributes ^ MethodAttributes.Abstract) | MethodAttributes.Final;

                    var previouslyCreatedMethod = methods.FirstOrDefault(mi => mi.Name == propertyMethod.Name && mi.ReturnType == propertyMethod.ReturnType && mi.Attributes == attrs && mi.CallingConvention == propertyMethod.CallingConvention);

                    if (!(previouslyCreatedMethod is null))
                        propertyBuilder.SetSetMethod(previouslyCreatedMethod);
                }
            }
        }
    }
}
