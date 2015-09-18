using System;
using System.Threading;
using log4net;

namespace TwainWeb.Standalone.App
{
	public class AsyncWorker<TArg>
	{
		private readonly System.ComponentModel.BackgroundWorker _backgroundWorker;
		private AutoResetEvent _waitHandle;
		private AsyncAction _method;
		private Exception _exception;
		private bool _wasException;
		private ILog _logger;

		public delegate void AsyncAction(TArg argument);

		public AsyncWorker()
		{
			_logger = LogManager.GetLogger(typeof(AsyncWorker<TArg>));
			_backgroundWorker = new System.ComponentModel.BackgroundWorker { WorkerSupportsCancellation = true };
		}
		private void InitializeBackgroundWorker()
		{
			// Attach event handlers to the BackgroundWorker object.
			_backgroundWorker.DoWork += backgroundWorker_DoWork;
			_backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
		}

		public void RunWorkAsync(TArg argument, AsyncAction meth)
		{
			_method = meth;
			_waitHandle = new AutoResetEvent(false);
			InitializeBackgroundWorker();

			_backgroundWorker.RunWorkerAsync(argument);
			_waitHandle.WaitOne();

			if (_wasException)
				throw _exception;
		}

		public void RunWorkAsync(TArg argument, AsyncAction meth, int waitTime)
		{
			_method = meth;
			_waitHandle = new AutoResetEvent(false);
			InitializeBackgroundWorker();

			_backgroundWorker.RunWorkerAsync(argument);
			_waitHandle.WaitOne(waitTime, false);

			if (_backgroundWorker.IsBusy)
			{
				_backgroundWorker.CancelAsync();
				_logger.Error("Превышено время ожидания выполнения асинхронной операции: " + meth.Method.Name);
			}

			_waitHandle.Reset();

			if (_wasException)
				throw _exception;

		}

		private void backgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			var child = new Thread(() =>
			{
				while (!_backgroundWorker.CancellationPending)
				{
					Thread.Sleep(100);
				}
				e.Cancel = true;
				
			});
			child.Start();

			var arg = (TArg)e.Argument;

			// Return the value through the Result property.
			_method(arg);
			_backgroundWorker.CancelAsync();
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

			_waitHandle.Set();

		}
	}
	public class AsyncWorker<TArg, TRes>
	{
		private readonly System.ComponentModel.BackgroundWorker _backgroundWorker;
		private AutoResetEvent _waitHandle;
		private TRes _result;
		private AsyncAction _method;
		private Exception _exception;
		private bool _wasException;
		private ILog _logger;

		public delegate TRes AsyncAction(TArg argument);

		public AsyncWorker()
		{
			_logger = LogManager.GetLogger(typeof(AsyncWorker<TArg>));
			_backgroundWorker = new System.ComponentModel.BackgroundWorker { WorkerSupportsCancellation = true};
		} 
		private void InitializeBackgroundWorker()
		{
			// Attach event handlers to the BackgroundWorker object.
			_backgroundWorker.DoWork += backgroundWorker_DoWork;
			_backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
		}

		public TRes RunWorkAsync(TArg argument, AsyncAction meth)
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

		public TRes RunWorkAsync(TArg argument, AsyncAction meth, int waitTime)
		{
			_method = meth;
			_waitHandle = new AutoResetEvent(false);
			InitializeBackgroundWorker();

			_backgroundWorker.RunWorkerAsync(argument);
			_waitHandle.WaitOne(waitTime, false);

			if (_backgroundWorker.IsBusy)
			{
				_logger.Error("Превышено время ожидания выполнения асинхронной операции: " + meth.Method.Name);
				_backgroundWorker.CancelAsync();
			}

			_waitHandle.Reset();

			if (_wasException)
				throw _exception;

			return _result;
		}

		private void backgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			var child = new Thread(() =>
			{
				while (!_backgroundWorker.CancellationPending)
				{
					Thread.Sleep(100);
				}
				e.Cancel = true;

			});
			child.Start();

			var arg = (TArg)e.Argument;
			
			// Return the value through the Result property.
			e.Result = _method(arg);
			_backgroundWorker.CancelAsync();
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
