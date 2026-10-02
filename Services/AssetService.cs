using System;
using System.Collections.Generic;
using System.Text;

using System.Security.Cryptography;
using StageFlow.Models;

namespace StageFlow.Services
{
    public sealed class AssetService
    {
        private static readonly string[] ImageExtensions =
        [
            ".png",
            ".jpg",
            ".jpeg",
            ".webp",
            ".bmp",
            ".gif"
        ];

        private static readonly string[] VideoExtensions =
        [
            ".mp4",
            ".mov",
            ".avi",
            ".mkv",
            ".webm",
            ".m4v"
        ];

        private static readonly string[] AudioExtensions =
        [
            ".mp3",
            ".wav",
            ".flac",
            ".aac",
            ".ogg",
            ".m4a"
        ];

        public string CreateProjectFolder(
            string stageflowFilePath)
        {
            string directory =
                Path.GetDirectoryName(stageflowFilePath)
                ?? Environment.CurrentDirectory;

            string projectName =
                Path.GetFileNameWithoutExtension(
                    stageflowFilePath);

            string projectFolder =
                Path.Combine(
                    directory,
                    projectName);

            Directory.CreateDirectory(projectFolder);

            string assetsFolder =
                Path.Combine(
                    projectFolder,
                    "Assets");

            Directory.CreateDirectory(assetsFolder);

            return assetsFolder;
        }

        public async Task<MediaAsset> ImportAssetAsync(
            string sourcePath,
            string stageflowFilePath)
        {
            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException(
                    "Asset not found.",
                    sourcePath);
            }

            string extension =
                Path.GetExtension(sourcePath)
                    .ToLowerInvariant();

            MediaAssetType type =
                GetAssetType(extension);

            string assetsFolder =
                CreateProjectFolder(
                    stageflowFilePath);

            string originalName =
                Path.GetFileNameWithoutExtension(
                    sourcePath);

            string hash =
                await CalculateHashAsync(
                    sourcePath);

            string safeName =
                SanitizeFileName(originalName);

            string destinationFileName =
                $"{safeName}_{hash[..8]}{extension}";

            string destinationPath =
                Path.Combine(
                    assetsFolder,
                    destinationFileName);

            if (!File.Exists(destinationPath))
            {
                await using FileStream source =
                    File.OpenRead(sourcePath);

                await using FileStream destination =
                    File.Create(destinationPath);

                await source.CopyToAsync(destination);
            }

            string projectFolder =
                Path.GetDirectoryName(
                    assetsFolder)!;

            string relativePath =
                Path.GetRelativePath(
                    projectFolder,
                    destinationPath);

            var fileInfo =
                new FileInfo(destinationPath);

            return new MediaAsset
            {
                Name = originalName,

                RelativePath =
                    relativePath.Replace(
                        '\\',
                        '/'),

                Type = type,

                FileSize =
                    fileInfo.Length,

                ImportedAt =
                    DateTime.UtcNow
            };
        }

        public async Task<MediaAsset> ImportTemporaryAssetAsync(
            string sourcePath)
        {
            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException(
                    "Asset not found.",
                    sourcePath);
            }

            string extension =
                Path.GetExtension(sourcePath)
                    .ToLowerInvariant();

            MediaAssetType type =
                GetAssetType(extension);

            string tempFolder =
                GetTemporaryAssetsFolder();

            Directory.CreateDirectory(tempFolder);

            string originalName =
                Path.GetFileNameWithoutExtension(
                    sourcePath);

            string hash =
                await CalculateHashAsync(sourcePath);

            string safeName =
                SanitizeFileName(originalName);

            string destinationFileName =
                $"{safeName}_{hash[..8]}{extension}";

            string destinationPath =
                Path.Combine(
                    tempFolder,
                    destinationFileName);

            if (!File.Exists(destinationPath))
            {
                await using FileStream source =
                    File.OpenRead(sourcePath);

                await using FileStream destination =
                    File.Create(destinationPath);

                await source.CopyToAsync(destination);
            }

            var fileInfo =
                new FileInfo(destinationPath);

            return new MediaAsset
            {
                Name = originalName,

                RelativePath =
                    destinationPath,

                Type = type,

                FileSize =
                    fileInfo.Length,

                ImportedAt =
                    DateTime.UtcNow
            };
        }

        public async Task ConvertTemporaryAssetsToProjectAsync(
            PresentationDocument document,
            string stageflowFilePath)
        {
            if (document.MediaAssets.Count == 0)
                return;

            string assetsFolder =
                CreateProjectFolder(
                    stageflowFilePath);

            string projectFolder =
                Path.GetDirectoryName(
                    assetsFolder)!;

            foreach (MediaAsset asset in document.MediaAssets)
            {
                if (string.IsNullOrWhiteSpace(
                        asset.RelativePath))
                    continue;

                if (!Path.IsPathRooted(
                        asset.RelativePath))
                    continue;

                string sourcePath =
                    asset.RelativePath;

                if (!File.Exists(sourcePath))
                    continue;

                string extension =
                    Path.GetExtension(sourcePath)
                        .ToLowerInvariant();

                string hash =
                    await CalculateHashAsync(
                        sourcePath);

                string safeName =
                    SanitizeFileName(
                        asset.Name);

                string destinationFileName =
                    $"{safeName}_{hash[..8]}{extension}";

                string destinationPath =
                    Path.Combine(
                        assetsFolder,
                        destinationFileName);

                if (!File.Exists(destinationPath))
                {
                    await using FileStream source =
                        File.OpenRead(sourcePath);

                    await using FileStream destination =
                        File.Create(destinationPath);

                    await source.CopyToAsync(
                        destination);
                }

                asset.RelativePath =
                    Path.GetRelativePath(
                        projectFolder,
                        destinationPath)
                    .Replace(
                        '\\',
                        '/');
            }
        }

        public string ResolveAssetPath(
            string relativePath,
            string stageflowFilePath)
        {
            if (Path.IsPathRooted(relativePath))
                return Path.GetFullPath(relativePath);

            string directory =
                Path.GetDirectoryName(
                    stageflowFilePath)
                ?? Environment.CurrentDirectory;

            string projectFolder =
                Path.Combine(
                    directory,
                    Path.GetFileNameWithoutExtension(
                        stageflowFilePath));

            return Path.GetFullPath(
                Path.Combine(
                    projectFolder,
                    relativePath));
        }

        public void DeleteAssetFile(
            MediaAsset asset,
            string stageflowFilePath)
        {
            string path =
                ResolveAssetPath(
                    asset.RelativePath,
                    stageflowFilePath);

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private static string GetTemporaryAssetsFolder()
        {
            string temp =
                Path.GetTempPath();

            return Path.Combine(
                temp,
                "StageFlow",
                "Assets");
        }

        public static bool IsSupported(
            string path)
        {
            string extension =
                Path.GetExtension(path)
                    .ToLowerInvariant();

            return
                ImageExtensions.Contains(extension) ||
                VideoExtensions.Contains(extension) ||
                AudioExtensions.Contains(extension);
        }

        public static MediaAssetType GetAssetType(
            string extension)
        {
            extension =
                extension.ToLowerInvariant();

            if (ImageExtensions.Contains(extension))
                return MediaAssetType.Image;

            if (VideoExtensions.Contains(extension))
                return MediaAssetType.Video;

            if (AudioExtensions.Contains(extension))
                return MediaAssetType.Audio;

            throw new NotSupportedException(
                $"Unsupported media type: {extension}");
        }

        private static async Task<string>
            CalculateHashAsync(string path)
        {
            await using FileStream stream =
                File.OpenRead(path);

            byte[] hash =
                await SHA256.HashDataAsync(stream);

            return Convert.ToHexString(hash)
                .ToLowerInvariant();
        }

        private static string SanitizeFileName(
            string name)
        {
            foreach (
                char invalid
                in Path.GetInvalidFileNameChars())
            {
                name =
                    name.Replace(
                        invalid,
                        '_');
            }

            return string.IsNullOrWhiteSpace(name)
                ? "asset"
                : name;
        }
    }
}
