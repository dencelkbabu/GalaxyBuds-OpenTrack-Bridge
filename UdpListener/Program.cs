using System.Net;
using System.Net.Sockets;
using System.Text;

Console.WriteLine("===========================================");
Console.WriteLine("  UDP Packet Listener - Port 4242");
Console.WriteLine("  Testing BudsHeadTrackingBridge");
Console.WriteLine("===========================================\n");

Console.WriteLine("[INFO] Listening for UDP packets on 127.0.0.1:4242");
Console.WriteLine("[INFO] Press Ctrl+C to stop\n");

try
{
    var udpClient = new UdpClient(4242);
    var remoteEP = new IPEndPoint(IPAddress.Any, 0);

    var count = 0;
    var startTime = DateTime.UtcNow;
    
    while (true)
    {
        var data = udpClient.Receive(ref remoteEP);
        var message = Encoding.ASCII.GetString(data);
        count++;
        
        // Print every 10th packet to avoid spam
        if (count % 10 == 0)
        {
            var elapsed = (DateTime.UtcNow - startTime).TotalSeconds;
            var hz = count / elapsed;
            Console.WriteLine($"[{count,5}] {message.Trim(),-40} | Rate: {hz:F1} Hz");
        }
    }
}
catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
{
    Console.WriteLine("\n[ERROR] Port 4242 is already in use!");
    Console.WriteLine("[INFO] This likely means OpenTrack is already listening on this port.");
    Console.WriteLine("[INFO] If OpenTrack is running and configured correctly, that's good!");
    Console.WriteLine("\n[TIP] Close OpenTrack to run this listener, or just test with OpenTrack directly.");
}
catch (Exception ex)
{
    Console.WriteLine($"\n[ERROR] {ex.Message}");
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();
