using System;
using System.Collections.Generic;
using System.Text;

namespace TwainWeb.Standalone.App
{
    public class ScanResult : DownloadFile
    {
        public byte[] FileContent { get { return this._fileContent; } }
        private byte[] _fileContent;        
        public string Error { get; set; }

        public bool Validate()
        {
            return Error == null;
        }

        public void FillContent()
        {
            this._fileContent = Encoding.UTF8.GetBytes("{\"file\": \""+this.FileName.ToString()+"\", \"temp\": \""+this.TempFile+"\"}");
        }
    }
}
