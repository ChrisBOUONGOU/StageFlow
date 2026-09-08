using System;
using System.Collections.Generic;
using System.Text;

namespace StageFlow.Models
{
    public sealed class PresentationDocument
    {
        public int Version { get; set; } = 1;

        public Guid Id { get; init; } = Guid.NewGuid();

        public string Name { get; set; } = "Untitled Presentation";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

        public List<Slide> Slides { get; set; } = new();

        public PresentationSettings Settings { get; set; } = new();

        public List<MediaAsset> MediaAssets { get; set; } = new();
    }
}
