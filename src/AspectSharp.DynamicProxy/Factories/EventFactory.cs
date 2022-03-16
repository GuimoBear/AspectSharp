using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectSharp.DynamicProxy.Factories
{
    internal static class EventFactory
    {
        public static void CreateEvents(Type serviceType, TypeBuilder typeBuilder, IEnumerable<MethodBuilder> methods, FieldInfo targetField)
        {
            foreach (var @event in serviceType.GetEvents())
            {
                var eventBuilder = typeBuilder.DefineEvent(@event.Name, @event.Attributes, @event.EventHandlerType);

                var addMethodInfo = @event.GetAddMethod();
                if (addMethodInfo != null)
                {
                    var attrs = (addMethodInfo.Attributes ^ MethodAttributes.Abstract) | MethodAttributes.Final;
                    var previouslyCreatedMethod = methods.FirstOrDefault(mi => mi.Name == addMethodInfo.Name && mi.ReturnType == addMethodInfo.ReturnType && mi.Attributes == attrs && mi.CallingConvention == addMethodInfo.CallingConvention);

                    if (!(previouslyCreatedMethod is null))
                        eventBuilder.SetAddOnMethod(previouslyCreatedMethod);
                }

                var removeMethodInfo = @event.GetRemoveMethod();
                if (removeMethodInfo != null)
                {
                    var attrs = (removeMethodInfo.Attributes ^ MethodAttributes.Abstract) | MethodAttributes.Final;
                    var previouslyCreatedMethod = methods.FirstOrDefault(mi => mi.Name == removeMethodInfo.Name && mi.ReturnType == removeMethodInfo.ReturnType && mi.Attributes == attrs && mi.CallingConvention == removeMethodInfo.CallingConvention);

                    if (!(previouslyCreatedMethod is null))
                        eventBuilder.SetRemoveOnMethod(previouslyCreatedMethod);
                }
            }
        }
    }
}
