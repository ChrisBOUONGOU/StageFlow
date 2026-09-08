using System;
using System.Collections.Generic;
using System.Text;

using StageFlow.Models;

namespace StageFlow.Services
{
    public sealed class ThemeService
    {
        public IReadOnlyList<PresentationTheme>
        GetBuiltInThemes()
        {
            return
                PresentationTheme
                    .CreateBuiltInThemes();
        }

        public PresentationTheme?
            FindTheme(string name)
        {
            return GetBuiltInThemes()
                .FirstOrDefault(
                    theme =>
                        string.Equals(
                            theme.Name,
                            name,
                            StringComparison.OrdinalIgnoreCase));
        }

        public void ApplyTheme(
            PresentationDocument document,
            PresentationTheme theme)
        {
            document.Settings.ThemeName =
                theme.Name;

            document.Settings.BackgroundColor =
                theme.BackgroundColor;

            document.Settings.FontFamily =
                theme.FontFamily;

            document.Settings.PrimaryColor =
                theme.PrimaryColor;

            document.Settings.SecondaryColor =
                theme.SecondaryColor;

            document.Settings.AccentColor =
                theme.AccentColor;

            foreach (Slide slide in document.Slides)
            {
                slide.Background =
                    theme.BackgroundColor;

                foreach (
                    SlideElement element
                    in slide.Elements)
                {
                    ApplyToElement(
                        element,
                        theme);
                }
            }
        }

        private static void ApplyToElement(
            SlideElement element,
            PresentationTheme theme)
        {
            if (element is TextElement text)
            {
                text.FontFamily =
                    theme.FontFamily;

                text.Color =
                    theme.PrimaryColor;

                if (text.FontSize <= 0)
                {
                    text.FontSize =
                        theme.BodyFontSize;
                }
            }

            if (element is ShapeElement shape)
            {
                if (shape.ShapeType ==
                    "Accent")
                {
                    shape.Fill =
                        theme.AccentColor;
                }
            }
        }
    }
}
