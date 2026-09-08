using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;

using Avalonia.Media;
using Avalonia.Media.Imaging;
using StageFlow.Models;
using StageFlow.Services;
using StageFlow.ViewModels;

namespace StageFlow.Views.Controls;

public partial class PreviewPanel : UserControl
{
    private MainWindowViewModel? _mainViewModel;

    private readonly VideoPlayerService _videoService;

    private Slide? _previewSlide;

    private VideoElement? _previewVideo;

    public PreviewPanel()
    {
        InitializeComponent();

        _videoService =
            new VideoPlayerService();

        _videoService.Initialize();

        PreviewVideoView.MediaPlayer =
            _videoService.Player;

        DataContextChanged +=
            PreviewPanel_DataContextChanged;

        Unloaded +=
            PreviewPanel_Unloaded;
    }

    private void PreviewPanel_DataContextChanged(
        object? sender,
        EventArgs e)
    {
        if (_mainViewModel != null)
        {
            _mainViewModel.Live.PropertyChanged -=
                Live_PropertyChanged;
        }

        _mainViewModel =
            DataContext as MainWindowViewModel;

        if (_mainViewModel == null)
            return;

        _mainViewModel.Live.PropertyChanged +=
            Live_PropertyChanged;

        RenderPreview(
            _mainViewModel.Live.PreviewSlide);
    }

    private void Live_PropertyChanged(
        object? sender,
        System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_mainViewModel == null)
            return;

        if (e.PropertyName ==
            nameof(LiveViewModel.PreviewSlide))
        {
            RenderPreview(
                _mainViewModel.Live.PreviewSlide);
        }
    }

    private void RenderPreview(
        Slide? slide)
    {
        StopVideo();

        PreviewCanvas.Children.Clear();

        _previewSlide = slide;

        if (slide == null)
        {
            PreviewTitle.Text =
                "No preview";

            PreviewCanvas.Background =
                Brushes.Black;

            return;
        }

        PreviewTitle.Text =
            slide.Title;

        try
        {
            PreviewCanvas.Background =
                Brush.Parse(
                    slide.Background);
        }
        catch
        {
            PreviewCanvas.Background =
                Brushes.Black;
        }

        foreach (
            SlideElement element
            in slide.Elements
                .Where(x => x.IsVisible)
                .OrderBy(x => x.ZIndex))
        {
            if (element is VideoElement video)
            {
                LoadPreviewVideo(video);
                continue;
            }

            Control control =
                CreateElementControl(
                    element);

            Canvas.SetLeft(
                control,
                element.X);

            Canvas.SetTop(
                control,
                element.Y);

            control.SetValue(
    Panel.ZIndexProperty,
    element.ZIndex);

            if (Math.Abs(element.Rotation) > 0.01)
            {
                control.RenderTransform =
                    new RotateTransform(
                        element.Rotation);
            }

            PreviewCanvas.Children.Add(
                control);
        }
    }

    private void LoadPreviewVideo(
        VideoElement video)
    {
        if (_mainViewModel == null)
            return;

        if (string.IsNullOrWhiteSpace(
                _mainViewModel.CurrentFilePath))
            return;

        try
        {
            var assetService =
                new AssetService();

            string path =
                assetService.ResolveAssetPath(
                    video.Source,
                    _mainViewModel.CurrentFilePath);

            if (!File.Exists(path))
                return;

            bool loaded =
                _videoService.Load(path);

            if (!loaded)
                return;

            _previewVideo =
                video;

            PreviewVideoView.Width =
                video.Width;

            PreviewVideoView.Height =
                video.Height;

            PreviewVideoView.IsVisible =
                true;

            Canvas.SetLeft(
                PreviewVideoView,
                video.X);

            Canvas.SetTop(
                PreviewVideoView,
                video.Y);

            PreviewVideoView.SetValue(
    Panel.ZIndexProperty,
    video.ZIndex + 1000);

            

            _videoService.SetVolume(
                (int)Math.Clamp(
                    video.Volume * 100,
                    0,
                    100));

            if (video.AutoPlay)
            {
                _videoService.Play();
            }
        }
        catch
        {
            PreviewVideoView.IsVisible =
                false;
        }
    }

    private void StopVideo()
    {
        _videoService.Stop();

        _previewVideo = null;

        PreviewVideoView.IsVisible =
            false;
    }

    private Control CreateElementControl(
        SlideElement element)
    {
        return element switch
        {
            TextElement text =>
                CreateTextControl(text),

            ShapeElement shape =>
                CreateShapeControl(shape),

            ImageElement image =>
                CreateImageControl(image),

            _ =>
                new Border()
        };
    }

    private Control CreateTextControl(
        TextElement text)
    {
        TextAlignment alignment =
            text.HorizontalAlignment switch
            {
                "Left" =>
                    TextAlignment.Left,

                "Right" =>
                    TextAlignment.Right,

                _ =>
                    TextAlignment.Center
            };

        VerticalAlignment vertical =
            text.VerticalAlignment switch
            {
                "Top" =>
                    VerticalAlignment.Top,

                "Bottom" =>
                    VerticalAlignment.Bottom,

                _ =>
                    VerticalAlignment.Center
            };

        IBrush foreground;

        try
        {
            foreground =
                Brush.Parse(text.Color);
        }
        catch
        {
            foreground =
                Brushes.White;
        }

        return new TextBlock
        {
            Text = text.Text,

            Width = text.Width,
            Height = text.Height,

            FontSize = text.FontSize,

            FontFamily =
                new FontFamily(
                    text.FontFamily),

            FontWeight =
                text.IsBold
                    ? FontWeight.Bold
                    : FontWeight.Normal,

            FontStyle =
                text.IsItalic
                    ? FontStyle.Italic
                    : FontStyle.Normal,

            Foreground =
                foreground,

            TextAlignment =
                alignment,

            VerticalAlignment =
                vertical,

            TextWrapping =
                TextWrapping.Wrap,

            Opacity =
                text.Opacity
        };
    }

    private Control CreateShapeControl(
        ShapeElement shape)
    {
        IBrush fill;

        try
        {
            fill =
                Brush.Parse(
                    shape.Fill);
        }
        catch
        {
            fill =
                Brushes.Transparent;
        }

        IBrush stroke;

        try
        {
            stroke =
                Brush.Parse(
                    shape.Stroke);
        }
        catch
        {
            stroke =
                Brushes.Transparent;
        }

        return new Border
        {
            Width = shape.Width,
            Height = shape.Height,

            Background = fill,
            BorderBrush = stroke,

            BorderThickness =
                new Thickness(
                    shape.StrokeThickness),

            Opacity = shape.Opacity
        };
    }

    private Control CreateImageControl(
        ImageElement image)
    {
        if (_mainViewModel == null ||
            string.IsNullOrWhiteSpace(
                _mainViewModel.CurrentFilePath))
        {
            return CreatePlaceholder(
                "IMAGE");
        }

        try
        {
            var assetService =
                new AssetService();

            string path =
                assetService.ResolveAssetPath(
                    image.Source,
                    _mainViewModel.CurrentFilePath);

            if (!File.Exists(path))
                return CreatePlaceholder(
                    "IMAGE NOT FOUND");

            var bitmap =
                new Bitmap(path);

            return new Avalonia.Controls.Image
            {
                Source = bitmap,

                Width = image.Width,
                Height = image.Height,

                Stretch =
                    image.StretchUniform
                        ? Stretch.Uniform
                        : Stretch.Fill,

                Opacity =
                    image.Opacity
            };
        }
        catch
        {
            return CreatePlaceholder(
                "IMAGE ERROR");
        }
    }

    private static Control CreatePlaceholder(
        string text)
    {
        return new Border
        {
            Background =
                Brushes.Black,

            Child =
                new TextBlock
                {
                    Text = text,

                    Foreground =
                        Brushes.White,

                    HorizontalAlignment =
                        HorizontalAlignment.Center,

                    VerticalAlignment =
                        VerticalAlignment.Center
                }
        };
    }

    private void GoLive_Click(
        object? sender,
        Avalonia.Interactivity.RoutedEventArgs e)
    {
        _mainViewModel?.GoLive();
    }

    private void PreviewPanel_Unloaded(
        object? sender,
        Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_mainViewModel != null)
        {
            _mainViewModel.Live.PropertyChanged -=
                Live_PropertyChanged;
        }

        _videoService.Dispose();
    }
}