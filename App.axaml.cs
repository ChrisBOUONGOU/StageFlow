using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using StageFlow.ViewModels;
using StageFlow.Views;
using System;

namespace StageFlow
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var splash = new SplashWindow();
                desktop.MainWindow = splash;
                splash.Show();

                var timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(1800)
                };

                timer.Tick += (_, _) =>
                {
                    timer.Stop();

                    var mainWindow = new MainWindow
                    {
                        DataContext = new MainWindowViewModel()
                    };

                    desktop.MainWindow = mainWindow;
                    mainWindow.Show();
                    splash.Close();
                };

                timer.Start();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}