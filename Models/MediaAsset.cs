using System;
using System.Collections.Generic;
using System.Text;

namespace StageFlow.Models
{

    public enum MediaAssetType
    {
        Image,
        Video,
        Audio
    }

    public sealed class MediaAsset
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public string Name { get; set; } = "Asset";

        public string RelativePath { get; set; } = string.Empty;

        public MediaAssetType Type { get; set; }

        public long FileSize { get; set; }

        public DateTime ImportedAt { get; set; } = DateTime.UtcNow;

        public string? AbsolutePath { get; set; }

        public string Extension =>
            Path.GetExtension(RelativePath);


        public string TypeIcon =>
            Type switch
            {
                MediaAssetType.Image => "🖼",
                MediaAssetType.Video => "▶",
                MediaAssetType.Audio => "♫",
                _ => "?"
            };

        public string FileSizeText
        {
            get
            {
                if (FileSize < 1024)
                    return $"{FileSize} B";

                if (FileSize < 1024 * 1024)
                    return $"{FileSize / 1024.0:F1} KB";

                if (FileSize < 1024 * 1024 * 1024)
                    return $"{FileSize / (1024.0 * 1024):F1} MB";

                return $"{FileSize / (1024.0 * 1024 * 1024):F1} GB";
            }
        }
    }
}
