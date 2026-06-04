using System;

namespace Project.Map
{
    [Flags]
    public enum NeighborDirection
    {
        None      = 0,
        Left      = 1 << 0, // 1
        Right     = 1 << 1, // 2
        Up        = 1 << 2, // 4
        Down      = 1 << 3, // 8
        UpLeft    = 1 << 4, // 16
        UpRight   = 1 << 5, // 32
        DownLeft  = 1 << 6, // 64
        DownRight = 1 << 7, // 128
    }
}