using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StageFlow.Models;
using StageFlow.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using static System.Net.WebRequestMethods;



namespace StageFlow.ViewModels
{
    public partial class MediaLibraryViewModel : ObservableObject
    {
        private readonly MainWindowViewModel _mainViewModel;

        private readonly AssetService _assetService = new();

        [ObservableProperty]
        private MediaAsset? selectedAsset;

        [ObservableProperty]
        private string filter = string.Empty;

        public ObservableCollection<MediaAsset> Assets { get; } = new();

        public IEnumerable<MediaAsset> FilteredAssets =>
            string.IsNullOrWhiteSpace(Filter)
                ? Assets
                : Assets.Where(asset =>
                    asset.Name.Contains(
                        Filter,
                        StringComparison.OrdinalIgnoreCase));

        public bool HasSelectedAsset =>
            SelectedAsset != null;

        public string SelectedAssetName =>
            SelectedAsset?.Name ?? "No media selected";

        public string SelectedAssetType =>
            SelectedAsset?.Type.ToString() ?? "-";

        public string SelectedAssetSize =>
            SelectedAsset?.FileSizeText ?? "-";

        public string SelectedAssetExtension =>
            SelectedAsset?.Extension ?? "-";

        public MediaLibraryViewModel(
            MainWindowViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            Refresh();

            _mainViewModel.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName ==
                    nameof(MainWindowViewModel.CurrentFilePath))
                {
                    Refresh();
                }
            };
        }

        partial void OnFilterChanged(string value)
        {
            OnPropertyChanged(
                nameof(FilteredAssets));
        }

        partial void OnSelectedAssetChanged(
            MediaAsset? value)
        {
            OnPropertyChanged(
                nameof(HasSelectedAsset));

            OnPropertyChanged(
                nameof(SelectedAssetName));

            OnPropertyChanged(
                nameof(SelectedAssetType));

            OnPropertyChanged(
                nameof(SelectedAssetSize));

            OnPropertyChanged(
                nameof(SelectedAssetExtension));

            OnPropertyChanged(
    nameof(SelectedAssetAbsolutePath));
        }

        public void Refresh()
        {
            Assets.Clear();

            foreach (
     MediaAsset asset
     in _mainViewModel.Document.MediaAssets)
            {
                if (!string.IsNullOrWhiteSpace(
                        _mainViewModel.CurrentFilePath))
                {
                    asset.AbsolutePath =
                        _assetService.ResolveAssetPath(
                            asset.RelativePath,
                            _mainViewModel.CurrentFilePath);
                }

                Assets.Add(asset);
            }

            OnPropertyChanged(
                nameof(FilteredAssets));
        }

        [RelayCommand]
        private void SelectAsset(MediaAsset? asset)
        {
            SelectedAsset = asset;
        }

        [RelayCommand]
        private async Task ImportMedia()
        {
            if (string.IsNullOrWhiteSpace(
                    _mainViewModel.CurrentFilePath))
            {
                _mainViewModel.StatusText =
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
                            Title = "Import media",
                            AllowMultiple = true,

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
                            },

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
                            },

                            new FilePickerFileType("Audio")
                            {
                                Patterns =
                                [
                                    "*.mp3",
                                    "*.wav",
                                    "*.flac",
                                    "*.aac",
                                    "*.ogg",
                                    "*.m4a"
                                ]
                            }
                            ]
                        });

            int importedCount = 0;

            foreach (IStorageFile file in files)
            {
                string? sourcePath =
                    file.TryGetLocalPath();

                if (string.IsNullOrWhiteSpace(sourcePath))
                    continue;

                try
                {
                    MediaAsset asset =
                        await _assetService.ImportAssetAsync(
                            sourcePath,
                            _mainViewModel.CurrentFilePath);

                    bool exists =
                        _mainViewModel.Document.MediaAssets
                            .Any(existing =>
                                existing.RelativePath ==
                                asset.RelativePath);

                    if (exists)
                        continue;

                    _mainViewModel.Document.MediaAssets
                        .Add(asset);

                    Assets.Add(asset);

                    importedCount++;
                }
                catch (Exception ex)
                {
                    _mainViewModel.StatusText =
                        $"Unable to import {file.Name}: {ex.Message}";
                }
            }

            _mainViewModel.StatusText =
                $"{importedCount} media imported.";

            OnPropertyChanged(
                nameof(FilteredAssets));
        }

        [RelayCommand]
        private void DeleteSelected()
        {
            if (SelectedAsset == null)
                return;

            if (string.IsNullOrWhiteSpace(
                    _mainViewModel.CurrentFilePath))
                return;

            try
            {
                _assetService.DeleteAssetFile(
                    SelectedAsset,
                    _mainViewModel.CurrentFilePath);

                _mainViewModel.Document.MediaAssets
                    .Remove(SelectedAsset);

                Assets.Remove(SelectedAsset);

                SelectedAsset = null;

                _mainViewModel.StatusText =
                    "Media deleted.";
            }
            catch (Exception ex)
            {
                _mainViewModel.StatusText =
                    $"Unable to delete media: {ex.Message}";
            }
        }

        [RelayCommand]
        private void InsertSelected()
        {
            if (SelectedAsset == null)
                return;

            Slide? slide =
                _mainViewModel.SelectedSlide;

            if (slide == null)
            {
                _mainViewModel.StatusText =
                    "Select a slide first.";

                return;
            }

            switch (SelectedAsset.Type)
            {
                case MediaAssetType.Image:

                    var image =
                        new ImageElement
                        {
                            Name =
                                SelectedAsset.Name,

                            Source =
                                SelectedAsset.RelativePath,

                            X = 300,
                            Y = 200,

                            Width = 800,
                            Height = 450,

                            StretchUniform = true,

                            MaintainAspectRatio = true,

                            ZIndex =
                                slide.Elements.Count
                        };

                    slide.Elements.Add(image);

                    _mainViewModel.Editor
                        .SelectedElement = image;

                    break;

                case MediaAssetType.Video:

                    var video =
                        new VideoElement
                        {
                            Name =
                                SelectedAsset.Name,

                            Source =
                                SelectedAsset.RelativePath,

                            X = 300,
                            Y = 200,

                            Width = 800,
                            Height = 450,

                            AutoPlay = true,

                            Loop = false,

                            Volume = 1,

                            ZIndex =
                                slide.Elements.Count
                        };

                    slide.Elements.Add(video);

                    _mainViewModel.Editor
                        .SelectedElement = video;

                    break;

                case MediaAssetType.Audio:

                    _mainViewModel.StatusText =
                        "Audio imported. Audio playback engine will be connected later.";

                    return;
            }

            _mainViewModel.Editor
                .RefreshSelectedProperties();

            _mainViewModel.StatusText =
                $"{SelectedAsset.Name} inserted.";
        }

        public string? SelectedAssetAbsolutePath
        {
            get
            {
                if (SelectedAsset == null ||
                    string.IsNullOrWhiteSpace(
                        _mainViewModel.CurrentFilePath))
                {
                    return null;
                }

                return _assetService.ResolveAssetPath(
                    SelectedAsset.RelativePath,
                    _mainViewModel.CurrentFilePath);
            }
        }

        public string GetAssetAbsolutePath(MediaAsset asset)
        {
            if (string.IsNullOrWhiteSpace(
                    _mainViewModel.CurrentFilePath))
            {
                return string.Empty;
            }

            return _assetService.ResolveAssetPath(
                asset.RelativePath,
                _mainViewModel.CurrentFilePath);
        }

        public bool IsImage(MediaAsset asset)
        {
            return asset.Type == MediaAssetType.Image;
        }
    }
}
