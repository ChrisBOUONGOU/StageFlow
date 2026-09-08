using System;
using System.Collections.Generic;
using System.Text;

namespace StageFlow.Services.Remote
{
    public sealed class RemoteState
    {
        public bool Connected { get; set; }

        public int CurrentSlide { get; set; }

        public int TotalSlides { get; set; }

        public string SlideTitle { get; set; } = string.Empty;

        public bool IsPresentationRunning { get; set; }

        public bool IsBlackout { get; set; }
    }
}
