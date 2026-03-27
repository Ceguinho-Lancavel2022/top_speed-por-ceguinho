using System;
using System.Collections.Generic;
using TopSpeed.Core.Multiplayer.Chat;
using TopSpeed.Menu;
using TopSpeed.Protocol;

namespace TopSpeed.Core.Multiplayer
{
    internal sealed partial class MultiplayerCoordinator
    {
        public void HandleRoomState(PacketRoomState roomState)
        {
            _roomsFlow.HandleRoomState(roomState);
        }

        internal void HandleRoomStateCore(PacketRoomState roomState)
        {
            var effects = new List<PacketEffect>();
            var wasInRoom = _state.Rooms.WasInRoom;
            var previousRoomId = _state.Rooms.LastRoomId;
            var previousIsHost = _state.Rooms.WasHost;
            var previousRoomType = _state.Rooms.CurrentRoom.RoomType;
            _state.Rooms.CurrentRoom = RoomMap.ToSnapshot(roomState);

            AddRoomJoinLeaveEffects(effects, wasInRoom, previousRoomId);

            _state.Rooms.WasInRoom = _state.Rooms.CurrentRoom.InRoom;
            _state.Rooms.LastRoomId = _state.Rooms.CurrentRoom.RoomId;
            _state.Rooms.WasHost = _state.Rooms.CurrentRoom.IsHost;
            if (!_state.Rooms.CurrentRoom.InRoom || !_state.Rooms.CurrentRoom.IsHost)
                effects.Add(PacketEffect.CancelRoomOptions());

            AddRoomStateNavigationEffects(effects, wasInRoom, previousRoomId);

            var roomControlsChanged =
                wasInRoom != _state.Rooms.CurrentRoom.InRoom ||
                previousIsHost != _state.Rooms.CurrentRoom.IsHost ||
                previousRoomType != _state.Rooms.CurrentRoom.RoomType;
            if (roomControlsChanged)
                AddRoomMenuRebuildEffects(effects);

            effects.Add(PacketEffect.RebuildRoomPlayers());
            DispatchPacketEffects(effects);
        }

        private void AddRoomJoinLeaveEffects(List<PacketEffect> effects, bool wasInRoom, uint previousRoomId)
        {
            if (_state.Rooms.CurrentRoom.InRoom)
            {
                if (!wasInRoom || previousRoomId != _state.Rooms.CurrentRoom.RoomId)
                {
                    effects.Add(PacketEffect.PlaySound("room_join.ogg"));
                    effects.Add(PacketEffect.AddRoomEventHistory(HistoryText.JoinedRoom(_state.Rooms.CurrentRoom.RoomName)));
                }

                return;
            }

            if (!wasInRoom)
                return;

            effects.Add(PacketEffect.PlaySound("room_leave.ogg"));
            var leaveText = HistoryText.LeftRoom();
            effects.Add(PacketEffect.Speak(leaveText));
            effects.Add(PacketEffect.AddRoomEventHistory(leaveText));
        }

        private void AddRoomStateNavigationEffects(List<PacketEffect> effects, bool wasInRoom, uint previousRoomId)
        {
            if (_state.Rooms.CurrentRoom.InRoom && (!wasInRoom || previousRoomId != _state.Rooms.CurrentRoom.RoomId))
            {
                effects.Add(PacketEffect.ShowRoot(MultiplayerMenuKeys.RoomControls));
            }
            else if (!_state.Rooms.CurrentRoom.InRoom && wasInRoom)
            {
                effects.Add(PacketEffect.ShowRoot(MultiplayerMenuKeys.Lobby));
            }
        }
    }
}
