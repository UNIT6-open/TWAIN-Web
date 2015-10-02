using System;
using System.Collections.Generic;
using log4net;
using TwainWeb.Standalone.App.Scanner;
using WIA;

namespace TwainWeb.Standalone.App.Wia
{
	public class WiaScannerManager:IScannerManager
	{
		private List<WiaSource> _sources;
		private WiaSource _currentSource;
		private ILog _log;

		public WiaScannerManager()
		{
			_sources = new List<WiaSource>();
			_log = LogManager.GetLogger(typeof(WiaScannerManager));
			_log.Info("Wia scanner manager is used");
		}

		public void ChangeSource(int index)
		{
			_log.Info("Wia: change source");
			WiaSource source;
			if (_sources.Count == 0)
				RefreshSources();

			try
			{
				source = _sources.Find(s=>s.Index == index);
			}
			catch (Exception)
			{
				throw new Exception(string.Format("Source with index {0} not found", index));
			}

			_currentSource = source;
			_log.Info("Wia: change source success");
		}

		public int? CurrentSourceIndex { get { return _currentSource == null ? (int?)null : _currentSource.Index; } }

		public ISource CurrentSource { get { return _currentSource; } }

		public int SourceCount
		{
			get
			{
				if (_sources.Count == 0)
					RefreshSources();

				return _sources == null ? 0 : _sources.Count;
			}
		}

		public ISource GetSource(int index)
		{
			ISource wiaSource;
			try
			{
				wiaSource = _sources.Find(x=>x.Index == index);
			}
			catch (Exception)
			{
				throw new Exception(string.Format("Source with index {0} not found", index));
			}

			return wiaSource;
		}

		public List<ISource> GetSources()
		{
			if (_sources.Count == 0)
			{
				RefreshSources();
			}
			return _sources == null ? null : _sources.ConvertAll(s=>(ISource)s);
		}
		
		private void RefreshSources()
		{
			_sources.Clear();

			var devices = new List<WiaSource>();
			var manager = new DeviceManager();

			var i = 0;
			foreach (DeviceInfo info in manager.DeviceInfos)
			{
				if (info.Type == WiaDeviceType.ScannerDeviceType)
				{
					try
					{
						var name = FindProperty(info.Properties, WiaProperty.Name);
						devices.Add(new WiaSource(i, (string)name.get_Value(), info.DeviceID));
						i++;
					}
					catch (Exception e)
					{
						_log.WarnFormat("Ошибка при добавлении источника: {0}", e);
					}			
				}
		
			}
			_sources = devices;
		}

		private static Property FindProperty(WIA.Properties properties, WiaProperty property)
		{
			foreach (Property prop in properties)
			{
				if (prop.PropertyID == (int)property)
				{
					return prop;
				}
			}
			return null;
		}

	}
}
