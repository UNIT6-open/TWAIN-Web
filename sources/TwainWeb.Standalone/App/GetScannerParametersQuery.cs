using System;
using System.Collections.Generic;
using System.Text;
using log4net;
using TwainWeb.Standalone.Scanner;

namespace TwainWeb.Standalone.App
{
	public class GetScannerParametersQuery
	{
		private readonly IScannerManager _scannerManager;
		private readonly CashSettings _cashSettings;
		private int? _sourceIndex;
		private readonly ILog _logger;
		private const int WaitTime = 15000;
		public GetScannerParametersQuery(IScannerManager scannerManager, CashSettings cashSettings, int? sourceIndex)
		{
			_scannerManager = scannerManager;
			_cashSettings = cashSettings;
			_sourceIndex = sourceIndex;

			_logger = LogManager.GetLogger(typeof(HttpServer));
		}
		public ActionResult Execute(object markerAsync)
		{
			var actionResult = new ActionResult { ContentType = "text/json" };

			ScannerSettings searchSetting = null;
			List<ISource> sources = null;

			lock (markerAsync)
			{
				var sourcesCount = _scannerManager.SourceCount;

				if (sourcesCount > 0)
				{
					//если выбранный источник существует, выбираем его; если нет - выбираем первый
					int sourceIndex;
					if (!_sourceIndex.HasValue || (_sourceIndex.Value > sourcesCount - 1))
						sourceIndex = 0;
					else
						sourceIndex = _sourceIndex.Value;
					

					if (_cashSettings.NeedUpdateNow(DateTime.UtcNow))
					{
						_cashSettings.Update(_scannerManager);
					}

					try
					{
						searchSetting = GetScannerSettings(_scannerManager, sourceIndex, _cashSettings);
					}
					catch (Exception)
					{
						_logger.Error("Can't obtain scanner settings");
					}

					sources = _scannerManager.GetSources();					
				}
			}

			var jsonResult = CreateJsonResult(sources, searchSetting, _sourceIndex);
			actionResult.Content = Encoding.UTF8.GetBytes(jsonResult);

			return actionResult;
		}

		private string CreateJsonResult(List<ISource> sources, ScannerSettings settings, int? sourceIndex)
		{
			var sb = new StringBuilder();
			sb.Append("{");

			if (sources != null && sources.Count != 0 && sourceIndex.HasValue)
			{
				sb.Append("\"sources\":{ \"selectedSource\": \"" + sourceIndex.Value + "\", \"sourcesList\":[");

				var i = 0;
				foreach (var sortedSource in sources)
				{
					sb.AppendFormat(
						"{{ \"key\": \"{0}\", \"value\": \"{1}\"}}",
						sortedSource.Index,
						sortedSource.Name);

					if (i != (sources.Count - 1))
						sb.Append(",");
						
					i++;
				}

				sb.Append("]}");

				if (settings != null)
					sb.Append(settings.Serialize());
				
			}

			sb.Append("}");

			return sb.ToString();
		}

		private ScannerSettings GetScannerSettings(IScannerManager scannerManager, int sourceIndex, CashSettings cashSettings)
		{
			var searchSetting = cashSettings.Search(scannerManager, sourceIndex);
			if (searchSetting != null) return searchSetting;

			var needOfChangeSource = _sourceIndex.HasValue && _sourceIndex != _scannerManager.CurrentSourceIndex;

			if (needOfChangeSource)
				new AsyncWorker<int>().RunWorkAsync(sourceIndex, "ChangeSource", scannerManager.ChangeSource, WaitTime);

			if (sourceIndex == scannerManager.CurrentSource.Index)
				searchSetting = cashSettings.PushCurrentSource(scannerManager);

			return searchSetting;
		}
	}
}			
