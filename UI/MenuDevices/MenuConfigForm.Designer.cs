namespace Base
{
    partial class MenuConfigForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MenuConfigForm));
            this.lb_step = new System.Windows.Forms.Label();
            this.bt_open_send = new System.Windows.Forms.Button();
            this.lb_name = new System.Windows.Forms.Label();
            this.bt_next = new System.Windows.Forms.Button();
            this.lb_psw = new System.Windows.Forms.Label();
            this.tb_message = new System.Windows.Forms.TextBox();
            this.cb_name = new System.Windows.Forms.ComboBox();
            this.tb_psw = new System.Windows.Forms.TextBox();
            this.bt_reset = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lb_step
            // 
            resources.ApplyResources(this.lb_step, "lb_step");
            this.lb_step.Name = "lb_step";
            // 
            // bt_open_send
            // 
            resources.ApplyResources(this.bt_open_send, "bt_open_send");
            this.bt_open_send.BackColor = System.Drawing.Color.White;
            this.bt_open_send.Name = "bt_open_send";
            this.bt_open_send.UseVisualStyleBackColor = false;
            this.bt_open_send.Click += new System.EventHandler(this.bt_open_send_Click);
            // 
            // lb_name
            // 
            resources.ApplyResources(this.lb_name, "lb_name");
            this.lb_name.Name = "lb_name";
            // 
            // bt_next
            // 
            resources.ApplyResources(this.bt_next, "bt_next");
            this.bt_next.FlatAppearance.BorderSize = 0;
            this.bt_next.Name = "bt_next";
            this.bt_next.UseVisualStyleBackColor = true;
            this.bt_next.Click += new System.EventHandler(this.bt_next_Click);
            // 
            // lb_psw
            // 
            resources.ApplyResources(this.lb_psw, "lb_psw");
            this.lb_psw.Name = "lb_psw";
            // 
            // tb_message
            // 
            resources.ApplyResources(this.tb_message, "tb_message");
            this.tb_message.BackColor = System.Drawing.SystemColors.Control;
            this.tb_message.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tb_message.Name = "tb_message";
            this.tb_message.ReadOnly = true;
            // 
            // cb_name
            // 
            resources.ApplyResources(this.cb_name, "cb_name");
            this.cb_name.FormattingEnabled = true;
            this.cb_name.Name = "cb_name";
            this.cb_name.SelectedIndexChanged += new System.EventHandler(this.cb_name_SelectedIndexChanged);
            // 
            // tb_psw
            // 
            resources.ApplyResources(this.tb_psw, "tb_psw");
            this.tb_psw.Name = "tb_psw";
            // 
            // bt_reset
            // 
            resources.ApplyResources(this.bt_reset, "bt_reset");
            this.bt_reset.FlatAppearance.BorderSize = 0;
            this.bt_reset.Name = "bt_reset";
            this.bt_reset.UseVisualStyleBackColor = true;
            this.bt_reset.Click += new System.EventHandler(this.bt_reset_Click);
            // 
            // MenuConfigForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tb_psw);
            this.Controls.Add(this.cb_name);
            this.Controls.Add(this.bt_reset);
            this.Controls.Add(this.lb_step);
            this.Controls.Add(this.bt_open_send);
            this.Controls.Add(this.bt_next);
            this.Controls.Add(this.lb_psw);
            this.Controls.Add(this.lb_name);
            this.Controls.Add(this.tb_message);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MenuConfigForm";
            this.ShowIcon = false;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MenuConfigureForm_FormClosing);
            this.Load += new System.EventHandler(this.MenuConfigureForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lb_step;
        private System.Windows.Forms.Button bt_open_send;
        private System.Windows.Forms.Label lb_name;
        private System.Windows.Forms.Button bt_next;
        private System.Windows.Forms.Label lb_psw;
        private System.Windows.Forms.TextBox tb_message;
        private System.Windows.Forms.ComboBox cb_name;
        private System.Windows.Forms.TextBox tb_psw;
        private System.Windows.Forms.Button bt_reset;
    }
}