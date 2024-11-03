
namespace Base.UI.MenuTools
{
    partial class MenuImportWorkOrderForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MenuImportWorkOrderForm));
            this.buttonX1 = new DTiws.View.ButtonX();
            this.label1 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.buttonX2 = new DTiws.View.ButtonX();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // buttonX1
            // 
            resources.ApplyResources(this.buttonX1, "buttonX1");
            this.buttonX1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.buttonX1.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.buttonX1.EnterForeColor = System.Drawing.Color.White;
            this.buttonX1.HoverBackColor = System.Drawing.Color.CadetBlue;
            this.buttonX1.HoverForeColor = System.Drawing.Color.White;
            this.buttonX1.Name = "buttonX1";
            this.buttonX1.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.buttonX1.PressForeColor = System.Drawing.Color.White;
            this.buttonX1.Radius = 6;
            this.buttonX1.UseVisualStyleBackColor = false;
            this.buttonX1.Click += new System.EventHandler(this.buttonX1_Click);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // buttonX2
            // 
            resources.ApplyResources(this.buttonX2, "buttonX2");
            this.buttonX2.BackColor = System.Drawing.SystemColors.ControlLight;
            this.buttonX2.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.buttonX2.EnterForeColor = System.Drawing.Color.White;
            this.buttonX2.HoverBackColor = System.Drawing.Color.CadetBlue;
            this.buttonX2.HoverForeColor = System.Drawing.Color.White;
            this.buttonX2.Name = "buttonX2";
            this.buttonX2.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.buttonX2.PressForeColor = System.Drawing.Color.White;
            this.buttonX2.Radius = 1;
            this.buttonX2.UseVisualStyleBackColor = false;
            this.buttonX2.Click += new System.EventHandler(this.buttonX2_Click);
            // 
            // textBox1
            // 
            resources.ApplyResources(this.textBox1, "textBox1");
            this.textBox1.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Name = "textBox1";
            this.textBox1.TabStop = false;
            this.textBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
            this.textBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
            // 
            // MenuImportWorkOrderForm
            // 
            this.AcceptButton = this.buttonX1;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.buttonX2);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonX1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MenuImportWorkOrderForm";
            this.ShowIcon = false;
            this.Load += new System.EventHandler(this.MenuImportWorkOrderForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private DTiws.View.ButtonX buttonX1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label5;
        private DTiws.View.ButtonX buttonX2;
        private System.Windows.Forms.TextBox textBox1;
    }
}