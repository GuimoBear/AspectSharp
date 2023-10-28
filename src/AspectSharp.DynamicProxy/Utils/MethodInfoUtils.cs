using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AspectSharp.DynamicProxy.Utils
{
    internal static class MethodInfoUtils
    {
        private static readonly ConcurrentDictionary<MethodInfo, string> _cachedStringRepresentations = new ConcurrentDictionary<MethodInfo, string>();

        public static string StringRepresentation(MethodInfo methodInfo)
        {
            return _cachedStringRepresentations.GetOrAdd(methodInfo, mi =>
            {
                var sb = new StringBuilder();
                //sb.AppendFormat("{0} ", mi.Attributes.ToString().Replace(",", "").ToLower());
                sb.AppendFormat("{0} ", TypeStringRepresentation(mi.ReturnType));
                sb.Append(mi.Name);
                if (mi.ContainsGenericParameters)
                {
                    sb.AppendFormat("<{0}>", string.Join(", ", mi.GetGenericArguments().Select(TypeStringRepresentation)));
                }
                sb.AppendFormat("({0})", string.Join(", ", mi.GetParameters().Select(ParameterStringRepresentation)));
                if (mi.ContainsGenericParameters)
                {
                    var genericArguments = mi.GetGenericArguments();
                    var constraints = new List<string>(genericArguments.Length);
                    for (int i = 0; i < genericArguments.Length; i++)
                    {
                        var strConstraints = "";
                        var genericArgument = genericArguments[i];
                        var attributes = genericArgument.GenericParameterAttributes;
                        if (attributes != GenericParameterAttributes.None)
                            strConstraints += string.Format("{0} ", attributes.ToString().ToLower());
                        strConstraints += string.Join(", ", genericArgument.GetGenericParameterConstraints().Select(t => TypeStringRepresentation(t.GetTypeInfo())));
                        if (!string.IsNullOrWhiteSpace(strConstraints))
                            constraints.Add(string.Format("{0}: {1}", genericArgument.Name, strConstraints));
                    }
                    if (constraints.Count > 0)
                        sb.AppendFormat("where {0}", string.Join(' ', constraints));
                }

                return sb.ToString().Trim();
            });
        }

        private static string TypeStringRepresentation(Type type)
        {
            if (type.IsGenericType)
                return string.Format("{0}<{1}>", (type.FullName ?? type.Name).TrimEnd('&'), string.Join(", ", type.GetGenericArguments().Select(g => TypeStringRepresentation(g))));
            return (type.FullName ?? type.Name).TrimEnd('&');
        }

        private static string ParameterStringRepresentation(ParameterInfo pi)
        {
            var stringTemplate = "{0} {1}";
            if (pi.IsOut)
                stringTemplate = "out {0} {1}";
            else if (pi.ParameterType.IsByRef && pi.ParameterType.IsAutoLayout && pi.ParameterType.Name.EndsWith('&'))
                stringTemplate = "ref {0} {1}";
            return string.Format(stringTemplate, TypeStringRepresentation(pi.ParameterType), pi.Name);
        }
    }
}
