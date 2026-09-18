using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Input;
using StageFlow.Models;
using StageFlow.Services;
using CommunityToolkit.Mvvm.ComponentModel;


namespace StageFlow.ViewModels
{
    public sealed class BibleViewModel : ObservableObject
    {
        private readonly MainWindowViewModel _mainViewModel;
        private readonly BibleService _bibleService;

        private BibleLanguage _selectedLanguage =
            BibleLanguage.French;

        private BibleTranslation? _selectedTranslation;

        private BibleBook? _selectedBook;

        private int _selectedChapter = 1;

        private BibleVerse? _selectedVerse;

        private string _searchText = string.Empty;

        private int _versesPerSlide = 2;

        private string _statusMessage =
            "Select a translation.";

        public ObservableCollection<BibleLanguage> Languages { get; }
            = new();

        public ObservableCollection<BibleTranslation> Translations { get; }
            = new();

        public ObservableCollection<BibleBook> Books { get; }
            = new();

        public ObservableCollection<BibleVerse> Verses { get; }
            = new();

        public ObservableCollection<BibleVerse> SearchResults { get; }
            = new();

        public BibleLanguage SelectedLanguage
        {
            get => _selectedLanguage;

            set
            {
                if (SetProperty(
                        ref _selectedLanguage,
                        value))
                {
                    _ = LoadTranslationsAsync();
                }
            }
        }

        public BibleTranslation? SelectedTranslation
        {
            get => _selectedTranslation;

            set
            {
                if (SetProperty(
                        ref _selectedTranslation,
                        value))
                {
                    _ = LoadBooksAsync();
                }
            }
        }

        public BibleBook? SelectedBook
        {
            get => _selectedBook;

            set
            {
                if (SetProperty(
                        ref _selectedBook,
                        value))
                {
                    SelectedChapter = 1;
                }
            }
        }

        public int SelectedChapter
        {
            get => _selectedChapter;

            set => SetProperty(
                ref _selectedChapter,
                Math.Max(1, value));
        }

        public BibleVerse? SelectedVerse
        {
            get => _selectedVerse;

            set => SetProperty(
                ref _selectedVerse,
                value);
        }

        public string SearchText
        {
            get => _searchText;

            set => SetProperty(
                ref _searchText,
                value);
        }

        public int VersesPerSlide
        {
            get => _versesPerSlide;

            set => SetProperty(
                ref _versesPerSlide,
                Math.Max(1, value));
        }

        public string StatusMessage
        {
            get => _statusMessage;

            private set => SetProperty(
                ref _statusMessage,
                value);
        }

        public ICommand LoadChapterCommand { get; }

        public ICommand SearchCommand { get; }

        public ICommand AddSelectedVerseCommand { get; }

        public ICommand AddChapterCommand { get; }

        public BibleViewModel(
            MainWindowViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            _bibleService = new BibleService();

            LoadChapterCommand =
                new AsyncRelayCommand(
                    LoadChapterAsync);

            SearchCommand =
                new AsyncRelayCommand(
                    SearchAsync);

            AddSelectedVerseCommand =
                new RelayCommand(
                    AddSelectedVerse);

            AddChapterCommand =
                new AsyncRelayCommand(
                    AddChapterAsync);

            foreach (var language in
                     Enum.GetValues<BibleLanguage>())
            {
                Languages.Add(language);
            }

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await LoadTranslationsAsync();
        }

        private async Task LoadTranslationsAsync()
        {
            try
            {
                StatusMessage =
                    "Loading translations...";

                var allTranslations =
                    await _bibleService
                        .GetTranslationsAsync();

                var translations =
                    allTranslations
                        .Where(t =>
                            t.Language ==
                            SelectedLanguage)
                        .ToList();

                Translations.Clear();

                foreach (var translation in translations)
                    Translations.Add(translation);

                SelectedTranslation =
                    Translations.FirstOrDefault();

                StatusMessage =
                    Translations.Count > 0
                        ? "Select a translation."
                        : "No translation available.";
            }
            catch (Exception ex)
            {
                StatusMessage =
                    $"Error : {ex.Message}";
            }
        }

        private async Task LoadBooksAsync()
        {
            if (SelectedTranslation == null)
                return;

            try
            {
                StatusMessage =
                    "Loading books...";

                var books =
                    await _bibleService
                        .GetBooksAsync(
                            SelectedTranslation);

                Books.Clear();

                foreach (var book in books)
                    Books.Add(book);

                SelectedBook =
                    Books.FirstOrDefault();

                StatusMessage =
                    Books.Count > 0
                        ? "Select a book."
                        : "Unable to load books.";
            }
            catch (Exception ex)
            {
                StatusMessage =
                    $"Error : {ex.Message}";
            }
        }

        private async Task LoadChapterAsync()
        {
            if (SelectedTranslation == null)
            {
                StatusMessage =
                    "Select a translation.";
                return;
            }

            if (SelectedBook == null)
            {
                StatusMessage =
                    "Select a book.";
                return;
            }

            try
            {
                StatusMessage =
                    $"Loading {SelectedBook.Name} {SelectedChapter}...";

                var chapter =
                    await _bibleService
                        .GetChapterAsync(
                            SelectedTranslation,
                            SelectedBook,
                            SelectedChapter);

                Verses.Clear();
                SearchResults.Clear();

                if (chapter == null)
                {
                    StatusMessage =
                        "Chapter not found.";
                    return;
                }

                foreach (var verse in chapter.Verses)
                    Verses.Add(verse);

                StatusMessage =
                    $"{chapter.Reference} — {chapter.Verses.Count} verses";
            }
            catch (Exception ex)
            {
                StatusMessage =
                    $"Error : {ex.Message}";
            }
        }

        private async Task SearchAsync()
        {
            SearchResults.Clear();

            if (string.IsNullOrWhiteSpace(SearchText))
                return;

            if (SelectedTranslation == null ||
                SelectedBook == null)
            {
                StatusMessage =
                    "Load a book first.";
                return;
            }

            try
            {
                StatusMessage =
                    "Searching...";

                var results =
                    await _bibleService.SearchAsync(
                        SelectedTranslation,
                        SelectedBook,
                        SelectedChapter,
                        SearchText);

                foreach (var result in results)
                    SearchResults.Add(result);

                StatusMessage =
                    $"{SearchResults.Count} result(s).";
            }
            catch (Exception ex)
            {
                StatusMessage =
                    $"Error : {ex.Message}";
            }
        }

        private void AddSelectedVerse()
        {
            if (SelectedVerse == null)
            {
                StatusMessage =
                    "Select a verse.";
                return;
            }

            var slide = CreateVerseSlide(
                SelectedVerse);

            _mainViewModel.Slides.Add(slide);

            _mainViewModel.SelectedSlide =
                slide;

            _mainViewModel.RefreshSlides();

            StatusMessage =
                $"{SelectedVerse.Reference} added to the presentation.";
        }

        private async Task AddChapterAsync()
        {
            if (SelectedTranslation == null ||
                SelectedBook == null)
            {
                StatusMessage =
                    "Select a translation and a book.";
                return;
            }

            try
            {
                var chapter =
                    await _bibleService.GetChapterAsync(
                        SelectedTranslation,
                        SelectedBook,
                        SelectedChapter);

                if (chapter == null)
                {
                    StatusMessage =
                        "Chapter not found.";
                    return;
                }

                var theme =
                    _mainViewModel.Theme
                        .SelectedTheme;

                var slides =
                    _bibleService.CreateSlides(
                        chapter,
                        VersesPerSlide,
                        theme);

                foreach (var slide in slides)
                    _mainViewModel.Slides.Add(slide);

                if (slides.Count > 0)
                {
                    _mainViewModel.SelectedSlide =
                        slides[0];
                }

                _mainViewModel.RefreshSlides();

                StatusMessage =
                    $"{chapter.Reference} added — {slides.Count} slide(s).";
            }
            catch (Exception ex)
            {
                StatusMessage =
                    $"Error : {ex.Message}";
            }
        }

        private Slide CreateVerseSlide(
            BibleVerse verse)
        {
            var theme =
                _mainViewModel.Theme.SelectedTheme;

            var slide = new Slide
            {
                Title = verse.Reference,
                Background =
                    theme?.BackgroundColor ?? "#10131A"
            };

            slide.Elements.Add(
                new TextElement
                {
                    Name = "Bible Reference",
                    X = 120,
                    Y = 90,
                    Width = 1680,
                    Height = 100,
                    Text = verse.Reference,
                    FontFamily =
                        theme?.FontFamily ?? "Inter",
                    FontSize =
                        theme?.TitleFontSize ?? 42,
                    IsBold = true,
                    Color =
                        theme?.AccentColor ?? "#4F8CFF",
                    HorizontalAlignment = "Center",
                    VerticalAlignment = "Center"
                });

            slide.Elements.Add(
                new TextElement
                {
                    Name = "Bible Verse",
                    X = 140,
                    Y = 260,
                    Width = 1640,
                    Height = 600,
                    Text = verse.Text,
                    FontFamily =
                        theme?.FontFamily ?? "Inter",
                    FontSize =
                        theme?.BodyFontSize ?? 52,
                    Color =
                        theme?.PrimaryColor ?? "#FFFFFF",
                    HorizontalAlignment = "Center",
                    VerticalAlignment = "Center"
                });

            return slide;
        }

        private sealed class RelayCommand : ICommand
        {
            private readonly Action _execute;

            public RelayCommand(Action execute)
            {
                _execute = execute;
            }

            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter)
                => true;

            public void Execute(object? parameter)
                => _execute();
        }

        private sealed class AsyncRelayCommand : ICommand
        {
            private readonly Func<Task> _execute;

            private bool _isExecuting;

            public AsyncRelayCommand(Func<Task> execute)
            {
                _execute = execute;
            }

            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter)
                => !_isExecuting;

            public async void Execute(object? parameter)
            {
                if (_isExecuting)
                    return;

                try
                {
                    _isExecuting = true;

                    CanExecuteChanged?.Invoke(
                        this,
                        EventArgs.Empty);

                    await _execute();
                }
                finally
                {
                    _isExecuting = false;

                    CanExecuteChanged?.Invoke(
                        this,
                        EventArgs.Empty);
                }
            }
        }
    }
}
