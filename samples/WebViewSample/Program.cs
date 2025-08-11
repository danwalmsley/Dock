using System;
using Avalonia;
using Dock.Model.Avalonia;
using Dock.Settings;
using WebViewControl;

namespace WebViewSample;

internal class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        WebView.Settings.OsrEnabled = false;

        DockSettings.UseFloatingDockAdorner = true;
        // Global docking is now configured per root dock via IRootDock.EnableGlobalDocking

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        GC.KeepAlive(typeof(Factory).Assembly);

        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
    }
}
