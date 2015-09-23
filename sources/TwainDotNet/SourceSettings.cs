using System.Collections.Generic;

namespace TwainDotNet
{
	public class SourceSettings
	{
		private readonly List<float> _resolutions;
		private readonly float? _physicalHeight;
		private readonly float? _physicalWidth;
		private readonly List<ushort> _pixelTypes;
		private readonly bool _hasADF;
		private readonly bool _hasFlatbed;
		private readonly bool _hasDuplex;

		public List<float> Resolutions {get { return _resolutions; }}
		public float? PhysicalHeight { get { return _physicalHeight; } }
		public float? PhysicalWidth { get { return _physicalWidth; } }

		public List<ushort> PixelTypes { get { return _pixelTypes; } }

		public bool HasADF { get { return _hasADF; } }
		public bool HasFlatbed { get { return _hasFlatbed; } }
		public bool HasDuplex { get { return _hasFlatbed; } }

		public SourceSettings(List<float> resolutions, 
			List<ushort> pixelTypes, 
			float? physicalHeight, float? physicalWidth, 
			bool hasADF, bool hasFlatbed, bool hasDuplex)
		{
			_pixelTypes = pixelTypes;
			_resolutions = resolutions;
			_physicalHeight = physicalHeight;
			_physicalWidth = physicalWidth;
			_hasADF = hasADF;
			_hasFlatbed = hasFlatbed;
			_hasDuplex = hasDuplex;
		}
	}
}
