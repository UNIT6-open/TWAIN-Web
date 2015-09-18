using System;
using System.Collections.Generic;
using System.ComponentModel;
using TwainWeb.Standalone.App.Extensions;

namespace TwainWeb.Standalone.App.TwainNet
{
	public static class TwainPixelTypeExtensions
	{
		public static Dictionary<int, string> GetSelectListDictionary(List<ushort> pixelTypes)
		{
			var resultDictionary = new Dictionary<int, string>();

			foreach (var pixelType in pixelTypes)
			{
				TwainPixelType supportedPixelType;
				string description;
				try
				{
					supportedPixelType = (TwainPixelType) pixelType;
					description = EnumExtensions.GetDescription(supportedPixelType);
				}
				catch (Exception)
				{
					continue;
				}
				
				resultDictionary.Add((int)supportedPixelType, description);
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
			RGB = 2,
			[Description("Палитра")]
			Palette = 3
		}
	}
}
