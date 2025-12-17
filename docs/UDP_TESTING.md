# Testing UDP Connection

## Test 1: Verify UDP Packets Are Being Sent

### Use the UDP Listener Tool

**In Terminal 1** (UDP Listener):
```bash
cd d:\software_dev\budspro-headtracking-port\UdpListener
dotnet run
```

**In Terminal 2** (Bridge):
```bash
cd d:\software_dev\budspro-headtracking-port\BudsHeadTrackingBridge
dotnet run
```

**Expected Output in Terminal 1**:
```
[   10] 5.00,0.00,0.00,0,0,0                     | Rate: 100.2 Hz
[   20] 10.00,0.00,0.00,0,0,0                    | Rate: 100.1 Hz
```

**If you see**: "Port 4242 is already in use" - That's actually GOOD! It means OpenTrack is listening.

```powershell
# Check if port 4242 is listening
Get-NetUDPEndpoint | Where-Object { $_.LocalPort -eq 4242 }
```

## Test 2: Verify Quaternion Fix

Run the bridge and check the output:
```bash
cd BudsHeadTrackingBridge
dotnet run
```

**Before the fix**, you saw:
```
[DEBUG] Yaw=0.00° Pitch=-50.50° Roll=0.00°
[DEBUG] Yaw=180.00° Pitch=-79.50° Roll=180.00°  ← Pitch jumping wildly
```

**After the fix**, you should see:
```
[DEBUG] Yaw=0.00° Pitch=0.00° Roll=0.00°
[DEBUG] Yaw=0.50° Pitch=0.00° Roll=0.00°  ← Smooth yaw-only rotation
[DEBUG] Yaw=1.00° Pitch=0.00° Roll=0.00°
```

## Test 3: OpenTrack Configuration Checklist

1. **OpenTrack is installed and running** ✓
2. **Input is set to "UDP over network"** ✓
3. **UDP port is 4242** ✓
4. **Output is set to "freetrack 2.0 Enhanced"** ✓
5. **"Start" button is clicked** (button shows "Stop") ✓
6. **3D preview window is visible** ✓

## If Still Not Working

### Check Windows Firewall
```powershell
# Allow OpenTrack through firewall
New-NetFirewallRule -DisplayName "OpenTrack UDP" -Direction Inbound -Protocol UDP -LocalPort 4242 -Action Allow
```

### Try Different Port
If 4242 is blocked, try port 5005:

1. In OpenTrack: Change UDP port to 5005
2. In `OpenTrackUdpSender.cs`, change:
   ```csharp
   public OpenTrackUdpSender(string host = "127.0.0.1", int port = 5005, int targetHz = 100)
   ```
3. Rebuild and run

### Check if Another App is Using Port 4242
```powershell
Get-Process -Id (Get-NetUDPEndpoint -LocalPort 4242).OwningProcess
```
