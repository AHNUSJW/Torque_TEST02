using Model;
using System;
using System.Windows.Forms;

namespace Base.UI.MenuUsers
{
    public partial class MenuAccountForm : Form
    {
        public MenuAccountForm()
        {
            InitializeComponent();
        }

        //窗口加载
        private void MenuAccountForm_Load(object sender, EventArgs e)
        {
            this.Text = MyDevice.languageType == 0 ? "欢迎使用": "Welcome";

            //获取当前运行的exe完整路径
            string str = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

            if (str.IndexOf("AiTorque.exe") != -1)
            {
                this.ShowIcon = true;//艾瑞特商标
            }
        }

        //窗口关闭
        private void MenuAccountForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Environment.Exit(0);
        }

        //登录按钮
        private void button1_Click(object sender, EventArgs e)
        {
            if ((comboBox1.Text == "zhoup") && (textBox1.Text == "AiTorque"))
            {
                MyDevice.userName = "zhoup";
                MyDevice.userPassword = "AiTorque";
            }

            this.Hide();
        }

        //取消按钮
        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
