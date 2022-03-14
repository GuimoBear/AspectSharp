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
                    var attrs = addMethodInfo.Attributes ^ MethodAttributes.Abstract;
                    var previouslyCreatedMethod = methods.FirstOrDefault(mi => mi.Name == addMethodInfo.Name && mi.ReturnType == addMethodInfo.ReturnType && mi.Attributes == attrs && mi.CallingConvention == addMethodInfo.CallingConvention);

                    if (previouslyCreatedMethod is not null)
                        eventBuilder.SetAddOnMethod(previouslyCreatedMethod);
                    else
                    {
                        var parameters = addMethodInfo.GetParameters();

                        var methodBuilder = typeBuilder.DefineMethod(addMethodInfo.Name, attrs, addMethodInfo.CallingConvention, addMethodInfo.ReturnType, parameters.Select(p => p.ParameterType).ToArray());

                        foreach (var parameter in parameters)
                            methodBuilder.DefineParameter(parameter.Position, parameter.Attributes, parameter.Name);

                        var cil = methodBuilder.GetILGenerator();
                        cil.Emit(OpCodes.Ldarg_0);
                        cil.Emit(OpCodes.Ldfld, targetField);
                        cil.Emit(OpCodes.Ldarg_1);
                        cil.Emit(OpCodes.Callvirt, addMethodInfo);
                        cil.Emit(OpCodes.Ret);

                        eventBuilder.SetAddOnMethod(methodBuilder);
                    }
                }

                var removeMethodInfo = @event.GetRemoveMethod();
                if (removeMethodInfo != null)
                {
                    var attrs = removeMethodInfo.Attributes ^ MethodAttributes.Abstract;
                    var previouslyCreatedMethod = methods.FirstOrDefault(mi => mi.Name == removeMethodInfo.Name && mi.ReturnType == removeMethodInfo.ReturnType && mi.Attributes == attrs && mi.CallingConvention == removeMethodInfo.CallingConvention);

                    if (previouslyCreatedMethod is not null)
                        eventBuilder.SetRemoveOnMethod(previouslyCreatedMethod);
                    else
                    {
                        var parameters = removeMethodInfo.GetParameters();
                        var methodBuilder = typeBuilder.DefineMethod(removeMethodInfo.Name, attrs, removeMethodInfo.CallingConvention, removeMethodInfo.ReturnType, parameters.Select(p => p.ParameterType).ToArray());

                        foreach (var parameter in parameters)
                            methodBuilder.DefineParameter(parameter.Position, parameter.Attributes, parameter.Name);

                        var cil = methodBuilder.GetILGenerator();
                        cil.Emit(OpCodes.Ldarg_0);
                        cil.Emit(OpCodes.Ldfld, targetField);
                        cil.Emit(OpCodes.Ldarg_1);
                        cil.Emit(OpCodes.Callvirt, removeMethodInfo);
                        cil.Emit(OpCodes.Ret);
                        eventBuilder.SetRemoveOnMethod(methodBuilder);
                    }
                }
            }
        }
    }
}
