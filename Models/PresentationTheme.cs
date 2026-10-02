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

        public double TitleFontSize { get; set; } = 48;

        public double BodyFontSize { get; set; } = 75;

        public bool TitleBold { get; set; } = true;

        public static List<PresentationTheme> CreateBuiltInThemes()
        {
            return
            [
                new PresentationTheme
            {
                Name = "Default",
                BackgroundColor = "#10131A",
                PrimaryColor = "#FFFFFF",
                SecondaryColor = "#B8C0CC",
                AccentColor = "#4F8CFF",
                FontFamily = "Inter",
                TitleFontSize = 75,
                BodyFontSize = 75,
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
                TitleFontSize = 75,
                BodyFontSize = 75,
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
                TitleFontSize = 75,
                BodyFontSize = 75,
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
                TitleFontSize = 75,
                BodyFontSize = 75,
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
                TitleFontSize = 75,
                BodyFontSize = 75,
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
                TitleFontSize = 75,
                BodyFontSize = 75,
                TitleBold = true
            },
            new PresentationTheme
{
    Name = "Forest",
    BackgroundColor = "#0B1F17",
    PrimaryColor = "#F1F8F5",
    SecondaryColor = "#A7C4B5",
    AccentColor = "#34D399",
    FontFamily = "Inter",
    TitleFontSize = 75,
    BodyFontSize = 75,
    TitleBold = true
},

new PresentationTheme
{
    Name = "Wine",
    BackgroundColor = "#240B16",
    PrimaryColor = "#FFFFFF",
    SecondaryColor = "#D8B4C4",
    AccentColor = "#E11D48",
    FontFamily = "Inter",
    TitleFontSize = 75,
    BodyFontSize = 75,
    TitleBold = true
},

new PresentationTheme
{
    Name = "Royal",
    BackgroundColor = "#15102B",
    PrimaryColor = "#FFFFFF",
    SecondaryColor = "#C4B5FD",
    AccentColor = "#A855F7",
    FontFamily = "Inter",
    TitleFontSize = 75,
    BodyFontSize = 75,
    TitleBold = true
},

new PresentationTheme
{
    Name = "Sunset",
    BackgroundColor = "#29140A",
    PrimaryColor = "#FFF7ED",
    SecondaryColor = "#FDBA74",
    AccentColor = "#F97316",
    FontFamily = "Inter",
    TitleFontSize = 75,
    BodyFontSize = 75,
    TitleBold = true
},

new PresentationTheme
{
    Name = "Crimson",
    BackgroundColor = "#1F0808",
    PrimaryColor = "#FFFFFF",
    SecondaryColor = "#FCA5A5",
    AccentColor = "#EF4444",
    FontFamily = "Inter",
    TitleFontSize = 75,
    BodyFontSize = 75,
    TitleBold = true
},

new PresentationTheme
{
    Name = "Teal",
    BackgroundColor = "#062A2A",
    PrimaryColor = "#F0FDFA",
    SecondaryColor = "#99F6E4",
    AccentColor = "#14B8A6",
    FontFamily = "Inter",
    TitleFontSize = 75,
    BodyFontSize = 75,
    TitleBold = true
},

new PresentationTheme
{
    Name = "Gold",
    BackgroundColor = "#17130A",
    PrimaryColor = "#FFFDF5",
    SecondaryColor = "#D6C79A",
    AccentColor = "#EAB308",
    FontFamily = "Inter",
    TitleFontSize = 75,
    BodyFontSize = 75,
    TitleBold = true
},

new PresentationTheme
{
    Name = "Arctic",
    BackgroundColor = "#EAF2F8",
    PrimaryColor = "#101828",
    SecondaryColor = "#475467",
    AccentColor = "#2563EB",
    FontFamily = "Inter",
    TitleFontSize = 75,
    BodyFontSize = 75,
    TitleBold = true
},

new PresentationTheme
{
    Name = "Pure White",
    BackgroundColor = "#FFFFFF",
    PrimaryColor = "#111827",
    SecondaryColor = "#4B5563",
    AccentColor = "#2563EB",
    FontFamily = "Inter",
    TitleFontSize = 75,
    BodyFontSize = 75,
    TitleBold = true
},

new PresentationTheme
{
    Name = "Warm",
    BackgroundColor = "#211A16",
    PrimaryColor = "#FFF8F0",
    SecondaryColor = "#D6C2B2",
    AccentColor = "#D97706",
    FontFamily = "Inter",
    TitleFontSize = 75,
    BodyFontSize = 75,
    TitleBold = true
},

new PresentationTheme
{
    Name = "Neon",
    BackgroundColor = "#09090B",
    PrimaryColor = "#FAFAFA",
    SecondaryColor = "#A1A1AA",
    AccentColor = "#22D3EE",
    FontFamily = "Inter",
    TitleFontSize = 75,
    BodyFontSize = 75,
    TitleBold = true
},

new PresentationTheme
{
    Name = "Classic",
    BackgroundColor = "#000000",
    PrimaryColor = "#FFFFFF",
    SecondaryColor = "#D1D5DB",
    AccentColor = "#FFFFFF",
    FontFamily = "Arial",
    TitleFontSize = 75,
    BodyFontSize = 75,
    TitleBold = true
}
            ];
        }
    }
}
