using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using TwainWeb.Standalone.App.Models;
using TwainWeb.Standalone.Scanner;
using TwainWeb.Standalone.Tools;
using TwainWeb.Standalone.Twain;

namespace TwainWeb.Standalone.App.Commands
{
	public class ScanCommand
	{
		private const int WaitTimeForChangeSource = 15000;
		private const int WaitTimaeForScan = 50000;

		public ScanCommand(ScanForm command, IScannerManager scannerManager)
		{
			_command = command;
			_scannerManager = scannerManager;
		}

		private readonly ScanForm _command;
		private readonly IScannerManager _scannerManager;

		public ScanResult Execute(object markerAsynchrone)
		{
			ScanResult scanResult;
			try
			{
				var scannedImages = new List<Image>();
				lock (markerAsynchrone)
				{
					if (_scannerManager.CurrentSourceIndex != _command.Source)
					{
						new AsyncWorker<int>().RunWorkAsync(_command.Source, "ChangeSource", _scannerManager.ChangeSource,
							WaitTimeForChangeSource);

						if (_scannerManager.CurrentSourceIndex != _command.Source)
						{
							return new SingleScanResult("Не удается изменить источник данных");
						}
					}

					var settingAcquire = new SettingsAcquire
					{
						Format = _command.Format,
						Resolution = _command.DPI,
						PixelType = _command.ColorMode
					};

					var images = new AsyncWorker<SettingsAcquire, List<Image>>().RunWorkAsync(settingAcquire, "Asquire",
						_scannerManager.CurrentSource.Scan, WaitTimaeForScan);

					if (images != null)
					{
						foreach (var image in images)
						{
							var clonedImage = (Image) image.Clone();
							image.Dispose();

							((Bitmap) clonedImage).SetResolution(_command.DPI, _command.DPI);
							scannedImages.Add(clonedImage);
						}

					}
				}
				if (scannedImages.Count == 0)
				{
					return new SingleScanResult(
							"Сканирование завершилось неудачей! Попробуйте переподключить сканер либо повторить сканирование с помощью другого устройства.");
				}
				
				if (scannedImages.Count == 1)
				{
					var image = scannedImages[0];
					scanResult = SaveImage(image);
					image.Dispose();
				}
				else
				{
					foreach (var scannedImage in scannedImages)
					{
						scannedImage.Dispose();
					}
					return new SingleScanResult(
							"Сканирование завершилось неудачей! Попробуйте переподключить сканер либо повторить сканирование с помощью другого устройства.");

				}
			}
			catch (TwainException ex)
			{
				return new SingleScanResult(ex.Message);
			}
			return scanResult;
		}

		private SingleScanResult SaveImage(Image image)
		{
			if (image == null) throw new ArgumentException("image");

			var filename = ImageTools.CreateFilename(
				_command.FileName, 
				_command.FileCounter,
				_command.IsPackage != null,
				(GlobalDictionaries.SaveAsValues)_command.SaveAs,
				_command.CompressionFormat.ImgFormat);

			var file = Path.GetTempFileName();
			var tempfile = Path.GetFileName(file);	

			var downloadFile = new DownloadFile(filename, tempfile);

			ImageTools.CompressAndSaveImage(image, file, _command.CompressionFormat);
		
			GlobalDictionaries.Scans.Add(downloadFile.TempFile);

			var result = new SingleScanResult();
			result.FillContent(downloadFile);
			return result;
		}

		#region createZip
		/*private ScanResult GetZip(List<Image> images)
{
	var imagesCount = images.Count;

	do
	{
		var img = images[images.Count - 1];
		images.Remove(img);
		img.Dispose();
	} while (images.Count > imagesCount/2);
	
	GC.Collect();
	var downloadFile = new DownloadFile();
	downloadFile.FileName = _command.FileName +
							(String.IsNullOrEmpty(_command.FileCounter) ? "" : ("_" + _command.FileCounter));
	if (_command.IsPackage == null || _command.SaveAs == (int)GlobalDictionaries.SaveAsValues.Pictures)
		downloadFile.FileName += "." + "zip";
		
	var file = Path.GetTempFileName();
		
	downloadFile.TempFile = Path.GetFileName(file);
	GlobalDictionaries.Scans.Add(downloadFile.TempFile);

	var fZip = File.Create(file);
	var zipOStream = new ZipOutputStream(fZip);
	var i = 1;
	foreach (var image in images)
	{
		using (var ms = new MemoryStream())
		{
			//image.Save(ms, ImageFormat.Bmp);
			Bitmap bm = new Bitmap(image);
			bm.Save(ms, ImageFormat.Bmp);
				

			ms.Position = 0;

			var entry = new ZipEntry((i++ + ".jpg"));
			zipOStream.PutNextEntry(entry);


			int bytesRead;
			var transferBuffer = new byte[1024];
			do
			{
				bytesRead = ms.Read(transferBuffer, 0, transferBuffer.Length);
				zipOStream.Write(transferBuffer, 0, bytesRead);
			} while (bytesRead > 0);

		}

	}
	zipOStream.Finish();
	zipOStream.Close();

	var result = new SingleScanResult();
	result.FillContent(new List<DownloadFile> {downloadFile});
	return result;
}*/

#endregion
	}
}
