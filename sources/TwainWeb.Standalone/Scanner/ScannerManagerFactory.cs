using TwainWeb.Standalone.MessageLoop;
using TwainWeb.Standalone.Twain;
using TwainWeb.Standalone.TwainNet;
using TwainWeb.Standalone.Wia;

namespace TwainWeb.Standalone.Scanner
{
	/// <summary>
	/// Фабрика менеджера сканнеров
	/// </summary>
	public class ScannerManagerFactory
	{
		/// <summary>
		/// В зависимости от настроек в App.config создает экземпляр менеджера сканнеров
		/// </summary>
		public IScannerManager GetScannerManager(WindowsMessageLoopThread messageLoop)
		{
			switch (Settings.Default.ScannerManager.ToLower())
			{
				case "wia":
					return new WiaScannerManager();
				case "twain":
					return new TwainScannerManager();
				case "twaindotnet":
					return new TwainDotNetScannerManager(messageLoop);
				default:
					return new WiaScannerManager();
			}
		}
	}
}
