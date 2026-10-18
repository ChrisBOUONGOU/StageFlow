using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Avalonia.Input;
using StageFlow.Models;
using StageFlow.Services;
using StageFlow.ViewModels;

namespace StageFlow.Views.Controls;

public partial class PropertiesPanel : UserControl
{
    public PropertiesPanel()
    {
        InitializeComponent();
    }
    private void VideoPlayPause_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (DataContext is not EditorViewModel editor ||
            editor.SelectedElement is not VideoElement video)
            return;

        VideoPlaybackService.Toggle(video.Id);
        editor.RefreshVideoPlaybackStateForControls();
    }

    private void VideoStop_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (DataContext is not EditorViewModel editor ||
            editor.SelectedElement is not VideoElement video)
            return;

        VideoPlaybackService.Stop(video.Id, resetPosition: true);
        editor.RefreshVideoPlaybackStateForControls();
    }

    private void VideoSeek_PointerPressed(
        object? sender,
        PointerPressedEventArgs e)
    {
        if (DataContext is EditorViewModel editor &&
            editor.SelectedElement is VideoElement)
            editor.SetVideoSeeking(true);
    }

    private void VideoSeek_PointerReleased(
        object? sender,
        PointerReleasedEventArgs e)
    {
        if (DataContext is not EditorViewModel editor ||
            editor.SelectedElement is not VideoElement)
            return;

        editor.SeekSelectedVideo((long)VideoProgressSlider.Value);
    }

    private void VideoRestart_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (DataContext is not EditorViewModel editor ||
            editor.SelectedElement is not VideoElement video)
            return;

        VideoPlaybackService.Restart(video.Id);
        editor.RefreshVideoPlaybackStateForControls();
    }

}