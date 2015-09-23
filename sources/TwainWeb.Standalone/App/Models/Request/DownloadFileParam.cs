using System.Collections.Generic;

namespace TwainWeb.Standalone.App.Models.Request
{
    public class DownloadFileParam
    {
        public List<DownloadFile> ListFiles { get; set; }
        public int SaveAs { get; set; }
    }
    public class DownloadFile
    {
	    public DownloadFile(string filename, string tempfile)
	    {
		    FileName = filename;
		    TempFile = tempfile;
	    }
        public string FileName { get; set; }
        public string TempFile { get; set; }
    }
}
