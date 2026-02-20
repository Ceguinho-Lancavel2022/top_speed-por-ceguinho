using System;
using System.Collections.Generic;
using System.Numerics;

namespace TopSpeed.Data
{
    public enum TrackSoundSourceType
    {
        Ambient = 0,
        Static = 1,
        Moving = 2,
        Random = 3
    }

    public enum TrackSoundRandomMode
    {
        OnStart = 0,
        PerArea = 1
    }

    public sealed class TrackSoundSourceDefinition
    {
        private static readonly IReadOnlyList<string> EmptyList = Array.Empty<string>();

        public TrackSoundSourceDefinition(
            string id,
            TrackSoundSourceType type,
            string? path,
            IReadOnlyList<string>? variantPaths,
            IReadOnlyList<string>? variantSourceIds,
            TrackSoundRandomMode randomMode,
            bool loop,
            float volume,
            bool spatial,
            bool allowHrtf,
            float fadeInSeconds,
            float fadeOutSeconds,
            float? crossfadeSeconds,
            float pitch,
            float pan,
            float? minDistance,
            float? maxDistance,
            float? rolloff,
            bool global,
            string? startAreaId,
            string? endAreaId,
            Vector3? startPosition,
            float? startRadiusMeters,
            Vector3? endPosition,
            float? endRadiusMeters,
            Vector3? position,
            float? speedMetersPerSecond)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Sound source id is required.", nameof(id));

            Id = id.Trim();
            Type = type;
            Path = string.IsNullOrWhiteSpace(path) ? null : path?.Trim();
            VariantPaths = variantPaths ?? EmptyList;
            VariantSourceIds = variantSourceIds ?? EmptyList;
            RandomMode = randomMode;
            Loop = loop;
            Volume = Clamp01(volume);
            Spatial = spatial;
            AllowHrtf = allowHrtf;
            FadeInSeconds = Math.Max(0f, fadeInSeconds);
            FadeOutSeconds = Math.Max(0f, fadeOutSeconds);
            CrossfadeSeconds = crossfadeSeconds.HasValue ? Math.Max(0f, crossfadeSeconds.Value) : (float?)null;
            Pitch = pitch <= 0f ? 1.0f : pitch;
            Pan = ClampPan(pan);
            MinDistance = minDistance;
            MaxDistance = maxDistance;
            Rolloff = rolloff;
            Global = global;
            StartAreaId = string.IsNullOrWhiteSpace(startAreaId) ? null : startAreaId?.Trim();
            EndAreaId = string.IsNullOrWhiteSpace(endAreaId) ? null : endAreaId?.Trim();
            StartPosition = startPosition;
            StartRadiusMeters = startRadiusMeters;
            EndPosition = endPosition;
            EndRadiusMeters = endRadiusMeters;
            Position = position;
            SpeedMetersPerSecond = speedMetersPerSecond;
        }

        public string Id { get; }
        public TrackSoundSourceType Type { get; }
        public string? Path { get; }
        public IReadOnlyList<string> VariantPaths { get; }
        public IReadOnlyList<string> VariantSourceIds { get; }
        public TrackSoundRandomMode RandomMode { get; }
        public bool Loop { get; }
        public float Volume { get; }
        public bool Spatial { get; }
        public bool AllowHrtf { get; }
        public float FadeInSeconds { get; }
        public float FadeOutSeconds { get; }
        public float? CrossfadeSeconds { get; }
        public float Pitch { get; }
        public float Pan { get; }
        public float? MinDistance { get; }
        public float? MaxDistance { get; }
        public float? Rolloff { get; }
        public bool Global { get; }
        public string? StartAreaId { get; }
        public string? EndAreaId { get; }
        public Vector3? StartPosition { get; }
        public float? StartRadiusMeters { get; }
        public Vector3? EndPosition { get; }
        public float? EndRadiusMeters { get; }
        public Vector3? Position { get; }
        public float? SpeedMetersPerSecond { get; }

        private static float Clamp01(float value)
        {
            if (value < 0f)
                return 0f;
            if (value > 1f)
                return 1f;
            return value;
        }

        private static float ClampPan(float value)
        {
            if (value < -1f)
                return -1f;
            if (value > 1f)
                return 1f;
            return value;
        }
    }

    public sealed class TrackRoomDefinition
    {
        public TrackRoomDefinition(
            string id,
            string? name,
            float reverbTimeSeconds,
            float reverbGain,
            float hfDecayRatio,
            float lateReverbGain,
            float diffusion,
            float airAbsorption,
            float occlusionScale,
            float transmissionScale,
            float? occlusionOverride = null,
            float? transmissionOverrideLow = null,
            float? transmissionOverrideMid = null,
            float? transmissionOverrideHigh = null,
            float? airAbsorptionOverrideLow = null,
            float? airAbsorptionOverrideMid = null,
            float? airAbsorptionOverrideHigh = null)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Room id is required.", nameof(id));

            Id = id.Trim();
            var trimmedName = name?.Trim();
            Name = string.IsNullOrWhiteSpace(trimmedName) ? null : trimmedName;
            ReverbTimeSeconds = Math.Max(0f, reverbTimeSeconds);
            ReverbGain = Clamp01(reverbGain);
            HfDecayRatio = Clamp01(hfDecayRatio);
            LateReverbGain = Clamp01(lateReverbGain);
            Diffusion = Clamp01(diffusion);
            AirAbsorption = Clamp01(airAbsorption);
            OcclusionScale = Clamp01(occlusionScale);
            TransmissionScale = Clamp01(transmissionScale);
            OcclusionOverride = ClampOptional01(occlusionOverride);
            TransmissionOverrideLow = ClampOptional01(transmissionOverrideLow);
            TransmissionOverrideMid = ClampOptional01(transmissionOverrideMid);
            TransmissionOverrideHigh = ClampOptional01(transmissionOverrideHigh);
            AirAbsorptionOverrideLow = ClampOptional01(airAbsorptionOverrideLow);
            AirAbsorptionOverrideMid = ClampOptional01(airAbsorptionOverrideMid);
            AirAbsorptionOverrideHigh = ClampOptional01(airAbsorptionOverrideHigh);
        }

        public string Id { get; }
        public string? Name { get; }
        public float ReverbTimeSeconds { get; }
        public float ReverbGain { get; }
        public float HfDecayRatio { get; }
        public float LateReverbGain { get; }
        public float Diffusion { get; }
        public float AirAbsorption { get; }
        public float OcclusionScale { get; }
        public float TransmissionScale { get; }
        public float? OcclusionOverride { get; }
        public float? TransmissionOverrideLow { get; }
        public float? TransmissionOverrideMid { get; }
        public float? TransmissionOverrideHigh { get; }
        public float? AirAbsorptionOverrideLow { get; }
        public float? AirAbsorptionOverrideMid { get; }
        public float? AirAbsorptionOverrideHigh { get; }

        private static float Clamp01(float value)
        {
            if (value < 0f)
                return 0f;
            if (value > 1f)
                return 1f;
            return value;
        }

        private static float? ClampOptional01(float? value)
        {
            if (!value.HasValue)
                return null;
            return Clamp01(value.Value);
        }
    }

    public sealed class TrackRoomOverrides
    {
        public float? ReverbTimeSeconds { get; set; }
        public float? ReverbGain { get; set; }
        public float? HfDecayRatio { get; set; }
        public float? LateReverbGain { get; set; }
        public float? Diffusion { get; set; }
        public float? AirAbsorption { get; set; }
        public float? OcclusionScale { get; set; }
        public float? TransmissionScale { get; set; }
        public float? OcclusionOverride { get; set; }
        public float? TransmissionOverrideLow { get; set; }
        public float? TransmissionOverrideMid { get; set; }
        public float? TransmissionOverrideHigh { get; set; }
        public float? AirAbsorptionOverrideLow { get; set; }
        public float? AirAbsorptionOverrideMid { get; set; }
        public float? AirAbsorptionOverrideHigh { get; set; }

        public bool HasAny =>
            ReverbTimeSeconds.HasValue ||
            ReverbGain.HasValue ||
            HfDecayRatio.HasValue ||
            LateReverbGain.HasValue ||
            Diffusion.HasValue ||
            AirAbsorption.HasValue ||
            OcclusionScale.HasValue ||
            TransmissionScale.HasValue ||
            OcclusionOverride.HasValue ||
            TransmissionOverrideLow.HasValue ||
            TransmissionOverrideMid.HasValue ||
            TransmissionOverrideHigh.HasValue ||
            AirAbsorptionOverrideLow.HasValue ||
            AirAbsorptionOverrideMid.HasValue ||
            AirAbsorptionOverrideHigh.HasValue;
    }

    public static class TrackRoomLibrary
    {
        private struct RoomValues
        {
            public float ReverbTimeSeconds;
            public float ReverbGain;
            public float HfDecayRatio;
            public float LateReverbGain;
            public float Diffusion;
            public float AirAbsorption;
            public float OcclusionScale;
            public float TransmissionScale;
        }

        private static readonly Dictionary<string, RoomValues> Presets =
            new Dictionary<string, RoomValues>(StringComparer.OrdinalIgnoreCase)
            {
                ["outdoor_open"] = new RoomValues { ReverbTimeSeconds = 0.4f, ReverbGain = 0.10f, HfDecayRatio = 0.80f, LateReverbGain = 0.10f, Diffusion = 0.20f, AirAbsorption = 0.60f, OcclusionScale = 0.40f, TransmissionScale = 0.70f },
                ["outdoor_urban"] = new RoomValues { ReverbTimeSeconds = 0.8f, ReverbGain = 0.20f, HfDecayRatio = 0.70f, LateReverbGain = 0.20f, Diffusion = 0.40f, AirAbsorption = 0.50f, OcclusionScale = 0.50f, TransmissionScale = 0.50f },
                ["outdoor_forest"] = new RoomValues { ReverbTimeSeconds = 0.6f, ReverbGain = 0.15f, HfDecayRatio = 0.50f, LateReverbGain = 0.15f, Diffusion = 0.30f, AirAbsorption = 0.80f, OcclusionScale = 0.60f, TransmissionScale = 0.60f },
                ["tunnel_short"] = new RoomValues { ReverbTimeSeconds = 1.2f, ReverbGain = 0.50f, HfDecayRatio = 0.60f, LateReverbGain = 0.50f, Diffusion = 0.70f, AirAbsorption = 0.20f, OcclusionScale = 0.80f, TransmissionScale = 0.30f },
                ["tunnel_long"] = new RoomValues { ReverbTimeSeconds = 2.4f, ReverbGain = 0.70f, HfDecayRatio = 0.50f, LateReverbGain = 0.70f, Diffusion = 0.80f, AirAbsorption = 0.20f, OcclusionScale = 0.90f, TransmissionScale = 0.20f },
                ["garage_small"] = new RoomValues { ReverbTimeSeconds = 1.0f, ReverbGain = 0.40f, HfDecayRatio = 0.60f, LateReverbGain = 0.40f, Diffusion = 0.60f, AirAbsorption = 0.30f, OcclusionScale = 0.70f, TransmissionScale = 0.30f },
                ["garage_large"] = new RoomValues { ReverbTimeSeconds = 1.8f, ReverbGain = 0.55f, HfDecayRatio = 0.60f, LateReverbGain = 0.60f, Diffusion = 0.70f, AirAbsorption = 0.30f, OcclusionScale = 0.70f, TransmissionScale = 0.30f },
                ["underpass"] = new RoomValues { ReverbTimeSeconds = 1.4f, ReverbGain = 0.45f, HfDecayRatio = 0.50f, LateReverbGain = 0.50f, Diffusion = 0.60f, AirAbsorption = 0.25f, OcclusionScale = 0.80f, TransmissionScale = 0.30f },
                ["canyon"] = new RoomValues { ReverbTimeSeconds = 2.8f, ReverbGain = 0.60f, HfDecayRatio = 0.40f, LateReverbGain = 0.60f, Diffusion = 0.50f, AirAbsorption = 0.35f, OcclusionScale = 0.60f, TransmissionScale = 0.40f },
                ["stadium_open"] = new RoomValues { ReverbTimeSeconds = 1.5f, ReverbGain = 0.45f, HfDecayRatio = 0.60f, LateReverbGain = 0.50f, Diffusion = 0.70f, AirAbsorption = 0.40f, OcclusionScale = 0.40f, TransmissionScale = 0.60f },
                ["hall_medium"] = new RoomValues { ReverbTimeSeconds = 1.6f, ReverbGain = 0.50f, HfDecayRatio = 0.60f, LateReverbGain = 0.50f, Diffusion = 0.80f, AirAbsorption = 0.30f, OcclusionScale = 0.70f, TransmissionScale = 0.30f },
                ["hall_large"] = new RoomValues { ReverbTimeSeconds = 2.6f, ReverbGain = 0.60f, HfDecayRatio = 0.50f, LateReverbGain = 0.60f, Diffusion = 0.80f, AirAbsorption = 0.25f, OcclusionScale = 0.80f, TransmissionScale = 0.20f },
                ["room_small"] = new RoomValues { ReverbTimeSeconds = 0.7f, ReverbGain = 0.30f, HfDecayRatio = 0.70f, LateReverbGain = 0.30f, Diffusion = 0.60f, AirAbsorption = 0.35f, OcclusionScale = 0.60f, TransmissionScale = 0.40f },
                ["room_medium"] = new RoomValues { ReverbTimeSeconds = 1.1f, ReverbGain = 0.40f, HfDecayRatio = 0.60f, LateReverbGain = 0.40f, Diffusion = 0.70f, AirAbsorption = 0.30f, OcclusionScale = 0.60f, TransmissionScale = 0.30f },
                ["room_large"] = new RoomValues { ReverbTimeSeconds = 1.8f, ReverbGain = 0.50f, HfDecayRatio = 0.50f, LateReverbGain = 0.50f, Diffusion = 0.70f, AirAbsorption = 0.25f, OcclusionScale = 0.70f, TransmissionScale = 0.30f }
            };

        public static bool IsPreset(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;
            return Presets.ContainsKey(name.Trim());
        }

        public static bool TryGetPreset(string name, out TrackRoomDefinition room)
        {
            room = null!;
            if (string.IsNullOrWhiteSpace(name))
                return false;
            if (!Presets.TryGetValue(name.Trim(), out var values))
                return false;

            var id = name.Trim();
            room = new TrackRoomDefinition(
                id,
                id,
                values.ReverbTimeSeconds,
                values.ReverbGain,
                values.HfDecayRatio,
                values.LateReverbGain,
                values.Diffusion,
                values.AirAbsorption,
                values.OcclusionScale,
                values.TransmissionScale);
            return true;
        }
    }
}
