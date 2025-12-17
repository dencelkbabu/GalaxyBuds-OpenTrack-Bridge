using Avalonia;

namespace BudsHeadTrackingBridge;

public static class Program
{
    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    private static extern bool AllocConsole();

    public static void Main(string[] args)
    {
        if (args.Contains("--debug"))
        {
            AllocConsole();
            System.Console.WriteLine("[DEBUG] Console allocated.");
        }

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
