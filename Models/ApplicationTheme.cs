using System;
using System.Collections.Generic;
using System.Text;

namespace StageFlow.Models
{
    public sealed class ApplicationTheme
    {

        public string Name { get; init; } = string.Empty;

        public string PrimaryColor { get; init; } = "#7A1F3D";
        public string AccentColor { get; init; } = "#2563EB";

        public string BackgroundColor { get; init; } = "#0B0D12";
        public string SurfaceColor { get; init; } = "#121722";
        public string SurfaceSecondaryColor { get; init; } = "#1A2030";
        public string BorderColor { get; init; } = "#2A3140";
        public string HoverColor { get; init; } = "#202A3A";

        public bool IsDark { get; init; }

        // Contraste automatique
        public string TextColor =>
            IsDark ? "#FFFFFF" : "#111111";

        public string SecondaryTextColor =>
            IsDark ? "#B8C0CC" : "#4B5563";

        public string LogoColor =>
            IsDark ? "#FFFFFF" : "#111111";

        public string IconColor =>
            IsDark ? "#FFFFFF" : "#111111";

        public string ButtonTextColor =>
            IsDark ? "#FFFFFF" : "#FFFFFF";

       

        public static IReadOnlyList<ApplicationTheme> BuiltInThemes { get; } =
            new List<ApplicationTheme>
            {
            new()
            {
                Name = "StageFlow Burgundy",

                PrimaryColor = "#7A1F3D",
                AccentColor = "#2563EB",

                BackgroundColor = "#0B0D12",
                SurfaceColor = "#121722",
                SurfaceSecondaryColor = "#1A2030",
                BorderColor = "#2A3140",
                HoverColor = "#202A3A",

                IsDark = true
            },

            new()
            {
                Name = "StageFlow Blue",

                PrimaryColor = "#2563EB",
                AccentColor = "#7A1F3D",

                BackgroundColor = "#08111F",
                SurfaceColor = "#0E1929",
                SurfaceSecondaryColor = "#162338",
                BorderColor = "#263B57",
                HoverColor = "#1D304A",

                IsDark = true
            },

            new()
            {
                Name = "StageFlow Midnight",

                PrimaryColor = "#7A1F3D",
                AccentColor = "#3B82F6",

                BackgroundColor = "#050609",
                SurfaceColor = "#0D1017",
                SurfaceSecondaryColor = "#151A24",
                BorderColor = "#252B36",
                HoverColor = "#1B2230",

                IsDark = true
            },

            new()
            {
                Name = "StageFlow Light",

                PrimaryColor = "#7A1F3D",
                AccentColor = "#2563EB",

                BackgroundColor = "#F3F5F8",
                SurfaceColor = "#FFFFFF",
                SurfaceSecondaryColor = "#E8ECF2",
                BorderColor = "#CBD2DC",
                HoverColor = "#E1E6ED",

                IsDark = false
            }
            };
    }
}
