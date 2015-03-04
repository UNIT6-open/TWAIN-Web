namespace TwainWeb.Standalone.App
{
    public abstract class ScanResult
    {
		protected ScanResult()
		{
		}
	    protected ScanResult(string error)
	    {
		    Error = error;
	    }
        public string Error { get; set; }

        public bool Validate()
        {
            return Error == null;
        }

		public abstract void FillContent(DownloadFile file);
		public byte[] Content { get; protected set; }
    }
}
