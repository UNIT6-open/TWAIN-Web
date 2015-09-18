using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Diagnostics;
using System.Reflection;
using System.ServiceProcess;
using System.Text;

namespace TwainWeb.Standalone
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
			if (_isInstalled && _service.Status == ServiceControllerStatus.Stopped)
			{
				_service.Start();
				_service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
			}
		}
		internal void Restart()
		{
			if (_isInstalled && _service.Status == ServiceControllerStatus.Running)
			{
				_service.Stop();
				_service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
			}
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
