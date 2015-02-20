using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceProcess;
using TwainWeb.Standalone.App;
using TwainWeb.Standalone.Scanner;
using TwainWeb.Standalone.Twain;

namespace TwainWeb.Standalone
{
    public class ScanService : ServiceBase
    {
        public MyError CheckServer()
        {
            var startResult = StartServer();
            if (startResult != null)
                return startResult;
            for (var i = 0; i < 100; i++)
            {
                if (StopServer() == null)
                    break;
            }
            return null;
        }


        private MyError StartServer()
        {
            try
            {
                httpServer = new HttpServer(10);
                httpServer.ProcessRequest += httpServer_ProcessRequest;
                cashSettings = new CashSettings();           
                httpServer.Start("http://+:" + _port + "/TWAIN@Web/");
            }
            catch (Exception ex)
            {
                return new MyError { Code = ((System.Net.HttpListenerException)ex).ErrorCode, Text = ex.ToString() };
            }
            return null;
        }
        private string StopServer()
        {
            try
            {
                
                httpServer.Stop();
            }
            catch (Exception ex) { return ex.Message; }
            return null;
        }
        
	    private readonly IScannerManager _scannerManager;
        public ScanService(int port)
        {
            _port = port;
            ServiceName = "TWAIN@Web";

	        var smFactory = new ScannerManagerFactory();
	        _scannerManager = smFactory.GetScannerManager();
        }

        private HttpServer httpServer;
        private CashSettings cashSettings;
        protected override void OnStart(string[] args)
        {
            StartServer();
        }
        private readonly object _markerAsynchrone = new object();
        private readonly int _port;
        void httpServer_ProcessRequest(System.Net.HttpListenerContext ctx)
        {
            ActionResult actionResult;           
            if (ctx.Request.HttpMethod == "POST")
            {
                var scanFormModelBinder = new ModelBinder(GetPostData(ctx.Request));
                if (ctx.Request.Url.AbsolutePath.Length > 11 && ctx.Request.Url.AbsolutePath.Substring(11) == "ajax")
                {
                    var method = scanFormModelBinder.BindAjaxMethod();
                    var ajaxMethods = new AjaxMethods(_markerAsynchrone);
                    switch (method)
                    {
                        case "GetScannerParameters":
                            actionResult = ajaxMethods.GetScannerParameters(_scannerManager, cashSettings, scanFormModelBinder.BindSourceIndex());
                            break;
                        case "Scan":
							actionResult = ajaxMethods.Scan(scanFormModelBinder.BindScanForm(), _scannerManager);
                            break;
                        default: 
                            actionResult = new ActionResult { Content = new byte[0] };
                            ctx.Response.Redirect("/TWAIN@Web/");
                            break;
                    }                    
                }
                else
                {
                    actionResult = new ActionResult { Content = new byte[0] };
                    ctx.Response.Redirect("/TWAIN@Web/");
                }
            }
            else if (ctx.Request.Url.AbsolutePath.Length < 11)
            {
                actionResult = new ActionResult { Content = new byte[0] };
                ctx.Response.Redirect("/TWAIN@Web/");
            }
            else
            {
                 var contr = new HomeController();
                var requestParameter = ctx.Request.Url.AbsolutePath.Substring(11);
                if (requestParameter != "download")
                {
                    // /twain@web/ — это 11 символов, а дальше — имя файла                  
                    if (requestParameter == "")
                        requestParameter = "index.html";

                    actionResult = contr.StaticFile(requestParameter);
                }
                else
                {
                    var fileParam = new ModelBinder(GetGetData(ctx.Request)).BindDownloadFile();
                    actionResult = contr.DownloadFile(fileParam);
                }
            }

            if (actionResult.FileNameToDownload != null)
                ctx.Response.AddHeader("Content-Disposition", "attachment; filename*=UTF-8''" + Uri.EscapeDataString(Uri.UnescapeDataString(actionResult.FileNameToDownload)));

            if (actionResult.ContentType != null)
                ctx.Response.ContentType = actionResult.ContentType;

            try
            {
                ctx.Response.OutputStream.Write(actionResult.Content, 0, actionResult.Content.Length);
            }
            catch (Exception)
            {

            }
        }

        private Dictionary<string, string> GetGetData(System.Net.HttpListenerRequest request)
        {
            var getData = new Dictionary<string, string>();
            var getDataString = request.RawUrl.Substring(request.RawUrl.IndexOf("?")+1);
            parseQueriString(getDataString, ref getData);
            return getData;
        }

        private Dictionary<string, string> GetPostData(System.Net.HttpListenerRequest request)
        {
            var postData = new Dictionary<string, string>();
            using (var reader = new StreamReader(request.InputStream))
            {
                var postedData = reader.ReadToEnd();
                parseQueriString(postedData, ref postData);
            }

            return postData;
        }

        private void parseQueriString(string query, ref Dictionary<string, string> data)
        {
            foreach (var item in query.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var tokens = item.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length < 2)
                {
                    continue;
                }
                var paramName = tokens[0];
                var paramValue = Uri.UnescapeDataString(tokens[1]);
                data.Add(paramName, paramValue);
            }
        }

        protected override void OnStop()
        {
            StopServer();
        }
    }

}
