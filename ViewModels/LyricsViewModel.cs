using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using StageFlow.Models;
using StageFlow.Services;

namespace StageFlow.ViewModels
{
    public partial class LyricsViewModel : ObservableObject
    {
        private readonly MainWindowViewModel
        _mainViewModel;

        private readonly LyricsService
            _lyricsService;

        public Window? HostWindow { get; set; }

        [ObservableProperty]
        private string songTitle = string.Empty;

        [ObservableProperty]
        private string artist = string.Empty;

        [ObservableProperty]
        private string lyricsText = string.Empty;

        [ObservableProperty]
        private int linesPerSlide = 4;

        [ObservableProperty]
        private LyricsDocument? currentDocument;

        public ObservableCollection<LyricsSection>
            Sections
        { get; } = new();

        public int SlideCount
        {
            get
            {
                if (CurrentDocument == null)
                    return 0;

                return CurrentDocument.Sections
                    .Sum(section =>
                        (int)Math.Ceiling(
                            section.Lines.Count /
                            (double)Math.Max(
                                1,
                                LinesPerSlide)));
            }
        }

        public bool HasLyrics =>
            CurrentDocument != null &&
            CurrentDocument.Sections.Count > 0;

        public LyricsViewModel(
            MainWindowViewModel mainViewModel)
        {
            _mainViewModel =
                mainViewModel;

            _lyricsService =
                new LyricsService();
        }

        [RelayCommand]
        private async Task ImportLyrics()
        {
            Window? window =
                GetMainWindow();

            if (window == null)
                return;

            IReadOnlyList<
                IStorageFile> files =
                await window.StorageProvider
                    .OpenFilePickerAsync(
                        new FilePickerOpenOptions
                        {
                            Title =
                                "Import Lyrics",

                            AllowMultiple = false,

                            FileTypeFilter =
                            [
                                new FilePickerFileType(
                                "Lyrics")
                            {
                                Patterns =
                                [
                                    "*.txt",
                                    "*.lrc"
                                ]
                            }
                            ]
                        });

            IStorageFile? file =
                files.FirstOrDefault();

            if (file == null)
                return;

            string? path =
                file.TryGetLocalPath();

            if (string.IsNullOrWhiteSpace(path))
            {
                _mainViewModel.SetStatus(
                    "Unable to access lyrics file.");

                return;
            }

            try
            {
                LyricsDocument document =
                    await _lyricsService
                        .ImportAsync(path);

                LoadDocument(document);

                _mainViewModel.SetStatus(
                    $"Lyrics imported: {document.Title}");
            }
            catch (Exception ex)
            {
                _mainViewModel.SetStatus(
                    $"Lyrics import failed: {ex.Message}");
            }
        }

        [RelayCommand]
        private void ParseLyrics()
        {
            if (string.IsNullOrWhiteSpace(
                LyricsText))
            {
                _mainViewModel.SetStatus(
                    "Enter lyrics first.");

                return;
            }

            LyricsDocument document =
                _lyricsService.Parse(
                    LyricsText,
                    string.IsNullOrWhiteSpace(
                        SongTitle)
                        ? "Untitled Song"
                        : SongTitle);

            document.Artist =
                Artist;

            LoadDocument(document);

            _mainViewModel.SetStatus(
                $"Lyrics parsed: {SlideCount} slides.");
        }

        [RelayCommand]
        private void CreateSlides()
        {
            if (CurrentDocument == null)
            {
                ParseLyrics();

                if (CurrentDocument == null)
                    return;
            }

            PresentationTheme theme =
                GetCurrentTheme();

            List<Slide> slides =
                _lyricsService.CreateSlides(
                    CurrentDocument,
                    theme,
                    Math.Max(
                        1,
                        LinesPerSlide));

            if (slides.Count == 0)
            {
                _mainViewModel.SetStatus(
                    "No lyrics slides could be created.");

                return;
            }

            foreach (Slide slide in slides)
            {
                _mainViewModel.Slides.Add(slide);
            }

            _mainViewModel.RefreshSlides();

            _mainViewModel.SelectedSlide =
                slides.First();

            _mainViewModel.SetStatus(
                $"{slides.Count} lyrics slides added.");
        }

        [RelayCommand]
        private void ClearLyrics()
        {
            SongTitle = string.Empty;
            Artist = string.Empty;
            LyricsText = string.Empty;
            CurrentDocument = null;

            Sections.Clear();

            OnPropertyChanged(
                nameof(HasLyrics));

            OnPropertyChanged(
                nameof(SlideCount));

            _mainViewModel.SetStatus(
                "Lyrics cleared.");
        }

        partial void OnLinesPerSlideChanged(
            int value)
        {
            OnPropertyChanged(
                nameof(SlideCount));
        }

        private void LoadDocument(
            LyricsDocument document)
        {
            CurrentDocument =
                document;

            SongTitle =
                document.Title;

            Artist =
                document.Artist;

            Sections.Clear();

            foreach (
                LyricsSection section
                in document.Sections)
            {
                Sections.Add(section);
            }

            LyricsText =
                string.Join(
                    Environment.NewLine +
                    Environment.NewLine,
                    document.Sections.Select(
                        section =>
                            $"[{section.Name}]" +
                            Environment.NewLine +
                            string.Join(
                                Environment.NewLine,
                                section.Lines)));

            OnPropertyChanged(
                nameof(HasLyrics));

            OnPropertyChanged(
                nameof(SlideCount));
        }

        private PresentationTheme
            GetCurrentTheme()
        {
            string themeName =
                _mainViewModel
                    .Document
                    .Settings
                    .ThemeName;

            return PresentationTheme
                .CreateBuiltInThemes()
                .FirstOrDefault(
                    theme =>
                        string.Equals(
                            theme.Name,
                            themeName,
                            StringComparison
                                .OrdinalIgnoreCase))
                ?? PresentationTheme
                    .CreateBuiltInThemes()
                    .First();
        }

        

        private Window? GetMainWindow()
        {
            return HostWindow;
        }
    }
}
