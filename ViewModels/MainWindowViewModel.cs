using Avalonia.Controls;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StageFlow.Models;
using StageFlow.Services;
using StageFlow.Services.Remote;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StageFlow.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly ProjectService _projectService;

        public PresentationDocument Document { get; }

        public ObservableCollection<Slide> Slides { get; }

        public EditorViewModel Editor { get; }

        public LiveViewModel Live { get; }

        public ThemeViewModel Theme { get; }

        public LyricsViewModel Lyrics { get; }
        public BibleViewModel Bible { get; }

        public ApplicationThemeViewModel ApplicationTheme { get; }

        private readonly RemoteServer _remoteServer;

        [ObservableProperty]
        private Slide? selectedSlide;

        [ObservableProperty]
        private SlideTheme? selectedTheme;

        [ObservableProperty]
        private Slide? liveSlide;

        [ObservableProperty]
        private string statusText = "Ready";

        [ObservableProperty]
        private string? currentFilePath;

        public string ProjectName
        {
            get => Document.Name;
            set
            {
                if (Document.Name == value)
                    return;

                Document.Name = value;
                OnPropertyChanged();
            }
        }

        public event Action<double>? VideoVolumeChanged;

        public event Action<bool>? VideoLoopChanged;

        private readonly TransitionService _transitionService = new();

        public MediaLibraryViewModel MediaLibrary
        {
            get;
        }

        public RemoteServer RemoteServer =>
    _remoteServer;

        public bool IsRemoteServerRunning =>
            _remoteServer.IsRunning;

        public string RemoteConnectionUrl =>
            _remoteServer.ConnectionUrl;

        public MainWindowViewModel()
        {
            _projectService = new ProjectService();

            Document = CreateDefaultDocument();

            Slides = new ObservableCollection<Slide>(
                Document.Slides);

            SelectedSlide = Slides.FirstOrDefault();

            Editor = new EditorViewModel(this);

            MediaLibrary =
    new MediaLibraryViewModel(this);

            Live = new LiveViewModel(this);

            Theme = new ThemeViewModel(this);
            Lyrics = new LyricsViewModel(this);
            Bible = new BibleViewModel(this);

            ApplicationTheme = new ApplicationThemeViewModel();

            _remoteServer = new RemoteServer(this);

            try
            {
                StartRemoteServer();
            }
            catch
            {
                // Le serveur Remote ne doit jamais empêcher
                // StageFlow de démarrer.
            }
        }

        private static PresentationDocument CreateDefaultDocument()
        {
            var document = new PresentationDocument
            {
                Name = "Untitled Presentation"
            };

            document.Slides.Add(
                CreateWelcomeSlide());

            document.Slides.Add(
                CreateMessageSlide());

            return document;
        }

        private static Slide CreateWelcomeSlide()
        {
            var slide = new Slide
            {
                Title = "Welcome",
                Background = "#10131A"
            };

            slide.Elements.Add(
                new TextElement
                {
                    Text = "Welcome to StageFlow",
                    X = 0,
                    Y = 0,
                    Width = 1920,
                    Height = 1080,
                    FontSize = 72,
                    HorizontalAlignment = "Center",
                    VerticalAlignment = "Center",
                    ZIndex = 1
                });

            return slide;
        }

        private static Slide CreateMessageSlide()
        {
            var slide = new Slide
            {
                Title = "Main Message",
                Background = "#10131A"
            };

            slide.Elements.Add(
                new TextElement
                {
                    Text = "Your message goes here",
                    X = 200,
                    Y = 400,
                    Width = 1520,
                    Height = 120,
                    FontSize = 60,
                    ZIndex = 1
                });

            return slide;
        }

        partial void OnSelectedSlideChanged(Slide? value)
        {
            StatusText = value == null
                ? "No slide selected"
                : $"Editing: {value.Title}";

            OnPropertyChanged(nameof(SelectedElements));
        }

        public IEnumerable<SlideElement> SelectedElements =>
            SelectedSlide?.Elements ?? Enumerable.Empty<SlideElement>();

        [RelayCommand]
        private void AddSlide()
        {
            var slide = new Slide
            {
                Title = $"Slide {Slides.Count + 1}"
            };

            slide.Elements.Add(
                new TextElement
                {
                    Text = "New Slide",
                    X = 200,
                    Y = 400,
                    Width = 1520,
                    Height = 120,
                    FontSize = 60
                });

            Document.Slides.Add(slide);
            Slides.Add(slide);

            SelectedSlide = slide;

            StatusText = "New slide created";
        }

        [RelayCommand]
        private void DeleteSlide()
        {
            if (SelectedSlide == null)
                return;

            if (Slides.Count <= 1)
                return;

            int index = Slides.IndexOf(
                SelectedSlide);

            Document.Slides.Remove(
                SelectedSlide);

            Slides.Remove(
                SelectedSlide);

            SelectedSlide =
                Slides[Math.Clamp(
                    index - 1,
                    0,
                    Slides.Count - 1)];

            StatusText = "Slide deleted";
        }

        [RelayCommand]
        private void SendLive()
        {
            if (SelectedSlide == null)
                return;

            LiveSlide = SelectedSlide;

            StatusText =
                $"LIVE: {SelectedSlide.Title}";
        }

        [RelayCommand]
        private void Blackout()
        {
            LiveSlide = null;

            StatusText = "Blackout";
        }

        [RelayCommand]
        private async Task SaveProject()
        {
            if (string.IsNullOrWhiteSpace(CurrentFilePath))
            {
                string directory =
                    Path.Combine(
                        Environment.GetFolderPath(
                            Environment.SpecialFolder.MyDocuments),
                        "StageFlow");

                Directory.CreateDirectory(directory);

                CurrentFilePath =
                    Path.Combine(
                        directory,
                        $"{Sanitize(ProjectName)}.stageflow.json");
            }
            
                await MediaLibrary.ConvertTemporaryAssetsToProjectAsync(
                    CurrentFilePath);

            await _projectService.SaveAsync(
                Document,
                CurrentFilePath);

            MediaLibrary.Refresh();

            StatusText =
                $"Saved: {Path.GetFileName(CurrentFilePath)}";
        }

        private static string Sanitize(string value)
        {
            foreach (char character
                     in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(
                    character,
                    '_');
            }

            return string.IsNullOrWhiteSpace(value)
                ? "Presentation"
                : value;
        }

        [RelayCommand]
        private void AddText()
        {
            if (SelectedSlide == null)
                return;

            var text = new TextElement
            {
                Text = "New Text",
                X = 300,
                Y = 300,
                Width = 800,
                Height = 120,
                FontSize = 48,
                ZIndex = SelectedSlide.Elements.Count + 1
            };

            SelectedSlide.Elements.Add(text);

            OnPropertyChanged(nameof(SelectedElements));

            StatusText = "Text element added";
        }

        public void NotifyVideoVolumeChanged(
    double volume)
        {
            VideoVolumeChanged?.Invoke(volume);
        }

        public void NotifyVideoLoopChanged(
            bool loop)
        {
            VideoLoopChanged?.Invoke(loop);
        }

        public async Task TransitionToSlideAsync(
    Slide slide)
        {
            if (slide == null)
                return;

            LiveSlide = slide;

            StatusText =
                $"Live: {slide.Title}";
        }

        public void SetPreviewSlide(Slide? slide)
        {
            Live.Preview(slide);
        }

        public void GoLive()
        {
            Live.GoLive();
        }

        public void BlackoutLive()
        {
            Live.Blackout();
        }

        public void RestoreLive()
        {
            Live.RestoreLive();
        }

        public void SetStatus(string message)
        {
            StatusText = message;
        }

        public void RefreshSlides()
        {
            OnPropertyChanged(nameof(Slides));
            OnPropertyChanged(nameof(SelectedSlide));
            OnPropertyChanged(nameof(Live));
            OnPropertyChanged(nameof(Editor));
            OnPropertyChanged(nameof(MediaLibrary));
            OnPropertyChanged(nameof(Theme));
            OnPropertyChanged(nameof(Lyrics));
            OnPropertyChanged(nameof(Bible));
        }

        [RelayCommand]
        private void AddSlidee()
        {
            var slide = new Slide
            {
                Title = $"Slide {Slides.Count + 1}",
                Background = "#10131A"
            };

            Slides.Add(slide);

            SelectedSlide = slide;

            Editor.RefreshCanvas();

            StatusText = $"Nouvelle slide créée : {slide.Title}";
        }

        [RelayCommand]
        private void DeleteSlidee()
        {
            if (SelectedSlide is null)
                return;

            if (Slides.Count <= 1)
            {
                StatusText = "Impossible de supprimer la dernière slide.";
                return;
            }

            var index = Slides.IndexOf(SelectedSlide);

            Slides.Remove(SelectedSlide);

            if (index >= Slides.Count)
                index = Slides.Count - 1;

            SelectedSlide = Slides[index];

            Editor.RefreshCanvas();

            StatusText = "Slide supprimée.";
        }

        [RelayCommand]
        private void DuplicateSlide()
        {
            if (SelectedSlide is null)
                return;

            var json = System.Text.Json.JsonSerializer.Serialize(
                SelectedSlide);

            var duplicate =
                System.Text.Json.JsonSerializer.Deserialize<Slide>(json);

            if (duplicate is null)
                return;

            duplicate.Title = $"{SelectedSlide.Title} Copy";

            var index = Slides.IndexOf(SelectedSlide);

            Slides.Insert(index + 1, duplicate);

            SelectedSlide = duplicate;

            Editor.RefreshCanvas();

            StatusText = "Slide dupliquée.";
        }

        public void StartRemoteServer()
        {
            _remoteServer.Start();

            OnPropertyChanged(
                nameof(IsRemoteServerRunning));

            OnPropertyChanged(
                nameof(RemoteConnectionUrl));
        }

        public void StopRemoteServer()
        {
            _remoteServer.Stop();

            OnPropertyChanged(
                nameof(IsRemoteServerRunning));
        }

        [RelayCommand]
        private void ApplyTheme(SlideTheme theme)
        {
            if (SelectedSlide == null)
                return;

            SelectedSlide.Theme = theme;

            SelectedTheme = theme;
        }

    }
}
