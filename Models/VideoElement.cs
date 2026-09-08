using System;
using System.Collections.Generic;
using System.Text;

namespace StageFlow.Models
{
    public sealed class VideoElement : SlideElement
    {
        public string Source { get; set; } = "";

        public bool Loop { get; set; }

        public bool AutoPlay { get; set; } = true;

        public double Volume { get; set; } = 1.0;
    }
}
