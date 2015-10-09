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
		private readonly FileLogger _logger;
		internal ServiceHelper(string servcieName, string serviceFilename)
		{
			_serviceFilename = serviceFilename;
			_serviceName = servcieName;
			_service = new ServiceController(_serviceName);

			_logger = FileLogger.GetLogger("InstallationLog.txt");
			try
			{
				// actually we need to try access ANY of service properties
				// at least once to trigger an exception
				// not neccessarily its name
				_status = _service.Status;
				_isInstalled = true;
			}
			catch (InvalidOperationException)
			{
				_isInstalled = false;
			}
			

		}
		internal bool Install()
		{
			_logger.Info(string.Format("Installation windows service '{0}'...", _serviceName));
			
			if (_isInstalled)
			{
				_logger.Info("Need to uninstall service");
				var success = Uninstall();
				if (!success)
				{
					_logger.Error("Installation failed");
					return false;
				}
			}

			var exeFile = Path.Combine(AssemblyDirectory, _serviceFilename);
			if (!File.Exists(exeFile))
			{
				_logger.Error(string.Format("File '{0}' not found", exeFile));
				MessageBox.Show(string.Format("Не удалось найти исполняемый файл: {0}.", exeFile));
				return false;
			}
			ManagedInstallerClass.InstallHelper(new[] { exeFile });
			SetRecoveryOptions(_serviceName);

			_isInstalled = true;
			_logger.Info(string.Format("Installation of windows service '{0}' completed successfully", _serviceName));
			return true;
		}

		internal bool Uninstall()
		{
			
			if (_isInstalled)
			{
				_logger.Info(string.Format("Uninstallation windows service '{0}'...", _serviceName));

				var stopSuccess = Stop();
				if (!stopSuccess) return false;

				var exeFile = Path.Combine(AssemblyDirectory, _serviceFilename);
				if (!File.Exists(exeFile))
				{
					_logger.Error(string.Format("File '{0}' not found", exeFile));
					MessageBox.Show(string.Format("Не удалось найти исполняемый файл: {0}.", exeFile));
					return false;
				}

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
							return false;
						}
						_service.Refresh();
						_status = _service.Status;
						Thread.Sleep(TimeSpan.FromMilliseconds(500));
					}
					catch (Exception e)
					{
						break;
					}
				}
				_isInstalled = false;
			}
			_logger.Info(string.Format("Windows service '{0}' was successfully uninstalled", _serviceName));
			return true;
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
		internal bool Start()
		{
			
			if (_isInstalled && 
				(_service.Status == ServiceControllerStatus.Stopped || 
				_service.Status == ServiceControllerStatus.StopPending))
			{
				_logger.Info(string.Format("Start service '{0}'...", _serviceName));
				
				try
				{
					_service.Start();
					_service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(50));
					_logger.Info(string.Format("Service '{0}' was started", _serviceName));
					return true;
				}
				catch (Exception)
				{
					_logger.Info(string.Format("Can not to start service '{0}'", _serviceName));
					return false;
				}				
			}
			return false;
		}
		internal bool Stop()
		{
			if (_isInstalled && _service.CanStop)
			{
				_logger.Info(string.Format("Stop service '{0}'...", _serviceName));
				
				try
				{
					_service.Stop();
					_service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(50));
					_logger.Info(string.Format("Service '{0}' was stopped", _serviceName));
					return true;
				}
				catch (Exception)
				{
					_logger.Info(string.Format("Can not to stop service '{0}'", _serviceName));
					return false;
				}						
			}
			return true;
		}
		internal void Restart()
		{
			_logger.Info(string.Format("Restart service '{0}'...", _serviceName));
			Stop();
			Thread.Sleep(TimeSpan.FromSeconds(1));
			Start();
			_logger.Info(string.Format("Service '{0}' was restarted", _serviceName));
		}

		void SetRecoveryOptions(string serviceName)
		{
			_logger.Info(string.Format("Setting recovery options..."));
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
			{
				_logger.Error(string.Format("Program 'sc' terminated with exitCode: {0}", exitCode));
			}
			else
			{
				_logger.Info(string.Format("Recovery options successfully setted"));
			}
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
