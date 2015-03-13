using System;
using System.Text;
using TwainWeb.Standalone.App.Commands;
using TwainWeb.Standalone.App.Queries;
using TwainWeb.Standalone.Scanner;

namespace TwainWeb.Standalone.App
{
	public class AjaxMethods
	{
		public AjaxMethods(object markerAsync)
		{
			_markerAsync = markerAsync;
		}

		private readonly object _markerAsync;	

		public ActionResult Scan(ScanForm command, IScannerManager scannerManager)
		{
			var scanResult = new ScanCommand(command, scannerManager).Execute(_markerAsync);

			if (scanResult.Validate())
				return new ActionResult { Content = scanResult.Content, ContentType = "text/json" };
			
			throw new Exception(scanResult.Error);
		}

		public ActionResult GetScannerParameters(IScannerManager scannerManager, CashSettings cashSettings, int? sourceIndex)
		{
			var queryResult = new GetScannerParametersQuery(scannerManager, cashSettings, sourceIndex).Execute(_markerAsync);
			
			if (queryResult.Validate())
				return new ActionResult{Content = Encoding.UTF8.GetBytes(queryResult.Serialize()), ContentType = "text/json"};

			throw new Exception(queryResult.Error);
		}
	}
}