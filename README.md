# Galaxy Buds Head-Tracking Bridge for OpenTrack

A lightweight C# bridge that extracts real-time head-tracking data from Samsung Galaxy Buds Pro and forwards it to OpenTrack for use in ETS2, ATS, and other simulation games.

## Features

- ✅ Real-time quaternion data extraction from Galaxy Buds Pro
- ✅ Automatic conversion to Euler angles (Yaw/Pitch/Roll)
- ✅ UDP streaming to OpenTrack at ~100-120 Hz
- ✅ Coordinate system mapping (Buds → OpenTrack)
- ✅ Optional re-centering support
- ✅ Low latency (<50ms)

## Requirements

### Hardware
- **Galaxy Buds Pro**, **Buds2 Pro**, or **Buds3 Pro** (with head-tracking support)
- Windows 11 PC with Bluetooth

### Software
- **.NET 10.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/10.0)
- **OpenTrack** - [Download](https://github.com/opentrack/opentrack/releases)
- **GalaxyBudsClient** (for initial device pairing) - [Download](https://github.com/timschneeb/GalaxyBudsClient/releases)

## Setup

### 1. Pair Galaxy Buds
1. Download and install [GalaxyBudsClient](https://github.com/timschneeb/GalaxyBudsClient/releases)
2. Pair your Galaxy Buds Pro with Windows via Bluetooth
3. Open GalaxyBudsClient and connect to your buds (this registers the device)
4. Verify audio is working

### 2. Build the Bridge
```bash
cd d:\software_dev\budspro-headtracking-port
dotnet build BudsHeadTrackingBridge/BudsHeadTrackingBridge.csproj
```

### 3. Configure OpenTrack
1. Install and launch OpenTrack
2. Set **Input** to: `UDP over network`
3. Set **Port** to: `4242`
4. Set **Output** to: `freetrack 2.0 Enhanced` (for ETS2/ATS)
5. Click **Start**

### 4. Run the Bridge
```bash
cd BudsHeadTrackingBridge
dotnet run
```

Expected output:
```
===========================================
  Galaxy Buds Head-Tracking Bridge v1.0
  For OpenTrack / ETS2 / ATS
===========================================

[INFO] Connecting to Galaxy Buds...
[SUCCESS] Connected to: Galaxy Buds Pro
[INFO] Starting head-tracking mode...
[SUCCESS] Head-tracking active!
[INFO] Sending data to OpenTrack on UDP 127.0.0.1:4242
```

### 5. Test in Game
1. Launch Euro Truck Simulator 2 or American Truck Simulator
2. Enable **TrackIR** or **Head Tracking** in game settings
3. Start driving and move your head
4. Camera should follow your head movement

## Usage

### Commands (while running)
- `r` - Recenter head position
- `c` - Clear recenter calibration
- `q` - Quit

### Performance Monitoring
The bridge displays statistics every 100 packets:
```
[DEBUG] Yaw=45.20° Pitch=-12.30° Roll=5.10° | Rate: 112.3 Hz | Sent: 1000
```

## Architecture

```
Galaxy Buds Pro
   ↓ (Bluetooth RFCOMM)
GalaxyBudsClient Platform Libraries
   ↓ (Quaternion events)
BudsHeadTrackingBridge
   ↓ (UDP: yaw,pitch,roll,0,0,0)
OpenTrack
   ↓ (TrackIR protocol)
ETS2 / ATS / Flight Sims
```

## Project Structure

```
BudsHeadTrackingBridge/
├── Program.cs                 # Main entry point
├── HeadPose.cs               # Head orientation data structure
├── MathExtensions.cs         # Quaternion → Euler conversion
├── CoordinateMapper.cs       # Buds → OpenTrack coordinate mapping
├── OpenTrackUdpSender.cs     # UDP sender with throttling
└── GalaxyBudsClient/         # Minimal required source files
    ├── Constants.cs          # Message IDs and enums
    ├── SpatialSensorManager.cs
    └── SpatialAudioDataDecoder.cs
```

## Troubleshooting

### "No registered Galaxy Buds device found"
- Run GalaxyBudsClient first to pair and register your device
- Ensure buds are connected in Windows Bluetooth settings

### "Failed to connect to Galaxy Buds"
- Check that buds are powered on and in range
- Verify audio is working (play music to test connection)
- Try disconnecting and reconnecting in Windows Bluetooth settings

### OpenTrack not receiving data
- Verify bridge shows `[SUCCESS] Head-tracking active!`
- Check OpenTrack is listening on UDP port 4242
- Ensure no firewall is blocking localhost UDP traffic

### Low update rate or lag
- Check bridge statistics - should show ~100-120 Hz
- Ensure no other apps are using the buds simultaneously
- Try closing GalaxyBudsClient if it's running

## Technical Details

### Coordinate System Mapping
| Axis | Galaxy Buds | OpenTrack |
|------|-------------|-----------|
| Yaw | Direct | Direct |
| Pitch | Direct | Inverted (-1×) |
| Roll | Direct | Direct |

### Update Rate
- **Target**: 100-120 Hz
- **Throttling**: 8-10ms minimum interval
- **Latency**: <50ms (Buds → OpenTrack)

### UDP Protocol
OpenTrack expects CSV format:
```
yaw,pitch,roll,x,y,z\n
```
Example: `45.20,-12.30,5.10,0,0,0\n`

## Credits

- **GalaxyBudsClient** by [@timschneeb](https://github.com/timschneeb) - Reverse-engineered Samsung protocol
- **OpenTrack** - Universal head-tracking solution

## License

This project uses components from GalaxyBudsClient (GPLv3). See individual source files for attribution.

## Future Enhancements

- [ ] Drift compensation (Madgwick/Mahony filters)
- [ ] Smoothing filters for jitter reduction
- [ ] Head-tilt-to-lean mapping for racing games
- [ ] OpenXR output for VR mods
- [ ] System tray icon for easy control
- [ ] Auto-reconnect on Bluetooth disconnect
- [ ] Configuration file for axis mapping/sensitivity
