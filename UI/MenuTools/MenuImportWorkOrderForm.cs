using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Model;
using BIL;
using Library;

//Lumi 20230811
//Lumi 20231027

namespace Base.UI.MenuTools
{
    public partial class MenuImportWorkOrderForm : Form
    {
        JDBC jdbc = new JDBC();
        List<WorksInfo> worksInfoList = new List<WorksInfo>();

        public string TextBox1Value
        {
            set { textBox1.Text = value; }
            get { return textBox1.Text; }
        }

        public MenuImportWorkOrderForm()
        {
            InitializeComponent();
        }

        //窗口初始化
        private void MenuImportWorkOrderForm_Load(object sender, EventArgs e)
        {
            this.ActiveControl = textBox1;
        }

        //打开工单
        private void buttonX1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Equals("")) return;

            bool isImportTableExist = false;         //是否存在要导入的工单

            try
            {
                worksInfoList = jdbc.GetListWork();      //获取工单统计表
                foreach (WorksInfo wi in worksInfoList)
                {
                    if (wi.work_order_id == textBox1.Text)
                    {
                        isImportTableExist = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("数据查询失败，请安装数据库");
                return;
            }

            if (!isImportTableExist)
            {
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("不存在工单" + textBox1.Text, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("There is no ticket" + textBox1.Text, "System prompt", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return;
            }

            this.DialogResult = DialogResult.OK;//这里的DialogResult是Form2类对象的属性
            this.Close();
        }

        //搜索工单
        private void buttonX2_Click(object sender, EventArgs e)
        {
            //搜索工单弹窗
            MenuSearchWorkOrderForm myMenuSearchWorkOrderForm = new MenuSearchWorkOrderForm();
            myMenuSearchWorkOrderForm.StartPosition = FormStartPosition.CenterParent;
            this.BringToFront();

            if (myMenuSearchWorkOrderForm.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = myMenuSearchWorkOrderForm.WorkOrderId;
            }
            else
            {
                return;
            }
        }

        //限制输入
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 使用正则表达式限制输入
            Regex regex = new Regex(@"^[a-zA-Z0-9_]+$");
            if (!regex.IsMatch(e.KeyChar.ToString()) && e.KeyChar != '\b')
            {
                e.Handled = true; // 阻止非法字符输入
            }
        }

        //允许复制粘贴
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            BoxRestrict.KeyUp_ControlXCV(sender, e);
        }
    }
}
