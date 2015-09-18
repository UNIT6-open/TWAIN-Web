using System;

namespace TwainWeb.Standalone.App.Twain
{
    public static class TwainExtensions
    {
        public static TwPixelType SearchPixelType(int key, TwPixelType def)
        {
            var resultSearch = def;
            foreach (var pixelType in (TwPixelType[])Enum.GetValues(typeof(TwPixelType)))
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
