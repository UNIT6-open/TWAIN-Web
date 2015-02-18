using System;
using System.Collections.Generic;
using TwainWeb.Standalone.Scanner;
using WIA;


namespace TwainWeb.Standalone.Wia
{
	public class WiaScannerManager:IScannerManager
	{
		private List<WiaSource> _sources;
		private WiaSource _currentSource;

		public WiaScannerManager()
		{
			_sources = new List<WiaSource>();
		}

		public void ChangeSource(int index)
		{
			WiaSource source;
			if (_sources.Count == 0)
				RefreshSources();

			try
			{
				source = _sources[index];
			}
			catch (Exception)
			{
				throw new Exception(string.Format("Source with index {0} not found", index));
			}

			_currentSource = source;

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
				wiaSource = _sources[index];
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
			var devices = new List<WiaSource>();
			var manager = new DeviceManager();

			var i = 0;
			foreach (DeviceInfo info in manager.DeviceInfos)
			{
				devices.Add(new WiaSource(info, i));
				i++;
			}
			_sources = devices;
		}

	}
}
