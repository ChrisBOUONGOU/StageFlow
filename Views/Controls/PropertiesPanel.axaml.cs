using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
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
    }

    private void VideoRestart_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (DataContext is not EditorViewModel editor ||
            editor.SelectedElement is not VideoElement video)
            return;

        VideoPlaybackService.Restart(video.Id);
    }

}