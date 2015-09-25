using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Win32;

namespace TwainWeb.ServiceManager
{
	class Program
	{
		static void Main(string[] args)
		{
/*			LoopSubKeysTest();
			Console.ReadKey();*/
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


		public const int Success = 0;
		public const int FileNotFound = 2;
		public const int AccessDenied = 5;
		public const int InvalidParameter = 87;
		public const int MoreData = 234;
		public const int NoMoreEntries = 259;
		public const int MarkedForDeletion = 1018;
		public const int BufferMaxLength = 2048;

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegEnumKeyW")]
		private static extern int RegEnumKeyW(UIntPtr keyBase, int index, StringBuilder nameBuffer, int bufferLength);

		public static UIntPtr HKEY_LOCAL_MACHINE = new UIntPtr(0x80000002u);
		public static UIntPtr HKEY_CURRENT_USER = new UIntPtr(0x80000001u);
		public static List<string> LoopSubKeysTest()
		{
			var productsRoot = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
			var entries = new List<string>();
			var helper = new RegistryHelper();
			var regKeyHandle = helper.GetRegistryKeyHandle(productsRoot);

			var buffer = new StringBuilder(BufferMaxLength);
			for (int index = 0; true; index++)
			{
				int result = RegEnumKeyW(regKeyHandle, index, buffer, buffer.Capacity);

				if (result == Success)
				{

					var productDisplayName = productsRoot + @"\" + buffer;

					var sf = RegistryHelper.ReadRegKey(HKEY_LOCAL_MACHINE, productDisplayName, "DisplayName");

					Console.WriteLine(productDisplayName + ": " + sf);
						buffer.Length = 0;
					

					entries.Add(buffer.ToString());
					buffer.Length = 0;
					continue;
				}

				if (result == NoMoreEntries) { break; }

				throw new ApplicationException("This RegEnumKeyW result is unknown");
			}

			return entries;
		}
	
		public static  string GetUninstallCommandFor(string productDisplayName)
		{

			RegistryKey localMachine = Registry.LocalMachine;
			
			string productsRoot = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData";
			var products = localMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Products");
	
			string[] productFolders = products.GetSubKeyNames();

			foreach (string p in productFolders)
			{
				RegistryKey installProperties = products.OpenSubKey(p + @"\InstallProperties");
				if (installProperties != null)
				{
					string displayName = (string)installProperties.GetValue("DisplayName");
					if ((displayName != null) && (displayName.Contains(productDisplayName)))
					{
						string uninstallCommand = (string)installProperties.GetValue("UninstallString");
						return uninstallCommand;
					}
				}
			}

			return "";

		}
	}
}
