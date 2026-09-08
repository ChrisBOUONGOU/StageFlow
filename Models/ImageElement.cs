using System;
using System.Collections.Generic;
using System.Text;

namespace StageFlow.Models
{
    public sealed class ImageElement : SlideElement
    {
        public string Source { get; set; } = "";

        public bool StretchUniform { get; set; } = true;

        public bool MaintainAspectRatio { get; set; } = true;

        


    }

}

