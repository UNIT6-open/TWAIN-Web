using System;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;

namespace TwainWeb.ServiceManager
{
	class ServiceHelper
	{
		private readonly string _serviceName;
		private readonly string _serviceFilename;
		private readonly ServiceController _service;
		private readonly bool _isInstalled;
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
				string serviceName = _service.DisplayName;
				_isInstalled = true;
			}
			catch (InvalidOperationException) { }
			finally
			{
				_service.Close();
			}

		}
		internal void Install()
		{
			var exeFile = Path.Combine(AssemblyDirectory, _serviceFilename);
			if (_isInstalled)
			{
				ManagedInstallerClass.InstallHelper(new[] { "/u", exeFile });
			}
			ManagedInstallerClass.InstallHelper(new[] { exeFile });

			SetRecoveryOptions(_serviceName);
		}

		internal void Uninstall()
		{
			if (_isInstalled)
			{
				var exeFile = Path.Combine(AssemblyDirectory, _serviceFilename);
				ManagedInstallerClass.InstallHelper(new[] { "/u", exeFile });
			}
		}

		private static string AssemblyDirectory
		{
			get
			{
				string codeBase = Assembly.GetExecutingAssembly().CodeBase;
				UriBuilder uri = new UriBuilder(codeBase);
				string path = Uri.UnescapeDataString(uri.Path);
				return Path.GetDirectoryName(path);
			}
		}
		internal void Start()
		{
			
			if (_isInstalled && 
				(_service.Status == ServiceControllerStatus.Stopped || 
				_service.Status == ServiceControllerStatus.StopPending))
			{
				_service.Start();
				_service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(50));
				
			}
		}
		internal void Stop()
		{
			if (_isInstalled && _service.CanStop)
			{
				_service.Stop();
				_service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(50));
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
	}
}
