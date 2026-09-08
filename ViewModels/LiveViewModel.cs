using System;
using System.Collections.Generic;
using System.Text;

using CommunityToolkit.Mvvm.ComponentModel;
using StageFlow.Models;

namespace StageFlow.ViewModels
{
    public partial class LiveViewModel : ObservableObject
    {
        private readonly MainWindowViewModel _mainViewModel;

        [ObservableProperty]
        private Slide? liveSlide;

        [ObservableProperty]
        private Slide? previewSlide;

        [ObservableProperty]
        private bool isBlackout;

        public LiveViewModel(
            MainWindowViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            PreviewSlide =
                mainViewModel.SelectedSlide;

            mainViewModel.PropertyChanged +=
                MainViewModel_PropertyChanged;
        }

        private void MainViewModel_PropertyChanged(
            object? sender,
            System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName ==
                nameof(MainWindowViewModel.SelectedSlide))
            {
                PreviewSlide =
                    _mainViewModel.SelectedSlide;
            }
        }

        public void Preview(
            Slide? slide)
        {
            PreviewSlide = slide;
        }

        public void GoLive()
        {
            if (PreviewSlide == null)
            {
                _mainViewModel.SetStatus(
                    "No preview slide selected.");

                return;
            }

            LiveSlide =
                PreviewSlide;

            IsBlackout =
                false;

            _mainViewModel.SetStatus(
                $"LIVE: {PreviewSlide.Title}");
        }

        public void Blackout()
        {
            IsBlackout =
                true;

            _mainViewModel.SetStatus(
                "LIVE: BLACKOUT");
        }

        public void RestoreLive()
        {
            IsBlackout =
                false;

            if (LiveSlide != null)
            {
                _mainViewModel.SetStatus(
                    $"LIVE: {LiveSlide.Title}");
            }
        }

        public void ClearLive()
        {
            LiveSlide = null;

            IsBlackout = false;

            _mainViewModel.SetStatus(
                "LIVE: cleared.");
        }
    }
}
