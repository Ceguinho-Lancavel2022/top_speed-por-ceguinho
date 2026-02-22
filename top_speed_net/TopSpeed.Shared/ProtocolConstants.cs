namespace TopSpeed.Protocol
{
    public static class ProtocolConstants
    {
        public const int MaxPlayers = 10;
        public const int MaxMultiTrackLength = 8192;
        public const int MaxMediaFileExtensionLength = 8;
        public const int MaxMediaBytes = 8 * 1024 * 1024;
        public const int MaxMediaChunkBytes = 900;
        public const byte Version = 0x1F;
        public const int DefaultFrequency = 22050;
        public const int MaxPlayerNameLength = 24;
        public const int MaxMotdLength = 128;
        public const int MaxRoomNameLength = 32;
        public const int MaxRoomListEntries = 64;
        public const int MaxProtocolMessageLength = 96;
        public const int MaxRoomPlayersToStart = 10;
        public const string ConnectionKey = "TopSpeedMultiplayer";
    }
}
