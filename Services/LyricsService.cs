using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using StageFlow.Models;

namespace StageFlow.Services
{
    public sealed class LyricsService
    {
        public async Task<LyricsDocument> ImportAsync(
        string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "Lyrics file not found.",
                    filePath);
            }

            string content =
                await File.ReadAllTextAsync(filePath);

            string title =
                Path.GetFileNameWithoutExtension(filePath);

            return Parse(content, title);
        }

        public LyricsDocument Parse(
            string content,
            string title = "Untitled Song")
        {
            var document = new LyricsDocument
            {
                Title = title
            };

            if (string.IsNullOrWhiteSpace(content))
                return document;

            string normalized =
                content.Replace("\r\n", "\n")
                       .Replace('\r', '\n');

            string[] lines =
                normalized.Split('\n');

            LyricsSection? currentSection = null;

            foreach (string rawLine in lines)
            {
                string line =
                    rawLine.Trim();

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // LRC timestamp:
                // [00:12.50]Amazing grace
                line = RemoveLrcTimestamp(line);

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Section markers:
                // [Verse 1]
                // [Chorus]
                // [Bridge]
                if (IsSectionMarker(line))
                {
                    string sectionName =
                        line[1..^1].Trim();

                    currentSection =
                        new LyricsSection
                        {
                            Name = sectionName
                        };

                    document.Sections.Add(
                        currentSection);

                    continue;
                }

                if (currentSection == null)
                {
                    currentSection =
                        new LyricsSection
                        {
                            Name = "Lyrics"
                        };

                    document.Sections.Add(
                        currentSection);
                }

                currentSection.Lines.Add(line);
            }

            return document;
        }

        public List<Slide> CreateSlides(
            LyricsDocument lyrics,
            PresentationTheme theme,
            int linesPerSlide = 4)
        {
            var slides = new List<Slide>();

            foreach (
                LyricsSection section
                in lyrics.Sections)
            {
                if (section.Lines.Count == 0)
                    continue;

                for (
                    int index = 0;
                    index < section.Lines.Count;
                    index += linesPerSlide)
                {
                    List<string> chunk =
                        section.Lines
                            .Skip(index)
                            .Take(linesPerSlide)
                            .ToList();

                    slides.Add(
                        CreateLyricsSlide(
                            lyrics,
                            section,
                            chunk,
                            theme,
                            index / linesPerSlide + 1));
                }
            }

            return slides;
        }

        private static Slide CreateLyricsSlide(
            LyricsDocument lyrics,
            LyricsSection section,
            List<string> lines,
            PresentationTheme theme,
            int part)
        {
            string text =
                string.Join(
                    Environment.NewLine,
                    lines);

            return new Slide
            {
                Title =
                    $"{lyrics.Title} - {section.Name} {part}",

                Background =
                    theme.BackgroundColor,

                Elements =
                [
                    new TextElement
                {
                    Name = "Lyrics",
                    Text = text,

                    FontFamily =
                        theme.FontFamily,

                    FontSize =
                        theme.BodyFontSize,

                    IsBold = false,

                    Color =
                        theme.PrimaryColor,

                    HorizontalAlignment =
                        "Center",

                    VerticalAlignment =
                        "Center",

                    X = 140,
                    Y = 220,

                    Width = 1640,
                    Height = 640,

                    ZIndex = 10
                }
                ]
            };
        }

        private static bool IsSectionMarker(
            string line)
        {
            if (!line.StartsWith('[') ||
                !line.EndsWith(']'))
            {
                return false;
            }

            string value =
                line[1..^1].Trim();

            if (string.IsNullOrWhiteSpace(value))
                return false;

            // Don't treat LRC timestamps as section names.
            return !Regex.IsMatch(
                value,
                @"^\d{1,2}:\d{2}(?:\.\d+)?$");
        }

        private static string RemoveLrcTimestamp(
            string line)
        {
            return Regex.Replace(
                line,
                @"^\[\d{1,2}:\d{2}(?:\.\d+)?\]\s*",
                string.Empty);
        }
    }
}
