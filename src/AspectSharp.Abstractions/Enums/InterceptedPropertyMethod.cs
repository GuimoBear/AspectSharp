using System;

namespace AspectSharp.Abstractions.Enums
{
    [Flags]
    public enum InterceptedPropertyMethod : byte
    {
        None = 0,
        Get = 1, 
        Set = 2,
        All = Get | Set
    }
}
