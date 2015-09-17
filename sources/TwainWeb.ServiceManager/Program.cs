using System;
using System.Threading;

namespace TwainWeb.ServiceManager
{
	class Program
	{
		static void Main(string[] args)
		{

			var serviceHelper = new ServiceHelper("TWAIN@Web");

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
				case "-restart":
					serviceHelper.Restart();
					return;
			}
		}
	}
}
