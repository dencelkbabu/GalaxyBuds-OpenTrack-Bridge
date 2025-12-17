using System.Numerics;

namespace BudsHeadTrackingBridge;

/// <summary>
/// Converts Galaxy Buds coordinate system to OpenTrack coordinate system
/// and provides optional re-centering functionality
/// </summary>
public class CoordinateMapper
{
    private int _mappingMode = 0;
    private readonly string[] _mappingNames = { 
        "0: Standard (No Swap)", 
        "1: Swap Y<->Z (Common IMU Fix)", 
        "2: Swap X<->Y", 
        "3: Swap X<->Z", 
        "4: Rotate X 90°", 
        "5: Rotate Y 90°",
        "6: FORCE SCALAR SWAP (Yaw<->Roll)"
    };
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

        Quaternion mappedQ = quaternion;

        // Apply Axis Permutations based on Mode
        // Note: For HEAD TRACKING, usually Y (Up) and Z (Forward) are the ones that get confused depending on sensor mount.
        switch (_mappingMode)
        {
            case 0: // Standard
                mappedQ = quaternion;
                break;
            case 1: // Swap Y and Z (The most common fix for "Sideways" sensors)
                mappedQ = new Quaternion(quaternion.X, quaternion.Z, -quaternion.Y, quaternion.W);
                break;
            case 2: // Swap X and Y
                mappedQ = new Quaternion(quaternion.Y, quaternion.X, quaternion.Z, quaternion.W);
                break;
            case 3: // Swap X and Z
                mappedQ = new Quaternion(quaternion.Z, quaternion.Y, quaternion.X, quaternion.W);
                break;
            case 4: // Rotate X 90
                mappedQ = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)(Math.PI / 2)) * quaternion;
                break;
            case 5: // Rotate Y 90
                mappedQ = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)(Math.PI / 2)) * quaternion;
                break;
        }

        // Apply re-centering if calibrated
        Quaternion adjustedQuaternion;
        if (_isCalibrated && _referenceQuaternion.HasValue)
        {
            var invRef = Quaternion.Conjugate(_referenceQuaternion.Value);
            adjustedQuaternion = invRef * mappedQ;
        }
        else
        {
            adjustedQuaternion = mappedQ;
        }

        // Convert to Euler angles (radians)
        var (roll, pitch, yaw) = adjustedQuaternion.ToRollPitchYaw();

        // Convert to degrees
        var yawDeg = yaw.ToDegrees();
        var pitchDeg = pitch.ToDegrees();
        var rollDeg = roll.ToDegrees();

        // Standard Output Mapping
        float mappedYaw, mappedPitch, mappedRoll;

        if (_mappingMode == 6) // DIRECT SCALAR SWAP (Brute Force)
        {
             mappedYaw = rollDeg;
             mappedPitch = -pitchDeg;
             mappedRoll = yawDeg;
        }
        else
        {
             mappedYaw = yawDeg;
             mappedPitch = -pitchDeg;
             mappedRoll = rollDeg;
        }

        // Debug logging (only enabled if console allocated)
         if (System.Environment.CommandLine.Contains("--debug"))
         {
             Console.WriteLine($"[TRACE] Mode: {_mappingNames[_mappingMode]} | YPR: {mappedYaw:F0},{mappedPitch:F0},{mappedRoll:F0}");
         }

        return new HeadPose(
            mappedYaw,
            mappedPitch,
            mappedRoll,
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        );
    }

    public void CycleMapping()
    {
        _mappingMode = (_mappingMode + 1) % _mappingNames.Length;
        Console.WriteLine($"[INFO] Switched Mapping Mode to: {_mappingNames[_mappingMode]}");
        // Force re-calibration on switch to avoid jumping
        _isCalibrated = false;
        _referenceQuaternion = null;
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
