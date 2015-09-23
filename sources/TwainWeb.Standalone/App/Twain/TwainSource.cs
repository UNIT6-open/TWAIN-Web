using System;
using System.Collections.Generic;
using System.Drawing;
using TwainWeb.Standalone.App.Models.Response;
using TwainWeb.Standalone.App.Scanner;

namespace TwainWeb.Standalone.App.Twain
{
	class TwainSource:ISource
	{
		public int Index { get; private set; }
		public string Name { get; private set; }

		private readonly Source _source;
		private readonly Twain32 _twain32;
		public TwainSource(Source source, Twain32 twain32, int index)
		{
			_source = source;
			_twain32 = twain32;

			Index = index;
			Name = _source.Name;
		}

		public ScannerSettings GetScannerSettings()
		{
			if (_source == null)
			{
				throw new Exception("Не выбран источник данных для сканера.");
			}

			var settings = new ScannerSettings(
				Index,
				Name,
				GetAllowableResolutions(),
				GetAllowablePixelTypes(),
				GetMaxHeight(),
				GetMaxWidth());

			return settings;
		}

		public List<Image> Scan(SettingsAcquire settings)
		{
			if (!_twain32.OpenSM())
			{
				throw new Exception("Возникла непредвиденная ошибка, пожалуйста перезапустите TWAIN@Web");
			}
			_twain32.MyAcquire(settings);

			if (_twain32.Images == null) return null;

			var images = new List<Image>();
			foreach (var image in _twain32.Images)
			{
				images.Add(image);
			}
			return images;
		}

		private float? GetMaxWidth()
		{
			return _twain32.GetPhisicalWidth();
		}

		private float? GetMaxHeight()
		{
			return _twain32.GetPhisicalHeight();
		}

		private Dictionary<int, string> GetAllowablePixelTypes()
		{
			var pixelTypesVector = _twain32.GetPixelTypes();
			if (pixelTypesVector == null) return null;

			var allowablePixelTypes = new Dictionary<int, string>();
			foreach (var pixelType in pixelTypesVector.Items)
			{
				var key = (int)(TwPixelType)pixelType;
				var value = (GlobalDictionaries.PixelTypes.ContainsKey((TwPixelType) pixelType)
					? GlobalDictionaries.PixelTypes[(TwPixelType) pixelType]
					: pixelType.ToString());
				
				allowablePixelTypes.Add(key, value);
			}

			return allowablePixelTypes;
		}

		private List<float> GetAllowableResolutions()
		{
			var resolutionsVector = _twain32.GetResolutions();
			if (resolutionsVector == null) return null;

			var allowableResolutions = new List<float>();
			foreach (var resolution in resolutionsVector.Items)
			{
				allowableResolutions.Add((float)resolution);
			}

			return allowableResolutions;
		}


	}
}
