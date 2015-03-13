using System;
using TwainWeb.Standalone.Twain;

namespace TwainWeb.Standalone.App
{
    public static class Extensions
    {
        public static TwPixelType SearchPixelType(int key, TwPixelType def)
        {
            var resultSearch = def;
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
