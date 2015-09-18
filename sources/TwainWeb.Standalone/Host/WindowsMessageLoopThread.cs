using System;
using System.Threading;
using System.Windows.Forms;
using TwainWeb.Standalone.App;
using TwainWeb.Standalone.App.Hook;

namespace TwainWeb.Standalone.Host
{
	public class WindowsMessageLoopThread
	{
		private Form _form;
		private IntPtr _hwnd;
		private ApplicationContext _threadContext;
		private readonly AutoResetEvent _initComplete = new AutoResetEvent(false);

		public IntPtr Hwnd { get { return _hwnd; } }

		public WindowsMessageLoopThread()
		{
			var messageLoop = new Thread(() =>
			{
				_form = new Form();
				_hwnd = _form.Handle;
				_threadContext = new ApplicationContext();
				_initComplete.Set();
				Application.Run(_threadContext);
			});
			messageLoop.SetApartmentState(ApartmentState.STA);
			messageLoop.Start();
			_initComplete.WaitOne();
		}

		public void Stop()
		{
			_form.BeginInvoke(new MethodInvoker(_form.Close));
			_threadContext.ExitThread();
		}

		private delegate int GetThreadId();


		private int GetControlThreadId()
		{
			return AppDomain.GetCurrentThreadId();			
		}

		public void Invoke(Delegate action, object[] parameters)
		{
			using (new MessageBoxHook((int)_form.Invoke(new GetThreadId(GetControlThreadId))))
			{
				_form.Invoke(action, parameters);
			}
		}

		public T Invoke<T>(Delegate action)
		{
			using (new MessageBoxHook((int)_form.Invoke(new GetThreadId(GetControlThreadId))))
			{					
				return (T)_form.Invoke(action);				
			}
		}

		public T Invoke<T>(Delegate action, object[] parameters)
		{
			using (new MessageBoxHook((int)_form.Invoke(new GetThreadId(GetControlThreadId))))
			{
				return (T)_form.Invoke(action, parameters);
			}			
		}
		/*public object BeginInvoke(Delegate action, object[] parameters)
		{

				return _form.BeginInvoke(action, parameters);
			
		}

		public object BeginInvoke(Delegate action)
		{

				return _form.BeginInvoke(action);
			
		}*/
	}
}
