using System;
using System.Threading;

namespace TwainWeb.ServiceManager
{
	class Program
	{
		static void Main(string[] args)
		{

			var serviceHelper = new ServiceHelper("TWAIN@Web", "TwainWeb.Standalone.exe");

			var parameter = string.Concat(args);
			switch (parameter)
			{
				case "-install":
					serviceHelper.Install();
					return;
				case "-uninstall":
					serviceHelper.Uninstall();
					return;
				case "-start":
					serviceHelper.Start();
					return;
				case "-stop":
					serviceHelper.Stop();
					return;
				case "-restart":
					serviceHelper.Restart();
					return;
			}
		}
	}
}
