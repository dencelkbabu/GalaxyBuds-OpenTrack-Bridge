using System.Numerics;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Platform.Model;
using GalaxyBudsClient.Message;

namespace BudsHeadTrackingBridge;

/// <summary>
/// Simplified Bluetooth manager for Galaxy Buds head-tracking
/// Wraps GalaxyBudsClient's BluetoothImpl for our specific use case
/// </summary>
public class BluetoothHeadTrackingManager : IDisposable
{
    private readonly BluetoothImpl _bluetooth;
    private SpatialSensorManager? _spatialManager;
    
    public event EventHandler<Quaternion>? QuaternionReceived;
    public event EventHandler? Connected;
    public event EventHandler<string>? Disconnected;
    public event EventHandler<string>? Error;
    
    public bool IsConnected => _bluetooth.IsConnected;
    
    public BluetoothHeadTrackingManager()
    {
        _bluetooth = BluetoothImpl.Instance;
        
        // Subscribe to connection events
        _bluetooth.Connected += OnConnected;
        _bluetooth.Disconnected += OnDisconnected;
        _bluetooth.BluetoothError += OnBluetoothError;
    }
    
    public async Task<bool> ConnectAsync()
    {
        try
        {
            Console.WriteLine("[INFO] Connecting to Galaxy Buds...");
            
            // Diagnostic logging
            Console.WriteLine($"[DEBUG] Settings path: {GalaxyBudsClient.Platform.PlatformUtils.CombineDataPath("settings.json")}");
            Console.WriteLine($"[DEBUG] HasValidDevice: {BluetoothImpl.HasValidDevice}");
            Console.WriteLine($"[DEBUG] Device count: {GalaxyBudsClient.Model.Config.Settings.Data.Devices.Count}");
            
            if (GalaxyBudsClient.Model.Config.Settings.Data.Devices.Count > 0)
            {
                var device = GalaxyBudsClient.Model.Config.Settings.Data.Devices[0];
                Console.WriteLine($"[DEBUG] Device found: {device.Name} ({device.MacAddress}) - Model: {device.Model}");
            }
            
            if (!BluetoothImpl.HasValidDevice)
            {
                Error?.Invoke(this, "No registered Galaxy Buds device found. Please pair your buds using GalaxyBudsClient first.");
                return false;
            }
            
            Console.WriteLine("[DEBUG] Calling BluetoothImpl.ConnectAsync()...");
            var result = await _bluetooth.ConnectAsync();
            Console.WriteLine($"[DEBUG] ConnectAsync returned: {result}");
            return result;
        }
        catch (BluetoothException ex)
        {
            Console.WriteLine($"[ERROR] BluetoothException caught:");
            Console.WriteLine($"[ERROR]   ErrorCode: {ex.ErrorCode}");
            Console.WriteLine($"[ERROR]   Message: {ex.Message}");
            Console.WriteLine($"[ERROR]   ErrorMessage: {ex.ErrorMessage}");
            Console.WriteLine($"[ERROR]   StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[ERROR]   InnerException: {ex.InnerException.Message}");
                Console.WriteLine($"[ERROR]   InnerException StackTrace: {ex.InnerException.StackTrace}");
            }
            Error?.Invoke(this, $"Connection failed: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, $"Connection failed: {ex.Message}");
            Console.WriteLine($"[DEBUG] Exception type: {ex.GetType().FullName}");
            Console.WriteLine($"[DEBUG] Exception: {ex}");
            return false;
        }
    }
    
    public async Task DisconnectAsync()
    {
        try
        {
            if (_spatialManager != null)
            {
                _spatialManager.Detach();
                _spatialManager.NewQuaternionReceived -= OnQuaternionReceived;
                _spatialManager.Dispose();
                _spatialManager = null;
            }
            
            await _bluetooth.DisconnectAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARNING] Error during disconnect: {ex.Message}");
        }
    }
    
    public void StartHeadTracking()
    {
        if (!IsConnected)
        {
            Console.WriteLine("[ERROR] Cannot start head-tracking: Not connected to Galaxy Buds");
            return;
        }
        
        try
        {
            Console.WriteLine("[INFO] Starting head-tracking mode...");
            
            // Create spatial sensor manager
            _spatialManager = new SpatialSensorManager();
            _spatialManager.NewQuaternionReceived += OnQuaternionReceived;
            
            // Attach head-tracking mode
            _spatialManager.Attach();
            
            Console.WriteLine("[SUCCESS] Head-tracking active!");
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, $"Failed to start head-tracking: {ex.Message}");
        }
    }
    
    public void StopHeadTracking()
    {
        if (_spatialManager != null)
        {
            Console.WriteLine("[INFO] Stopping head-tracking mode...");
            _spatialManager.Detach();
        }
    }
    
    private void OnConnected(object? sender, EventArgs e)
    {
        var deviceName = _bluetooth.DeviceName;
        Console.WriteLine($"[SUCCESS] Connected to: {deviceName}");
        Connected?.Invoke(this, EventArgs.Empty);
    }
    
    private void OnDisconnected(object? sender, string reason)
    {
        Console.WriteLine($"[INFO] Disconnected: {reason}");
        Disconnected?.Invoke(this, reason);
    }
    
    private void OnBluetoothError(object? sender, BluetoothException ex)
    {
        var message = ex.ErrorMessage ?? ex.Message;
        Console.WriteLine($"[ERROR] Bluetooth error: {message}");
        Error?.Invoke(this, message);
    }
    
    private void OnQuaternionReceived(object? sender, Quaternion quaternion)
    {
        QuaternionReceived?.Invoke(this, quaternion);
    }
    
    public void Dispose()
    {
        _bluetooth.Connected -= OnConnected;
        _bluetooth.Disconnected -= OnDisconnected;
        _bluetooth.BluetoothError -= OnBluetoothError;
        
        _spatialManager?.Dispose();
    }
}
