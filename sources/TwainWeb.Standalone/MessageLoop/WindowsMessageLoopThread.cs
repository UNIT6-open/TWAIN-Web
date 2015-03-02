using System;
using System.Threading;
using System.Windows.Forms;
using TwainWeb.Standalone.App;

namespace TwainWeb.Standalone.MessageLoop
{
	public class WindowsMessageLoopThread
	{
		private readonly Thread _thread;
		private readonly AutoResetEvent _startedEvent = new AutoResetEvent(false);
		private Form _form;
		private IntPtr _hwnd;

		public IntPtr Hwnd { get { return _hwnd; } }

		public WindowsMessageLoopThread()
		{
			_thread = new Thread(ThreadFunction);
			_thread.Start();
			_startedEvent.WaitOne();
		}

		public void Stop()
		{
			_form.BeginInvoke(new MethodInvoker(_form.Close));
			_thread.Join();
		}

		private void ThreadFunction()
		{
		
			_form = new Form
			{
				WindowState = FormWindowState.Minimized, 
				ShowInTaskbar = false
			};
			_hwnd = _form.Handle;

			_startedEvent.Set();
			Application.Run(_form);

			
		}

		private delegate int GetThreadId();
		private int GetControlThreadId()
		{
			return AppDomain.GetCurrentThreadId();			
		}

		public object Invoke(Delegate action, object[] parameters)
		{
			using (new MessageBoxHook((int)_form.Invoke(new GetThreadId(GetControlThreadId))))
			{
				return _form.Invoke(action, parameters);
			}			
		}
		public object Invoke(Delegate action)
		{
			using (new MessageBoxHook((int)_form.Invoke(new GetThreadId(GetControlThreadId))))
			{
				return _form.Invoke(action);
			}
		}
		public object BeginInvoke(Delegate action, object[] parameters)
		{
			return _form.BeginInvoke(action, parameters);
		}

		public object BeginInvoke(Delegate action)
		{
			return _form.BeginInvoke(action);
		}
	}
}
