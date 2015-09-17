using System;
using System.Globalization;
using System.Windows.Forms;
using TwainWeb.Standalone.Scanner;


namespace TwainWeb.Standalone
{
    public partial class FormForSetPort : Form
    {
        private readonly bool _isSetParameters;
        private bool _firstShowEvent;
        public FormForSetPort()
        {            
            InitializeComponent();
			UpdatePort(Settings.Default.Port);

			var scannerManagerSetting = Settings.Default.ScannerManager;
			ScannerManager scannerManager;

			try
			{
				scannerManager = (ScannerManager)Enum.Parse(typeof(ScannerManager), scannerManagerSetting, true);
			}
			catch (Exception)
			{
				scannerManager = ScannerManager.Wia;
			}

			UpdateScannerManager(scannerManager);
        }

        public FormForSetPort(bool isSetParameters)
            : this()
        {
            _isSetParameters = isSetParameters;
            _firstShowEvent = true;
        }

		private void UpdatePort(int portNumber)
	    {
			port.Text = portNumber.ToString(CultureInfo.InvariantCulture);
	    }

	    private void UpdateScannerManager(ScannerManager scannerManager)
	    {
			switch (scannerManager)
			{
				case ScannerManager.TwainDotNet:
					radioButtonTwain.Select();
					break;
				default:
					radioButtonWia.Select();
					break;
			}
	    }

        private void UpdatePortSettings(int portNumber)
        {
			if (RuntimeConfigurationManager.UpdateAppSettings("Port", portNumber.ToString(CultureInfo.InvariantCulture)))
				UpdatePort(portNumber);
        }

		private void UpdateScannerManagerSettings(ScannerManager scannerManager)
	    {
			if (RuntimeConfigurationManager.UpdateAppSettings("ScannerManager", scannerManager.ToString()))
				UpdateScannerManager(scannerManager);
	    }

        private void button1_Click(object sender, EventArgs e)
        {
			var newScannerManager = ScannerManager.Wia;
			if (radioButtonTwain.Checked)
				newScannerManager = ScannerManager.TwainDotNet;

			UpdateScannerManagerSettings(newScannerManager);


            int processedPort;
            try
            {
                processedPort = Convert.ToInt32(port.Text);
            }
            catch (Exception)
            {
                processedPort = 80;
            }
            UpdatePortSettings(processedPort);
            if(processCheckServer(processedPort))
                Close();
        }

        private bool processCheckServer(int portNumber)
        {
			var scanService = new ScanService(portNumber);
            var resultCheckServer = scanService.CheckServer();
            if (resultCheckServer != null)
            {
                string error;
                if (resultCheckServer.Code == 32)
					error = "Порт " + portNumber + " занят другим процессом. ";                    
                else
                    error = "Непредусмотренная ошибка. Отправьте это сообщение разработчикам." + Environment.NewLine + Environment.NewLine + resultCheckServer.Text + Environment.NewLine + Environment.NewLine;
                error += "Попробуйте изменить или освободить порт.";
                MessageBox.Show(error);
                
                return false;
            }
            return true;
        }

        private void FormForSetPort_Shown(object sender, EventArgs e)
        {
            if (!_isSetParameters && _firstShowEvent)
            {
                _firstShowEvent = false;
                if (processCheckServer(Settings.Default.Port))
                {
                    Close();
                }
            }
        }
    }
}
