using System.Collections.Generic;
using System.Drawing;
using TwainWeb.Standalone.App;
using TwainWeb.Standalone.Twain;

namespace TwainWeb.Standalone.Scanner
{
	public interface ISource
	{
		int Index { get; }
		string Name { get; }
		ScannerSettings GetScannerSettings();
		List<Image> Scan(SettingsAcquire settings);
	}
}
