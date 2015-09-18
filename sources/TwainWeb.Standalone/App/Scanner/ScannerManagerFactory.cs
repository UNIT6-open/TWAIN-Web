using System;
using TwainWeb.Standalone.App.Twain;
using TwainWeb.Standalone.App.TwainNet;
using TwainWeb.Standalone.App.Wia;
using TwainWeb.Standalone.Host;

namespace TwainWeb.Standalone.App.Scanner
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

			var scannerManagerSetting = Settings.Default.ScannerManager;
			ScannerManager scannerManager;

			try
			{
				scannerManager = (ScannerManager)Enum.Parse(typeof(ScannerManager), scannerManagerSetting, true);
			}
			catch (Exception)
			{
				scannerManager = ScannerManager.Wia;
			}

			switch (scannerManager)
			{
				case ScannerManager.Wia:
					return new WiaScannerManager();
				case ScannerManager.Twain:
					return new TwainScannerManager();
				case ScannerManager.TwainDotNet:
					return new TwainDotNetScannerManager(messageLoop);
				default:
					return new WiaScannerManager();
			}
		}
	}
}
