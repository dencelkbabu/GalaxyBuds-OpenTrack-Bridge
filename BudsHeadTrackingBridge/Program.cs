using System.Numerics;
using BudsHeadTrackingBridge;

Console.WriteLine("===========================================");
Console.WriteLine("  Galaxy Buds Head-Tracking Bridge v1.0");
Console.WriteLine("  For OpenTrack / ETS2 / ATS");
Console.WriteLine("  [TEST MODE - Mock Data]");
Console.WriteLine("===========================================\n");

// Initialize components
var udpSender = new OpenTrackUdpSender(targetHz: 100);

// Statistics
var sentCount = 0;
var lastStatsTime = DateTime.UtcNow;

Console.WriteLine("[INFO] Test mode: Generating mock head-tracking data");
Console.WriteLine("[INFO] Sending data to OpenTrack on UDP 127.0.0.1:4242");
Console.WriteLine("\nCommands:");
Console.WriteLine("  'r' - Recenter head position");
Console.WriteLine("  'c' - Clear recenter calibration");
Console.WriteLine("  'q' - Quit\n");

// Simulate head movement with a simple pattern
var angle = 0.0f;

try
{
    var cts = new CancellationTokenSource();
    
    // Start mock data generation task
    var dataTask = Task.Run(async () =>
    {
        while (!cts.Token.IsCancellationRequested)
        {
            // Generate mock head pose directly (simple yaw rotation)
            angle += 0.5f; // Degrees per update
            if (angle > 360) angle -= 360;
            
            // Create head pose directly - pure yaw rotation
            var headPose = new HeadPose(
                yaw: angle,      // Rotating yaw from 0 to 360
                pitch: 0.0f,     // No pitch movement
                roll: 0.0f,      // No roll movement
                timestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            );
            
            // Send to OpenTrack (throttled)
            if (udpSender.SendPose(headPose))
            {
                sentCount++;
                
                // Print debug info every 100 packets
                if (sentCount % 100 == 0)
                {
                    var elapsed = (DateTime.UtcNow - lastStatsTime).TotalSeconds;
                    var hz = 100.0 / elapsed;
                    Console.WriteLine($"[DEBUG] {headPose} | Rate: {hz:F1} Hz | Sent: {sentCount}");
                    lastStatsTime = DateTime.UtcNow;
                }
            }
            
            await Task.Delay(10); // ~100 Hz
        }
    }, cts.Token);

    // Main loop - handle user commands
    while (true)
    {
        if (Console.KeyAvailable)
        {
            var key = Console.ReadKey(true).KeyChar;
            
            switch (key)
            {
                case 'r':
                case 'R':
                    Console.WriteLine("[INFO] Recenter not applicable in test mode (no quaternions)");
                    break;
                    
                case 'c':
                case 'C':
                    Console.WriteLine("[INFO] Clear recenter not applicable in test mode");
                    break;
                    
                case 'q':
                case 'Q':
                    Console.WriteLine("\n[INFO] Shutting down...");
                    cts.Cancel();
                    goto exit;
            }
        }
        
        await Task.Delay(100);
    }

    exit:
    await dataTask;
}
catch (Exception ex)
{
    Console.WriteLine($"\n[ERROR] Fatal error: {ex.Message}");
    Console.WriteLine($"[ERROR] Stack trace:\n{ex.StackTrace}");
}
finally
{
    // Cleanup
    udpSender?.Dispose();
    
    Console.WriteLine("\n[INFO] Cleanup complete. Press any key to exit...");
    Console.ReadKey();
}
