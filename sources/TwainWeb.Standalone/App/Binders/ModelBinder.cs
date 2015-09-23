using System;
using System.Collections.Generic;
using TwainWeb.Standalone.App.Models;
using TwainWeb.Standalone.App.Models.Request;

namespace TwainWeb.Standalone.App.Binders
{
    public class ModelBinder
    {
        private readonly Dictionary<string, string> _query;

        public ModelBinder(Dictionary<string, string> query)
        {
            _query = query;
        }

        private string TryGet(string key)
        {
            return _query.ContainsKey(key) ? _query[key] : null;
        }

        public DownloadFileParam BindDownloadFile()
        {
            var downloadParam = new DownloadFileParam { ListFiles = new List<DownloadFile>()};
            var i = 0;
            while(true)
            {                
                var fileName = TryGet("fileName"+i);
                var tempFile = TryGet("fileId"+i);
                if (fileName == null || tempFile == null)
                    break;
                downloadParam.ListFiles.Add(new DownloadFile(fileName,tempFile));
                i++;
            }                       
            if(downloadParam.ListFiles.Count == 0)
                throw new Exception("Нечего загружать (неверный запрос)");
            
            downloadParam.SaveAs = TryGetInt("saveAs", (int)GlobalDictionaries.SaveAsValues.Pictures);
            return downloadParam;
        }

        public int? BindSourceIndex()
        {
            int? sourceIndex = TryGetInt("sourceIndex", -1);
            return sourceIndex == -1 ? null : sourceIndex;
        }

        public string BindAjaxMethod()
        {
            return TryGet("method");
        }
        
        public ScanForm BindScanForm()
        {                 
            var command = new ScanForm
            {
                FileName = TryGet("Form.FileName"),
                FileCounter = TryGet("Form.FileCounter"),
                CompressionFormat = new CompressionFormat(TryGet("Form.CompressionFormat")),                
                ColorMode = TryGetInt("Form.ColorMode", 0),
                DPI = TryGetFloat("Form.Dpi", 150f),
                Source = TryGetInt("Form.Source", 0),
                IsPackage = TryGet("isPackage"),
                SaveAs = TryGetInt("Form.SaveAs", 0),
                Format = new FormatPage(TryGet("Form.Format")),
				DocumentHandlingCap = TryGetNullableInt("Form.ScanFeed", null)
            };           
            
            return command;
        }
        

        private float TryGetFloat(string key, float defaultValue)
        {
            var stringValue = TryGet(key);
            if (stringValue == null)
                return defaultValue;
            
            float floatResult;
            var parseResult = float.TryParse(stringValue, out floatResult);

            return parseResult ? floatResult : defaultValue;
        }

        private int TryGetInt(string key, int defaultValue)
        {
            var stringValue = TryGet(key);
            if (stringValue == null)
                return defaultValue;

            int intResult;
            var parseResult = int.TryParse(stringValue, out intResult);

            return parseResult ? intResult : defaultValue;
        }
		private int? TryGetNullableInt(string key, int? defaultValue)
        {
            var stringValue = TryGet(key);
            if (stringValue == null)
                return defaultValue;

            int intResult;
            var parseResult = int.TryParse(stringValue, out intResult);

            return parseResult ? intResult : defaultValue;
        }

    }
}
