using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace TwainWeb.Standalone.App
{
    public class ModelBinder
    {
        private readonly Dictionary<string, string> query;

        public ModelBinder(Dictionary<string, string> query)
        {
            this.query = query;
        }

        private string TryGet(string key)
        {
            return query.ContainsKey(key) ? query[key] : null;
        }

        public DownloadFileParam BindDownloadFile()
        {
            var downloadParam = new DownloadFileParam { ListFiles = new List<DownloadFile>()};
            var i = 0;
            while(true)
            {                
                var FileName = TryGet("fileName"+i);
                var TempFile = TryGet("fileId"+i);
                if (FileName == null || TempFile == null)
                    break;
                downloadParam.ListFiles.Add(new DownloadFile{ FileName = FileName, TempFile = TempFile });
                i++;
            };                        
            if(downloadParam.ListFiles.Count == 0)
                throw new Exception("Нечего загружать (неверный запрос)");
            var saveAs = TryGetInt("saveAs", (int)GlobalDictionaries.SaveAsValues.Pictures);
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
                DPI = TryGetFloat("Form.DPI", 150f),
                Source = TryGetInt("Form.Source", 0),
                IsPackage = TryGet("isPackage"),
                SaveAs = TryGetInt("Form.SaveAs", 0),
                Format = new FormatPage(TryGet("Form.Format"))
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

    }
}
