using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;

namespace BudsHeadTrackingBridge;

/// <summary>
/// Sends head pose data to OpenTrack via UDP
/// Implements throttling to maintain ~100-120 Hz update rate
/// </summary>
public class OpenTrackUdpSender : IDisposable
{
    private readonly UdpClient _udpClient;
    private readonly IPEndPoint _endpoint;
    private readonly Stopwatch _throttleTimer;
    private readonly int _minIntervalMs;

    public OpenTrackUdpSender(string host = "127.0.0.1", int port = 4242, int targetHz = 100)
    {
        _udpClient = new UdpClient();
        _endpoint = new IPEndPoint(IPAddress.Parse(host), port);
        _throttleTimer = Stopwatch.StartNew();
        _minIntervalMs = 1000 / targetHz; // e.g., 10ms for 100Hz

        Console.WriteLine($"[INFO] OpenTrack UDP sender initialized: {host}:{port} @ {targetHz}Hz");
    }

    /// <summary>
    /// Send head pose to OpenTrack (throttled)
    /// </summary>
    public bool SendPose(HeadPose pose)
    {
        // Throttle to prevent flooding
        if (_throttleTimer.ElapsedMilliseconds < _minIntervalMs)
        {
            return false; // Skipped due to throttling
        }

        try
        {
            // OpenTrack UDP protocol: 6 IEEE double-precision floats (little-endian)
            // Order: X, Y, Z, Yaw, Pitch, Roll
            // Total: 6 * 8 bytes = 48 bytes per packet
            
            var data = new byte[48]; // 6 doubles * 8 bytes each
            var offset = 0;
            
            // Position (X, Y, Z) - we send 0 for head-tracking only
            Buffer.BlockCopy(BitConverter.GetBytes((double)0.0), 0, data, offset, 8); offset += 8; // X
            Buffer.BlockCopy(BitConverter.GetBytes((double)0.0), 0, data, offset, 8); offset += 8; // Y
            Buffer.BlockCopy(BitConverter.GetBytes((double)0.0), 0, data, offset, 8); offset += 8; // Z
            
            // Rotation (Yaw, Pitch, Roll) - in degrees
            Buffer.BlockCopy(BitConverter.GetBytes((double)pose.Yaw), 0, data, offset, 8); offset += 8;
            Buffer.BlockCopy(BitConverter.GetBytes((double)pose.Pitch), 0, data, offset, 8); offset += 8;
            Buffer.BlockCopy(BitConverter.GetBytes((double)pose.Roll), 0, data, offset, 8);

            _udpClient.Send(data, data.Length, _endpoint);

            _throttleTimer.Restart();
            return true; // Successfully sent
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to send UDP: {ex.Message}");
            return false;
        }
    }

    public void Dispose()
    {
        _udpClient?.Dispose();
        Console.WriteLine("[INFO] OpenTrack UDP sender disposed");
    }
}
