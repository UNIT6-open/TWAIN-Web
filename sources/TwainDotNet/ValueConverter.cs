using System;
using System.Globalization;
using TwainDotNet.TwainNative;

namespace TwainDotNet
{
	/// <summary>
	/// Capabilities converter
	/// </summary>
	public class ValueConverter
	{
		/// <summary>
		/// Tries to convert a value to <see cref="Fix32"/> if possible.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static Fix32 ConvertToFix32(object value)
		{
			if (value == null) return default(Fix32);

			var fix32 = value as Fix32;
			if (fix32 != null)
			{
				return fix32;
			}
			return Convert.ToSingle(value, CultureInfo.InvariantCulture);
		}
	}
}
