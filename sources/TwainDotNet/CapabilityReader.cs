using TwainDotNet.TwainNative;
using TwainDotNet.Win32;

namespace TwainDotNet
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;

	namespace NTwain
	{
		/// <summary>
		/// The one-stop class for reading raw TWAIN cap values from the cap container.
		/// This contains all the properties for the 4 container types.
		/// </summary>
		public class CapabilityReader
		{

			/// <summary>
			/// Reads the value from a <see cref="TwainCapability" /> that was returned
			/// from a TWAIN source.
			/// </summary>
			/// <param name="capability">The capability.</param>
			/// <returns></returns>
			/// <exception cref="System.ArgumentNullException">
			/// capability
			/// or
			/// memoryManager
			/// </exception>
			/// <exception cref="System.ArgumentException">capability</exception>
			public static CapabilityReader ReadValue(TwainCapability capability)
			{
				if (capability == null) { throw new ArgumentNullException("capability"); }
				
				if (capability.Handle != IntPtr.Zero)
				{
					IntPtr baseAddr = IntPtr.Zero;
					try
					{
						baseAddr = Kernel32Native.GlobalLock(capability.Handle); 
						switch (capability.ContainerType)
						{
							case ContainerType.Array:
								return new CapabilityReader
								{
									ContainerType = capability.ContainerType,
								}.ReadArrayValue(baseAddr);
							case ContainerType.Enum:
								return new CapabilityReader
								{
									ContainerType = capability.ContainerType,
								}.ReadEnumValue(baseAddr);
							case ContainerType.One:
								return new CapabilityReader
								{
									ContainerType = capability.ContainerType,
								}.ReadOneValue(baseAddr);
							case ContainerType.Range:
								return new CapabilityReader
								{
									ContainerType = capability.ContainerType,
								}.ReadRangeValue(baseAddr);
							default:
								throw new ArgumentException("Capability has bad format");
						}
					}
					finally
					{
						if (baseAddr != IntPtr.Zero)
						{
							//memoryManager.Unlock(baseAddr);
							Kernel32Native.GlobalUnlock(capability.Handle);
						
						}
					}
				}

				return new CapabilityReader();
				
			}

			#region common prop

			/// <summary>
			/// Gets the underlying container type. 
			/// </summary>
			/// <value>
			/// The container.
			/// </value>
			public ContainerType ContainerType { get; private set; }

			/// <summary>
			/// Gets the type of the TWAIN value.
			/// </summary>
			/// <value>
			/// The type of the value.
			/// </value>
			public TwainType ItemType { get; private set; }

			/// <summary>
			/// Gets the one value if container is <see cref="TwainNative.ContainerType.Array"/>.
			/// </summary>
			/// <value>
			/// The one value.
			/// </value>
			public object OneValue { get; private set; }

			/// <summary>
			/// Gets the collection values if container is <see cref="TwainNative.ContainerType.Enum"/> or <see cref="TwainNative.ContainerType.Range"/> .
			/// </summary>
			/// <value>
			/// The collection values.
			/// </value>
			public IList<object> CollectionValues { get; private set; }

			#endregion

			#region enum prop

			/// <summary>
			/// Gets the current value index if container is <see cref="TwainNative.ContainerType.Enum"/>.
			/// </summary>
			public int EnumCurrentIndex { get; private set; }
			/// <summary>
			/// Gets the default value index if container is <see cref="TwainNative.ContainerType.Enum" />.
			/// </summary>
			public int EnumDefaultIndex { get; private set; }

			#endregion

			#region range prop

			/// <summary>
			/// Gets the current value if container is <see cref="TwainNative.ContainerType.Range" />.
			/// </summary>
			/// <value>
			/// The range current value.
			/// </value>
			public object RangeCurrentValue { get; private set; }
			/// <summary>
			/// Gets the default value if container is <see cref="TwainNative.ContainerType.Range" />.
			/// </summary>
			/// <value>
			/// The range default value.
			/// </value>
			public object RangeDefaultValue { get; private set; }
			/// <summary>
			/// The least positive/most negative value of the range.
			/// </summary>
			/// <value>
			/// The range minimum value.
			/// </value>
			public object RangeMinValue { get; private set; }
			/// <summary>
			/// The most positive/least negative value of the range.
			/// </summary>
			/// <value>
			/// The range maximum value.
			/// </value>
			public object RangeMaxValue { get; private set; }
			/// <summary>
			/// The delta between two adjacent values of the range.
			/// e.g. Item2 - Item1 = StepSize;
			/// </summary>
			/// <value>
			/// The size of the range step.
			/// </value>
			public object RangeStepSize { get; private set; }

			#endregion

			#region reader methods

			/// <summary>
			/// Don't care what contain it is, just populates the specified list with the capability values (count be one or many).
			/// </summary>
			/// <param name="toPopulate">The list to populate the values.</param>
			/// <returns></returns>
			public IList<object> PopulateFromCapValues(IList<object> toPopulate)
			{
				if (toPopulate == null) { toPopulate = new List<object>(); }

				switch (ContainerType)
				{
					case ContainerType.One:
						if (OneValue != null)
						{
							toPopulate.Add(OneValue);
						}
						break;
					case ContainerType.Array:
					case ContainerType.Enum:
						if (CollectionValues != null)
						{
							foreach (var o in CollectionValues)
							{
								toPopulate.Add(o);
							}
						}
						break;
					case ContainerType.Range:
						PopulateRange(toPopulate);
						break;
				}
				return toPopulate;
			}

			private void PopulateRange(IList<object> toPopulate)
			{
				// horrible cast but should work.
				// in the for loop we also compare against min in case the step
				// is parsed as negative number and causes infinite loop.
				switch (ItemType)
				{
					case TwainType.Fix32:
						{
							var min = ((Fix32)RangeMinValue).ToFloat();
							var max = ((Fix32)RangeMaxValue).ToFloat();
							var step = ((Fix32)RangeStepSize).ToFloat();

							for (var i = min; i >= min && i <= max; i += step)
							{
								toPopulate.Add(i);
							}
						}
						break;
					case TwainType.UInt32:
						{
							var min = (uint)RangeMinValue;
							var max = (uint)RangeMaxValue;
							var step = (uint)RangeStepSize;

							for (var i = min; i >= min && i <= max; i += step)
							{
								toPopulate.Add(i);
							}
						}
						break;
					case TwainType.Int32:
						{
							var min = (int)RangeMinValue;
							var max = (int)RangeMaxValue;
							var step = (int)RangeStepSize;

							for (var i = min; i >= min && i <= max; i += step)
							{
								toPopulate.Add(i);
							}
						}
						break;
					// these should never happen since TW_ENUM fields are 4 bytes but you never know
					case TwainType.UInt16:
						{
							var min = (ushort)RangeMinValue;
							var max = (ushort)RangeMaxValue;
							var step = (ushort)RangeStepSize;

							for (var i = min; i >= min && i <= max; i += step)
							{
								toPopulate.Add(i);
							}
						}
						break;
					case TwainType.Int16:
						{
							var min = (short)RangeMinValue;
							var max = (short)RangeMaxValue;
							var step = (short)RangeStepSize;

							for (var i = min; i >= min && i <= max; i += step)
							{
								toPopulate.Add(i);
							}
						}
						break;
					case TwainType.UInt8:
						{
							var min = (byte)RangeMinValue;
							var max = (byte)RangeMaxValue;
							var step = (byte)RangeStepSize;

							for (var i = min; i >= min && i <= max; i += step)
							{
								toPopulate.Add(i);
							}
						}
						break;
					case TwainType.Int8:
						{
							var min = (sbyte)RangeMinValue;
							var max = (sbyte)RangeMaxValue;
							var step = (sbyte)RangeStepSize;

							for (var i = min; i >= min && i <= max; i += step)
							{
								toPopulate.Add(i);
							}
						}
						break;
				}
			}


			CapabilityReader ReadOneValue(IntPtr baseAddr)
			{
				int offset = 0;
				ItemType = (TwainType)(ushort)Marshal.ReadInt16(baseAddr, offset);
				offset += 2;
				OneValue = ReadValue(baseAddr, ref offset, ItemType);
				return this;
			}

			CapabilityReader ReadArrayValue(IntPtr baseAddr)
			{
				int offset = 0;
				ItemType = (TwainType)(ushort)Marshal.ReadInt16(baseAddr, offset);
				offset += 2;
				var count = Marshal.ReadInt32(baseAddr, offset);
				offset += 4;
				if (count > 0)
				{
					CollectionValues = new object[count];
					for (int i = 0; i < count; i++)
					{
						CollectionValues[i] = ReadValue(baseAddr, ref offset, ItemType);
						
					}
				}
				return this;
			}

			CapabilityReader ReadEnumValue(IntPtr baseAddr)
			{
				int offset = 0;
				ItemType = (TwainType)(ushort)Marshal.ReadInt16(baseAddr, offset);
				offset += 2;
				int count = Marshal.ReadInt32(baseAddr, offset);
				offset += 4;
				EnumCurrentIndex = Marshal.ReadInt32(baseAddr, offset);
				offset += 4;
				EnumDefaultIndex = Marshal.ReadInt32(baseAddr, offset);
				offset += 4;
				if (count > 0)
				{
					CollectionValues = new object[count];
					for (int i = 0; i < count; i++)
					{
						CollectionValues[i] = ReadValue(baseAddr, ref offset, ItemType);
					}
				}
				return this;
			}

			CapabilityReader ReadRangeValue(IntPtr baseAddr)
			{
				int offset = 0;
				ItemType = (TwainType)(ushort)Marshal.ReadInt16(baseAddr, offset);
				offset += 2;

				RangeMinValue = ReadValue(baseAddr, ref offset, ItemType);
				RangeMaxValue = ReadValue(baseAddr, ref offset, ItemType);
				RangeStepSize = ReadValue(baseAddr, ref offset, ItemType);
				RangeDefaultValue = ReadValue(baseAddr, ref offset, ItemType);
				RangeCurrentValue = ReadValue(baseAddr, ref offset, ItemType);

/*				RangeMinValue = (uint)Marshal.ReadInt32(baseAddr, offset);
				offset += 4;
				RangeMaxValue = (uint)Marshal.ReadInt32(baseAddr, offset);
				offset += 4;
				RangeStepSize = (uint)Marshal.ReadInt32(baseAddr, offset);
				offset += 4;
				RangeDefaultValue = (uint)Marshal.ReadInt32(baseAddr, offset);
				offset += 4;
				RangeCurrentValue = (uint)Marshal.ReadInt32(baseAddr, offset);*/

				return this;
			}

			#endregion

			/// <summary>
			/// Reads a TWAIN value.
			/// </summary>
			/// <param name="baseAddress">The base address.</param>
			/// <param name="offset">The offset.</param>
			/// <param name="type">The TWAIN type.</param>
			/// <returns></returns>
			public static object ReadValue(IntPtr baseAddress, ref int offset, TwainType type)
			{
				object val = null;
				switch (type)
				{
					case TwainType.Int8:
						val = (sbyte)Marshal.ReadByte(baseAddress, offset);
						break;
					case TwainType.UInt8:
						val = Marshal.ReadByte(baseAddress, offset);
						break;
					case TwainType.Bool:
					case TwainType.UInt16:
						val = (ushort)Marshal.ReadInt16(baseAddress, offset);
						break;
					case TwainType.Int16:
						val = Marshal.ReadInt16(baseAddress, offset);
						break;
					case TwainType.UInt32:
						val = (uint)Marshal.ReadInt32(baseAddress, offset);
						break;
					case TwainType.Int32:
						val = Marshal.ReadInt32(baseAddress, offset);
						break;
					case TwainType.Fix32:
						Fix32 f32 = new Fix32();
						f32.Whole = Marshal.ReadInt16(baseAddress, offset);
						f32.Frac = (ushort)Marshal.ReadInt16(baseAddress, offset + 2);
						val = f32;
						break;
					case TwainType.Frame:
						Frame frame = new Frame();
						frame.Left = (Fix32)ReadValue(baseAddress, ref offset, TwainType.Fix32);
						frame.Top = (Fix32)ReadValue(baseAddress, ref offset, TwainType.Fix32);
						frame.Right = (Fix32)ReadValue(baseAddress, ref offset, TwainType.Fix32);
						frame.Bottom = (Fix32)ReadValue(baseAddress, ref offset, TwainType.Fix32);
						return frame; // no need to update offset again after reading fix32
					case TwainType.Str128:
					case TwainType.Str255:
					case TwainType.Str32:
					case TwainType.Str64:
						val = Marshal.PtrToStringAnsi(new IntPtr(baseAddress.ToInt64() + offset));
						break;
/*					case TwainType.Handle:
						val = Marshal.ReadIntPtr(baseAddress, offset);
						break;*/
				}
				offset += GetItemTypeSize(type);
				return val;
			}
			/// <summary>
			/// Gets the byte size of the item type.
			/// </summary>
			/// <param name="type"></param>
			/// <returns></returns>
			public static int GetItemTypeSize(TwainType type)
			{
				if (__sizes.ContainsKey(type))
				{
					return __sizes[type];
				}
				return 0;
			}

			static readonly IDictionary<TwainType, int> __sizes = new Dictionary<TwainType, int>
        {
            {TwainType.Int8, 1},
            {TwainType.UInt8, 1},
            {TwainType.Int16, 2},
            {TwainType.UInt16, 2},
            {TwainType.Bool, 2},
            {TwainType.Int32, 4},
            {TwainType.UInt32, 4},
            {TwainType.Fix32, 4},
            {TwainType.Frame, 16},
            {TwainType.Str32, 34},
            {TwainType.Str64, 66},
            {TwainType.Str128, 130},
            {TwainType.Str255, 256},
            // TODO: find out if it should be fixed 4 bytes or intptr size
          //  {TwainType.Handle, IntPtr.Size},
        };
		}


	}

}
