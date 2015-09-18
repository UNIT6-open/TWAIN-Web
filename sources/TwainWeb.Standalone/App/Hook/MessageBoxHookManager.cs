using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TwainWeb.Standalone.App.Hook
{
	/// <summary>
	/// Class for inject hooks
	/// </summary>
	public class MessageBoxHookManager
	{
		public delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

		//private delegate bool EnumChildProc(IntPtr hWnd, IntPtr lParam);

		private const int WH_CALLWNDPROCRET = 12;

		[DllImport("user32.dll")]
		private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

		[DllImport("user32.dll")]
		private static extern int UnhookWindowsHookEx(IntPtr idHook);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr GetModuleHandle(string lpModuleName); 

		[StructLayout(LayoutKind.Sequential)]
		public struct CWPRETSTRUCT
		{
			public IntPtr lResult;
			public IntPtr lParam;
			public IntPtr wParam;
			public uint message;
			public IntPtr hwnd;
		};

		/// <summary>
		/// Enables MessageBoxManager functionality
		/// </summary>
		/// <remarks>
		/// MessageBoxManager functionality is enabled on current thread only.
		/// Each thread that needs MessageBoxManager functionality has to call this method.
		/// </remarks>
		public IntPtr Register(HookProc hookProc)
		{
			var hookId = SetWindowsHookEx(WH_CALLWNDPROCRET, hookProc, IntPtr.Zero, AppDomain.GetCurrentThreadId());
			Debug.WriteLine(string.Format("Message hook registered for thread id = {0}; hookId = {1}", AppDomain.GetCurrentThreadId(), hookId));			
			return hookId;			
		}

		/// <summary>
		/// Enables MessageBoxManager functionality
		/// </summary>
		/// <param name="hookProc">Function for hook handling</param>
		/// <param name="threadId">Thread identifier</param>
		/// <remarks>
		/// MessageBoxManager functionality is enabled on thread with <see cref="threadId"/>.
		/// Each thread that needs MessageBoxManager functionality has to call this method.
		/// </remarks>
		public IntPtr Register(HookProc hookProc, int threadId)
		{
			var hookId =SetWindowsHookEx(WH_CALLWNDPROCRET, hookProc, IntPtr.Zero, threadId);
			Debug.WriteLine(string.Format("Message hook registered for thread id = {0}; hookId = {1}", threadId, hookId));
			return hookId;
		}

		/// <summary>
		/// Disables MessageBoxManager functionality
		/// </summary>
		/// <remarks>
		/// Disables MessageBoxManager functionality on current thread only.
		/// </remarks>
		public void Unregister(IntPtr hHook)
		{		
			if (hHook != IntPtr.Zero)
			{
				UnhookWindowsHookEx(hHook);
				Debug.WriteLine(string.Format("Message hook unregistered, id = {0}", hHook));
			}
		}
	}
}
