namespace Duka2CTF
{
    partial class MainForm
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
            txtInputPath = new TextBox();
            numDigits = new NumericUpDown();
            txtLogs = new RichTextBox();
            btnStart = new Button();
            lblInputPath = new Label();
            lblDigits = new Label();
            lblLogs = new Label();
            tabControlMain = new TabControl();
            tabConvertCSV = new TabPage();
            tabDownloadDukascopy = new TabPage();
            lblDigitsDownload = new Label();
            numDigitsDownload = new NumericUpDown();
            lblOutputFolder = new Label();
            lblTo = new Label();
            lblFrom = new Label();
            lblSymbol = new Label();
            progressBarDownload = new ProgressBar();
            btnCancel = new Button();
            btnDownloadConvert = new Button();
            btnBrowseOutput = new Button();
            txtOutputFolder = new TextBox();
            dtpTo = new DateTimePicker();
            dtpFrom = new DateTimePicker();
            cmbSymbol = new ComboBox();
            btnTestToCSV = new Button(); // NÚT MỚI: Tải Convert → CSV
            ((System.ComponentModel.ISupportInitialize)numDigits).BeginInit();
            tabControlMain.SuspendLayout();
            tabConvertCSV.SuspendLayout();
            tabDownloadDukascopy.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numDigitsDownload).BeginInit();
            SuspendLayout();
            // 
            // txtInputPath
            // 
            txtInputPath.Location = new Point(143, 20);
            txtInputPath.Name = "txtInputPath";
            txtInputPath.Size = new Size(400, 23);
            txtInputPath.TabIndex = 0;
            // 
            // numDigits
            // 
            numDigits.Location = new Point(143, 60);
            numDigits.Maximum = new decimal(new int[] { 6, 0, 0, 0 });
            numDigits.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numDigits.Name = "numDigits";
            numDigits.Size = new Size(120, 23);
            numDigits.TabIndex = 1;
            numDigits.Value = new decimal(new int[] { 5, 0, 0, 0 });
            // 
            // txtLogs
            // 
            txtLogs.Dock = DockStyle.Bottom;
            txtLogs.Location = new Point(3, 260);
            txtLogs.Name = "txtLogs";
            txtLogs.Size = new Size(770, 170);
            txtLogs.TabIndex = 2;
            txtLogs.Text = "";
            // 
            // btnStart
            // 
            btnStart.Location = new Point(143, 100);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(150, 30);
            btnStart.TabIndex = 3;
            btnStart.Text = "Chuyển CSV → CTF";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click;
            // 
            // lblInputPath
            // 
            lblInputPath.AutoSize = true;
            lblInputPath.Location = new Point(20, 23);
            lblInputPath.Name = "lblInputPath";
            lblInputPath.Size = new Size(112, 15);
            lblInputPath.TabIndex = 4;
            lblInputPath.Text = "Đường dẫn file CSV:";
            // 
            // lblDigits
            // 
            lblDigits.AutoSize = true;
            lblDigits.Location = new Point(20, 62);
            lblDigits.Name = "lblDigits";
            lblDigits.Size = new Size(118, 15);
            lblDigits.TabIndex = 5;
            lblDigits.Text = "Số chữ số thập phân:";
            // 
            // lblLogs
            // 
            lblLogs.AutoSize = true;
            lblLogs.Location = new Point(20, 207);
            lblLogs.Name = "lblLogs";
            lblLogs.Size = new Size(117, 15);
            lblLogs.TabIndex = 6;
            lblLogs.Text = "Nhật ký hoạt động:";
            // 
            // tabControlMain
            // 
            tabControlMain.Controls.Add(tabConvertCSV);
            tabControlMain.Controls.Add(tabDownloadDukascopy);
            tabControlMain.Dock = DockStyle.Fill;
            tabControlMain.Location = new Point(0, 0);
            tabControlMain.Name = "tabControlMain";
            tabControlMain.SelectedIndex = 0;
            tabControlMain.Size = new Size(784, 461);
            tabControlMain.TabIndex = 7;
            // 
            // tabConvertCSV
            // 
            tabConvertCSV.Controls.Add(lblDigits);
            tabConvertCSV.Controls.Add(lblInputPath);
            tabConvertCSV.Controls.Add(btnStart);
            tabConvertCSV.Controls.Add(numDigits);
            tabConvertCSV.Controls.Add(txtInputPath);
            tabConvertCSV.Location = new Point(4, 24);
            tabConvertCSV.Name = "tabConvertCSV";
            tabConvertCSV.Padding = new Padding(3);
            tabConvertCSV.Size = new Size(776, 433);
            tabConvertCSV.TabIndex = 0;
            tabConvertCSV.Text = "Convert từ CSV";
            tabConvertCSV.UseVisualStyleBackColor = true;
            // 
            // tabDownloadDukascopy
            // 
            tabDownloadDukascopy.Controls.Add(lblDigitsDownload);
            tabDownloadDukascopy.Controls.Add(numDigitsDownload);
            tabDownloadDukascopy.Controls.Add(lblOutputFolder);
            tabDownloadDukascopy.Controls.Add(lblTo);
            tabDownloadDukascopy.Controls.Add(lblFrom);
            tabDownloadDukascopy.Controls.Add(lblSymbol);
            tabDownloadDukascopy.Controls.Add(progressBarDownload);
            tabDownloadDukascopy.Controls.Add(btnCancel);
            tabDownloadDukascopy.Controls.Add(btnDownloadConvert);
            tabDownloadDukascopy.Controls.Add(btnTestToCSV); // THÊM NÚT MỚI VÀO TAB
            tabDownloadDukascopy.Controls.Add(btnBrowseOutput);
            tabDownloadDukascopy.Controls.Add(txtOutputFolder);
            tabDownloadDukascopy.Controls.Add(dtpTo);
            tabDownloadDukascopy.Controls.Add(dtpFrom);
            tabDownloadDukascopy.Controls.Add(cmbSymbol);
            tabDownloadDukascopy.Controls.Add(txtLogs);
            tabDownloadDukascopy.Location = new Point(4, 24);
            tabDownloadDukascopy.Name = "tabDownloadDukascopy";
            tabDownloadDukascopy.Padding = new Padding(3);
            tabDownloadDukascopy.Size = new Size(776, 433);
            tabDownloadDukascopy.TabIndex = 1;
            tabDownloadDukascopy.Text = "Tải trực tiếp từ Dukascopy";
            tabDownloadDukascopy.UseVisualStyleBackColor = true;
            // 
            // lblDigitsDownload
            // 
            lblDigitsDownload.AutoSize = true;
            lblDigitsDownload.Location = new Point(20, 163);
            lblDigitsDownload.Name = "lblDigitsDownload";
            lblDigitsDownload.Size = new Size(118, 15);
            lblDigitsDownload.TabIndex = 13;
            lblDigitsDownload.Text = "Số chữ số thập phân:";
            // 
            // numDigitsDownload
            // 
            numDigitsDownload.Location = new Point(159, 161);
            numDigitsDownload.Maximum = new decimal(new int[] { 6, 0, 0, 0 });
            numDigitsDownload.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numDigitsDownload.Name = "numDigitsDownload";
            numDigitsDownload.Size = new Size(120, 23);
            numDigitsDownload.TabIndex = 12;
            numDigitsDownload.Value = new decimal(new int[] { 5, 0, 0, 0 });
            // 
            // lblOutputFolder
            // 
            lblOutputFolder.AutoSize = true;
            lblOutputFolder.Location = new Point(20, 131);
            lblOutputFolder.Name = "lblOutputFolder";
            lblOutputFolder.Size = new Size(97, 15);
            lblOutputFolder.TabIndex = 11;
            lblOutputFolder.Text = "Thư mục lưu .ctf:";
            // 
            // lblTo
            // 
            lblTo.AutoSize = true;
            lblTo.Location = new Point(20, 101);
            lblTo.Name = "lblTo";
            lblTo.Size = new Size(60, 15);
            lblTo.TabIndex = 10;
            lblTo.Text = "Đến ngày:";
            // 
            // lblFrom
            // 
            lblFrom.AutoSize = true;
            lblFrom.Location = new Point(20, 71);
            lblFrom.Name = "lblFrom";
            lblFrom.Size = new Size(52, 15);
            lblFrom.TabIndex = 9;
            lblFrom.Text = "Từ ngày:";
            // 
            // lblSymbol
            // 
            lblSymbol.AutoSize = true;
            lblSymbol.Location = new Point(20, 41);
            lblSymbol.Name = "lblSymbol";
            lblSymbol.Size = new Size(135, 15);
            lblSymbol.TabIndex = 8;
            lblSymbol.Text = "Symbol (ví dụ: XAUUSD)";
            // 
            // progressBarDownload
            // 
            progressBarDownload.Location = new Point(159, 226);
            progressBarDownload.Name = "progressBarDownload";
            progressBarDownload.Size = new Size(481, 15);
            progressBarDownload.TabIndex = 7;
            // 
            // btnCancel
            // 
            btnCancel.Enabled = false;
            btnCancel.Location = new Point(476, 190);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(100, 30);
            btnCancel.TabIndex = 6;
            btnCancel.Text = "Hủy";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnDownloadConvert
            // 
            btnDownloadConvert.Location = new Point(159, 190);
            btnDownloadConvert.Name = "btnDownloadConvert";
            btnDownloadConvert.Size = new Size(150, 30);
            btnDownloadConvert.TabIndex = 5;
            btnDownloadConvert.Text = "Tải & Convert → CTF";
            btnDownloadConvert.UseVisualStyleBackColor = true;
            btnDownloadConvert.Click += btnDownloadConvert_Click;
            // 
            // btnTestToCSV           // NÚT MỚI
            // 
            btnTestToCSV.Location = new Point(320, 190);
            btnTestToCSV.Name = "btnTestToCSV";
            btnTestToCSV.Size = new Size(150, 30);
            btnTestToCSV.TabIndex = 14;
            btnTestToCSV.Text = "Tải Convert → CSV";
            btnTestToCSV.UseVisualStyleBackColor = true;
            btnTestToCSV.Click += btnTestToCSV_Click;
            // 
            // btnBrowseOutput
            // 
            btnBrowseOutput.Location = new Point(565, 128);
            btnBrowseOutput.Name = "btnBrowseOutput";
            btnBrowseOutput.Size = new Size(75, 23);
            btnBrowseOutput.TabIndex = 4;
            btnBrowseOutput.Text = "Chọn...";
            btnBrowseOutput.UseVisualStyleBackColor = true;
            btnBrowseOutput.Click += btnBrowseOutput_Click;
            // 
            // txtOutputFolder
            // 
            txtOutputFolder.Location = new Point(159, 128);
            txtOutputFolder.Name = "txtOutputFolder";
            txtOutputFolder.Size = new Size(400, 23);
            txtOutputFolder.TabIndex = 3;
            // 
            // dtpTo
            // 
            dtpTo.Format = DateTimePickerFormat.Short;
            dtpTo.Location = new Point(159, 98);
            dtpTo.Name = "dtpTo";
            dtpTo.Size = new Size(150, 23);
            dtpTo.TabIndex = 2;
            // 
            // dtpFrom
            // 
            dtpFrom.Format = DateTimePickerFormat.Short;
            dtpFrom.Location = new Point(159, 68);
            dtpFrom.Name = "dtpFrom";
            dtpFrom.Size = new Size(150, 23);
            dtpFrom.TabIndex = 1;
            // 
            // cmbSymbol
            // 
            cmbSymbol.FormattingEnabled = true;
            cmbSymbol.Location = new Point(159, 38);
            cmbSymbol.Name = "cmbSymbol";
            cmbSymbol.Size = new Size(150, 23);
            cmbSymbol.TabIndex = 0;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 461);
            Controls.Add(tabControlMain);
            Name = "MainForm";
            Text = "Duka2CTF - Converter & Downloader";
            ((System.ComponentModel.ISupportInitialize)numDigits).EndInit();
            tabControlMain.ResumeLayout(false);
            tabConvertCSV.ResumeLayout(false);
            tabConvertCSV.PerformLayout();
            tabDownloadDukascopy.ResumeLayout(false);
            tabDownloadDukascopy.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numDigitsDownload).EndInit();
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox txtInputPath;
        private System.Windows.Forms.NumericUpDown numDigits;
        private System.Windows.Forms.RichTextBox txtLogs;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Label lblInputPath;
        private System.Windows.Forms.Label lblDigits;
        private System.Windows.Forms.Label lblLogs;
        private System.Windows.Forms.TabControl tabControlMain;
        private System.Windows.Forms.TabPage tabConvertCSV;
        private System.Windows.Forms.TabPage tabDownloadDukascopy;
        private System.Windows.Forms.ComboBox cmbSymbol;
        private System.Windows.Forms.DateTimePicker dtpFrom;
        private System.Windows.Forms.DateTimePicker dtpTo;
        private System.Windows.Forms.TextBox txtOutputFolder;
        private System.Windows.Forms.Button btnBrowseOutput;
        private System.Windows.Forms.Button btnDownloadConvert;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ProgressBar progressBarDownload;
        private System.Windows.Forms.Label lblSymbol;
        private System.Windows.Forms.Label lblFrom;
        private System.Windows.Forms.Label lblTo;
        private System.Windows.Forms.Label lblOutputFolder;
        private System.Windows.Forms.Label lblDigitsDownload;
        private System.Windows.Forms.NumericUpDown numDigitsDownload;
        private System.Windows.Forms.Button btnTestToCSV; // NÚT MỚI ĐÃ THÊM
    }
}