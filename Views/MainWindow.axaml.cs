using Avalonia;
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
       
        private bool _isApplicationFullscreen;
        private PresentationWindow? _presentationWindow;
       

        public MainWindow()
        {
            InitializeComponent();

            KeyDown += MainWindow_KeyDown;
            DataContextChanged += MainWindow_DataContextChanged;
           
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

        private void AddText_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel viewModel)
                return;

            if (viewModel.SelectedSlide is null)
                return;

            viewModel.SelectedSlide.Elements.Add(
                new TextElement
                {
                    Text = "New Text",
                    X = 760,
                    Y = 450,
                    Width = 400,
                    Height = 100,
                    FontSize = 48,
                    HorizontalAlignment = "Center",
                    VerticalAlignment = "Center",
                    ZIndex = 1
                });

            viewModel.Editor.RefreshCanvas();
        }

        private async void SlideTitle_DoubleTapped(object? sender, TappedEventArgs e)
        {
            if (sender is not TextBlock textBlock)
                return;

            if (textBlock.DataContext is not Slide slide)
                return;

            var dialog = new Window
            {
                Title = "Rename Slide",
                Width = 400,
                Height = 180
            };

            var textBox = new TextBox
            {
                Text = slide.Title,
                Margin = new Thickness(20)
            };

            var button = new Button
            {
                Content = "OK",
               
                Margin = new Thickness(20)
            };

            button.Click += (_, _) =>
            {
                if (!string.IsNullOrWhiteSpace(textBox.Text))
                {
                    slide.Title = textBox.Text.Trim();
                }

                dialog.Close();
            };

            var panel = new StackPanel();

            panel.Children.Add(textBox);
            panel.Children.Add(button);

            dialog.Content = panel;

            await dialog.ShowDialog(TopLevel.GetTopLevel(this) as Window);
        }

        private void SlideTitleEditor_KeyDown(object? sender, KeyEventArgs e)
        {
            if (sender is not TextBox editor)
                return;

            if (e.Key == Key.Enter)
            {
                SaveSlideTitle(editor);
            }

            if (e.Key == Key.Escape)
            {
                CancelSlideTitle(editor);
            }
        }

        private void SlideTitleEditor_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (sender is TextBox editor)
            {
                SaveSlideTitle(editor);
            }
        }

        private void SaveSlideTitle(TextBox editor)
        {
            if (editor.DataContext is not Slide slide)
                return;

            var newTitle = editor.Text?.Trim();

            if (!string.IsNullOrWhiteSpace(newTitle))
            {
                slide.Title = newTitle;
            }

            if (editor.Parent is not Grid grid)
                return;

            if (grid.FindControl<TextBlock>("TitleText") is TextBlock titleText)
            {
                titleText.Text = slide.Title;
                titleText.IsVisible = true;
            }

            editor.IsVisible = false;
        }

        private void CancelSlideTitle(TextBox editor)
        {
            if (editor.Parent is not Grid grid)
                return;

            if (editor.DataContext is Slide slide)
            {
                editor.Text = slide.Title;
            }

            if (grid.FindControl<TextBlock>("TitleText") is TextBlock titleText)
            {
                titleText.IsVisible = true;
            }

            editor.IsVisible = false;
        }


    }
}