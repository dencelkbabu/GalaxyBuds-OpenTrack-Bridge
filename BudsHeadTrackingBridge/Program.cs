using System.Numerics;
using BudsHeadTrackingBridge;

Console.WriteLine("===========================================");
Console.WriteLine("  Galaxy Buds Head-Tracking Bridge v1.0");
Console.WriteLine("  For OpenTrack / ETS2 / ATS");
Console.WriteLine("===========================================\n");

// Check if user wants test mode or real mode
Console.WriteLine("Select mode:");
Console.WriteLine("  1 - Real Galaxy Buds (requires paired hardware)");
Console.WriteLine("  2 - Test Mode (mock data)");
Console.Write("\nEnter choice (1 or 2): ");

var choice = Console.ReadLine();
var useRealBuds = choice == "1";

if (useRealBuds)
{
    await RunRealModeAsync();
}
else
{
    await RunTestModeAsync();
}

async Task RunRealModeAsync()
{
    Console.WriteLine("\n[INFO] Starting in REAL MODE with Galaxy Buds\n");
    
    // CRITICAL: Initialize Windows Bluetooth backend
    // Without this, PlatformImpl uses DummyPlatformImplCreator which doesn't support Bluetooth
    try
    {
        Console.WriteLine("[INFO] Initializing Windows Bluetooth backend...");
        GalaxyBudsClient.Platform.PlatformImpl.SwitchWindowsBackend();
        Console.WriteLine("[SUCCESS] Bluetooth backend initialized");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] Failed to initialize Bluetooth backend: {ex.Message}");
        Console.WriteLine("[INFO] Falling back to test mode...");
        await RunTestModeAsync();
        return;
    }
    
    // Initialize components
    var coordinateMapper = new CoordinateMapper();
    var udpSender = new OpenTrackUdpSender(targetHz: 100);
    var bluetoothManager = new BluetoothHeadTrackingManager();
    
    // Statistics
    var sentCount = 0;
    var lastStatsTime = DateTime.UtcNow;
    var lastQuaternion = Quaternion.Identity;
    
    try
    {
        // Connect to Galaxy Buds
        var connected = await bluetoothManager.ConnectAsync();
        if (!connected)
        {
            Console.WriteLine("\n[ERROR] Failed to connect to Galaxy Buds");
            Console.WriteLine("[INFO] Make sure your Galaxy Buds are:");
            Console.WriteLine("  1. Paired with Windows via Bluetooth");
            Console.WriteLine("  2. Registered in GalaxyBudsClient (run it once to pair)");
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
            return;
        }
        
        // Start head-tracking
        bluetoothManager.StartHeadTracking();
        
        // Subscribe to quaternion events
        bluetoothManager.QuaternionReceived += (sender, quaternion) =>
        {
            lastQuaternion = quaternion;
            
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
                        coordinateMapper.Recenter(lastQuaternion);
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
        bluetoothManager.StopHeadTracking();
        await bluetoothManager.DisconnectAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n[ERROR] Fatal error: {ex.Message}");
        Console.WriteLine($"[ERROR] Stack trace:\n{ex.StackTrace}");
    }
    finally
    {
        // Cleanup
        bluetoothManager?.Dispose();
        udpSender?.Dispose();
        
        Console.WriteLine("\n[INFO] Cleanup complete. Press any key to exit...");
        Console.ReadKey();
    }
}

async Task RunTestModeAsync()
{
    Console.WriteLine("\n[INFO] Starting in TEST MODE with mock data\n");
    
    // Initialize components
    var udpSender = new OpenTrackUdpSender(targetHz: 100);
    
    // Statistics
    var sentCount = 0;
    var lastStatsTime = DateTime.UtcNow;
    
    Console.WriteLine("[INFO] Test mode: Generating mock head-tracking data");
    Console.WriteLine("[INFO] Sending data to OpenTrack on UDP 127.0.0.1:4242");
    Console.WriteLine("\nCommands:");
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
                
                if (key == 'q' || key == 'Q')
                {
                    Console.WriteLine("\n[INFO] Shutting down...");
                    cts.Cancel();
                    break;
                }
            }
            
            await Task.Delay(100);
        }

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
}
