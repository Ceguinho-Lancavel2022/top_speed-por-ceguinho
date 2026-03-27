using System.Collections.Generic;

namespace TopSpeed.Vehicles.Parsing
{
    internal static partial class VehicleTsvParser
    {
        private static void ParseEngineValues(Section section, ParsedValues values, List<VehicleTsvIssue> issues)
        {
            values.IdleRpm = RequireFloatRange(section, "idle_rpm", 300f, 3000f, issues);
            values.MaxRpm = RequireFloatRange(section, "max_rpm", 1000f, 20000f, issues);
            values.RevLimiter = RequireFloatRange(section, "rev_limiter", 800f, 18000f, issues);
            values.AutoShiftRpm = RequireFloatRange(section, "auto_shift_rpm", 0f, 18000f, issues);
            values.EngineBraking = RequireFloatRange(section, "engine_braking", 0f, 1.5f, issues);
            values.MassKg = RequireFloatRange(section, "mass_kg", 20f, 10000f, issues);
            values.DrivetrainEfficiency = RequireFloatRange(section, "drivetrain_efficiency", 0.1f, 1.0f, issues);
            values.DragCoefficient = RequireFloatRange(section, "drag_coefficient", 0.01f, 1.5f, issues);
            values.FrontalArea = RequireFloatRange(section, "frontal_area", 0.05f, 10f, issues);
            values.RollingResistance = RequireFloatRange(section, "rolling_resistance", 0.001f, 0.1f, issues);
            values.LaunchRpm = RequireFloatRange(section, "launch_rpm", 0f, 18000f, issues);
        }

        private static void ParseTorqueValues(Section section, ParsedValues values, List<VehicleTsvIssue> issues)
        {
            values.EngineBrakingTorque = RequireFloatRange(section, "engine_braking_torque", 0f, 3000f, issues);
            values.PeakTorque = RequireFloatRange(section, "peak_torque", 10f, 3000f, issues);
            values.PeakTorqueRpm = RequireFloatRange(section, "peak_torque_rpm", 500f, 18000f, issues);
            values.IdleTorque = RequireFloatRange(section, "idle_torque", 0f, 3000f, issues);
            values.RedlineTorque = RequireFloatRange(section, "redline_torque", 0f, 3000f, issues);
            values.PowerFactor = RequireFloatRange(section, "power_factor", 0.05f, 2f, issues);
            values.EngineInertiaKgm2 = RequireFloatRange(section, "engine_inertia_kgm2", 0.01f, 5f, issues);
            values.EngineFrictionTorqueNm = RequireFloatRange(section, "engine_friction_torque_nm", 0f, 1000f, issues);
            values.DrivelineCouplingRate = RequireFloatRange(section, "driveline_coupling_rate", 0.1f, 80f, issues);
        }

        private static void ParseDrivetrainValues(Section section, ParsedValues values, List<VehicleTsvIssue> issues)
        {
            values.FinalDrive = RequireFloatRange(section, "final_drive", 0.5f, 8f, issues);
            values.ReverseMaxSpeed = RequireFloatRange(section, "reverse_max_speed", 1f, 100f, issues);
            values.ReversePowerFactor = RequireFloatRange(section, "reverse_power_factor", 0.05f, 2f, issues);
            values.ReverseGearRatio = RequireFloatRange(section, "reverse_gear_ratio", 0.5f, 8f, issues);
            values.BrakeStrength = RequireFloatRange(section, "brake_strength", 0.1f, 5f, issues);
        }
    }
}
