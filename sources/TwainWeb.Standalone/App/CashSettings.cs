using System;
using System.Collections.Generic;
using TwainWeb.Standalone.Scanner;

namespace TwainWeb.Standalone.App
{
	public class CashSettings
	{
		private readonly List<ScannerSettings> _scannersSettings = new List<ScannerSettings>();
		private DateTime _lastUpdateTime = DateTime.UtcNow;

		public bool NeedUpdateNow(DateTime dateUtcNow)
		{
			return (dateUtcNow - _lastUpdateTime).Minutes >= 1;
		}

		public ScannerSettings Search(IScannerManager scannerManager, int searchIndex)
		{
			string sourceName;
			int? sourceId;
			try
			{
				var sourceProduct = scannerManager.GetSource(searchIndex);
				sourceName = sourceProduct.Name;
				sourceId = sourceProduct.Index;
			}
			catch (Exception)
			{
				return null;
			}

			foreach (var setting in _scannersSettings)
			{
				if (setting.Compare(sourceId.Value, sourceName))
					return setting;
			}
			return null;
		}

		public void Update(IScannerManager scannerManager)
		{
			if (scannerManager.SourceCount > 0)
			{
				var settingsForDelete = new List<ScannerSettings>();
				
				foreach (var setting in _scannersSettings)
				{
					var activeSource = false;
					for (var i = 0; i < scannerManager.SourceCount; i++)
					{
						var sourceProduct = scannerManager.GetSource(i);
						if (setting.Compare(sourceProduct.Index, sourceProduct.Name))
						{
							activeSource = true;
							break;
						}
					}
					if (!activeSource)
						settingsForDelete.Add(setting);
				}
				foreach (var setting in settingsForDelete)
				{
					_scannersSettings.Remove(setting);
				}
			}
			else
				_scannersSettings.RemoveAll(x => true);

			_lastUpdateTime = DateTime.UtcNow;
		}

		public ScannerSettings PushCurrentSource(IScannerManager scannerManager)
		{
			var settings = scannerManager.CurrentSource.GetScannerSettings();
			_scannersSettings.Add(settings);
			return settings;
		}
	}
}