using BIL;
using Model;
using Library;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

//Lumi 20231026

namespace Base.UI.MenuTools
{
    public partial class MenuSearchWorkOrderForm : Form
    {
        JDBC jdbc = new JDBC();
        List<string> workOrderIDList = new List<string>();
        private string workOrderId = "";

        public string WorkOrderId
        {
            set { workOrderId = value; }
            get { return workOrderId; }
        }

        public MenuSearchWorkOrderForm()
        {
            InitializeComponent();

            //设置窗体的双缓冲
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
            //利用反射设置DataGridView的双缓冲
            Type dgvType = this.dataGridView1.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(this.dataGridView1, true, null);
        }

        //页面加载
        private void MenuSearchWorkOrderForm_Load(object sender, EventArgs e)
        {
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;    //选行
            refreshTable();   //刷新表
        }

        //Lostfocus当一个控件失去焦点时触发,定义控件失去焦点时的方法
        private void textBoxEx1_LostFocus(object sender, EventArgs e)
        {
            if (textBoxEx1.Text == "")
            {
                textBoxEx1.Text = MyDevice.languageType == 0 ? "输入信息后点击查找键" : "Enter the information first and then click the search button";
                //显示的字体颜色，灰色
                textBoxEx1.ForeColor = Color.Gray;
            }
        }

        //GotFocus是在一个对象得到焦点时发生，定义控件得到焦点时的方法
        private void textBoxEx1_GotFocus(object sender, EventArgs e)
        {
            if (textBoxEx1.Text == "输入信息后点击查找键" || textBoxEx1.Text == "Enter the information first and then click the search button")
            {
                textBoxEx1.Text = "";
                //输入的字体颜色，黑色
                textBoxEx1.ForeColor = Color.Black;
            }
        }

        //查找
        private void buttonX1_Click(object sender, EventArgs e)
        {
            // 查找worksInfoList中包含搜索字符串的所有项
            List<string> filteredList = workOrderIDList.Where(item => item.Contains(textBoxEx1.Text)).ToList();

            // 在DataGridView1中显示筛选后的列表
            dataGridView1.Rows.Clear(); // 清除现有行

            foreach (string item in filteredList)
            {
                dataGridView1.Rows.Add(item); // 将每个项添加到DataGridView的新行中
            }
        }

        //左键双击选择
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                workOrderId = this.dataGridView1.CurrentCell.Value.ToString();

                this.DialogResult = DialogResult.OK;//这里的DialogResult是Form2类对象的属性
                this.Close();
            }
        }

        //右键菜单
        private void dataGridView1_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                if (e.RowIndex == -1) return;

                this.dataGridView1.Rows[e.RowIndex].Selected = true;  //是否选中当前行
                this.dataGridView1.CurrentCell = this.dataGridView1.Rows[e.RowIndex].Cells[0];

                //每次选中行都刷新到datagridview中的活动单元格
                this.contextMenuStrip1.Show(this.dataGridView1, e.Location);
                //指定控件（DataGridView），指定位置（鼠标指定位置）
                this.contextMenuStrip1.Show(Cursor.Position);        //锁定右键列表出现的位置
            }
        }

        //刷新表格
        private void refreshTable()
        {
            List<WorksInfo> worksInfoList = new List<WorksInfo>();

            dataGridView1.Rows.Clear();
            dataGridView1.ClearSelection();

            worksInfoList = jdbc.GetListWork();
            if (worksInfoList == null) return;
            foreach (WorksInfo worksInfo in worksInfoList)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGridView1, worksInfo.work_order_id);
                dataGridView1.Rows.Add(row);
                workOrderIDList.Add(worksInfo.work_order_id);
            }
        }

        //复制工单号
        private void 复制ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                // 获取=列名为"工单号"的单元格的值
                string value = this.dataGridView1.CurrentCell.Value.ToString();

                // 将值复制到剪贴板
                Clipboard.SetText(value);
            }
        }

        //限制输入
        private void textBoxEx1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 使用正则表达式限制输入
            Regex regex = new Regex(@"^[a-zA-Z0-9_]+$");
            if (!regex.IsMatch(e.KeyChar.ToString()) && e.KeyChar != '\b')
            {
                e.Handled = true; // 阻止非法字符输入
            }
        }

        //允许复制粘贴
        private void textBoxEx1_KeyDown(object sender, KeyEventArgs e)
        {
            BoxRestrict.KeyUp_ControlXCV(sender, e);
        }
    }
}
