using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TwainWeb.Standalone.App;
using TwainWeb.Standalone.App.Models.Response;

namespace TWAIN_Web.Tests
{
    [TestClass]
    public class ProcessingAllowedFormatsTests
    {
        [TestMethod]
        public void NearStandartFormat()
        {
            var scannerSettings = new ScannerSettings(0, "test", null, null, 11.69f, 8.27f);
            scannerSettings = new ScannerSettings(0, "test", null, null, 11.73f, 8.31f);
            scannerSettings = new ScannerSettings(0, "test", null, null, 11.66f, 8.23f);
            scannerSettings = new ScannerSettings(0, "test", null, null, 13, 10.23f);
            scannerSettings = new ScannerSettings(0, "test", null, null, 40, 50);
            scannerSettings = new ScannerSettings(0, "test", null, null, 10.23f, 13);
        }
   } 
}
