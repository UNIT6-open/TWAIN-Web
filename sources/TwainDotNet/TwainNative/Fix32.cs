using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace TwainDotNet.TwainNative
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
	public class Fix32 : IEquatable<Fix32>
    {
        public short Whole;

        public ushort Frac;

	    public Fix32()
	    {
	    }

		/// <summary>
		/// Implicit float to Fix32 conversion operator
		/// </summary>
		/// <param name="f"></param>
		/// <returns></returns>
		public static implicit operator Fix32(float f)  // explicit byte to digit conversion operator
		{
			var value = new Fix32(f);
			return value;
		}

		/// <summary>
		/// Implicit Fix32 to float conversion operator
		/// </summary>
		/// <param name="f"></param>
		/// <returns></returns>
		public static implicit operator float(Fix32 f)  // implicit digit to byte conversion operator
		{
			return f.ToFloat();  // implicit conversion
		}

	    public Fix32(float f)
        {
            // http://www.dosadi.com/forums/archive/index.php?t-2534.html
            var val = (int)(f * 65536.0F);
            this.Whole = Convert.ToInt16(val >> 16);    // most significant 16 bits
            this.Frac = Convert.ToUInt16(val & 0xFFFF); // least
        }        

        public float ToFloat()
        {
            var frac = Convert.ToSingle(this.Frac);
            return this.Whole + frac / 65536.0F;
        }
		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
		/// <returns>
		/// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (!(obj is Fix32))
				return false;

			return Equals((Fix32)obj);
		}
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns></returns>
		public bool Equals(Fix32 other)
		{
			return Whole == other.Whole && Frac == other.Frac;
		}
		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return Whole ^ Frac;
		}

		public override string ToString()
		{
			return ToFloat().ToString(CultureInfo.InvariantCulture);
		}
    }
}