using System;
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
			{
				return new ActionResult { Content = scanResult.Content, ContentType = "text/json" };
			}

			throw new Exception(scanResult.Error);
		}

		public ActionResult GetScannerParameters(IScannerManager scannerManager, CashSettings cashSettings, int? sourceIndex)
		{
			var parameters = new GetScannerParametersQuery(scannerManager, cashSettings, sourceIndex).Execute(_markerAsync);
			
			return parameters;
		}
	}
}