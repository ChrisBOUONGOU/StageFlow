using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using LibVLCSharp.Shared;
using StageFlow.Models;
using StageFlow.Services;
using StageFlow.ViewModels;
using System.Diagnostics;
using Avalonia.Controls.Primitives;



namespace StageFlow.Views.Controls;

public partial class SlideCanvas : UserControl
{
    private SlideElement? _draggedElement;

    private Point _dragStart;

    private double _startX;
    private double _startY;

    private bool _isResizing;

    private ResizeHandle _resizeHandle;

    private double _startWidth;
    private double _startHeight;

    private double _startElementX;
    private double _startElementY;

    private TextBox? _textEditor;
    private TextElement? _editingTextElement;
    private string _originalText = string.Empty;

    private readonly LibVLC _libVLC;
    private readonly Dictionary<Guid, MediaPlayer> _videoPlayers = new();

    public SlideCanvas()
    {
        InitializeComponent();

        _libVLC = new LibVLC();

        EditorCanvas.PointerPressed +=
            CanvasPointerPressed;

        EditorCanvas.PointerMoved +=
            CanvasPointerMoved;

        EditorCanvas.PointerReleased +=
            CanvasPointerReleased;

        KeyDown +=
            OnKeyDown;

        KeyDown += CanvasKeyDown;

        DragDrop.SetAllowDrop(this, true);

        AddHandler(
            DragDrop.DragOverEvent,
            CanvasDragOver);

        AddHandler(
            DragDrop.DropEvent,
            CanvasDrop);
    }

    protected override void OnDataContextChanged(
        EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is EditorViewModel editor)
        {
            editor.PropertyChanged +=
                EditorPropertyChanged;
        }

        RenderCanvas();
    }

    private void EditorPropertyChanged(
        object? sender,
        System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName ==
            nameof(EditorViewModel.CurrentElements))
        {
            RenderCanvas();
            return;
        }

        if (e.PropertyName ==
                nameof(EditorViewModel.SelectedElement))
        {
            UpdateSelectionVisual();
        }
    }

    private EditorViewModel? Editor =>
        DataContext as EditorViewModel;

    private void RenderCanvas()
    {
        EditorCanvas.Children.Clear();

        var slide = Editor?.CurrentSlide;

        if (slide == null)
            return;

        EditorCanvas.Background =
            Brush.Parse(slide.Background);

        foreach (var element in slide.Elements
                     .Where(x => x.IsVisible)
                     .OrderBy(x => x.ZIndex))
        {
            Control control =
                CreateElementControl(element);

            Canvas.SetLeft(control, element.X);
            Canvas.SetTop(control, element.Y);

            control.Width = element.Width;
            control.Height = element.Height;

            control.Opacity = element.Opacity;

            control.Tag = element;

           

            EditorCanvas.Children.Add(control);

            if (Editor?.SelectedElement == element)
            {
                AddSelectionBox(element);

                if (element is TextElement text)
                {
                    AddTextToolbar(text);
                }
            }
        }
    }


private void AddTextToolbar(TextElement text)
    {
        var toolbar = CreateTextToolbar(text);

        EditorCanvas.Children.Add(toolbar);

        toolbar.Measure(new Size(
            double.PositiveInfinity,
            double.PositiveInfinity));

        double toolbarWidth =
            toolbar.DesiredSize.Width;

        double toolbarHeight =
            toolbar.DesiredSize.Height;

        double x =
            text.X +
            (text.Width - toolbarWidth) / 2;

        double y =
            text.Y -
            toolbarHeight -
            12;

        // Si la toolbar dépasse à gauche
        if (x < 5)
            x = 5;

        // Si elle dépasse à droite
        if (x + toolbarWidth > 1920)
            x = 1920 - toolbarWidth - 5;

        // Si elle dépasse en haut,
        // on la place sous le texte
        if (y < 5)
        {
            y = text.Y +
                text.Height +
                12;
        }

        Canvas.SetLeft(toolbar, x);
        Canvas.SetTop(toolbar, y);
    }



    private Control? CreateElementControl(
        SlideElement element)
    {
        Control? control = element switch
        {
            TextElement text => CreateTextControl(text),
            ShapeElement shape => CreateShapeControl(shape),
            ImageElement image => CreateImageControl(image),
            VideoElement video => CreateVideoControl(video),
            _ => null
        };

        if (control == null)
            return null;

        control.Tag = element;

        control.PointerPressed += ElementPointerPressed;

        return control;
    }


private Control CreateTextToolbar(TextElement text)
    {
        var toolbar = new Border
        {
            Background = Brush.Parse("#20242D"),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(6),
            Tag = "TextToolbar",
            ZIndex = 20000
        };

        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4
        };

        // =====================================================
        // DIMINUER LA TAILLE
        // =====================================================

        var decreaseButton = new Button
        {
            Content = "A−",
            Width = 36,
            Height = 32
        };

        decreaseButton.Click += (_, _) =>
        {
            if (Editor?.SelectedElement is not TextElement)
                return;

            Editor.SelectedFontSize =
                Math.Max(8, Editor.SelectedFontSize - 2);
        };

        panel.Children.Add(decreaseButton);

        // =====================================================
        // TAILLE
        // =====================================================

        var fontSizeBox = new TextBox
        {
            Width = 48,
            Height = 32,
            Text = Editor?.SelectedFontSize.ToString("0") ?? "48",
            HorizontalContentAlignment =
                HorizontalAlignment.Center,
            VerticalContentAlignment =
                VerticalAlignment.Center
        };

        fontSizeBox.KeyDown += (_, e) =>
        {
            if (e.Key != Key.Enter)
                return;

            if (double.TryParse(
                    fontSizeBox.Text,
                    out double size))
            {
                if (Editor != null)
                {
                    Editor.SelectedFontSize =
                        Math.Max(8, size);
                }
            }

            e.Handled = true;
        };

        panel.Children.Add(fontSizeBox);

        // =====================================================
        // AUGMENTER LA TAILLE
        // =====================================================

        var increaseButton = new Button
        {
            Content = "A+",
            Width = 36,
            Height = 32
        };

        increaseButton.Click += (_, _) =>
        {
            if (Editor?.SelectedElement is not TextElement)
                return;

            Editor.SelectedFontSize += 2;
        };

        panel.Children.Add(increaseButton);

        // =====================================================
        // BOLD
        // =====================================================

        var boldButton = new ToggleButton
        {
            Content = "B",
            Width = 34,
            Height = 32,
            FontWeight = FontWeight.Bold,
            IsChecked = text.IsBold
        };

        boldButton.Click += (_, _) =>
        {
            if (Editor == null)
                return;

            Editor.SelectedBold =
                boldButton.IsChecked == true;
        };

        panel.Children.Add(boldButton);

        // =====================================================
        // ITALIC
        // =====================================================

        var italicButton = new ToggleButton
        {
            Content = "I",
            Width = 34,
            Height = 32,
            FontStyle = FontStyle.Italic,
            IsChecked = text.IsItalic
        };

        italicButton.Click += (_, _) =>
        {
            if (Editor == null)
                return;

            Editor.SelectedItalic =
                italicButton.IsChecked == true;
        };

        panel.Children.Add(italicButton);

        // =====================================================
        // COULEUR
        // =====================================================

        var colorButton = new Button
        {
            Content = "●",
            Width = 34,
            Height = 32,
            Foreground = Brush.Parse(text.Color)
        };

        colorButton.Click += (_, _) =>
        {
            ShowTextColorPicker(colorButton);
        };

        panel.Children.Add(colorButton);

        toolbar.Child = panel;

        return toolbar;
    }



private void ShowTextColorPicker(Button target)
    {
        if (Editor?.SelectedElement is not TextElement text)
            return;

        var colorPicker = new ColorPicker
        {
            Color = Color.Parse(text.Color),
            Width = 250,
            Height = 300
        };

        var popup = new Popup
        {
            PlacementTarget = target,
            Placement = PlacementMode.Bottom,
            HorizontalOffset = 0,
            VerticalOffset = 5,
            IsLightDismissEnabled = true,
            Child = colorPicker
        };

        colorPicker.ColorChanged += (_, e) =>
        {
            if (Editor?.SelectedElement is not TextElement selectedText)
                return;

            // Utilise ton EditorViewModel existant
            Editor.SelectedColorValue = e.NewColor;

            // Met à jour immédiatement le bouton
            target.Foreground =
                new SolidColorBrush(e.NewColor);
        };

        popup.Open();
    }




    private Control CreateTextControl(TextElement text)
    {
        var textBlock = new TextBlock
        {
            Text = text.Text,
            FontFamily = new FontFamily(text.FontFamily),
            FontSize = text.FontSize,
            Foreground = Brush.Parse(text.Color),
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = ConvertHorizontalAlignment(
                text.HorizontalAlignment),
            VerticalAlignment = ConvertVerticalAlignment(
                text.VerticalAlignment)
        };

        if (text.IsBold)
            textBlock.FontWeight = FontWeight.Bold;

        if (text.IsItalic)
            textBlock.FontStyle = FontStyle.Italic;

        var border = new Border
        {
            Background = Brushes.Transparent,
            Child = textBlock,
            Width = text.Width,
            Height = text.Height,
            Opacity = text.Opacity
        };

        return border;
    }

    private Control CreateShapeControl(ShapeElement shape)
    {
        return new Border
        {
            Width = shape.Width,
            Height = shape.Height,
            Background = Brush.Parse(shape.Fill),
            BorderBrush = Brush.Parse(shape.Stroke),
            BorderThickness = new Thickness(
                shape.StrokeThickness),
            Opacity = shape.Opacity
        };
    }

    private Control CreateImageControl(ImageElement image)
    {
        if (string.IsNullOrWhiteSpace(image.Source))
        {
            return CreateImagePlaceholder(
                "IMAGE",
                "#202631");
        }

        try
        {
            string imagePath =
     ResolveImagePath(image);

           

            if (Editor != null &&
                !string.IsNullOrWhiteSpace(
                    Editor.MainViewModel.CurrentFilePath))
            {
                var assetService = new AssetService();

                imagePath =
                    assetService.ResolveAssetPath(
                        image.Source,
                        Editor.MainViewModel.CurrentFilePath);
            }

            if (!File.Exists(imagePath))
            {
                return CreateImagePlaceholder(
                    "IMAGE NOT FOUND",
                    "#3A2020");
            }

            var bitmap =
                new Bitmap(imagePath);

            var imageControl =
                new Avalonia.Controls.Image
                {
                    Source = bitmap,

                    Stretch =
                        image.StretchUniform
                            ? Stretch.Uniform
                            : Stretch.Fill,

                    HorizontalAlignment =
                        HorizontalAlignment.Stretch,

                    VerticalAlignment =
                        VerticalAlignment.Stretch
                };

            return new Border
            {
                Width = image.Width,
                Height = image.Height,
                Background = Brushes.Transparent,
                Opacity = image.Opacity,
                Child = imageControl
            };
        }
        catch
        {
            return CreateImagePlaceholder(
                "UNABLE TO LOAD IMAGE",
                "#3A2020");
        }
    }

    private static Control CreateImagePlaceholder(
    string text,
    string background)
    {
        return new Border
        {
            Background =
                Brush.Parse(background),

            BorderBrush =
                Brushes.Gray,

            BorderThickness =
                new Thickness(1),

            Child = new TextBlock
            {
                Text = text,

                HorizontalAlignment =
                    HorizontalAlignment.Center,

                VerticalAlignment =
                    VerticalAlignment.Center,

                Foreground =
                    Brushes.White
            }
        };
    }

    private Control CreateVideoControl(VideoElement video)
    {
        if (string.IsNullOrWhiteSpace(video.Source))
            return CreateImagePlaceholder("VIDEO", "#202631");

        if (Editor == null ||
            string.IsNullOrWhiteSpace(Editor.MainViewModel.CurrentFilePath))
        {
            return CreateImagePlaceholder(
                "VIDEO PATH NOT AVAILABLE",
                "#3A2020");
        }

        try
        {
            var assetService = new AssetService();

            string videoPath = assetService.ResolveAssetPath(
                video.Source,
                Editor.MainViewModel.CurrentFilePath);

            if (!File.Exists(videoPath))
                return CreateImagePlaceholder(
                    "VIDEO NOT FOUND",
                    "#3A2020");

            if (!_videoPlayers.TryGetValue(
                    video.Id,
                    out MediaPlayer? mediaPlayer))
            {
                mediaPlayer = new MediaPlayer(_libVLC);
                _videoPlayers[video.Id] = mediaPlayer;
            }

            var videoView = new LibVLCSharp.Avalonia.VideoView
            {
                MediaPlayer = mediaPlayer,

                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,

                IsHitTestVisible = false
            };

            if (mediaPlayer.Media == null)
            {
                using var media = new Media(
                    _libVLC,
                    videoPath,
                    FromType.FromPath);

                mediaPlayer.Media = media;
            }

            mediaPlayer.Volume =
                (int)Math.Clamp(video.Volume * 100, 0, 100);

            if (video.AutoPlay && !mediaPlayer.IsPlaying)
                mediaPlayer.Play();

            return new Border
            {
                Width = video.Width,
                Height = video.Height,

                Background = Brushes.Black,

                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(0),

                Padding = new Thickness(0),

                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,

                Opacity = video.Opacity,

                IsHitTestVisible = true,

                Child = videoView
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                $"Video rendering error: {ex}");

            return CreateImagePlaceholder(
                "VIDEO ERROR",
                "#3A2020");
        }
    }

    private void AddSelectionBox(
        SlideElement element)
    {
        var selection = new Border
        {
            BorderBrush = Brushes.DeepSkyBlue,
            BorderThickness = new Thickness(3),
            Background = Brushes.Transparent,
            IsHitTestVisible = false,
            Tag = "SelectionBorder"
        };

        Canvas.SetLeft(
            selection,
            element.X - 3);

        Canvas.SetTop(
            selection,
            element.Y - 3);

        selection.Width =
            element.Width + 6;

        selection.Height =
            element.Height + 6;

        selection.ZIndex = 10000;

        EditorCanvas.Children.Add(
            selection);

        AddHandle(
            element,
            HandlePosition.TopLeft);

        AddHandle(
            element,
            HandlePosition.Top);

        AddHandle(
            element,
            HandlePosition.TopRight);

        AddHandle(
            element,
            HandlePosition.Left);

        AddHandle(
            element,
            HandlePosition.Right);

        AddHandle(
            element,
            HandlePosition.BottomLeft);

        AddHandle(
            element,
            HandlePosition.Bottom);

        AddHandle(
            element,
            HandlePosition.BottomRight);
    }

    private void AddHandle(
        SlideElement element,
        HandlePosition position)
    {
        var handle =
       new Border
       {
           Width = 14,
           Height = 14,

           Background =
               Brushes.White,

           BorderBrush =
               Brushes.DeepSkyBlue,

           BorderThickness =
               new Thickness(2),

           Tag = new HandleData
           {
               Element = element,
               Position = position
           },

           Cursor =
               GetCursor(position)
       };

        double x;
        double y;

        switch (position)
        {
            case HandlePosition.TopLeft:
                x = element.X - 7;
                y = element.Y - 7;
                break;

            case HandlePosition.Top:
                x = element.X +
                    element.Width / 2 - 7;
                y = element.Y - 7;
                break;

            case HandlePosition.TopRight:
                x = element.X +
                    element.Width - 7;
                y = element.Y - 7;
                break;

            case HandlePosition.Left:
                x = element.X - 7;
                y = element.Y +
                    element.Height / 2 - 7;
                break;

            case HandlePosition.Right:
                x = element.X +
                    element.Width - 7;
                y = element.Y +
                    element.Height / 2 - 7;
                break;

            case HandlePosition.BottomLeft:
                x = element.X - 7;
                y = element.Y +
                    element.Height - 7;
                break;

            case HandlePosition.Bottom:
                x = element.X +
                    element.Width / 2 - 7;
                y = element.Y +
                    element.Height - 7;
                break;

            default:
                x = element.X +
                    element.Width - 7;

                y = element.Y +
                    element.Height - 7;
                break;
        }

        Canvas.SetLeft(handle, x);
        Canvas.SetTop(handle, y);

        handle.PointerPressed +=
            HandlePointerPressed;

        EditorCanvas.Children.Add(handle);
    }

    private static Cursor GetCursor(
        HandlePosition position)
    {
        return position switch
        {
            HandlePosition.TopLeft =>
                new Cursor(StandardCursorType.TopLeftCorner),

            HandlePosition.TopRight =>
                new Cursor(StandardCursorType.TopRightCorner),

            HandlePosition.BottomLeft =>
                new Cursor(StandardCursorType.BottomLeftCorner),

            HandlePosition.BottomRight =>
                new Cursor(StandardCursorType.BottomRightCorner),

            HandlePosition.Top =>
                new Cursor(StandardCursorType.TopSide),

            HandlePosition.Bottom =>
                new Cursor(StandardCursorType.BottomSide),

            HandlePosition.Left =>
                new Cursor(StandardCursorType.LeftSide),

            HandlePosition.Right =>
                new Cursor(StandardCursorType.RightSide),

            _ =>
                new Cursor(StandardCursorType.Arrow)
        };
    }

    private void ElementPointerPressed(
        object? sender,
        PointerPressedEventArgs e)
    {

        if (sender is not Control control)
            return;

        if (control.Tag is not SlideElement element)
            return;

        Focus();

        if (e.ClickCount == 2 &&
            element is TextElement textElement)
        {
            BeginTextEditing(textElement);

            e.Handled = true;
            return;
        }

        Editor!.SelectedElement = element;

        // Commence immédiatement le déplacement.
        _draggedElement = element;

        _dragStart =
            e.GetPosition(EditorCanvas);

        _startX =
            element.X;

        _startY =
            element.Y;

        _isResizing = false;

        e.Handled = true;
    }

    private void HandlePointerPressed(
        object? sender,
        PointerPressedEventArgs e)
    {
        if (sender is not Control control ||
            control.Tag is not HandleData data)
        {
            return;
        }

        Editor!.SelectedElement =
            data.Element;

        _draggedElement =
            data.Element;

        _resizeHandle =
            ConvertHandle(data.Position);

        _isResizing = true;

        _dragStart =
            e.GetPosition(EditorCanvas);

        _startWidth =
            data.Element.Width;

        _startHeight =
            data.Element.Height;

        _startElementX =
            data.Element.X;

        _startElementY =
            data.Element.Y;

        Focus();

        e.Handled = true;
    }

    private static ResizeHandle ConvertHandle(
        HandlePosition position)
    {
        return position switch
        {
            HandlePosition.TopLeft =>
                ResizeHandle.TopLeft,

            HandlePosition.Top =>
                ResizeHandle.Top,

            HandlePosition.TopRight =>
                ResizeHandle.TopRight,

            HandlePosition.Left =>
                ResizeHandle.Left,

            HandlePosition.Right =>
                ResizeHandle.Right,

            HandlePosition.BottomLeft =>
                ResizeHandle.BottomLeft,

            HandlePosition.Bottom =>
                ResizeHandle.Bottom,

            _ =>
                ResizeHandle.BottomRight
        };
    }

    private void CanvasPointerPressed(
        object? sender,
        PointerPressedEventArgs e)
    {
        if (e.Source == EditorCanvas)
        {
            if (Editor != null)
                Editor.SelectedElement = null;
        }

        Focus();
    }

    private void CanvasPointerMoved(
        object? sender,
        PointerEventArgs e)
    {
        if (_draggedElement == null)
            return;

        Point current =
            e.GetPosition(EditorCanvas);

        double deltaX =
            current.X - _dragStart.X;

        double deltaY =
            current.Y - _dragStart.Y;

        if (_isResizing)
        {
            ResizeElement(
                deltaX,
                deltaY);
        }
        else
        {
            _draggedElement.X =
                Math.Max(
                    0,
                    _startX + deltaX);

            _draggedElement.Y =
                Math.Max(
                    0,
                    _startY + deltaY);
        }

        // Mise à jour uniquement du contrôle existant.
        UpdateSelectedElementVisual();

        e.Handled = true;
    }

    private void UpdateSelectedElementVisual()
    {
        if (Editor?.SelectedElement == null)
            return;

        SlideElement element =
            Editor.SelectedElement;

        Control? elementControl = null;

        foreach (Control control in EditorCanvas.Children)
        {
            if (control.Tag is SlideElement controlElement &&
                controlElement.Id == element.Id)
            {
                elementControl = control;
                break;
            }
        }

        if (elementControl == null)
            return;

        // Déplacement / redimensionnement du vrai contrôle.
        Canvas.SetLeft(
            elementControl,
            element.X);

        Canvas.SetTop(
            elementControl,
            element.Y);

        elementControl.Width =
            element.Width;

        elementControl.Height =
            element.Height;

        elementControl.Opacity =
            element.Opacity;

        UpdateSelectionVisual();
    }

    private void UpdateSelectionVisual()
    {
        if (Editor?.SelectedElement == null)
            return;

        SlideElement element =
            Editor.SelectedElement;

        foreach (Control control in EditorCanvas.Children)
        {
            if (control.Tag is string tag &&
                tag == "SelectionBorder")
            {
                Canvas.SetLeft(
                    control,
                    element.X - 3);

                Canvas.SetTop(
                    control,
                    element.Y - 3);

                control.Width =
                    element.Width + 6;

                control.Height =
                    element.Height + 6;
            }

            if (control.Tag is HandleData handleData &&

                handleData.Element.Id == element.Id)
            {
                UpdateHandlePosition(
                    control,
                    element,
                    handleData.Position);
            }


            if (control.Tag is string toolbarTag &&
            toolbarTag == "TextToolbar" &&
            element is TextElement text)
            {
                UpdateTextToolbarPosition(
                    control,
                    text);
            }
        }
    }


private void UpdateTextToolbarPosition(
    Control toolbar,
    TextElement text)
    {
        toolbar.Measure(new Size(
            double.PositiveInfinity,
            double.PositiveInfinity));

        double toolbarWidth =
            toolbar.DesiredSize.Width;

        double toolbarHeight =
            toolbar.DesiredSize.Height;

        double x =
            text.X +
            (text.Width - toolbarWidth) / 2;

        double y =
            text.Y -
            toolbarHeight -
            12;

        if (x < 5)
            x = 5;

        if (x + toolbarWidth > 1920)
            x = 1920 - toolbarWidth - 5;

        if (y < 5)
        {
            y =
                text.Y +
                text.Height +
                12;
        }

        Canvas.SetLeft(toolbar, x);
        Canvas.SetTop(toolbar, y);
    }



    private static void UpdateHandlePosition(
    Control handle,
    SlideElement element,
    HandlePosition position)
    {
        double x;
        double y;

        switch (position)
        {
            case HandlePosition.TopLeft:
                x = element.X - 7;
                y = element.Y - 7;
                break;

            case HandlePosition.Top:
                x = element.X +
                    element.Width / 2 - 7;
                y = element.Y - 7;
                break;

            case HandlePosition.TopRight:
                x = element.X +
                    element.Width - 7;
                y = element.Y - 7;
                break;

            case HandlePosition.Left:
                x = element.X - 7;
                y = element.Y +
                    element.Height / 2 - 7;
                break;

            case HandlePosition.Right:
                x = element.X +
                    element.Width - 7;
                y = element.Y +
                    element.Height / 2 - 7;
                break;

            case HandlePosition.BottomLeft:
                x = element.X - 7;
                y = element.Y +
                    element.Height - 7;
                break;

            case HandlePosition.Bottom:
                x = element.X +
                    element.Width / 2 - 7;
                y = element.Y +
                    element.Height - 7;
                break;

            default:
                x = element.X +
                    element.Width - 7;
                y = element.Y +
                    element.Height - 7;
                break;
        }

        Canvas.SetLeft(handle, x);
        Canvas.SetTop(handle, y);
    }

    private void ResizeElement(
        double deltaX,
        double deltaY)
    {
        if (_draggedElement == null)
            return;

        const double minimumSize = 30;

        double x = _startElementX;
        double y = _startElementY;

        double width = _startWidth;
        double height = _startHeight;

        switch (_resizeHandle)
        {
            case ResizeHandle.Left:

                x = _startElementX + deltaX;

                width =
                    _startWidth - deltaX;

                break;

            case ResizeHandle.Right:

                width =
                    _startWidth + deltaX;

                break;

            case ResizeHandle.Top:

                y = _startElementY + deltaY;

                height =
                    _startHeight - deltaY;

                break;

            case ResizeHandle.Bottom:

                height =
                    _startHeight + deltaY;

                break;

            case ResizeHandle.TopLeft:

                x = _startElementX + deltaX;
                y = _startElementY + deltaY;

                width =
                    _startWidth - deltaX;

                height =
                    _startHeight - deltaY;

                break;

            case ResizeHandle.TopRight:

                y = _startElementY + deltaY;

                width =
                    _startWidth + deltaX;

                height =
                    _startHeight - deltaY;

                break;

            case ResizeHandle.BottomLeft:

                x = _startElementX + deltaX;

                width =
                    _startWidth - deltaX;

                height =
                    _startHeight + deltaY;

                break;

            case ResizeHandle.BottomRight:

                width =
                    _startWidth + deltaX;

                height =
                    _startHeight + deltaY;

                break;
        }

        if (width < minimumSize)
        {
            width = minimumSize;

            if (_resizeHandle is
                ResizeHandle.Left or
                ResizeHandle.TopLeft or
                ResizeHandle.BottomLeft)
            {
                x =
                    _startElementX +
                    _startWidth -
                    minimumSize;
            }
        }

        if (height < minimumSize)
        {
            height = minimumSize;

            if (_resizeHandle is
                ResizeHandle.Top or
                ResizeHandle.TopLeft or
                ResizeHandle.TopRight)
            {
                y =
                    _startElementY +
                    _startHeight -
                    minimumSize;
            }
        }

        _draggedElement.X =
            Math.Max(0, x);

        _draggedElement.Y =
            Math.Max(0, y);

        _draggedElement.Width =
            width;

        _draggedElement.Height =
            height;
    }

    private void CanvasPointerReleased(
        object? sender,
        PointerReleasedEventArgs e)
    {
        _draggedElement = null;
        _isResizing = false;
    }

    private void OnKeyDown(
        object? sender,
        KeyEventArgs e)
    {
        if (e.Key == Key.Delete)
        {
            Editor?.DeleteSelectedCommand.Execute(null);

            e.Handled = true;
        }

        if (e.Key == Key.Escape)
        {
            if (Editor != null)
                Editor.SelectedElement = null;

            e.Handled = true;
        }
    }

    private sealed class HandleData
    {
        public required SlideElement Element { get; init; }

        public required HandlePosition Position { get; init; }
    }

    private enum HandlePosition
    {
        TopLeft,
        Top,
        TopRight,
        Left,
        Right,
        BottomLeft,
        Bottom,
        BottomRight
    }

    private enum ResizeHandle
    {
        TopLeft,
        Top,
        TopRight,
        Left,
        Right,
        BottomLeft,
        Bottom,
        BottomRight
    }

    private void BeginTextEditing(TextElement textElement)
    {
        if (_textEditor != null)
            EndTextEditing(true);

        _editingTextElement = textElement;
        _originalText = textElement.Text;

        _textEditor = new TextBox
        {
            Text = textElement.Text,
            FontFamily = new FontFamily(textElement.FontFamily),
            FontSize = textElement.FontSize,
            Background = Brushes.Transparent,
            Foreground = Brushes.Black,
   
            BorderThickness = new Avalonia.Thickness(1),
            BorderBrush = Brushes.DeepSkyBlue,
            Padding = new Avalonia.Thickness(4),
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            HorizontalContentAlignment = ConvertHorizontalAlignment(
                textElement.HorizontalAlignment),
            VerticalContentAlignment = ConvertVerticalAlignment(
                textElement.VerticalAlignment)
        };

        if (textElement.IsBold)
            _textEditor.FontWeight = FontWeight.Bold;

        if (textElement.IsItalic)
            _textEditor.FontStyle = FontStyle.Italic;

        Canvas.SetLeft(_textEditor, textElement.X);
        Canvas.SetTop(_textEditor, textElement.Y);

        _textEditor.Width = textElement.Width;
        _textEditor.Height = textElement.Height;

        EditorCanvas.Children.Add(_textEditor);

        _textEditor.KeyDown += TextEditorKeyDown;
        _textEditor.LostFocus += TextEditorLostFocus;

        _textEditor.Focus();
        _textEditor.SelectAll();
    }

    private void TextEditorKeyDown(
    object? sender,
    KeyEventArgs e)
    {
        if (_editingTextElement == null ||
            _textEditor == null)
            return;

        if (e.Key == Key.Escape)
        {
            EndTextEditing(false);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Enter &&
            e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            EndTextEditing(true);
            e.Handled = true;
        }
    }

    private void EndTextEditing(bool save)
    {
        if (_textEditor == null ||
            _editingTextElement == null)
            return;

        if (save)
        {
            _editingTextElement.Text =
                _textEditor.Text ?? string.Empty;
        }
        else
        {
            _editingTextElement.Text =
                _originalText;
        }

        _textEditor.KeyDown -= TextEditorKeyDown;
        _textEditor.LostFocus -= TextEditorLostFocus;

        EditorCanvas.Children.Remove(_textEditor);

        _textEditor = null;
        _editingTextElement = null;
        _originalText = string.Empty;

        Editor?.RefreshSelectedProperties();

        RenderCanvas();
    }

    private void TextEditorLostFocus(
    object? sender,
    Avalonia.Interactivity.RoutedEventArgs e)
    {
        EndTextEditing(true);
    }

    private static HorizontalAlignment ConvertHorizontalAlignment(
    string alignment)
    {
        return alignment switch
        {
            "Left" => HorizontalAlignment.Left,
            "Right" => HorizontalAlignment.Right,
            "Center" => HorizontalAlignment.Center,
            "Stretch" => HorizontalAlignment.Stretch,
            _ => HorizontalAlignment.Center
        };
    }


    private static VerticalAlignment ConvertVerticalAlignment(
        string alignment)
    {
        return alignment switch
        {
            "Top" => VerticalAlignment.Top,
            "Bottom" => VerticalAlignment.Bottom,
            "Center" => VerticalAlignment.Center,
            "Stretch" => VerticalAlignment.Stretch,
            _ => VerticalAlignment.Center
        };
    }

    private void CanvasKeyDown(
    object? sender,
    KeyEventArgs e)
    {
        if (Editor == null)
            return;

        if (_textEditor != null)
            return;

        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            switch (e.Key)
            {
                case Key.C:
                    Editor.CopySelectedCommand.Execute(null);
                    e.Handled = true;
                    return;

                case Key.V:
                    Editor.PasteCommand.Execute(null);
                    e.Handled = true;
                    return;

                case Key.D:
                    Editor.DuplicateSelectedCommand.Execute(null);
                    e.Handled = true;
                    return;
            }
        }

        if (e.Key == Key.Delete)
        {
            Editor.DeleteSelectedCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void CanvasDragOver(
    object? sender,
    DragEventArgs e)
    {
        if (e.Data.Contains(
                "StageFlow.MediaAsset"))
        {
            e.DragEffects =
                DragDropEffects.Copy;
        }
        else
        {
            e.DragEffects =
                DragDropEffects.None;
        }

        e.Handled = true;
    }

    private void CanvasDrop(
    object? sender,
    DragEventArgs e)
    {
        if (Editor == null)
            return;

        if (!e.Data.Contains(
                "StageFlow.MediaAsset"))
        {
            return;
        }

        string? assetIdText =
            e.Data.Get(
                "StageFlow.MediaAsset")
            as string;

        if (!Guid.TryParse(
                assetIdText,
                out Guid assetId))
        {
            return;
        }

        MediaAsset? asset =
            Editor.MainViewModel.MediaLibrary
                .Assets
                .FirstOrDefault(
                    x => x.Id == assetId);

        if (asset == null)
            return;

        Slide? slide =
            Editor.CurrentSlide;

        if (slide == null)
            return;

        Point dropPoint =
            e.GetPosition(
                EditorCanvas);

        double x =
            Math.Max(
                0,
                dropPoint.X);

        double y =
            Math.Max(
                0,
                dropPoint.Y);

        switch (asset.Type)
        {
            case MediaAssetType.Image:

                AddImageFromAsset(
                    asset,
                    slide,
                    x,
                    y);

                break;

            case MediaAssetType.Video:

                AddVideoFromAsset(
                    asset,
                    slide,
                    x,
                    y);

                break;

            case MediaAssetType.Audio:

                Editor.MainViewModel.StatusText =
                    "Audio cannot be placed visually on the slide.";

                break;
        }

        e.Handled = true;
    }

    private void AddImageFromAsset(
    MediaAsset asset,
    Slide slide,
    double x,
    double y)
    {
        var image =
            new ImageElement
            {
                Name = asset.Name,

                Source =
                    asset.RelativePath,

                X = x - 400,
                Y = y - 225,

                Width = 800,
                Height = 450,

                StretchUniform = true,

                MaintainAspectRatio = true,

                ZIndex =
                    slide.Elements.Count
            };

        image.X =
            Math.Clamp(
                image.X,
                0,
                1920 - image.Width);

        image.Y =
            Math.Clamp(
                image.Y,
                0,
                1080 - image.Height);

        slide.Elements.Add(
            image);

        Editor.SelectedElement =
            image;

        Editor.RefreshSelectedProperties();

        RenderCanvas();

        Editor.MainViewModel.StatusText =
            $"{asset.Name} added to slide.";
    }

    private void AddVideoFromAsset(
    MediaAsset asset,
    Slide slide,
    double x,
    double y)
    {
        var video =
            new VideoElement
            {
                Name = asset.Name,

                Source =
                    asset.RelativePath,

                X = x - 400,
                Y = y - 225,

                Width = 800,
                Height = 450,

                AutoPlay = false,

                Loop = false,

                Volume = 1.0,

                ZIndex =
                    slide.Elements.Count
            };

        video.X =
            Math.Clamp(
                video.X,
                0,
                1920 - video.Width);

        video.Y =
            Math.Clamp(
                video.Y,
                0,
                1080 - video.Height);

        slide.Elements.Add(
            video);

        Editor.SelectedElement =
            video;

        Editor.RefreshSelectedProperties();

        RenderCanvas();

        Editor.MainViewModel.StatusText =
            $"{asset.Name} added to slide.";
    }

    private string ResolveImagePath(
    ImageElement image)
    {
        if (Editor == null)
            return image.Source;

        if (string.IsNullOrWhiteSpace(
                Editor.MainViewModel.CurrentFilePath))
        {
            return image.Source;
        }

        var assetService =
            new AssetService();

        return assetService.ResolveAssetPath(
            image.Source,
            Editor.MainViewModel.CurrentFilePath);
    }



}