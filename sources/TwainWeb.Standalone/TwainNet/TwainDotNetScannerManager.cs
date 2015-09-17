using System;
using System.Collections.Generic;
using log4net;
using TwainDotNet.WinFroms;
using TwainWeb.Standalone.MessageLoop;
using TwainWeb.Standalone.Scanner;

namespace TwainWeb.Standalone.TwainNet
{
	public class TwainDotNetScannerManager:IScannerManager
	{
		private readonly WindowsMessageLoopThread _windowsMessageLoop;
		private readonly TwainDotNet.Twain _twain;
		private List<TwainDotNetSource> _sources;
		private TwainDotNetSource _currentSource;
		private ILog _log;

		private delegate TwainDotNet.Twain Init(IntPtr hwnd);

		public int? CurrentSourceIndex { get { return _currentSource == null ? (int?)null : _currentSource.Index; } }
		public ISource CurrentSource { get { return _currentSource; } }

		public TwainDotNetScannerManager(WindowsMessageLoopThread windowsMessageLoop)
		{
			_windowsMessageLoop = windowsMessageLoop;
			_sources = new List<TwainDotNetSource>();
			_log = LogManager.GetLogger(typeof(TwainDotNetScannerManager));

			var init = new Init(Initialize);
			_twain = _windowsMessageLoop.Invoke<TwainDotNet.Twain>(init, new object[] { _windowsMessageLoop.Hwnd });

			RefreshSources();
		}
		public int SourceCount
		{
			get
			{
				return _sources == null ? 0 : _sources.Count;
			}
		}
		public ISource GetSource(int index)
		{
			ISource twainSource;
			try
			{
				twainSource = _sources[index];
			}
			catch (Exception)
			{
				throw new Exception(string.Format("Source with index {0} not found", index));
			}

			return twainSource;
		}

		public List<ISource> GetSources()
		{
			if (_sources.Count == 0)
			{
				RefreshSources();
			}
			return _sources == null ? null : _sources.ConvertAll(s => (ISource)s);
		}

		public void ChangeSource(int index)
		{
			TwainDotNetSource source;
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

		private static TwainDotNet.Twain Initialize(IntPtr hwnd)
		{

			var twain = new TwainDotNet.Twain(new WinFormsWindowMessageHook(hwnd));
			return twain;
		}

		private void RefreshSources()
		{
			_sources.Clear();

			var i = 0;
			var sources = new List<TwainDotNetSource>();
			foreach (var sourceName in _twain.SourceNames)
			{
				sources.Add(new TwainDotNetSource(i, sourceName, _twain, _windowsMessageLoop));
				i++;
			}

			_sources = sources;	
		}
	}
}
