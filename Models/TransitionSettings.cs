using System;
using System.Collections.Generic;
using System.Text;

namespace StageFlow.Models
{
    public enum TransitionType
    {
        Cut,
        Fade,
        Dissolve,
        SlideLeft,
        SlideRight,
        SlideUp,
        SlideDown
    }
    public sealed class TransitionSettings
    {
        public TransitionType Type { get; set; }
        = TransitionType.Cut;

        public double Duration { get; set; }
            = 0.3;
    }
}
