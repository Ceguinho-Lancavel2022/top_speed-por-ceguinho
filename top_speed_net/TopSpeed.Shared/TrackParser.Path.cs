using System;
using System.Collections.Generic;
using System.IO;

namespace TopSpeed.Data
{
    public static partial class TrackTsmParser
    {
        public static string? ResolveTrackPath(string nameOrPath)
        {
            if (string.IsNullOrWhiteSpace(nameOrPath))
                return null;
            var trimmed = nameOrPath.Trim();

            if (Directory.Exists(trimmed))
            {
                var fromDirectory = Path.Combine(trimmed, "track.tsm");
                if (IsFolderTrackPath(fromDirectory))
                    return Path.GetFullPath(fromDirectory);
            }

            if (File.Exists(trimmed))
            {
                var asFile = Path.GetFullPath(trimmed);
                return IsFolderTrackPath(asFile) ? asFile : null;
            }

            var tracksRoot = Path.Combine(AppContext.BaseDirectory, "Tracks");
            var candidates = new List<string>
            {
                Path.Combine(tracksRoot, trimmed)
            };

            if (!Path.HasExtension(trimmed))
                candidates.Add(Path.Combine(tracksRoot, trimmed, "track.tsm"));

            foreach (var candidate in candidates)
            {
                var fullPath = Path.GetFullPath(candidate);
                if (IsFolderTrackPath(fullPath))
                    return fullPath;
            }

            return null;
        }

        private static bool IsFolderTrackPath(string path)
        {
            if (!File.Exists(path))
                return false;

            if (!string.Equals(Path.GetFileName(path), "track.tsm", StringComparison.OrdinalIgnoreCase))
                return false;

            var directory = Path.GetDirectoryName(path);
            return !string.IsNullOrWhiteSpace(directory);
        }
    }
}
