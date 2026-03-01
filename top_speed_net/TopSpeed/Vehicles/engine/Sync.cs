using System;

namespace TopSpeed.Vehicles
{
    internal sealed partial class EngineModel
    {
        public void SyncFromSpeed(float speedGameUnits, int gear, float elapsed, int throttleInput = 0)
        {
            var clampedGear = Math.Max(1, Math.Min(_gearCount, gear));
            float targetRpm;

            if (clampedGear == 1)
            {
                var gearMax = _gearMaxSpeedMps[clampedGear - 1] * 3.6f;
                var positionInGear = gearMax <= 0f ? 0f : Math.Min(1f, Math.Max(0f, speedGameUnits / gearMax));
                targetRpm = _idleRpm + ((_revLimiter - _idleRpm) * positionInGear);
            }
            else
            {
                var gearMin = _gearMinSpeedMps[clampedGear - 1] * 3.6f;
                var gearRange = Math.Max(0.1f, (_gearMaxSpeedMps[clampedGear - 1] - _gearMinSpeedMps[clampedGear - 1]) * 3.6f);
                var positionInGear = Math.Min(1f, Math.Max(0f, (speedGameUnits - gearMin) / gearRange));

                var shiftRpm = _idleRpm + ((_revLimiter - _idleRpm) * 0.35f);
                if (positionInGear < 0.07f)
                {
                    var dropProgress = (0.07f - positionInGear) / 0.07f;
                    targetRpm = shiftRpm + ((_revLimiter - shiftRpm) * dropProgress);
                }
                else
                {
                    var riseProgress = (positionInGear - 0.07f) / 0.93f;
                    targetRpm = shiftRpm + ((_revLimiter - shiftRpm) * riseProgress);
                }
            }

            targetRpm = Math.Max(_idleRpm, Math.Min(_maxRpm, targetRpm));
            var throttle = Math.Max(0, throttleInput) / 100f;
            if (throttle > 0.1f)
            {
                var rpmRiseRate = 3000f * throttle;
                if (_rpm < targetRpm)
                    _rpm = Math.Min(targetRpm, _rpm + (rpmRiseRate * elapsed));
                else
                    _rpm = Math.Max(targetRpm, _rpm - (rpmRiseRate * 0.5f * elapsed));
            }
            else
            {
                var decayRate = (throttle <= 0.05f ? 5000f : 3500f) * _engineBraking;
                var riseRate = 1800f * _engineBraking;

                if (_rpm > targetRpm)
                    _rpm = Math.Max(targetRpm, _rpm - (decayRate * elapsed));
                else
                    _rpm = Math.Min(targetRpm, _rpm + (riseRate * elapsed));
            }

            _rpm = Math.Max(_idleRpm, Math.Min(_maxRpm, _rpm));

            var speedMps = speedGameUnits / 3.6f;
            _distanceMeters += speedMps * elapsed;
            _speedMps = speedMps;
        }
    }
}
