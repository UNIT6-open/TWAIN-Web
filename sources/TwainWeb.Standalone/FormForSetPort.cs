using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using TwainWeb.Standalone.Properties;


namespace TwainWeb.Standalone
{
    public partial class FormForSetPort : Form
    {
        private bool isSetParameters;
        private bool firstShowEvent;
        public FormForSetPort()
        {            
            InitializeComponent();
        }

        public FormForSetPort(bool isSetParameters)
            : this()
        {
            this.isSetParameters = isSetParameters;
            this.firstShowEvent = true;
        }

        
        private void SetPort(int port)
        {
            var configDoc = new XmlDocument();
            configDoc.Load("TwainWeb.Standalone.exe.config");
            var configurationNode = configDoc.SelectSingleNode("configuration");
            if (configurationNode == null)
                return;
            for (int i = 0; i < configurationNode.ChildNodes.Count; i++)
            {
                if (configurationNode.ChildNodes[i].Name == "applicationSettings")
                {
                    for (int j = 0; j < configurationNode.ChildNodes[i].ChildNodes.Count; j++)
                    {
                        if (configurationNode.ChildNodes[i].ChildNodes[j].Name == "TwainWeb.Standalone.Properties.Settings")
                        {
                            for (int k = 0; k < configurationNode.ChildNodes[i].ChildNodes[j].ChildNodes.Count; k++)
                            {
                                if (configurationNode.ChildNodes[i].ChildNodes[j].ChildNodes[k].Name == "setting")
                                {
                                    if (configurationNode.ChildNodes[i].ChildNodes[j].ChildNodes[k].Attributes["name"] != null && configurationNode.ChildNodes[i].ChildNodes[j].ChildNodes[k].Attributes["name"].Value == "port")
                                    {
                                        for (int l = 0; l < configurationNode.ChildNodes[i].ChildNodes[j].ChildNodes[k].ChildNodes.Count; l++)
                                        {
                                            if (configurationNode.ChildNodes[i].ChildNodes[j].ChildNodes[k].ChildNodes[l].Name == "value")
                                            {
                                                configurationNode.ChildNodes[i].ChildNodes[j].ChildNodes[k].ChildNodes[l].InnerText = port.ToString();
                                                this.port.Text = port.ToString();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            configDoc.Save("TwainWeb.Standalone.exe.config");
        }

        private void button1_Click(object sender, EventArgs e)
        {            
            int processedPort;
            try
            {
                processedPort = Convert.ToInt32(port.Text);
            }
            catch (Exception ex)
            {
                processedPort = 80;
            }
            this.SetPort(processedPort);
            if(this.processCheckServer(processedPort))
                this.Close();
        }

        private bool processCheckServer(int port)
        {
            var scanService = new ScanService(port);
            var resultCheckServer = scanService.CheckServer();
            if (resultCheckServer != null)
            {
                string error;
                if (resultCheckServer.Code == 32)
                   error = "Порт " + port + " занят другим процессом. ";                    
                else
                    error = "Не предусмотренная ошибка. Отправьте это сообщение разработчикам разработчикам." + Environment.NewLine + Environment.NewLine + resultCheckServer.Text + Environment.NewLine + Environment.NewLine;
                error += "Попробуйте изменить или освободить порт.";
                MessageBox.Show(error);
                
                return false;
            }
            return true;
        }

        private void FormForSetPort_Shown(object sender, EventArgs e)
        {
            if (!isSetParameters && firstShowEvent)
            {
                firstShowEvent = false;
                if (processCheckServer(Settings.Default.Port))
                {
                    this.Close();
                }
            }
        }
    }
}
