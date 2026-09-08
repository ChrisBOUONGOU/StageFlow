using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using StageFlow.ViewModels;

namespace StageFlow.Views;

public partial class LiveOutputWindow : Window
{
    private MainWindowViewModel? _mainViewModel;

    public LiveOutputWindow(
        MainWindowViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;

        _mainViewModel = viewModel;

        Opened += LiveOutputWindow_Opened;
        Closed += LiveOutputWindow_Closed;
    }

    private void LiveOutputWindow_Opened(
        object? sender,
        EventArgs e)
    {
        ConfigureExternalOutput();

        MoveToSecondaryScreen();
    }

    private void ConfigureExternalOutput()
    {
        LiveOutputPanel.ConfigureExternalOutput();
    }

    private void MoveToSecondaryScreen()
    {
        IReadOnlyList<Screen> screens =
            Screens.All;

        if (screens.Count < 2)
        {
            _mainViewModel?.SetStatus(
                "Second screen not detected.");

            Close();

            return;
        }

        Screen? primary =
            Screens.Primary;

        Screen? secondary =
            screens.FirstOrDefault(
                screen =>
                    primary == null ||
                    !ReferenceEquals(
                        screen,
                        primary));

        secondary ??=
            screens
                .FirstOrDefault();

        if (secondary == null)
        {
            _mainViewModel?.SetStatus(
                "Unable to find output screen.");

            Close();

            return;
        }

        PixelRect bounds =
            secondary.Bounds;

        Position =
            new PixelPoint(
                bounds.X,
                bounds.Y);

        Width =
            bounds.Width;

        Height =
            bounds.Height;

        WindowState =
            WindowState.FullScreen;

        _mainViewModel?.SetStatus(
            $"Live output: {secondary.DisplayName ?? "Secondary screen"}");
    }

    private void LiveOutputWindow_Closed(
        object? sender,
        EventArgs e)
    {
        if (_mainViewModel != null)
        {
            _mainViewModel.SetStatus(
                "Live output disconnected.");
        }
    }
}