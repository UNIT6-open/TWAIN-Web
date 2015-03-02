using System.Runtime.InteropServices;

namespace TwainDotNet.TwainNative
{

		/// <summary>
		/// /* TWON_ENUMERATION. Container for a collection of values. */
		/// typedef struct {
		///    TW_UINT16  ItemType;
		///    TW_UINT32  NumItems;     /* How many items in ItemList                 */
		///    TW_UINT32  CurrentIndex; /* Current value is in ItemList[CurrentIndex] */
		///    TW_UINT32  DefaultIndex; /* Powerup value is in ItemList[DefaultIndex] */
		///    TW_UINT8   ItemList[1];  /* Array of ItemType values starts here       */
		/// } TW_ENUMERATION, FAR * pTW_ENUMERATION;
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 2)]
		public class CapabilityRangeValue
		{
			public TwainType TwainType { get; set; }

			public int MinValue { get; set; }
			public int MaxValue { get; set; }
			public int StepSize { get; set; }
			public int DefaultValue; /* Power-up value.                        */
			public int CurrentValue; /* The value that is currently in effect. */

#pragma warning disable 169
			/// <summary>
			/// The start of the array values
			/// </summary>
			byte _valueStart;
#pragma warning restore 169
		}
	}

