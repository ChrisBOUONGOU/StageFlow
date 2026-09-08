using System;
using System.Collections.Generic;
using System.Text;

namespace StageFlow.Models
{
    public sealed class PresentationSettings
    {
        public int Width { get; set; } = 870;

        public int Height { get; set; } = 485;

        public string BackgroundColor { get; set; } = "#000000";

        public string ThemeName { get; set; } = "Default";

        public string FontFamily { get; set; } = "Inter";

        public string PrimaryColor { get; set; } = "#FFFFFF";

        public string SecondaryColor { get; set; } = "#B8C0CC";

        public string AccentColor { get; set; } = "#4F8CFF";
    }
}
