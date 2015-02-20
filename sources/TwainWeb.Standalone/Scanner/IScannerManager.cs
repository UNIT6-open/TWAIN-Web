using System.Collections.Generic;

namespace TwainWeb.Standalone.Scanner
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
