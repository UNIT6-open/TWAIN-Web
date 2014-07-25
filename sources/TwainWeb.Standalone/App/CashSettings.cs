using System;
using System.Collections.Generic;
using System.Text;
using TwainWeb.Standalone.Twain;

namespace TwainWeb.Standalone.App
{
    public class CashSettings
    {
        private List<ScannerSettings> scannersSettings = new List<ScannerSettings>();
        public DateTime LastUpdateTime = DateTime.UtcNow;

        public bool NeedUpdateNow(DateTime dateUtcNow)
        {
            return (dateUtcNow - this.LastUpdateTime).Minutes >= 1;
        }

        public ScannerSettings Search(Twain32 _twain, int searchIndex)
        {
            string sourceName = null;
            int? sourceID = null;
            try
            {
                var sourceProduct = _twain.GetSourceProduct(searchIndex);
                sourceName = sourceProduct.Name;
                sourceID = sourceProduct.ID;
            }
            catch (Exception ex)
            {
                return null;
            }

            foreach (var setting in scannersSettings)
            {
                if (setting.Compare(sourceID.Value, sourceName))
                    return setting;
            }
            return null;
        }

        public void Update(Twain32 _twain)
        {
            if(_twain.CloseSM())
            {
                if(_twain.OpenSM())
                {
                    if (_twain.SourcesCount > 0)
                    {
                        bool activeSource;
                        var settingsForDelete = new List<ScannerSettings>();
                        foreach (var setting in scannersSettings)
                        {
                            activeSource = false;
                            for (int i = 0; i < _twain.SourcesCount; i++)
                            {
                                var sourceProduct = _twain.GetSourceProduct(i);
                                if (setting.Compare(sourceProduct.ID, sourceProduct.Name))
                                {
                                    activeSource = true;
                                    break;
                                }                                
                            }
                            if (!activeSource)
                                settingsForDelete.Add(setting);
                        }
                        foreach (var setting in settingsForDelete)
                        {
                            this.scannersSettings.Remove(setting);
                        }
                    }
                    else
                        this.scannersSettings.RemoveAll(x => true);
                    this.LastUpdateTime = DateTime.UtcNow;
                }
            }
        }

        public ScannerSettings PushCurrentSource(Twain32 _twain)
        {
            Twain32.Enumeration resolutions = null;
            Twain32.Enumeration pixelTypes = null;
            float? maxHeight = null;
            float? maxWidth = null;
            try
            {
                resolutions = _twain.GetResolutions();
                pixelTypes = _twain.GetPixelTypes();
                maxHeight = _twain.GetPhisicalHeight();
                maxWidth = _twain.GetPhisicalWidth();
            }
            catch (Exception ex) {  } 
            var sourceProduct = _twain.GetSourceProduct(_twain.SourceIndex);
            var settings = new ScannerSettings(sourceProduct.ID, sourceProduct.Name, resolutions, pixelTypes, maxHeight, maxWidth);
            this.scannersSettings.Add(settings);
            return settings;
        }
    }
}
