using System;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;

namespace TwainWeb.ServiceManager
{
	class ServiceHelper:IDisposable
	{
		private readonly string _serviceName;
		private readonly string _serviceFilename;
		private readonly ServiceController _service;
		private bool _isInstalled;
		private ServiceControllerStatus _status;
		internal ServiceHelper(string servcieName, string serviceFilename)
		{
			_serviceFilename = serviceFilename;
			_serviceName = servcieName;
			_service = new ServiceController(_serviceName);

			try
			{
				// actually we need to try access ANY of service properties
				// at least once to trigger an exception
				// not neccessarily its name
				_status = _service.Status;
				File.AppendAllText("D:\\twain.txt", _status + "\r\n");
				_isInstalled = true;
			}
			catch (InvalidOperationException)
			{
				
			}
			

		}
		internal void Install()
		{
			File.AppendAllText("D:\\twain.txt", "install start\r\n");
			var exeFile = Path.Combine(AssemblyDirectory, _serviceFilename);
			if (_isInstalled)
			{
				Uninstall();
			}
			ManagedInstallerClass.InstallHelper(new[] { exeFile });

			SetRecoveryOptions(_serviceName);
			_isInstalled = true;
			File.AppendAllText("D:\\twain.txt", "installed\r\n");
		}

		internal void Uninstall()
		{
			
			if (_isInstalled)
			{
				Stop();
				File.AppendAllText("D:\\twain.txt", "uninstall start\r\n");
				var exeFile = Path.Combine(AssemblyDirectory, _serviceFilename);
				ManagedInstallerClass.InstallHelper(new[] { "/u", exeFile });
				var sp = new Stopwatch();
				sp.Start();
				
				while (true)
				{
					try
					{
						if (sp.ElapsedMilliseconds > 40000)
						{
							MessageBox.Show("При удалении Twain@Web возникла ошибка. Попробуйте повторно запустить программу удаления.");
							Environment.Exit(-1);
						}
						_service.Refresh();
						_status = _service.Status;
						Thread.Sleep(TimeSpan.FromMilliseconds(500));
					}
					catch (Exception e)
					{
						File.AppendAllText("D:\\twain.txt", "removed\r\n");
						break;
					}
				}
				File.AppendAllText("D:\\twain.txt", "uninstalled\r\n");
				_isInstalled = false;
			}
		}

		private static string AssemblyDirectory
		{
			get
			{
				var codeBase = Assembly.GetExecutingAssembly().CodeBase;
				var uri = new UriBuilder(codeBase);
				var path = Uri.UnescapeDataString(uri.Path);
				return Path.GetDirectoryName(path);
			}
		}
		internal void Start()
		{
			
			if (_isInstalled && 
				(_service.Status == ServiceControllerStatus.Stopped || 
				_service.Status == ServiceControllerStatus.StopPending))
			{
				File.AppendAllText("D:\\twain.txt", "start\r\n");
				_service.Start();
				_service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(50));
				File.AppendAllText("D:\\twain.txt", "started\r\n");
			}
		}
		internal void Stop()
		{
			if (_isInstalled && _service.CanStop)
			{
				File.AppendAllText("D:\\twain.txt", "stop...\r\n");
				_service.Stop();
				_service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(50));
				File.AppendAllText("D:\\twain.txt", "stoped\r\n");
			}
		}
		internal void Restart()
		{
			Stop();
			Thread.Sleep(TimeSpan.FromSeconds(1));
			Start();
		}

		static void SetRecoveryOptions(string serviceName)
		{
			int exitCode;
			using (var process = new Process())
			{
				var startInfo = process.StartInfo;
				startInfo.FileName = "sc";
				startInfo.WindowStyle = ProcessWindowStyle.Hidden;

				// tell Windows that the service should restart if it fails
				startInfo.Arguments = string.Format("failure \"{0}\" reset= 0 actions= restart/60000", serviceName);

				process.Start();
				process.WaitForExit();

				exitCode = process.ExitCode;
			}

			if (exitCode != 0)
				throw new InvalidOperationException();
		}

		public void Dispose()
		{
			if (_service != null)
			{
				_service.Close();
				_service.Dispose();
			}
		}
	}
}
