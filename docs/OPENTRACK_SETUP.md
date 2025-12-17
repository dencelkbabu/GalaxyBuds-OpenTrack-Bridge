# OpenTrack Setup Guide

## Step 1: Install OpenTrack

1. Download OpenTrack from: https://github.com/opentrack/opentrack/releases
2. Get the latest Windows installer (e.g., `opentrack-installer.exe`)
3. Run the installer and follow the prompts
4. Launch OpenTrack

## Step 2: Configure OpenTrack

### Input Configuration
1. In the OpenTrack main window, find the **"Input"** dropdown (top section)
2. Click the dropdown and select **"UDP over network"**
3. Click the **settings icon** (wrench/hammer) next to the Input dropdown
4. In the UDP settings dialog:
   - **Port**: Set to `4242`
   - **Add to axis**: Leave all at `0`
   - Click **OK**

### Output Configuration
1. Find the **"Output"** dropdown (middle section)
2. Click the dropdown and select **"freetrack 2.0 Enhanced"**
   - Alternative: Just "freetrack" works too
3. No additional output configuration needed for testing

### Mapping (Optional for Testing)
- You can leave the default mapping for now
- The bridge sends: Yaw, Pitch, Roll (no translation)

## Step 3: Start Tracking

1. Click the big **"Start"** button in OpenTrack
2. The button should change to **"Stop"** and turn green/highlighted
3. You should see a **3D preview window** appear showing a head/octopus model

## Step 4: Run the Bridge

In a separate terminal:
```bash
cd d:\software_dev\budspro-headtracking-port\BudsHeadTrackingBridge
dotnet run
```

## Step 5: Verify It's Working

### What You Should See:

**In the Bridge Console**:
```
[DEBUG] Yaw=45.00° Pitch=0.00° Roll=0.00° | Rate: 100.0 Hz | Sent: 100
[DEBUG] Yaw=90.00° Pitch=0.00° Roll=0.00° | Rate: 100.0 Hz | Sent: 200
```

**In OpenTrack**:
- The 3D head model should **rotate smoothly** left-to-right (yaw)
- Pitch and Roll should stay at 0
- The rotation should be continuous and smooth

### Troubleshooting

**Problem**: 3D model doesn't move

**Solutions**:
1. **Check OpenTrack is started**: The "Start" button should show "Stop"
2. **Verify UDP port**: Settings → Input → UDP → Port should be `4242`
3. **Check Windows Firewall**: Allow OpenTrack through firewall
4. **Restart OpenTrack**: Sometimes it needs a fresh start
5. **Check the bridge output**: Should show `Rate: ~100 Hz` and increasing Sent count

**Problem**: Model moves erratically

- This is normal for the test version - it's just rotating continuously
- The movement should be smooth, not jumpy

**Problem**: "Access Denied" or port errors

- Another application might be using port 4242
- Change the port in both OpenTrack AND the bridge code

## Next Steps

Once you see the 3D model rotating smoothly in OpenTrack, you've confirmed:
✅ UDP communication works
✅ Coordinate mapping is correct
✅ OpenTrack integration is functional

Then you can test in a game (ETS2/ATS) or proceed to add real Galaxy Buds integration!
