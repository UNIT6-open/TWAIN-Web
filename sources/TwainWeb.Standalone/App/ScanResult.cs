using System.Text;

namespace TwainWeb.Standalone.App
{
    public class ScanResult : DownloadFile
    {
        public byte[] FileContent { get { return _fileContent; } }
        private byte[] _fileContent;        
        public string Error { get; set; }

        public bool Validate()
        {
            return Error == null;
        }

        public void FillContent()
        {
            _fileContent = Encoding.UTF8.GetBytes("{\"file\": \""+FileName+"\", \"temp\": \""+TempFile+"\"}");
        }
    }
}
