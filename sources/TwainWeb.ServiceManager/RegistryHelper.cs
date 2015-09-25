using System;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace TwainWeb.ServiceManager
{
	public class RegistryHelper
	{
		public static UIntPtr HKEY_CURRENT_USER = (UIntPtr)0x80000001;
		public static UIntPtr HKEY_LOCAL_MACHINE = (UIntPtr)0x80000002;
		public static int KEY_QUERY_VALUE = 0x0001;
		public static int KEY_READ = 0x20019;

		public static int KEY_SET_VALUE = 0x0002;
		public static int KEY_CREATE_SUB_KEY = 0x0004;
		public static int KEY_ENUMERATE_SUB_KEYS = 0x0008;
		public static int KEY_WOW64_64KEY = 0x0100;
		public static int KEY_WOW64_32KEY = 0x0200;

		public const int Success = 0;
		public const int FileNotFound = 2;
		public const int AccessDenied = 5;
		public const int InvalidParameter = 87;
		public const int MoreData = 234;
		public const int NoMoreEntries = 259;
		public const int MarkedForDeletion = 1018;

		public const int BufferMaxLength = 2048;

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegOpenKeyExW", SetLastError = true)]
		public static extern int RegOpenKeyExW(UIntPtr hKey, string subKey, uint options, int sam, out UIntPtr phkResult);

		[DllImport("advapi32.dll", CharSet = CharSet.Auto)]
		public static extern int RegOpenKeyEx(
		  UIntPtr hKey,
		  string subKey,
		  int ulOptions,
		  int samDesired,
		  out UIntPtr hkResult);
		[DllImport("advapi32.dll", SetLastError = true)]
		public static extern int RegCloseKey(
			UIntPtr hKey);

		// Alternate definition - more correct
		[DllImport("advapi32.dll", SetLastError = true)]
		static extern uint RegQueryValueEx(
			UIntPtr hKey,
			string lpValueName,
			IntPtr lpReserved,
			out RegistryValueKind lpType,
			StringBuilder lpData,
			ref int lpcbData);
		public static string ReadRegKey(UIntPtr rootKey, string keyPath, string valueName)
		{
			UIntPtr hKey;
			if (RegOpenKeyEx(rootKey, keyPath, 0, KEY_READ, out hKey) == 0)
			{
				int size = 1024;
				RegistryValueKind type;
				string keyValue = null;
				var keyBuffer = new StringBuilder((int)size);

				if (RegQueryValueEx(hKey, valueName, IntPtr.Zero, out type, keyBuffer, ref size) == 0)
					keyValue = keyBuffer.ToString();

				RegCloseKey(hKey);

				return (keyValue);
			}

			return (null);  // Return null if the value could not be read
		}


		/// <summary>
		///Try to find the regsitrykey in the 64 bit part of the registry.
		///If not found, try to find the registrykey in the 32 bit part of the registry.
		/// </summary>
		/// <param name="regKeyPath"></param>
		/// <returns>A registrykeyhandle</returns>
		public UIntPtr GetRegistryKeyHandle(string regKeyPath)
		{
			UIntPtr regKeyHandle;

			// Check parameters
			if (string.IsNullOrEmpty(regKeyPath))
			{
				throw new ArgumentNullException("regKeyPath", "GetRegistryKeyHandle: regKeyPath is null or empty.");
			}

			// KEY_WOW64_64KEY
			// Access a 64-bit key from either a 32-bit or 64-bit application (not supported on Windows 2000).
			// 64-bit key = all keys in HKEY_LOCAL_MACHINE\Software except the HKEY_LOCAL_MACHINE\Software\Wow6432Node
			//
			// Check if the registrykey can be found in the 64 bit registry part of the register
			if (RegOpenKeyExW(HKEY_LOCAL_MACHINE, regKeyPath, 0, KEY_READ | KEY_WOW64_64KEY, out regKeyHandle) != Success)
			{
				// KEY_WOW64_32KEY
				// Access a 32-bit key from either a 32-bit or 64-bit application. (not supported on Windows 2000)
				// 32-bit key = all keys in HKEY_LOCAL_MACHINE\Software\Wow6432Node
				//
				// Check if the registrykey can be found in the 32 bit registry part of the register
				if (RegOpenKeyExW(HKEY_LOCAL_MACHINE, regKeyPath, 0, KEY_READ | KEY_WOW64_32KEY, out regKeyHandle) != Success)
				{
					throw new ApplicationException(string.Format(@"GetRegistryKeyHandle: Could not find regstrykey [{0}\{1}]",
						Registry.LocalMachine, regKeyPath));
				}
			}

			return regKeyHandle;
		}
	}
}
