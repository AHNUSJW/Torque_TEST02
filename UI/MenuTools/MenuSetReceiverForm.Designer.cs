namespace Base.UI.MenuTools
{
    partial class MenuSetReceiverForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MenuSetReceiverForm));
            this.tb_wifiAddr = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.cb_rfCheck = new HZH_Controls.Controls.UCCombox();
            this.cb_rfBaud = new HZH_Controls.Controls.UCCombox();
            this.cb_rfRate = new HZH_Controls.Controls.UCCombox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.cb_rfOption = new HZH_Controls.Controls.UCCombox();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.button4 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.ucCombox1 = new HZH_Controls.Controls.UCCombox();
            this.label8 = new System.Windows.Forms.Label();
            this.ucProcessLineExt1 = new HZH_Controls.Controls.UCProcessLineExt();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tb_wifiAddr
            // 
            this.tb_wifiAddr.BackColor = System.Drawing.Color.White;
            this.tb_wifiAddr.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.tb_wifiAddr, "tb_wifiAddr");
            this.tb_wifiAddr.Name = "tb_wifiAddr";
            this.tb_wifiAddr.Leave += new System.EventHandler(this.tb_WIFIAddr_Leave);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // cb_rfCheck
            // 
            this.cb_rfCheck.BackColor = System.Drawing.Color.Transparent;
            this.cb_rfCheck.BackColorExt = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.cb_rfCheck.BoxStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.cb_rfCheck.ConerRadius = 5;
            this.cb_rfCheck.DropPanelHeight = -1;
            resources.ApplyResources(this.cb_rfCheck, "cb_rfCheck");
            this.cb_rfCheck.FillColor = System.Drawing.Color.White;
            this.cb_rfCheck.IsRadius = true;
            this.cb_rfCheck.IsShowRect = true;
            this.cb_rfCheck.ItemWidth = 70;
            this.cb_rfCheck.Name = "cb_rfCheck";
            this.cb_rfCheck.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.cb_rfCheck.RectWidth = 1;
            this.cb_rfCheck.SelectedIndex = -1;
            this.cb_rfCheck.SelectedValue = "";
            this.cb_rfCheck.Source = null;
            this.cb_rfCheck.TextValue = null;
            this.cb_rfCheck.TriangleColor = System.Drawing.Color.LightSteelBlue;
            // 
            // cb_rfBaud
            // 
            this.cb_rfBaud.BackColor = System.Drawing.Color.Transparent;
            this.cb_rfBaud.BackColorExt = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.cb_rfBaud.BoxStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.cb_rfBaud.ConerRadius = 5;
            this.cb_rfBaud.DropPanelHeight = -1;
            resources.ApplyResources(this.cb_rfBaud, "cb_rfBaud");
            this.cb_rfBaud.FillColor = System.Drawing.Color.White;
            this.cb_rfBaud.IsRadius = true;
            this.cb_rfBaud.IsShowRect = true;
            this.cb_rfBaud.ItemWidth = 70;
            this.cb_rfBaud.Name = "cb_rfBaud";
            this.cb_rfBaud.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.cb_rfBaud.RectWidth = 1;
            this.cb_rfBaud.SelectedIndex = -1;
            this.cb_rfBaud.SelectedValue = "";
            this.cb_rfBaud.Source = null;
            this.cb_rfBaud.TextValue = null;
            this.cb_rfBaud.TriangleColor = System.Drawing.Color.LightSteelBlue;
            // 
            // cb_rfRate
            // 
            this.cb_rfRate.BackColor = System.Drawing.Color.Transparent;
            this.cb_rfRate.BackColorExt = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.cb_rfRate.BoxStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.cb_rfRate.ConerRadius = 5;
            this.cb_rfRate.DropPanelHeight = -1;
            resources.ApplyResources(this.cb_rfRate, "cb_rfRate");
            this.cb_rfRate.FillColor = System.Drawing.Color.White;
            this.cb_rfRate.IsRadius = true;
            this.cb_rfRate.IsShowRect = true;
            this.cb_rfRate.ItemWidth = 70;
            this.cb_rfRate.Name = "cb_rfRate";
            this.cb_rfRate.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.cb_rfRate.RectWidth = 1;
            this.cb_rfRate.SelectedIndex = -1;
            this.cb_rfRate.SelectedValue = "";
            this.cb_rfRate.Source = null;
            this.cb_rfRate.TextValue = null;
            this.cb_rfRate.TriangleColor = System.Drawing.Color.LightSteelBlue;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tb_wifiAddr);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.cb_rfOption);
            this.groupBox2.Controls.Add(this.cb_rfBaud);
            this.groupBox2.Controls.Add(this.cb_rfCheck);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.cb_rfRate);
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // cb_rfOption
            // 
            this.cb_rfOption.BackColor = System.Drawing.Color.Transparent;
            this.cb_rfOption.BackColorExt = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.cb_rfOption.BoxStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.cb_rfOption.ConerRadius = 5;
            this.cb_rfOption.DropPanelHeight = -1;
            resources.ApplyResources(this.cb_rfOption, "cb_rfOption");
            this.cb_rfOption.FillColor = System.Drawing.Color.White;
            this.cb_rfOption.IsRadius = true;
            this.cb_rfOption.IsShowRect = true;
            this.cb_rfOption.ItemWidth = 70;
            this.cb_rfOption.Name = "cb_rfOption";
            this.cb_rfOption.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.cb_rfOption.RectWidth = 1;
            this.cb_rfOption.SelectedIndex = -1;
            this.cb_rfOption.SelectedValue = "";
            this.cb_rfOption.Source = null;
            this.cb_rfOption.TextValue = null;
            this.cb_rfOption.TriangleColor = System.Drawing.Color.LightSteelBlue;
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this.button1, "button1");
            this.button1.Name = "button1";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.button4);
            this.groupBox3.Controls.Add(this.button3);
            this.groupBox3.Controls.Add(this.button2);
            this.groupBox3.Controls.Add(this.ucCombox1);
            this.groupBox3.Controls.Add(this.label8);
            resources.ApplyResources(this.groupBox3, "groupBox3");
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.TabStop = false;
            // 
            // button4
            // 
            this.button4.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this.button4, "button4");
            this.button4.Name = "button4";
            this.button4.UseVisualStyleBackColor = false;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button3
            // 
            this.button3.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this.button3, "button3");
            this.button3.Name = "button3";
            this.button3.UseVisualStyleBackColor = false;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this.button2, "button2");
            this.button2.Name = "button2";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // ucCombox1
            // 
            this.ucCombox1.BackColor = System.Drawing.Color.Transparent;
            this.ucCombox1.BackColorExt = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.ucCombox1.BoxStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.ucCombox1.ConerRadius = 5;
            this.ucCombox1.DropPanelHeight = -1;
            this.ucCombox1.FillColor = System.Drawing.Color.White;
            resources.ApplyResources(this.ucCombox1, "ucCombox1");
            this.ucCombox1.IsRadius = true;
            this.ucCombox1.IsShowRect = true;
            this.ucCombox1.ItemWidth = 70;
            this.ucCombox1.Name = "ucCombox1";
            this.ucCombox1.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.ucCombox1.RectWidth = 1;
            this.ucCombox1.SelectedIndex = -1;
            this.ucCombox1.SelectedValue = "";
            this.ucCombox1.Source = null;
            this.ucCombox1.TextValue = null;
            this.ucCombox1.TriangleColor = System.Drawing.Color.LightSteelBlue;
            // 
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.label8.Name = "label8";
            // 
            // ucProcessLineExt1
            // 
            this.ucProcessLineExt1.BackColor = System.Drawing.Color.Transparent;
            this.ucProcessLineExt1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(228)))), ((int)(((byte)(231)))), ((int)(((byte)(237)))));
            this.ucProcessLineExt1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(157)))), ((int)(((byte)(144)))));
            resources.ApplyResources(this.ucProcessLineExt1, "ucProcessLineExt1");
            this.ucProcessLineExt1.MaxValue = 100;
            this.ucProcessLineExt1.Name = "ucProcessLineExt1";
            this.ucProcessLineExt1.Value = 0;
            this.ucProcessLineExt1.ValueBGColor = System.Drawing.Color.FromArgb(((int)(((byte)(228)))), ((int)(((byte)(231)))), ((int)(((byte)(237)))));
            this.ucProcessLineExt1.ValueColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(157)))), ((int)(((byte)(144)))));
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // MenuSetReceiverForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.ucProcessLineExt1);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox2);
            this.Name = "MenuSetReceiverForm";
            this.ShowIcon = false;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MenuSetReceiverForm_FormClosing);
            this.Load += new System.EventHandler(this.MenuSetReceiverForm_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox tb_wifiAddr;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private HZH_Controls.Controls.UCCombox cb_rfCheck;
        private HZH_Controls.Controls.UCCombox cb_rfBaud;
        private HZH_Controls.Controls.UCCombox cb_rfRate;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button button1;
        private HZH_Controls.Controls.UCCombox cb_rfOption;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button button2;
        private HZH_Controls.Controls.UCCombox ucCombox1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button button3;
        private HZH_Controls.Controls.UCProcessLineExt ucProcessLineExt1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button button4;
    }
}