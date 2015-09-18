namespace TwainWeb.Standalone.Configurator
{
    partial class FormForSetPort
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.fontDialog1 = new System.Windows.Forms.FontDialog();
			this.button1 = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.radioButtonTwain = new System.Windows.Forms.RadioButton();
			this.radioButtonWia = new System.Windows.Forms.RadioButton();
			this.port = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(439, 130);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 3;
			this.button1.Text = "OK";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.port);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Location = new System.Drawing.Point(12, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(252, 112);
			this.groupBox1.TabIndex = 4;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Сетевые настройки";
			// 
			// radioButtonTwain
			// 
			this.radioButtonTwain.AutoSize = true;
			this.radioButtonTwain.Location = new System.Drawing.Point(6, 62);
			this.radioButtonTwain.Name = "radioButtonTwain";
			this.radioButtonTwain.Size = new System.Drawing.Size(61, 17);
			this.radioButtonTwain.TabIndex = 7;
			this.radioButtonTwain.TabStop = true;
			this.radioButtonTwain.Text = "TWAIN";
			this.radioButtonTwain.UseVisualStyleBackColor = true;
			// 
			// radioButtonWia
			// 
			this.radioButtonWia.AutoSize = true;
			this.radioButtonWia.Location = new System.Drawing.Point(6, 39);
			this.radioButtonWia.Name = "radioButtonWia";
			this.radioButtonWia.Size = new System.Drawing.Size(46, 17);
			this.radioButtonWia.TabIndex = 6;
			this.radioButtonWia.TabStop = true;
			this.radioButtonWia.Text = "WIA";
			this.radioButtonWia.UseVisualStyleBackColor = true;
			// 
			// port
			// 
			this.port.Location = new System.Drawing.Point(91, 37);
			this.port.Name = "port";
			this.port.Size = new System.Drawing.Size(88, 20);
			this.port.TabIndex = 4;
			this.port.Text = "80";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(7, 40);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(35, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Порт:";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.radioButtonTwain);
			this.groupBox2.Controls.Add(this.radioButtonWia);
			this.groupBox2.Location = new System.Drawing.Point(270, 13);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(244, 111);
			this.groupBox2.TabIndex = 5;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Драйвер";
			// 
			// FormForSetPort
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(526, 165);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.button1);
			this.Name = "FormForSetPort";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Конфигурация TWAIN@Web";
			this.Shown += new System.EventHandler(this.FormForSetPort_Shown);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FontDialog fontDialog1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox port;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.RadioButton radioButtonTwain;
		private System.Windows.Forms.RadioButton radioButtonWia;
		private System.Windows.Forms.GroupBox groupBox2;

    }
}