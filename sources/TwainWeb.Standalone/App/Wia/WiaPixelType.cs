using System.ComponentModel;

namespace TwainWeb.Standalone.App.Wia
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
}
