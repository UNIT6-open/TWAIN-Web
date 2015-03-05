namespace TwainWeb.Standalone.App.Models
{
    public abstract class ScanResult:Result
    {
		protected ScanResult()
		{
		}
	    protected ScanResult(string error)
	    {
		    Error = error;
	    }
       
		public abstract void FillContent(DownloadFile file);
		public byte[] Content { get; protected set; }
    }
}
