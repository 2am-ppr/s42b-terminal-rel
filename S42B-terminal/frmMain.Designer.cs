namespace S42B_terminal
{
	partial class frmMain
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.cmbCommand = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.cmbValue = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.cmbSerialDriver = new System.Windows.Forms.ComboBox();
			this.btnConnect = new System.Windows.Forms.Button();
			this.cmbBaudDriver = new System.Windows.Forms.ComboBox();
			this.btnSend = new System.Windows.Forms.Button();
			this.lblLimits = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.tmrDumpLog = new System.Windows.Forms.Timer(this.components);
			this.label5 = new System.Windows.Forms.Label();
			this.cmbBaudMarlin = new System.Windows.Forms.ComboBox();
			this.btnConnectMarlin = new System.Windows.Forms.Button();
			this.cmbSerialMarlin = new System.Windows.Forms.ComboBox();
			this.txtGcodeTest = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.btnPidTest = new System.Windows.Forms.Button();
			this.cmbPidDivisor = new System.Windows.Forms.ComboBox();
			this.label7 = new System.Windows.Forms.Label();
			this.txtMarlinResponse = new System.Windows.Forms.TextBox();
			this.btnDiscDriver = new System.Windows.Forms.Button();
			this.btnDiscMarlin = new System.Windows.Forms.Button();
			this.txtGcodePrep = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabLog = new System.Windows.Forms.TabPage();
			this.tabControl1.SuspendLayout();
			this.tabLog.SuspendLayout();
			this.SuspendLayout();
			// 
			// textBox1
			// 
			this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBox1.Location = new System.Drawing.Point(0, 0);
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(881, 297);
			this.textBox1.TabIndex = 0;
			// 
			// cmbCommand
			// 
			this.cmbCommand.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cmbCommand.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbCommand.FormattingEnabled = true;
			this.cmbCommand.Location = new System.Drawing.Point(77, 37);
			this.cmbCommand.Name = "cmbCommand";
			this.cmbCommand.Size = new System.Drawing.Size(439, 21);
			this.cmbCommand.TabIndex = 1;
			this.cmbCommand.SelectionChangeCommitted += new System.EventHandler(this.cmbCommand_SelectionChangeCommitted);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 40);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(54, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Command";
			// 
			// cmbValue
			// 
			this.cmbValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmbValue.FormattingEnabled = true;
			this.cmbValue.Location = new System.Drawing.Point(562, 37);
			this.cmbValue.Name = "cmbValue";
			this.cmbValue.Size = new System.Drawing.Size(121, 21);
			this.cmbValue.TabIndex = 3;
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(522, 40);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(34, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "Value";
			// 
			// cmbSerialDriver
			// 
			this.cmbSerialDriver.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cmbSerialDriver.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbSerialDriver.FormattingEnabled = true;
			this.cmbSerialDriver.Location = new System.Drawing.Point(77, 10);
			this.cmbSerialDriver.Name = "cmbSerialDriver";
			this.cmbSerialDriver.Size = new System.Drawing.Size(439, 21);
			this.cmbSerialDriver.TabIndex = 5;
			this.cmbSerialDriver.SelectionChangeCommitted += new System.EventHandler(this.cmbSerialDriver_SelectionChangeCommitted);
			// 
			// btnConnect
			// 
			this.btnConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnConnect.Location = new System.Drawing.Point(826, 8);
			this.btnConnect.Name = "btnConnect";
			this.btnConnect.Size = new System.Drawing.Size(75, 23);
			this.btnConnect.TabIndex = 6;
			this.btnConnect.Text = "Connect";
			this.btnConnect.UseVisualStyleBackColor = true;
			this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
			// 
			// cmbBaudDriver
			// 
			this.cmbBaudDriver.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmbBaudDriver.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbBaudDriver.FormattingEnabled = true;
			this.cmbBaudDriver.Items.AddRange(new object[] {
            "19200",
            "38400",
            "115200",
            "2000000"});
			this.cmbBaudDriver.Location = new System.Drawing.Point(588, 10);
			this.cmbBaudDriver.Name = "cmbBaudDriver";
			this.cmbBaudDriver.Size = new System.Drawing.Size(95, 21);
			this.cmbBaudDriver.TabIndex = 7;
			this.cmbBaudDriver.SelectionChangeCommitted += new System.EventHandler(this.cmbBaudDriver_SelectionChangeCommitted);
			// 
			// btnSend
			// 
			this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSend.Enabled = false;
			this.btnSend.Location = new System.Drawing.Point(826, 37);
			this.btnSend.Name = "btnSend";
			this.btnSend.Size = new System.Drawing.Size(75, 23);
			this.btnSend.TabIndex = 8;
			this.btnSend.Text = "Send";
			this.btnSend.UseVisualStyleBackColor = true;
			this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
			// 
			// lblLimits
			// 
			this.lblLimits.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lblLimits.AutoSize = true;
			this.lblLimits.Location = new System.Drawing.Point(689, 40);
			this.lblLimits.Name = "lblLimits";
			this.lblLimits.Size = new System.Drawing.Size(27, 13);
			this.lblLimits.TabIndex = 9;
			this.lblLimits.Text = "asdf";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 13);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(55, 13);
			this.label3.TabIndex = 10;
			this.label3.Text = "S42B Port";
			// 
			// label4
			// 
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(522, 13);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(58, 13);
			this.label4.TabIndex = 11;
			this.label4.Text = "Baud Rate";
			// 
			// tmrDumpLog
			// 
			this.tmrDumpLog.Enabled = true;
			this.tmrDumpLog.Tick += new System.EventHandler(this.tmrDumpLog_Tick);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(12, 67);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(57, 13);
			this.label5.TabIndex = 15;
			this.label5.Text = "Marlin Port";
			// 
			// cmbBaudMarlin
			// 
			this.cmbBaudMarlin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmbBaudMarlin.FormattingEnabled = true;
			this.cmbBaudMarlin.Items.AddRange(new object[] {
            "250000",
            "115200",
            "57600"});
			this.cmbBaudMarlin.Location = new System.Drawing.Point(588, 64);
			this.cmbBaudMarlin.Name = "cmbBaudMarlin";
			this.cmbBaudMarlin.Size = new System.Drawing.Size(95, 21);
			this.cmbBaudMarlin.TabIndex = 14;
			this.cmbBaudMarlin.SelectionChangeCommitted += new System.EventHandler(this.cmbBaudMarlin_SelectionChangeCommitted);
			// 
			// btnConnectMarlin
			// 
			this.btnConnectMarlin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnConnectMarlin.Location = new System.Drawing.Point(826, 62);
			this.btnConnectMarlin.Name = "btnConnectMarlin";
			this.btnConnectMarlin.Size = new System.Drawing.Size(75, 23);
			this.btnConnectMarlin.TabIndex = 13;
			this.btnConnectMarlin.Text = "Connect";
			this.btnConnectMarlin.UseVisualStyleBackColor = true;
			this.btnConnectMarlin.Click += new System.EventHandler(this.btnConnectMarlin_Click);
			// 
			// cmbSerialMarlin
			// 
			this.cmbSerialMarlin.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cmbSerialMarlin.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbSerialMarlin.FormattingEnabled = true;
			this.cmbSerialMarlin.Location = new System.Drawing.Point(77, 64);
			this.cmbSerialMarlin.Name = "cmbSerialMarlin";
			this.cmbSerialMarlin.Size = new System.Drawing.Size(439, 21);
			this.cmbSerialMarlin.TabIndex = 12;
			this.cmbSerialMarlin.SelectionChangeCommitted += new System.EventHandler(this.cmbSerialMarlin_SelectionChangeCommitted);
			// 
			// txtGcodeTest
			// 
			this.txtGcodeTest.Location = new System.Drawing.Point(171, 110);
			this.txtGcodeTest.Multiline = true;
			this.txtGcodeTest.Name = "txtGcodeTest";
			this.txtGcodeTest.Size = new System.Drawing.Size(150, 114);
			this.txtGcodeTest.TabIndex = 16;
			this.txtGcodeTest.Text = "G0 X100 ; move\r\nG4 P100 ; dwell\r\nG0 X-100 ; move\r\nG4 P100 ; dwell";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(12, 94);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(70, 13);
			this.label6.TabIndex = 18;
			this.label6.Text = "Prep GCODE";
			// 
			// btnPidTest
			// 
			this.btnPidTest.Enabled = false;
			this.btnPidTest.Location = new System.Drawing.Point(327, 137);
			this.btnPidTest.Name = "btnPidTest";
			this.btnPidTest.Size = new System.Drawing.Size(102, 23);
			this.btnPidTest.TabIndex = 19;
			this.btnPidTest.Text = "Run PID Test";
			this.btnPidTest.UseVisualStyleBackColor = true;
			this.btnPidTest.Click += new System.EventHandler(this.btnPidTest_Click);
			// 
			// cmbPidDivisor
			// 
			this.cmbPidDivisor.FormattingEnabled = true;
			this.cmbPidDivisor.Items.AddRange(new object[] {
            "5",
            "10",
            "20",
            "50",
            "100",
            "200",
            "500",
            "1000"});
			this.cmbPidDivisor.Location = new System.Drawing.Point(327, 110);
			this.cmbPidDivisor.Name = "cmbPidDivisor";
			this.cmbPidDivisor.Size = new System.Drawing.Size(102, 21);
			this.cmbPidDivisor.TabIndex = 20;
			this.cmbPidDivisor.SelectionChangeCommitted += new System.EventHandler(this.cmbPidDivisor_SelectionChangeCommitted);
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(324, 94);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(77, 13);
			this.label7.TabIndex = 21;
			this.label7.Text = "Sample Divisor";
			// 
			// txtMarlinResponse
			// 
			this.txtMarlinResponse.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtMarlinResponse.Location = new System.Drawing.Point(435, 110);
			this.txtMarlinResponse.Multiline = true;
			this.txtMarlinResponse.Name = "txtMarlinResponse";
			this.txtMarlinResponse.Size = new System.Drawing.Size(466, 114);
			this.txtMarlinResponse.TabIndex = 22;
			// 
			// btnDiscDriver
			// 
			this.btnDiscDriver.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnDiscDriver.Enabled = false;
			this.btnDiscDriver.Location = new System.Drawing.Point(745, 8);
			this.btnDiscDriver.Name = "btnDiscDriver";
			this.btnDiscDriver.Size = new System.Drawing.Size(75, 23);
			this.btnDiscDriver.TabIndex = 23;
			this.btnDiscDriver.Text = "Disconnect";
			this.btnDiscDriver.UseVisualStyleBackColor = true;
			this.btnDiscDriver.Click += new System.EventHandler(this.btnDiscDriver_Click);
			// 
			// btnDiscMarlin
			// 
			this.btnDiscMarlin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnDiscMarlin.Enabled = false;
			this.btnDiscMarlin.Location = new System.Drawing.Point(745, 62);
			this.btnDiscMarlin.Name = "btnDiscMarlin";
			this.btnDiscMarlin.Size = new System.Drawing.Size(75, 23);
			this.btnDiscMarlin.TabIndex = 23;
			this.btnDiscMarlin.Text = "Disconnect";
			this.btnDiscMarlin.UseVisualStyleBackColor = true;
			this.btnDiscMarlin.Click += new System.EventHandler(this.btnDiscMarlin_Click);
			// 
			// txtGcodePrep
			// 
			this.txtGcodePrep.Location = new System.Drawing.Point(15, 110);
			this.txtGcodePrep.Multiline = true;
			this.txtGcodePrep.Name = "txtGcodePrep";
			this.txtGcodePrep.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtGcodePrep.Size = new System.Drawing.Size(150, 114);
			this.txtGcodePrep.TabIndex = 16;
			this.txtGcodePrep.Text = resources.GetString("txtGcodePrep.Text");
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(168, 94);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(69, 13);
			this.label8.TabIndex = 18;
			this.label8.Text = "Test GCODE";
			// 
			// tabControl1
			// 
			this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabControl1.Controls.Add(this.tabLog);
			this.tabControl1.Location = new System.Drawing.Point(12, 230);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(889, 323);
			this.tabControl1.TabIndex = 24;
			// 
			// tabLog
			// 
			this.tabLog.Controls.Add(this.textBox1);
			this.tabLog.Location = new System.Drawing.Point(4, 22);
			this.tabLog.Name = "tabLog";
			this.tabLog.Padding = new System.Windows.Forms.Padding(3);
			this.tabLog.Size = new System.Drawing.Size(881, 297);
			this.tabLog.TabIndex = 0;
			this.tabLog.Text = "Log";
			this.tabLog.UseVisualStyleBackColor = true;
			// 
			// frmMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(913, 565);
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.btnDiscMarlin);
			this.Controls.Add(this.btnDiscDriver);
			this.Controls.Add(this.txtMarlinResponse);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.cmbPidDivisor);
			this.Controls.Add(this.btnPidTest);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.txtGcodePrep);
			this.Controls.Add(this.txtGcodeTest);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.cmbBaudMarlin);
			this.Controls.Add(this.btnConnectMarlin);
			this.Controls.Add(this.cmbSerialMarlin);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.lblLimits);
			this.Controls.Add(this.btnSend);
			this.Controls.Add(this.cmbBaudDriver);
			this.Controls.Add(this.btnConnect);
			this.Controls.Add(this.cmbSerialDriver);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.cmbValue);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.cmbCommand);
			this.Name = "frmMain";
			this.Text = "S42B Terminal";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.tabControl1.ResumeLayout(false);
			this.tabLog.ResumeLayout(false);
			this.tabLog.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.ComboBox cmbCommand;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox cmbValue;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox cmbSerialDriver;
		private System.Windows.Forms.Button btnConnect;
		private System.Windows.Forms.ComboBox cmbBaudDriver;
		private System.Windows.Forms.Button btnSend;
		private System.Windows.Forms.Label lblLimits;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Timer tmrDumpLog;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.ComboBox cmbBaudMarlin;
		private System.Windows.Forms.Button btnConnectMarlin;
		private System.Windows.Forms.ComboBox cmbSerialMarlin;
		private System.Windows.Forms.TextBox txtGcodeTest;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Button btnPidTest;
		private System.Windows.Forms.ComboBox cmbPidDivisor;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TextBox txtMarlinResponse;
		private System.Windows.Forms.Button btnDiscDriver;
		private System.Windows.Forms.Button btnDiscMarlin;
		private System.Windows.Forms.TextBox txtGcodePrep;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabLog;
	}
}

