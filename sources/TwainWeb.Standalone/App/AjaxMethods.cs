using System;
using System.Text;
using TwainWeb.Standalone.Twain;

namespace TwainWeb.Standalone.App
{
	public class AjaxMethods
	{
		public AjaxMethods(object markerAsync)
		{
			_markerAsync = markerAsync;
		}

		private const int WaitTime = 30000;
		private readonly object _markerAsync;

/*		private ScannerSettings ChangeSource(Twain32 _twain, int sourceIndex, CashSettings cashSettings)
		{
			var searchSetting = cashSettings.Search(_twain, sourceIndex);
			if (searchSetting == null)
			{
				AsyncMethods.asyncWithWaitTime<int>(sourceIndex, "ChangeSource", _twain.ChangeSource, WaitTime, _twain.waitHandle);
				if (sourceIndex == _twain.SourceIndex && _twain.ChangeSourceResp == null)
					searchSetting = cashSettings.PushCurrentSource(_twain);
			}
			return searchSetting;
		}*/

		private ScannerSettings ChangeSource(IScannerManager scannerManager, int sourceIndex, CashSettings cashSettings)
		{
			var searchSetting = cashSettings.Search(scannerManager, sourceIndex);
			if (searchSetting == null)
			{
				scannerManager.ChangeSource(sourceIndex);
				//AsyncMethods.asyncWithWaitTime<int>(sourceIndex, "ChangeSource", _twain.ChangeSource, WaitTime, _twain.waitHandle);
				if (sourceIndex == scannerManager.CurrentSource.Index)
					searchSetting = cashSettings.PushCurrentSource(scannerManager);
			}
			return searchSetting;
		}

		public ActionResult Scan(ScanForm command, IScannerManager scannerManager)
		{
			var scanResult = new ScanCommand(command, scannerManager).Execute(_markerAsync);

			if (scanResult.Validate())
				return new ActionResult {Content = scanResult.FileContent, ContentType = "text/json"};

			throw new Exception(scanResult.Error);
		}

		public ActionResult GetScannerParameters(IScannerManager scannerManager, CashSettings cashSettings, int? sourceIndex)
		{
			var actionResult = new ActionResult {ContentType = "text/json"};
			lock (_markerAsync)
			{
				ScannerSettings searchSetting = null;

				if (cashSettings.NeedUpdateNow(DateTime.UtcNow))
				{
					var currentSourceIndex = scannerManager.CurrentSourceIndex;

					cashSettings.Update(scannerManager);
					if (currentSourceIndex.HasValue)
						searchSetting = ChangeSource(scannerManager, currentSourceIndex.Value, cashSettings);
				}

				var jsonResult = "{";
				if (scannerManager.SourceCount > 0)
				{
					var needOfChangeSource = sourceIndex.HasValue && sourceIndex != scannerManager.CurrentSourceIndex;
					if (needOfChangeSource)
						searchSetting = ChangeSource(scannerManager, sourceIndex.Value, cashSettings);

					else if (scannerManager.CurrentSourceIndex.HasValue)
					{
						searchSetting =
							cashSettings.Search(scannerManager, scannerManager.CurrentSourceIndex.Value) 
							?? cashSettings.PushCurrentSource(scannerManager);
					}

					var sources = scannerManager.GetSources();
					jsonResult += "\"sources\":{ \"selectedSource\": \"" + sourceIndex + "\", \"sourcesList\":[";

					var i = 0;
					foreach (var sortedSource in sources)
					{
						if (!needOfChangeSource && searchSetting == null)
						{
							searchSetting = ChangeSource(scannerManager, sortedSource.Index, cashSettings);
						}

						jsonResult += "{ \"key\": \"" + sortedSource.Index + "\", \"value\": \"" + sortedSource.Name + "\"}" +
						              (i != (sources.Count - 1) ? "," : "");
						i++;
					}

					jsonResult += "]}";

					jsonResult += searchSetting != null ? searchSetting.Serialize() : "";
				}
				jsonResult += "}";
				actionResult.Content = Encoding.UTF8.GetBytes(jsonResult);
			}


			return actionResult;
		}

/*		private Dictionary<int, string> GetSources(Twain32 twain)
		{
			var sourses = new Dictionary<int, string>();

			for (var i = 0; i < twain.SourcesCount; i++)
			{
				sourses.Add(i, twain.GetSourceProduct(i).Name);
			}
			return sourses;
		}

		private Dictionary<int, string> SortSources(Dictionary<int, string> initialDictionary)
		{
			var sourses = new Dictionary<int, string>();

			var soursesWithoutWia = new Dictionary<int, string>();

			foreach (var sourse in initialDictionary)
			{
				if (sourse.Value.ToLower().Contains("wia"))
				{
					sourses.Add(sourse.Key, sourse.Value);
				}
				else
				{
					soursesWithoutWia.Add(sourse.Key, sourse.Value);
				}
			}

			foreach (var otherSourse in soursesWithoutWia)
			{
				sourses.Add(otherSourse.Key, otherSourse.Value);
			}

			return sourses;
		}*/
	}
}