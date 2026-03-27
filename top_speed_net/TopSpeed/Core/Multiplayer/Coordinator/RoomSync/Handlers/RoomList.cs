using System;
using System.Collections.Generic;
using TopSpeed.Localization;
using TopSpeed.Menu;
using TopSpeed.Network;
using TopSpeed.Protocol;

namespace TopSpeed.Core.Multiplayer
{
    internal sealed partial class MultiplayerCoordinator
    {
        public void HandleRoomList(PacketRoomList roomList)
        {
            _roomsFlow.HandleRoomList(roomList);
        }

        internal void HandleRoomListCore(PacketRoomList roomList)
        {
            var effects = new List<PacketEffect>();
            _state.Rooms.RoomList = RoomMap.ToList(roomList);
            if (!_state.Rooms.IsRoomBrowserOpenPending)
                return;

            _state.Rooms.IsRoomBrowserOpenPending = false;
            if (!string.Equals(_menu.CurrentId, MultiplayerMenuKeys.Lobby, StringComparison.Ordinal))
                return;

            var rooms = _state.Rooms.RoomList.Rooms ?? Array.Empty<RoomSummaryInfo>();
            if (rooms.Length == 0)
            {
                effects.Add(PacketEffect.Speak(LocalizationService.Mark("No game rooms are currently available.")));
                DispatchPacketEffects(effects);
                return;
            }

            effects.Add(PacketEffect.UpdateRoomBrowser());
            effects.Add(PacketEffect.Push(MultiplayerMenuKeys.RoomBrowser));
            DispatchPacketEffects(effects);
        }
    }
}
