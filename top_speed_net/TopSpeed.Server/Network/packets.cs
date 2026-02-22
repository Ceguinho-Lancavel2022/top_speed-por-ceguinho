using System.Net;
using TopSpeed.Protocol;
using TopSpeed.Server.Protocol;

namespace TopSpeed.Server.Network
{
    internal sealed partial class RaceServer
    {
        private void OnPacket(IPEndPoint endPoint, byte[] payload)
        {
            if (!PacketSerializer.TryReadHeader(payload, out var header))
            {
                _logger.Warning($"Dropped packet with invalid header from {endPoint}.");
                return;
            }
            if (header.Version != ProtocolConstants.Version)
            {
                _logger.Debug($"Dropped packet with protocol version mismatch from {endPoint}: received={header.Version}, expected={ProtocolConstants.Version}.");
                return;
            }

            lock (_lock)
            {
                var player = GetOrAddPlayer(endPoint);
                if (player == null)
                    return;

                player.LastSeenUtc = DateTime.UtcNow;
                if (_packetHandlers.TryGetValue(header.Command, out var handler))
                    handler(player, payload, endPoint);
                else
                    _logger.Warning($"Ignoring unknown packet command {(byte)header.Command} from {endPoint}.");
            }
        }

        private void RegisterPackets()
        {
            _packetHandlers[Command.KeepAlive] = (_, _, _) => { };
            _packetHandlers[Command.PlayerHello] = (player, payload, endPoint) =>
            {
                if (PacketSerializer.TryReadPlayerHello(payload, out var hello))
                    HandlePlayerHello(player, hello);
                else
                    PacketFail(endPoint, Command.PlayerHello);
            };
            _packetHandlers[Command.PlayerState] = (player, payload, endPoint) =>
            {
                if (PacketSerializer.TryReadPlayerState(payload, out var state))
                    HandlePlayerState(player, state);
                else
                    PacketFail(endPoint, Command.PlayerState);
            };
            _packetHandlers[Command.PlayerDataToServer] = (player, payload, endPoint) =>
            {
                if (PacketSerializer.TryReadPlayerData(payload, out var data))
                    HandlePlayerData(player, data);
                else
                    PacketFail(endPoint, Command.PlayerDataToServer);
            };
            _packetHandlers[Command.PlayerStarted] = (player, payload, endPoint) =>
            {
                if (PacketSerializer.TryReadPlayer(payload, out _))
                    HandlePlayerStarted(player);
                else
                    PacketFail(endPoint, Command.PlayerStarted);
            };
            _packetHandlers[Command.PlayerFinished] = (player, payload, endPoint) =>
            {
                if (PacketSerializer.TryReadPlayer(payload, out var finished))
                    HandlePlayerFinished(player, finished);
                else
                    PacketFail(endPoint, Command.PlayerFinished);
            };
            _packetHandlers[Command.PlayerCrashed] = (player, payload, endPoint) =>
            {
                if (PacketSerializer.TryReadPlayer(payload, out var crashed))
                    HandlePlayerCrashed(player, crashed);
                else
                    PacketFail(endPoint, Command.PlayerCrashed);
            };
            _packetHandlers[Command.PlayerMediaBegin] = (player, payload, endPoint) =>
            {
                if (PacketSerializer.TryReadPlayerMediaBegin(payload, out var begin))
                    OnMediaBegin(player, begin);
                else
                    PacketFail(endPoint, Command.PlayerMediaBegin);
            };
            _packetHandlers[Command.PlayerMediaChunk] = (player, payload, endPoint) =>
            {
                if (PacketSerializer.TryReadPlayerMediaChunk(payload, out var chunk))
                    OnMediaChunk(player, chunk);
                else
                    PacketFail(endPoint, Command.PlayerMediaChunk);
            };
            _packetHandlers[Command.PlayerMediaEnd] = (player, payload, endPoint) =>
            {
                if (PacketSerializer.TryReadPlayerMediaEnd(payload, out var end))
                    OnMediaEnd(player, end);
                else
                    PacketFail(endPoint, Command.PlayerMediaEnd);
            };
            _packetHandlers[Command.RoomListRequest] = (player, _, _) => SendRoomList(player);
            _packetHandlers[Command.RoomCreate] = (player, payload, endPoint) =>
            {
                if (PacketSerializer.TryReadRoomCreate(payload, out var create))
                    HandleCreateRoom(player, create);
                else
                    PacketFail(endPoint, Command.RoomCreate);
            };
            _packetHandlers[Command.RoomJoin] = (player, payload, endPoint) =>
            {
                if (PacketSerializer.TryReadRoomJoin(payload, out var join))
                    HandleJoinRoom(player, join);
                else
                    PacketFail(endPoint, Command.RoomJoin);
            };
            _packetHandlers[Command.RoomLeave] = (player, _, _) => HandleLeaveRoom(player, true);
            _packetHandlers[Command.RoomSetTrack] = (player, payload, endPoint) =>
            {
                if (PacketSerializer.TryReadRoomSetTrack(payload, out var track))
                    HandleSetTrack(player, track);
                else
                    PacketFail(endPoint, Command.RoomSetTrack);
            };
            _packetHandlers[Command.RoomSetLaps] = (player, payload, endPoint) =>
            {
                if (PacketSerializer.TryReadRoomSetLaps(payload, out var laps))
                    HandleSetLaps(player, laps);
                else
                    PacketFail(endPoint, Command.RoomSetLaps);
            };
            _packetHandlers[Command.RoomStartRace] = (player, _, _) => HandleStartRace(player);
            _packetHandlers[Command.RoomSetPlayersToStart] = (player, payload, endPoint) =>
            {
                if (PacketSerializer.TryReadRoomSetPlayersToStart(payload, out var setPlayers))
                    HandleSetPlayersToStart(player, setPlayers);
                else
                    PacketFail(endPoint, Command.RoomSetPlayersToStart);
            };
            _packetHandlers[Command.RoomAddBot] = (player, _, _) => HandleAddBot(player);
            _packetHandlers[Command.RoomRemoveBot] = (player, _, _) => HandleRemoveBot(player);
            _packetHandlers[Command.RoomPlayerReady] = (player, payload, endPoint) =>
            {
                if (PacketSerializer.TryReadRoomPlayerReady(payload, out var ready))
                    HandlePlayerReady(player, ready);
                else
                    PacketFail(endPoint, Command.RoomPlayerReady);
            };
        }

        private void PacketFail(IPEndPoint endPoint, Command command)
        {
            _logger.Warning($"Failed to parse {command} packet from {endPoint}.");
        }

        private PlayerConnection? GetOrAddPlayer(IPEndPoint endpoint)
        {
            var key = endpoint.ToString();
            if (_endpointIndex.TryGetValue(key, out var id) && _players.TryGetValue(id, out var existing))
                return existing;

            if (_players.Count >= _config.MaxPlayers)
            {
                _transport.Send(endpoint, PacketSerializer.WriteGeneral(Command.Disconnect));
                _logger.Warning($"Refused connection from {endpoint}: server is full.");
                return null;
            }

            var playerId = _nextPlayerId++;
            var player = new PlayerConnection(endpoint, playerId);
            _players[playerId] = player;
            _endpointIndex[key] = playerId;

            _transport.Send(endpoint, PacketSerializer.WritePlayerNumber(playerId, 0));
            if (!string.IsNullOrWhiteSpace(_config.Motd))
                _transport.Send(endpoint, PacketSerializer.WriteServerInfo(new PacketServerInfo { Motd = _config.Motd }));

            SendRoomState(player, null);
            SendRoomList(player);
            _logger.Info($"Connection established: playerId={player.Id}, endpoint={endpoint}.");
            return player;
        }
    }
}
