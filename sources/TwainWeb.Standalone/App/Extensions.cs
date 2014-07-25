using System;
using System.Collections.Generic;
using System.Text;
using TwainWeb.Standalone.Twain;

namespace TwainWeb.Standalone.App
{
    public static class Extensions
    {
        public static TwPixelType SearchPixelType(int key, TwPixelType def)
        {
            TwPixelType resultSearch = def;
            foreach (TwPixelType pixelType in (TwPixelType[])Enum.GetValues(typeof(TwPixelType)))
            {
                if ((int)pixelType == key)
                {
                    resultSearch = pixelType;
                    break;
                }
            }
            return resultSearch;
        }
    }
}
