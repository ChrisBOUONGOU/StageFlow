using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Animation;

using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using StageFlow.Models;
using StageFlow.Services;
using StageFlow.ViewModels;
using LibVLCSharp.Shared;

using Avalonia.VisualTree;


namespace StageFlow.Views.Controls;

public partial class LiveOutputPanel : UserControl
{
    private MainWindowViewModel? _mainViewModel;

    private readonly VideoPlayerService _videoService;

    private Slide? _currentLiveSlide;

    private string? _currentVideoPath;

    private bool _isChangingSlide;

    public LiveOutputPanel()
    {
        InitializeComponent();

        _videoService =
            new VideoPlayerService();

        _videoService.Initialize();

        LiveVideoView.MediaPlayer =
            _videoService.Player;

        if (_videoService.Player != null)
        {
            _videoService.Player.EndReached +=
                VideoPlayer_EndReached;
        }

        DataContextChanged +=
            LiveOutputPanel_DataContextChanged;

        Unloaded +=
            LiveOutputPanel_Unloaded;
    }

    // ============================================================
    // DATA CONTEXT
    // ============================================================

    private void LiveOutputPanel_DataContextChanged(
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

        RenderLiveSlide(
            _mainViewModel.Live.LiveSlide);
    }

    // ============================================================
    // LIVE EVENTS
    // ============================================================

    private async void Live_PropertyChanged(
        object? sender,
        System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_mainViewModel == null)
            return;

        if (e.PropertyName ==
            nameof(LiveViewModel.LiveSlide))
        {
            Slide? slide =
                _mainViewModel.Live.LiveSlide;

            if (slide != null)
            {
                await ShowSlideAsync(slide);
            }

            return;
        }

        if (e.PropertyName ==
            nameof(LiveViewModel.IsBlackout))
        {
            ApplyBlackout(
                _mainViewModel.Live.IsBlackout);
        }
    }

    // ============================================================
    // SLIDE DISPLAY
    // ============================================================

    private async Task ShowSlideAsync(
        Slide slide)
    {
        if (_isChangingSlide)
            return;

        _isChangingSlide = true;

        try
        {
            Slide? previous =
                _currentLiveSlide;

            if (previous == null)
            {
                RenderLiveSlide(slide);

                _currentLiveSlide =
                    slide;

                return;
            }

            await TransitionToSlideAsync(
                previous,
                slide);

            _currentLiveSlide =
                slide;
        }
        finally
        {
            _isChangingSlide = false;
        }
    }

    private async Task TransitionToSlideAsync(
        Slide previous,
        Slide next)
    {
        StopCurrentVideo();

        RenderLiveSlide(next);

        await Dispatcher.UIThread.InvokeAsync(
            () => { },
            DispatcherPriority.Render);

        TransitionSettings settings =
            next.Transition;

        switch (settings.Type)
        {
            case TransitionType.Cut:
                return;

            case TransitionType.Fade:
            case TransitionType.Dissolve:
                await FadeTransitionAsync(
                    settings.Duration);
                break;

            case TransitionType.SlideLeft:
                await SlideTransitionAsync(
                    settings.Duration,
                    -1,
                    0);
                break;

            case TransitionType.SlideRight:
                await SlideTransitionAsync(
                    settings.Duration,
                    1,
                    0);
                break;

            case TransitionType.SlideUp:
                await SlideTransitionAsync(
                    settings.Duration,
                    0,
                    -1);
                break;

            case TransitionType.SlideDown:
                await SlideTransitionAsync(
                    settings.Duration,
                    0,
                    1);
                break;
        }
    }

    private async Task FadeTransitionAsync(
    double duration)
    {
        double seconds =
            Math.Max(0.05, duration);

        BlackoutOverlay.Opacity = 0;

        BlackoutOverlay.IsVisible =
            true;

        await Dispatcher.UIThread.InvokeAsync(
            () => { },
            DispatcherPriority.Render);

        await Task.Delay(
            TimeSpan.FromSeconds(
                seconds / 2));

        BlackoutOverlay.IsVisible =
            false;
    }

    private async Task SlideTransitionAsync(
    double duration,
    double directionX,
    double directionY)
    {
        double seconds =
            Math.Max(0.05, duration);

        double distance =
            directionX != 0
                ? 1920
                : 1080;

        var transform =
            new Avalonia.Media.TranslateTransform
            {
                X = directionX * distance,
                Y = directionY * distance
            };

        LiveCanvas.RenderTransform =
            transform;

        await Dispatcher.UIThread.InvokeAsync(
            () => { },
            DispatcherPriority.Render);

        var animation =
            new Avalonia.Animation.Animation
            {
                Duration =
                    TimeSpan.FromSeconds(seconds),

                Children =
                {
                new Avalonia.Animation.KeyFrame
                {
                    Cue =
                        new Avalonia.Animation.Cue(0),

                    Setters =
                    {
                        new Avalonia.Styling.Setter(
                            Avalonia.Media.TranslateTransform.XProperty,
                            directionX * distance),

                        new Avalonia.Styling.Setter(
                            Avalonia.Media.TranslateTransform.YProperty,
                            directionY * distance)
                    }
                },

                new Avalonia.Animation.KeyFrame
                {
                    Cue =
                        new Avalonia.Animation.Cue(1),

                    Setters =
                    {
                        new Avalonia.Styling.Setter(
                            Avalonia.Media.TranslateTransform.XProperty,
                            0d),

                        new Avalonia.Styling.Setter(
                            Avalonia.Media.TranslateTransform.YProperty,
                            0d)
                    }
                }
                }
            };

        await animation.RunAsync(
            LiveCanvas);

        LiveCanvas.RenderTransform =
            null;
    }

    private async Task PlayTransitionAsync(
        TransitionSettings settings)
    {
        if (settings.Type ==
            TransitionType.Cut)
        {
            return;
        }

        double duration =
            Math.Max(
                0.05,
                settings.Duration);

        TimeSpan time =
            TimeSpan.FromSeconds(
                duration);

        if (settings.Type ==
            TransitionType.Fade ||
            settings.Type ==
            TransitionType.Dissolve)
        {
            await FadeStageAsync(time);
            return;
        }

        await Task.Delay(time);
    }

    private async Task FadeStageAsync(
        TimeSpan duration)
    {
        BlackoutOverlay.IsVisible = true;

        await Task.Delay(
            TimeSpan.FromTicks(
                duration.Ticks / 2));

        BlackoutOverlay.IsVisible = false;
    }

    // ============================================================
    // RENDER
    // ============================================================

    private void RenderLiveSlide(
        Slide? slide)
    {
        StopCurrentVideo();

        LiveCanvas.Children.Clear();

        LiveVideoView.IsVisible =
            false;

        if (slide == null)
        {
            LiveTitle.Text =
                "No live slide";

            LiveCanvas.Background =
                Brushes.Black;

            return;
        }

        LiveTitle.Text =
            slide.Title;

        try
        {
            LiveCanvas.Background =
                Brush.Parse(
                    slide.Background);
        }
        catch
        {
            LiveCanvas.Background =
                Brushes.Black;
        }

        foreach (
            SlideElement element
            in slide.Elements
                .Where(x => x.IsVisible)
                .OrderBy(x => x.ZIndex))
        {
            Control control =
                CreateElementControl(
                    element);

            if (element is VideoElement)
            {
                continue;
            }

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

            LiveCanvas.Children.Add(
                control);
        }
    }

    // ============================================================
    // ELEMENT CREATION
    // ============================================================

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

            VideoElement video =>
                CreateVideoControl(video),

            _ =>
                new Border()
        };
    }

    // ============================================================
    // TEXT
    // ============================================================

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

        VerticalAlignment verticalAlignment =
            text.VerticalAlignment switch
            {
                "Top" =>
                    VerticalAlignment.Top,

                "Bottom" =>
                    VerticalAlignment.Bottom,

                _ =>
                    VerticalAlignment.Center
            };

        FontWeight weight =
            text.IsBold
                ? FontWeight.Bold
                : FontWeight.Normal;

        FontStyle style =
            text.IsItalic
                ? FontStyle.Italic
                : FontStyle.Normal;

        IBrush foreground;

        try
        {
            foreground =
                Brush.Parse(
                    text.Color);
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

            FontSize =
                text.FontSize,

            FontFamily =
                new FontFamily(
                    text.FontFamily),

            FontWeight = weight,

            FontStyle = style,

            Foreground =
                foreground,

            TextAlignment =
                alignment,

            VerticalAlignment =
                verticalAlignment,

            TextWrapping =
                TextWrapping.Wrap,

            Opacity =
                text.Opacity
        };
    }

    // ============================================================
    // SHAPE
    // ============================================================

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
            Width =
                shape.Width,

            Height =
                shape.Height,

            Background =
                fill,

            BorderBrush =
                stroke,

            BorderThickness =
                new Thickness(
                    shape.StrokeThickness),

            Opacity =
                shape.Opacity
        };
    }

    // ============================================================
    // IMAGE
    // ============================================================

    private Control CreateImageControl(
        ImageElement image)
    {
        if (_mainViewModel == null)
            return CreatePlaceholder(
                "IMAGE");

        if (string.IsNullOrWhiteSpace(
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
            {
                return CreatePlaceholder(
                    "IMAGE NOT FOUND");
            }

            var bitmap =
                new Bitmap(path);

            return new Avalonia.Controls.Image
            {
                Source =
                    bitmap,

                Width =
                    image.Width,

                Height =
                    image.Height,

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

    // ============================================================
    // VIDEO
    // ============================================================

    private Control CreateVideoControl(
        VideoElement video)
    {
        if (_mainViewModel == null)
            return CreatePlaceholder(
                "VIDEO");

        if (string.IsNullOrWhiteSpace(
                _mainViewModel.CurrentFilePath))
        {
            return CreatePlaceholder(
                "VIDEO");
        }

        try
        {
            var assetService =
                new AssetService();

            string path =
                assetService.ResolveAssetPath(
                    video.Source,
                    _mainViewModel.CurrentFilePath);

            if (!File.Exists(path))
            {
                return CreatePlaceholder(
                    "VIDEO NOT FOUND");
            }

            LoadLiveVideo(
                path,
                video);

            LiveVideoView.Width =
                video.Width;

            LiveVideoView.Height =
                video.Height;

            LiveVideoView.IsVisible =
                true;

            Canvas.SetLeft(
                LiveVideoView,
                video.X);

            Canvas.SetTop(
                LiveVideoView,
                video.Y);

            LiveVideoView.SetValue(
    Panel.ZIndexProperty,
    video.ZIndex + 1000);

            return new Border
            {
                Width =
                    video.Width,

                Height =
                    video.Height,

                Background =
                    Brushes.Transparent,

                IsHitTestVisible =
                    false
            };
        }
        catch
        {
            return CreatePlaceholder(
                "VIDEO ERROR");
        }
    }

    private void LoadLiveVideo(
        string path,
        VideoElement video)
    {
        StopCurrentVideo();

        _currentVideoPath =
            path;

        bool loaded =
            _videoService.Load(path);

        if (!loaded)
        {
            LiveTitle.Text =
                "Unable to load live video";

            return;
        }

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

    // ============================================================
    // VIDEO LOOP
    // ============================================================

    private void VideoPlayer_EndReached(
        object? sender,
        EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_mainViewModel == null)
                return;

            if (_mainViewModel.Live.LiveSlide
                is not Slide slide)
            {
                return;
            }

            VideoElement? video =
                slide.Elements
                    .OfType<VideoElement>()
                    .FirstOrDefault(x =>
                        x.IsVisible &&
                        !string.IsNullOrWhiteSpace(
                            x.Source));

            if (video == null ||
                !video.Loop)
            {
                return;
            }

            _videoService.Restart();
        });
    }

    // ============================================================
    // STOP VIDEO
    // ============================================================

    private void StopCurrentVideo()
    {
        _videoService.Stop();

        _currentVideoPath =
            null;

        LiveVideoView.IsVisible =
            false;
    }

    // ============================================================
    // BLACKOUT
    // ============================================================

    private void ApplyBlackout(
        bool blackout)
    {
        BlackoutOverlay.IsVisible =
            blackout;

        if (blackout)
        {
            LiveTitle.Text =
                "BLACKOUT";

            return;
        }

        if (_currentLiveSlide != null)
        {
            LiveTitle.Text =
                _currentLiveSlide.Title;
        }
    }

    // ============================================================
    // BUTTONS
    // ============================================================

    private void GoLive_Click(
        object? sender,
        Avalonia.Interactivity.RoutedEventArgs e)
    {
        _mainViewModel?.GoLive();
    }

    private void Blackout_Click(
        object? sender,
        Avalonia.Interactivity.RoutedEventArgs e)
    {
        _mainViewModel?.BlackoutLive();
    }

    private void Restore_Click(
        object? sender,
        Avalonia.Interactivity.RoutedEventArgs e)
    {
        _mainViewModel?.RestoreLive();
    }

    // ============================================================
    // CLEANUP
    // ============================================================

    private void LiveOutputPanel_Unloaded(
        object? sender,
        Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_mainViewModel != null)
        {
            _mainViewModel.Live.PropertyChanged -=
                Live_PropertyChanged;
        }

        if (_videoService.Player != null)
        {
            _videoService.Player.EndReached -=
                VideoPlayer_EndReached;
        }

        _videoService.Dispose();
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
                    Text =
                        text,

                    Foreground =
                        Brushes.White,

                    HorizontalAlignment =
                        HorizontalAlignment.Center,

                    VerticalAlignment =
                        VerticalAlignment.Center
                }
        };
    }

    public void ConfigureExternalOutput()
    {
        LiveHeader.IsVisible = false;

        LiveStageBorder.Margin =
            new Thickness(0);

        LiveCanvas.Background =
            Brushes.Black;
    }
}