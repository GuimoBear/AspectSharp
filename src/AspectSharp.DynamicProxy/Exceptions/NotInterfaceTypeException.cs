using System;

namespace AspectSharp.DynamicProxy.Exceptions
{
    public class NotInterfaceTypeException : Exception
    {
        public NotInterfaceTypeException(Type type) : base(string.Format("'{0}' must be an interface", type.FullName)) { }
    }
}
