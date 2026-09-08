using System;
using System.Collections.Generic;
using System.Text;

namespace StageFlow.Models
{
    public sealed class PresentationTheme
    {

        public string Name { get; set; } = "Default";

        public string BackgroundColor { get; set; } = "#000000";

        public string PrimaryColor { get; set; } = "#FFFFFF";

        public string SecondaryColor { get; set; } = "#B8C0CC";

        public string AccentColor { get; set; } = "#4F8CFF";

        public string FontFamily { get; set; } = "Inter";

        public double TitleFontSize { get; set; } = 72;

        public double BodyFontSize { get; set; } = 42;

        public bool TitleBold { get; set; } = true;

        public static List<PresentationTheme> CreateBuiltInThemes()
        {
            return
            [
                new PresentationTheme
            {
                Name = "Default",
                BackgroundColor = "#000000",
                PrimaryColor = "#FFFFFF",
                SecondaryColor = "#B8C0CC",
                AccentColor = "#4F8CFF",
                FontFamily = "Inter",
                TitleFontSize = 72,
                BodyFontSize = 42,
                TitleBold = true
            },

            new PresentationTheme
            {
                Name = "Midnight",
                BackgroundColor = "#080B12",
                PrimaryColor = "#FFFFFF",
                SecondaryColor = "#CBD5E1",
                AccentColor = "#60A5FA",
                FontFamily = "Inter",
                TitleFontSize = 72,
                BodyFontSize = 42,
                TitleBold = true
            },

            new PresentationTheme
            {
                Name = "Elegant",
                BackgroundColor = "#111111",
                PrimaryColor = "#F5F1E8",
                SecondaryColor = "#C9C0AF",
                AccentColor = "#D6A85F",
                FontFamily = "Georgia",
                TitleFontSize = 70,
                BodyFontSize = 40,
                TitleBold = true
            },

            new PresentationTheme
            {
                Name = "Minimal",
                BackgroundColor = "#F5F5F5",
                PrimaryColor = "#111111",
                SecondaryColor = "#555555",
                AccentColor = "#222222",
                FontFamily = "Inter",
                TitleFontSize = 68,
                BodyFontSize = 40,
                TitleBold = true
            },

            new PresentationTheme
            {
                Name = "Ocean",
                BackgroundColor = "#061826",
                PrimaryColor = "#FFFFFF",
                SecondaryColor = "#B9D8E8",
                AccentColor = "#22C1DC",
                FontFamily = "Inter",
                TitleFontSize = 72,
                BodyFontSize = 42,
                TitleBold = true
            },

            new PresentationTheme
            {
                Name = "Worship",
                BackgroundColor = "#09070F",
                PrimaryColor = "#FFFFFF",
                SecondaryColor = "#D8D0E5",
                AccentColor = "#A78BFA",
                FontFamily = "Inter",
                TitleFontSize = 76,
                BodyFontSize = 44,
                TitleBold = true
            }
            ];
        }
    }
}
