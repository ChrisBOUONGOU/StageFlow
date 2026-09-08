using System;
using System.Collections.Generic;
using System.Text;

using StageFlow.Models;

namespace StageFlow.Services
{
    public sealed class TemplateService
    {
        public IReadOnlyList<Slide>
        GetTemplates(
            PresentationTheme theme)
        {
            return
            [
                CreateTitleTemplate(theme),
            CreateContentTemplate(theme),
            CreateQuoteTemplate(theme),
            CreateAnnouncementTemplate(theme)
            ];
        }

        private static Slide CreateTitleTemplate(
            PresentationTheme theme)
        {
            return new Slide
            {
                Title = "Title",
                Background =
                    theme.BackgroundColor,

                Elements =
                [
                    new TextElement
                {
                    Name = "Title",
                    Text = "Presentation Title",
                    FontFamily =
                        theme.FontFamily,
                    FontSize =
                        theme.TitleFontSize,
                    IsBold =
                        theme.TitleBold,
                    Color =
                        theme.PrimaryColor,
                    HorizontalAlignment =
                        "Center",
                    VerticalAlignment =
                        "Center",
                    X = 160,
                    Y = 390,
                    Width = 1600,
                    Height = 150,
                    ZIndex = 1
                }
                ]
            };
        }

        private static Slide CreateContentTemplate(
            PresentationTheme theme)
        {
            return new Slide
            {
                Title = "Content",
                Background =
                    theme.BackgroundColor,

                Elements =
                [
                    new TextElement
                {
                    Name = "Title",
                    Text = "Section Title",
                    FontFamily =
                        theme.FontFamily,
                    FontSize = 64,
                    IsBold = true,
                    Color =
                        theme.PrimaryColor,
                    HorizontalAlignment =
                        "Left",
                    VerticalAlignment =
                        "Center",
                    X = 120,
                    Y = 100,
                    Width = 1680,
                    Height = 100,
                    ZIndex = 2
                },

                new ShapeElement
                {
                    Name = "Accent",
                    ShapeType = "Accent",
                    Fill =
                        theme.AccentColor,
                    X = 120,
                    Y = 210,
                    Width = 260,
                    Height = 8,
                    ZIndex = 1
                },

                new TextElement
                {
                    Name = "Content",
                    Text = "Add your content here.",
                    FontFamily =
                        theme.FontFamily,
                    FontSize =
                        theme.BodyFontSize,
                    Color =
                        theme.SecondaryColor,
                    HorizontalAlignment =
                        "Left",
                    VerticalAlignment =
                        "Top",
                    X = 120,
                    Y = 280,
                    Width = 1680,
                    Height = 500,
                    ZIndex = 2
                }
                ]
            };
        }

        private static Slide CreateQuoteTemplate(
            PresentationTheme theme)
        {
            return new Slide
            {
                Title = "Quote",
                Background =
                    theme.BackgroundColor,

                Elements =
                [
                    new TextElement
                {
                    Name = "Quote",
                    Text = "“Your quote goes here.”",
                    FontFamily =
                        theme.FontFamily,
                    FontSize = 58,
                    IsItalic = true,
                    Color =
                        theme.PrimaryColor,
                    HorizontalAlignment =
                        "Center",
                    VerticalAlignment =
                        "Center",
                    X = 180,
                    Y = 330,
                    Width = 1560,
                    Height = 220,
                    ZIndex = 1
                },

                new TextElement
                {
                    Name = "Author",
                    Text = "— Author",
                    FontFamily =
                        theme.FontFamily,
                    FontSize = 32,
                    Color =
                        theme.AccentColor,
                    HorizontalAlignment =
                        "Center",
                    VerticalAlignment =
                        "Center",
                    X = 500,
                    Y = 600,
                    Width = 920,
                    Height = 70,
                    ZIndex = 2
                }
                ]
            };
        }

        private static Slide CreateAnnouncementTemplate(
            PresentationTheme theme)
        {
            return new Slide
            {
                Title = "Announcement",
                Background =
                    theme.BackgroundColor,

                Elements =
                [
                    new ShapeElement
                {
                    Name = "Accent",
                    ShapeType = "Accent",
                    Fill =
                        theme.AccentColor,
                    X = 0,
                    Y = 0,
                    Width = 1920,
                    Height = 18,
                    ZIndex = 1
                },

                new TextElement
                {
                    Name = "Announcement",
                    Text = "ANNOUNCEMENT",
                    FontFamily =
                        theme.FontFamily,
                    FontSize = 34,
                    IsBold = true,
                    Color =
                        theme.AccentColor,
                    HorizontalAlignment =
                        "Center",
                    VerticalAlignment =
                        "Center",
                    X = 200,
                    Y = 260,
                    Width = 1520,
                    Height = 70,
                    ZIndex = 2
                },

                new TextElement
                {
                    Name = "Message",
                    Text = "Your announcement",
                    FontFamily =
                        theme.FontFamily,
                    FontSize =
                        theme.TitleFontSize,
                    IsBold =
                        theme.TitleBold,
                    Color =
                        theme.PrimaryColor,
                    HorizontalAlignment =
                        "Center",
                    VerticalAlignment =
                        "Center",
                    X = 150,
                    Y = 380,
                    Width = 1620,
                    Height = 200,
                    ZIndex = 2
                }
                ]
            };
        }
    }
}
