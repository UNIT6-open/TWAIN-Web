using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using TwainDotNet;
using TwainDotNet.TwainNative;
using TwainWeb.Standalone.App;
using TwainWeb.Standalone.MessageLoop;
using TwainWeb.Standalone.Scanner;
using TwainWeb.Standalone.Twain;

namespace TwainWeb.Standalone.TwainNet
{
	class TwainDotNetSource:ISource
	{
		private readonly TwainDotNet.Twain _twain;
		private readonly ManualResetEvent _scanCompleteEvent;		
		private readonly WindowsMessageLoopThread _windowsMessageLoop;

		private List<Image> _images;
		
		private delegate void StartScan(ScanSettings settings);
		private delegate SourceSettings GetSettings();

		public TwainDotNetSource(int i, string sourceName, TwainDotNet.Twain twain, WindowsMessageLoopThread windowsMessageLoop)
		{
			Name = sourceName;
			Index = i;
			_windowsMessageLoop = windowsMessageLoop;
			_scanCompleteEvent = new ManualResetEvent(false);
			_twain = twain;
		}

		public int Index { get; private set; }
		public string Name { get; private set; }

		public ScannerSettings GetScannerSettings()
		{
			var getSettings = new GetSettings(GetTwainScannerSettings);
			var settings = _windowsMessageLoop.Invoke<SourceSettings>(getSettings);
			var scannerSettings = new ScannerSettings(Index, Name, settings.Resolutions, TwainPixelTypeExtensions.GetSelectListDictionary(settings.PixelTypes), settings.PhysicalHeight, settings.PhysicalWidth);

			return scannerSettings;
		}

		public List<Image> Scan(SettingsAcquire settings)
		{
			_images = new List<Image>();
			_scanCompleteEvent.Reset();

			var scanSettings = new ScanSettings
			{
				Resolution = new ResolutionSettings {
					Dpi = (int) settings.Resolution, 
					ColourSetting = (ColourSetting)settings.PixelType
				},
				Area = new AreaSettings(Units.Inches, 0, 0, settings.Format.Height, settings.Format.Width),
				ShowProgressIndicatorUI = false,
				ShowTwainUI = false
			};

			var scan = new StartScan(StartTwainScan);
			_windowsMessageLoop.Invoke(scan, new object[] { scanSettings });

			_scanCompleteEvent.WaitOne();
			return _images;
		}

		private void Twain_TransferImage(object sender, TransferImageEventArgs e)
		{
			if (e.Image != null)
			{
				var img = e.Image;
				if (_images == null) _images = new List<Image>();

				_images.Add((Image)img.Clone());
				img.Dispose();
			}
		}

		private void Twain_ScanningComplete(object sender, ScanningCompleteEventArgs e)
		{
			_twain.ScanningComplete -= Twain_ScanningComplete;
			_twain.TransferImage -= Twain_TransferImage;
			_scanCompleteEvent.Set();
		}

		private SourceSettings GetTwainScannerSettings()
		{
			//return new AsyncWorker<string, SourceSettings>().RunWorkAsync(Name, _twain.GetAwailableSourceSettings, 4000);
			return _twain.GetAwailableSourceSettings(Name);
		}
		private void StartTwainScan(ScanSettings settings)
		{
			_twain.ScanningComplete += Twain_ScanningComplete;
			_twain.TransferImage += Twain_TransferImage;
			settings.ShowTwainUI = false;
			settings.ShowProgressIndicatorUI = false;

			_twain.SelectSource(Name);
			_twain.StartScanning(settings);
		}
	}
}
