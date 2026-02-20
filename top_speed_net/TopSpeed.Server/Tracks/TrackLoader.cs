using System;
using System.Collections.Generic;
using TopSpeed.Data;

namespace TopSpeed.Server.Tracks
{
    internal static class TrackLoader
    {
        private const float MinPartLength = 50.0f;

        public static TrackData LoadTrack(string nameOrPath, byte defaultLaps)
        {
            if (TrackCatalog.BuiltIn.TryGetValue(nameOrPath, out var builtIn))
            {
                var laps = ResolveLaps(nameOrPath, defaultLaps);
                return new TrackData(builtIn.UserDefined, builtIn.Weather, builtIn.Ambience, builtIn.Definitions, laps);
            }

            var data = ReadCustomTrackData(nameOrPath);
            data.Laps = ResolveLaps(nameOrPath, defaultLaps);
            return data;
        }

        private static byte ResolveLaps(string trackName, byte defaultLaps)
        {
            return trackName.IndexOf("adv", StringComparison.OrdinalIgnoreCase) < 0
                ? defaultLaps
                : (byte)1;
        }

        private static TrackData ReadCustomTrackData(string filename)
        {
            if (!TrackTsmParser.TryLoad(filename, out var parsed, out var issues, MinPartLength))
            {
                LogTrackIssues(filename, issues);
                return CreateFallbackTrack();
            }
            return parsed;
        }

        private static TrackData CreateFallbackTrack()
        {
            var definitions = new[]
            {
                new TrackDefinition(TrackType.Straight, TrackSurface.Asphalt, TrackNoise.NoNoise, MinPartLength)
            };

            return new TrackData(true, TrackWeather.Sunny, TrackAmbience.NoAmbience, definitions);
        }

        private static void LogTrackIssues(string filename, IReadOnlyList<TrackTsmIssue> issues)
        {
            if (issues == null || issues.Count == 0)
            {
                Console.WriteLine($"[TrackLoader] Failed to load '{filename}'.");
                return;
            }

            Console.WriteLine($"[TrackLoader] Failed to load '{filename}':");
            for (var i = 0; i < issues.Count; i++)
                Console.WriteLine($"  - {issues[i]}");
        }
    }
}
