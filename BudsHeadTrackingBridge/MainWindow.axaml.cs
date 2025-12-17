using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
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
    
    // Real mode components
    private BluetoothHeadTrackingManager? _bluetoothManager;
    private CoordinateMapper? _coordinateMapper;
    private OpenTrackUdpSender? _udpSender;
    private DateTime _lastStatsTime;
    private int _sentCount;

    public MainWindow()
    {
        Title = "Galaxy Buds Head-Tracking Bridge";
        Width = 900;
        Height = 700;
        
        // Deep dark background
        Background = new SolidColorBrush(Color.Parse("#121212"));
        
        // Header
        var headerPanel = new StackPanel { Margin = new Thickness(0, 20, 0, 20) };
        var titleText = new TextBlock
        {
            Text = "Galaxy Buds Bridge",
            FontSize = 24,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#FFFFFF")),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        var subtitleText = new TextBlock
        {
            Text = "Head Tracking for OpenTrack / ETS2 / ATS",
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.Parse("#AAAAAA")),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 0)
        };
        headerPanel.Children.Add(titleText);
        headerPanel.Children.Add(subtitleText);

        _consoleOutput = new TextBlock
        {
            FontFamily = new FontFamily("Consolas,Courier New,monospace"),
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.Parse("#00FF00")), // Terminal green
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(5),
            VerticalAlignment = VerticalAlignment.Top
        };
        
        _consoleScrollViewer = new ScrollViewer 
        { 
            Content = _consoleOutput,
            VerticalScrollBarVisibility = ScrollBarVisibility.Visible, // Force scrollbar to be visible
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        };

        // Console container with border
        var consoleBorder = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#000000")),
            BorderBrush = new SolidColorBrush(Color.Parse("#333333")),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Margin = new Thickness(20, 0, 20, 20),
            Padding = new Thickness(5),
            Child = _consoleScrollViewer
        };
        
        // Action Buttons (Recenter, Clear, Quit)
        var recenterButton = CreateStyledButton("Recenter (R)", "#6200EE", 150);
        recenterButton.Click += (s, e) => RequestRecenter();

        var cycleButton = CreateStyledButton("Cycle Axis (M)", "#018786", 150);
        cycleButton.Click += (s, e) => {
            _coordinateMapper?.CycleMapping();
        };

        var clearButton = CreateStyledButton("Clear (C)", "#B00020", 150);
        clearButton.Click += (s, e) => {
             _coordinateMapper?.ClearRecenter();
             Console.WriteLine("[COMMAND] Cleared recenter calibration");
        };

        var quitButton = CreateStyledButton("Quit (Q)", "#333333", 150);
        quitButton.Click += (s, e) => Close();

        _actionPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10),
            Spacing = 10,
            Children = { recenterButton, cycleButton, clearButton, quitButton },
            IsVisible = false // Hidden initially, shown when running
        };

        // Mode Buttons
        var realModeButton = CreateStyledButton("Start Real Mode (Galaxy Buds)", "#3700B3"); // Deep Purple
        realModeButton.Click += async (s, e) => await StartRealMode();
        
        var testModeButton = CreateStyledButton("Start Test Mode (Simulation)", "#03DAC6");  // Teal
        testModeButton.Foreground = new SolidColorBrush(Color.Parse("#000000")); // Dark text on teal
        testModeButton.Click += async (s, e) => await StartTestMode();
        
        _buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 20),
            Spacing = 20,
            Children = { realModeButton, testModeButton }
        };
        
        // Main Layout
        var mainPanel = new DockPanel();
        
        var topContainer = new StackPanel();
        topContainer.Children.Add(headerPanel);
        topContainer.Children.Add(_buttonPanel);
        topContainer.Children.Add(_actionPanel); // Add action buttons here
        
        DockPanel.SetDock(topContainer, Dock.Top);
        mainPanel.Children.Add(topContainer);
        mainPanel.Children.Add(consoleBorder); // Fills remaining space
        
        Content = mainPanel;
        
        // Redirect console output to window
        var originalOut = Console.Out;
        var writer = new WindowConsoleWriter(this, originalOut);
        Console.SetOut(writer);
        
        // Events
        Loaded += OnLoaded;
        KeyDown += OnKeyDown;
    }

    private ScrollViewer _consoleScrollViewer;
    private StackPanel _actionPanel;

    private Button CreateStyledButton(string content, string colorHex, double width = 250)
    {
        return new Button
        {
            Content = content,
            Width = width,
            Height = 45,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            FontSize = 14,
            FontWeight = FontWeight.SemiBold,
            Background = new SolidColorBrush(Color.Parse(colorHex)),
            Foreground = new SolidColorBrush(Colors.White),
            CornerRadius = new CornerRadius(8),
        };
    }

    private void RequestRecenter()
    {
        Console.WriteLine("[COMMAND] Requesting Recenter (will apply on next packet)");
        _pendingRecenter = true;
    }

    private void OnKeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (!_isRunning) return;
        
        switch (e.Key)
        {
            case Avalonia.Input.Key.R:
                RequestRecenter();
                break;
            case Avalonia.Input.Key.C:
                _coordinateMapper?.ClearRecenter();
                Console.WriteLine("[COMMAND] Cleared recenter calibration");
                break;
            case Avalonia.Input.Key.M:
                _coordinateMapper?.CycleMapping();
                break;
            case Avalonia.Input.Key.Q:
                Close();
                break;
        }
    }

    private bool _pendingRecenter = false;

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
        _actionPanel.IsVisible = true; // Show actions
        
        Console.WriteLine("[INFO] Starting in REAL MODE with Galaxy Buds\n");
        await RunRealModeAsync();
    }

    private async Task StartTestMode()
    {
        if (_isRunning) return;
        _isRunning = true;
        _buttonPanel.IsVisible = false;
        _actionPanel.IsVisible = true; // Show actions
        
        Console.WriteLine("[INFO] Starting in TEST MODE (mock data)\n");
        await RunTestModeAsync();
    }

    public void AppendText(string text)
    {
        Dispatcher.UIThread.Post(async () =>
        {
             _consoleText.Append(text);
             _consoleOutput.Text = _consoleText.ToString();
             
             // Wait for layout update to ensure correct scrolling
             await Dispatcher.UIThread.InvokeAsync(() => {}, DispatcherPriority.Background);
             _consoleScrollViewer.ScrollToEnd();
        });
    }

    private async Task RunRealModeAsync()
    {
        _coordinateMapper = new CoordinateMapper();
        _udpSender = new OpenTrackUdpSender(targetHz: 200);
        _bluetoothManager = new BluetoothHeadTrackingManager();
        
        _bluetoothManager.Connected += (s, e) => 
        {
            Console.WriteLine("[SUCCESS] Connected to Galaxy Buds!");
            _bluetoothManager.StartHeadTracking();
        };
        _bluetoothManager.Disconnected += (s, reason) => Console.WriteLine($"[INFO] Disconnected: {reason}");
        _bluetoothManager.Error += (s, msg) => Console.WriteLine($"[ERROR] {msg}");
        
        _bluetoothManager.QuaternionReceived += OnQuaternionReceived;
        
        Console.WriteLine("[INFO] Initializing connection...");
        if (await _bluetoothManager.ConnectAsync())
        {
            // Keep alive check loop
            while (_isRunning && _bluetoothManager.IsConnected)
            {
                await Task.Delay(1000);
            }
        }
        else
        {
            Console.WriteLine("[ERROR] Failed to connect. Please check if GalaxyBudsClient is installed and buds are paired.");
            _isRunning = false;
        }
    }

    private void OnQuaternionReceived(object? sender, System.Numerics.Quaternion q)
    {
        if (_coordinateMapper == null || _udpSender == null) return;

        // Handle pending recenter
        if (_pendingRecenter)
        {
           _coordinateMapper.Recenter(q);
           _pendingRecenter = false;
        }

        var headPose = _coordinateMapper.QuaternionToHeadPose(q);
        
        if (_udpSender.SendPose(headPose))
        {
            _sentCount++;
            
            if (_sentCount == 1 || _sentCount % 10 == 0)
            {
                var elapsed = (DateTime.UtcNow - _lastStatsTime).TotalSeconds;
                if (elapsed > 0)
                {
                   var hz = _sentCount == 1 ? 0 : 10.0 / elapsed; 
                   Console.WriteLine($"[DEBUG] {headPose} | Rate: {hz:F1} Hz | Sent: {_sentCount}");
                }
                else
                {
                    Console.WriteLine($"[DEBUG] {headPose} | Sent: {_sentCount}");
                }
                _lastStatsTime = DateTime.UtcNow;
            }
        }
    }
    
    protected override void OnClosed(EventArgs e)
    {
        _isRunning = false;
        _bluetoothManager?.Dispose();
        _udpSender?.Dispose();
        base.OnClosed(e);
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
    private readonly System.IO.TextWriter _originalOut;

    public WindowConsoleWriter(MainWindow window, System.IO.TextWriter originalOut)
    {
        _window = window;
        _originalOut = originalOut;
    }

    public override void WriteLine(string? value)
    {
        _originalOut.WriteLine(value);
        // Performance optimization: Don't print high-frequency TRACE/DEBUG logs to the UI window, only to the console
        if (value != null && !value.Contains("[TRACE]") && !value.Contains("[DEBUG]"))
        {
            _window.AppendText(value + Environment.NewLine);
        }
    }

    public override void Write(string? value)
    {
        _originalOut.Write(value);
        _window.AppendText(value ?? string.Empty);
    }

    public override System.Text.Encoding Encoding => System.Text.Encoding.UTF8;
}
