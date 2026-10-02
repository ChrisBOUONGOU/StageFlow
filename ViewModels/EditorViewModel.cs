using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StageFlow.Models;
using StageFlow.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace StageFlow.ViewModels
{
    public partial class EditorViewModel : ObservableObject
    {
        private readonly MainWindowViewModel _mainViewModel;

        [ObservableProperty]
        private SlideElement? selectedElement;

        private SlideElement? _clipboardElement;

        [ObservableProperty]
        private string statusMessage = string.Empty;

        [ObservableProperty]
        private bool hasSelectedSlide;

        [ObservableProperty]
        private TransitionType selectedTransitionType = TransitionType.Cut;

        [ObservableProperty]
        private double selectedTransitionDuration = 0.5;

        private readonly AssetService _assetService = new();


        public MainWindowViewModel MainViewModel =>
    _mainViewModel;

        public bool IsVideoElement =>
    SelectedElement is VideoElement;

        public string SelectedVideoSource
        {
            get =>
                SelectedElement is VideoElement video
                    ? video.Source
                    : string.Empty;
        }

        public bool SelectedAutoPlay
        {
            get =>
                SelectedElement is VideoElement video &&
                video.AutoPlay;

            set
            {
                if (SelectedElement is not VideoElement video)
                    return;

                video.AutoPlay = value;

                OnPropertyChanged();
            }
        }

        public bool SelectedLoop
        {
            get =>
                SelectedElement is VideoElement video &&
                video.Loop;

            set
            {
                if (SelectedElement is not VideoElement video)
                    return;

                video.Loop = value;

                OnPropertyChanged();

                MainViewModel.NotifyVideoLoopChanged(true);
            }
        }

        public double SelectedVideoVolume
        {
            get =>
                SelectedElement is VideoElement video
                    ? video.Volume
                    : 1.0;

            set
            {
                if (SelectedElement is not VideoElement video)
                    return;

                video.Volume =
                    Math.Clamp(value, 0, 1);

                OnPropertyChanged();

                MainViewModel.NotifyVideoVolumeChanged(
                    video.Volume);
            }
        }



        public EditorViewModel(MainWindowViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            _mainViewModel.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(MainWindowViewModel.SelectedSlide))
                {
                    SelectedElement = null;

                    OnPropertyChanged(nameof(CurrentSlide));
                    OnPropertyChanged(nameof(CurrentElements));

                    OnPropertyChanged(nameof(HasSelectedSlide));
                    OnPropertyChanged(nameof(SelectedTransitionType));
                    OnPropertyChanged(nameof(SelectedTransitionDuration));
                }
            };
        }

        // =========================================================
        // CURRENT SLIDE
        // =========================================================

        public Slide? CurrentSlide =>
            _mainViewModel.SelectedSlide;

        public IEnumerable<SlideElement> CurrentElements =>
            CurrentSlide?.Elements ?? Enumerable.Empty<SlideElement>();


        // =========================================================
        // SELECTED ELEMENT
        // =========================================================

        partial void OnSelectedElementChanged(SlideElement? value)
        {
            NotifySelectedElementProperties();

            OnPropertyChanged(nameof(IsTextElement));
            OnPropertyChanged(nameof(IsVideoElement));

            OnPropertyChanged(nameof(SelectedText));
            OnPropertyChanged(nameof(SelectedFontFamily));
            OnPropertyChanged(nameof(SelectedFontSize));
            OnPropertyChanged(nameof(SelectedBold));
            OnPropertyChanged(nameof(SelectedItalic));
            OnPropertyChanged(nameof(SelectedColor));
            OnPropertyChanged(nameof(SelectedColorValue));
            OnPropertyChanged(nameof(SelectedHorizontalAlignment));
            OnPropertyChanged(nameof(SelectedVerticalAlignment));

           
            OnPropertyChanged(nameof(SelectedVideoSource));
            OnPropertyChanged(nameof(SelectedAutoPlay));
            OnPropertyChanged(nameof(SelectedLoop));
            OnPropertyChanged(nameof(SelectedVideoVolume));

            OnPropertyChanged(nameof(HasSelectedSlide));
            OnPropertyChanged(nameof(SelectedTransitionType));
            OnPropertyChanged(nameof(SelectedTransitionDuration));
        }

        public bool IsTextElement =>
            SelectedElement is TextElement;


        // =========================================================
        // POSITION
        // =========================================================

        public double SelectedX
        {
            get => SelectedElement?.X ?? 0;

            set
            {
                if (SelectedElement == null)
                    return;

                SelectedElement.X = value;

                OnPropertyChanged();
                RefreshCanvas();
            }
        }

        public double SelectedY
        {
            get => SelectedElement?.Y ?? 0;

            set
            {
                if (SelectedElement == null)
                    return;

                SelectedElement.Y = value;

                OnPropertyChanged();
                RefreshCanvas();
            }
        }


        // =========================================================
        // SIZE
        // =========================================================

        public double SelectedWidth
        {
            get => SelectedElement?.Width ?? 0;

            set
            {
                if (SelectedElement == null)
                    return;

                SelectedElement.Width = Math.Max(30, value);

                OnPropertyChanged();
                RefreshCanvas();
            }
        }

        public double SelectedHeight
        {
            get => SelectedElement?.Height ?? 0;

            set
            {
                if (SelectedElement == null)
                    return;

                SelectedElement.Height = Math.Max(30, value);

                OnPropertyChanged();
                RefreshCanvas();
            }
        }


        // =========================================================
        // TRANSFORM
        // =========================================================

        public double SelectedRotation
        {
            get => SelectedElement?.Rotation ?? 0;

            set
            {
                if (SelectedElement == null)
                    return;

                SelectedElement.Rotation = value;

                OnPropertyChanged();
                RefreshCanvas();
            }
        }

        public double SelectedOpacity
        {
            get => SelectedElement?.Opacity ?? 1;

            set
            {
                if (SelectedElement == null)
                    return;

                SelectedElement.Opacity =
                    Math.Clamp(value, 0, 1);

                OnPropertyChanged();
                RefreshCanvas();
            }
        }


        // =========================================================
        // TEXT
        // =========================================================

        public string SelectedText
        {
            get =>
                SelectedElement is TextElement text
                    ? text.Text
                    : string.Empty;

            set
            {
                if (SelectedElement is not TextElement text)
                    return;

                text.Text = value;

                OnPropertyChanged();
                RefreshCanvas();
            }
        }


        // =========================================================
        // FONT FAMILY
        // =========================================================

        public string SelectedFontFamily
        {
            get => SelectedElement is TextElement text
               ? text.FontFamily
               : "Inter";

            set
            {
                if (SelectedElement is not TextElement text)
                    return;

                if (string.IsNullOrWhiteSpace(value))
                    return;

                text.FontFamily = value;

                OnPropertyChanged(nameof(SelectedFontFamily));
                RefreshCanvas();
            }
        }

        public ObservableCollection<string> AvailableFonts { get; } = new()
{
    "Inter",
    "Arial",
    "Calibri",
    "Georgia",
    "Times New Roman",
    "Verdana"
};


        // =========================================================
        // FONT SIZE
        // =========================================================

        public double SelectedFontSize
        {
            get =>
                SelectedElement is TextElement text
                    ? text.FontSize
                    : 48;

            set
            {
                if (SelectedElement is not TextElement text)
                    return;

                text.FontSize = Math.Max(1, value);

                OnPropertyChanged();
                RefreshCanvas();
            }
        }


        // =========================================================
        // BOLD
        // =========================================================

        public bool SelectedBold
        {
            get =>
                SelectedElement is TextElement text &&
                text.IsBold;

            set
            {
                if (SelectedElement is not TextElement text)
                    return;

                text.IsBold = value;

                OnPropertyChanged();
                RefreshCanvas();
            }
        }


        // =========================================================
        // ITALIC
        // =========================================================

        public bool SelectedItalic
        {
            get =>
                SelectedElement is TextElement text &&
                text.IsItalic;

            set
            {
                if (SelectedElement is not TextElement text)
                    return;

                text.IsItalic = value;

                OnPropertyChanged();
                RefreshCanvas();
            }
        }


        // =========================================================
        // COLOR
        // =========================================================

        public string SelectedColor
        {
            get =>
                SelectedElement is TextElement text
                    ? text.Color
                    : "#FFFFFF";

            set
            {
                if (SelectedElement is not TextElement text)
                    return;

                if (string.IsNullOrWhiteSpace(value))
                    return;

                text.Color = value;

                OnPropertyChanged();
                RefreshCanvas();
            }
        }

        public Color SelectedColorValue
        {
            get
            {
                if (SelectedElement is not TextElement text)
                    return Colors.White;

                return Color.TryParse(text.Color, out var color)
                    ? color
                    : Colors.White;
            }

            set
            {
                if (SelectedElement is not TextElement text)
                    return;

                text.Color = $"#{value.R:X2}{value.G:X2}{value.B:X2}";

                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedColor));
                RefreshCanvas();
            }
        }


        // =========================================================
        // HORIZONTAL ALIGNMENT
        // =========================================================

        public string SelectedHorizontalAlignment
        {
            get =>
                SelectedElement is TextElement text
                    ? text.HorizontalAlignment
                    : "Center";

            set
            {
                if (SelectedElement is not TextElement text)
                    return;

                text.HorizontalAlignment = value;

                OnPropertyChanged();
                RefreshCanvas();
            }
        }


        // =========================================================
        // VERTICAL ALIGNMENT
        // =========================================================

        public string SelectedVerticalAlignment
        {
            get =>
                SelectedElement is TextElement text
                    ? text.VerticalAlignment
                    : "Center";

            set
            {
                if (SelectedElement is not TextElement text)
                    return;

                text.VerticalAlignment = value;

                OnPropertyChanged();
                RefreshCanvas();
            }
        }


        // =========================================================
        // ADD TEXT
        // =========================================================

        [RelayCommand]
        private void AddText()
        {
            if (CurrentSlide == null)
                return;

            var element = new TextElement
            {
                Text = "New Text",
                X = 300,
                Y = 350,
                Width = 900,
                Height = 120,
                FontSize = 52,
                Color = "#FFFFFF",
                HorizontalAlignment = "Center",
                VerticalAlignment = "Center",
                ZIndex = CurrentSlide.Elements.Count
            };

            CurrentSlide.Elements.Add(element);

            SelectedElement = element;

            RefreshCanvas();
        }


        // =========================================================
        // ADD RECTANGLE
        // =========================================================

        [RelayCommand]
        private void AddRectangle()
        {
            if (CurrentSlide == null)
                return;

            var element = new ShapeElement
            {
                ShapeType = "Rectangle",
                X = 500,
                Y = 400,
                Width = 600,
                Height = 300,
                Fill = "#30384A",
                Stroke = "#FFFFFF",
                StrokeThickness = 0,
                ZIndex = CurrentSlide.Elements.Count
            };

            CurrentSlide.Elements.Add(element);

            SelectedElement = element;

            RefreshCanvas();
        }


        // =========================================================
        // DELETE
        // =========================================================

        [RelayCommand]
        private void DeleteSelected()
        {
            if (CurrentSlide == null ||
                SelectedElement == null)
                return;

            CurrentSlide.Elements.Remove(SelectedElement);

            SelectedElement = null;

            RefreshCanvas();
        }


        // =========================================================
        // MOVE ELEMENT
        // =========================================================

        public void MoveElement(
            SlideElement element,
            double x,
            double y)
        {
            element.X = x;
            element.Y = y;

            RefreshSelectedProperties();
            RefreshCanvas();
        }


        // =========================================================
        // REFRESH
        // =========================================================

        private void NotifySelectedElementProperties()
        {
            OnPropertyChanged(nameof(SelectedX));
            OnPropertyChanged(nameof(SelectedY));

            OnPropertyChanged(nameof(SelectedWidth));
            OnPropertyChanged(nameof(SelectedHeight));

            OnPropertyChanged(nameof(SelectedRotation));
            OnPropertyChanged(nameof(SelectedOpacity));

            OnPropertyChanged(nameof(SelectedText));
            OnPropertyChanged(nameof(SelectedFontFamily));
            OnPropertyChanged(nameof(SelectedFontSize));

            OnPropertyChanged(nameof(SelectedBold));
            OnPropertyChanged(nameof(SelectedItalic));

            OnPropertyChanged(nameof(SelectedColor));
            OnPropertyChanged(nameof(SelectedColorValue));

            OnPropertyChanged(
                nameof(SelectedHorizontalAlignment));

            OnPropertyChanged(
                nameof(SelectedVerticalAlignment));

            OnPropertyChanged(nameof(IsVideoElement));
OnPropertyChanged(nameof(SelectedVideoSource));
OnPropertyChanged(nameof(SelectedAutoPlay));
OnPropertyChanged(nameof(SelectedLoop));
OnPropertyChanged(nameof(SelectedVideoVolume));

            OnPropertyChanged(nameof(HasSelectedSlide));
            OnPropertyChanged(nameof(SelectedTransitionType));
            OnPropertyChanged(nameof(SelectedTransitionDuration));
        }


        public void RefreshSelectedProperties()
        {
            NotifySelectedElementProperties();
        }


        public void RefreshCanvas()
        {
            OnPropertyChanged(nameof(CurrentElements));
            OnPropertyChanged(nameof(CurrentSlide));
        }

        [RelayCommand]
        private void SetHorizontalAlignment(string? alignment)
        {
            if (SelectedElement is not TextElement text)
                return;

            if (string.IsNullOrWhiteSpace(alignment))
                return;

            text.HorizontalAlignment = alignment;

            OnPropertyChanged(nameof(SelectedHorizontalAlignment));

            RefreshCanvas();
        }


        [RelayCommand]
        private void SetVerticalAlignment(string? alignment)
        {
            if (SelectedElement is not TextElement text)
                return;

            if (string.IsNullOrWhiteSpace(alignment))
                return;

            text.VerticalAlignment = alignment;

            OnPropertyChanged(nameof(SelectedVerticalAlignment));

            RefreshCanvas();
        }

        [RelayCommand]
        private void CopySelected()
        {
            if (SelectedElement == null)
                return;

            _clipboardElement = CloneElement(SelectedElement);

            StatusMessage = "Element copied";
        }


        [RelayCommand]
        private void Paste()
        {
            if (CurrentSlide == null ||
                _clipboardElement == null)
                return;

            var copy = CloneElement(_clipboardElement);

            copy.X += 30;
            copy.Y += 30;

            copy.ZIndex = CurrentSlide.Elements.Count;

            CurrentSlide.Elements.Add(copy);

            SelectedElement = copy;

            RefreshCanvas();

            StatusMessage = "Element pasted";
        }


        [RelayCommand]
        private void DuplicateSelected()
        {
            if (CurrentSlide == null ||
                SelectedElement == null)
                return;

            var copy = CloneElement(SelectedElement);

            copy.X += 30;
            copy.Y += 30;

            copy.ZIndex = CurrentSlide.Elements.Count;

            CurrentSlide.Elements.Add(copy);

            SelectedElement = copy;

            RefreshCanvas();

            StatusMessage = "Element duplicated";
        }

        private static SlideElement CloneElement(SlideElement element)
        {
            return element switch
            {
                TextElement text => new TextElement
                {
                    Name = text.Name,
                    X = text.X,
                    Y = text.Y,
                    Width = text.Width,
                    Height = text.Height,
                    Rotation = text.Rotation,
                    Opacity = text.Opacity,
                    ZIndex = text.ZIndex,
                    IsVisible = text.IsVisible,

                    Text = text.Text,
                    FontFamily = text.FontFamily,
                    FontSize = text.FontSize,
                    IsBold = text.IsBold,
                    IsItalic = text.IsItalic,
                    Color = text.Color,
                    HorizontalAlignment = text.HorizontalAlignment,
                    VerticalAlignment = text.VerticalAlignment,
                    LetterSpacing = text.LetterSpacing,
                    LineSpacing = text.LineSpacing
                },

                ImageElement image => new ImageElement
                {
                    Name = image.Name,
                    X = image.X,
                    Y = image.Y,
                    Width = image.Width,
                    Height = image.Height,
                    Rotation = image.Rotation,
                    Opacity = image.Opacity,
                    ZIndex = image.ZIndex,
                    IsVisible = image.IsVisible,

                    Source = image.Source,

                    StretchUniform =
        image.StretchUniform,

                    MaintainAspectRatio =
        image.MaintainAspectRatio
                },

                VideoElement video => new VideoElement
                {
                    Name = video.Name,
                    X = video.X,
                    Y = video.Y,
                    Width = video.Width,
                    Height = video.Height,
                    Rotation = video.Rotation,
                    Opacity = video.Opacity,
                    ZIndex = video.ZIndex,
                    IsVisible = video.IsVisible,

                    Source = video.Source,
                    Loop = video.Loop,
                    AutoPlay = video.AutoPlay,
                    Volume = video.Volume
                },

                ShapeElement shape => new ShapeElement
                {
                    Name = shape.Name,
                    X = shape.X,
                    Y = shape.Y,
                    Width = shape.Width,
                    Height = shape.Height,
                    Rotation = shape.Rotation,
                    Opacity = shape.Opacity,
                    ZIndex = shape.ZIndex,
                    IsVisible = shape.IsVisible,

                    ShapeType = shape.ShapeType,
                    Fill = shape.Fill,
                    Stroke = shape.Stroke,
                    StrokeThickness = shape.StrokeThickness
                },

                _ => throw new NotSupportedException(
                    $"Element type not supported: {element.GetType().Name}")
            };
        }

        [RelayCommand]
        private async Task AddImage()
        {
            if (CurrentSlide == null)
                return;

            if (string.IsNullOrWhiteSpace(
                    _mainViewModel.CurrentFilePath))
            {
                StatusMessage =
                    "Save the project before importing media.";

                return;
            }

            Window? window =
                App.Current?.ApplicationLifetime
                    is Avalonia.Controls
                        .ApplicationLifetimes
                        .IClassicDesktopStyleApplicationLifetime desktop
                    ? desktop.MainWindow
                    : null;

            if (window == null)
                return;

            IReadOnlyList<IStorageFile> files =
                await window.StorageProvider
                    .OpenFilePickerAsync(
                        new FilePickerOpenOptions
                        {
                            Title = "Select an image",
                            AllowMultiple = false,

                            FileTypeFilter =
                            [
                                new FilePickerFileType("Images")
                        {
                            Patterns =
                            [
                                "*.png",
                                "*.jpg",
                                "*.jpeg",
                                "*.webp",
                                "*.bmp",
                                "*.gif"
                            ]
                        }
                            ]
                        });

            if (files.Count == 0)
                return;

            string? sourcePath =
                files[0].TryGetLocalPath();

            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                StatusMessage =
                    "Unable to access the selected image.";

                return;
            }

            try
            {
                MediaAsset asset =
                    await _assetService.ImportAssetAsync(
                        sourcePath,
                        _mainViewModel.CurrentFilePath);

                _mainViewModel.Document.MediaAssets.Add(
                    asset);

                asset.AbsolutePath =
    _assetService.ResolveAssetPath(
        asset.RelativePath,
        _mainViewModel.CurrentFilePath);

                var image = new ImageElement
                {
                    Name = asset.Name,

                    Source = asset.RelativePath,

                    X = 300,
                    Y = 200,

                    Width = 800,
                    Height = 450,

                    StretchUniform = true,

                    MaintainAspectRatio = true,

                    ZIndex = CurrentSlide.Elements.Count
                };

                CurrentSlide.Elements.Add(image);

                SelectedElement = image;

                RefreshCanvas();

                StatusMessage =
                    "Image imported into project.";
            }
            catch (Exception ex)
            {
                StatusMessage =
                    $"Image import failed: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task AddVideo()
        {
            if (CurrentSlide == null)
                return;

            if (string.IsNullOrWhiteSpace(
                    _mainViewModel.CurrentFilePath))
            {
                StatusMessage =
                    "Save the project before importing media.";

                return;
            }

            Window? window =
                App.Current?.ApplicationLifetime
                    is Avalonia.Controls
                        .ApplicationLifetimes
                        .IClassicDesktopStyleApplicationLifetime desktop
                    ? desktop.MainWindow
                    : null;

            if (window == null)
                return;

            IReadOnlyList<IStorageFile> files =
                await window.StorageProvider
                    .OpenFilePickerAsync(
                        new FilePickerOpenOptions
                        {
                            Title = "Select a video",
                            AllowMultiple = false,

                            FileTypeFilter =
                            [
                                new FilePickerFileType("Videos")
                        {
                            Patterns =
                            [
                                "*.mp4",
                                "*.mov",
                                "*.avi",
                                "*.mkv",
                                "*.webm",
                                "*.m4v"
                            ]
                        }
                            ]
                        });

            if (files.Count == 0)
                return;

            string? sourcePath =
                files[0].TryGetLocalPath();

            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                StatusMessage =
                    "Unable to access the selected video.";

                return;
            }

            try
            {
                MediaAsset asset =
                    await _assetService.ImportAssetAsync(
                        sourcePath,
                        _mainViewModel.CurrentFilePath);

                bool alreadyExists =
                    _mainViewModel.Document.MediaAssets
                        .Any(existing =>
                            existing.RelativePath ==
                            asset.RelativePath);

                if (!alreadyExists)
                {
                    _mainViewModel.Document.MediaAssets.Add(asset);
                }

                asset.AbsolutePath =
                    _assetService.ResolveAssetPath(
                        asset.RelativePath,
                        _mainViewModel.CurrentFilePath);

                var video = new VideoElement
                {
                    Source = asset.RelativePath,

                    X = 0,
                    Y = 0,

                    Width = _mainViewModel.Document.Settings.Width,
                    Height = _mainViewModel.Document.Settings.Height,

                    AutoPlay = true,
                    Loop = false,
                    Volume = 1.0,

                    ZIndex = 0
                };

                CurrentSlide.Elements.Add(video);

                SelectedElement = video;

                RefreshCanvas();

                StatusMessage =
                    "Video imported into project.";
            }
            catch (Exception ex)
            {
                StatusMessage =
                    $"Video import failed: {ex.Message}";
            }
        }
    }
}
