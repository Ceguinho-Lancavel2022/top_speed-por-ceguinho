using System;

namespace TopSpeed.Vehicles
{
    internal sealed partial class EngineModel
    {
        public void Reset()
        {
            _rpm = 0f;
            _speedMps = 0f;
            _distanceMeters = 0f;
        }

        public void ResetForCrash()
        {
            _rpm = 0f;
            _speedMps = 0f;
        }

        public void StartEngine()
        {
            _rpm = _idleRpm;
        }

        public void SetSpeed(float speedMps)
        {
            _speedMps = Math.Max(0f, speedMps);
        }
    }
}
