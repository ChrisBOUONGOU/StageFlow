using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using StageFlow.Models;
using StageFlow.Services;
using StageFlow.ViewModels;
using StageFlow.Views.Controls;



namespace StageFlow.Views
{
    public partial class MainWindow : Window
    {
        private LiveOutputWindow? _liveOutputWindow;
        private bool _isApplicationFullscreen;
        private PresentationWindow? _presentationWindow;
       

        public MainWindow()
        {
            InitializeComponent();

            KeyDown += MainWindow_KeyDown;
            DataContextChanged += MainWindow_DataContextChanged;
            Closed += MainWindow_Closed;
            KeyDown += MainWindow_KeyDown;
        }

        private void MainWindow_KeyDown(object? sender, KeyEventArgs e)
        {
            // ESC : sortir du plein écran
            if (e.Key == Key.Escape)
            {
                if (_isApplicationFullscreen)
                {
                    ExitFullscreen();
                    e.Handled = true;
                }

                return;
            }

            // F11 : activer / désactiver le plein écran
            if (e.Key == Key.F11)
            {
                ToggleFullscreen();
                e.Handled = true;
                return;
            }

            // Ne pas intercepter les flèches lorsqu'on écrit
            // dans une zone de texte.
            if (e.Source is TextBox)
                return;

            if (DataContext is not MainWindowViewModel viewModel)
                return;

            // Slide précédente
            if (e.Key == Key.Left)
            {
                NavigateToPreviousSlide(viewModel);
                e.Handled = true;
                return;
            }

            // Slide suivante
            if (e.Key == Key.Right)
            {
                NavigateToNextSlide(viewModel);
                e.Handled = true;
            }
        }

        private static void NavigateToPreviousSlide(
            MainWindowViewModel viewModel)
        {
            if (viewModel.Slides.Count == 0 ||
                viewModel.SelectedSlide is null)
            {
                return;
            }

            var currentIndex =
                viewModel.Slides.IndexOf(viewModel.SelectedSlide);

            if (currentIndex <= 0)
                return;

            viewModel.SelectedSlide =
                viewModel.Slides[currentIndex - 1];

            viewModel.Editor.RefreshCanvas();
        }

        private static void NavigateToNextSlide(
            MainWindowViewModel viewModel)
        {
            if (viewModel.Slides.Count == 0 ||
                viewModel.SelectedSlide is null)
            {
                return;
            }

            var currentIndex =
                viewModel.Slides.IndexOf(viewModel.SelectedSlide);

            if (currentIndex < 0 ||
                currentIndex >= viewModel.Slides.Count - 1)
            {
                return;
            }

            viewModel.SelectedSlide =
                viewModel.Slides[currentIndex + 1];

            viewModel.Editor.RefreshCanvas();
        }

        private void ToggleFullscreen()
        {
            if (_isApplicationFullscreen)
            {
                ExitFullscreen();
            }
            else
            {
                EnterFullscreen();
            }
        }

        private void EnterFullscreen()
        {
            _isApplicationFullscreen = true;
            WindowState = WindowState.FullScreen;
        }

        private void ExitFullscreen()
        {
            _isApplicationFullscreen = false;
            WindowState = WindowState.Normal;
        }

        private void MainWindow_DataContextChanged(
            object? sender,
            EventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.Lyrics.HostWindow = this;
            }
        }

        private void MainWindow_Closed(
            object? sender,
            EventArgs e)
        {
            _liveOutputWindow?.Close();
            _liveOutputWindow = null;
        }

        private void OpenOutput_Click(
            object? sender,
            RoutedEventArgs e)
        {
            if (_liveOutputWindow is not null)
                return;

            if (DataContext is not MainWindowViewModel viewModel)
                return;

            _liveOutputWindow = new LiveOutputWindow(viewModel);

            _liveOutputWindow.Closed += (_, _) =>
            {
                _liveOutputWindow = null;
            };

            _liveOutputWindow.Show();
        }

        private void GoLive_Click(
            object? sender,
            RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.Live.GoLive();
            }
        }

        private void RightToolButton_Click(
    object? sender,
    RoutedEventArgs e)
        {
            if (sender is not Button button)
                return;

            var tag = button.Tag?.ToString();

            PropertiesPanel.IsVisible = tag == "Properties";
            MediaLibraryPanel.IsVisible = tag == "Media";
            ThemePanel.IsVisible = tag == "Themes";
            LyricsPanel.IsVisible = tag == "Lyrics";
            BiblePanel.IsVisible = tag == "Bible";
            ApplicationThemePanel.IsVisible = tag == "ApplicationTheme";
        }

        private void OpenPresentation_Click(
    object? sender,
    RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel viewModel)
                return;

            if (_presentationWindow is not null)
            {
                _presentationWindow.Activate();
                return;
            }

            _presentationWindow =
                new PresentationWindow(viewModel);

            _presentationWindow.Closed +=
                (_, _) =>
                {
                    _presentationWindow = null;
                };

            _presentationWindow.Show();

            _presentationWindow.Activate();
        }

        private async void AddImage_Click(
    object? sender,
    RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel viewModel)
                return;

            await viewModel.Editor.AddImageCommand.ExecuteAsync(null);
        }

        private async void AddVideo_Click(
    object? sender,
    RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel viewModel)
                return;

            await viewModel.Editor.AddVideoCommand.ExecuteAsync(null);
        }


    }
}