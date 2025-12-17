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
        // Safety check
        if (float.IsNaN(quaternion.W) || float.IsNaN(quaternion.X) || 
            float.IsNaN(quaternion.Y) || float.IsNaN(quaternion.Z))
        {
            return new HeadPose(0, 0, 0, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }

        // Apply re-centering if calibrated
        Quaternion adjustedQuaternion;
        if (_isCalibrated && _referenceQuaternion.HasValue)
        {
            // Use Conjugate instead of Inverse for performance/stability on normalized quats
            // Diff = Ref* * Cur
            var invRef = Quaternion.Conjugate(_referenceQuaternion.Value);
            adjustedQuaternion = invRef * quaternion;
        }
        else
        {
            adjustedQuaternion = quaternion;
        }

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
        var mappedPitch = -pitchDeg;  // Inverted for OpenTrack
        var mappedRoll = yawDeg;      // Was rollDeg

        // Debug logging (only enabled if console allocated)
        // Console.WriteLine($"[TRACE] Raw: {quaternion} | Adj: {adjustedQuaternion} | YPR: {mappedYaw:F0},{mappedPitch:F0},{mappedRoll:F0}");

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
        // Ensure we capture a valid, normalized quaternion
        if (currentQuaternion.LengthSquared() < 0.001f) return;

        _referenceQuaternion = Quaternion.Normalize(currentQuaternion);
        _isCalibrated = true;
        
        Console.WriteLine($"[INFO] Re-centered. Ref: {_referenceQuaternion}");
        
        // Verify immediate result
        var verify = Quaternion.Conjugate(_referenceQuaternion.Value) * _referenceQuaternion.Value;
        var (r, p, y) = verify.ToRollPitchYaw();
        Console.WriteLine($"[DEBUG] Zero Check: {r},{p},{y} (Should be 0,0,0)");
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
