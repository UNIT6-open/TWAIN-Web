using System.Collections.Generic;
using System.Drawing.Imaging;
using TwainWeb.Standalone.App.Twain;

namespace TwainWeb.Standalone.App
{
    
    public static class GlobalDictionaries
    {
        public static List<string> Scans = new List<string>();

        public static Dictionary<string, ImageFormat> ImgFormats
        {
            get
            {
                return new Dictionary<string, ImageFormat>
                {
                    {"jpg", ImageFormat.Jpeg},
                    {ImageFormat.Bmp.ToString(), ImageFormat.Bmp}
                };
            }
        }

        public static string SearchingKeyInImgFormats(ImageFormat value)
        {
            var resultKey = value.ToString();
            foreach(var el in ImgFormats)
            {
                if (value == el.Value)
                    resultKey = el.Key;
            }
            return resultKey;
        }

        public static Dictionary<TwPixelType, string> PixelTypes
        {
            get
            {
                return new Dictionary<TwPixelType, string>
                {
                    {TwPixelType.RGB, "Цветное"},
                    {TwPixelType.BW, "Черно/белое"},
                    {TwPixelType.Gray, "Оттенки серого"}
                };
            }
        }
        public enum SaveAsValues 
        {
            Pictures,
            Pdf,
            Archive
        }

	    public static Dictionary<SaveAsValues, string> DefaultFileFormats = new Dictionary<SaveAsValues, string>
	    {
		    {SaveAsValues.Pictures, "jpg"},
			{SaveAsValues.Pdf, "pdf"},
			{SaveAsValues.Archive, "zip"}
	    };
        public static List<FormatPage> Formats
        {
            get
            {
                return new List<FormatPage>
                {
                    new FormatPage{Width=33.11f,Height= 46.81f,Name="A0"},
                    new FormatPage{Width=23.39f,Height= 33.11f,Name="A1"},
                    new FormatPage{Width=16.54f,Height= 23.39f,Name="A2"},                    
                    new FormatPage{Width=11.69f,Height= 16.54f,Name="A3"},
                    new FormatPage{Width=8.27f, Height= 11.69f,Name="A4"},
                    new FormatPage{Width=5.83f, Height= 8.27f,Name="A5"},
                    new FormatPage{Width=4.13f, Height= 5.84f,Name="A6"}
                };
            }
        }
               
    }
}
