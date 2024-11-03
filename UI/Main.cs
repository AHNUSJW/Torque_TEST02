using Base.UI.MenuDevices;
using Base.UI.MenuTools;
using Base.UI.MenuUsers;
using Library;
using Model;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace Base
{
    public partial class Main : Form
    {
        private static String activeForm = "";//激活的窗口
        private AutoFormSize autoFormSize = new AutoFormSize();

        //获取"激活的窗口"字段
        public static new String ActiveForm
        {
            set
            {
                activeForm = value;
            }
            get
            {
                return activeForm;
            }
        }

        public Main()
        {
            InitializeComponent();
        }

        //窗口加载
        private void AiTorque_Load(object sender, EventArgs e)
        {
            this.Hide();
            MenuAccountForm myAccountForm = new MenuAccountForm();
            myAccountForm.StartPosition = FormStartPosition.CenterScreen;
            myAccountForm.ShowDialog();
            this.Show();
            this.BringToFront();

            //获取电脑的MAC地址                                    
            MyDevice.myMac = string.Join("-", Regex.Split(MyDevice.GetMacByWmi(), ":"));

            MenuItemEnable();
            autoFormSize.controllInitializeSize(this);
        }

        //串口关闭
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            //关闭串口
            if (MyDevice.protocol.IsOpen)
            {
                //断开串口时，置位is_serial_closing标记更新
                MyDevice.protocol.Is_serial_closing = true;

                //处理当前在消息队列中的所有 Windows 消息
                //防止界面停止响应
                //https://blog.csdn.net/sinat_23338865/article/details/52596818
                while (MyDevice.protocol.Is_serial_listening)
                {
                    Application.DoEvents();
                }

                //关闭串口
                MyDevice.protocol.Protocol_PortClose();
            }
        }

        //界面是否启用
        private void MenuItemEnable()
        {
            if (MyDevice.devSum > 0)
            {
                //设备
                DeviceCalToolStripMenuItem.Enabled = true;
                DeviceDataToolStripMenuItem.Enabled = true;

                //路由器第二次连接会有问题，尚未解决
                if (MyDevice.protocol.type == COMP.NET)
                {
                    DeviceConnectToolStripMenuItem.Enabled = false;
                }
                else
                {
                    DeviceConnectToolStripMenuItem.Enabled = true;
                }

                //工具
                ToolDealDataMenuItem.Enabled = true;
                ToolSetWorkOrderMenuItem.Enabled = true;
                ToolDealWorkOrderMenuItem.Enabled = true;
                
                //仅BEM1老版本无拧紧策略
                if (MyDevice.protocol.type == COMP.SelfUART && MyDevice.mSUT.devType == TYPE.BEM)
                {
                    ToolSetWorkOrderMenuItem.Enabled = false;
                }
            }
            else
            {
                //设备
                DeviceCalToolStripMenuItem.Enabled = false;
                DeviceDataToolStripMenuItem.Enabled = false;

                //工具
                ToolDealDataMenuItem.Enabled = true;
                ToolSetWorkOrderMenuItem.Enabled = false;
                ToolDealWorkOrderMenuItem.Enabled = true;
            }
        }

        //用户登录
        private void FileAccountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MenuAccountForm myAccountForm = new MenuAccountForm();

            //
            myAccountForm.Text = MyDevice.languageType == 0 ? "切换用户！" : "Switch users";
            myAccountForm.StartPosition = FormStartPosition.CenterScreen;
            myAccountForm.ShowDialog();
        }

        //退出系统
        private void FileExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //退出所有窗口
            System.Environment.Exit(0);
        }

        //设备连接
        private void DeviceConnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form form in this.MdiChildren)
            {
                if (form.GetType().Name != "MenuConnectForm")
                {
                    form.Close();
                }
            }
            MenuConnectForm myMenuConnectForm = new MenuConnectForm();
            myMenuConnectForm.StartPosition = FormStartPosition.CenterScreen;
            myMenuConnectForm.ShowDialog();

            MenuItemEnable();
        }

        //设备参数
        private void DeviceCalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form form in this.MdiChildren)
            {
                if (form.GetType().Name != "myMenuCalForm")
                {
                    form.Close();
                }
            }
            MenuCalForm myMenuCalForm = new MenuCalForm();
            myMenuCalForm.StartPosition = FormStartPosition.CenterScreen;
            myMenuCalForm.ShowDialog();
        }

        //设备数据
        private void DeviceDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form form in this.MdiChildren)
            {
                if (form.GetType().Name == "MenuDataForm")
                {
                    form.BringToFront();
                    return;
                }
                else
                {
                    form.Close();
                }
            }

            MenuDataForm myMenuDataForm = new MenuDataForm();
            myMenuDataForm.MdiParent = this;
            myMenuDataForm.Show();
            myMenuDataForm.WindowState = FormWindowState.Maximized;
        }

        //拧紧策略
        private void ToolSetWorkOrderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form form in this.MdiChildren)
            {
                if (form.GetType().Name == "MenuShowStrategyForm")
                {
                    form.BringToFront();
                    return;
                }
                else
                {
                    form.Close();
                }
            }

            MenuShowStrategyForm myMenuShowStrategyForm = new MenuShowStrategyForm();
            myMenuShowStrategyForm.MdiParent = this;
            myMenuShowStrategyForm.Show();
            myMenuShowStrategyForm.WindowState = FormWindowState.Maximized;
        }

        //工单处理
        private void ToolDealWorkOrderMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form form in this.MdiChildren)
            {
                if (form.GetType().Name == "MenuDealWorkOrderForm")
                {
                    form.BringToFront();
                    return;
                }
                else
                {
                    form.Close();
                }
            }

            MenuDealWorkOrderForm myMenuWorkOrderForm = new MenuDealWorkOrderForm();
            myMenuWorkOrderForm.MdiParent = this;
            myMenuWorkOrderForm.Show();
            myMenuWorkOrderForm.WindowState = FormWindowState.Maximized;
        }

        //数据处理
        private void ToolDealDataToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            foreach (Form form in this.MdiChildren)
            {
                if (form.GetType().Name == "myMenuDealDataForm")
                {
                    form.BringToFront();
                    return;
                }
                else
                {
                    form.Close();
                }
            }

            MenuDealDataForm myMenuDealDataForm = new MenuDealDataForm();
            myMenuDealDataForm.MdiParent = this;
            myMenuDealDataForm.Show();
            myMenuDealDataForm.WindowState = FormWindowState.Maximized;
        }

        //接收器设置
        private void ToolSetReceiverMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form form in this.MdiChildren)
            {
                if (form.GetType().Name != "myMenuSetReceiverForm")
                {
                    form.Close();
                }
            }
            MenuSetReceiverForm myMenuSetReceiverForm = new MenuSetReceiverForm();
            myMenuSetReceiverForm.StartPosition = FormStartPosition.CenterScreen;
            myMenuSetReceiverForm.ShowDialog();
        }

        //设备配网
        private void DeviceConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form form in this.MdiChildren)
            {
                if (form.GetType().Name != "myMenuConfigForm")
                {
                    form.Close();
                }
            }
            MenuConfigForm myMenuConfigForm = new MenuConfigForm();
            myMenuConfigForm.StartPosition = FormStartPosition.CenterScreen;
            myMenuConfigForm.ShowDialog();
        }

        //语言切换（中文）
        private void ChineseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //zh-CN 为中文，更多的关于 Culture 的字符串请查 MSDN
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("zh-CN");
            //语言设置为中文
            MyDevice.languageType = 0;
            //提示
            MessageBox.Show("请重新启动软件");
            //MessageBox.Show("请重新启动软件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            //对当前窗体应用更改后的资源
            ApplyResource();
            //保存选择的语言
            RecordLanguage(0);
        }

        //语言切换（英文）
        private void EnglishToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //en 为英文，更多的关于 Culture 的字符串请查 MSDN
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en");
            //语言设置为英文
            MyDevice.languageType = 1;
            //提示
            MessageBox.Show("Please restart the software.");
            //MessageBox.Show("Please restart the software.","Hint", MessageBoxButtons.OK,MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            //对当前窗体应用更改后的资源
            ApplyResource();
            //保存选择的语言
            RecordLanguage(1);
        }

        /// <summary>
        ///  应用资源
        /// ApplyResources 的第一个参数为要设置的控件
        ///                  第二个参数为在资源文件中的ID，默认为控件的名称
        /// </summary>
        private void ApplyResource()
        {
            SuspendLayout();// SuspendLayout()是临时挂起控件的布局逻辑（msdn）
            ComponentResourceManager res = new ComponentResourceManager(typeof(Main));
            foreach (Control ctl in Controls)
            {
                if (ctl == menuStrip1)
                {
                    foreach (ToolStripMenuItem ctl2 in menuStrip1.Items)
                    {
                        res.ApplyResources(ctl2, ctl2.Name);
                        foreach (ToolStripMenuItem ctl3 in ctl2.DropDownItems)
                        {
                            res.ApplyResources(ctl3, ctl3.Name);
                        }

                    }
                }
                else
                {
                    res.ApplyResources(ctl, ctl.Name);
                }
            }
            res.ApplyResources(this.ChineseToolStripMenuItem, "ChineseToolStripMenuItem");
            res.ApplyResources(this.EnglishToolStripMenuItem, "EnglishToolStripMenuItem");
            this.ResumeLayout(false);
            this.PerformLayout();
            res.ApplyResources(this, "$this");
            SuspendLayout();
        }

        //保存选择的语言
        public void RecordLanguage(int language)
        {
            //空
            if (MyDevice.userDAT == null)
            {
                return;
            }
            //创建新路径
            else if (!Directory.Exists(MyDevice.userDAT))
            {
                Directory.CreateDirectory(MyDevice.userDAT);
            }

            //写入
            try
            {
                string mePath = MyDevice.userDAT + @"\Language.txt";//设置文件路径
                if (File.Exists(mePath))
                {
                    System.IO.File.SetAttributes(mePath, FileAttributes.Normal);
                }
                File.WriteAllText(mePath, language.ToString());
                System.IO.File.SetAttributes(mePath, FileAttributes.ReadOnly);
            }
            catch
            {
            }
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            //autoFormSize.controlAutoSize(this);
        }
    }
}
