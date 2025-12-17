using System.Numerics;

namespace BudsHeadTrackingBridge;

/// <summary>
/// Converts Galaxy Buds coordinate system to OpenTrack coordinate system
/// and provides optional re-centering functionality
/// </summary>
public class CoordinateMapper
{
    private Quaternion? _referenceQuaternion;
    private bool _isCalibrated;

    /// <summary>
    /// Convert quaternion from Galaxy Buds to HeadPose for OpenTrack
    /// </summary>
    public HeadPose QuaternionToHeadPose(Quaternion quaternion)
    {
        // Apply re-centering if calibrated
        var adjustedQuaternion = _isCalibrated && _referenceQuaternion.HasValue
            ? Quaternion.Inverse(_referenceQuaternion.Value) * quaternion
            : quaternion;

        // Convert to Euler angles (radians)
        var (roll, pitch, yaw) = adjustedQuaternion.ToRollPitchYaw();

        // Convert to degrees
        var yawDeg = yaw.ToDegrees();
        var pitchDeg = pitch.ToDegrees();
        var rollDeg = roll.ToDegrees();

        // Apply coordinate system mapping
        // Galaxy Buds → OpenTrack coordinate transformation
        // User report: Left/Right (Yaw) movement changes Roll value.
        // This implies the sensor axis for Roll corresponds to physical Yaw.
        // We swap them here to correct it.
        
        var mappedYaw = rollDeg;      // Was yawDeg
        var mappedPitch = -pitchDeg;  // Inverted for OpenTrack (kept same)
        var mappedRoll = yawDeg;      // Was rollDeg

        return new HeadPose(
            mappedYaw,
            mappedPitch,
            mappedRoll,
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        );
    }

    /// <summary>
    /// Set the current orientation as the reference (center) position
    /// </summary>
    public void Recenter(Quaternion currentQuaternion)
    {
        _referenceQuaternion = currentQuaternion;
        _isCalibrated = true;
        Console.WriteLine("[INFO] Re-centered head tracking");
    }

    /// <summary>
    /// Clear the reference position
    /// </summary>
    public void ClearRecenter()
    {
        _referenceQuaternion = null;
        _isCalibrated = false;
        Console.WriteLine("[INFO] Cleared re-center calibration");
    }
}
