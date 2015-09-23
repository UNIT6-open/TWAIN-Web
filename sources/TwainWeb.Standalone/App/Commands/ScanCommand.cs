using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using log4net;
using TwainWeb.Standalone.App.Models.Request;
using TwainWeb.Standalone.App.Models.Response;
using TwainWeb.Standalone.App.Scanner;
using TwainWeb.Standalone.App.Tools;
using TwainWeb.Standalone.App.Twain;

namespace TwainWeb.Standalone.App.Commands
{
	public class ScanCommand
	{
		private const int WaitTimeForChangeSource = 15000;
		private const int WaitTimaeForScan = 50000;

		private readonly ILog _log;

		public ScanCommand(ScanForm command, IScannerManager scannerManager)
		{
			_command = command;
			_scannerManager = scannerManager;
			_log = LogManager.GetLogger(typeof (ScanCommand));
		}

		private readonly ScanForm _command;
		private readonly IScannerManager _scannerManager;

		public ScanResult Execute(object markerAsynchrone)
		{
			_log.Info("======================================= SCAN COMMAND ========================================");
			_log.Info(string.Format("Start execute with scan params: " +
			                        "source={0}, sourceFeed={1}, dpi={2}, colorMode={3}, compressionFormat={4}, format={5}, " +
			                        "isPackage={6}, saveAs={7}", 
				_command.Source, 
				_command.DocumentHandlingCap, 
				_command.DPI,
				_command.ColorMode,
				_command.CompressionFormat.ImgFormat,
				_command.Format.Name,
				_command.IsPackage,
				_command.SaveAs));

			ScanResult scanResult;
			try
			{
				var scannedImages = new List<Image>();
				lock (markerAsynchrone)
				{
					if (_scannerManager.CurrentSourceIndex != _command.Source)
					{
						new AsyncWorker<int>().RunWorkAsync(_command.Source, _scannerManager.ChangeSource,
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
						PixelType = _command.ColorMode,
						ScanSource =  _command.DocumentHandlingCap
					};

					var images = new AsyncWorker<SettingsAcquire, List<Image>>().RunWorkAsync(settingAcquire, _scannerManager.CurrentSource.Scan, WaitTimaeForScan);

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
					var downloadFile = SaveImage(image);
					var singleScanResult = new SingleScanResult();
					singleScanResult.FillContent(downloadFile);

					scanResult = singleScanResult;

					image.Dispose();
				}
				else
				{
					var downloadFiles = new List<DownloadFile>();
					int counter;
					try
					{
						counter = int.Parse(_command.FileCounter);
					}
					catch (Exception)
					{
						counter = 1;
					}
					foreach (var scannedImage in scannedImages)
					{
						var downloadFile = SaveImage(scannedImage, counter++);
						downloadFiles.Add(downloadFile);
						scannedImage.Dispose();
					}
				
					var multipleScanResult = new MultipleScanResult();
					multipleScanResult.FillContent(downloadFiles);
					scanResult = multipleScanResult;
					/*return new SingleScanResult(
							"Сканирование завершилось неудачей! Попробуйте переподключить сканер либо повторить сканирование с помощью другого устройства.");*/

				}
			}
			catch (TwainException ex)
			{
				return new SingleScanResult(ex.Message);
			}

			_log.Info("Scan command executed");
			return scanResult;
		}

		private DownloadFile SaveImage(Image image, int? counter = null)
		{
			if (image == null) throw new ArgumentException("image");

			var filename = ImageTools.CreateFilename(
				_command.FileName, 
				counter.HasValue?counter.Value.ToString():_command.FileCounter,
				_command.IsPackage != null,
				(GlobalDictionaries.SaveAsValues)_command.SaveAs,
				_command.CompressionFormat.ImgFormat);

			var file = Path.GetTempFileName();
			var tempfile = Path.GetFileName(file);	

			var downloadFile = new DownloadFile(filename, tempfile);

			ImageTools.CompressAndSaveImage(image, file, _command.CompressionFormat);
		
			GlobalDictionaries.Scans.Add(downloadFile.TempFile);

			return downloadFile;
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
