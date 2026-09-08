using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using StageFlow.Models;
using StageFlow.Services;

namespace StageFlow.ViewModels
{
    public partial class ApplicationThemeViewModel : ObservableObject
    {
        private readonly ApplicationThemeService _themeService;

        public ObservableCollection<ApplicationTheme> Themes { get; }

        [ObservableProperty]
        private ApplicationTheme? selectedTheme;

        [ObservableProperty]
        private string statusMessage = "Thème StageFlow";

        public ApplicationThemeViewModel()
        {
            _themeService = new ApplicationThemeService();

            Themes = new ObservableCollection<ApplicationTheme>(
                _themeService.Themes);

            _themeService.Initialize();

            SelectedTheme = _themeService.CurrentTheme;
        }

        [RelayCommand]
        private void ApplyTheme()
        {
            if (SelectedTheme is null)
                return;

            _themeService.ApplyTheme(SelectedTheme);

            StatusMessage =
                $"Thème appliqué : {SelectedTheme.Name}";
        }
    }
}
