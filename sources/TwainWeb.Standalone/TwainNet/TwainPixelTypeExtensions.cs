using System;
using System.Collections.Generic;
using System.ComponentModel;
using TwainWeb.Standalone.App;

namespace TwainWeb.Standalone.TwainNet
{
	public static class TwainPixelTypeExtensions
	{
		public static Dictionary<int, string> GetSelectListDictionary(List<ushort> pixelTypes)
		{
			var resultDictionary = new Dictionary<int, string>();

			foreach (var pixelType in pixelTypes)
			{
				TwainPixelType supportedPixelType;
				try
				{
					supportedPixelType = (TwainPixelType) pixelType;
				}
				catch (Exception)
				{
					continue;
				}
				
				resultDictionary.Add((int)supportedPixelType, EnumExtensions.GetDescription(supportedPixelType));
			}

			return resultDictionary;
		}

		private enum TwainPixelType
		{
			[Description("Черно-белое")]
			BW = 0,
			[Description("Оттенки серого")]
			Grey = 1,
			[Description("Цветное")]
			RGB = 2
		}
	}
}
