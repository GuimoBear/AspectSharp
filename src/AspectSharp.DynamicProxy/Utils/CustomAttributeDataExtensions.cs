using System.Linq;
using System.Reflection;

namespace AspectSharp.DynamicProxy.Utils
{
    internal static class CustomAttributeDataExtensions
    {
        public static object CreateInstance(this CustomAttributeData data)
        {
            var arguments = data.ConstructorArguments.Select(arg => arg.Value);

            var attribute = data.Constructor.Invoke(arguments.ToArray());

            foreach (var namedArgument in data.NamedArguments)
            {
                if (namedArgument.MemberInfo is PropertyInfo propertyInfo)
                    propertyInfo.SetValue(attribute, namedArgument.TypedValue.Value, null);
                else if (namedArgument.MemberInfo is FieldInfo fieldInfo)
                    fieldInfo.SetValue(attribute, namedArgument.TypedValue.Value);
            }

            return attribute;
        }
    }
}
