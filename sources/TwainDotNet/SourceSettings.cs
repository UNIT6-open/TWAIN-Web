using System.Collections.Generic;

namespace TwainDotNet
{
	public class SourceSettings
	{
		private readonly List<float> _resolutions;
		private readonly float? _physicalHeight;
		private readonly float? _physicalWidth;
		private readonly List<ushort> _pixelTypes; 

		public List<float> Resolutions {get { return _resolutions; }}
		public float? PhysicalHeight { get { return _physicalHeight; } }
		public float? PhysicalWidth { get { return _physicalWidth; } }

		public List<ushort> PixelTypes { get { return _pixelTypes; } }

		public SourceSettings(List<float> resolutions, List<ushort> pixelTypes, float? physicalHeight, float? physicalWidth)
		{
			_pixelTypes = pixelTypes;
			_resolutions = resolutions;
			_physicalHeight = physicalHeight;
			_physicalWidth = physicalWidth;
		}
	}
}
