using System;
using System.Collections.Generic;
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

		public static Dictionary<int, string> GetDictionaryWithEnumValues(Type type)
		{
			var values = Enum.GetValues(type);
			var result = new Dictionary<int, string>();

			foreach (var value in values)
			{
				var description = GetDescription((Enum)value);
				result.Add((int)value, description);
			}

			return result;
		} 
	}
}
