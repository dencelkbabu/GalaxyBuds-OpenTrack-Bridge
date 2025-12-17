# OpenTrack Configuration Checklist

## The data is PERFECT now! 
Your bridge output shows:
```
[DEBUG] Yaw=50.00° Pitch=0.00° Roll=0.00°
[DEBUG] Yaw=100.00° Pitch=0.00° Roll=0.00°
```
✅ Clean, smooth rotation - exactly what OpenTrack needs!

## Why OpenTrack Isn't Receiving Data

The UDP listener confirmed packets ARE being sent. The issue is OpenTrack configuration.

### **Step-by-Step OpenTrack Setup:**

#### 1. **Input Configuration**
- Click **"Input"** dropdown
- Select **"UDP over network"**
- Click the **hammer/wrench icon** next to Input
- In the settings dialog:
  - **Port**: `4242`
  - **Add to axis**: All should be `0`
  - Click **OK**

#### 2. **CRITICAL: Check the Protocol**
OpenTrack's UDP input expects a specific format. Let me check what we're sending...

Our format: `yaw,pitch,roll,0,0,0\n`
Example: `50.00,0.00,0.00,0,0,0\n`

**This should work**, but OpenTrack might expect:
- Different axis order
- Different units (radians vs degrees)
- Different separator

#### 3. **Try Alternative: FreePIE UDP Protocol**

Instead of "UDP over network", try:
1. Input: **"FreePIE UDP receiver"**
2. This uses a different protocol that might be more compatible

#### 4. **Verify OpenTrack is Actually Listening**

Run this PowerShell command while OpenTrack is running:
```powershell
Get-NetUDPEndpoint | Where-Object { $_.LocalPort -eq 4242 } | Select-Object LocalAddress, LocalPort, OwningProcess
```

**Expected**: Should show OpenTrack's process ID

**If empty**: OpenTrack isn't listening! Try:
- Restart OpenTrack
- Click "Start" button
- Check if another app is using port 4242

#### 5. **Check OpenTrack Logs**

In OpenTrack:
- Go to **Options** → **Output** 
- Enable **"Enable logging"**
- Check the log file for errors

#### 6. **Test with Different Port**

If 4242 doesn't work, try port **5005**:

**In OpenTrack**: Change UDP port to 5005

**In our bridge**: Edit `OpenTrackUdpSender.cs` line 13:
```csharp
public OpenTrackUdpSender(string host = "127.0.0.1", int port = 5005, int targetHz = 100)
```

Then rebuild: `dotnet build`

---

## Quick Test: Does OpenTrack See ANY Movement?

1. In OpenTrack, with tracking started
2. Look at the **raw input values** display (usually shows X, Y, Z, Yaw, Pitch, Roll)
3. Do ANY numbers change when the bridge is running?

If **YES**: The data is being received but not mapped correctly
If **NO**: OpenTrack isn't listening on the port

---

## Most Likely Issue

OpenTrack's "UDP over network" input might expect a **different packet format**. 

**Try this**: Use **"FreePIE UDP receiver"** instead - it's more commonly used for custom head tracking solutions.
