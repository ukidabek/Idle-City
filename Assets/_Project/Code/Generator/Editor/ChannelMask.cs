using System;

namespace Code.Generator
{
    [Flags]
    public enum ChannelMask
    {
        None = 0,
        R = 1,
        G = 2,
        B = 4,
        A = 8,
        All = R | G | B | A
    }
}