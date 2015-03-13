using System.Collections.Generic;
using System.Text;
using TwainWeb.Standalone.Scanner;

namespace TwainWeb.Standalone.App.Models
{
	public class ScannerParametersQueryResult:Result
	{
		private readonly List<ISource> _sources;
		private readonly ScannerSettings _settings;
		private int? _sourceIndex;

		public ScannerParametersQueryResult(string error)
		{
			Error = error;
		}
		public ScannerParametersQueryResult(List<ISource> sources, ScannerSettings settings, int? sourceIndex)
		{
			_sourceIndex = sourceIndex;
			_sources = sources;
			_settings = settings;
		}

		public string Serialize()
		{
			var sb = new StringBuilder();
			sb.Append("{");

			if (_sources != null && _sources.Count != 0 && _sourceIndex.HasValue)
			{
				sb.Append("\"sources\":{ \"selectedSource\": \"" + _sourceIndex.Value + "\", \"sourcesList\":[");

				var i = 0;
				foreach (var sortedSource in _sources)
				{
					sb.AppendFormat(
						"{{ \"key\": \"{0}\", \"value\": \"{1}\"}}",
						sortedSource.Index,
						sortedSource.Name);

					if (i != (_sources.Count - 1))
						sb.Append(",");
						
					i++;
				}

				sb.Append("]}");

				if (_settings != null)
					sb.Append(_settings.Serialize());
				
			}
			sb.Append("}");

			return sb.ToString();
		}


	}
}
