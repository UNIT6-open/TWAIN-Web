using System;
using System.Text;
using log4net;
using TwainWeb.Standalone.Scanner;

namespace TwainWeb.Standalone.App
{
	public class AjaxMethods
	{
		public AjaxMethods(object markerAsync)
		{
			_markerAsync = markerAsync;
			_logger = LogManager.GetLogger(typeof(HttpServer));
		}

		private readonly ILog _logger;
		private const int WaitTime = 15000;
		private readonly object _markerAsync;

		private ScannerSettings ChangeSource(IScannerManager scannerManager, int sourceIndex, CashSettings cashSettings)
		{
			var searchSetting = cashSettings.Search(scannerManager, sourceIndex);
			if (searchSetting == null)
			{
				new AsyncWorker<int>().RunWorkAsync(sourceIndex, "ChangeSource", scannerManager.ChangeSource, WaitTime);

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
				//если выбранный источник существует, выбираем его; если нет - выбираем первый
				if (sourceIndex.HasValue && sourceIndex.Value > scannerManager.SourceCount - 1)
				{
					sourceIndex = scannerManager.SourceCount > 0 ? 0 : (int?)null;					
				}

				ScannerSettings searchSetting = null;

				if (cashSettings.NeedUpdateNow(DateTime.UtcNow))
				{
					var currentSourceIndex = scannerManager.CurrentSourceIndex;

					cashSettings.Update(scannerManager);

					if (currentSourceIndex.HasValue)
					{
						try
						{
							searchSetting = ChangeSource(scannerManager, currentSourceIndex.Value, cashSettings);
						}
						catch (Exception)
						{
							_logger.Error("Changing source failed");
						}
					}
				}

				var jsonResult = "{";
				if (scannerManager.SourceCount > 0)
				{
					var needOfChangeSource = sourceIndex.HasValue && sourceIndex != scannerManager.CurrentSourceIndex;
					if (needOfChangeSource)
						try
						{
							searchSetting = ChangeSource(scannerManager, sourceIndex.Value, cashSettings);
						}
						catch (Exception)
						{
							_logger.Error("Changing source failed");
						}

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
							try
							{
								searchSetting = ChangeSource(scannerManager, sortedSource.Index, cashSettings);
							}
							catch (Exception)
							{
								_logger.Error("Changing source failed");
							}
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
	}
}