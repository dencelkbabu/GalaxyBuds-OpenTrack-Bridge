# Galaxy Buds Head-Tracking Bridge for OpenTrack

A lightweight C# bridge that extracts real-time head-tracking data from Samsung Galaxy Buds Pro and forwards it to OpenTrack for use in ETS2, ATS, and other simulation games.

## Features

- ✅ **Real-time head tracking** from Galaxy Buds Pro/Buds2 Pro/Buds3 Pro
- ✅ **Modern GUI interface** with Avalonia UI
- ✅ **Two coordinate mapping modes**:
  - Mode 1 (Standard): Yaw/Roll swapped - optimized for Galaxy Buds orientation
  - Mode 2 (Direct): No axis swapping - alternative mapping
- ✅ **Recenter functionality** - set current position as zero point
- ✅ **UDP streaming to OpenTrack** at ~100-120 Hz
- ✅ **Test mode** - simulate head tracking without hardware
- ✅ **Low latency** (<50ms)
- ✅ **Proper cleanup** - no hanging on quit

## Requirements

### Hardware
- **Galaxy Buds Pro**, **Buds2 Pro**, or **Buds3 Pro** (with head-tracking support)
- Windows 11 PC with Bluetooth

### Software
- **OpenTrack** - [Download](https://github.com/opentrack/opentrack/releases)
- **GalaxyBudsClient** (for initial device pairing) - [Download](https://github.com/timschneeb/GalaxyBudsClient/releases)

> **Note**: The application is a standalone executable (~300MB). No .NET installation required!

## Setup

### 1. Pair Galaxy Buds
1. Download and install [GalaxyBudsClient](https://github.com/timschneeb/GalaxyBudsClient/releases)
2. Pair your Galaxy Buds Pro with Windows via Bluetooth
3. Open GalaxyBudsClient and connect to your buds (this registers the device)
4. Verify audio is working

### 2. Download the Bridge
Download the latest release from [GitHub Releases](https://github.com/YOUR_USERNAME/budspro-headtracking-port/releases)

Extract the ZIP file to a folder of your choice.

### 3. Configure OpenTrack
1. Install and launch OpenTrack
2. Set **Input** to: `UDP over network`
3. Set **Port** to: `4242`
4. Set **Output** to: `freetrack 2.0 Enhanced` (for ETS2/ATS)
5. Click **Start**

### 4. Run the Bridge
Double-click `BudsHeadTrackingBridge.exe` from the extracted folder.

A GUI window will appear with two mode options:
- **Start Real Mode (Galaxy Buds)** - Connect to your Galaxy Buds
- **Start Test Mode (Simulation)** - Test without hardware

Expected console output:
```
===========================================
  Galaxy Buds Head-Tracking Bridge v1.0
  For OpenTrack / ETS2 / ATS
===========================================

[INFO] Connecting to Galaxy Buds...
[SUCCESS] Connected to: Galaxy Buds Pro
[INFO] Starting head-tracking mode...
[SUCCESS] Head-tracking active!
```

### 5. Test in Game
1. Launch Euro Truck Simulator 2 or American Truck Simulator
2. Enable **TrackIR** or **Head Tracking** in game settings
3. Start driving and move your head
4. Camera should follow your head movement

## Usage

### GUI Controls
- **Recenter (R)** - Set current head position as the new zero point
- **Cycle Axis (M)** - Switch between Mode 1 (Standard) and Mode 2 (Direct)
- **Clear (C)** - Remove recenter calibration and return to absolute tracking
- **Quit (Q)** - Exit application

### Keyboard Shortcuts
- `R` - Recenter head position
- `M` - Cycle between mapping modes
- `C` - Clear recenter calibration
- `Q` - Quit

### Coordinate Mapping Modes

**Mode 1: Standard (Default)**
- Swaps Yaw ↔ Roll axes
- Optimized for Galaxy Buds IMU orientation
- Use this mode for normal operation

**Mode 2: Direct**
- No axis swapping
- Alternative mapping for troubleshooting
- Switch with `M` key or "Cycle Axis" button

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
├── Program.cs                        # Main entry point
├── MainWindow.axaml                  # GUI layout
├── MainWindow.axaml.cs               # GUI logic and event handlers
├── HeadPose.cs                       # Head orientation data structure
├── MathExtensions.cs                 # Quaternion → Euler conversion
├── CoordinateMapper.cs               # Coordinate mapping with 2 modes
├── OpenTrackUdpSender.cs             # UDP sender with throttling
├── BluetoothHeadTrackingManager.cs   # Bluetooth connection manager
└── (Uses GalaxyBudsClient libraries) # Spatial sensor integration
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

**Mode 1 (Standard - Default)**
| Axis | Galaxy Buds | OpenTrack |
|------|-------------|-----------|
| Yaw | Roll | Yaw |
| Pitch | Pitch | -Pitch |
| Roll | Yaw | Roll |

**Mode 2 (Direct)**
| Axis | Galaxy Buds | OpenTrack |
|------|-------------|-----------|
| Yaw | Yaw | Yaw |
| Pitch | Pitch | -Pitch |
| Roll | Roll | Roll |

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

- **GalaxyBudsClient** by [@timschneeb](https://github.com/timschneeb) - Original reverse-engineered Samsung protocol
- **OpenTrack** - Universal head-tracking solution

## Development
This project was vibe coded using **Antigravity** powered by **Gemini 3 Pro** and **Claude Sonnet 4.5**.

## License

This project uses components from GalaxyBudsClient (GPLv3). See individual source files for attribution.

## Changelog

### v1.0-beta (2025-12-17)
- ✅ Modern Avalonia UI with dark theme
- ✅ Simplified to 2 coordinate mapping modes (removed broken modes 1-5)
- ✅ Fixed app hang on quit with proper async cleanup
- ✅ Recenter and Clear functionality with GUI buttons
- ✅ Test mode for simulation without hardware
- ✅ Real-time debug output in console window

## Future Enhancements

- [ ] Drift compensation (Madgwick/Mahony filters)
- [ ] Smoothing filters for jitter reduction
- [ ] Head-tilt-to-lean mapping for racing games
- [ ] OpenXR output for VR mods
- [ ] System tray icon for easy control
- [ ] Auto-reconnect on Bluetooth disconnect
- [ ] Configuration file for sensitivity tuning
