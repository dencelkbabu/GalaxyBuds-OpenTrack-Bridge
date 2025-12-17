using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Numerics;
using System.Text;

namespace BudsHeadTrackingBridge;

public class MainWindow : Window
{
    private readonly StringBuilder _consoleText = new();
    private readonly TextBlock _consoleOutput;
    private readonly StackPanel _buttonPanel;
    private bool _isRunning = false;

    public MainWindow()
    {
        Title = "Galaxy Buds Head-Tracking Bridge";
        Width = 800;
        Height = 600;
        Background = new SolidColorBrush(Color.Parse("#1E1E1E"));
        
        // Create console output TextBlock
        _consoleOutput = new TextBlock
        {
            Margin = new Thickness(10),
            FontFamily = new FontFamily("Consolas,Courier New,monospace"),
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.Parse("#D4D4D4")),
            TextWrapping = TextWrapping.Wrap
        };
        
        // Create mode selection buttons
        var realModeButton = new Button
        {
            Content = "Real Galaxy Buds Mode",
            Width = 200,
            Height = 40,
            Margin = new Thickness(10)
        };
        realModeButton.Click += async (s, e) => await StartRealMode();
        
        var testModeButton = new Button
        {
            Content = "Test Mode (Mock Data)",
            Width = 200,
            Height = 40,
            Margin = new Thickness(10)
        };
        testModeButton.Click += async (s, e) => await StartTestMode();
        
        _buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(10),
            Children = { realModeButton, testModeButton }
        };
        
        // Layout
        var mainPanel = new DockPanel();
        DockPanel.SetDock(_buttonPanel, Dock.Top);
        mainPanel.Children.Add(_buttonPanel);
        
        var scrollViewer = new ScrollViewer
        {
            Content = _consoleOutput
        };
        mainPanel.Children.Add(scrollViewer);
        
        Content = mainPanel;
        
        // Redirect console output to window
        var writer = new WindowConsoleWriter(this);
        Console.SetOut(writer);
        
        // Show welcome message
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("===========================================");
        Console.WriteLine("  Galaxy Buds Head-Tracking Bridge v1.0");
        Console.WriteLine("  For OpenTrack / ETS2 / ATS");
        Console.WriteLine("===========================================\n");
        Console.WriteLine("Select mode using the buttons above:\n");
    }

    private async Task StartRealMode()
    {
        if (_isRunning) return;
        _isRunning = true;
        _buttonPanel.IsVisible = false;
        
        Console.WriteLine("[INFO] Starting in REAL MODE with Galaxy Buds\n");
        await RunRealModeAsync();
    }

    private async Task StartTestMode()
    {
        if (_isRunning) return;
        _isRunning = true;
        _buttonPanel.IsVisible = false;
        
        Console.WriteLine("[INFO] Starting in TEST MODE (mock data)\n");
        await RunTestModeAsync();
    }

    public void AppendText(string text)
    {
        Dispatcher.UIThread.Post(() =>
        {
            _consoleText.AppendLine(text);
            _consoleOutput.Text = _consoleText.ToString();
        });
    }

    private async Task RunRealModeAsync()
    {
        // TODO: Implement real Galaxy Buds mode
        Console.WriteLine("[INFO] Real mode not yet implemented in GUI version");
        Console.WriteLine("[INFO] Falling back to test mode...\n");
        await RunTestModeAsync();
    }

    private async Task RunTestModeAsync()
    {
        var udpSender = new OpenTrackUdpSender(targetHz: 100);
        var sentCount = 0;
        var lastStatsTime = DateTime.UtcNow;
        
        Console.WriteLine("[INFO] Test mode: Generating mock head-tracking data");
        Console.WriteLine("[INFO] Sending data to OpenTrack on UDP 127.0.0.1:4242\n");
        
        var angle = 0.0f;
        
        try
        {
            while (true)
            {
                angle += 0.5f;
                if (angle > 360) angle -= 360;
                
                var headPose = new HeadPose(
                    yaw: angle,
                    pitch: 0.0f,
                    roll: 0.0f,
                    timestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                );
                
                if (udpSender.SendPose(headPose))
                {
                    sentCount++;
                    
                    if (sentCount % 100 == 0)
                    {
                        var elapsed = (DateTime.UtcNow - lastStatsTime).TotalSeconds;
                        var hz = 100.0 / elapsed;
                        Console.WriteLine($"[DEBUG] {headPose} | Rate: {hz:F1} Hz | Sent: {sentCount}");
                        lastStatsTime = DateTime.UtcNow;
                    }
                }
                
                await Task.Delay(10);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[ERROR] Fatal error: {ex.Message}");
        }
        finally
        {
            udpSender?.Dispose();
        }
    }
}

public class WindowConsoleWriter : System.IO.TextWriter
{
    private readonly MainWindow _window;

    public WindowConsoleWriter(MainWindow window)
    {
        _window = window;
    }

    public override void WriteLine(string? value)
    {
        _window.AppendText(value ?? string.Empty);
    }

    public override void Write(string? value)
    {
        _window.AppendText(value ?? string.Empty);
    }

    public override System.Text.Encoding Encoding => System.Text.Encoding.UTF8;
}
