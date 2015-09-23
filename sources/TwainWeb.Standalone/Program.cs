using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Windows.Forms;
using TwainWeb.Standalone.Configurator;
using TwainWeb.Standalone.Host;

namespace TwainWeb.Standalone
{
    static class Program
    {
        
        static void Main(string[] args)
        {
	      
			//запуск службы
	        if (!Environment.UserInteractive || args.Length == 0)
	        {
		        ServiceBase.Run(new ScanService(Settings.Default.Port));
	        }
			
			//запуск консольного приложения Twain@WEB
			else if (args.Length == 1 && args[0] == "console")
			{
				RunFromConsole();
			}

			//перезапуск конфигуратора от имени администратора
			else if (args[0] == "config")
				RunConfiguratorAsAdmin();

			//запуск конфигуратора
		    else if (args.Length == 2 && args[0] == "configrun")
		    {
			    int secondParam;
			    var parseSuccess = int.TryParse(args[1], out secondParam);

				// по умолчанию проверка порта
				var argument = ConfigrunArg.CheckPortAvailability;
			    if (parseSuccess && Enum.IsDefined(typeof (ConfigrunArg), secondParam))
			    {
				    argument = (ConfigrunArg) secondParam;
			    }
			
				RunConfigurator(argument);
		    }

			//запуск браузера с открытой вкладкой Twain@WEB
		    else if (args[0] == "run")
			    OpenInBrowser();


        }

	    private static void RunConfiguratorAsAdmin()
	    {
		    var proc = new ProcessStartInfo
		    {
			    UseShellExecute = true,
			    WorkingDirectory = Environment.CurrentDirectory,
			    FileName = Application.ExecutablePath,
			    Verb = "runas",
			    Arguments = "configrun 2"
		    };
		    try
		    {
			    Process.Start(proc);
		    }
		    catch (Exception){}
	    }

	    private static void OpenInBrowser()
	    {
		    Process.Start("http://127.0.0.1:" + Settings.Default.Port + "/TWAIN@Web/");
	    }

		private static Process RunServiceManager(string command)
	    {
			var proc = new Process();
			var psi = new ProcessStartInfo
			{
				CreateNoWindow = true,
				FileName = "TwainWeb.ServiceManager.exe",
				Arguments = command,
				WindowStyle = ProcessWindowStyle.Hidden
			};

			proc.StartInfo = psi;
			proc.Start();

			return proc;
	    }

		private static void RunConfigurator(ConfigrunArg parameter)
		{
			var needToStartStop = parameter == ConfigrunArg.ConfigWithServiceInterruption;
			var needToChangeSettingsInConfigurator = 
				parameter == ConfigrunArg.Config ||
				parameter == ConfigrunArg.ConfigWithServiceInterruption;

			if (needToStartStop)			
				RunServiceManager("-stop").WaitForExit();

			Application.EnableVisualStyles();
		    Application.SetCompatibleTextRenderingDefault(false);
			var mainForm = new FormForSetPort(needToChangeSettingsInConfigurator);
		    Application.Run(mainForm);

			if (needToStartStop)
				RunServiceManager("-start").WaitForExit();
	    }

		
/*		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool AllocConsole();*/
		// P/Invoke:
		private enum FileType { Unknown, Disk, Char, Pipe };
		private enum StdHandle { Stdin = -10, Stdout = -11, Stderr = -12 };
		[DllImport("kernel32.dll")]
		private static extern FileType GetFileType(IntPtr hdl);
		[DllImport("kernel32.dll")]
		private static extern IntPtr GetStdHandle(StdHandle std);
	    private static void RunFromConsole()
	    {
			ConsoleManager.Show();

		    var service = new ScanService(Settings.Default.Port);
		    try
		    {
			    service.Start();
			    Console.WriteLine("Press any key to stop service");
			    Console.ReadKey();
		    }
		    catch (Exception e)
		    {
			    Console.WriteLine(e);
		    }
		    finally
		    {
			    service.Stop();
		    }
	    }

		private enum ConfigrunArg
		{
			/// <summary>
			/// Запуск конфигуратора для проверки доступности порта
			/// </summary>
			CheckPortAvailability,

			/// <summary>
			/// Запуск конфигуратора для настройки Twain@WEB
			/// </summary>
			Config,

			/// <summary>
			/// Запуск конфигуратора с прерыванием работы службы
			/// </summary>
			ConfigWithServiceInterruption
		}
           
    }
}
