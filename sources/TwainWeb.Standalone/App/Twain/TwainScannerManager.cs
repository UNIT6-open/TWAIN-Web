using System;
using System.Collections.Generic;
using TwainWeb.Standalone.App.Scanner;

namespace TwainWeb.Standalone.App.Twain
{
	class TwainScannerManager:IScannerManager
	{
		private readonly List<TwainSource> _sources;
		private TwainSource _currentSource;
		private readonly Twain32 _twain32;

		public TwainScannerManager()
		{
			_sources = new List<TwainSource>();
			_twain32 = new Twain32();
			_twain32.AppProductName = "Twain@Web";
		}
		public int? CurrentSourceIndex 
		{ 
			get
			{
				return _twain32.TwainState.IsOpenDataSource ? (int?) _twain32.SourceIndex : null;
			} 
		}
		public ISource CurrentSource { get { return _currentSource; } }

		public int SourceCount
		{
			get
			{
				if (_sources.Count == 0)
				{
					RefreshSources();
				}
				return _twain32.SourcesCount;
			}
		}

		public ISource GetSource(int index)
		{
			ISource source;
			try
			{
				source = _sources[index];
			}
			catch (Exception)
			{
				throw new Exception(string.Format("Source with index {0} not found", index));
			}

			return source;
		}

		public List<ISource> GetSources()
		{
			if (_sources.Count == 0)
			{
				RefreshSources();
			}
			if (_sources == null) return null;

			var sortedSources = SortSources(_sources.ConvertAll(s => (ISource)s));
			
			return sortedSources;
		}

		public void ChangeSource(int index)
		{
			_twain32.ChangeSource(index);
			if (_twain32.ChangeSourceResp != null)
				throw new Exception(_twain32.ChangeSourceResp);

			var currentSource = _twain32.GetSourceProduct(_twain32.SourceIndex);
			_currentSource = new TwainSource(currentSource, _twain32, index);
		}

		private void RefreshSources()
		{
			_sources.Clear();

			if (_twain32.CloseSM())
				if (_twain32.OpenSM())
				{

					for (var i = 0; i < _twain32.SourcesCount; i++)
					{
						var source = _twain32.GetSourceProduct(i);
						_sources.Add(new TwainSource(source, _twain32, i));
					}
				}
				else
					throw new Exception("Не удалось открыть менеджер источников");
			else
				throw new Exception("Не удалось закрыть менеджер источников");
			
				
		}
		private List<ISource> SortSources(IEnumerable<ISource> initialSources)
		{
			if (initialSources == null) throw new ArgumentNullException("initialSources");

			var sourses = new List<ISource>();

			var soursesWithoutWia = new List<ISource>();

			foreach (var sourse in initialSources)
			{
				if (sourse.Name.ToLower().Contains("wia"))
				{
					sourses.Add(sourse);
				}
				else
				{
					soursesWithoutWia.Add(sourse);
				}
			}

			foreach (var otherSourse in soursesWithoutWia)
			{
				sourses.Add(otherSourse);
			}

			return sourses;
		} 
	}
}
