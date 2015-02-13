using System;
using System.Runtime.InteropServices;

namespace TwainWeb.Standalone.App
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
			return SetWindowsHookEx(WH_CALLWNDPROCRET, hookProc, IntPtr.Zero, AppDomain.GetCurrentThreadId());
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
			}
		}
	}
}
