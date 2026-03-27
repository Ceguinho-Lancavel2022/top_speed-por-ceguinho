using System.Collections.Generic;
using TopSpeed.Core.Multiplayer.Chat;
using TopSpeed.Network;
using TopSpeed.Protocol;

namespace TopSpeed.Core.Multiplayer
{
    internal sealed partial class MultiplayerCoordinator
    {
        public void HandleRoomEvent(PacketRoomEvent roomEvent)
        {
            _roomsFlow.HandleRoomEvent(roomEvent);
        }

        internal void HandleRoomEventCore(PacketRoomEvent roomEvent)
        {
            var eventInfo = RoomMap.ToEvent(roomEvent);
            if (eventInfo == null)
                return;

            var effects = new List<PacketEffect>();
            var session = SessionOrNull();
            var isCreator = session != null && eventInfo.HostPlayerId == session.PlayerId;
            var suppressRemoteRoomCreatedNotice = ShouldSuppressRemoteRoomCreatedNotice(eventInfo, isCreator);

            if (eventInfo.Kind == RoomEventKind.RoomCreated && !isCreator && !suppressRemoteRoomCreatedNotice)
                effects.Add(PacketEffect.PlaySound("room_created.ogg"));

            if (!suppressRemoteRoomCreatedNotice)
            {
                var roomEventText = HistoryText.FromRoomEvent(eventInfo);
                if (!string.IsNullOrWhiteSpace(roomEventText))
                    effects.Add(PacketEffect.AddRoomEventHistory(roomEventText));
            }

            ApplyRoomListEvent(eventInfo);

            ApplyCurrentRoomEvent(eventInfo, effects, out var beginLoadout, out var localHostChanged);
            if (localHostChanged)
                AddRoomMenuRebuildEffects(effects);

            if (_state.Rooms.CurrentRoom.InRoom)
                effects.Add(PacketEffect.RebuildRoomPlayers());

            if (beginLoadout)
                effects.Add(PacketEffect.BeginRaceLoadout());

            DispatchPacketEffects(effects);
        }

        private bool ShouldSuppressRemoteRoomCreatedNotice(RoomEventInfo eventInfo, bool isCreator)
        {
            return eventInfo.Kind == RoomEventKind.RoomCreated
                && _state.Rooms.CurrentRoom.InRoom
                && !isCreator
                && _state.Rooms.CurrentRoom.RoomId != eventInfo.RoomId;
        }
    }
}
