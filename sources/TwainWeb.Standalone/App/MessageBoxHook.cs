using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TwainWeb.Standalone.App
{
	public class MessageBoxHook:IDisposable
	{
		[DllImport("user32.dll")]
		private static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		private static extern int EndDialog(IntPtr hDlg, IntPtr nResult);

		private const int WM_INITDIALOG = 0x0110;
		private readonly MessageBoxHookManager _manager;
		[ThreadStatic] private static IntPtr _hHook;
		public MessageBoxHook()
		{
			_manager = new MessageBoxHookManager();

			if (_hHook == IntPtr.Zero)
				_hHook =_manager.Register(MessageBoxHookProc);

		}

		public void Dispose()
		{
			_manager.Unregister(_hHook);
		}

		/// <summary>
		/// Hook for autoclosing dialogs
		/// </summary>
		/// <param name="nCode">The code that the hook procedure uses to determine how to process the message. </param>
		/// <param name="wParam">Depends on the nCode parameter. For details, see the following Remarks section.</param>
		/// <param name="lParam">Depends on the nCode parameter. For details, see the following Remarks section.</param>
		/// <returns>If nCode is less than zero, the hook procedure must return the value returned by CallNextHookEx. If nCode is greater than or equal to zero, it is highly recommended that you call CallNextHookEx and return the value it returns; otherwise, other applications that have installed WH_CALLWNDPROCRET hooks will not receive hook notifications and may behave incorrectly as a result. If the hook procedure does not call CallNextHookEx, the return value should be zero.</returns>
		private IntPtr MessageBoxHookProc(int nCode, IntPtr wParam, IntPtr lParam)
		{
			if (nCode < 0)
			{
				return CallNextHookEx(_hHook, nCode, wParam, lParam);
			}
			var msg = (MessageBoxHookManager.CWPRETSTRUCT)Marshal.PtrToStructure(lParam, typeof(MessageBoxHookManager.CWPRETSTRUCT));

			if ((msg.message == WM_INITDIALOG))
			{
				//todo: write to log
				EndDialog(msg.hwnd, new IntPtr((int)MessageBoxButtons.OK));
			}
			return CallNextHookEx(_hHook, nCode, wParam, lParam);
		}
	}
}
