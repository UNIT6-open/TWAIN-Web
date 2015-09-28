using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Metadata;
using System.Threading;

namespace TwainWeb.Standalone.App.Models.Response
{    
    
    public class ScannerSettings
    {
	    private const float Backlash = 0.3f;
	    private readonly string _name;
        private readonly int? _id;
		private readonly List<float> _flatbedResolutions;
		private readonly List<float> _feederResolutions;
		private readonly Dictionary<int, string> _pixelTypes;
        private List<FormatPage> _allowedFormats;
	    private readonly Dictionary<int, string> _scanFeeds;
		public ScannerSettings(int id, string name, List<float> flatbedResolutions = null, List<float> feederResolutions = null, Dictionary<int, string> pixelTypes = null, float? maxHeight = null, float? maxWidth = null, Dictionary<int, string> scanFeeds = null)
        {
            _name = name;
            _id = id;
			_flatbedResolutions = flatbedResolutions!=null && flatbedResolutions.Count>20
				?ReduceResolutionsCount(flatbedResolutions)
				:flatbedResolutions;
			_feederResolutions = feederResolutions != null && feederResolutions.Count > 20
				? ReduceResolutionsCount(feederResolutions)
				: feederResolutions; 
            _pixelTypes = pixelTypes;
            FillAllowedFormats(maxHeight, maxWidth);
			_scanFeeds = scanFeeds;
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
		        _allowedFormats = GlobalDictionaries.Formats;
		        return;
	        }
	        _allowedFormats = new List<FormatPage>();
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
                        _allowedFormats.Add(format);
                    else
                    {
                        reduceSize(ref maxWidth, ref maxHeight);
                        _allowedFormats.Add(new FormatPage { Name = format.Name, Height = maxHeight.Value, Width = maxWidth.Value });
                    }
                }
                else
                {
                    if (format.EqualsWithBackslash(maxWidth.Value, maxHeight.Value, Backlash))
                    {
                        _allowedFormats.Add(new FormatPage { Name = format.Name, Height = maxHeight.Value, Width = maxWidth.Value });
                        useStandartSizes = false;                        
                    }
                    else if (prevFormat != null && prevFormat.EqualsWithBackslash(maxWidth.Value, maxHeight.Value, Backlash))
                    {
                        _allowedFormats.Add(new FormatPage { Name = prevFormat.Name, Height = maxHeight.Value, Width = maxWidth.Value });
                        reduceSize(ref maxWidth, ref maxHeight);
                        _allowedFormats.Add(new FormatPage { Name = format.Name, Height = maxHeight.Value, Width = maxWidth.Value });
                        useStandartSizes = false;
                    }
                    else
                    {
                        _allowedFormats.Add(new FormatPage { Name = "Максимальный размер", Height = maxHeight.Value, Width = maxWidth.Value });
                        _allowedFormats.Add(format);
                        useStandartSizes = true;
                    }                    
                }
            }
            if(_allowedFormats.Count == 0) _allowedFormats.Add(new FormatPage{Name="Максимальный размер", Width=maxWidth.Value, Height=maxHeight.Value});
        }

        public bool Compare(int id, string name)
        {
            return id == _id && _name == name;
        }

	    private List<float> ReduceResolutionsCount(List<float> source)
	    {
		    if (source == null || source.Count == 0) return source;

			var result = new List<float>();
		    for (var i = 0; i < source.Count; i++)
		    {
				if (i == 0 || i == source.Count - 1 || Math.Abs(source[i] % 50) < 0.1f)
			    {
				    result.Add(source[i]);
			    }
		    }
		    return result;
	    }
        public string Serialize()
        {
            var result = "";

			if (_flatbedResolutions != null && _flatbedResolutions.Count > 0)
			{
				result += ",\"flatbedResolutions\": [";
				for (var i = 0; i < _flatbedResolutions.Count; i++)
				{
					result += string.Format("{{\"key\": \"{0}\", \"value\":\"{0}\"}}{1}", 
						_flatbedResolutions[i], 
						i != (_flatbedResolutions.Count - 1) ? "," : "");
				}
				result += "]";
			}
			if (_feederResolutions != null && _feederResolutions.Count > 0)
			{
				result += ",\"feederResolutions\": [";
				for (var i = 0; i < _feederResolutions.Count; i++)
				{
					result += string.Format("{{\"key\": \"{0}\", \"value\":\"{0}\"}}{1}", 
						_feederResolutions[i], 
						i != (_feederResolutions.Count - 1) ? "," : "");
				}
				result += "]";
			}
			
           var iter = 0;
            if (_pixelTypes != null && _pixelTypes.Count > 0)
            {
                result += ",\"pixelTypes\": [";
	            foreach (var pixelType in _pixelTypes)
	            {
		            result += "{ \"key\": \"" + pixelType.Key + "\", \"value\": \"" +
                        pixelType.Value + "\"}"
						+ (iter != (_pixelTypes.Count - 1) ? "," : "");

					iter++;
	            }
	           
                result += "]";
            }
            if (_allowedFormats != null)
            {
                result += ",\"allowedFormats\": [";
                for (var i =0; i< _allowedFormats.Count; i++)
                {
                    result += "{\"key\": \"" + _allowedFormats[i].Width + "*" + _allowedFormats[i].Height + "\", \"value\":\"" + _allowedFormats[i].Name + "\"}"
                        + (i != (_allowedFormats.Count - 1) ? "," : "");
                }
                result += "]";
            }

	        if (_scanFeeds != null)
	        {
				result += ",\"scanFeeds\": [";
		        var currentKeyNumber = 0;
		        foreach (var key in _scanFeeds.Keys)
		        {
			        result += "{\"key\": \"" + key + "\", \"value\":\"" + _scanFeeds[key] + "\"}"
						+ (currentKeyNumber != (_scanFeeds.Keys.Count - 1) ? "," : "");
			        currentKeyNumber++;
		        }
				result += "]";
	        }
            return result;
        }
    }
}
