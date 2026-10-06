using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;
using StageFlow.Models;
using System;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace StageFlow.Services
{
    public static class SlideRenderer
    {
        // =========================================================
        // LIBVLC
        // =========================================================

        private static readonly LibVLC _libVLC =
            new LibVLC();

        // Un MediaPlayer par vidéo
        private static readonly
            Dictionary<Guid, MediaPlayer> _videoPlayers =
                new();

        // =========================================================
        // CRÉATION DU SLIDE
        // =========================================================

        public static Control CreateSlide(
            Slide slide,
            string? projectFilePath = null,
            bool presentationMode= true)
        {
            var canvas = new Canvas
            {
                Width = 1920,
                Height = 1080,

                Background =
                    Brush.Parse(
                        string.IsNullOrWhiteSpace(
                            slide.Background)
                            ? "#000000"
                            : slide.Background)
            };

            foreach (var element in
                     slide.Elements
                         .Where(x => x.IsVisible)
                         .OrderBy(x => x.ZIndex))
            {
                var control =
                    CreateElement(
                        element,
                        projectFilePath);

                if (control == null)
                    continue;

                // =================================================
                // POSITION
                // =================================================

                Canvas.SetLeft(
                    control,
                    element.X);

                Canvas.SetTop(
                    control,
                    element.Y);



                // =================================================
                // DIMENSIONS
                // =================================================

                control.Width =
                    element.Width;

                control.Height =
                    element.Height;

                // =================================================
                // OPACITÉ
                // =================================================

                control.Opacity =
                    element.Opacity;

                // =================================================
                // ROTATION
                // =================================================

                if (element.Rotation != 0)
                {
                    control.RenderTransform =
                        new RotateTransform(
                            element.Rotation);

                    control.RenderTransformOrigin =
                        new RelativePoint(
                            0.5,
                            0.5,
                            RelativeUnit.Relative);
                }

                if (presentationMode &&
    element is VideoElement)
                {
                    Canvas.SetLeft(control, 0);
                    Canvas.SetTop(control, 0);

                    control.Width = 1600;
                    control.Height = 970;
                }

                // =================================================
                // Z-INDEX
                // =================================================

                control.SetValue(
                    Panel.ZIndexProperty,
                    element.ZIndex);

                canvas.Children.Add(
                    control);


            }

            return canvas;
        }

        // =========================================================
        // CRÉATION DES ELEMENTS
        // =========================================================

        private static Control? CreateElement(
            SlideElement element,
            string? projectFilePath)
        {
            return element switch
            {
                TextElement text =>
                    CreateText(text),

                ShapeElement shape =>
                    CreateShape(shape),

                ImageElement image =>
                    CreateImage(
                        image,
                        projectFilePath),

                VideoElement video =>
                    CreateVideo(
                        video,
                        projectFilePath),

                _ => null
            };
        }

        // =========================================================
        // TEXTE
        // =========================================================

        private static Control CreateText(
            TextElement text)
        {
            var textBlock =
                new TextBlock
                {
                    Text =
                        text.Text,

                    FontFamily =
                        new FontFamily(
                            text.FontFamily),

                    FontSize =
                        text.FontSize,

                    Foreground =
                        Brush.Parse(
                            text.Color),

                    TextWrapping =
                        TextWrapping.Wrap,

                    HorizontalAlignment =
                        ConvertHorizontalAlignment(
                            text.HorizontalAlignment),

                    VerticalAlignment =
                        ConvertVerticalAlignment(
                            text.VerticalAlignment)
                };

            if (text.IsBold)
            {
                textBlock.FontWeight =
                    FontWeight.Bold;
            }

            if (text.IsItalic)
            {
                textBlock.FontStyle =
                    FontStyle.Italic;
            }

            return new Border
            {
                Width = 1920,

                Height = 1080,

                Background =
                    Brushes.Transparent,

                Child =
                    textBlock
            };
        }

        // =========================================================
        // FORME
        // =========================================================

        private static Control CreateShape(
            ShapeElement shape)
        {
            return new Border
            {
                Width =
                    shape.Width,

                Height =
                    shape.Height,

                Background =
                    Brush.Parse(
                        shape.Fill),

                BorderBrush =
                    Brush.Parse(
                        shape.Stroke),

                BorderThickness =
                    new Thickness(
                        shape.StrokeThickness)
            };
        }

        // =========================================================
        // IMAGE
        // =========================================================

        private static Control CreateImage(
            ImageElement image,
            string? projectFilePath)
        {
            if (string.IsNullOrWhiteSpace(
                    image.Source))
            {
                return CreatePlaceholder(
                    "IMAGE",
                    "#202631");
            }

            try
            {
                string imagePath =
                    ResolveAssetPath(
                        image.Source,
                        projectFilePath);

                if (!File.Exists(imagePath))
                {
                    return CreatePlaceholder(
                        "IMAGE NOT FOUND",
                        "#3A2020");
                }

                var bitmap =
                    new Bitmap(
                        imagePath);

                var imageControl =
                    new Avalonia.Controls.Image
                    {
                        Source =
                            bitmap,

                        Stretch =
                            image.StretchUniform
                                ? Stretch.Uniform
                                : Stretch.Fill,

                        HorizontalAlignment =
                            HorizontalAlignment.Stretch,

                        VerticalAlignment =
                            VerticalAlignment.Stretch
                    };

                return new Border
                {
                    Width =
                        image.Width,

                    Height =
                        image.Height,

                    Background =
                        Brushes.Transparent,

                    Child =
                        imageControl
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"Image rendering error: {ex}");

                return CreatePlaceholder(
                    "UNABLE TO LOAD IMAGE",
                    "#3A2020");
            }
        }

        // =========================================================
        // VIDEO
        // =========================================================

        private static Control CreateVideo(
            VideoElement video,
            string? projectFilePath)
        {
            // -----------------------------------------------------
            // SOURCE VIDE
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(
                    video.Source))
            {
                return CreatePlaceholder(
                    "VIDEO",
                    "#202631");
            }

            try
            {
                // -------------------------------------------------
                // CHEMIN DE LA VIDÉO
                // -------------------------------------------------

                string videoPath =
                    ResolveAssetPath(
                        video.Source,
                        projectFilePath);

                if (!File.Exists(videoPath))
                {
                    return CreatePlaceholder(
                        "VIDEO NOT FOUND",
                        "#3A2020");
                }

                // -------------------------------------------------
                // MEDIA PLAYER
                // -------------------------------------------------

                if (!_videoPlayers.TryGetValue(
                        video.Id,
                        out MediaPlayer? mediaPlayer))
                {
                    mediaPlayer =
        new MediaPlayer(_libVLC);

                    _videoPlayers[video.Id] =
                        mediaPlayer;

                    mediaPlayer.EndReached +=
                        (_, _) =>
                        {
                            if (!video.Loop)
                                return;

                            try
                            {
                                // Revenir au début sans faire Stop()
                                // pour éviter les problèmes avec LibVLC.
                                mediaPlayer.Time = 0;

                                mediaPlayer.Play();
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(
                                    $"Video loop error: {ex}");
                            }
                        };
                }

                // -------------------------------------------------
                // VIDEO VIEW
                // -------------------------------------------------

                var videoView =
                    new LibVLCSharp
                        .Avalonia
                        .VideoView
                    {
                        MediaPlayer =
                            mediaPlayer,

                        HorizontalAlignment =
                            HorizontalAlignment.Stretch,

                        VerticalAlignment =
                            VerticalAlignment.Stretch,

                        IsHitTestVisible =
                            false
                    };

                // -------------------------------------------------
                // MEDIA
                // -------------------------------------------------

                if (mediaPlayer.Media == null)
                {
                    var media =
                        new Media(
                            _libVLC,
                            videoPath,
                            FromType.FromPath);

                    mediaPlayer.Media =
                        media;
                }

                // -------------------------------------------------
                // VOLUME
                // -------------------------------------------------

                mediaPlayer.Volume =
                    (int)Math.Clamp(
                        video.Volume * 100,
                        0,
                        100);

                // -------------------------------------------------
                // LECTURE AUTOMATIQUE
                // -------------------------------------------------

                if (video.AutoPlay &&
                    !mediaPlayer.IsPlaying)
                {
                    try
                    {
                        // Toujours repartir du début lorsqu'on
                        // revient sur le slide.
                        mediaPlayer.Time = 0;

                        mediaPlayer.Play();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(
                            $"Video playback error: {ex}");
                    }
                }

                // -------------------------------------------------
                // CONTAINER VIDÉO
                // -------------------------------------------------

                return new Border
                {
                    Width =
                        video.Width,

                    Height =
                        video.Height,

                    Background =
                        Brushes.Black,

                    BorderBrush =
                        Brushes.Transparent,

                    BorderThickness =
                        new Thickness(0),

                    Padding =
                        new Thickness(0),

                    HorizontalAlignment =
                        HorizontalAlignment.Stretch,

                    VerticalAlignment =
                        VerticalAlignment.Stretch,

                    Opacity =
                        video.Opacity,

                    Child =
                        videoView
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"Video rendering error: {ex}");

                return CreatePlaceholder(
                    "VIDEO ERROR",
                    "#3A2020");
            }
        }

        // =========================================================
        // PLACEHOLDER
        // =========================================================

        private static Control CreatePlaceholder(
            string text,
            string background)
        {
            return new Border
            {
                Background =
                    Brush.Parse(
                        background),

                BorderBrush =
                    Brushes.Gray,

                BorderThickness =
                    new Thickness(1),

                Child =
                    new TextBlock
                    {
                        Text =
                            text,

                        Foreground =
                            Brushes.White,

                        HorizontalAlignment =
                            HorizontalAlignment.Center,

                        VerticalAlignment =
                            VerticalAlignment.Center
                    }
            };
        }

        // =========================================================
        // RESOLUTION DES ASSETS
        // =========================================================

        private static string ResolveAssetPath(
            string source,
            string? projectFilePath)
        {
            if (string.IsNullOrWhiteSpace(
                    projectFilePath))
            {
                return source;
            }

            var assetService =
                new AssetService();

            return assetService.ResolveAssetPath(
                source,
                projectFilePath);
        }

        // =========================================================
        // ALIGNEMENT HORIZONTAL
        // =========================================================

        private static HorizontalAlignment
            ConvertHorizontalAlignment(
                string alignment)
        {
            return alignment switch
            {
                "Left" =>
                    HorizontalAlignment.Left,

                "Right" =>
                    HorizontalAlignment.Right,

                "Stretch" =>
                    HorizontalAlignment.Stretch,

                _ =>
                    HorizontalAlignment.Center
            };
        }

        // =========================================================
        // ALIGNEMENT VERTICAL
        // =========================================================

        private static VerticalAlignment
            ConvertVerticalAlignment(
                string alignment)
        {
            return alignment switch
            {
                "Top" =>
                    VerticalAlignment.Top,

                "Bottom" =>
                    VerticalAlignment.Bottom,

                "Stretch" =>
                    VerticalAlignment.Stretch,

                _ =>
                    VerticalAlignment.Center
            };
        }

        // =========================================================
        // ARRÊTER TOUTES LES VIDÉOS
        // =========================================================

        public static void StopAllVideos()
        {
            foreach (var player in _videoPlayers.Values)
            {
                try
                {
                    if (player.IsPlaying)
                    {
                        player.Stop();
                    }

                    // Très important :
                    // on remet la vidéo au début lorsqu'on quitte le slide.
                    if (player.Media != null)
                    {
                        player.Time = 0;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(
                        $"Error resetting video: {ex}");
                }
            }
        }
    }
}
