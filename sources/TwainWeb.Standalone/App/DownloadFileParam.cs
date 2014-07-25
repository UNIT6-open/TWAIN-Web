using System;
using System.Collections.Generic;
using System.Text;

namespace TwainWeb.Standalone.App
{
    public class DownloadFileParam
    {
        public List<DownloadFile> ListFiles { get; set; }
        public int SaveAs { get; set; }
    }
    public class DownloadFile
    {
        public string FileName { get; set; }
        public string TempFile { get; set; }
    }
}
