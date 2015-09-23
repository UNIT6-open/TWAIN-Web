using System.Collections.Generic;
using System.Drawing;
using TwainWeb.Standalone.App.Models.Response;
using TwainWeb.Standalone.App.Twain;

namespace TwainWeb.Standalone.App.Scanner
{
	public interface ISource
	{
		int Index { get; }
		string Name { get; }
		ScannerSettings GetScannerSettings();
		List<Image> Scan(SettingsAcquire settings);
	}
}
