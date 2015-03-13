using System;
using System.Text;

namespace TwainWeb.Standalone.App.Models
{
	class SingleScanResult:ScanResult
	{
		public SingleScanResult(string error) : base(error) { }
		public SingleScanResult() { }
		public override void FillContent( DownloadFile file)
		{
			if (file == null) throw new ArgumentNullException("file");

            Content = Encoding.UTF8.GetBytes(string.Format("{{\"file\": \"{0}\", \"temp\": \"{1}\"}}", file.FileName, file.TempFile));
        }

		
	}
}
