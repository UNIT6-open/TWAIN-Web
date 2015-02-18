using System;
using System.Threading;

namespace TwainWeb.Standalone.App
{
	public class AsyncWorker<TArg, TRes>
	{
		private readonly System.ComponentModel.BackgroundWorker _backgroundWorker = new System.ComponentModel.BackgroundWorker();
		private AutoResetEvent _waitHandle;
		private TRes _result;
		private AsyncAction _method;
		private Exception _exception;
		private bool _wasException;

		public delegate TRes AsyncAction(TArg argument);
		private void InitializeBackgroundWorker()
		{
			// Attach event handlers to the BackgroundWorker object.
			_backgroundWorker.DoWork += backgroundWorker_DoWork;
			_backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
		}

		public TRes RunWorkAsync(TArg argument, string nameThread, AsyncAction meth)
		{
			_method = meth;
			_waitHandle = new AutoResetEvent(false);
			InitializeBackgroundWorker();

			_backgroundWorker.RunWorkerAsync(argument);
			_waitHandle.WaitOne();

			if (_wasException)
				throw _exception;

			return _result;
		}

		public TRes RunWorkAsync(TArg argument, string nameThread, AsyncAction meth, int waitTime)
		{
			_method = meth;
			_waitHandle = new AutoResetEvent(false);
			InitializeBackgroundWorker();

			_backgroundWorker.RunWorkerAsync(argument);
			_waitHandle.WaitOne(waitTime);

			_waitHandle.Reset();

			if (_wasException)
				throw _exception;

			return _result;
		}

		private void backgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			var arg = (TArg)e.Argument;
			
			// Return the value through the Result property.
			e.Result = _method(arg);
			
		}

		private void backgroundWorker_RunWorkerCompleted(
			object sender,
			System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			// Access the result through the Result property. 
			if (e.Error != null)
			{
				_wasException = true;
				_exception = e.Error;
			}
			else
				_result = (TRes) e.Result;

			_waitHandle.Set();

		}
	}
}
