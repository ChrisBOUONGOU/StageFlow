using System;
using System.Collections.Generic;
using System.Text;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using StageFlow.Models;
using System;
using System.IO;
using System.Linq;

namespace StageFlow.Services
{
    public static class SlideRenderer
    {
        public static Control CreateSlide(
        Slide slide,
        string? projectFilePath = null)
        {
            var canvas = new Canvas
            {
                Width = 1920,
                Height = 1080,
                Background = Brush.Parse(
                    string.IsNullOrWhiteSpace(slide.Background)
                        ? "#000000"
                        : slide.Background)
            };

            foreach (var element in slide.Elements
                         .Where(x => x.IsVisible)
                         .OrderBy(x => x.ZIndex))
            {
                var control = CreateElement(
                    element,
                    projectFilePath);

                if (control == null)
                    continue;

                Canvas.SetLeft(
                    control,
                    element.X);

                Canvas.SetTop(
                    control,
                    element.Y);

                control.Width = element.Width;
                control.Height = element.Height;
                control.Opacity = element.Opacity;

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

                control.SetValue(
    Panel.ZIndexProperty,
    element.ZIndex);

                canvas.Children.Add(control);
            }

            return canvas;
        }

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

        private static Control CreateText(
            TextElement text)
        {
            var textBlock = new TextBlock
            {
                Text = text.Text,
                FontFamily =
                    new FontFamily(text.FontFamily),
                FontSize = text.FontSize,
                Foreground =
                    Brush.Parse(text.Color),
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
                textBlock.FontWeight =
                    FontWeight.Bold;

            if (text.IsItalic)
                textBlock.FontStyle =
                    FontStyle.Italic;

            return new Border
            {
                Width = text.Width,
                Height = text.Height,
                Background = Brushes.Transparent,
                Child = textBlock
            };
        }

        private static Control CreateShape(
            ShapeElement shape)
        {
            return new Border
            {
                Width = shape.Width,
                Height = shape.Height,
                Background =
                    Brush.Parse(shape.Fill),
                BorderBrush =
                    Brush.Parse(shape.Stroke),
                BorderThickness =
                    new Thickness(
                        shape.StrokeThickness)
            };
        }

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
                    new Bitmap(imagePath);

                var imageControl =
                    new Avalonia.Controls.Image
                    {
                        Source = bitmap,

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
                    Width = image.Width,
                    Height = image.Height,
                    Background =
                        Brushes.Transparent,
                    Child = imageControl
                };
            }
            catch
            {
                return CreatePlaceholder(
                    "UNABLE TO LOAD IMAGE",
                    "#3A2020");
            }
        }

        private static Control CreateVideo(
            VideoElement video,
            string? projectFilePath)
        {
            /*
             * Le MediaElement pourra être branché ici
             * lorsque ton moteur vidéo de StageFlow
             * sera utilisé par la présentation.
             */

            return new Border
            {
                Width = video.Width,
                Height = video.Height,
                Background = Brushes.Black,
                Child = new TextBlock
                {
                    Text = "VIDEO",
                    FontSize = 24,
                    Foreground = Brushes.White,
                    HorizontalAlignment =
                        HorizontalAlignment.Center,
                    VerticalAlignment =
                        VerticalAlignment.Center
                }
            };
        }

        private static Control CreatePlaceholder(
            string text,
            string background)
        {
            return new Border
            {
                Background =
                    Brush.Parse(background),

                BorderBrush =
                    Brushes.Gray,

                BorderThickness =
                    new Thickness(1),

                Child = new TextBlock
                {
                    Text = text,

                    Foreground =
                        Brushes.White,

                    HorizontalAlignment =
                        HorizontalAlignment.Center,

                    VerticalAlignment =
                        VerticalAlignment.Center
                }
            };
        }

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
    }
}
