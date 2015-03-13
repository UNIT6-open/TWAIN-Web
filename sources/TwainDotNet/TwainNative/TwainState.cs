namespace TwainDotNet.TwainNative
{
	public enum TwainState
	{
		PreSession = 1,
		SourceManagerLoaded = 2,
		SourceManagerOpen = 3,
		SourceOpen = 4,
		SourceEnabled = 5,
		TransferReady = 6,
		Transfering = 7
	}
}
