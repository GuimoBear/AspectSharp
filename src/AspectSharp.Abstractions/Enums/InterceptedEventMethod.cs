using System;

namespace AspectSharp.Abstractions.Enums
{
    [Flags]
    public enum InterceptedEventMethod : byte
    {
        None = 0,
        Add = 1,
        Remove = 2,
        All = Add | Remove
    }
}
