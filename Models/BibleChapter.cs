using System;
using System.Collections.Generic;
using System.Text;

namespace StageFlow.Models
{
    public sealed class BibleChapter
    {
        public string BookId { get; set; } = string.Empty;

        public string BookName { get; set; } = string.Empty;

        public int Number { get; set; }

        public List<BibleVerse> Verses { get; set; } = new();

        public string Reference =>
            $"{BookName} {Number}";
    }
}
