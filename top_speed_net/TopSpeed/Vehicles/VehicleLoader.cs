using System;
using System.Collections.Generic;
using System.IO;
using TopSpeed.Core;
using TopSpeed.Data;
using TopSpeed.Protocol;
using TopSpeed.Tracks;

namespace TopSpeed.Vehicles
{
    internal static class VehicleLoader
    {
        private const string BuiltinPrefix = "builtin";
        private const string DefaultVehicleFolder = "default";

        public static VehicleDefinition LoadOfficial(int vehicleIndex, TrackWeather weather)
        {
            if (vehicleIndex < 0 || vehicleIndex >= VehicleCatalog.VehicleCount)
                vehicleIndex = 0;

            var parameters = VehicleCatalog.Vehicles[vehicleIndex];
            var vehiclesRoot = Path.Combine(AssetPaths.SoundsRoot, "Vehicles");
            var currentVehicleFolder = $"Vehicle{vehicleIndex + 1}";

            var def = new VehicleDefinition
            {
                CarType = (CarType)vehicleIndex,
                Name = parameters.Name,
                UserDefined = false,
                SurfaceTractionFactor = parameters.SurfaceTractionFactor,
                Deceleration = parameters.Deceleration,
                TopSpeed = parameters.TopSpeed,
                IdleFreq = parameters.IdleFreq,
                TopFreq = parameters.TopFreq,
                ShiftFreq = parameters.ShiftFreq,
                Gears = parameters.Gears,
                Steering = parameters.Steering,
                HasWipers = parameters.HasWipers == 1 && weather == TrackWeather.Rain ? 1 : 0,
                IdleRpm = parameters.IdleRpm,
                MaxRpm = parameters.MaxRpm,
                RevLimiter = parameters.RevLimiter,
                AutoShiftRpm = parameters.AutoShiftRpm > 0f ? parameters.AutoShiftRpm : parameters.RevLimiter * 0.92f,
                EngineBraking = parameters.EngineBraking,
                MassKg = parameters.MassKg,
                DrivetrainEfficiency = parameters.DrivetrainEfficiency,
                EngineBrakingTorqueNm = parameters.EngineBrakingTorqueNm,
                TireGripCoefficient = parameters.TireGripCoefficient,
                PeakTorqueNm = parameters.PeakTorqueNm,
                PeakTorqueRpm = parameters.PeakTorqueRpm,
                IdleTorqueNm = parameters.IdleTorqueNm,
                RedlineTorqueNm = parameters.RedlineTorqueNm,
                DragCoefficient = parameters.DragCoefficient,
                FrontalAreaM2 = parameters.FrontalAreaM2,
                RollingResistanceCoefficient = parameters.RollingResistanceCoefficient,
                LaunchRpm = parameters.LaunchRpm,
                FinalDriveRatio = parameters.FinalDriveRatio,
                ReverseMaxSpeedKph = parameters.ReverseMaxSpeedKph,
                ReversePowerFactor = parameters.ReversePowerFactor,
                ReverseGearRatio = parameters.ReverseGearRatio,
                TireCircumferenceM = parameters.TireCircumferenceM,
                LateralGripCoefficient = parameters.LateralGripCoefficient,
                HighSpeedStability = parameters.HighSpeedStability,
                WheelbaseM = parameters.WheelbaseM,
                MaxSteerDeg = parameters.MaxSteerDeg,
                WidthM = parameters.WidthM,
                LengthM = parameters.LengthM,
                PowerFactor = parameters.PowerFactor,
                GearRatios = parameters.GearRatios,
                BrakeStrength = parameters.BrakeStrength,
                TransmissionPolicy = parameters.TransmissionPolicy
            };

            foreach (VehicleAction action in Enum.GetValues(typeof(VehicleAction)))
            {
                var overridePath = parameters.GetSoundPath(action);
                if (!string.IsNullOrWhiteSpace(overridePath))
                {
                    def.SetSoundPath(action, Path.Combine(vehiclesRoot, overridePath!));
                }
                else
                {
                    def.SetSoundPath(action, ResolveOfficialFallback(vehiclesRoot, currentVehicleFolder, action));
                }
            }

            return def;
        }

        public static VehicleDefinition LoadCustom(string vehicleFile, TrackWeather weather)
        {
            var filePath = Path.IsPathRooted(vehicleFile)
                ? vehicleFile
                : Path.Combine(AssetPaths.Root, vehicleFile);
            var builtinRoot = Path.Combine(AssetPaths.SoundsRoot, "Vehicles");
            if (!VehicleTsvParser.TryLoadFromFile(filePath, out var parsed, out var issues))
            {
                var message = issues == null || issues.Count == 0
                    ? "Unknown parse error."
                    : string.Join(" ", issues);
                throw new InvalidDataException($"Failed to load custom vehicle '{filePath}'. {message}");
            }

            var hasWipers = weather == TrackWeather.Rain ? parsed.HasWipers : 0;

            var def = new VehicleDefinition
            {
                CarType = CarType.Vehicle1,
                Name = parsed.Meta.Name,
                UserDefined = true,
                CustomFile = Path.GetFileNameWithoutExtension(filePath),
                CustomVersion = parsed.Meta.Version,
                CustomDescription = parsed.Meta.Description,
                SurfaceTractionFactor = parsed.SurfaceTractionFactor,
                Deceleration = parsed.Deceleration,
                TopSpeed = parsed.TopSpeed,
                IdleFreq = parsed.IdleFreq,
                TopFreq = parsed.TopFreq,
                ShiftFreq = parsed.ShiftFreq,
                Gears = parsed.Gears,
                Steering = parsed.Steering,
                HasWipers = hasWipers,
                IdleRpm = parsed.IdleRpm,
                MaxRpm = parsed.MaxRpm,
                RevLimiter = parsed.RevLimiter,
                AutoShiftRpm = parsed.AutoShiftRpm > 0f ? parsed.AutoShiftRpm : parsed.RevLimiter * 0.92f,
                EngineBraking = parsed.EngineBraking,
                MassKg = parsed.MassKg,
                DrivetrainEfficiency = parsed.DrivetrainEfficiency,
                EngineBrakingTorqueNm = parsed.EngineBrakingTorqueNm,
                TireGripCoefficient = parsed.TireGripCoefficient,
                PeakTorqueNm = parsed.PeakTorqueNm,
                PeakTorqueRpm = parsed.PeakTorqueRpm,
                IdleTorqueNm = parsed.IdleTorqueNm,
                RedlineTorqueNm = parsed.RedlineTorqueNm,
                DragCoefficient = parsed.DragCoefficient,
                FrontalAreaM2 = parsed.FrontalAreaM2,
                RollingResistanceCoefficient = parsed.RollingResistanceCoefficient,
                LaunchRpm = parsed.LaunchRpm,
                FinalDriveRatio = parsed.FinalDriveRatio,
                ReverseMaxSpeedKph = parsed.ReverseMaxSpeedKph,
                ReversePowerFactor = parsed.ReversePowerFactor,
                ReverseGearRatio = parsed.ReverseGearRatio,
                TireCircumferenceM = parsed.TireCircumferenceM,
                LateralGripCoefficient = parsed.LateralGripCoefficient,
                HighSpeedStability = parsed.HighSpeedStability,
                WheelbaseM = parsed.WheelbaseM,
                MaxSteerDeg = parsed.MaxSteerDeg,
                WidthM = parsed.WidthM,
                LengthM = parsed.LengthM,
                PowerFactor = parsed.PowerFactor,
                GearRatios = parsed.GearRatios,
                BrakeStrength = parsed.BrakeStrength,
                TransmissionPolicy = parsed.TransmissionPolicy
            };

            def.SetSoundPath(VehicleAction.Engine, ResolveCustomVehicleSound(parsed.Sounds.Engine, builtinRoot, parsed.SourceDirectory, VehicleAction.Engine));
            def.SetSoundPath(VehicleAction.Start, ResolveCustomVehicleSound(parsed.Sounds.Start, builtinRoot, parsed.SourceDirectory, VehicleAction.Start));
            def.SetSoundPath(VehicleAction.Horn, ResolveCustomVehicleSound(parsed.Sounds.Horn, builtinRoot, parsed.SourceDirectory, VehicleAction.Horn));
            if (!string.IsNullOrWhiteSpace(parsed.Sounds.Throttle))
                def.SetSoundPath(VehicleAction.Throttle, ResolveCustomVehicleSound(parsed.Sounds.Throttle!, builtinRoot, parsed.SourceDirectory, VehicleAction.Throttle));
            def.SetSoundPath(VehicleAction.Brake, ResolveCustomVehicleSound(parsed.Sounds.Brake, builtinRoot, parsed.SourceDirectory, VehicleAction.Brake));
            def.SetSoundPaths(VehicleAction.Crash, ResolveCustomVehicleSoundList(parsed.Sounds.CrashVariants, builtinRoot, parsed.SourceDirectory, VehicleAction.Crash));
            if (parsed.Sounds.BackfireVariants != null && parsed.Sounds.BackfireVariants.Count > 0)
                def.SetSoundPaths(VehicleAction.Backfire, ResolveCustomVehicleSoundList(parsed.Sounds.BackfireVariants, builtinRoot, parsed.SourceDirectory, VehicleAction.Backfire));

            return def;
        }

        private static string? ResolveOfficialFallback(string root, string vehicleFolder, VehicleAction action)
        {
            var fileName = GetDefaultFileName(action);
            var primaryPath = Path.GetFullPath(Path.Combine(root, vehicleFolder, fileName));
            if (File.Exists(primaryPath))
                return primaryPath;

            // Only fallback to 'default' folder for non-optional sounds
            // Throttle and Backfire are vehicle-specific features
            if (action == VehicleAction.Backfire || action == VehicleAction.Throttle)
                return null;

            var fallbackPath = Path.GetFullPath(Path.Combine(root, DefaultVehicleFolder, fileName));
            if (File.Exists(fallbackPath))
                return fallbackPath;

            return null;
        }

        private static string GetDefaultFileName(VehicleAction action)
        {
            switch (action)
            {
                case VehicleAction.Engine: return "engine.wav";
                case VehicleAction.Start: return "start.wav";
                case VehicleAction.Horn: return "horn.wav";
                case VehicleAction.Throttle: return "throttle.wav";
                case VehicleAction.Crash: return "crash.wav";
                case VehicleAction.Brake: return "brake.wav";
                case VehicleAction.Backfire: return "backfire.wav";
                default: throw new ArgumentOutOfRangeException(nameof(action));
            }
        }

        private static string[] ResolveCustomVehicleSoundList(
            IReadOnlyList<string> values,
            string builtinRoot,
            string vehicleRoot,
            VehicleAction builtinAction)
        {
            var result = new List<string>();
            for (var i = 0; i < values.Count; i++)
            {
                var resolved = ResolveCustomVehicleSound(values[i], builtinRoot, vehicleRoot, builtinAction);
                if (!string.IsNullOrWhiteSpace(resolved))
                    result.Add(resolved!);
            }

            if (result.Count == 0)
                throw new InvalidDataException($"No valid sound paths resolved for {builtinAction}.");

            return result.ToArray();
        }

        private static string ResolveCustomVehicleSound(
            string value,
            string builtinRoot,
            string vehicleRoot,
            VehicleAction builtinAction)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidDataException($"Missing required sound value for {builtinAction}.");

            var trimmed = value.Trim();
            if (trimmed.StartsWith(BuiltinPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var fromBuiltin = ResolveCustomBuiltinSound(trimmed, builtinRoot, builtinAction);
                if (!string.IsNullOrWhiteSpace(fromBuiltin))
                    return fromBuiltin!;
                throw new InvalidDataException($"Builtin sound reference '{trimmed}' for {builtinAction} could not be resolved.");
            }

            if (Path.IsPathRooted(trimmed))
                throw new InvalidDataException($"Absolute sound paths are not allowed for custom vehicles: {trimmed}");

            var normalized = trimmed
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar)
                .TrimStart(Path.DirectorySeparatorChar);

            if (normalized.IndexOf(':') >= 0 || ContainsTraversal(normalized))
                throw new InvalidDataException($"Invalid custom sound path '{trimmed}'. Paths must stay inside the vehicle folder.");

            var rootFull = Path.GetFullPath(vehicleRoot);
            var candidate = Path.GetFullPath(Path.Combine(rootFull, normalized));
            if (!IsInsideRoot(rootFull, candidate))
                throw new InvalidDataException($"Custom sound path '{trimmed}' escapes the vehicle folder.");
            if (!File.Exists(candidate))
                throw new FileNotFoundException($"Custom vehicle sound file not found: {candidate}", candidate);
            return candidate;
        }

        private static bool ContainsTraversal(string path)
        {
            var parts = path.Split(Path.DirectorySeparatorChar);
            for (var i = 0; i < parts.Length; i++)
            {
                var segment = parts[i].Trim();
                if (segment == "." || segment == "..")
                    return true;
            }
            return false;
        }

        private static bool IsInsideRoot(string rootFull, string candidate)
        {
            if (string.Equals(rootFull, candidate, StringComparison.OrdinalIgnoreCase))
                return true;
            var rootWithSeparator = rootFull.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return candidate.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase);
        }

        private static string? ResolveCustomBuiltinSound(string token, string builtinRoot, VehicleAction action)
        {
            if (!int.TryParse(token.Substring(BuiltinPrefix.Length), out var index))
                return null;
            index -= 1;
            if (index < 0 || index >= VehicleCatalog.VehicleCount)
                return null;

            var vehiclesRoot = builtinRoot;
            var parameters = VehicleCatalog.Vehicles[index];
            var file = parameters.GetSoundPath(action);
            if (!string.IsNullOrWhiteSpace(file))
                return Path.Combine(vehiclesRoot, file!);

            return ResolveOfficialFallback(vehiclesRoot, $"Vehicle{index + 1}", action);
        }

        private static string? ResolveSound(string? value, string builtinRoot, string customVehiclesRoot, Func<VehicleParameters, string?> selector)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (value!.StartsWith(BuiltinPrefix, StringComparison.OrdinalIgnoreCase))
            {
                if (!int.TryParse(value.Substring(BuiltinPrefix.Length), out var index))
                    return null;
                index -= 1;
                if (index < 0 || index >= VehicleCatalog.VehicleCount)
                    return null;
                var parameters = VehicleCatalog.Vehicles[index];
                var file = selector(parameters);
                
                // If it's a builtin reference, we should still handle official fallbacks if the catalog doesn't provide a path
                if (string.IsNullOrWhiteSpace(file))
                {
                    return ResolveOfficialFallback(builtinRoot, $"Vehicle{index + 1}", GetActionFromSelector(selector));
                }

                return Path.Combine(builtinRoot, file!);
            }

            return Path.IsPathRooted(value) ? value : Path.Combine(customVehiclesRoot, value);
        }

        private static VehicleAction GetActionFromSelector(Func<VehicleParameters, string?> selector)
        {
            // Simple hack to detect the action from the selector if needed for builtin resolution
            // In a production environment, we'd pass the action explicitly.
            var testParams = new VehicleParameters(
                "Test", "e", "s", "h", "t", "c", "b", "f",
                0, 0, 0, 0, 0, 0, 0, 0, 0,
                idleRpm: 0, maxRpm: 0, revLimiter: 0, autoShiftRpm: 0, engineBraking: 0,
                massKg: 0, drivetrainEfficiency: 0, engineBrakingTorqueNm: 0, tireGripCoefficient: 0,
                finalDriveRatio: 0, tireCircumferenceM: 0, powerFactor: 0, gearRatios: null, brakeStrength: 0);
            var result = selector(testParams);
            switch (result)
            {
                case "e": return VehicleAction.Engine;
                case "s": return VehicleAction.Start;
                case "h": return VehicleAction.Horn;
                case "t": return VehicleAction.Throttle;
                case "c": return VehicleAction.Crash;
                case "b": return VehicleAction.Brake;
                case "f": return VehicleAction.Backfire;
                default: return VehicleAction.Engine;
            }
        }

        private static Dictionary<string, string> ReadVehicleFile(string filePath)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(filePath))
                return result;

            var section = string.Empty;
            foreach (var line in File.ReadLines(filePath))
            {
                var trimmed = line.Trim();
                if (trimmed.Length == 0)
                    continue;
                if (trimmed.StartsWith(";") || trimmed.StartsWith("#"))
                    continue;
                if (trimmed.StartsWith("[") && trimmed.EndsWith("]") && trimmed.Length > 2)
                {
                    section = trimmed.Substring(1, trimmed.Length - 2).Trim();
                    continue;
                }
                var idx = trimmed.IndexOf('=');
                if (idx <= 0)
                    continue;
                var key = trimmed.Substring(0, idx).Trim();
                var value = trimmed.Substring(idx + 1).Trim();
                result[key] = value;
                if (!string.IsNullOrWhiteSpace(section))
                    result[$"{section}.{key}"] = value;
            }
            return result;
        }

        private static TransmissionPolicy ReadTransmissionPolicy(
            Dictionary<string, string> values,
            int gears,
            float idleRpm,
            float revLimiter,
            float autoShiftRpm)
        {
            var resolvedGears = Math.Max(1, gears);
            var intendedTopSpeedGear = ReadInt(values, "policy.top_speed_gear", resolvedGears);
            var allowOverdrive = ReadBool(values, "policy.allow_overdrive_above_game_top_speed", false);

            var baseCooldown = ReadFloat(values, "policy.base_auto_shift_cooldown", 0.15f);
            var fallbackUpshiftDelay = ReadFloat(values, "policy.upshift_delay_default", baseCooldown);
            var perGearUpshiftDelays = new float[resolvedGears];
            for (var gear = 1; gear <= resolvedGears; gear++)
            {
                perGearUpshiftDelays[gear - 1] = fallbackUpshiftDelay;
                if (gear >= resolvedGears)
                    continue;

                // Preferred explicit transition key: [policy] upshift_delay_5_6 = 0.24
                var transitionKey = $"policy.upshift_delay_{gear}_{gear + 1}";
                var sourceGearKey = $"policy.upshift_delay_g{gear}";
                var overrideDelay = ReadFloat(values, transitionKey, float.NaN);
                if (!float.IsNaN(overrideDelay))
                {
                    perGearUpshiftDelays[gear - 1] = overrideDelay;
                    continue;
                }

                overrideDelay = ReadFloat(values, sourceGearKey, float.NaN);
                if (!float.IsNaN(overrideDelay))
                    perGearUpshiftDelays[gear - 1] = overrideDelay;
            }

            var defaultUpshiftFraction = 0.92f;
            if (revLimiter > idleRpm && autoShiftRpm > 0f)
                defaultUpshiftFraction = Math.Max(0.05f, Math.Min(1.0f, (autoShiftRpm - idleRpm) / (revLimiter - idleRpm)));

            var upshiftRpmFraction = ReadFloat(values, "policy.auto_upshift_rpm_fraction", defaultUpshiftFraction);
            var upshiftRpmAbsolute = ReadFloat(values, "policy.auto_upshift_rpm", 0f);
            if (upshiftRpmAbsolute > 0f && revLimiter > idleRpm)
                upshiftRpmFraction = (upshiftRpmAbsolute - idleRpm) / (revLimiter - idleRpm);

            var downshiftRpmFraction = ReadFloat(values, "policy.auto_downshift_rpm_fraction", 0.35f);
            var downshiftRpmAbsolute = ReadFloat(values, "policy.auto_downshift_rpm", 0f);
            if (downshiftRpmAbsolute > 0f && revLimiter > idleRpm)
                downshiftRpmFraction = (downshiftRpmAbsolute - idleRpm) / (revLimiter - idleRpm);

            var topSpeedPursuitSpeedFraction = ReadFloat(values, "policy.top_speed_pursuit_speed_fraction", 0.97f);
            var upshiftHysteresis = ReadFloat(values, "policy.upshift_hysteresis", 0.05f);
            var minUpshiftNetAccel = ReadFloat(values, "policy.min_upshift_net_accel_mps2", -0.05f);
            var preferIntendedGearNearLimit = ReadBool(values, "policy.prefer_intended_top_speed_gear_near_limit", true);

            return new TransmissionPolicy(
                intendedTopSpeedGear: intendedTopSpeedGear,
                allowOverdriveAboveGameTopSpeed: allowOverdrive,
                upshiftRpmFraction: upshiftRpmFraction,
                downshiftRpmFraction: downshiftRpmFraction,
                upshiftHysteresis: upshiftHysteresis,
                baseAutoShiftCooldownSeconds: baseCooldown,
                minUpshiftNetAccelerationMps2: minUpshiftNetAccel,
                topSpeedPursuitSpeedFraction: topSpeedPursuitSpeedFraction,
                preferIntendedTopSpeedGearNearLimit: preferIntendedGearNearLimit,
                upshiftCooldownBySourceGear: perGearUpshiftDelays);
        }

        private static int ReadInt(Dictionary<string, string> values, string key, int defaultValue)
        {
            if (values.TryGetValue(key, out var raw) && int.TryParse(raw, out var value))
                return value;
            return defaultValue;
        }

        private static string ReadString(Dictionary<string, string> values, string key, string defaultValue)
        {
            if (values.TryGetValue(key, out var raw))
                return raw;
            return defaultValue;
        }

        private static bool ReadBool(Dictionary<string, string> values, string key, bool defaultValue)
        {
            if (!values.TryGetValue(key, out var raw))
                return defaultValue;

            if (bool.TryParse(raw, out var boolValue))
                return boolValue;

            if (int.TryParse(raw, out var intValue))
                return intValue != 0;

            switch (raw.Trim().ToLowerInvariant())
            {
                case "yes":
                case "y":
                case "on":
                case "true":
                    return true;
                case "no":
                case "n":
                case "off":
                case "false":
                    return false;
                default:
                    return defaultValue;
            }
        }

        private static float ReadFloat(Dictionary<string, string> values, string key, float defaultValue)
        {
            if (values.TryGetValue(key, out var raw) && float.TryParse(raw, out var value))
                return value;
            return defaultValue;
        }

        private static float CalculateTireCircumferenceM(int widthMm, int aspectPercent, int rimInches)
        {
            var sidewallMm = widthMm * (aspectPercent / 100f);
            var diameterMm = (rimInches * 25.4f) + (2f * sidewallMm);
            return (float)(Math.PI * (diameterMm / 1000f));
        }

        private static float[]? ReadFloatArray(Dictionary<string, string> values, string key)
        {
            if (!values.TryGetValue(key, out var raw) || string.IsNullOrWhiteSpace(raw))
                return null;

            var parts = raw.Split(',');
            var result = new System.Collections.Generic.List<float>();
            foreach (var part in parts)
            {
                if (float.TryParse(part.Trim(), out var value))
                    result.Add(value);
            }
            return result.Count > 0 ? result.ToArray() : null;
        }
    }
}


