using System;
using System.Collections.Generic;
using System.Text;

namespace StageFlow.Models
{
    public sealed class ShapeElement : SlideElement
    {
        public string ShapeType { get; set; } = "Rectangle";

        public string Fill { get; set; } = "#FFFFFF";

        public string Stroke { get; set; } = "#FFFFFF";

        public double StrokeThickness { get; set; }
    }
}
