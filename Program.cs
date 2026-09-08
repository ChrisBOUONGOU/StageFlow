using Avalonia;
using System;
using LibVLCSharp.Shared;

namespace StageFlow
{
    internal static class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args) {
            Core.Initialize();

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args); 
        }
        public static AppBuilder BuildAvaloniaApp() { 
            return AppBuilder.Configure<App>().UsePlatformDetect().WithInterFont().LogToTrace(); 
        }
    }
}
