using System;
using System.Collections.Generic;
using System.Text;
using StageFlow.Models;

namespace StageFlow.Services
{
    public interface IBibleProvider
    {
        Task<IReadOnlyList<BibleTranslation>> GetTranslationsAsync();

        Task<IReadOnlyList<BibleBook>> GetBooksAsync(
            BibleTranslation translation);

        Task<BibleChapter?> GetChapterAsync(
            BibleTranslation translation,
            BibleBook book,
            int chapter);

        Task<IReadOnlyList<BibleVerse>> SearchAsync(
            BibleTranslation translation,
            BibleBook book,
            int chapter,
            string searchText);
    }
}
