using System;

namespace TopSpeed.Protocol
{
    [Flags]
    public enum RoomGameRules : uint
    {
        None = 0,
        GhostMode = 1u << 0
    }
}
