using System;
using System.Collections.Generic;
using System.Text;
using TwainWeb.Standalone.App;

namespace TwainWeb.Standalone.Twain
{
    public class SettingsAcquire
    {
        public float Resolution { get; set; }
        public TwPixelType pixelType { get; set; }
        public FormatPage Format { get; set; }
    }
}
