using System;

namespace TopSpeed.Vehicles
{
    /// <summary>
    /// Simulates realistic engine behavior with RPM, throttle response, and engine braking.
    /// </summary>
    internal sealed partial class EngineModel
    {
        private readonly float _idleRpm;
        private readonly float _maxRpm;
        private readonly float _revLimiter;
        private readonly float _autoShiftRpm;
        private readonly float _engineBraking;
        private readonly float _topSpeedKmh;
        private readonly float _finalDriveRatio;
        private readonly float _tireCircumferenceM;
        private readonly int _gearCount;
        private readonly float[] _gearRatios;
        private readonly float[] _gearMaxSpeedMps;
        private readonly float[] _gearAutoShiftSpeedMps;
        private readonly float[] _gearMinSpeedMps;

        private float _rpm;
        private float _distanceMeters;
        private float _speedMps;

        public EngineModel(
            float idleRpm,
            float maxRpm,
            float revLimiter,
            float autoShiftRpm,
            float engineBraking,
            float topSpeedKmh,
            float finalDriveRatio,
            float tireCircumferenceM,
            int gearCount,
            float[]? gearRatios = null)
        {
            _idleRpm = Math.Max(500f, idleRpm);
            _maxRpm = Math.Max(_idleRpm + 1000f, maxRpm);
            _revLimiter = Math.Min(_maxRpm, Math.Max(_idleRpm, revLimiter));
            _autoShiftRpm = autoShiftRpm <= 0f
                ? _revLimiter * 0.92f
                : Math.Max(_idleRpm, Math.Min(_revLimiter, autoShiftRpm));
            _engineBraking = Math.Max(0.05f, Math.Min(1.0f, engineBraking));
            _topSpeedKmh = Math.Max(50f, topSpeedKmh);
            _finalDriveRatio = Math.Max(0.1f, finalDriveRatio);
            _tireCircumferenceM = Math.Max(0.5f, tireCircumferenceM);
            _gearCount = Math.Max(1, gearCount);
            _rpm = 0f;
            _distanceMeters = 0f;
            _speedMps = 0f;

            _gearRatios = gearRatios != null && gearRatios.Length == _gearCount
                ? gearRatios
                : CalculateGearRatios(_gearCount);

            _gearMaxSpeedMps = new float[_gearCount];
            _gearAutoShiftSpeedMps = new float[_gearCount];
            _gearMinSpeedMps = new float[_gearCount];
            var shiftRpm = _idleRpm + ((_revLimiter - _idleRpm) * 0.35f);
            for (var i = 0; i < _gearCount; i++)
            {
                var gearIndex = i + 1;
                _gearMaxSpeedMps[i] = SpeedMpsFromRpm(_revLimiter, gearIndex);
                _gearAutoShiftSpeedMps[i] = SpeedMpsFromRpm(_autoShiftRpm, gearIndex);
                _gearMinSpeedMps[i] = i == 0 ? 0f : SpeedMpsFromRpm(shiftRpm, gearIndex);
            }
        }

        public float Rpm => _rpm;
        public float SpeedKmh => _speedMps * 3.6f;
        public float SpeedMps => _speedMps;
        public float DistanceMeters => _distanceMeters;

        public void OverrideRpm(float rpm)
        {
            var clamped = Math.Max(_idleRpm, Math.Min(_revLimiter, rpm));
            if (clamped > _rpm)
                _rpm = clamped;
        }

        public float GetGearMaxSpeedKmh(int gear)
        {
            var clampedGear = Math.Max(1, Math.Min(_gearCount, gear));
            return _gearMaxSpeedMps[clampedGear - 1] * 3.6f;
        }

        public float GetGearMinSpeedKmh(int gear)
        {
            var clampedGear = Math.Max(1, Math.Min(_gearCount, gear));
            return _gearMinSpeedMps[clampedGear - 1] * 3.6f;
        }

        public float GetGearRangeKmh(int gear)
        {
            var clampedGear = Math.Max(1, Math.Min(_gearCount, gear));
            var range = _gearMaxSpeedMps[clampedGear - 1] - _gearMinSpeedMps[clampedGear - 1];
            return Math.Max(0.1f, range * 3.6f);
        }

        public float GetGearRatio(int gear)
        {
            var clampedGear = Math.Max(1, Math.Min(_gearCount, gear));
            return _gearRatios[clampedGear - 1];
        }

        public int GetGearForSpeedKmh(float speedKmh)
        {
            var speedMps = Math.Max(0f, speedKmh / 3.6f);
            var topSpeedMps = _topSpeedKmh / 3.6f;
            for (var i = 0; i < _gearCount; i++)
            {
                var gearMax = i == _gearCount - 1 ? _gearMaxSpeedMps[i] : _gearAutoShiftSpeedMps[i];
                gearMax = Math.Min(gearMax, topSpeedMps);
                if (speedMps <= gearMax + 0.01f)
                    return i + 1;
            }

            return _gearCount;
        }
    }
}
