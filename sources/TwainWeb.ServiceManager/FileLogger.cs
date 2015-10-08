using System;
using System.IO;

namespace TwainWeb.ServiceManager
{
	internal class FileLogger
	{
		private readonly string _filename;
		internal FileLogger(string filename)
		{
			_filename = filename;
		}

		internal static FileLogger GetLogger(string filename)
		{
			return new FileLogger(filename);
		} 
		internal void Info(string text)
		{
			AppendText("INFO", text);
		}

		internal void Error(string text)
		{
			AppendText("ERROR", text);
		}

		private void AppendText(string type, string text)
		{
			try
			{
				File.AppendAllText(_filename, string.Format("{0} :: {1} :: {2}\r\n", type, DateTime.Now, text));
			}
			catch (Exception)
			{
				
			}
		}
	}
}
