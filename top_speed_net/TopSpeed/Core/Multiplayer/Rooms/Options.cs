using System;
using System.Collections.Generic;
using System.IO;
using TopSpeed.Common;
using TopSpeed.Data;
using TopSpeed.Localization;
using TopSpeed.Menu;
using TopSpeed.Protocol;

namespace TopSpeed.Core.Multiplayer
{
    internal sealed partial class MultiplayerCoordinator
    {
        private readonly struct RoomCustomTrackOption
        {
            public RoomCustomTrackOption(string key, string display)
            {
                Key = key ?? string.Empty;
                Display = display ?? string.Empty;
            }

            public string Key { get; }
            public string Display { get; }
        }

        private void OpenRoomOptionsMenu()
        {
            if (!_state.Rooms.CurrentRoom.InRoom)
            {
                _speech.Speak(LocalizationService.Mark("You are not currently inside a game room."));
                return;
            }

            if (!_state.Rooms.CurrentRoom.IsHost)
            {
                _speech.Speak(LocalizationService.Mark("Only the host can change game options."));
                return;
            }

            BeginRoomOptionsDraft();
            RebuildRoomOptionsMenu();
            _menu.Push(MultiplayerMenuKeys.RoomOptions);
        }

        private void BeginRoomOptionsDraft()
        {
            _state.Rooms.RoomOptionsDraftActive = true;
            _state.Rooms.RoomOptionsTrackRandom = false;
            _state.Rooms.RoomOptionsTrackName = string.IsNullOrWhiteSpace(_state.Rooms.CurrentRoom.TrackName)
                ? TrackList.RaceTracks[0].Key
                : _state.Rooms.CurrentRoom.TrackName;
            _state.Rooms.RoomOptionsLaps = _state.Rooms.CurrentRoom.Laps > 0 ? _state.Rooms.CurrentRoom.Laps : (byte)1;
            _state.Rooms.RoomOptionsPlayersToStart = _state.Rooms.CurrentRoom.PlayersToStart >= 2 ? _state.Rooms.CurrentRoom.PlayersToStart : (byte)2;
            _state.Rooms.RoomOptionsGameRulesFlags = _state.Rooms.CurrentRoom.GameRulesFlags;
            if (_state.Rooms.CurrentRoom.RoomType == GameRoomType.OneOnOne)
                _state.Rooms.RoomOptionsPlayersToStart = 2;
        }

        private void CancelRoomOptionsChanges()
        {
            _state.Rooms.RoomOptionsDraftActive = false;
            _state.Rooms.RoomOptionsTrackRandom = false;
            _state.Rooms.RoomOptionsGameRulesFlags = 0;
        }

        private void ConfirmRoomOptionsChanges()
        {
            var session = SessionOrNull();
            if (session == null)
            {
                _speech.Speak(LocalizationService.Mark("Not connected to a server."));
                return;
            }

            if (!_state.Rooms.CurrentRoom.InRoom || !_state.Rooms.CurrentRoom.IsHost || !_state.Rooms.RoomOptionsDraftActive)
            {
                _speech.Speak(LocalizationService.Mark("Only the host can change game options."));
                return;
            }

            var appliedAny = false;
            var currentTrack = string.IsNullOrWhiteSpace(_state.Rooms.CurrentRoom.TrackName) ? TrackList.RaceTracks[0].Key : _state.Rooms.CurrentRoom.TrackName;
            if (!string.Equals(currentTrack, _state.Rooms.RoomOptionsTrackName, StringComparison.OrdinalIgnoreCase))
            {
                if (!TrySend(session.SendRoomSetTrack(_state.Rooms.RoomOptionsTrackName), "track change request"))
                    return;
                appliedAny = true;
            }

            if (_state.Rooms.CurrentRoom.Laps != _state.Rooms.RoomOptionsLaps)
            {
                if (!TrySend(session.SendRoomSetLaps(_state.Rooms.RoomOptionsLaps), "lap count change request"))
                    return;
                appliedAny = true;
            }

            if (_state.Rooms.CurrentRoom.RoomType != GameRoomType.OneOnOne)
            {
                var playersToStart = _state.Rooms.RoomOptionsPlayersToStart < 2 ? (byte)2 : _state.Rooms.RoomOptionsPlayersToStart;
                if (_state.Rooms.CurrentRoom.PlayersToStart != playersToStart)
                {
                    if (!TrySend(session.SendRoomSetPlayersToStart(playersToStart), "player count change request"))
                        return;
                    appliedAny = true;
                }
            }

            var gameRules = _state.Rooms.RoomOptionsGameRulesFlags & (uint)RoomGameRules.GhostMode;
            if (_state.Rooms.CurrentRoom.GameRulesFlags != gameRules)
            {
                if (!TrySend(session.SendRoomSetGameRules(gameRules), "game rules change request"))
                    return;
                appliedAny = true;
            }

            CancelRoomOptionsChanges();
            _menu.ShowRoot(MultiplayerMenuKeys.RoomControls);
            _speech.Speak(appliedAny
                ? LocalizationService.Mark("Room options updated.")
                : LocalizationService.Mark("No option changes to apply."));
        }

        private string GetRoomOptionsTrackText()
        {
            if (!_state.Rooms.RoomOptionsDraftActive)
                BeginRoomOptionsDraft();

            if (_state.Rooms.RoomOptionsTrackRandom)
            {
                return LocalizationService.Mark("Track, currently random chosen.");
            }

            var trackName = TryGetTrackDisplay(_state.Rooms.RoomOptionsTrackName, out var display)
                ? display
                : _state.Rooms.RoomOptionsTrackName;
            return LocalizationService.Format(
                LocalizationService.Mark("Track, currently {0}."),
                trackName);
        }

        private int GetRoomOptionsLapsIndex()
        {
            if (!_state.Rooms.RoomOptionsDraftActive)
                BeginRoomOptionsDraft();

            var laps = _state.Rooms.RoomOptionsLaps < 1 ? (byte)1 : _state.Rooms.RoomOptionsLaps;
            return Math.Max(0, Math.Min(LapCountOptions.Length - 1, laps - 1));
        }

        private void SetRoomOptionsLaps(byte laps)
        {
            if (!_state.Rooms.RoomOptionsDraftActive)
                BeginRoomOptionsDraft();

            if (laps < 1 || laps > 16)
                return;
            _state.Rooms.RoomOptionsLaps = laps;
        }

        private int GetRoomOptionsPlayersToStartIndex()
        {
            if (!_state.Rooms.RoomOptionsDraftActive)
                BeginRoomOptionsDraft();

            var playersToStart = _state.Rooms.RoomOptionsPlayersToStart < 2 ? (byte)2 : _state.Rooms.RoomOptionsPlayersToStart;
            return Math.Max(0, Math.Min(RoomCapacityOptions.Length - 1, playersToStart - 2));
        }

        private void SetRoomOptionsPlayersToStart(byte playersToStart)
        {
            if (!_state.Rooms.RoomOptionsDraftActive)
                BeginRoomOptionsDraft();

            if (_state.Rooms.CurrentRoom.RoomType == GameRoomType.OneOnOne)
            {
                _state.Rooms.RoomOptionsPlayersToStart = 2;
                return;
            }

            if (playersToStart < 2 || playersToStart > ProtocolConstants.MaxRoomPlayersToStart)
                return;

            _state.Rooms.RoomOptionsPlayersToStart = playersToStart;
        }

        private void OpenRoomTrackTypeMenu()
        {
            if (!_state.Rooms.RoomOptionsDraftActive)
                BeginRoomOptionsDraft();

            RebuildRoomTrackTypeMenu();
            RebuildRoomTrackMenu(MultiplayerMenuKeys.RoomTrackRace, TrackCategory.RaceTrack);
            RebuildRoomTrackMenu(MultiplayerMenuKeys.RoomTrackAdventure, TrackCategory.StreetAdventure);
            _menu.Push(MultiplayerMenuKeys.RoomTrackType);
        }

        private void OpenRoomGameRulesMenu()
        {
            if (!_state.Rooms.RoomOptionsDraftActive)
                BeginRoomOptionsDraft();

            RebuildRoomGameRulesMenu();
            _menu.Push(MultiplayerMenuKeys.RoomGameRules);
        }

        private bool GetRoomOptionsGhostModeEnabled()
        {
            if (!_state.Rooms.RoomOptionsDraftActive)
                BeginRoomOptionsDraft();

            return (_state.Rooms.RoomOptionsGameRulesFlags & (uint)RoomGameRules.GhostMode) != 0u;
        }

        private void SetRoomOptionsGhostModeEnabled(bool enabled)
        {
            if (!_state.Rooms.RoomOptionsDraftActive)
                BeginRoomOptionsDraft();

            var flags = _state.Rooms.RoomOptionsGameRulesFlags;
            if (enabled)
                flags |= (uint)RoomGameRules.GhostMode;
            else
                flags &= ~(uint)RoomGameRules.GhostMode;

            _state.Rooms.RoomOptionsGameRulesFlags = flags;
        }

        private void AnnounceCurrentRoomGameRules()
        {
            if (!_state.Rooms.CurrentRoom.InRoom)
            {
                _speech.Speak(LocalizationService.Mark("You are not currently inside a game room."));
                return;
            }

            _speech.Speak(FormatGameRulesSummary(_state.Rooms.CurrentRoom.GameRulesFlags));
        }

        private static string FormatGameRulesSummary(uint gameRulesFlags)
        {
            var ghostEnabled = (gameRulesFlags & (uint)RoomGameRules.GhostMode) != 0u;
            return ghostEnabled
                ? LocalizationService.Mark("Ghost mode is enabled.")
                : LocalizationService.Mark("Ghost mode is disabled.");
        }

        private void RebuildRoomTrackTypeMenu()
        {
            var items = new List<MenuItem>
            {
                new MenuItem(LocalizationService.Mark("Race track"), MenuAction.None, nextMenuId: MultiplayerMenuKeys.RoomTrackRace),
                new MenuItem(LocalizationService.Mark("Street adventure"), MenuAction.None, nextMenuId: MultiplayerMenuKeys.RoomTrackAdventure),
                new MenuItem(LocalizationService.Mark("Custom track"), MenuAction.None, onActivate: OpenRoomCustomTrackMenuOrAnnounce),
                new MenuItem(LocalizationService.Mark("Random"), MenuAction.None, onActivate: SelectRandomRoomTrackAny),
                new MenuItem(LocalizationService.Mark("Go back"), MenuAction.Back)
            };

            _menu.UpdateItems(MultiplayerMenuKeys.RoomTrackType, items);
        }

        private void RebuildRoomTrackMenu(string menuId, TrackCategory category)
        {
            var items = new List<MenuItem>();
            var tracks = TrackList.GetTracks(category);
            for (var i = 0; i < tracks.Count; i++)
            {
                var track = tracks[i];
                items.Add(new MenuItem(track.Display, MenuAction.None, onActivate: () => SelectRoomTrack(track.Key, false)));
            }

            items.Add(new MenuItem(LocalizationService.Mark("Random"), MenuAction.None, onActivate: () => SelectRandomRoomTrackCategory(category)));
            items.Add(new MenuItem(LocalizationService.Mark("Go back"), MenuAction.Back));
            _menu.UpdateItems(menuId, items);
        }

        private void OpenRoomCustomTrackMenuOrAnnounce()
        {
            var customTracks = GetRoomCustomTrackOptions();
            if (customTracks.Count == 0)
            {
                _speech.Speak(LocalizationService.Mark("No compatible custom tracks found. For now, each custom track folder name must be 12 characters or less."));
                return;
            }

            RebuildRoomCustomTrackMenu();
            _menu.Push(MultiplayerMenuKeys.RoomTrackRace);
        }

        private void RebuildRoomCustomTrackMenu()
        {
            var items = new List<MenuItem>();
            var customTracks = GetRoomCustomTrackOptions();
            for (var i = 0; i < customTracks.Count; i++)
            {
                var track = customTracks[i];
                items.Add(new MenuItem(track.Display, MenuAction.None, onActivate: () => SelectRoomTrack(track.Key, false)));
            }

            items.Add(new MenuItem(LocalizationService.Mark("Random"), MenuAction.None, onActivate: () => SelectRandomRoomTrackCategory(TrackCategory.CustomTrack)));
            items.Add(new MenuItem(LocalizationService.Mark("Go back"), MenuAction.Back));
            _menu.UpdateItems(MultiplayerMenuKeys.RoomTrackRace, items);
        }

        private void SelectRandomRoomTrackAny()
        {
            var customTracks = GetRoomCustomTrackOptions();
            var total = RoomTrackOptions.Length + customTracks.Count;
            if (total <= 0)
            {
                SelectRoomTrack(TrackList.RaceTracks[0].Key, true);
                return;
            }

            var index = Algorithm.RandomInt(total);
            if (index < RoomTrackOptions.Length)
            {
                SelectRoomTrack(RoomTrackOptions[index].Key, true);
                return;
            }

            var customIndex = index - RoomTrackOptions.Length;
            SelectRoomTrack(customTracks[customIndex].Key, true);
        }

        private void SelectRandomRoomTrackCategory(TrackCategory category)
        {
            if (category == TrackCategory.CustomTrack)
            {
                var customTracks = GetRoomCustomTrackOptions();
                if (customTracks.Count == 0)
                {
                    SelectRandomRoomTrackAny();
                    return;
                }

                var customIndex = Algorithm.RandomInt(customTracks.Count);
                SelectRoomTrack(customTracks[customIndex].Key, true);
                return;
            }

            var tracks = TrackList.GetTracks(category);
            if (tracks.Count == 0)
            {
                SelectRandomRoomTrackAny();
                return;
            }

            var index = Algorithm.RandomInt(tracks.Count);
            SelectRoomTrack(tracks[index].Key, true);
        }

        private void SelectRoomTrack(string trackKey, bool randomChosen)
        {
            _state.Rooms.RoomOptionsTrackName = string.IsNullOrWhiteSpace(trackKey) ? TrackList.RaceTracks[0].Key : trackKey;
            _state.Rooms.RoomOptionsTrackRandom = randomChosen;
            ReturnToRoomOptionsMenu();
            _speech.Speak(GetRoomOptionsTrackText());
        }

        private void ReturnToRoomOptionsMenu()
        {
            if (string.Equals(_menu.CurrentId, MultiplayerMenuKeys.RoomOptions, StringComparison.Ordinal))
                return;

            while (_menu.CanPop && !string.Equals(_menu.CurrentId, MultiplayerMenuKeys.RoomOptions, StringComparison.Ordinal))
                _menu.PopToPrevious(announceTitle: false);

            if (!string.Equals(_menu.CurrentId, MultiplayerMenuKeys.RoomOptions, StringComparison.Ordinal))
                _menu.Push(MultiplayerMenuKeys.RoomOptions);
        }

        private bool TryGetTrackDisplay(string trackKey, out string display)
        {
            display = string.Empty;
            if (string.IsNullOrWhiteSpace(trackKey))
                return false;

            for (var i = 0; i < RoomTrackOptions.Length; i++)
            {
                if (!string.Equals(RoomTrackOptions[i].Key, trackKey, StringComparison.OrdinalIgnoreCase))
                    continue;

                display = RoomTrackOptions[i].Display;
                return true;
            }

            var customTracks = GetRoomCustomTrackOptions();
            for (var i = 0; i < customTracks.Count; i++)
            {
                if (!string.Equals(customTracks[i].Key, trackKey, StringComparison.OrdinalIgnoreCase))
                    continue;

                display = customTracks[i].Display;
                return true;
            }

            return false;
        }

        private IReadOnlyList<RoomCustomTrackOption> GetRoomCustomTrackOptions()
        {
            var files = Scan.Find("Tracks", "*.tsm");
            if (files.Count == 0)
                return Array.Empty<RoomCustomTrackOption>();

            var options = new List<RoomCustomTrackOption>(files.Count);
            var knownKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < files.Count; i++)
            {
                var file = files[i];
                if (!TrackTsmParser.TryLoadFromFile(file, out var parsed, out _))
                    continue;

                var key = ResolveRoomCustomTrackKey(file);
                if (string.IsNullOrWhiteSpace(key) || key.Length > 12 || !knownKeys.Add(key))
                    continue;

                var display = string.IsNullOrWhiteSpace(parsed.Name)
                    ? ResolveRoomCustomTrackDisplay(file)
                    : parsed.Name!;

                if (string.IsNullOrWhiteSpace(display))
                    display = LocalizationService.Mark("Custom track");

                options.Add(new RoomCustomTrackOption(key, display));
            }

            options.Sort((a, b) => string.Compare(a.Display, b.Display, StringComparison.OrdinalIgnoreCase));
            return options;
        }

        private static string ResolveRoomCustomTrackKey(string file)
        {
            var directory = Path.GetDirectoryName(file);
            if (string.IsNullOrWhiteSpace(directory))
                return string.Empty;

            var folder = Path.GetFileName(directory);
            return string.IsNullOrWhiteSpace(folder) ? string.Empty : folder;
        }

        private static string ResolveRoomCustomTrackDisplay(string file)
        {
            var directory = Path.GetDirectoryName(file);
            if (string.IsNullOrWhiteSpace(directory))
                return Path.GetFileNameWithoutExtension(file);

            var folder = Path.GetFileName(directory);
            return string.IsNullOrWhiteSpace(folder) ? Path.GetFileNameWithoutExtension(file) : folder;
        }
    }
}