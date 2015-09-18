using System;
using System.ComponentModel;

namespace TwainWeb.Standalone.App.Extensions
{
	public static class EnumExtensions
	{
		public static string GetDescription(Enum enumValue)
		{
			var fi = enumValue.GetType().GetField(enumValue.ToString());

			var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

			if (attributes.Length > 0)
				return attributes[0].Description;

			return enumValue.ToString();
		}
	}
}
