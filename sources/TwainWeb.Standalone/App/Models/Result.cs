namespace TwainWeb.Standalone.App.Models
{
	public class Result
	{
		public string Error { get; set; }

		public bool Validate()
		{
			return Error == null;
		}
	}
}
