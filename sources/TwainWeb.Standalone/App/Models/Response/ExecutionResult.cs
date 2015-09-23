namespace TwainWeb.Standalone.App.Models.Response
{
	public class ExecutionResult
	{
		public ExecutionResult()
		{
		}

		public ExecutionResult(string error)
		{
			Error = error;
		}
		public string Error { get; set; }

		public bool Validate()
		{
			return Error == null;
		}
	}
}
