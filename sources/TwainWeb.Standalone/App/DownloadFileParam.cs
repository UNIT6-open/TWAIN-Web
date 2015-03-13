using System.Collections.Generic;

namespace TwainWeb.Standalone.App
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

	public class FileWithContent
	{
		public FileWithContent(DownloadFile fileName, byte[] content)
		{
			FileName = fileName;
			Content = content;
		}
		public DownloadFile FileName { get; private set; }
		public byte[] Content { get; private set; }
	}
}
