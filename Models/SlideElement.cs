using Avalonia.Controls.Documents;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace StageFlow.Models
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(TextElement), "text")]
    [JsonDerivedType(typeof(ImageElement), "image")]
    [JsonDerivedType(typeof(VideoElement), "video")]
    [JsonDerivedType(typeof(ShapeElement), "shape")]
    public abstract class SlideElement
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Name { get; set; } = "Element";

        public double X { get; set; }
        public double Y { get; set; }

        public double Width { get; set; } = 400;
        public double Height { get; set; } = 100;

        public double Rotation { get; set; }

        public double Opacity { get; set; } = 1.0;

        public int ZIndex { get; set; }

        public bool IsVisible { get; set; } = true;

        
    }
}
