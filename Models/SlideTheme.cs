using System;
using System.Collections.Generic;
using System.Text;

namespace StageFlow.Models
{
    public sealed class SlideTheme
    {
        public string Name { get; set; } = string.Empty;

        public string BackgroundColor { get; set; } = "#FFFFFF";

        public string PrimaryColor { get; set; } = "#000000";

        public string SecondaryColor { get; set; } = "#666666";

        public string AccentColor { get; set; } = "#8B0000";

        public string FontFamily { get; set; } = "Arial";
    }
}
