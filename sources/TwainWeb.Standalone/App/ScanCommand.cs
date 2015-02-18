using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using TwainWeb.Standalone.Twain;

namespace TwainWeb.Standalone.App
{
    public class ScanCommand
    {
        private int waitTimeForChangeSource = 15000;
	    private const int WaitTimaeForScan = 30000;

	    public ScanCommand(ScanForm command, IScannerManager scannerManager)
        {                     
            _command = command;
         
	        _scannerManager = scannerManager;
        }
        private readonly ScanForm _command;
        private readonly IScannerManager _scannerManager;

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }     
        public ScanResult Execute(object markerAsynchrone)
        {
            var scanResult = new ScanResult();
            try
            {
                Image image = null;
                lock(markerAsynchrone)
                {
					if (_scannerManager.CurrentSourceIndex != _command.Source)
					{
						_scannerManager.ChangeSource(_command.Source);

						if (_scannerManager.CurrentSourceIndex != _command.Source)
						{
							scanResult.Error = "Не удается изменить источник данных";
							return scanResult;
						}
					}

					var settingAcquire = new SettingsAcquire { Format = _command.Format, Resolution = _command.DPI, pixelType = Extensions.SearchPixelType(_command.ColorMode, TwPixelType.BW) };


					//var images = _scannerManager.Scan(settingAcquire);
					var images = new AsyncWorker<SettingsAcquire, List<Image>>().RunWorkAsync(settingAcquire, "Asquire",
		                _scannerManager.CurrentSource.Scan, WaitTimaeForScan);

					if (images != null && images.Count == 1)
					{
						image = (Image)images[0].Clone();
						((Bitmap)image).SetResolution(_command.DPI, _command.DPI);
					}
                }
                if (image == null)
                {
                    scanResult.Error = "Сканирование завершилось неудачей! Попробуйте переподключить сканер либо повторить сканирование с помощью другого устройства.";
                    return scanResult;
                }                
                SaveImage(ref scanResult, image);
                scanResult.FillContent();
            }
            catch (TwainException ex)
            {
                scanResult.Error = ex.Message;
            }
            return scanResult;
        }

        void SaveImage(ref ScanResult scanResult, Image image)
        {
            try
            {
                scanResult.FileName = _command.FileName+(String.IsNullOrEmpty(_command.FileCounter) ? "" : ("_" + _command.FileCounter));
                if (_command.IsPackage == null || _command.SaveAs == (int)GlobalDictionaries.SaveAsValues.Pictures)
                    scanResult.FileName += "." + GlobalDictionaries.SearchingKeyInImgFormats(_command.CompressionFormat.ImgFormat);
                var jgpEncoder = GetEncoder(_command.CompressionFormat.ImgFormat);
                var myEncoder = Encoder.Quality;
                var parameters = new EncoderParameters(1);
                parameters.Param[0] = new EncoderParameter(myEncoder, _command.CompressionFormat.compression);
                var file = Path.GetTempFileName();
                image.Save(file, jgpEncoder, parameters);
                scanResult.TempFile = Path.GetFileName(file);
                GlobalDictionaries.Scans.Add(scanResult.TempFile);
            }
            catch (Exception ex){ scanResult.Error = ex.Message; }
        }        
    }
}
