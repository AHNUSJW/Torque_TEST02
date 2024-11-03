namespace Base.UI.MenuTools
{
    partial class MenuExportForm
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
            this.bt_load = new System.Windows.Forms.Button();
            this.tb_load = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.bt_report = new System.Windows.Forms.Button();
            this.tb_time = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.tb_opsn = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tb_standard = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tb_commodity = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tb_company = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tb_fileName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // bt_load
            // 
            this.bt_load.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.bt_load.Location = new System.Drawing.Point(301, 104);
            this.bt_load.Margin = new System.Windows.Forms.Padding(2);
            this.bt_load.Name = "bt_load";
            this.bt_load.Size = new System.Drawing.Size(27, 22);
            this.bt_load.TabIndex = 37;
            this.bt_load.Text = "...";
            this.bt_load.UseVisualStyleBackColor = true;
            this.bt_load.Click += new System.EventHandler(this.bt_load_Click);
            // 
            // tb_load
            // 
            this.tb_load.Font = new System.Drawing.Font("宋体", 10.8F);
            this.tb_load.Location = new System.Drawing.Point(97, 104);
            this.tb_load.Margin = new System.Windows.Forms.Padding(2);
            this.tb_load.Name = "tb_load";
            this.tb_load.ReadOnly = true;
            this.tb_load.Size = new System.Drawing.Size(200, 24);
            this.tb_load.TabIndex = 36;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("宋体", 10.8F);
            this.label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label3.Location = new System.Drawing.Point(25, 108);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 15);
            this.label3.TabIndex = 35;
            this.label3.Text = "保存路径";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label9.Location = new System.Drawing.Point(23, 44);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(281, 12);
            this.label9.TabIndex = 34;
            this.label9.Text = "----------------------------------------------";
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("楷体", 22.2F);
            this.label8.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label8.Location = new System.Drawing.Point(77, 16);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(178, 30);
            this.label8.TabIndex = 33;
            this.label8.Text = "报 告 信 息";
            // 
            // bt_report
            // 
            this.bt_report.Font = new System.Drawing.Font("宋体", 10.8F);
            this.bt_report.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.bt_report.Location = new System.Drawing.Point(232, 362);
            this.bt_report.Margin = new System.Windows.Forms.Padding(2);
            this.bt_report.Name = "bt_report";
            this.bt_report.Size = new System.Drawing.Size(80, 38);
            this.bt_report.TabIndex = 32;
            this.bt_report.Text = "生成报告";
            this.bt_report.UseVisualStyleBackColor = true;
            this.bt_report.Click += new System.EventHandler(this.bt_report_Click);
            // 
            // tb_time
            // 
            this.tb_time.Font = new System.Drawing.Font("宋体", 10.8F);
            this.tb_time.Location = new System.Drawing.Point(97, 328);
            this.tb_time.Margin = new System.Windows.Forms.Padding(2);
            this.tb_time.Name = "tb_time";
            this.tb_time.ReadOnly = true;
            this.tb_time.Size = new System.Drawing.Size(200, 24);
            this.tb_time.TabIndex = 31;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("宋体", 10.8F);
            this.label7.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label7.Location = new System.Drawing.Point(25, 331);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(67, 15);
            this.label7.TabIndex = 30;
            this.label7.Text = "报告日期";
            // 
            // tb_opsn
            // 
            this.tb_opsn.BackColor = System.Drawing.SystemColors.Control;
            this.tb_opsn.Font = new System.Drawing.Font("宋体", 10.8F);
            this.tb_opsn.Location = new System.Drawing.Point(97, 282);
            this.tb_opsn.Margin = new System.Windows.Forms.Padding(2);
            this.tb_opsn.Name = "tb_opsn";
            this.tb_opsn.ReadOnly = true;
            this.tb_opsn.Size = new System.Drawing.Size(200, 24);
            this.tb_opsn.TabIndex = 29;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("宋体", 10.8F);
            this.label6.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label6.Location = new System.Drawing.Point(26, 285);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(68, 15);
            this.label6.TabIndex = 28;
            this.label6.Text = "流 水 号";
            // 
            // tb_standard
            // 
            this.tb_standard.Font = new System.Drawing.Font("宋体", 10.8F);
            this.tb_standard.Location = new System.Drawing.Point(97, 240);
            this.tb_standard.Margin = new System.Windows.Forms.Padding(2);
            this.tb_standard.Name = "tb_standard";
            this.tb_standard.Size = new System.Drawing.Size(200, 24);
            this.tb_standard.TabIndex = 27;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("宋体", 10.8F);
            this.label5.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label5.Location = new System.Drawing.Point(25, 244);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(67, 15);
            this.label5.TabIndex = 26;
            this.label5.Text = "执行标准";
            // 
            // tb_commodity
            // 
            this.tb_commodity.Font = new System.Drawing.Font("宋体", 10.8F);
            this.tb_commodity.Location = new System.Drawing.Point(97, 201);
            this.tb_commodity.Margin = new System.Windows.Forms.Padding(2);
            this.tb_commodity.Name = "tb_commodity";
            this.tb_commodity.Size = new System.Drawing.Size(200, 24);
            this.tb_commodity.TabIndex = 25;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("宋体", 10.8F);
            this.label4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label4.Location = new System.Drawing.Point(25, 205);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(69, 15);
            this.label4.TabIndex = 24;
            this.label4.Text = "品    名";
            // 
            // tb_company
            // 
            this.tb_company.Font = new System.Drawing.Font("宋体", 10.8F);
            this.tb_company.Location = new System.Drawing.Point(97, 158);
            this.tb_company.Margin = new System.Windows.Forms.Padding(2);
            this.tb_company.Name = "tb_company";
            this.tb_company.Size = new System.Drawing.Size(200, 24);
            this.tb_company.TabIndex = 23;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 10.8F);
            this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label2.Location = new System.Drawing.Point(25, 162);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 15);
            this.label2.TabIndex = 22;
            this.label2.Text = "公司名称";
            // 
            // tb_fileName
            // 
            this.tb_fileName.Font = new System.Drawing.Font("宋体", 10.8F);
            this.tb_fileName.Location = new System.Drawing.Point(97, 64);
            this.tb_fileName.Margin = new System.Windows.Forms.Padding(2);
            this.tb_fileName.Name = "tb_fileName";
            this.tb_fileName.Size = new System.Drawing.Size(200, 24);
            this.tb_fileName.TabIndex = 21;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 10.8F);
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(25, 68);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 15);
            this.label1.TabIndex = 20;
            this.label1.Text = "文件名称";
            // 
            // MenuExportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(350, 417);
            this.Controls.Add(this.bt_load);
            this.Controls.Add(this.tb_load);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.bt_report);
            this.Controls.Add(this.tb_time);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.tb_opsn);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.tb_standard);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tb_commodity);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tb_company);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tb_fileName);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MenuExportForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "报告编辑";
            this.Load += new System.EventHandler(this.MenuExportForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button bt_load;
        private System.Windows.Forms.TextBox tb_load;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button bt_report;
        private System.Windows.Forms.TextBox tb_time;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tb_opsn;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tb_standard;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tb_commodity;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tb_company;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tb_fileName;
        private System.Windows.Forms.Label label1;
    }
}