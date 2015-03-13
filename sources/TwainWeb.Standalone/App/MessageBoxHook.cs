using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using log4net;

namespace TwainWeb.Standalone.App
{
	public class MessageBoxHook:IDisposable
	{
		[DllImport("user32.dll")]
		private static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		private static extern int EndDialog(IntPtr hDlg, IntPtr nResult);

		[DllImport("user32.dll", SetLastError = false)]
		public static extern IntPtr GetDlgItem(IntPtr hDlg, int nIDDlgItem);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
		public static extern IntPtr SendMessage(HandleRef hWnd, uint Msg, IntPtr wParam, string lParam);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

		public const uint WM_SETTEXT = 0x000C;
		private const int WM_INITDIALOG = 0x0110;

		private readonly MessageBoxHookManager _manager;

		[ThreadStatic] private static IntPtr _hHook;
		public MessageBoxHook()
		{
			_manager = new MessageBoxHookManager();

			if (_hHook == IntPtr.Zero)
				_hHook =_manager.Register(MessageBoxHookProc);

		}

		public MessageBoxHook(int threadId)
		{
			_manager = new MessageBoxHookManager();

			if (_hHook == IntPtr.Zero)
				_hHook = _manager.Register(MessageBoxHookProc, threadId);

		}
		public void Dispose()
		{
			_manager.Unregister(_hHook);
			_hHook = IntPtr.Zero;
		}

		/// <summary>
		/// Hook for autoclosing dialogs
		/// </summary>
		/// <param name="nCode">The code that the hook procedure uses to determine how to process the message. </param>
		/// <param name="wParam">Depends on the nCode parameter.</param>
		/// <param name="lParam">Depends on the nCode parameter.</param>
		/// <returns>If nCode is less than zero, the hook procedure must return the value returned by CallNextHookEx.</returns>
		private IntPtr MessageBoxHookProc(int nCode, IntPtr wParam, IntPtr lParam)
		{
			if (nCode < 0)
			{
				return CallNextHookEx(_hHook, nCode, wParam, lParam);
			}
			var msg = (MessageBoxHookManager.CWPRETSTRUCT)Marshal.PtrToStructure(lParam, typeof(MessageBoxHookManager.CWPRETSTRUCT));

			if ((msg.message == WM_INITDIALOG))
			{
				var sb = new StringBuilder(260);

				var hDialogText = GetDlgItem(msg.hwnd, 0xFFFF);
				if (hDialogText != IntPtr.Zero)
					GetWindowText(hDialogText, sb, sb.Capacity);

				LogManager.GetLogger(typeof(MessageBoxHookManager)).ErrorFormat("System dialog is blocked: \r\n"+sb);
				EndDialog(msg.hwnd, new IntPtr((int)MessageBoxButtons.OK));
			}
			return CallNextHookEx(_hHook, nCode, wParam, lParam);
		}
	}
}
