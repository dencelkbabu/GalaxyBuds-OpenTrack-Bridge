# Galaxy Buds Hardware Setup Guide

## Prerequisites

### 1. Bluetooth Adapter
✅ **Your Status**: Bluetooth adapter detected and working
- Service: Running
- Device: "Dencel's Buds2 Pro" connected

### 2. Pair Galaxy Buds with Windows
✅ **Your Status**: Already paired

### 3. Register Device with GalaxyBudsClient
⚠️ **Required**: You must run the official GalaxyBudsClient application ONCE to register your Galaxy Buds.

---

## Setup Steps

### Step 1: Download and Run GalaxyBudsClient

1. **Download**: Get the latest release from https://github.com/timschneeb/GalaxyBudsClient/releases
2. **Install**: Run the installer
3. **Launch**: Open GalaxyBudsClient
4. **Connect**: It should automatically detect your paired Galaxy Buds
5. **Verify**: Make sure you see your device name and battery status

### Step 2: Register the Device

Once GalaxyBudsClient connects successfully:
1. The device will be automatically registered in its settings
2. You can now close GalaxyBudsClient
3. The registration persists - you only need to do this once

### Step 3: Run the Bridge

```bash
cd d:\software_dev\budspro-headtracking-port\BudsHeadTrackingBridge
dotnet run
# Select "1" for Real Mode
```

---

## Troubleshooting

### Error: "Bluetooth driver missing or not supported"

**Cause**: Device not registered with GalaxyBudsClient

**Solution**:
1. Download and run the official GalaxyBudsClient app
2. Connect to your Galaxy Buds once
3. Close GalaxyBudsClient
4. Run the bridge again

### Error: "No registered Galaxy Buds device found"

**Cause**: GalaxyBudsClient hasn't saved device configuration

**Solution**:
1. Open GalaxyBudsClient
2. Go to Settings → Devices
3. Make sure your Galaxy Buds are listed
4. Try the bridge again

### Error: "Connection failed"

**Possible Causes**:
- Galaxy Buds are in the case (take them out)
- Galaxy Buds are connected to another device (disconnect)
- Bluetooth is disabled (enable it)
- Galaxy Buds are out of range

---

## Alternative: Use Test Mode

If you can't get the hardware working right now, you can still test the OpenTrack integration:

```bash
dotnet run
# Select "2" for Test Mode
```

This will use mock data to verify the OpenTrack connection works.

---

## Configuration File Location

GalaxyBudsClient stores device configuration in:
```
C:\Users\Dencel\AppData\Roaming\GalaxyBudsClient\
```

The bridge reads from this same location to find your registered devices.

---

## Next Steps

1. **Install GalaxyBudsClient** (if not already installed)
2. **Run it once** to register your Buds2 Pro
3. **Close it**
4. **Run the bridge** in Real Mode
5. **Enjoy head-tracking!**
