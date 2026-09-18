using CommunityToolkit.Mvvm.ComponentModel;
using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace StageFlow.Models
{
    public sealed class Slide : ObservableObject
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public string _title { get; set; } = "New Slide";

        public string _background { get; set; } = "#151922";

        public string Title
        {
            get => _title;
            set
            {
                if (_title == value)
                    return;

                _title = value;
                OnPropertyChanged();
            }
        }

        public string Background
        {
            get => _background;
            set
            {
                if (_background == value)
                    return;

                _background = value;
                OnPropertyChanged();
            }
        }


        public List<SlideElement> Elements { get; set; } = new();

        public TransitionSettings Transition { get; set; } = new();

        public SlideTheme Theme { get; set; } = new();
    }
}
