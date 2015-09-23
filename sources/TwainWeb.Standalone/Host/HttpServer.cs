using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using log4net;
using TwainWeb.Standalone.App.Controllers;
using TwainWeb.Standalone.App.Models;
using TwainWeb.Standalone.App.Models.Response;

namespace TwainWeb.Standalone.Host
{
    /// <summary>
    /// http://stackoverflow.com/a/4672704
    /// </summary>
    public class HttpServer : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly Thread _listenerThread;
        private readonly Thread[] _workers;
        private readonly ManualResetEvent _stop, _ready;
        private readonly Queue<HttpListenerContext> _queue;
	    private readonly ILog _logger;

        public HttpServer(int maxThreads)
        {
			_logger = LogManager.GetLogger(typeof(HttpServer));

            _workers = new Thread[maxThreads];

            _queue = new Queue<HttpListenerContext>();
            _stop = new ManualResetEvent(false);
            _ready = new ManualResetEvent(false);
            _listener = new HttpListener();
            _listenerThread = new Thread(HandleRequests);
        }

        public void Start(string prefix)
        {
            _listener.Prefixes.Add(prefix);
            _listener.Start();
            _listenerThread.Start();

            for (int i = 0; i < _workers.Length; i++)
            {
                _workers[i] = new Thread(Worker);
                _workers[i].Start();
            }
        }

        public void Dispose()
        { Stop(); }

        public void Stop()
        {
            _stop.Set();


            _listenerThread.Join();
			_listener.Stop();
            foreach (Thread worker in _workers)
                worker.Join();

          
        }

        private void HandleRequests()
        {
            while (_listener.IsListening)
            {
                var context = _listener.BeginGetContext(ContextReady, null);

                if (0 == WaitHandle.WaitAny(new[] { _stop, context.AsyncWaitHandle }))
                    return;
            }
        }

        private void ContextReady(IAsyncResult ar)
        {
	        try
	        {
		        lock (_queue)
		        {
			        _queue.Enqueue(_listener.EndGetContext(ar));
			        _ready.Set();
		        }
	        }
	        catch { }
        }

        private void Worker()
        {
            WaitHandle[] wait = { _ready, _stop };
            while (0 == WaitHandle.WaitAny(wait))
            {
                HttpListenerContext context;
                lock (_queue)
                {
                    if (_queue.Count > 0)
                        context = _queue.Dequeue();
                    else
                    {
                        _ready.Reset();
                        continue;
                    }
                }

                try { ProcessRequest(context); }
                catch (Exception e)
                {
                    try
                    {
                        var actionResult = new ActionResult { Content = Encoding.UTF8.GetBytes(e.Message), ContentType = "text/plain" };
	                    if (context.Response.OutputStream.CanWrite)
	                    {
		                    context.Response.OutputStream.Write(actionResult.Content, 0, actionResult.Content.Length);		             
	                    }
						_logger.Error(e.ToString());
                    }
                    catch (Exception ex)
                    {
						_logger.Error(ex.ToString());
						
                    }
                }

                try
                {
                    context.Response.OutputStream.Close();
                }
                catch (Exception e)
                {
					//_logger.Error(e.ToString());
                }
            }
        }

        public event Action<HttpListenerContext> ProcessRequest;
    }
}