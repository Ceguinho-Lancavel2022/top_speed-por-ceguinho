using System;
using System.Collections.Generic;
using TopSpeed.Menu;
using TopSpeed.Protocol;

namespace TopSpeed.Core.Multiplayer
{
    internal sealed partial class MultiplayerCoordinator
    {
        private const string OnlinePlayersScreenId = "online_players_main";
        private const string MainRoomName = "main room";

        private void OpenOnlinePlayersMenu()
        {
            var session = SessionOrNull();
            if (session == null)
            {
                _speech.Speak("Not connected to a server.");
                return;
            }

            if (_state.Rooms.IsOnlinePlayersOpenPending)
                return;

            _state.Rooms.IsOnlinePlayersOpenPending = true;
            if (!TrySend(session.SendOnlinePlayersRequest(), "online players request"))
                _state.Rooms.IsOnlinePlayersOpenPending = false;
        }

        private void RebuildOnlinePlayersMenu()
        {
            var players = _state.Rooms.OnlinePlayers.Players ?? Array.Empty<OnlinePlayerInfo>();
            var items = new List<MenuItem>();
            for (var i = 0; i < players.Length; i++)
            {
                items.Add(new MenuItem(FormatOnlinePlayerLabel(players[i]), MenuAction.None));
            }

            items.Add(new MenuItem("Go back", MenuAction.Back));
            _menu.SetScreens(
                MultiplayerMenuKeys.OnlinePlayers,
                new[] { new MenuView(OnlinePlayersScreenId, items, $"{players.Length} people are connected.") },
                OnlinePlayersScreenId);
        }

        private static string FormatOnlinePlayerLabel(OnlinePlayerInfo player)
        {
            var name = string.IsNullOrWhiteSpace(player.Name)
                ? $"Player {player.PlayerNumber + 1}"
                : player.Name;
            var roomName = string.IsNullOrWhiteSpace(player.RoomName)
                ? MainRoomName
                : player.RoomName;
            var state = player.PresenceState switch
            {
                OnlinePresenceState.PreparingToRace => "Preparing to race",
                OnlinePresenceState.Racing => "racing",
                _ => "available"
            };
            return $"{name}, {state}: {roomName}";
        }
    }
}
