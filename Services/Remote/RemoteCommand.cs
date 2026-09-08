using System;
using System.Collections.Generic;
using System.Text;

namespace StageFlow.Services.Remote
{
    public sealed class RemoteCommand
    {
        public string Command { get; set; } = string.Empty;

        public int? SlideIndex { get; set; }
    }
}
