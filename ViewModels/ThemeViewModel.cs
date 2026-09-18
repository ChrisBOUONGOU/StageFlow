using System;
using System.Collections.Generic;
using System.Text;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StageFlow.Models;
using StageFlow.Services;


namespace StageFlow.ViewModels 
{
    public partial class ThemeViewModel : ObservableObject
    {
        private readonly MainWindowViewModel _mainViewModel;
        private readonly ThemeService _themeService;

        public ObservableCollection<PresentationTheme> Themes { get; } = new();

        [ObservableProperty]
        private PresentationTheme? selectedTheme;

        public ThemeViewModel(MainWindowViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            _themeService = new ThemeService();

            foreach (PresentationTheme theme in _themeService.GetBuiltInThemes())
            {
                Themes.Add(theme);
            }

            SelectedTheme =
                Themes.FirstOrDefault(
                    x => x.Name ==
                        mainViewModel.Document.Settings.ThemeName)
                ?? Themes.FirstOrDefault();
        }

        [RelayCommand]
        private void SelectTheme(PresentationTheme theme)
        {
            SelectedTheme = theme;
        }

        [RelayCommand]
        private void ApplyTheme()
        {
            if (SelectedTheme == null)
                return;

            _themeService.ApplyTheme(
                _mainViewModel.Document,
                SelectedTheme);

            _mainViewModel.RefreshSlides();
            _mainViewModel.Editor.RefreshCanvas();

            _mainViewModel.SetStatus(
                $"Theme applied: {SelectedTheme.Name}");
        }

        partial void OnSelectedThemeChanged(
            PresentationTheme? value)
        {
            OnPropertyChanged(nameof(SelectedThemeName));
            OnPropertyChanged(nameof(SelectedThemeFont));
            OnPropertyChanged(nameof(SelectedThemeBackground));
        }

        public string SelectedThemeName =>
            SelectedTheme?.Name ?? "-";

        public string SelectedThemeFont =>
            SelectedTheme?.FontFamily ?? "-";

        public string SelectedThemeBackground =>
            SelectedTheme?.BackgroundColor ?? "-";
    }
}
