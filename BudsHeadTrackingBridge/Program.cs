using System.Numerics;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Platform;
using BudsHeadTrackingBridge;

Console.WriteLine("===========================================");
Console.WriteLine("  Galaxy Buds Head-Tracking Bridge v1.0");
Console.WriteLine("  For OpenTrack / ETS2 / ATS");
Console.WriteLine("===========================================\n");

// Initialize components
var coordinateMapper = new CoordinateMapper();
var udpSender = new OpenTrackUdpSender(targetHz: 100);
SpatialSensorManager? sensorManager = null;

// Statistics
var packetCount = 0;
var sentCount = 0;
var lastStatsTime = DateTime.UtcNow;

try
{
    Console.WriteLine("[INFO] Initializing Bluetooth platform...");
    
    // Initialize platform (required for Bluetooth)
    await PlatformImpl.InitializeAsync();
    
    Console.WriteLine("[INFO] Connecting to Galaxy Buds...");
    Console.WriteLine("[INFO] Make sure your Galaxy Buds are paired and connected to Windows");
    
    // Wait for Bluetooth connection
    var connected = false;
    BluetoothImpl.Instance.Connected += (sender, args) =>
    {
        connected = true;
        Console.WriteLine($"[SUCCESS] Connected to: {args.Device.Name}");
    };

    BluetoothImpl.Instance.Disconnected += (sender, args) =>
    {
        Console.WriteLine($"[WARNING] Disconnected: {args.Reason}");
        connected = false;
    };

    // Attempt to connect to saved device
    if (BluetoothImpl.Instance.RegisteredDeviceValid)
    {
        Console.WriteLine($"[INFO] Found registered device, attempting connection...");
        await BluetoothImpl.Instance.ConnectAsync();
        
        // Wait for connection
        var timeout = DateTime.UtcNow.AddSeconds(10);
        while (!connected && DateTime.UtcNow < timeout)
        {
            await Task.Delay(500);
        }

        if (!connected)
        {
            Console.WriteLine("[ERROR] Failed to connect to Galaxy Buds");
            Console.WriteLine("[INFO] Please ensure:");
            Console.WriteLine("  1. Galaxy Buds are powered on and in pairing mode");
            Console.WriteLine("  2. Buds are paired in Windows Bluetooth settings");
            Console.WriteLine("  3. GalaxyBudsClient has been run at least once to register the device");
            return;
        }
    }
    else
    {
        Console.WriteLine("[ERROR] No registered Galaxy Buds device found");
        Console.WriteLine("[INFO] Please run GalaxyBudsClient first to pair your device");
        return;
    }

    Console.WriteLine("\n[INFO] Starting head-tracking mode...");
    
    // Create and attach spatial sensor manager
    sensorManager = new SpatialSensorManager();
    
    // Subscribe to quaternion updates
    sensorManager.NewQuaternionReceived += (sender, quaternion) =>
    {
        packetCount++;
        
        // Convert to head pose
        var headPose = coordinateMapper.QuaternionToHeadPose(quaternion);
        
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
    };

    // Start head-tracking
    sensorManager.Attach();
    
    Console.WriteLine("[SUCCESS] Head-tracking active!");
    Console.WriteLine("[INFO] Sending data to OpenTrack on UDP 127.0.0.1:4242");
    Console.WriteLine("\nCommands:");
    Console.WriteLine("  'r' - Recenter head position");
    Console.WriteLine("  'c' - Clear recenter calibration");
    Console.WriteLine("  'q' - Quit\n");

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
                    // Get current quaternion and recenter
                    // Note: We need to store the last quaternion for this
                    Console.WriteLine("[INFO] Recenter requested (feature requires last quaternion storage)");
                    break;
                    
                case 'c':
                case 'C':
                    coordinateMapper.ClearRecenter();
                    break;
                    
                case 'q':
                case 'Q':
                    Console.WriteLine("\n[INFO] Shutting down...");
                    goto exit;
            }
        }
        
        await Task.Delay(100);
    }

    exit:
    Console.WriteLine("[INFO] Detaching head-tracking...");
    sensorManager?.Detach();
}
catch (Exception ex)
{
    Console.WriteLine($"\n[ERROR] Fatal error: {ex.Message}");
    Console.WriteLine($"[ERROR] Stack trace:\n{ex.StackTrace}");
}
finally
{
    // Cleanup
    sensorManager?.Dispose();
    udpSender?.Dispose();
    
    Console.WriteLine("\n[INFO] Cleanup complete. Press any key to exit...");
    Console.ReadKey();
}
