using System;
using System.Collections.Generic;
using System.Text;

namespace StageFlow.Models
{
    public sealed class TextElement : SlideElement
    {
        public string Text { get; set; } = "New Text";

        public string FontFamily { get; set; } = "Inter";

        public double FontSize { get; set; } = 48;

        public bool IsBold { get; set; }

        public bool IsItalic { get; set; }

        public string Color { get; set; } = "#FFFFFF";

        public string HorizontalAlignment { get; set; } = "Center";

        public string VerticalAlignment { get; set; } = "Center";

        public double LetterSpacing { get; set; }

        public double LineSpacing { get; set; } = 1.0;
    }
}
