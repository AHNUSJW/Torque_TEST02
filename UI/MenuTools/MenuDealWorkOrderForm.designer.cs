namespace Base.UI.MenuTools
{
    partial class MenuDealWorkOrderForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MenuDealWorkOrderForm));
            this.label9 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.备注 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.复制工单号ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.生成条形码ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.生成二维码ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editNoteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DelWorkOrderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonX2 = new DTiws.View.ButtonX();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label9
            // 
            resources.ApplyResources(this.label9, "label9");
            this.label9.Name = "label9";
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.dataGridView1);
            this.panel1.Name = "panel1";
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.BackgroundColor = System.Drawing.SystemColors.ControlLight;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2,
            this.Column7,
            this.备注});
            resources.ApplyResources(this.dataGridView1, "dataGridView1");
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.TabStop = false;
            this.dataGridView1.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellDoubleClick);
            this.dataGridView1.CellMouseUp += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView1_CellMouseUp);
            // 
            // Column1
            // 
            this.Column1.FillWeight = 50F;
            resources.ApplyResources(this.Column1, "Column1");
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            // 
            // Column2
            // 
            resources.ApplyResources(this.Column2, "Column2");
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            // 
            // Column7
            // 
            resources.ApplyResources(this.Column7, "Column7");
            this.Column7.Name = "Column7";
            this.Column7.ReadOnly = true;
            // 
            // 备注
            // 
            resources.ApplyResources(this.备注, "备注");
            this.备注.Name = "备注";
            this.备注.ReadOnly = true;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.复制工单号ToolStripMenuItem,
            this.生成条形码ToolStripMenuItem,
            this.生成二维码ToolStripMenuItem,
            this.editNoteToolStripMenuItem,
            this.DelWorkOrderToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            resources.ApplyResources(this.contextMenuStrip1, "contextMenuStrip1");
            // 
            // 复制工单号ToolStripMenuItem
            // 
            this.复制工单号ToolStripMenuItem.Name = "复制工单号ToolStripMenuItem";
            resources.ApplyResources(this.复制工单号ToolStripMenuItem, "复制工单号ToolStripMenuItem");
            this.复制工单号ToolStripMenuItem.Click += new System.EventHandler(this.复制工单号ToolStripMenuItem_Click);
            // 
            // 生成条形码ToolStripMenuItem
            // 
            this.生成条形码ToolStripMenuItem.Name = "生成条形码ToolStripMenuItem";
            resources.ApplyResources(this.生成条形码ToolStripMenuItem, "生成条形码ToolStripMenuItem");
            this.生成条形码ToolStripMenuItem.Click += new System.EventHandler(this.生成条形码ToolStripMenuItem_Click);
            // 
            // 生成二维码ToolStripMenuItem
            // 
            this.生成二维码ToolStripMenuItem.Name = "生成二维码ToolStripMenuItem";
            resources.ApplyResources(this.生成二维码ToolStripMenuItem, "生成二维码ToolStripMenuItem");
            this.生成二维码ToolStripMenuItem.Click += new System.EventHandler(this.生成二维码ToolStripMenuItem_Click);
            // 
            // editNoteToolStripMenuItem
            // 
            this.editNoteToolStripMenuItem.Name = "editNoteToolStripMenuItem";
            resources.ApplyResources(this.editNoteToolStripMenuItem, "editNoteToolStripMenuItem");
            this.editNoteToolStripMenuItem.Click += new System.EventHandler(this.editNoteToolStripMenuItem_Click);
            // 
            // DelWorkOrderToolStripMenuItem
            // 
            this.DelWorkOrderToolStripMenuItem.Name = "DelWorkOrderToolStripMenuItem";
            resources.ApplyResources(this.DelWorkOrderToolStripMenuItem, "DelWorkOrderToolStripMenuItem");
            this.DelWorkOrderToolStripMenuItem.Click += new System.EventHandler(this.DelWorkOrderToolStripMenuItem_Click);
            // 
            // buttonX2
            // 
            resources.ApplyResources(this.buttonX2, "buttonX2");
            this.buttonX2.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.buttonX2.EnterForeColor = System.Drawing.Color.White;
            this.buttonX2.HoverBackColor = System.Drawing.Color.CadetBlue;
            this.buttonX2.HoverForeColor = System.Drawing.Color.White;
            this.buttonX2.Name = "buttonX2";
            this.buttonX2.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.buttonX2.PressForeColor = System.Drawing.Color.White;
            this.buttonX2.Radius = 6;
            this.buttonX2.UseVisualStyleBackColor = true;
            this.buttonX2.Click += new System.EventHandler(this.buttonX2_Click);
            // 
            // MenuDealWorkOrderForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightBlue;
            this.Controls.Add(this.buttonX2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label9);
            this.Name = "MenuDealWorkOrderForm";
            this.ShowIcon = false;
            this.Load += new System.EventHandler(this.MenuDealWorkOrderForm_Load);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private DTiws.View.ButtonX buttonX2;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem DelWorkOrderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editNoteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 生成条形码ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 生成二维码ToolStripMenuItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column7;
        private System.Windows.Forms.DataGridViewTextBoxColumn 备注;
        private System.Windows.Forms.ToolStripMenuItem 复制工单号ToolStripMenuItem;
    }
}