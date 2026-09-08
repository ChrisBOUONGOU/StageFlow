using System;
using System.Collections.Generic;
using System.Text;
using StageFlow.Models;

namespace StageFlow.Services
{
    public sealed class BibleService
    {
        private readonly IBibleProvider _provider;

        public BibleService()
        {
            _provider = new FreeBibleProvider();
        }

        public BibleService(IBibleProvider provider)
        {
            _provider = provider;
        }

        public Task<IReadOnlyList<BibleTranslation>>
            GetTranslationsAsync()
        {
            return _provider.GetTranslationsAsync();
        }

        public Task<IReadOnlyList<BibleBook>>
            GetBooksAsync(
                BibleTranslation translation)
        {
            return _provider.GetBooksAsync(translation);
        }

        public Task<BibleChapter?>
            GetChapterAsync(
                BibleTranslation translation,
                BibleBook book,
                int chapter)
        {
            return _provider.GetChapterAsync(
                translation,
                book,
                chapter);
        }

        public Task<IReadOnlyList<BibleVerse>>
            SearchAsync(
                BibleTranslation translation,
                BibleBook book,
                int chapter,
                string searchText)
        {
            return _provider.SearchAsync(
                translation,
                book,
                chapter,
                searchText);
        }

        public List<Slide> CreateSlides(
            BibleChapter chapter,
            int versesPerSlide,
            PresentationTheme? theme = null)
        {
            versesPerSlide =
                Math.Max(1, versesPerSlide);

            var slides = new List<Slide>();

            for (
                var i = 0;
                i < chapter.Verses.Count;
                i += versesPerSlide)
            {
                var verses = chapter.Verses
                    .Skip(i)
                    .Take(versesPerSlide)
                    .ToList();

                var firstVerse = verses.First();
                var lastVerse = verses.Last();

                var reference =
                    firstVerse.Verse == lastVerse.Verse
                        ? $"{chapter.BookName} {chapter.Number}:{firstVerse.Verse}"
                        : $"{chapter.BookName} {chapter.Number}:{firstVerse.Verse}-{lastVerse.Verse}";

                var text = string.Join(
                    Environment.NewLine + Environment.NewLine,
                    verses.Select(v =>
                        $"{v.Verse}  {v.Text}"));

                var slide = new Slide
                {
                    Title = reference,
                    Background =
                        theme?.BackgroundColor ?? "#10131A"
                };

                var titleElement =
                    new TextElement
                    {
                        Name = "Bible Reference",
                        X = 100,
                        Y = 80,
                        Width = 1720,
                        Height = 100,
                        Text = reference,
                        FontFamily =
                            theme?.FontFamily ?? "Inter",
                        FontSize =
                            theme?.TitleFontSize ?? 42,
                        IsBold = true,
                        Color =
                            theme?.AccentColor ?? "#4F8CFF",
                        HorizontalAlignment = "Center",
                        VerticalAlignment = "Center"
                    };

                var bodyElement =
                    new TextElement
                    {
                        Name = "Bible Text",
                        X = 140,
                        Y = 230,
                        Width = 1640,
                        Height = 720,
                        Text = text,
                        FontFamily =
                            theme?.FontFamily ?? "Inter",
                        FontSize =
                            theme?.BodyFontSize ?? 46,
                        Color =
                            theme?.PrimaryColor ?? "#FFFFFF",
                        HorizontalAlignment = "Center",
                        VerticalAlignment = "Center"
                    };

                slide.Elements.Add(titleElement);
                slide.Elements.Add(bodyElement);

                slides.Add(slide);
            }

            return slides;
        }
    }
}
