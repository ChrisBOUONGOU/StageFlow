using System;
using System.Collections.Generic;
using System.Text;

namespace StageFlow.Models
{
    public sealed class BibleTranslation
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string ShortName { get; set; } = string.Empty;

        public BibleLanguage Language { get; set; }

        public string LanguageName =>
            Language switch
            {
                BibleLanguage.French => "Français",
                BibleLanguage.English => "English",
                BibleLanguage.Spanish => "Español",
                BibleLanguage.Portuguese => "Português",
                _ => string.Empty
            };

        public override string ToString()
        {
            return $"{Name} ({ShortName})";
        }
    }
}
