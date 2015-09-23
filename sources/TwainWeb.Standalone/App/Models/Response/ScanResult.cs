namespace TwainWeb.Standalone.App.Models.Response
{
    public abstract class ScanResult:ExecutionResult
    {
		protected ScanResult()
		{
		}

	    protected ScanResult(string error):base(error)
	    {
	    }

		public byte[] Content { get; set; }
    }
}
