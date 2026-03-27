using System;
using TopSpeed.Localization;
using TopSpeed.Menu;
using TopSpeed.Protocol;

namespace TopSpeed.Core.Multiplayer
{
    internal sealed partial class MultiplayerCoordinator
    {
        public void HandleOnlinePlayers(PacketOnlinePlayers onlinePlayers)
        {
            _roomsFlow.HandleOnlinePlayers(onlinePlayers);
        }

        internal void HandleOnlinePlayersCore(PacketOnlinePlayers onlinePlayers)
        {
            _state.Rooms.OnlinePlayers = OnlineMap.ToList(onlinePlayers);
            if (!_state.Rooms.IsOnlinePlayersOpenPending)
                return;

            _state.Rooms.IsOnlinePlayersOpenPending = false;
            if (!string.Equals(_menu.CurrentId, MultiplayerMenuKeys.Lobby, StringComparison.Ordinal))
                return;

            var players = _state.Rooms.OnlinePlayers.Players ?? Array.Empty<OnlinePlayerInfo>();
            if (players.Length < 2)
            {
                _speech.Speak(LocalizationService.Mark("Only you are connected right now."));
                return;
            }

            RebuildOnlinePlayersMenu();
            _menu.Push(MultiplayerMenuKeys.OnlinePlayers);
        }
    }
}
