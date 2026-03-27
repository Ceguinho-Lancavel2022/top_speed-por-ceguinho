using TopSpeed.Physics.Powertrain;
using TopSpeed.Physics.Torque;
using Xunit;

namespace TopSpeed.Tests.Physics
{
    [Trait("Category", "SharedPhysics")]
    public sealed class PowertrainBehaviorTests
    {
        [Fact]
        public void DriveRpm_AtStandstill_TracksLaunchTarget()
        {
            var config = BuildConfiguration();
            var rpm = Calculator.DriveRpm(
                config,
                gear: 1,
                speedMps: 0f,
                throttle: 1f,
                inReverse: false);

            Assert.Equal(config.LaunchRpm, rpm, 3);
        }

        [Fact]
        public void DriveRpm_AtExtremeSpeed_ClampsToRevLimiter()
        {
            var config = BuildConfiguration();
            var rpm = Calculator.DriveRpm(
                config,
                gear: 1,
                speedMps: 400f,
                throttle: 1f,
                inReverse: false);

            Assert.Equal(config.RevLimiter, rpm, 3);
        }

        [Fact]
        public void DriveAccel_WithZeroLongitudinalGrip_IsNonPositive()
        {
            var config = BuildConfiguration();
            var accel = Calculator.DriveAccel(
                config,
                gear: 2,
                speedMps: 25f,
                throttle: 0.9f,
                surfaceTractionModifier: 1f,
                longitudinalGripFactor: 0f);

            Assert.True(accel <= 0f);
        }

        [Fact]
        public void ReverseAccel_IsLowerThanForwardAccel_ForSameInput()
        {
            var config = BuildConfiguration();
            var speedMps = 12f;
            var throttle = 0.8f;

            var forward = Calculator.DriveAccel(
                config,
                gear: 1,
                speedMps: speedMps,
                throttle: throttle,
                surfaceTractionModifier: 1f,
                longitudinalGripFactor: 1f);
            var reverse = Calculator.ReverseAccel(
                config,
                speedMps: speedMps,
                throttle: throttle,
                surfaceTractionModifier: 1f,
                longitudinalGripFactor: 1f);

            Assert.True(forward > reverse);
        }

        [Fact]
        public void EngineBrakeDecel_RisesWithHigherEngineRpm()
        {
            var config = BuildConfiguration();

            var lowRpmDecel = Calculator.EngineBrakeDecelKph(
                config,
                gear: 2,
                inReverse: false,
                speedMps: 8f,
                surfaceDecelerationModifier: 1f,
                currentEngineRpm: 1400f);
            var highRpmDecel = Calculator.EngineBrakeDecelKph(
                config,
                gear: 2,
                inReverse: false,
                speedMps: 8f,
                surfaceDecelerationModifier: 1f,
                currentEngineRpm: 5200f);

            Assert.True(highRpmDecel > lowRpmDecel);
        }

        private static Config BuildConfiguration()
        {
            var torqueCurve = CurveFactory.FromLegacy(
                idleRpm: 900f,
                revLimiter: 7600f,
                peakTorqueRpm: 3600f,
                idleTorqueNm: 180f,
                peakTorqueNm: 650f,
                redlineTorqueNm: 360f);

            return new Config(
                massKg: 1650f,
                drivetrainEfficiency: 0.85f,
                engineBrakingTorqueNm: 300f,
                tireGripCoefficient: 1.0f,
                brakeStrength: 1.0f,
                wheelRadiusM: 0.34f,
                engineBraking: 0.3f,
                idleRpm: 900f,
                revLimiter: 7600f,
                finalDriveRatio: 3.70f,
                powerFactor: 0.7f,
                peakTorqueNm: 650f,
                peakTorqueRpm: 3600f,
                idleTorqueNm: 180f,
                redlineTorqueNm: 360f,
                dragCoefficient: 0.30f,
                frontalAreaM2: 2.2f,
                rollingResistanceCoefficient: 0.015f,
                launchRpm: 2400f,
                reversePowerFactor: 0.55f,
                reverseGearRatio: 3.2f,
                engineInertiaKgm2: 0.24f,
                engineFrictionTorqueNm: 20f,
                drivelineCouplingRate: 12f,
                gears: 6,
                gearRatios: new[] { 3.5f, 2.2f, 1.5f, 1.2f, 1.0f, 0.85f },
                torqueCurve: torqueCurve);
        }
    }
}
