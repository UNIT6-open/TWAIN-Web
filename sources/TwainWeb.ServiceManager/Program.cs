using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace TwainWeb.ServiceManager
{
	class Program
	{
		static void Main(string[] args)
		{

				using (var serviceHelper = new ServiceHelper("TWAIN@Web", "TwainWeb.Standalone.exe"))
				{

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

						case "-run-uninstaller":

							var uninstallString = GetUninstallString();

							if (File.Exists(uninstallString))
							{
								var process = new Process {StartInfo = {FileName = uninstallString}};
								process.Start();
								process.WaitForExit();

								var pr = Process.GetProcessesByName("appun-1");
								if (pr.Length > 0)
								{
									pr[0].WaitForExit();
									var exitCode = File.Exists(uninstallString) ? 1 : 0;
									Environment.Exit(exitCode);

								}
							}
							
							
							return;
					}
				}
			
		}

		public static UIntPtr HKEY_LOCAL_MACHINE = new UIntPtr(0x80000002u);

		public static string GetUninstallString()
		{
			const string productsRoot = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\TWAIN@Web Standalone";
			var helper = new RegistryHelper();
			var uninstallString = helper.ReadRegKey(HKEY_LOCAL_MACHINE, productsRoot, "UninstallString");

			return uninstallString;
		}
	}
}
