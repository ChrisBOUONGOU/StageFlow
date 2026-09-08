using System;
using System.Collections.Generic;
using System.Text;

using System.Security.Cryptography;
using System.Text.Json;
using StageFlow.Models;

namespace StageFlow.Services
{
    public sealed class FreeBibleProvider : IBibleProvider
    {
        private const string BaseUrl = "https://free.bible/bible";

        private static readonly HttpClient HttpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        private readonly string _cacheDirectory;

        private readonly List<BibleTranslation> _translations =
        [
            new BibleTranslation
        {
            Id = "segond",
            Name = "Louis Segond",
            ShortName = "SEGOND",
            Language = BibleLanguage.French
        },

        new BibleTranslation
        {
            Id = "web",
            Name = "World English Bible",
            ShortName = "WEB",
            Language = BibleLanguage.English
        },

        new BibleTranslation
        {
            Id = "rv1909",
            Name = "Reina-Valera 1909",
            ShortName = "RV1909",
            Language = BibleLanguage.Spanish
        },

        new BibleTranslation
        {
            Id = "pt",
            Name = "Bíblia Portuguesa Mundial",
            ShortName = "BPM",
            Language = BibleLanguage.Portuguese
        }
        ];

        public FreeBibleProvider()
        {
            _cacheDirectory = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "StageFlow",
                "BibleCache");

            Directory.CreateDirectory(_cacheDirectory);
        }

        public Task<IReadOnlyList<BibleTranslation>> GetTranslationsAsync()
        {
            return Task.FromResult<IReadOnlyList<BibleTranslation>>(
                _translations);
        }

        public async Task<IReadOnlyList<BibleBook>> GetBooksAsync(
            BibleTranslation translation)
        {
            var url =
                $"{BaseUrl}/{translation.Id}/index.json";

            var json = await GetCachedJsonAsync(url);

            if (string.IsNullOrWhiteSpace(json))
                return [];

            return ParseBooks(json);
        }

        public async Task<BibleChapter?> GetChapterAsync(
            BibleTranslation translation,
            BibleBook book,
            int chapter)
        {
            if (chapter <= 0)
                return null;

            var url =
                $"{BaseUrl}/{translation.Id}/{book.Id}/{chapter}.json";

            var json = await GetCachedJsonAsync(url);

            if (string.IsNullOrWhiteSpace(json))
                return null;

            return ParseChapter(
                json,
                book,
                chapter);
        }

        public async Task<IReadOnlyList<BibleVerse>> SearchAsync(
            BibleTranslation translation,
            BibleBook book,
            int chapter,
            string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return [];

            var loadedChapter = await GetChapterAsync(
                translation,
                book,
                chapter);

            if (loadedChapter == null)
                return [];

            return loadedChapter.Verses
                .Where(v =>
                    v.Text.Contains(
                        searchText,
                        StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private async Task<string?> GetCachedJsonAsync(string url)
        {
            var cacheFile = GetCacheFile(url);

            if (File.Exists(cacheFile))
            {
                try
                {
                    var fileInfo = new FileInfo(cacheFile);

                    if (DateTime.UtcNow - fileInfo.LastWriteTimeUtc <
                        TimeSpan.FromDays(30))
                    {
                        return await File.ReadAllTextAsync(cacheFile);
                    }
                }
                catch
                {
                    // Re-download below.
                }
            }

            try
            {
                using var response =
                    await HttpClient.GetAsync(url);

                response.EnsureSuccessStatusCode();

                var json =
                    await response.Content.ReadAsStringAsync();

                await File.WriteAllTextAsync(
                    cacheFile,
                    json);

                return json;
            }
            catch
            {
                // If the network is unavailable, use an existing cache.
                if (File.Exists(cacheFile))
                {
                    return await File.ReadAllTextAsync(cacheFile);
                }

                return null;
            }
        }

        private string GetCacheFile(string url)
        {
            var hash = SHA256.HashData(
                Encoding.UTF8.GetBytes(url));

            var fileName =
                Convert.ToHexString(hash).ToLowerInvariant() +
                ".json";

            return Path.Combine(
                _cacheDirectory,
                fileName);
        }

        private static List<BibleBook> ParseBooks(string json)
        {
            var result = new List<BibleBook>();

            try
            {
                using var document =
                    JsonDocument.Parse(json);

                var root = document.RootElement;

                JsonElement booksElement;

                if (root.ValueKind == JsonValueKind.Array)
                {
                    booksElement = root;
                }
                else if (
                    root.TryGetProperty(
                        "books",
                        out var books))
                {
                    booksElement = books;
                }
                else
                {
                    return result;
                }

                foreach (var item in booksElement.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.Object)
                        continue;

                    var id = GetString(
                        item,
                        "id",
                        "book",
                        "slug");

                    var name = GetString(
                        item,
                        "name",
                        "title");

                    var abbreviation = GetString(
                        item,
                        "abbreviation",
                        "abbr",
                        "shortName");

                    var chapterCount = GetInt(
                        item,
                        "chapters",
                        "chapterCount",
                        "count");

                    if (string.IsNullOrWhiteSpace(id))
                        continue;

                    if (string.IsNullOrWhiteSpace(name))
                        name = id;

                    result.Add(
                        new BibleBook
                        {
                            Id = id,
                            Name = name,
                            Abbreviation = abbreviation,
                            ChapterCount = chapterCount
                        });
                }
            }
            catch
            {
                // Return empty list if the JSON is invalid.
            }

            return result;
        }

        private static BibleChapter? ParseChapter(
            string json,
            BibleBook book,
            int chapterNumber)
        {
            try
            {
                using var document =
                    JsonDocument.Parse(json);

                var root = document.RootElement;

                var chapter = new BibleChapter
                {
                    BookId = book.Id,
                    BookName = book.Name,
                    Number = chapterNumber
                };

                if (!root.TryGetProperty(
                        "verses",
                        out var verses))
                {
                    return chapter;
                }

                foreach (var item in verses.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.Object)
                        continue;

                    var verseNumber = GetInt(
                        item,
                        "v",
                        "verse");

                    var text = GetString(
                        item,
                        "t",
                        "text");

                    if (verseNumber <= 0)
                        continue;

                    chapter.Verses.Add(
                        new BibleVerse
                        {
                            Verse = verseNumber,
                            Text = text,
                            BookName = book.Name,
                            Chapter = chapterNumber
                        });
                }

                return chapter;
            }
            catch
            {
                return null;
            }
        }

        private static string GetString(
            JsonElement element,
            params string[] properties)
        {
            foreach (var property in properties)
            {
                if (!element.TryGetProperty(
                        property,
                        out var value))
                {
                    continue;
                }

                if (value.ValueKind == JsonValueKind.String)
                    return value.GetString() ?? string.Empty;
            }

            return string.Empty;
        }

        private static int GetInt(
            JsonElement element,
            params string[] properties)
        {
            foreach (var property in properties)
            {
                if (!element.TryGetProperty(
                        property,
                        out var value))
                {
                    continue;
                }

                if (value.ValueKind == JsonValueKind.Number &&
                    value.TryGetInt32(out var number))
                {
                    return number;
                }

                if (value.ValueKind == JsonValueKind.String &&
                    int.TryParse(
                        value.GetString(),
                        out number))
                {
                    return number;
                }
            }

            return 0;
        }
    }
}
