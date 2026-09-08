using System;
using System.Collections.Generic;
using System.Text;


namespace StageFlow.Models
{
    public sealed class Slide
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public string Title { get; set; } = "New Slide";

        public string Background { get; set; } = "#151922";

        public List<SlideElement> Elements { get; set; } = new();

        public TransitionSettings Transition { get; set; } = new();
    }
}
