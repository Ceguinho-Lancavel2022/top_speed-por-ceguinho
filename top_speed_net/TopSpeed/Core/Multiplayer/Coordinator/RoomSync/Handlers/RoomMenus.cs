using System.Collections.Generic;

namespace TopSpeed.Core.Multiplayer
{
    internal sealed partial class MultiplayerCoordinator
    {
        private static void AddRoomMenuRebuildEffects(List<PacketEffect> effects)
        {
            effects.Add(PacketEffect.RebuildRoomControls());
            effects.Add(PacketEffect.RebuildRoomOptions());
            effects.Add(PacketEffect.RebuildRoomGameRules());
        }
    }
}
