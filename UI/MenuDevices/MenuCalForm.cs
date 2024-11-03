using HZH_Controls.Controls;
using Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

//Junzhe 20230807

//多设备保存参数统一使用0x63指令(0x63、0x64合并)
//有线/蓝牙设备保持不变，使用0x63和0x64指令

namespace Base.UI.MenuDevices
{
    public partial class MenuCalForm : Form
    {
        String unit = ""; //单位
        private XET actXET;//需操作的设备
        private volatile TASKS nextTask;//按键操作指令
        private volatile int addrIndex;//已连接设备的地址指针
        private List<Byte> mutiAddres = new List<Byte>();//存储已连接设备的地址
        private DataGridViewTextBoxEditingControl CellEdit = null;
        private string oldActiveForm;

        public MenuCalForm()
        {
            InitializeComponent();
        }

        public class GridModel
        {
            public string Device { get; set; }
        }

        //界面初始化
        private void MenuCalForm_Load(object sender, EventArgs e)
        {
            //初始化设备数据
            actXET = MyDevice.actDev;

            //
            oldActiveForm = Main.ActiveForm;
            Main.ActiveForm = "SetDevices";

            //获取当前力矩单位
            switch (actXET.torqueUnit)
            {
                case UNIT.UNIT_nm: unit = "N·m"; break;
                case UNIT.UNIT_lbfin: unit = "lbf·in"; break;
                case UNIT.UNIT_lbfft: unit = "lbf·ft"; break;
                case UNIT.UNIT_kgcm: unit = "kgf·cm"; break;
            }

            //初始化table
            InitTableA1M0();
            InitTableA1MX();
            InitTableA2M0();
            InitTableA2MX();

            MyDevice.myUpdate += new freshHandler(receivePara);
        }

        //关闭窗口
        private void MenuCalForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            MyDevice.myUpdate -= new freshHandler(receivePara);
            Main.ActiveForm = oldActiveForm;
        }

        //数据输入
        private void data_KeyPress(object sender, KeyPressEventArgs e)
        {
            //只允许输入数字,负号,小数点和删除键
            if (((e.KeyChar < '0') || (e.KeyChar > '9')) && (e.KeyChar != '.') && (e.KeyChar != 8))
            {
                e.Handled = true;
                return;
            }

            //小数点只能出现1位
            if ((e.KeyChar == '.') && ((DataGridViewTextBoxEditingControl)sender).Text.Contains("."))
            {
                e.Handled = true;
                return;
            }

            //第一位不能为小数点
            if ((e.KeyChar == '.') && (((DataGridViewTextBoxEditingControl)sender).Text.Length == 0))
            {
                e.Handled = true;
                return;
            }
        }

        //table表键盘输入拦截
        private void dataGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            CellEdit = (DataGridViewTextBoxEditingControl)e.Control;
            CellEdit.KeyPress += data_KeyPress;
        }

        //更新A1MX表
        private void UpdateTableA1MX()
        {
            dataGridView1.Rows[0].Cells[1].Value = actXET.a1mxTable.A1M0.torqueTarget / 100.0f;
            dataGridView2.Rows[0].Cells[1].Value = actXET.a1mxTable.A1M1.torqueLow / 100.0f;
            dataGridView2.Rows[0].Cells[2].Value = actXET.a1mxTable.A1M1.torqueHigh / 100.0f;
            dataGridView2.Rows[1].Cells[1].Value = actXET.a1mxTable.A1M2.torqueLow / 100.0f;
            dataGridView2.Rows[1].Cells[2].Value = actXET.a1mxTable.A1M2.torqueHigh / 100.0f;
            dataGridView2.Rows[2].Cells[1].Value = actXET.a1mxTable.A1M3.torqueLow / 100.0f;
            dataGridView2.Rows[2].Cells[2].Value = actXET.a1mxTable.A1M3.torqueHigh / 100.0f;
            dataGridView2.Rows[3].Cells[1].Value = actXET.a1mxTable.A1M4.torqueLow / 100.0f;
            dataGridView2.Rows[3].Cells[2].Value = actXET.a1mxTable.A1M4.torqueHigh / 100.0f;
            dataGridView2.Rows[4].Cells[1].Value = actXET.a1mxTable.A1M5.torqueLow / 100.0f;
            dataGridView2.Rows[4].Cells[2].Value = actXET.a1mxTable.A1M5.torqueHigh / 100.0f;
            dataGridView2.Rows[5].Cells[1].Value = actXET.a1mxTable.A1M6.torqueLow / 100.0f;
            dataGridView2.Rows[5].Cells[2].Value = actXET.a1mxTable.A1M6.torqueHigh / 100.0f;
            dataGridView2.Rows[6].Cells[1].Value = actXET.a1mxTable.A1M7.torqueLow / 100.0f;
            dataGridView2.Rows[6].Cells[2].Value = actXET.a1mxTable.A1M7.torqueHigh / 100.0f;
            dataGridView2.Rows[7].Cells[1].Value = actXET.a1mxTable.A1M8.torqueLow / 100.0f;
            dataGridView2.Rows[7].Cells[2].Value = actXET.a1mxTable.A1M8.torqueHigh / 100.0f;
            dataGridView2.Rows[8].Cells[1].Value = actXET.a1mxTable.A1M9.torqueLow / 100.0f;
            dataGridView2.Rows[8].Cells[2].Value = actXET.a1mxTable.A1M9.torqueHigh / 100.0f;
        }

        //更新A2MX表
        private void UpdateTableA2MX()
        {
            dataGridView3.Rows[0].Cells[1].Value = actXET.a2mxTable.A2M0.torquePre / 100.0f;
            dataGridView3.Rows[0].Cells[2].Value = actXET.a2mxTable.A2M0.angleTarget / 10.0f;
            dataGridView4.Rows[0].Cells[1].Value = actXET.a2mxTable.A2M1.torquePre / 100.0f;
            dataGridView4.Rows[0].Cells[2].Value = actXET.a2mxTable.A2M1.angleLow / 10.0f;
            dataGridView4.Rows[0].Cells[3].Value = actXET.a2mxTable.A2M1.angleHigh / 10.0f;
            dataGridView4.Rows[1].Cells[1].Value = actXET.a2mxTable.A2M2.torquePre / 100.0f;
            dataGridView4.Rows[1].Cells[2].Value = actXET.a2mxTable.A2M2.angleLow / 10.0f;
            dataGridView4.Rows[1].Cells[3].Value = actXET.a2mxTable.A2M2.angleHigh / 10.0f;
            dataGridView4.Rows[2].Cells[1].Value = actXET.a2mxTable.A2M3.torquePre / 100.0f;
            dataGridView4.Rows[2].Cells[2].Value = actXET.a2mxTable.A2M3.angleLow / 10.0f;
            dataGridView4.Rows[2].Cells[3].Value = actXET.a2mxTable.A2M3.angleHigh / 10.0f;
            dataGridView4.Rows[3].Cells[1].Value = actXET.a2mxTable.A2M4.torquePre / 100.0f;
            dataGridView4.Rows[3].Cells[2].Value = actXET.a2mxTable.A2M4.angleLow / 10.0f;
            dataGridView4.Rows[3].Cells[3].Value = actXET.a2mxTable.A2M4.angleHigh / 10.0f;
            dataGridView4.Rows[4].Cells[1].Value = actXET.a2mxTable.A2M5.torquePre / 100.0f;
            dataGridView4.Rows[4].Cells[2].Value = actXET.a2mxTable.A2M5.angleLow / 10.0f;
            dataGridView4.Rows[4].Cells[3].Value = actXET.a2mxTable.A2M5.angleHigh / 10.0f;
            dataGridView4.Rows[5].Cells[1].Value = actXET.a2mxTable.A2M6.torquePre / 100.0f;
            dataGridView4.Rows[5].Cells[2].Value = actXET.a2mxTable.A2M6.angleLow / 10.0f;
            dataGridView4.Rows[5].Cells[3].Value = actXET.a2mxTable.A2M6.angleHigh / 10.0f;
            dataGridView4.Rows[6].Cells[1].Value = actXET.a2mxTable.A2M7.torquePre / 100.0f;
            dataGridView4.Rows[6].Cells[2].Value = actXET.a2mxTable.A2M7.angleLow / 10.0f;
            dataGridView4.Rows[6].Cells[3].Value = actXET.a2mxTable.A2M7.angleHigh / 10.0f;
            dataGridView4.Rows[7].Cells[1].Value = actXET.a2mxTable.A2M8.torquePre / 100.0f;
            dataGridView4.Rows[7].Cells[2].Value = actXET.a2mxTable.A2M8.angleLow / 10.0f;
            dataGridView4.Rows[7].Cells[3].Value = actXET.a2mxTable.A2M8.angleHigh / 10.0f;
            dataGridView4.Rows[8].Cells[1].Value = actXET.a2mxTable.A2M9.torquePre / 100.0f;
            dataGridView4.Rows[8].Cells[2].Value = actXET.a2mxTable.A2M9.angleLow / 10.0f;
            dataGridView4.Rows[8].Cells[3].Value = actXET.a2mxTable.A2M9.angleHigh / 10.0f;
        }

        //初始化A2M0表
        private void InitTableA1M0()
        {
            //属性
            dataGridView1.Size = new System.Drawing.Size(313, 83);
            dataGridView1.EditMode = DataGridViewEditMode.EditOnEnter;
            dataGridView1.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.LightSkyBlue;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersHeight = 40;
            dataGridView1.RowTemplate.Height = 40;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToOrderColumns = false;
            dataGridView1.AllowUserToResizeColumns = false;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.Font = new Font("Arial", 12, FontStyle.Bold);

            //加列
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
            dataGridView1.Columns[0].Width = 100;
            dataGridView1.Columns[1].Width = 210;
            dataGridView1.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;

            //标题
            //dataGridView1.Columns[0].HeaderText = "模 式";
            //dataGridView1.Columns[1].HeaderText = "目标扭矩(" + unit + ")";
            dataGridView1.Columns[1].HeaderText = MyDevice.languageType == 0 ? "目标扭矩(" + unit + ")" : "Target torque(" + unit + ")";

            //加行
            dataGridView1.Rows.Add();
            dataGridView1.Rows[0].Cells[0].Value = "A1 M0";
            dataGridView1.Rows[0].Cells[0].ReadOnly = true;
            dataGridView1.Rows[0].Cells[1].Value = actXET.a1mxTable.A1M0.torqueTarget / 100.0f;
        }

        //初始化A1MX表
        private void InitTableA1MX()
        {
            //属性
            dataGridView2.Size = new System.Drawing.Size(523, 403);
            dataGridView2.EditMode = DataGridViewEditMode.EditOnEnter;
            dataGridView2.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView2.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView2.ColumnHeadersDefaultCellStyle.BackColor = Color.LightSkyBlue;
            dataGridView2.EnableHeadersVisualStyles = false;
            dataGridView2.ColumnHeadersHeight = 40;
            dataGridView2.RowTemplate.Height = 40;
            dataGridView2.AllowUserToAddRows = false;
            dataGridView2.AllowUserToDeleteRows = false;
            dataGridView2.AllowUserToOrderColumns = false;
            dataGridView2.AllowUserToResizeColumns = false;
            dataGridView2.AllowUserToResizeRows = false;
            dataGridView2.RowHeadersVisible = false;
            dataGridView2.Font = new Font("Arial", 12, FontStyle.Bold);

            //加列
            dataGridView2.Columns.Add(new DataGridViewTextBoxColumn());
            dataGridView2.Columns.Add(new DataGridViewTextBoxColumn());
            dataGridView2.Columns.Add(new DataGridViewTextBoxColumn());
            dataGridView2.Columns[0].Width = 100;
            dataGridView2.Columns[1].Width = 210;
            dataGridView2.Columns[2].Width = 210;
            dataGridView2.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView2.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView2.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;

            //标题
            //dataGridView2.Columns[0].HeaderText = "模 式";
            //dataGridView2.Columns[1].HeaderText = "扭矩下限(" + unit + ")";
            //dataGridView2.Columns[2].HeaderText = "扭矩上限(" + unit + ")";
            dataGridView2.Columns[1].HeaderText = MyDevice.languageType == 0 ? "扭矩下限(" + unit + ")" : "Lower torque limit(" + unit + ")";
            dataGridView2.Columns[2].HeaderText = MyDevice.languageType == 0 ? "扭矩上限(" + unit + ")" : "Upper torque limit(" + unit + ")";

            //加行
            dataGridView2.Rows.Add();
            dataGridView2.Rows[0].Cells[0].Value = "A1 M1";
            dataGridView2.Rows[0].Cells[0].ReadOnly = true;
            dataGridView2.Rows[0].Cells[1].Value = actXET.a1mxTable.A1M1.torqueLow / 100.0f;
            dataGridView2.Rows[0].Cells[2].Value = actXET.a1mxTable.A1M1.torqueHigh / 100.0f;

            //加行
            dataGridView2.Rows.Add();
            dataGridView2.Rows[1].Cells[0].Value = "A1 M2";
            dataGridView2.Rows[1].Cells[0].ReadOnly = true;
            dataGridView2.Rows[1].Cells[1].Value = actXET.a1mxTable.A1M2.torqueLow / 100.0f;
            dataGridView2.Rows[1].Cells[2].Value = actXET.a1mxTable.A1M2.torqueHigh / 100.0f;

            //加行
            dataGridView2.Rows.Add();
            dataGridView2.Rows[2].Cells[0].Value = "A1 M3";
            dataGridView2.Rows[2].Cells[0].ReadOnly = true;
            dataGridView2.Rows[2].Cells[1].Value = actXET.a1mxTable.A1M3.torqueLow / 100.0f;
            dataGridView2.Rows[2].Cells[2].Value = actXET.a1mxTable.A1M3.torqueHigh / 100.0f;

            //加行
            dataGridView2.Rows.Add();
            dataGridView2.Rows[3].Cells[0].Value = "A1 M4";
            dataGridView2.Rows[3].Cells[0].ReadOnly = true;
            dataGridView2.Rows[3].Cells[1].Value = actXET.a1mxTable.A1M4.torqueLow / 100.0f;
            dataGridView2.Rows[3].Cells[2].Value = actXET.a1mxTable.A1M4.torqueHigh / 100.0f;

            //加行
            dataGridView2.Rows.Add();
            dataGridView2.Rows[4].Cells[0].Value = "A1 M5";
            dataGridView2.Rows[4].Cells[0].ReadOnly = true;
            dataGridView2.Rows[4].Cells[1].Value = actXET.a1mxTable.A1M5.torqueLow / 100.0f;
            dataGridView2.Rows[4].Cells[2].Value = actXET.a1mxTable.A1M5.torqueHigh / 100.0f;

            //加行
            dataGridView2.Rows.Add();
            dataGridView2.Rows[5].Cells[0].Value = "A1 M6";
            dataGridView2.Rows[5].Cells[0].ReadOnly = true;
            dataGridView2.Rows[5].Cells[1].Value = actXET.a1mxTable.A1M6.torqueLow / 100.0f;
            dataGridView2.Rows[5].Cells[2].Value = actXET.a1mxTable.A1M6.torqueHigh / 100.0f;

            //加行
            dataGridView2.Rows.Add();
            dataGridView2.Rows[6].Cells[0].Value = "A1 M7";
            dataGridView2.Rows[6].Cells[0].ReadOnly = true;
            dataGridView2.Rows[6].Cells[1].Value = actXET.a1mxTable.A1M7.torqueLow / 100.0f;
            dataGridView2.Rows[6].Cells[2].Value = actXET.a1mxTable.A1M7.torqueHigh / 100.0f;

            //加行
            dataGridView2.Rows.Add();
            dataGridView2.Rows[7].Cells[0].Value = "A1 M8";
            dataGridView2.Rows[7].Cells[0].ReadOnly = true;
            dataGridView2.Rows[7].Cells[1].Value = actXET.a1mxTable.A1M8.torqueLow / 100.0f;
            dataGridView2.Rows[7].Cells[2].Value = actXET.a1mxTable.A1M8.torqueHigh / 100.0f;

            //加行
            dataGridView2.Rows.Add();
            dataGridView2.Rows[8].Cells[0].Value = "A1 M9";
            dataGridView2.Rows[8].Cells[0].ReadOnly = true;
            dataGridView2.Rows[8].Cells[1].Value = actXET.a1mxTable.A1M9.torqueLow / 100.0f;
            dataGridView2.Rows[8].Cells[2].Value = actXET.a1mxTable.A1M9.torqueHigh / 100.0f;
        }

        //初始化A2M0表
        private void InitTableA2M0()
        {
            //属性
            dataGridView3.Size = new System.Drawing.Size(503, 83);
            dataGridView3.EditMode = DataGridViewEditMode.EditOnEnter;
            dataGridView3.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView3.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView3.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView3.ColumnHeadersDefaultCellStyle.BackColor = Color.LightSkyBlue;
            dataGridView3.EnableHeadersVisualStyles = false;
            dataGridView3.ColumnHeadersHeight = 40;
            dataGridView3.RowTemplate.Height = 40;
            dataGridView3.AllowUserToAddRows = false;
            dataGridView3.AllowUserToDeleteRows = false;
            dataGridView3.AllowUserToOrderColumns = false;
            dataGridView3.AllowUserToResizeColumns = false;
            dataGridView3.AllowUserToResizeRows = false;
            dataGridView3.RowHeadersVisible = false;
            dataGridView3.Font = new Font("Arial", 12, FontStyle.Bold);

            //加列
            dataGridView3.Columns.Add(new DataGridViewTextBoxColumn());
            dataGridView3.Columns.Add(new DataGridViewTextBoxColumn());
            dataGridView3.Columns.Add(new DataGridViewTextBoxColumn());
            dataGridView3.Columns[0].Width = 100;
            dataGridView3.Columns[1].Width = 200;
            dataGridView3.Columns[2].Width = 200;
            dataGridView3.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView3.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView3.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;

            //标题
            //dataGridView3.Columns[0].HeaderText = "模 式";
            //dataGridView3.Columns[1].HeaderText = "预设扭矩(" + unit + ")";
            //dataGridView3.Columns[2].HeaderText = "目标角度°";
            dataGridView3.Columns[1].HeaderText = MyDevice.languageType == 0 ? "预设扭矩(" + unit + ")" : "Preset torque(" + unit + ")";

            //加行
            dataGridView3.Rows.Add();
            dataGridView3.Rows[0].Cells[0].Value = "A2 M0";
            dataGridView3.Rows[0].Cells[0].ReadOnly = true;
            dataGridView3.Rows[0].Cells[1].Value = actXET.a2mxTable.A2M0.torquePre / 100.0f;
            dataGridView3.Rows[0].Cells[2].Value = actXET.a2mxTable.A2M0.angleTarget / 10.0f;
        }

        //初始化A2MX表
        private void InitTableA2MX()
        {
            //属性
            dataGridView4.Size = new System.Drawing.Size(703, 403);
            dataGridView4.EditMode = DataGridViewEditMode.EditOnEnter;
            dataGridView4.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView4.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView4.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView4.ColumnHeadersDefaultCellStyle.BackColor = Color.LightSkyBlue;
            dataGridView4.EnableHeadersVisualStyles = false;
            dataGridView4.ColumnHeadersHeight = 40;
            dataGridView4.RowTemplate.Height = 40;
            dataGridView4.AllowUserToAddRows = false;
            dataGridView4.AllowUserToDeleteRows = false;
            dataGridView4.AllowUserToOrderColumns = false;
            dataGridView4.AllowUserToResizeColumns = false;
            dataGridView4.AllowUserToResizeRows = false;
            dataGridView4.RowHeadersVisible = false;
            dataGridView4.Font = new Font("Arial", 12, FontStyle.Bold);

            //加列
            dataGridView4.Columns.Add(new DataGridViewTextBoxColumn());
            dataGridView4.Columns.Add(new DataGridViewTextBoxColumn());
            dataGridView4.Columns.Add(new DataGridViewTextBoxColumn());
            dataGridView4.Columns.Add(new DataGridViewTextBoxColumn());
            dataGridView4.Columns[0].Width = 100;
            dataGridView4.Columns[1].Width = 200;
            dataGridView4.Columns[2].Width = 200;
            dataGridView4.Columns[3].Width = 200;
            dataGridView4.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView4.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView4.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView4.Columns[3].SortMode = DataGridViewColumnSortMode.NotSortable;

            //标题
            //dataGridView4.Columns[0].HeaderText = "模 式";
            //dataGridView4.Columns[1].HeaderText = "预设扭矩(" + unit + ")";
            //dataGridView4.Columns[2].HeaderText = "角度下限°";
            //dataGridView4.Columns[3].HeaderText = "目标上限°";
            dataGridView4.Columns[1].HeaderText = MyDevice.languageType == 0 ? "预设扭矩(" + unit + ")" : "Preset torque(" + unit + ")"; ;

            //加行
            dataGridView4.Rows.Add();
            dataGridView4.Rows[0].Cells[0].Value = "A2 M1";
            dataGridView4.Rows[0].Cells[0].ReadOnly = true;
            dataGridView4.Rows[0].Cells[1].Value = actXET.a2mxTable.A2M1.torquePre / 100.0f;
            dataGridView4.Rows[0].Cells[2].Value = actXET.a2mxTable.A2M1.angleLow / 10.0f;
            dataGridView4.Rows[0].Cells[3].Value = actXET.a2mxTable.A2M1.angleHigh / 10.0f;

            //加行
            dataGridView4.Rows.Add();
            dataGridView4.Rows[1].Cells[0].Value = "A2 M2";
            dataGridView4.Rows[1].Cells[0].ReadOnly = true;
            dataGridView4.Rows[1].Cells[1].Value = actXET.a2mxTable.A2M2.torquePre / 100.0f;
            dataGridView4.Rows[1].Cells[2].Value = actXET.a2mxTable.A2M2.angleLow / 10.0f;
            dataGridView4.Rows[1].Cells[3].Value = actXET.a2mxTable.A2M2.angleHigh / 10.0f;

            //加行
            dataGridView4.Rows.Add();
            dataGridView4.Rows[2].Cells[0].Value = "A2 M3";
            dataGridView4.Rows[2].Cells[0].ReadOnly = true;
            dataGridView4.Rows[2].Cells[1].Value = actXET.a2mxTable.A2M3.torquePre / 100.0f;
            dataGridView4.Rows[2].Cells[2].Value = actXET.a2mxTable.A2M3.angleLow / 10.0f;
            dataGridView4.Rows[2].Cells[3].Value = actXET.a2mxTable.A2M3.angleHigh / 10.0f;

            //加行
            dataGridView4.Rows.Add();
            dataGridView4.Rows[3].Cells[0].Value = "A2 M4";
            dataGridView4.Rows[3].Cells[0].ReadOnly = true;
            dataGridView4.Rows[3].Cells[1].Value = actXET.a2mxTable.A2M4.torquePre / 100.0f;
            dataGridView4.Rows[3].Cells[2].Value = actXET.a2mxTable.A2M4.angleLow / 10.0f;
            dataGridView4.Rows[3].Cells[3].Value = actXET.a2mxTable.A2M4.angleHigh / 10.0f;

            //加行
            dataGridView4.Rows.Add();
            dataGridView4.Rows[4].Cells[0].Value = "A2 M5";
            dataGridView4.Rows[4].Cells[0].ReadOnly = true;
            dataGridView4.Rows[4].Cells[1].Value = actXET.a2mxTable.A2M5.torquePre / 100.0f;
            dataGridView4.Rows[4].Cells[2].Value = actXET.a2mxTable.A2M5.angleLow / 10.0f;
            dataGridView4.Rows[4].Cells[3].Value = actXET.a2mxTable.A2M5.angleHigh / 10.0f;

            //加行
            dataGridView4.Rows.Add();
            dataGridView4.Rows[5].Cells[0].Value = "A2 M6";
            dataGridView4.Rows[5].Cells[0].ReadOnly = true;
            dataGridView4.Rows[5].Cells[1].Value = actXET.a2mxTable.A2M6.torquePre / 100.0f;
            dataGridView4.Rows[5].Cells[2].Value = actXET.a2mxTable.A2M6.angleLow / 10.0f;
            dataGridView4.Rows[5].Cells[3].Value = actXET.a2mxTable.A2M6.angleHigh / 10.0f;

            //加行
            dataGridView4.Rows.Add();
            dataGridView4.Rows[6].Cells[0].Value = "A2 M7";
            dataGridView4.Rows[6].Cells[0].ReadOnly = true;
            dataGridView4.Rows[6].Cells[1].Value = actXET.a2mxTable.A2M7.torquePre / 100.0f;
            dataGridView4.Rows[6].Cells[2].Value = actXET.a2mxTable.A2M7.angleLow / 10.0f;
            dataGridView4.Rows[6].Cells[3].Value = actXET.a2mxTable.A2M7.angleHigh / 10.0f;

            //加行
            dataGridView4.Rows.Add();
            dataGridView4.Rows[7].Cells[0].Value = "A2 M8";
            dataGridView4.Rows[7].Cells[0].ReadOnly = true;
            dataGridView4.Rows[7].Cells[1].Value = actXET.a2mxTable.A2M8.torquePre / 100.0f;
            dataGridView4.Rows[7].Cells[2].Value = actXET.a2mxTable.A2M8.angleLow / 10.0f;
            dataGridView4.Rows[7].Cells[3].Value = actXET.a2mxTable.A2M8.angleHigh / 10.0f;

            //加行
            dataGridView4.Rows.Add();
            dataGridView4.Rows[8].Cells[0].Value = "A2 M9";
            dataGridView4.Rows[8].Cells[0].ReadOnly = true;
            dataGridView4.Rows[8].Cells[1].Value = actXET.a2mxTable.A2M9.torquePre / 100.0f;
            dataGridView4.Rows[8].Cells[2].Value = actXET.a2mxTable.A2M9.angleLow / 10.0f;
            dataGridView4.Rows[8].Cells[3].Value = actXET.a2mxTable.A2M9.angleHigh / 10.0f;
        }

        //更新A1Mx
        private void button1_Click(object sender, EventArgs e)
        {
            if (ucDataGridView1.SelectRows.Count == 0)
            {
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("请选择至少一个设备！");
                }
                else
                {
                    MessageBox.Show("Please select at least one device！");
                }
                return;
            }

            //if (actXET.isActive == false) return;

            //更新设备参数
            for (int i = 0; i < ucDataGridView1.SelectRows.Count; i++)
            {
                //获取已选设备地址
                if (MyDevice.protocol.type != COMP.SelfUART)
                {
                    MyDevice.protocol.addr = mutiAddres[ucDataGridView1.SelectRows[i].RowIndex];
                    //net模式下ip地址需要更换，即需要更换port
                    if (MyDevice.protocol.type == COMP.NET)
                    {
                        MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
                    }
                    actXET = MyDevice.actDev;
                }

                actXET.a1mxTable.A1M0.torqueTarget = (UInt32)(Convert.ToDouble(dataGridView1.Rows[0].Cells[1].Value.ToString()) * 100 + 0.5);
                actXET.a1mxTable.A1M1.torqueLow = (UInt32)(Convert.ToDouble(dataGridView2.Rows[0].Cells[1].Value.ToString()) * 100 + 0.5);
                actXET.a1mxTable.A1M1.torqueHigh = (UInt32)(Convert.ToDouble(dataGridView2.Rows[0].Cells[2].Value.ToString()) * 100 + 0.5);
                actXET.a1mxTable.A1M2.torqueLow = (UInt32)(Convert.ToDouble(dataGridView2.Rows[1].Cells[1].Value.ToString()) * 100 + 0.5);
                actXET.a1mxTable.A1M2.torqueHigh = (UInt32)(Convert.ToDouble(dataGridView2.Rows[1].Cells[2].Value.ToString()) * 100 + 0.5);
                actXET.a1mxTable.A1M3.torqueLow = (UInt32)(Convert.ToDouble(dataGridView2.Rows[2].Cells[1].Value.ToString()) * 100 + 0.5);
                actXET.a1mxTable.A1M3.torqueHigh = (UInt32)(Convert.ToDouble(dataGridView2.Rows[2].Cells[2].Value.ToString()) * 100 + 0.5);
                actXET.a1mxTable.A1M4.torqueLow = (UInt32)(Convert.ToDouble(dataGridView2.Rows[3].Cells[1].Value.ToString()) * 100 + 0.5);
                actXET.a1mxTable.A1M4.torqueHigh = (UInt32)(Convert.ToDouble(dataGridView2.Rows[3].Cells[2].Value.ToString()) * 100 + 0.5);
                actXET.a1mxTable.A1M5.torqueLow = (UInt32)(Convert.ToDouble(dataGridView2.Rows[4].Cells[1].Value.ToString()) * 100 + 0.5);
                actXET.a1mxTable.A1M5.torqueHigh = (UInt32)(Convert.ToDouble(dataGridView2.Rows[4].Cells[2].Value.ToString()) * 100 + 0.5);
                actXET.a1mxTable.A1M6.torqueLow = (UInt32)(Convert.ToDouble(dataGridView2.Rows[5].Cells[1].Value.ToString()) * 100 + 0.5);
                actXET.a1mxTable.A1M6.torqueHigh = (UInt32)(Convert.ToDouble(dataGridView2.Rows[5].Cells[2].Value.ToString()) * 100 + 0.5);
                actXET.a1mxTable.A1M7.torqueLow = (UInt32)(Convert.ToDouble(dataGridView2.Rows[6].Cells[1].Value.ToString()) * 100 + 0.5);
                actXET.a1mxTable.A1M7.torqueHigh = (UInt32)(Convert.ToDouble(dataGridView2.Rows[6].Cells[2].Value.ToString()) * 100 + 0.5);
                actXET.a1mxTable.A1M8.torqueLow = (UInt32)(Convert.ToDouble(dataGridView2.Rows[7].Cells[1].Value.ToString()) * 100 + 0.5);
                actXET.a1mxTable.A1M8.torqueHigh = (UInt32)(Convert.ToDouble(dataGridView2.Rows[7].Cells[2].Value.ToString()) * 100 + 0.5);
                actXET.a1mxTable.A1M9.torqueLow = (UInt32)(Convert.ToDouble(dataGridView2.Rows[8].Cells[1].Value.ToString()) * 100 + 0.5);
                actXET.a1mxTable.A1M9.torqueHigh = (UInt32)(Convert.ToDouble(dataGridView2.Rows[8].Cells[2].Value.ToString()) * 100 + 0.5);

                actXET.a1mxTable.A1M0.torqueTarget = getDataCheck(actXET.a1mxTable.A1M0.torqueTarget, actXET.torqueMinima, actXET.torqueCapacity);
                actXET.a1mxTable.A1M1.torqueLow = getDataCheck(actXET.a1mxTable.A1M1.torqueLow, actXET.torqueMinima, actXET.torqueCapacity);
                actXET.a1mxTable.A1M1.torqueHigh = getDataCheck(actXET.a1mxTable.A1M1.torqueHigh, actXET.a1mxTable.A1M1.torqueLow, actXET.torqueCapacity);
                actXET.a1mxTable.A1M2.torqueLow = getDataCheck(actXET.a1mxTable.A1M2.torqueLow, actXET.torqueMinima, actXET.torqueCapacity);
                actXET.a1mxTable.A1M2.torqueHigh = getDataCheck(actXET.a1mxTable.A1M2.torqueHigh, actXET.a1mxTable.A1M2.torqueLow, actXET.torqueCapacity);
                actXET.a1mxTable.A1M3.torqueLow = getDataCheck(actXET.a1mxTable.A1M3.torqueLow, actXET.torqueMinima, actXET.torqueCapacity);
                actXET.a1mxTable.A1M3.torqueHigh = getDataCheck(actXET.a1mxTable.A1M3.torqueHigh, actXET.a1mxTable.A1M3.torqueLow, actXET.torqueCapacity);
                actXET.a1mxTable.A1M4.torqueLow = getDataCheck(actXET.a1mxTable.A1M4.torqueLow, actXET.torqueMinima, actXET.torqueCapacity);
                actXET.a1mxTable.A1M4.torqueHigh = getDataCheck(actXET.a1mxTable.A1M4.torqueHigh, actXET.a1mxTable.A1M4.torqueLow, actXET.torqueCapacity);
                actXET.a1mxTable.A1M5.torqueLow = getDataCheck(actXET.a1mxTable.A1M5.torqueLow, actXET.torqueMinima, actXET.torqueCapacity);
                actXET.a1mxTable.A1M5.torqueHigh = getDataCheck(actXET.a1mxTable.A1M5.torqueHigh, actXET.a1mxTable.A1M5.torqueLow, actXET.torqueCapacity);
                actXET.a1mxTable.A1M6.torqueLow = getDataCheck(actXET.a1mxTable.A1M6.torqueLow, actXET.torqueMinima, actXET.torqueCapacity);
                actXET.a1mxTable.A1M6.torqueHigh = getDataCheck(actXET.a1mxTable.A1M6.torqueHigh, actXET.a1mxTable.A1M6.torqueLow, actXET.torqueCapacity);
                actXET.a1mxTable.A1M7.torqueLow = getDataCheck(actXET.a1mxTable.A1M7.torqueLow, actXET.torqueMinima, actXET.torqueCapacity);
                actXET.a1mxTable.A1M7.torqueHigh = getDataCheck(actXET.a1mxTable.A1M7.torqueHigh, actXET.a1mxTable.A1M7.torqueLow, actXET.torqueCapacity);
                actXET.a1mxTable.A1M8.torqueLow = getDataCheck(actXET.a1mxTable.A1M8.torqueLow, actXET.torqueMinima, actXET.torqueCapacity);
                actXET.a1mxTable.A1M8.torqueHigh = getDataCheck(actXET.a1mxTable.A1M8.torqueHigh, actXET.a1mxTable.A1M8.torqueLow, actXET.torqueCapacity);
                actXET.a1mxTable.A1M9.torqueLow = getDataCheck(actXET.a1mxTable.A1M9.torqueLow, actXET.torqueMinima, actXET.torqueCapacity);
                actXET.a1mxTable.A1M9.torqueHigh = getDataCheck(actXET.a1mxTable.A1M9.torqueHigh, actXET.a1mxTable.A1M9.torqueLow, actXET.torqueCapacity);
            }

            //按键状态
            button1.BackColor = Color.Firebrick;
            button2.BackColor = Color.Transparent;
            button3.BackColor = Color.Transparent;
            button4.BackColor = Color.Transparent;

            if (MyDevice.protocol.type != COMP.SelfUART)
            {
                addrIndex = 0;
                MyDevice.protocol.addr = mutiAddres[ucDataGridView1.SelectRows[addrIndex].RowIndex];
                //net模式下ip地址需要更换，即需要更换port
                if (MyDevice.protocol.type == COMP.NET)
                {
                    MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
                }
                actXET = MyDevice.actDev;
                button1.Text = MyDevice.languageType == 0 ? "设备" + MyDevice.protocol.addr.ToString() : "Device" + MyDevice.protocol.addr.ToString();
            }

            //写A1MxDAT
            nextTask = TASKS.WRITE_A1M01DAT;
            MyDevice.protocol.Protocol_ClearState();
            MyDevice.protocol.Protocol_WriteA1MXTasks();

            actXET.oldAx = 0xFF;
            actXET.oldMx = 0xFF;
            actXET.oldTU = 0x00;
        }

        //更新A2Mx
        private void button2_Click(object sender, EventArgs e)
        {
            if (ucDataGridView1.SelectRows.Count == 0)
            {
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("请选择至少一个设备！");
                }
                else
                {
                    MessageBox.Show("Please select at least one device！");
                }
                return;
            }

            //if (actXET.isActive == false) return;

            //更新设备参数
            for (int i = 0; i < ucDataGridView1.SelectRows.Count; i++)
            {
                //获取已选设备地址
                if (MyDevice.protocol.type != COMP.SelfUART)
                {
                    MyDevice.protocol.addr = mutiAddres[ucDataGridView1.SelectRows[i].RowIndex];
                    //net模式下ip地址需要更换，即需要更换port
                    if (MyDevice.protocol.type == COMP.NET)
                    {
                        MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
                    }
                    actXET = MyDevice.actDev;
                }

                actXET.a2mxTable.A2M0.torquePre = (UInt32)(Convert.ToDouble(dataGridView3.Rows[0].Cells[1].Value.ToString()) * 100 + 0.5);
                actXET.a2mxTable.A2M0.angleTarget = (UInt16)(Convert.ToDouble(dataGridView3.Rows[0].Cells[2].Value.ToString()) * 10 + 0.5);
                actXET.a2mxTable.A2M1.torquePre = (UInt32)(Convert.ToDouble(dataGridView4.Rows[0].Cells[1].Value.ToString()) * 100 + 0.5);
                actXET.a2mxTable.A2M1.angleLow = (UInt16)(Convert.ToDouble(dataGridView4.Rows[0].Cells[2].Value.ToString()) * 10 + 0.5);
                actXET.a2mxTable.A2M1.angleHigh = (UInt16)(Convert.ToDouble(dataGridView4.Rows[0].Cells[3].Value.ToString()) * 10 + 0.5);
                actXET.a2mxTable.A2M2.torquePre = (UInt32)(Convert.ToDouble(dataGridView4.Rows[1].Cells[1].Value.ToString()) * 100 + 0.5);
                actXET.a2mxTable.A2M2.angleLow = (UInt16)(Convert.ToDouble(dataGridView4.Rows[1].Cells[2].Value.ToString()) * 10 + 0.5);
                actXET.a2mxTable.A2M2.angleHigh = (UInt16)(Convert.ToDouble(dataGridView4.Rows[1].Cells[3].Value.ToString()) * 10 + 0.5);
                actXET.a2mxTable.A2M3.torquePre = (UInt32)(Convert.ToDouble(dataGridView4.Rows[2].Cells[1].Value.ToString()) * 100 + 0.5);
                actXET.a2mxTable.A2M3.angleLow = (UInt16)(Convert.ToDouble(dataGridView4.Rows[2].Cells[2].Value.ToString()) * 10 + 0.5);
                actXET.a2mxTable.A2M3.angleHigh = (UInt16)(Convert.ToDouble(dataGridView4.Rows[2].Cells[3].Value.ToString()) * 10 + 0.5);
                actXET.a2mxTable.A2M4.torquePre = (UInt32)(Convert.ToDouble(dataGridView4.Rows[3].Cells[1].Value.ToString()) * 100 + 0.5);
                actXET.a2mxTable.A2M4.angleLow = (UInt16)(Convert.ToDouble(dataGridView4.Rows[3].Cells[2].Value.ToString()) * 10 + 0.5);
                actXET.a2mxTable.A2M4.angleHigh = (UInt16)(Convert.ToDouble(dataGridView4.Rows[3].Cells[3].Value.ToString()) * 10 + 0.5);
                actXET.a2mxTable.A2M5.torquePre = (UInt32)(Convert.ToDouble(dataGridView4.Rows[4].Cells[1].Value.ToString()) * 100 + 0.5);
                actXET.a2mxTable.A2M5.angleLow = (UInt16)(Convert.ToDouble(dataGridView4.Rows[4].Cells[2].Value.ToString()) * 10 + 0.5);
                actXET.a2mxTable.A2M5.angleHigh = (UInt16)(Convert.ToDouble(dataGridView4.Rows[4].Cells[3].Value.ToString()) * 10 + 0.5);
                actXET.a2mxTable.A2M6.torquePre = (UInt32)(Convert.ToDouble(dataGridView4.Rows[5].Cells[1].Value.ToString()) * 100 + 0.5);
                actXET.a2mxTable.A2M6.angleLow = (UInt16)(Convert.ToDouble(dataGridView4.Rows[5].Cells[2].Value.ToString()) * 10 + 0.5);
                actXET.a2mxTable.A2M6.angleHigh = (UInt16)(Convert.ToDouble(dataGridView4.Rows[5].Cells[3].Value.ToString()) * 10 + 0.5);
                actXET.a2mxTable.A2M7.torquePre = (UInt32)(Convert.ToDouble(dataGridView4.Rows[6].Cells[1].Value.ToString()) * 100 + 0.5);
                actXET.a2mxTable.A2M7.angleLow = (UInt16)(Convert.ToDouble(dataGridView4.Rows[6].Cells[2].Value.ToString()) * 10 + 0.5);
                actXET.a2mxTable.A2M7.angleHigh = (UInt16)(Convert.ToDouble(dataGridView4.Rows[6].Cells[3].Value.ToString()) * 10 + 0.5);
                actXET.a2mxTable.A2M8.torquePre = (UInt32)(Convert.ToDouble(dataGridView4.Rows[7].Cells[1].Value.ToString()) * 100 + 0.5);
                actXET.a2mxTable.A2M8.angleLow = (UInt16)(Convert.ToDouble(dataGridView4.Rows[7].Cells[2].Value.ToString()) * 10 + 0.5);
                actXET.a2mxTable.A2M8.angleHigh = (UInt16)(Convert.ToDouble(dataGridView4.Rows[7].Cells[3].Value.ToString()) * 10 + 0.5);
                actXET.a2mxTable.A2M9.torquePre = (UInt32)(Convert.ToDouble(dataGridView4.Rows[8].Cells[1].Value.ToString()) * 100 + 0.5);
                actXET.a2mxTable.A2M9.angleLow = (UInt16)(Convert.ToDouble(dataGridView4.Rows[8].Cells[2].Value.ToString()) * 10 + 0.5);
                actXET.a2mxTable.A2M9.angleHigh = (UInt16)(Convert.ToDouble(dataGridView4.Rows[8].Cells[3].Value.ToString()) * 10 + 0.5);

                actXET.a2mxTable.A2M0.torquePre = getDataCheck(actXET.a2mxTable.A2M0.torquePre, actXET.torqueMinima, actXET.torqueCapacity);
                actXET.a2mxTable.A2M0.angleTarget = getDataCheck(actXET.a2mxTable.A2M0.angleTarget, 0, 3600);
                actXET.a2mxTable.A2M1.torquePre = getDataCheck(actXET.a2mxTable.A2M1.torquePre, actXET.torqueMinima, actXET.torqueCapacity);
                actXET.a2mxTable.A2M1.angleLow = getDataCheck(actXET.a2mxTable.A2M1.angleLow, 0, 3600);
                actXET.a2mxTable.A2M1.angleHigh = getDataCheck(actXET.a2mxTable.A2M1.angleHigh, actXET.a2mxTable.A2M1.angleLow, 3600);
                actXET.a2mxTable.A2M2.torquePre = getDataCheck(actXET.a2mxTable.A2M2.torquePre, actXET.torqueMinima, actXET.torqueCapacity);
                actXET.a2mxTable.A2M2.angleLow = getDataCheck(actXET.a2mxTable.A2M2.angleLow, 0, 3600);
                actXET.a2mxTable.A2M2.angleHigh = getDataCheck(actXET.a2mxTable.A2M2.angleHigh, actXET.a2mxTable.A2M2.angleLow, 3600);
                actXET.a2mxTable.A2M3.torquePre = getDataCheck(actXET.a2mxTable.A2M3.torquePre, actXET.torqueMinima, actXET.torqueCapacity);
                actXET.a2mxTable.A2M3.angleLow = getDataCheck(actXET.a2mxTable.A2M3.angleLow, 0, 3600);
                actXET.a2mxTable.A2M3.angleHigh = getDataCheck(actXET.a2mxTable.A2M3.angleHigh, actXET.a2mxTable.A2M3.angleLow, 3600);
                actXET.a2mxTable.A2M4.torquePre = getDataCheck(actXET.a2mxTable.A2M4.torquePre, actXET.torqueMinima, actXET.torqueCapacity);
                actXET.a2mxTable.A2M4.angleLow = getDataCheck(actXET.a2mxTable.A2M4.angleLow, 0, 3600);
                actXET.a2mxTable.A2M4.angleHigh = getDataCheck(actXET.a2mxTable.A2M4.angleHigh, actXET.a2mxTable.A2M4.angleLow, 3600);
                actXET.a2mxTable.A2M5.torquePre = getDataCheck(actXET.a2mxTable.A2M5.torquePre, actXET.torqueMinima, actXET.torqueCapacity);
                actXET.a2mxTable.A2M5.angleLow = getDataCheck(actXET.a2mxTable.A2M5.angleLow, 0, 3600);
                actXET.a2mxTable.A2M5.angleHigh = getDataCheck(actXET.a2mxTable.A2M5.angleHigh, actXET.a2mxTable.A2M5.angleLow, 3600);
                actXET.a2mxTable.A2M6.torquePre = getDataCheck(actXET.a2mxTable.A2M6.torquePre, actXET.torqueMinima, actXET.torqueCapacity);
                actXET.a2mxTable.A2M6.angleLow = getDataCheck(actXET.a2mxTable.A2M6.angleLow, 0, 3600);
                actXET.a2mxTable.A2M6.angleHigh = getDataCheck(actXET.a2mxTable.A2M6.angleHigh, actXET.a2mxTable.A2M6.angleLow, 3600);
                actXET.a2mxTable.A2M7.torquePre = getDataCheck(actXET.a2mxTable.A2M7.torquePre, actXET.torqueMinima, actXET.torqueCapacity);
                actXET.a2mxTable.A2M7.angleLow = getDataCheck(actXET.a2mxTable.A2M7.angleLow, 0, 3600);
                actXET.a2mxTable.A2M7.angleHigh = getDataCheck(actXET.a2mxTable.A2M7.angleHigh, actXET.a2mxTable.A2M7.angleLow, 3600);
                actXET.a2mxTable.A2M8.torquePre = getDataCheck(actXET.a2mxTable.A2M8.torquePre, actXET.torqueMinima, actXET.torqueCapacity);
                actXET.a2mxTable.A2M8.angleLow = getDataCheck(actXET.a2mxTable.A2M8.angleLow, 0, 3600);
                actXET.a2mxTable.A2M8.angleHigh = getDataCheck(actXET.a2mxTable.A2M8.angleHigh, actXET.a2mxTable.A2M8.angleLow, 3600);
                actXET.a2mxTable.A2M9.torquePre = getDataCheck(actXET.a2mxTable.A2M9.torquePre, actXET.torqueMinima, actXET.torqueCapacity);
                actXET.a2mxTable.A2M9.angleLow = getDataCheck(actXET.a2mxTable.A2M9.angleLow, 0, 3600);
                actXET.a2mxTable.A2M9.angleHigh = getDataCheck(actXET.a2mxTable.A2M9.angleHigh, actXET.a2mxTable.A2M9.angleLow, 3600);
            }

            //按键状态
            button2.BackColor = Color.Firebrick;
            button1.BackColor = Color.Transparent;
            button3.BackColor = Color.Transparent;
            button4.BackColor = Color.Transparent;

            if (MyDevice.protocol.type != COMP.SelfUART)
            {
                addrIndex = 0;
                MyDevice.protocol.addr = mutiAddres[ucDataGridView1.SelectRows[addrIndex].RowIndex];
                //net模式下ip地址需要更换，即需要更换port
                if (MyDevice.protocol.type == COMP.NET)
                {
                    MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
                }
                actXET = MyDevice.actDev;
                button2.Text = MyDevice.languageType == 0 ? "设备" + MyDevice.protocol.addr.ToString() : "Device" + MyDevice.protocol.addr.ToString();
            }

            //写A2MxDAT
            nextTask = TASKS.WRITE_A2M01DAT;
            MyDevice.protocol.Protocol_ClearState();
            MyDevice.protocol.Protocol_WriteA2MXTasks();

            actXET.oldAx = 0xFF;
            actXET.oldMx = 0xFF;
            actXET.oldTU = 0x00;
        }

        //保存参数
        private void button4_Click(object sender, EventArgs e)
        {
            if (ucDataGridView1.SelectRows.Count == 0)
            {
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("请选择至少一个设备！");
                }
                else
                {
                    MessageBox.Show("Please select at least one device！");
                }
                return;
            }

            //更新设备参数
            for (int i = 0; i < ucDataGridView1.SelectRows.Count; i++)
            {
                //获取已选设备地址
                if (MyDevice.protocol.type != COMP.SelfUART)
                {
                    MyDevice.protocol.addr = mutiAddres[ucDataGridView1.SelectRows[i].RowIndex];
                    //net模式下ip地址需要更换，即需要更换port
                    if (MyDevice.protocol.type == COMP.NET)
                    {
                        MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
                    }
                    actXET = MyDevice.actDev;
                }

                actXET.modePt = (byte)comboBox1.SelectedIndex;
                actXET.modeAx = (byte)comboBox2.SelectedIndex;
                actXET.modeMx = (byte)comboBox3.SelectedIndex;
                actXET.torqueUnit = (UNIT)comboBox4.SelectedIndex;
                actXET.angleSpeed = comboBox5.SelectedIndex;
                actXET.modeRec = (byte)comboBox6.SelectedIndex;
                actXET.isKeyLock = 0;
                actXET.screwNum = 0;
            }

            //按键状态
            button4.BackColor = Color.Firebrick;
            button1.BackColor = Color.Transparent;
            button2.BackColor = Color.Transparent;
            button3.BackColor = Color.Transparent;

            //获取已选设备地址
            if (MyDevice.protocol.type != COMP.SelfUART)
            {
                addrIndex = 0;
                MyDevice.protocol.addr = mutiAddres[ucDataGridView1.SelectRows[addrIndex].RowIndex];
                if (MyDevice.protocol.type == COMP.NET)
                {
                    MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
                }
                actXET = MyDevice.actDev;
            }

            //写入参数
            nextTask = TASKS.WRITE_PARA;
            MyDevice.protocol.Protocol_ClearState();
            MyDevice.protocol.Protocol_Write_SendCOM(TASKS.WRITE_PARA);
        }

        //清除设备缓存
        private void button3_Click(object sender, EventArgs e)
        {
            if (ucDataGridView1.SelectRows.Count == 0)
            {
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("请选择至少一个设备！");
                }
                else
                {
                    MessageBox.Show("Please select at least one device！");
                }
                return;
            }

            //按键状态
            button3.BackColor = Color.Firebrick;
            button1.BackColor = Color.Transparent;
            button2.BackColor = Color.Transparent;
            button4.BackColor = Color.Transparent;

            //获取已选设备地址
            if (MyDevice.protocol.type != COMP.SelfUART)
            {
                addrIndex = 0;
                MyDevice.protocol.addr = mutiAddres[ucDataGridView1.SelectRows[addrIndex].RowIndex];
                //net模式下ip地址需要更换，即需要更换port
                if (MyDevice.protocol.type == COMP.NET)
                {
                    MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
                }
                actXET = MyDevice.actDev;
            }

            //清除设备缓存
            nextTask = TASKS.WRITE_RECSIZE;
            MyDevice.protocol.Protocol_ClearState();
            MyDevice.protocol.Protocol_Write_SendCOM(TASKS.WRITE_RECSIZE);
        }

        //设备列表选择
        private void ucDataGridView1_ItemClick(object sender, DataGridViewEventArgs e)
        {
            //若只选择1个设备则更新此设备的参数
            if (ucDataGridView1.SelectRows.Count == 1)
            {
                //获取已选设备地址
                if (MyDevice.protocol.type != COMP.SelfUART)
                {
                    MyDevice.protocol.addr = mutiAddres[ucDataGridView1.SelectRows[0].RowIndex];
                    //net模式下ip地址需要更换，即需要更换port
                    if (MyDevice.protocol.type == COMP.NET)
                    {
                        MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
                    }
                    actXET = MyDevice.actDev;
                }

                //更新comboBox参数
                comboBox1.SelectedIndex = actXET.modePt;
                comboBox2.SelectedIndex = actXET.modeAx;
                comboBox3.SelectedIndex = actXET.modeMx;
                comboBox4.SelectedIndex = (int)actXET.torqueUnit;
                comboBox5.SelectedIndex = actXET.angleSpeed;
                comboBox6.SelectedIndex = actXET.modeRec;

                //更新table
                UpdateTableA1MX();
                UpdateTableA2MX();
            }
        }

        //设备列表初始化
        private void ucDataGridView1_Load(object sender, EventArgs e)
        {
            //设备列表控件
            List<DataGridViewColumnEntity> lstCulumns = new List<DataGridViewColumnEntity>();

            //新增"设备列表"列
            if (MyDevice.languageType == 0)
            {
                lstCulumns.Add(new DataGridViewColumnEntity() { DataField = "Device", HeadText = "设备列表", Width = 80, WidthType = SizeType.Absolute });
            }
            else
            {
                lstCulumns.Add(new DataGridViewColumnEntity() { DataField = "Device", HeadText = "DeviceList", Width = 100, WidthType = SizeType.Absolute });
            }
            this.ucDataGridView1.Columns = lstCulumns;
            this.ucDataGridView1.IsShowCheckBox = true;
            List<object> lstSource = new List<object>();

            //单设备
            if(MyDevice.protocol.type == COMP.SelfUART)
            {
                GridModel model = new GridModel()
                {
                    Device = "单设备"
                };
                lstSource.Add(model);
            }
            //多设备
            else
            {
                //将已连接设备的地址存入列表
                for (Byte i = 1; i != 0; i++)
                {
                    if (MyDevice.protocol.type == COMP.XF)
                    {
                        if (MyDevice.mXF[i].sTATE == STATE.CONNECTED)
                        {
                            mutiAddres.Add(i);
                        }
                    }
                    else if (MyDevice.protocol.type == COMP.NET)
                    {
                        if (MyDevice.mNET[i].sTATE == STATE.CONNECTED)
                        {
                            mutiAddres.Add(i);
                        }
                    }
                }

                //根据已连接设备数新增"设备"行 MyDevice.devSum
                for (int i = 0; i < MyDevice.devSum; i++)
                {
                    GridModel model = new GridModel()
                    {
                        Device = MyDevice.languageType == 0  ? "设备" + mutiAddres[i].ToString() : "Device" + mutiAddres[i].ToString()
                    };
                    lstSource.Add(model);
                }
            }

            this.ucDataGridView1.DataSource = lstSource;
        }

        //
        private UInt32 getDataCheck(UInt32 dat, UInt32 low, UInt32 high)
        {
            return (dat > low) ? ((dat > high) ? high : dat) : low;
        }

        //
        private UInt16 getDataCheck(UInt16 dat, UInt16 low, UInt16 high)
        {
            return (dat > low) ? ((dat > high) ? high : dat) : low;
        }

        //委托
        private void receivePara()
        {
            //其它线程的操作请求
            if (this.InvokeRequired)
            {
                try
                {
                    freshHandler meDelegate = new freshHandler(receivePara);
                    this.Invoke(meDelegate, new object[] { });
                }
                catch
                {
                    //MessageBox.Show("MenuCalForm receiveData err 2");
                }
            }
            //本线程的操作请求
            else
            {
                switch (nextTask)
                {
                    //保存参数
                    case TASKS.WRITE_PARA:
                        //继续更新
                        if (addrIndex < (ucDataGridView1.SelectRows.Count - 1))
                        {
                            addrIndex++;
                            MyDevice.protocol.addr = mutiAddres[ucDataGridView1.SelectRows[addrIndex].RowIndex];
                            //net模式下ip地址需要更换，即需要更换port
                            if (MyDevice.protocol.type == COMP.NET)
                            {
                                MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
                            }
                            actXET = MyDevice.actDev;
                            MyDevice.protocol.Protocol_Write_SendCOM(TASKS.WRITE_PARA);
                        }
                        //更新完成
                        else
                        {
                            //有线/蓝牙连接需多发一条WRITE_RECMODE指令
                            if (MyDevice.protocol.type == COMP.SelfUART)
                            {
                                //MyDevice.protocol.Protocol_Write_SendCOM(TASKS.WRITE_RECMODE, actXET.modeRec);
                                button4.BackColor = Color.Green;
                                nextTask = TASKS.NULL;
                            }
                            else
                            {
                                button4.BackColor = Color.Green;
                            }
                        }
                        break;

                    //清除缓存数据
                    case TASKS.WRITE_RECSIZE:
                        //继续更新
                        if (addrIndex < (ucDataGridView1.SelectRows.Count - 1))
                        {
                            addrIndex++;
                            MyDevice.protocol.addr = mutiAddres[ucDataGridView1.SelectRows[addrIndex].RowIndex];
                            //net模式下ip地址需要更换，即需要更换port
                            if (MyDevice.protocol.type == COMP.NET)
                            {
                                MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
                            }
                            actXET = MyDevice.actDev;
                            MyDevice.protocol.Protocol_Write_SendCOM(TASKS.WRITE_RECSIZE);
                        }
                        //更新完成
                        else
                        {
                            button3.BackColor = Color.Green;
                        }
                        break;

                    //更新A1MX
                    case TASKS.WRITE_A1M01DAT:
                        //继续发送
                        MyDevice.protocol.Protocol_WriteA1MXTasks();

                        //所有流程读取完成后执行
                        if (MyDevice.protocol.trTASK == TASKS.NULL)
                        {
                            //继续更新
                            if (addrIndex < (ucDataGridView1.SelectRows.Count - 1))
                            {
                                addrIndex++;
                                MyDevice.protocol.addr = mutiAddres[ucDataGridView1.SelectRows[addrIndex].RowIndex];
                                //net模式下ip地址需要更换，即需要更换port
                                if (MyDevice.protocol.type == COMP.NET)
                                {
                                    MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
                                }
                                actXET = MyDevice.actDev;

                                button1.Text = MyDevice.languageType == 0 ? "设备" + MyDevice.protocol.addr.ToString() : "Device" + MyDevice.protocol.addr.ToString();
                                MyDevice.protocol.Protocol_WriteA1MXTasks();
                            }
                            //更新完成
                            else
                            {
                                UpdateTableA1MX();
                                button1.Text = MyDevice.languageType == 0 ? "更 新" : "Refresh";
                                button1.BackColor = Color.Green;
                                actXET.oldAx = 0xFF;
                                actXET.oldMx = 0xFF;
                                actXET.oldTU = 0x00;
                            }
                        }
                        break;

                    //更新A2MX
                    case TASKS.WRITE_A2M01DAT:
                        //继续发送
                        MyDevice.protocol.Protocol_WriteA2MXTasks();

                        //所有流程读取完成后执行
                        if (MyDevice.protocol.trTASK == TASKS.NULL)
                        {
                            //继续更新
                            if (addrIndex < (ucDataGridView1.SelectRows.Count - 1))
                            {
                                addrIndex++;
                                MyDevice.protocol.addr = mutiAddres[ucDataGridView1.SelectRows[addrIndex].RowIndex];
                                //net模式下ip地址需要更换，即需要更换port
                                if (MyDevice.protocol.type == COMP.NET)
                                {
                                    MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
                                }
                                actXET = MyDevice.actDev;

                                button2.Text = MyDevice.languageType == 0 ? "设备" + MyDevice.protocol.addr.ToString() : "Device" + MyDevice.protocol.addr.ToString();
                                MyDevice.protocol.Protocol_WriteA2MXTasks();
                            }
                            //更新完成
                            else
                            {
                                UpdateTableA2MX();
                                button2.Text = MyDevice.languageType == 0 ? "更 新" : "Refresh";
                                button2.BackColor = Color.Green;
                                actXET.oldAx = 0xFF;
                                actXET.oldMx = 0xFF;
                                actXET.oldTU = 0x00;
                            }
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        //Peak模式和Track模式切换限定缓存模式
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
            {
                comboBox6.Enabled = true;
            }
            else
            {
                //协航XH06版本peak模式下只有缓存模式
                if (actXET.devType == TYPE.XH06)
                {
                    comboBox6.SelectedIndex = 2;
                    comboBox6.Enabled = false;
                }
            }
        }
    }
}
