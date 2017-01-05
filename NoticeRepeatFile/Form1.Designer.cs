namespace NoticeRepeatFile
{
    partial class Form1
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.button1 = new System.Windows.Forms.Button();
            this.dg1 = new System.Windows.Forms.DataGridView();
            this.txtMsg = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.txtKeyword = new System.Windows.Forms.TextBox();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.txtAvi = new System.Windows.Forms.TextBox();
            this.txtPath = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dg1)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(391, 39);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Import";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // dg1
            // 
            this.dg1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dg1.Location = new System.Drawing.Point(12, 83);
            this.dg1.Name = "dg1";
            this.dg1.RowTemplate.Height = 24;
            this.dg1.Size = new System.Drawing.Size(612, 215);
            this.dg1.TabIndex = 3;
            // 
            // txtMsg
            // 
            this.txtMsg.Location = new System.Drawing.Point(12, 304);
            this.txtMsg.Multiline = true;
            this.txtMsg.Name = "txtMsg";
            this.txtMsg.Size = new System.Drawing.Size(612, 27);
            this.txtMsg.TabIndex = 4;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(559, 13);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 5;
            this.button2.Text = "Search";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // txtKeyword
            // 
            this.txtKeyword.Location = new System.Drawing.Point(391, 13);
            this.txtKeyword.Name = "txtKeyword";
            this.txtKeyword.Size = new System.Drawing.Size(162, 22);
            this.txtKeyword.TabIndex = 6;
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Text = "notifyIcon1";
            this.notifyIcon1.Visible = true;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(295, 12);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 7;
            this.button3.Text = "TorrentFolder";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(295, 41);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 9;
            this.button4.Text = "AviFolder";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // txtAvi
            // 
            this.txtAvi.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::NoticeRepeatFile.Properties.Settings.Default, "AviFolder", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.txtAvi.Location = new System.Drawing.Point(12, 41);
            this.txtAvi.Name = "txtAvi";
            this.txtAvi.ReadOnly = true;
            this.txtAvi.Size = new System.Drawing.Size(268, 22);
            this.txtAvi.TabIndex = 8;
            this.txtAvi.Text = global::NoticeRepeatFile.Properties.Settings.Default.AviFolder;
            // 
            // txtPath
            // 
            this.txtPath.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::NoticeRepeatFile.Properties.Settings.Default, "torrentFolder", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.txtPath.Location = new System.Drawing.Point(12, 12);
            this.txtPath.Name = "txtPath";
            this.txtPath.ReadOnly = true;
            this.txtPath.Size = new System.Drawing.Size(268, 22);
            this.txtPath.TabIndex = 2;
            this.txtPath.Text = global::NoticeRepeatFile.Properties.Settings.Default.torrentFolder;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(689, 343);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.txtAvi);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.txtKeyword);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.txtMsg);
            this.Controls.Add(this.dg1);
            this.Controls.Add(this.txtPath);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.SizeChanged += new System.EventHandler(this.Form1_SizeChanged);
            ((System.ComponentModel.ISupportInitialize)(this.dg1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.DataGridView dg1;
        private System.Windows.Forms.TextBox txtMsg;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox txtKeyword;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TextBox txtAvi;
        private System.Windows.Forms.Button button4;
    }
}

