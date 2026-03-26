using System;
using System.IO;
using Avalonia;

namespace Logtingsval2026;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Ved dobbeltklik fra Finder er CWD ofte / — Avalonia/.NET skal køre fra mappen med .dll/.dylib.
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
