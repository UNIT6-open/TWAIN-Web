using System.ComponentModel;

namespace TwainWeb.Standalone.App.Models
{
	public enum ScanFeed
	{
		[Description("Планшетный")]
		Flatbad = 0,

		[Description("Автоподатчик (односторонее сканирование)")]
		Feeder = 1,

		[Description("Автоподатчик (двухстороннее сканирование)")]
		Duplex = 2,
	}
}
