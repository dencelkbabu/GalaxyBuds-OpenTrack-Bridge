using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace BudsHeadTrackingBridge;

public class App : Application
{
    public override void Initialize()
    {
        // No XAML for App class
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }
        
        base.OnFrameworkInitializationCompleted();
    }
}
