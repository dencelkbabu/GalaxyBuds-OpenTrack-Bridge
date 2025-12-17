using System.Numerics;

namespace BudsHeadTrackingBridge;

/// <summary>
/// Math utilities for quaternion to Euler angle conversion
/// Adapted from GalaxyBudsClient.Utils.Extensions.MathExtensions
/// </summary>
public static class MathExtensions
{
    /// <summary>
    /// Convert quaternion to Euler angles (roll, pitch, yaw) in radians
    /// Uses a more stable conversion that avoids gimbal lock
    /// </summary>
    public static (float roll, float pitch, float yaw) ToRollPitchYaw(this Quaternion q)
    {
        // Normalize quaternion first
        q = Quaternion.Normalize(q);
        
        // Convert to Euler angles using stable formulas
        // Roll (x-axis rotation)
        float sinr_cosp = 2.0f * (q.W * q.X + q.Y * q.Z);
        float cosr_cosp = 1.0f - 2.0f * (q.X * q.X + q.Y * q.Y);
        float roll = (float)Math.Atan2(sinr_cosp, cosr_cosp);

        // Pitch (y-axis rotation)
        float sinp = 2.0f * (q.W * q.Y - q.Z * q.X);
        float pitch;
        if (Math.Abs(sinp) >= 1)
            pitch = (float)Math.CopySign(Math.PI / 2, sinp); // Use 90 degrees if out of range
        else
            pitch = (float)Math.Asin(sinp);

        // Yaw (z-axis rotation)
        float siny_cosp = 2.0f * (q.W * q.Z + q.X * q.Y);
        float cosy_cosp = 1.0f - 2.0f * (q.Y * q.Y + q.Z * q.Z);
        float yaw = (float)Math.Atan2(siny_cosp, cosy_cosp);

        return (roll, pitch, yaw);
    }

    /// <summary>
    /// Convert radians to degrees
    /// </summary>
    public static float ToDegrees(this float radians)
    {
        return radians * (180.0f / (float)Math.PI);
    }
}
