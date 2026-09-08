using StageFlow.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StageFlow.Services
{
    public sealed class ProjectService
    {
        private readonly JsonSerializerOptions _options;

        public ProjectService()
        {
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task SaveAsync(
            PresentationDocument document,
            string filePath)
        {
            document.ModifiedAt = DateTime.UtcNow;

            string json = JsonSerializer.Serialize(
                document,
                _options);

            string? directory =
                Path.GetDirectoryName(filePath);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(
                filePath,
                json);
        }

        public async Task<PresentationDocument> LoadAsync(
            string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "StageFlow project not found.",
                    filePath);
            }

            string json =
                await File.ReadAllTextAsync(filePath);

            PresentationDocument? document =
                JsonSerializer.Deserialize<PresentationDocument>(
                    json,
                    _options);

            if (document == null)
            {
                throw new InvalidDataException(
                    "The StageFlow project is invalid.");
            }

            return document;
        }
    }
}
