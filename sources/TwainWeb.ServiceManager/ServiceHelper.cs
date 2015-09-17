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
		private readonly ServiceController _service;
		private readonly bool _isInstalled;
		internal ServiceHelper(string servcieName)
		{
			_serviceName = servcieName;
			_service = new ServiceController(_serviceName);
			_isInstalled = _service != null;
		}
		internal void Install()
		{
			if (_isInstalled)
			{
				ManagedInstallerClass.InstallHelper(new[] { "/u", Assembly.GetExecutingAssembly().Location });
			}
			ManagedInstallerClass.InstallHelper(new[] { Assembly.GetExecutingAssembly().Location });

			SetRecoveryOptions(_serviceName);
		}

		internal void Uninstall()
		{
			if (_isInstalled)
			{
				ManagedInstallerClass.InstallHelper(new[] { "/u", Assembly.GetExecutingAssembly().Location });
			}
		}

		internal void Start()
		{
			File.AppendAllText("D:\\logSH.txt", "start\r\n");
			if (_isInstalled && 
				(_service.Status == ServiceControllerStatus.Stopped || 
				_service.Status == ServiceControllerStatus.StopPending))
			{
				File.AppendAllText("D:\\logSH.txt", "status before: " + _service.Status + "\r\n");
				_service.Start();
				_service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(50));
				File.AppendAllText("D:\\logSH.txt", "status after: " + _service.Status + "\r\n");
			}
		}
		internal void Stop()
		{
			File.AppendAllText("D:\\logSH.txt", "stop\r\n");
			if (_isInstalled && _service.CanStop)
			{
				File.AppendAllText("D:\\logSH.txt", "status before: " + _service.Status + "\r\n");
				_service.Stop();
				_service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(50));
				File.AppendAllText("D:\\logSH.txt", "status after: " + _service.Status + "\r\n");
			}
		}
		internal void Restart()
		{
			File.AppendAllText("D:\\logSH.txt", "restart\r\n");
			Stop();
			Thread.Sleep(TimeSpan.FromSeconds(5));
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
