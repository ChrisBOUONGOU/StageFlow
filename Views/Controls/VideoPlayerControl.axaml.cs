using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using LibVLCSharp.Shared;
using StageFlow.Models;
using StageFlow.Services;
using StageFlow.ViewModels;


namespace StageFlow.Views.Controls;

public partial class VideoPlayerControl : UserControl
{
    private readonly VideoPlayerService _videoService;

    private readonly DispatcherTimer _timer;

    private bool _isDraggingTimeline;

    private string? _currentVideoPath;

    public VideoPlayerControl()
    {
        InitializeComponent();

        _videoService =
            new VideoPlayerService();

        _videoService.Initialize();

        VideoView.MediaPlayer =
            _videoService.Player;

        _timer =
            new DispatcherTimer
            {
                Interval =
                    TimeSpan.FromMilliseconds(200)
            };

        _timer.Tick += Timer_Tick;

        _timer.Start();

        Unloaded +=
            VideoPlayerControl_Unloaded;
    }

    public void LoadVideo(
        string filePath,
        bool autoPlay = false,
        bool loop = false)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return;

        if (!File.Exists(filePath))
        {
            VideoNameText.Text =
                "Video file not found";

            return;
        }

        bool loaded =
            _videoService.Load(filePath);

        if (!loaded)
        {
            VideoNameText.Text =
                "Unable to load video";

            return;
        }

        _currentVideoPath =
            filePath;

        VideoNameText.Text =
            System.IO.Path.GetFileName(
                filePath);

        LoopCheckBox.IsChecked =
            loop;

        TimelineSlider.Value = 0;

        TimeText.Text =
            "00:00 / 00:00";

        PlayPauseButton.Content = "▶";

        if (autoPlay)
        {
            _videoService.Play();

            PlayPauseButton.Content =
                "Ⅱ";
        }
    }

    public void LoadVideoElement(
        VideoElement video)
    {
        if (string.IsNullOrWhiteSpace(
                video.Source))
        {
            return;
        }

        string? projectPath =
            GetCurrentProjectPath();

        if (string.IsNullOrWhiteSpace(
                projectPath))
        {
            return;
        }

        var assetService =
            new AssetService();

        string videoPath =
            assetService.ResolveAssetPath(
                video.Source,
                projectPath);

        LoadVideo(
            videoPath,
            video.AutoPlay,
            video.Loop);

        SetVolume(
            video.Volume);
    }

    private string? GetCurrentProjectPath()
    {
        if (DataContext is MainWindowViewModel main)
            return main.CurrentFilePath;

        return null;
    }

    public void SetVolume(
        double volume)
    {
        int value =
            (int)Math.Clamp(
                volume * 100,
                0,
                100);

        VolumeSlider.Value =
            value;

        _videoService.SetVolume(value);
    }

    private void PlayPause_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (_videoService.IsPlaying)
        {
            _videoService.Pause();

            PlayPauseButton.Content =
                "▶";
        }
        else
        {
            _videoService.Play();

            PlayPauseButton.Content =
                "Ⅱ";
        }
    }

    private void Restart_Click(
        object? sender,
        RoutedEventArgs e)
    {
        _videoService.Restart();

        PlayPauseButton.Content =
            "Ⅱ";
    }

    private void Volume_ValueChanged(
        object? sender,
        Avalonia.Controls.Primitives
            .RangeBaseValueChangedEventArgs e)
    {
        _videoService.SetVolume(
            (int)e.NewValue);
    }

    private void Timeline_PointerPressed(
        object? sender,
        PointerPressedEventArgs e)
    {
        _isDraggingTimeline = true;
    }

    private void Timeline_PointerReleased(
        object? sender,
        PointerReleasedEventArgs e)
    {
        _isDraggingTimeline = false;

        SeekFromSlider();
    }

    private void SeekFromSlider()
    {
        long duration =
            _videoService.Duration;

        if (duration <= 0)
            return;

        double percentage =
            TimelineSlider.Value / 100.0;

        long position =
            (long)(duration * percentage);

        _videoService.Seek(position);
    }

    private void Timer_Tick(
        object? sender,
        EventArgs e)
    {
        if (_isDraggingTimeline)
            return;

        UpdateTimeline();

        if (!_videoService.IsPlaying &&
            _videoService.Duration > 0 &&
            _videoService.CurrentTime >=
                _videoService.Duration - 300)
        {
            if (LoopCheckBox.IsChecked == true)
            {
                _videoService.Restart();
            }
            else
            {
                PlayPauseButton.Content =
                    "▶";
            }
        }
    }

    private void UpdateTimeline()
    {
        long duration =
            _videoService.Duration;

        long current =
            _videoService.CurrentTime;

        if (duration <= 0)
        {
            TimelineSlider.Value = 0;

            TimeText.Text =
                "00:00 / 00:00";

            return;
        }

        if (!_isDraggingTimeline)
        {
            TimelineSlider.Value =
                current * 100.0 / duration;
        }

        TimeText.Text =
            $"{FormatTime(current)} / {FormatTime(duration)}";
    }

    private static string FormatTime(
        long milliseconds)
    {
        TimeSpan time =
            TimeSpan.FromMilliseconds(
                Math.Max(0, milliseconds));

        if (time.TotalHours >= 1)
        {
            return time.ToString(
                @"hh\:mm\:ss");
        }

        return time.ToString(
            @"mm\:ss");
    }

    private void VideoPlayerControl_Unloaded(
        object? sender,
        RoutedEventArgs e)
    {
        _timer.Stop();

        _videoService.Dispose();
    }

    public void PlayVideo()
    {
        _videoService.Play();

        PlayPauseButton.Content = "Ⅱ";
    }

    public void PauseVideo()
    {
        _videoService.Pause();

        PlayPauseButton.Content = "▶";
    }

    public void RestartVideo()
    {
        _videoService.Restart();

        PlayPauseButton.Content = "Ⅱ";
    }

    public void SetLoop(bool loop)
    {
        LoopCheckBox.IsChecked = loop;
    }
}