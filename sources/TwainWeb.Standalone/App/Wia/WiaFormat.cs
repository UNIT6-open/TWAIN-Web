using System.Collections.Generic;

namespace TwainWeb.Standalone.App.Wia
{
	public enum WiaFormat
	{
		BMP,
		PNG,
		GIF,
		JPEG,
		TIFF
	}
	public static class WiaFormatDictionary
	{
		private static readonly Dictionary<WiaFormat, string> _dictionary = new Dictionary<WiaFormat, string>
		{
			{
				WiaFormat.BMP,
				"{B96B3CAB-0728-11D3-9D7B-0000F81EF32E}"
			},
			{
				WiaFormat.PNG,
				"{B96B3CAF-0728-11D3-9D7B-0000F81EF32E}"
			},
			{
				WiaFormat.GIF,
				"{B96B3CB0-0728-11D3-9D7B-0000F81EF32E}"
			},
			{
				WiaFormat.JPEG,
				"{B96B3CAE-0728-11D3-9D7B-0000F81EF32E}"
			},
			{
				WiaFormat.TIFF,
				"{B96B3CB1-0728-11D3-9D7B-0000F81EF32E}"
			}
		};

		public static string Get(WiaFormat key)
		{
			return _dictionary[key];
		}
	}
}
