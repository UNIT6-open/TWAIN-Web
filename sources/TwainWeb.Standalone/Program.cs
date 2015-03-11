using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Windows.Forms;


namespace TwainWeb.Standalone
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>     
        ///            

        static void Main(string[] args)
        {
            if (args.Length == 0)
                ServiceBase.Run(new ScanService(Settings.Default.Port));
            else
            {
                if (args[0] == "config")
                {
                    var proc = new ProcessStartInfo();
                    proc.UseShellExecute = true;
                    proc.WorkingDirectory = Environment.CurrentDirectory;
                    proc.FileName = Application.ExecutablePath;
                    proc.Verb = "runas";
                    proc.Arguments = "configrun 2";
                    try
                    {
                        Process.Start(proc);
                    }
                    catch (Exception)
                    {
                    }

                }
                else if (args.Length == 2 && args[0] == "configrun")
                {
                    int isSetParameters;
                    int.TryParse(args[1], out isSetParameters);
                    if (isSetParameters == 2)
                        Process.Start(Environment.CurrentDirectory + "/Files/bat/stop.bat").WaitForExit();
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);                    
                    var mainForm = new FormForSetPort((isSetParameters == 1 || isSetParameters == 2));                    
                    Application.Run(mainForm);   
                    if(isSetParameters == 2)
                        Process.Start(Environment.CurrentDirectory + "/Files/bat/start.bat");
                }
                else if (args[0] == "run")
                    Process.Start("http://127.0.0.1:" + Settings.Default.Port + "/TWAIN@Web/");
            }         
        }        
    }
}
