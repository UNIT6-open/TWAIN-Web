using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading;
using TwainWeb.Standalone.Twain;

namespace TwainWeb.Standalone.App
{
    public class ScanCommand
    {
        private int waitTimeForChangeSource = 15000;     
        private int waitTimaeForScan = 300000;
        public ScanCommand(ScanForm command, Twain32 twain)
        {                     
            _command = command;
            _twain = twain;
        }
        private ScanForm _command;
        private Twain32 _twain;

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
                    if (!_twain.OpenSM())
                    {
                        scanResult.Error = "Возникла непредвиденная ошибка, пожалуйста перезапустите TWAIN@Web";
                        return scanResult;
                    }
                    if (this._twain.SourceIndex != _command.Source)
                    {
                        AsyncMethods.asyncWithWaitTime<int>(_command.Source, "ChangeSource", _twain.ChangeSource, waitTimeForChangeSource, _twain.waitHandle);
                        if (_command.Source != _twain.SourceIndex)
                        {
                            scanResult.Error = _twain.ChangeSourceResp;
                            return scanResult;
                        }
                    }
                    var settingAcquire = new SettingsAcquire {Format = _command.Format, Resolution = _command.DPI, pixelType = Extensions.SearchPixelType(_command.ColorMode, TwPixelType.BW) };
                    AsyncMethods.asyncWithWaitTime<SettingsAcquire>(settingAcquire, "MyAcquire", _twain.MyAcquire, waitTimaeForScan, _twain.waitHandle);
                    if (_twain.Images.Count == 1)
                    {
                        image = (Image)_twain.Images[0].Clone();
                        ((Bitmap)image).SetResolution(_command.DPI, _command.DPI);
                    }
                }
                if (image == null)
                {
                    scanResult.Error = "Сканирование завершилось неудачей! Попробуйте переподключить сканер либо повторить сканирование с помощью другого устройства.";
                    return scanResult;
                }                
                this.SaveImage(ref scanResult, image);
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
                var myEncoder = System.Drawing.Imaging.Encoder.Quality;
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
