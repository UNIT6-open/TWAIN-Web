namespace TwainDotNet.TwainNative
{
	/// <summary>
	/// The data types of item in TWAIN, used in the
	/// capability containers.
	/// Corresponds to TWTY_* values.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags")]
	public enum ItemType : ushort
	{
		/// <summary>
		/// Means Item is a an 8 bit value.
		/// </summary>
		Int8 = 0,
		/// <summary>
		/// Means Item is a 16 bit value.
		/// </summary>
		Int16 = 1,
		/// <summary>
		/// Means Item is a 32 bit value.
		/// </summary>
		Int32 = 2,
		/// <summary>
		/// Means Item is an unsigned 8 bit value.
		/// </summary>
		UInt8 = 3,
		/// <summary>
		/// Means Item is an unsigned 16 bit value.
		/// </summary>
		UInt16 = 4,
		/// <summary>
		/// Means Item is an unsigned 32 bit value.
		/// </summary>
		UInt32 = 5,
		/// <summary>
		/// Means Item is an unsigned 16 bit value (supposedly, YMMV).
		/// </summary>
		Bool = 6,
		/// <summary>
		/// Means Item is a <see cref="Fix32"/>.
		/// </summary>
		Fix32 = 7,
		/// <summary>
		/// Means Item is a <see cref="Frame"/>.
		/// </summary>
		Frame = 8,
		/// <summary>
		/// Means Item is a 32 char string (max).
		/// </summary>
		String32 = 9,
		/// <summary>
		/// Means Item is a 64 char string (max).
		/// </summary>
		String64 = 0xa,
		/// <summary>
		/// Means Item is a 128 char string (max).
		/// </summary>
		String128 = 0xb,
		/// <summary>
		/// Means Item is a char string shorter than 255.
		/// </summary>
		String255 = 0xc,
		//String1024 = 0xd,
		//Unicode512 = 0xe,
		/// <summary>
		/// Means Item is a handle (pointer).
		/// </summary>
		Handle = 0xf
	}
}
