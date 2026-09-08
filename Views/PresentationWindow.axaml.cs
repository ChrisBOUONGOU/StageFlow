using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using StageFlow.Models;
using StageFlow.Services;
using StageFlow.ViewModels;
using System;



namespace StageFlow.Views;

public partial class PresentationWindow : Window
{
    private readonly MainWindowViewModel _viewModel;

    public PresentationWindow(
        MainWindowViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;

        WindowState =
            WindowState.FullScreen;

        SystemDecorations =
            SystemDecorations.None;

        CanResize = false;

        ShowInTaskbar = false;

        _viewModel.PropertyChanged +=
            ViewModel_PropertyChanged;

        Opened +=
            PresentationWindow_Opened;

        UpdateSlide();
    }

    private void PresentationWindow_Opened(
        object? sender,
        EventArgs e)
    {
        WindowState =
            WindowState.FullScreen;

        Activate();

        Focus();

        UpdateSlide();
    }

    private void ViewModel_PropertyChanged(
        object? sender,
        System.ComponentModel
            .PropertyChangedEventArgs e)
    {
        if (e.PropertyName ==
            nameof(
                MainWindowViewModel.SelectedSlide))
        {
            UpdateSlide();
        }
    }

    private void UpdateSlide()
    {
        var slide =
            _viewModel.SelectedSlide;

        if (slide == null)
        {
            SlideContent.Content = null;
            return;
        }

        SlideContent.Content =
            SlideRenderer.CreateSlide(
                slide,
                _viewModel.CurrentFilePath);
    }

    // =========================================================
    // KEYBOARD
    // =========================================================

    private void PresentationWindow_KeyDown(
        object? sender,
        KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Right:
            case Key.Down:
            case Key.Space:

                NextSlide();

                e.Handled = true;
                break;

            case Key.Left:
            case Key.Up:

                PreviousSlide();

                e.Handled = true;
                break;

            case Key.Home:

                FirstSlide();

                e.Handled = true;
                break;

            case Key.End:

                LastSlide();

                e.Handled = true;
                break;

            case Key.Escape:

                Close();

                e.Handled = true;
                break;
        }
    }

    // =========================================================
    // MOUSE
    // =========================================================

    private void PresentationWindow_PointerPressed(
        object? sender,
        PointerPressedEventArgs e)
    {
        var properties =
            e.GetCurrentPoint(this)
                .Properties;

        if (properties.PointerUpdateKind ==
            PointerUpdateKind.LeftButtonPressed)
        {
            NextSlide();

            e.Handled = true;

            return;
        }

        if (properties.PointerUpdateKind ==
            PointerUpdateKind.RightButtonPressed)
        {
            PreviousSlide();

            e.Handled = true;
        }
    }

    // =========================================================
    // NAVIGATION
    // =========================================================

    private void NextSlide()
    {
        if (_viewModel.Slides.Count == 0)
            return;

        if (_viewModel.SelectedSlide == null)
            return;

        int index =
            _viewModel.Slides.IndexOf(
                _viewModel.SelectedSlide);

        if (index < 0)
            return;

        if (index >=
            _viewModel.Slides.Count - 1)
            return;

        _viewModel.SelectedSlide =
            _viewModel.Slides[index + 1];
    }

    private void PreviousSlide()
    {
        if (_viewModel.Slides.Count == 0)
            return;

        if (_viewModel.SelectedSlide == null)
            return;

        int index =
            _viewModel.Slides.IndexOf(
                _viewModel.SelectedSlide);

        if (index <= 0)
            return;

        _viewModel.SelectedSlide =
            _viewModel.Slides[index - 1];
    }

    private void FirstSlide()
    {
        if (_viewModel.Slides.Count == 0)
            return;

        _viewModel.SelectedSlide =
            _viewModel.Slides[0];
    }

    private void LastSlide()
    {
        if (_viewModel.Slides.Count == 0)
            return;

        _viewModel.SelectedSlide =
            _viewModel.Slides[
                _viewModel.Slides.Count - 1];
    }

    // =========================================================
    // CLOSE
    // =========================================================

    protected override void OnClosed(
        EventArgs e)
    {
        _viewModel.PropertyChanged -=
            ViewModel_PropertyChanged;

        base.OnClosed(e);
    }
}

