using System.ComponentModel;

namespace TwainWeb.Standalone.Wia
{
	public enum WiaPixelType
	{
		[Description("Цветное")]
		Color = 1,
		[Description("Оттенки серого")]
		Greyscale = 2,
		[Description("Черно-белое")]
		BlackWhite = 4
	}
	public static class PixelTypeEnumExtensions
	{
		public static string GetDescription(WiaPixelType enumValue)
		{
			var fi = enumValue.GetType().GetField(enumValue.ToString());

			var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

			if (attributes.Length > 0)
				return attributes[0].Description;

			return enumValue.ToString();
		}
	}
}
