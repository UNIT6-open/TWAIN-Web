using System;
using System.Globalization;
using System.Windows.Forms;
using TwainWeb.Standalone.Scanner;


namespace TwainWeb.Standalone
{
    public partial class FormForSetPort : Form
    {
		/// <summary>
		/// Флаг, указывающий на цель запуска конфигуратора. 
		/// Если true, конфигуратор запущен намеренно с целью изменения значений. 
		/// Если false, конфигуратор запущен перед запуском Twain@WEB с целью проверки доступности порта.
		/// </summary>
        private readonly bool _needToChangeSettings;
		private bool _firstShowEvent;

		/// <summary>
		/// Конструктор по умолчанию. Считывает настройки из конфигурационного файла и обновляет интерфейс.
		/// </summary>
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

		/// <summary>
		/// Конструктор с возможностью указать цель запуска конфигуратора
		/// </summary>
		/// <param name="needToChangeSettings">
		/// Флаг, указывающий на цель запуска конфигуратора. 
		/// Если true, конфигуратор запущен намеренно с целью изменения значений. 
		/// Если false, конфигуратор запущен перед запуском Twain@WEB с целью проверки доступности порта.</param>
		public FormForSetPort(bool needToChangeSettings) : this()
        {
			_needToChangeSettings = needToChangeSettings;
            _firstShowEvent = true;
        }

		/// <summary>
		/// Обновление настроек порта для запуска Twain@WEB в интерфейсе.
		/// </summary>
		/// <param name="portNumber">Порт для запуска Twain@WEB.</param>
		private void UpdatePort(int portNumber)
	    {
			port.Text = portNumber.ToString(CultureInfo.InvariantCulture);
	    }

		/// <summary>
		/// Обновление активного менеджера сканеров в интерфейсе.
		/// </summary>
		/// <param name="scannerManager">Активный менеджер сканеров.</param>
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

		/// <summary>
		/// Сохранение выбранного порта в конфигурационный файл.
		/// </summary>
		/// <param name="portNumber">Порт для запуска Twain@WEB.</param>
        private void SavePortSettings(int portNumber)
        {
			if (RuntimeConfigurationManager.UpdateAppSettings("Port", portNumber.ToString(CultureInfo.InvariantCulture)))
				UpdatePort(portNumber);
        }

		/// <summary>
		/// Сохранение выбранного менеджера сканеров в конфигурационный файл.
		/// </summary>
		/// <param name="scannerManager">Выбранных менеджер сканеров.</param>
		private void SaveScannerManagerSettings(ScannerManager scannerManager)
	    {
			if (RuntimeConfigurationManager.UpdateAppSettings("ScannerManager", scannerManager.ToString()))
				UpdateScannerManager(scannerManager);
	    }

		/// <summary>
		/// Обработчик нажатия кнопки Ok (сохранение настроек в конфигурационный файл).
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
			var newScannerManager = ScannerManager.Wia;
			if (radioButtonTwain.Checked)
				newScannerManager = ScannerManager.TwainDotNet;

			SaveScannerManagerSettings(newScannerManager);


            int processedPort;
            try
            {
                processedPort = Convert.ToInt32(port.Text);
            }
            catch (Exception)
            {
                processedPort = 80;
            }
            SavePortSettings(processedPort);
			if (CheckPortAvailability(processedPort))
                Close();
        }

		/// <summary>
		/// Проверка доступности порта.
		/// </summary>
		/// <param name="portNumber">Номер порта.</param>
		/// <returns>True, если порт доступен, и false, если порт занят.</returns>

		private bool CheckPortAvailability(int portNumber)
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

		/// <summary>
		/// Первый запуск конфигуратора: если конфигуратор запущен лишь для того, чтобы проверить доступность порта, 
		/// и порт доступен, закрываем конфигуратор.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private void FormForSetPort_Shown(object sender, EventArgs e)
        {
			if (!_needToChangeSettings && _firstShowEvent)
            {
                _firstShowEvent = false;
				if (CheckPortAvailability(Settings.Default.Port))
                {
                    Close();
                }
            }
        }
    }
}
