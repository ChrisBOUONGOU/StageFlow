using Avalonia;
using Avalonia.Media;
using StageFlow.Models;
using System;
using System.Collections.Generic;
using System.Text;

using System.Text.Json;
using System.Security.Cryptography;


namespace StageFlow.Services
{
    public sealed class ApplicationThemeService
    {
        private const string PrimaryBrush = "AppPrimaryBrush";
        private const string AccentBrush = "AppAccentBrush";
        private const string BackgroundBrush = "AppBackgroundBrush";
        private const string SurfaceBrush = "AppSurfaceBrush";
        private const string SurfaceSecondaryBrush = "AppSurfaceSecondaryBrush";
        private const string BorderBrush = "AppBorderBrush";
        private const string TextBrush = "AppTextBrush";
        private const string SecondaryTextBrush = "AppSecondaryTextBrush";
        private const string HoverBrush = "AppHoverBrush";
        private const string LogoBrush = "AppLogoBrush";
        private const string IconBrush = "AppIconBrush";


        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        private static string SettingsDirectory =>
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "StageFlow");

        private static string SettingsFile =>
            Path.Combine(SettingsDirectory, "application-theme.json");

        public IReadOnlyList<ApplicationTheme> Themes =>
            ApplicationTheme.BuiltInThemes;

        public ApplicationTheme CurrentTheme { get; private set; } =
            ApplicationTheme.BuiltInThemes[0];

        public ApplicationThemeService()
        {
        }

        public void Initialize()
        {
            var savedThemeName = LoadSavedThemeName();

            var theme = Themes.FirstOrDefault(
                t => string.Equals(
                    t.Name,
                    savedThemeName,
                    StringComparison.OrdinalIgnoreCase));

            ApplyTheme(
                theme ?? ApplicationTheme.BuiltInThemes[0],
                save: false);
        }

        public void ApplyTheme(ApplicationTheme theme)
        {
            ApplyTheme(theme, save: true);
        }

        private void ApplyTheme(
            ApplicationTheme theme,
            bool save)
        {
            if (Application.Current is null)
                return;

            CurrentTheme = theme;

            SetBrush(PrimaryBrush, theme.PrimaryColor);
            SetBrush(AccentBrush, theme.AccentColor);
            SetBrush(BackgroundBrush, theme.BackgroundColor);
            SetBrush(SurfaceBrush, theme.SurfaceColor);
            SetBrush(
                SurfaceSecondaryBrush,
                theme.SurfaceSecondaryColor);

            SetBrush(BorderBrush, theme.BorderColor);
            SetBrush(TextBrush, theme.TextColor);
            SetBrush(
                SecondaryTextBrush,
                theme.SecondaryTextColor);

            SetBrush(HoverBrush, theme.HoverColor);
            SetBrush(TextBrush, theme.TextColor);
            SetBrush(SecondaryTextBrush, theme.SecondaryTextColor);

            SetBrush(LogoBrush, theme.LogoColor);
            SetBrush(IconBrush, theme.IconColor);
            SetBrush(
    "AppBibleVerseTextBrush",
    theme.IsDark ? "#000000" : "#111111");



            if (save)
                SaveThemeName(theme.Name);
        }

        private static void SetBrush(
            string key,
            string color)
        {
            if (Application.Current is null)
                return;

            Application.Current.Resources[key] =
                new SolidColorBrush(
                    Color.Parse(color));
        }

        private static string? LoadSavedThemeName()
        {
            try
            {
                if (!File.Exists(SettingsFile))
                    return null;

                var json = File.ReadAllText(SettingsFile);

                var settings =
                    JsonSerializer.Deserialize<
                        ApplicationThemeSettings>(
                        json,
                        JsonOptions);

                return settings?.ThemeName;
            }
            catch
            {
                return null;
            }
        }

        private static void SaveThemeName(string themeName)
        {
            try
            {
                Directory.CreateDirectory(SettingsDirectory);

                var settings = new ApplicationThemeSettings
                {
                    ThemeName = themeName
                };

                var json =
                    JsonSerializer.Serialize(
                        settings,
                        JsonOptions);

                File.WriteAllText(
                    SettingsFile,
                    json);
            }
            catch
            {
                // Une erreur de sauvegarde du thème
                // ne doit jamais empêcher StageFlow
                // de fonctionner.
            }
        }

        private sealed class ApplicationThemeSettings
        {
            public string ThemeName { get; set; } =
                string.Empty;
        }
    }
}
