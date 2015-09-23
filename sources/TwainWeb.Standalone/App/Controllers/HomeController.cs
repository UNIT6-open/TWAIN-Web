using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using TwainWeb.Standalone.App.Models;
using TwainWeb.Standalone.App.Models.Request;
using TwainWeb.Standalone.App.Models.Response;

namespace TwainWeb.Standalone.App.Controllers
{
	public class HomeController
    {
        private readonly Dictionary<string, string> _mimeTypes = new Dictionary<string, string>
        {
            {"html", "text/html"},
            {"js", "application/javascript"},
            {"css", "text/css"},
            {"png", "image/png"},
            {"jpg", "image/jpeg"},
            {"bmp", "image/bmp"},            
            {"gif", "image/gif"},
            {"pdf","application/pdf"},
            {"zip","application/zip"},
			{"ico", "image/x-icon"}
        };

        private static readonly string FilesLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files");

        public ActionResult StaticFile(string requestFileName)
        {            
            string mimeType = null;
            var extension = Path.GetExtension(requestFileName);
            if (!string.IsNullOrEmpty(extension))
                mimeType = _mimeTypes[extension.Substring(1).ToLower()];
            requestFileName = Path.Combine(FilesLocation, requestFileName);
            byte[] response;
            if (File.Exists(requestFileName))
                response = File.ReadAllBytes(requestFileName);
            else
                response = Encoding.UTF8.GetBytes("Not found");

            return new ActionResult { Content = response, ContentType = mimeType };
        }

        public ActionResult DownloadFile(DownloadFileParam fileParam)
        {   
            foreach(var fileCheck in fileParam.ListFiles)
            {
                if (!GlobalDictionaries.Scans.Exists(x => x == fileCheck.TempFile))
                    throw new Exception("Не найден файл для скачивания!");
            }
            var tempDir = Path.GetTempPath();
            byte[] response;
            var fileId = fileParam.ListFiles[0].TempFile;
            var fileName = fileParam.ListFiles[0].FileName;
            string mimeType = null;
            
	        if (fileParam.SaveAs == (int) GlobalDictionaries.SaveAsValues.Pdf)
	        {
		        fileId = MakePdf(fileParam.ListFiles);
		        mimeType = _mimeTypes["pdf"];
				fileName = Path.ChangeExtension(fileName, ".pdf");
	        }
	        else
	        {
				var extension = Path.GetExtension(fileName);
				if (!string.IsNullOrEmpty(extension))
					mimeType = _mimeTypes[extension.Substring(1).ToLower()];
	        }
            var file = Path.Combine(tempDir, fileId);
            if (File.Exists(file))
            {
                response = File.ReadAllBytes(file);
                File.Delete(file);
                GlobalDictionaries.Scans.Remove(fileId);
                foreach (var img in fileParam.ListFiles)
                {
                    if (File.Exists(tempDir + img.TempFile))
                    {
						File.Delete(tempDir + img.TempFile);
                        GlobalDictionaries.Scans.Remove(img.TempFile);
                    }
                }
            }
            else
                response = Encoding.UTF8.GetBytes("Not found");
            return new ActionResult { Content = response, ContentType = mimeType, FileNameToDownload = fileName };
        }
        private string MakePdf(IEnumerable<DownloadFile> listPictures)
        {
            try
            {
                using (var document = new PdfDocument())
                {
                    document.Info.Creator = "TWAIN@Web 1.3beta (http://unit6.ru/twain-web)";
                    var dir = Path.GetTempPath();
                    foreach (var image in listPictures)
                    {
                        // Create an empty page
                        var page = document.AddPage();
                        // Get an XGraphics object for drawing
                        var gfx = XGraphics.FromPdfPage(page);
                        var ximage = XImage.FromFile(dir + image.TempFile);
                        page.Height = ximage.PointHeight;                        
                        page.Width = ximage.PointWidth;
                        gfx.DrawImage(ximage, 0, 0);
                        ximage.Dispose();
                    }
                    var file = Path.GetTempFileName();
                    document.Save(file);
                    document.Close();
                    var fileId = Path.GetFileName(file);
                    GlobalDictionaries.Scans.Add(fileId);
                    return fileId;
                }                
            }
            catch (Exception ex) { return null; }
        }
    }
}
