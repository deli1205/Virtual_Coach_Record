namespace Record_mix_03
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.BT_file_path = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.BT_path_file_path = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.BT_record_start = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(1, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(512, 424);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Location = new System.Drawing.Point(533, 0);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(612, 424);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox2.TabIndex = 1;
            this.pictureBox2.TabStop = false;
            // 
            // BT_file_path
            // 
            this.BT_file_path.Location = new System.Drawing.Point(12, 430);
            this.BT_file_path.Name = "BT_file_path";
            this.BT_file_path.Size = new System.Drawing.Size(75, 23);
            this.BT_file_path.TabIndex = 2;
            this.BT_file_path.Text = "瀏覽";
            this.BT_file_path.UseVisualStyleBackColor = true;
            this.BT_file_path.Click += new System.EventHandler(this.BT_Video_Path_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(96, 435);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "影片儲存路徑";
            // 
            // BT_path_file_path
            // 
            this.BT_path_file_path.Location = new System.Drawing.Point(12, 468);
            this.BT_path_file_path.Name = "BT_path_file_path";
            this.BT_path_file_path.Size = new System.Drawing.Size(75, 23);
            this.BT_path_file_path.TabIndex = 5;
            this.BT_path_file_path.Text = "瀏覽";
            this.BT_path_file_path.UseVisualStyleBackColor = true;
            this.BT_path_file_path.Click += new System.EventHandler(this.BT_path_file_path_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(96, 473);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "軌跡儲存路徑";
            // 
            // BT_record_start
            // 
            this.BT_record_start.Location = new System.Drawing.Point(296, 452);
            this.BT_record_start.Name = "BT_record_start";
            this.BT_record_start.Size = new System.Drawing.Size(75, 23);
            this.BT_record_start.TabIndex = 7;
            this.BT_record_start.Text = "開始記錄";
            this.BT_record_start.UseVisualStyleBackColor = true;
            this.BT_record_start.Click += new System.EventHandler(this.BT_record_start_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1275, 503);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.BT_record_start);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.BT_path_file_path);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.BT_file_path);
            this.Controls.Add(this.pictureBox2);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Button BT_file_path;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button BT_path_file_path;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button BT_record_start;
    }
}

