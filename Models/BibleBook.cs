using System;
using System.Collections.Generic;
using System.Text;

namespace StageFlow.Models
{
    public sealed class BibleBook
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Abbreviation { get; set; } = string.Empty;

        public int ChapterCount { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
