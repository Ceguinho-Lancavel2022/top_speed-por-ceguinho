using System;

namespace TopSpeed.Collision
{
    public enum VehicleCollisionContactType : byte
    {
        Unknown = 0,
        RearEnd = 1,
        SideSwipe = 2,
        Mixed = 3
    }

    public readonly struct VehicleCollisionBody
    {
        public VehicleCollisionBody(
            float positionX,
            float positionY,
            float speedKph,
            float widthM,
            float lengthM,
            float massKg)
        {
            PositionX = positionX;
            PositionY = Math.Max(0f, positionY);
            SpeedKph = Math.Max(0f, speedKph);
            WidthM = Math.Max(0.1f, widthM);
            LengthM = Math.Max(0.1f, lengthM);
            MassKg = Math.Max(1f, massKg);
        }

        public float PositionX { get; }
        public float PositionY { get; }
        public float SpeedKph { get; }
        public float WidthM { get; }
        public float LengthM { get; }
        public float MassKg { get; }
    }

    public readonly struct VehicleCollisionImpulse
    {
        public VehicleCollisionImpulse(float bumpX, float bumpY, float speedDeltaKph)
        {
            BumpX = bumpX;
            BumpY = bumpY;
            SpeedDeltaKph = speedDeltaKph;
        }

        public float BumpX { get; }
        public float BumpY { get; }
        public float SpeedDeltaKph { get; }
    }

    public readonly struct VehicleCollisionResponse
    {
        public VehicleCollisionResponse(
            VehicleCollisionImpulse first,
            VehicleCollisionImpulse second,
            VehicleCollisionContactType contactType,
            float impactSeverity,
            float relativeSpeedKph)
        {
            First = first;
            Second = second;
            ContactType = contactType;
            ImpactSeverity = Math.Max(0f, Math.Min(1f, impactSeverity));
            RelativeSpeedKph = Math.Max(0f, relativeSpeedKph);
        }

        public VehicleCollisionImpulse First { get; }
        public VehicleCollisionImpulse Second { get; }
        public VehicleCollisionContactType ContactType { get; }
        public float ImpactSeverity { get; }
        public float RelativeSpeedKph { get; }
    }
}
