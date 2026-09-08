using System;
using System.Collections.Generic;
using System.Text;

namespace StageFlow.Models
{
    public sealed class BibleVerse
    {
        public int Verse { get; set; }

        public string Text { get; set; } = string.Empty;

        public string BookName { get; set; } = string.Empty;

        public int Chapter { get; set; }

        public string Reference =>
            $"{BookName} {Chapter}:{Verse}";

        public string DisplayText =>
            $"{Verse}. {Text}";
    }
}
