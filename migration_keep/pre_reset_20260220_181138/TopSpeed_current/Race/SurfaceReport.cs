using System;
using System.Collections.Generic;
using System.Numerics;

namespace TopSpeed.Race
{
    internal static class SurfaceReport
    {
        private const float LevelThresholdDegrees = 0.5f;

        public static string? FormatSlopeBank(Vector3 surfaceUp, Vector3 headingForward)
        {
            var slope = ComputeSlopeDegrees(surfaceUp, headingForward);
            var bank = ComputeBankDegrees(surfaceUp, headingForward);

            List<string>? parts = null;
            if (Math.Abs(slope) >= LevelThresholdDegrees)
            {
                parts ??= new List<string>(2);
                parts.Add(DescribeSlope(slope));
            }
            if (Math.Abs(bank) >= LevelThresholdDegrees)
            {
                parts ??= new List<string>(2);
                parts.Add(DescribeBank(bank));
            }

            if (parts == null || parts.Count == 0)
                return null;

            return string.Join(", ", parts);
        }

        private static float ComputeSlopeDegrees(Vector3 surfaceUp, Vector3 headingForward)
        {
            var forward = headingForward;
            forward.Y = 0f;
            if (forward.LengthSquared() <= 0.000001f)
                return 0f;

            forward = Vector3.Normalize(forward);
            var projected = forward - (surfaceUp * Vector3.Dot(forward, surfaceUp));
            if (projected.LengthSquared() <= 0.000001f)
                return 0f;

            projected = Vector3.Normalize(projected);
            var angle = Math.Atan2(projected.Y, Vector3.Dot(projected, forward));
            return (float)(angle * 180f / Math.PI);
        }

        private static float ComputeBankDegrees(Vector3 surfaceUp, Vector3 headingForward)
        {
            var forward = headingForward;
            forward.Y = 0f;
            if (forward.LengthSquared() <= 0.000001f)
                return 0f;

            forward = Vector3.Normalize(forward);
            var right = Vector3.Cross(forward, Vector3.UnitY);
            if (right.LengthSquared() <= 0.000001f)
                return 0f;

            right = Vector3.Normalize(right);
            var projected = right - (surfaceUp * Vector3.Dot(right, surfaceUp));
            if (projected.LengthSquared() <= 0.000001f)
                return 0f;

            projected = Vector3.Normalize(projected);
            var angle = Math.Atan2(projected.Y, Vector3.Dot(projected, right));
            return (float)(angle * 180f / Math.PI);
        }

        private static string DescribeSlope(float degrees)
        {
            var abs = Math.Abs(degrees);
            return degrees > 0f
                ? $"slope up {abs:F1} degrees"
                : $"slope down {abs:F1} degrees";
        }

        private static string DescribeBank(float degrees)
        {
            var abs = Math.Abs(degrees);
            return degrees > 0f
                ? $"bank right {abs:F1} degrees"
                : $"bank left {abs:F1} degrees";
        }
    }
}
