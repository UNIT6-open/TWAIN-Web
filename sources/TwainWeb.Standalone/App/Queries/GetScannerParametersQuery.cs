using System;
using System.Collections.Generic;
using System.Threading;
using log4net;
using TwainWeb.Standalone.App.Cache;
using TwainWeb.Standalone.App.Models.Response;
using TwainWeb.Standalone.App.Scanner;

namespace TwainWeb.Standalone.App.Queries
{
	public class GetScannerParametersQuery
	{
		private readonly IScannerManager _scannerManager;
		private readonly CacheSettings _cacheSettings;
		private int? _sourceIndex;
		private readonly ILog _logger;
		private const int ChangeSourceWaitTime = 15000;
		private const int PushSettingsWaitTime = 5000;
		public GetScannerParametersQuery(IScannerManager scannerManager, CacheSettings cacheSettings, int? sourceIndex)
		{
			if (scannerManager == null) throw new Exception("Невозможно получить параметры сканирования, т.к. менеджер источников данных не был инициализирован");

			_scannerManager = scannerManager;
			_cacheSettings = cacheSettings;
			_sourceIndex = sourceIndex;

			_logger = LogManager.GetLogger(typeof(GetScannerParametersQuery));
		}
		public ScannerParametersQueryResult Execute(object markerAsync)
		{
			_logger.Info("======================================= GET PARAMS QUERY ========================================");
			_logger.Info("Scanner index: " + _sourceIndex);
			ScannerSettings searchSetting = null;
			List<ISource> sources = null;

			if (Monitor.TryEnter(markerAsync))
			{
				_logger.Debug("Enter to monitor");
				try
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


						if (_cacheSettings.NeedUpdateNow(DateTime.UtcNow))
						{
							_logger.Debug("Update cache");
							_cacheSettings.Update(_scannerManager);
						}

						try
						{
							searchSetting = GetScannerSettings(sourceIndex);
						}
						catch (Exception e)
						{
							_logger.Error("Can't obtain scanner settings: " + e);
						}

						sources = _scannerManager.GetSources();
					}
				}
				catch (Exception e)
				{
					return new ScannerParametersQueryResult(string.Format("Ошибка при получении информации об источниках: {0}", e));
				}
				finally
				{
					Monitor.Exit(markerAsync);
				}
			}
			else
			{
				return new ScannerParametersQueryResult(string.Format("Не удалось получить информацию об источниках: сканер занят"));
			}

			_logger.Info("Scan settings: " + (searchSetting == null? "": searchSetting.Serialize()));
			return new ScannerParametersQueryResult(sources, searchSetting, _sourceIndex);
		}

		private ScannerSettings GetScannerSettings(int sourceIndex)
		{
			_logger.Debug("Searching scanner settings in cache...");
			var searchSetting = _cacheSettings.Search(_scannerManager, sourceIndex);

			if (searchSetting != null)
			{
				_logger.Debug("Scanner settings was found");
				return searchSetting;
			}
			else
			{
				_logger.Debug("Scanner settings was not found");
			}

			var needOfChangeSource = _sourceIndex.HasValue && _sourceIndex != _scannerManager.CurrentSourceIndex;

			if (needOfChangeSource)
				new AsyncWorker<int>().RunWorkAsync(sourceIndex, _scannerManager.ChangeSource, ChangeSourceWaitTime);

			if (_scannerManager.CurrentSource == null)
				throw new Exception("Не удалось выбрать источник");

			if (sourceIndex == _scannerManager.CurrentSource.Index)
				searchSetting = new AsyncWorker<IScannerManager, ScannerSettings>()
					.RunWorkAsync(_scannerManager, _cacheSettings.PushCurrentSource, PushSettingsWaitTime);

			return searchSetting;
		}
	}
}			
