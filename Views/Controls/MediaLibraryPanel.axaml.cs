using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using StageFlow.Models;
using StageFlow.ViewModels;
using Avalonia.Interactivity;






namespace StageFlow.Views.Controls;

public partial class MediaLibraryPanel : UserControl
{
    public MediaLibraryPanel()
    {
        InitializeComponent();
    }

    private void AssetPointerPressed(
       object? sender,
       PointerPressedEventArgs e)
    {
        if (sender is not Control control)
            return;

        if (control.DataContext is not MediaAsset asset)
            return;

        var data = new DataObject();

        data.Set(
            "StageFlow.MediaAsset",
            asset.Id.ToString());

        DragDrop.DoDragDrop(
            e,
            data,
            DragDropEffects.Copy);
    }

    protected override void OnAttachedToVisualTree(
        VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        AddHandler(
            PointerPressedEvent,
            AssetPointerPressed,
            RoutingStrategies.Bubble);
    }

    protected override void OnDetachedFromVisualTree(
        VisualTreeAttachmentEventArgs e)
    {
        RemoveHandler(
            PointerPressedEvent,
            AssetPointerPressed);

        base.OnDetachedFromVisualTree(e);
    }
}