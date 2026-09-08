using System;
using System.Collections.Generic;
using System.Text;

namespace StageFlow.Models
{
    public sealed class LyricsDocument
    {
        public string Title { get; set; } = "Untitled Song";

        public string Artist { get; set; } = string.Empty;

        public List<LyricsSection> Sections { get; set; } = new();
    }


    public sealed class LyricsSection
    {
        public string Name { get; set; } = string.Empty;

        public List<string> Lines { get; set; } = new();
    }
}
