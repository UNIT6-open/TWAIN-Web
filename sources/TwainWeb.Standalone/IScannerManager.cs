using System.Collections.Generic;
using TwainWeb.Standalone.Scanner;



namespace TwainWeb.Standalone
{
	public interface IScannerManager
	{
		int? CurrentSourceIndex { get; }
		
		ISource CurrentSource { get; }
		
		int SourceCount { get; }
		
		ISource GetSource(int index);
	
		List<ISource> GetSources();

		void ChangeSource(int index);

	}
}
