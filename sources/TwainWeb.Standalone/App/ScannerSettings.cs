using System;
using System.Collections.Generic;
using System.Text;
using TwainWeb.Standalone.Twain;

namespace TwainWeb.Standalone.App
{    
    
    public class ScannerSettings
    {
        private float backlash = 0.3f;
        private string _name;
        private int? _id;
        private Twain32.Enumeration _resolutions;
        private Twain32.Enumeration _pixelTypes;
        private List<FormatPage> allowedFormats;        
        public ScannerSettings(int id, string name, Twain32.Enumeration resolutions = null, Twain32.Enumeration pixelTypes = null, float? maxHeight = null, float? maxWidth = null)
        {
            this._name = name;
            this._id = id;
            this._resolutions = resolutions;
            this._pixelTypes = pixelTypes;
            FillAllowedFormats(maxHeight, maxWidth);
        }

        private void reduceSize(ref float? maxWidth, ref float? maxHeight)
        {
            if (!maxHeight.HasValue || !maxWidth.HasValue)
                return;
            var buffer = maxHeight;
            maxHeight = maxWidth;
            maxWidth = buffer / 2;
        }

        private void FillAllowedFormats(float? maxHeight, float? maxWidth)
        {
	        if (!maxHeight.HasValue || !maxWidth.HasValue)
	        {
		        allowedFormats = GlobalDictionaries.Formats;
		        return;
	        }
	        allowedFormats = new List<FormatPage>();
            FormatPage prevFormat = null;
            bool? useStandartSizes = null;
            foreach(var format in GlobalDictionaries.Formats)
            {
                if (format.Height > maxHeight || format.Width > maxWidth)
                {
                    prevFormat = format; 
                    continue; 
                }

                if (useStandartSizes.HasValue)
                {
                    if (useStandartSizes.Value)
                        allowedFormats.Add(format);
                    else
                    {
                        reduceSize(ref maxWidth, ref maxHeight);
                        allowedFormats.Add(new FormatPage { Name = format.Name, Height = maxHeight.Value, Width = maxWidth.Value });
                    }
                }
                else
                {
                    if (format.EqualsWithBackslash(maxWidth.Value, maxHeight.Value, backlash))
                    {
                        allowedFormats.Add(new FormatPage { Name = format.Name, Height = maxHeight.Value, Width = maxWidth.Value });
                        useStandartSizes = false;                        
                    }
                    else if (prevFormat != null && prevFormat.EqualsWithBackslash(maxWidth.Value, maxHeight.Value, backlash))
                    {
                        allowedFormats.Add(new FormatPage { Name = prevFormat.Name, Height = maxHeight.Value, Width = maxWidth.Value });
                        reduceSize(ref maxWidth, ref maxHeight);
                        allowedFormats.Add(new FormatPage { Name = format.Name, Height = maxHeight.Value, Width = maxWidth.Value });
                        useStandartSizes = false;
                    }
                    else
                    {
                        allowedFormats.Add(new FormatPage { Name = "Максимальный размер", Height = maxHeight.Value, Width = maxWidth.Value });
                        allowedFormats.Add(format);
                        useStandartSizes = true;
                    }                    
                }
            }
            if(allowedFormats.Count == 0) allowedFormats.Add(new FormatPage{Name="Максимальный размер", Width=maxWidth.Value, Height=maxHeight.Value});
        }

        public bool Compare(int id, string name)
        {
            return id == _id && _name == name;
        }

        public string Serialize()
        {
            var result = "";
            if (_resolutions != null && _resolutions.Count > 0)
                result += string.Format(",\"minResolution\": \"{0}\", \"maxResolution\": \"{1}\"", _resolutions[0], _resolutions[_resolutions.Count - 1]);

            if (_pixelTypes != null && _pixelTypes.Count > 0)
            {
                result += ",\"pixelTypes\": [";
                for (int i = 0; i < _pixelTypes.Count; i++)
                {
                    result += "{ \"key\": \"" + (int)(TwPixelType)_pixelTypes[i] + "\", \"value\": \"" +
                        (GlobalDictionaries.PixelTypes.ContainsKey((TwPixelType)_pixelTypes[i]) ? GlobalDictionaries.PixelTypes[(TwPixelType)_pixelTypes[i]] : _pixelTypes[i].ToString()) + "\"}"
                        + (i != (_pixelTypes.Count - 1) ? "," : "");
                }
                result += "]";
            }
            if (this.allowedFormats != null)
            {
                result += ",\"allowedFormats\": [";
                for (var i =0; i< allowedFormats.Count; i++)
                {
                    result += "{\"key\": \"" + allowedFormats[i].Width + "*" + allowedFormats[i].Height + "\", \"value\":\"" + allowedFormats[i].Name + "\"}"
                        + (i != (allowedFormats.Count - 1) ? "," : "");
                }
                result += "]";
            }
            return result;
        }
    }
}
