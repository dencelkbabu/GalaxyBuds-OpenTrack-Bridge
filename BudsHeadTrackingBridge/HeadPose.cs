using System.Numerics;

namespace BudsHeadTrackingBridge;

/// <summary>
/// Represents the head orientation in 3D space
/// </summary>
public struct HeadPose
{
    public float Yaw { get; set; }      // Rotation around Y axis (degrees)
    public float Pitch { get; set; }    // Rotation around X axis (degrees)
    public float Roll { get; set; }     // Rotation around Z axis (degrees)
    public long Timestamp { get; set; } // Unix timestamp in milliseconds

    public HeadPose(float yaw, float pitch, float roll, long timestamp)
    {
        Yaw = yaw;
        Pitch = pitch;
        Roll = roll;
        Timestamp = timestamp;
    }

    public override string ToString()
    {
        return $"Yaw={Yaw:F2}° Pitch={Pitch:F2}° Roll={Roll:F2}°";
    }
}
