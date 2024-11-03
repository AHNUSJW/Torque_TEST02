using Base.UI.MenuDevices;
using Library;
using Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Base
{
    public partial class MenuConfigForm : Form
    {
        List<Socket> clientList = new List<Socket>();
        Socket server, client; //
        string[] wifiFile = new string[10];
        string[] password = new string[10];
        int n_wifi = 0;//记录WiFi存储数量
        String mePath;//存取wifi名和密码的文件路径
        string AddressIP = string.Empty;//ip地址
        string wifiName;//存储本地WiFi名

        System.Timers.Timer timer;//设置定时器

        public MenuConfigForm()
        {
            InitializeComponent();

            cb_name.KeyPress += new KeyPressEventHandler(BoxRestrict.KeyPress_NoEnter);
            tb_psw.KeyPress += new KeyPressEventHandler(BoxRestrict.KeyPress_NoEnter);
        }

        /// <summary>
        /// 下一页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_next_Click(object sender, EventArgs e)
        {
            if (lb_step.Text == "0")
            {
                lb_step.Text = "1";
                //语言为中文时
                if (MyDevice.languageType == 0)
                {
                    tb_message.Text = "① 打开扭矩扳手，归零后同时长按 U + V\r\n\r\n② 输入密码：U + V + M + U\r\n\r\n③ 设置设备号（0~9999），设置完成后短按U显示PC1，点击下一步";
                }
                else
                {
                    tb_message.Text = "① Open the torque wrench and press U + V long after returning to zero.\r\n② Enter the password U + V + M + U.\r\n③ Set the device number (0~9999), press U to display PC1, and click Next.";
                }
            }
            else if (lb_step.Text == "1")
            {
                lb_step.Text = "2";
                tb_message.Text = MyDevice.languageType == 0 ? "\r\n\r\n\r\n将电脑网络切换为BEM4Wx（x为0-9999）" : "\r\n\r\n\r\nSwitch computer network to BEM4Wx (x is 0-9999).";
                bt_next.Visible = false;
                timer = new System.Timers.Timer(100);// 实例化Timer类，设置间隔时间为100毫秒
                timer.Elapsed += new System.Timers.ElapsedEventHandler(selectWifiName);//到达时间的时候执行事件
                timer.AutoReset = true;//设置是执行一次（false）还是一直执行(true)
                timer.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件
                timer.Start(); //启动定时器
            }
            else if (lb_step.Text == "2")
            {
                lb_step.Text = "3";
                //语言为中文时
                if (MyDevice.languageType == 0)
                {
                    tb_message.Text = "\r\n\r\n当前连接的网络为：" + wifiName + "\r\n\r\n确认无误后点击下一步";
                }
                else
                {
                    tb_message.Text = "\r\n\r\nThe network currently connected is:" + wifiName + "\r\n\r\nConfirm and click Next.";
                }
            }
            else if (lb_step.Text == "3")
            {
                tb_message.Text = MyDevice.languageType == 0 ? "\r\n\r\n\r\n扳手短按 U 建立通讯..." : "\r\n\r\n\r\nWrench press U short to establish communication...";
                getConnect();
                bt_next.Visible = false;
            }
            else if (lb_step.Text == "4")
            {
                bt_open_send.Visible = true;
                lb_name.Visible = true;
                lb_psw.Visible = true;
                cb_name.Visible = true;
                tb_psw.Visible = true;
                cb_name.Focus();
                tb_message.Visible = false;
            }
            else if (lb_step.Text == "5")
            {
                SaveToDat();
                bt_open_send.BackColor = SystemColors.Control;
                tb_message.Visible = true;
                bt_reset.Visible = true;
                lb_step.Text = "6";
                bt_next.Visible = true;
                lb_name.Visible = false;
                lb_psw.Visible = false;
                cb_name.Visible = false;
                tb_psw.Visible = false;
                bt_open_send.Visible = false;
                //语言为中文时
                if (MyDevice.languageType == 0)
                {
                    tb_message.Text = "① 扭矩扳手黄灯闪烁，等待验证\r\n\r\n② 若黄灯闪烁超过30秒，则密码错误，扭矩扳手重启后重新配网\r\n\r\n③ 若绿灯亮起，则密码正确，点击下一步进行联网";
                }
                else
                {
                    tb_message.Text = "① Torque wrench yellow light flashing, waiting for verification.\r\n② If the yellow light blinks for more than 30 seconds, the password is incorrect, and the network is reconfigured after the torque wrench is restarted.\r\n③ If the green light is on, the password is correct. Click Next to connect to the Internet.";
                }
            }
            else if (lb_step.Text == "6")
            {
                MenuConfigureForm_FormClosing(null, null);
                this.Hide();
                MenuConnectForm myConnectForm = new MenuConnectForm();
                myConnectForm.StartPosition = FormStartPosition.CenterParent;
                myConnectForm.ShowDialog();
                this.BringToFront();
                myConnectForm.Dispose();
            }
        }

        /// <summary>
        /// 重新配网
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_reset_Click(object sender, EventArgs e)
        {
            closeConnect();
            lb_step.Text = "1";
            //语言为中文时
            if (MyDevice.languageType == 0)
            {
                tb_message.Text = "① 打开扭矩扳手，归零后同时长按 U + V\r\n\r\n② 输入密码：U + V + M + U\r\n\r\n③ 设置设备号（0~9999），设置完成后短按U显示PC1，点击下一步";

            }
            else
            {
                tb_message.Text = "① Open the torque wrench and press U + V long after returning to zero.\r\n② Enter the password U + V + M + U.\r\n③ Set the device number (0~9999), press U to display PC1, and click Next.";
            }
            bt_reset.Visible = false;
        }

        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuConfigureForm_Load(object sender, EventArgs e)
        {
            lb_step.Text = "1";
            bt_next.Enabled = true;
            //语言为中文时
            if (MyDevice.languageType == 0)
            {
                tb_message.Text = "① 打开扭矩扳手，归零后同时长按 U + V\r\n\r\n② 输入密码：U + V + M + U\r\n\r\n③ 设置设备号（0~9999），设置完成后短按U显示PC1，点击下一步";

            }
            else
            {
                tb_message.Text = "① Open the torque wrench and press U + V long after returning to zero.\r\n② Enter the password U + V + M + U.\r\n③ Set the device number (0~9999), press U to display PC1, and click Next.";
            }
            //
            //空
            if (MyDevice.userDAT != null)
            {
                mePath = MyDevice.userDAT + @"\Wifi.txt";
                if (File.Exists(mePath))
                {
                    System.IO.File.SetAttributes(mePath, FileAttributes.Normal);
                    string[] lines = File.ReadAllLines(mePath);
                    for (int i = 0, j = 0; i < lines.Length; i += 2, j++)
                    {
                        cb_name.Items.Add(lines[i]);
                        wifiFile[i] = lines[i];
                        password[j] = lines[i + 1];
                        wifiFile[i + 1] = lines[i + 1];
                        n_wifi++;
                    }
                    if (cb_name.Items.Count > 0)
                    {
                        cb_name.SelectedIndex = 0;
                        tb_psw.Text = password[0];
                    }
                }
            }
        }

        /// <summary>
        /// 发送WiFi名和密码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_open_send_Click(object sender, EventArgs e)
        {
            if (bt_open_send.BackColor != Color.Green)
            {
                foreach (Socket proSocket in clientList)
                {
                    if (proSocket.Connected)
                    {
                        //按照指定编码将string编程字节数组
                        Encoding chs = Encoding.GetEncoding("gb2312");
                        byte[] name = chs.GetBytes(cb_name.Text.Trim());
                        byte[] psw = chs.GetBytes(tb_psw.Text.Trim());
                        //创建一个数组发送WiFi名字和密码
                        byte[] data = new byte[name.Length + psw.Length + 3];
                        data[0] = 0x02;
                        name.CopyTo(data, 1);
                        data[name.Length + 1] = 0x17;
                        psw.CopyTo(data, name.Length + 2);
                        data[data.Length - 1] = 0x03;
                        try
                        {
                            proSocket.Send(data, SocketFlags.None);
                        }
                        catch
                        {
                            if (MyDevice.languageType == 0)//语言选择为中文
                            {
                                MessageBox.Show("连接已断开，发送失败");
                            }
                            else
                            {
                                MessageBox.Show("The connection is down. Sending failed.");
                            }
                            break;
                        }
                    }
                }
                bt_open_send.BackColor = Color.Green;
            }
        }

        /// <summary>
        /// 建立连接
        /// </summary>
        public void getConnect()
        {
            //获取本地的IP地址

            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    AddressIP = _IPAddress.ToString();
                }
            }
            if (server != null)
                server.Close();


            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(new IPEndPoint(IPAddress.Parse(AddressIP), Int32.Parse("5678")));
            server.Listen(10);//监听
            ThreadPool.QueueUserWorkItem(new WaitCallback(AcceptSocketClient), server);//线程池
        }

        /// <summary>
        /// 接收客户端连接
        /// </summary>
        /// <param name="obj"></param>
        private void AcceptSocketClient(object obj)
        {
            while (true)
            {
                try
                {
                    client = server.Accept();//接受客户端链接
                    clientList.Add(client);
                    ThreadPool.QueueUserWorkItem(ReceiveData, client);
                    this.Invoke(new Action(() =>
                    {
                        lb_step.Text = "4";
                        bt_next_Click(null, null);
                    }));
                }
                catch (Exception)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuConfigureForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (lb_step.Text == "6" || lb_step.Text == "1")
            {
                if (server == null)
                {
                    return;
                }
                else
                {
                    closeConnect();
                }
            }
            else
            {
                if (DialogResult.Yes ==
                    (MyDevice.languageType == 0 ?
                    MessageBox.Show("正在设备配网，确认退出?", "提示", MessageBoxButtons.YesNo) :
                    Library.MessageBoxEX.Show("Network is being configured for the device. Confirm exit?", "Hint", MessageBoxButtons.YesNo, new string[] { "Yes", "No" }))
                    )
                {
                    timer.Stop();
                    if (server == null)
                    {
                        return;
                    }
                    else
                    {
                        closeConnect();
                    }
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        //实时监测当前WiFi名
        private void selectWifiName(object source, System.Timers.ElapsedEventArgs e)
        {
            timer.Stop();//关闭定时器
            wifiName = Library.WifiName.GetWIFIInfo();
            if (wifiName != null && wifiName.Contains("BEM4W"))
            {
                this.Invoke(new Action(() =>
                {
                    bt_next.Visible = true;
                }));
            }
            else
            {
                timer.Start();//开启定时器
            }
        }

        //不停在接受客户端的信息
        private void ReceiveData(object obj)
        {
            Socket client = obj as Socket;
            byte[] data = new byte[2 * 1024];
            while (client.Connected)
            {
                int len = 0;
                try
                {
                    len = client.Receive(data, SocketFlags.None);
                    if (len <= 0)
                    {
                        //客户端正常退出，直接关闭连接
                        clientList.Remove(client);
                        break;
                    }
                    string msg = null;
                    msg = Encoding.Default.GetString(data, 0, len);
                    if (msg == "OK")
                    {
                        this.Invoke(new Action(() =>
                        {
                            lb_step.Text = "5";
                            bt_next_Click(null, null);
                        }));

                    }
                }
                catch (Exception ex)
                {
                    //客户端产生异常了
                    clientList.Remove(client);
                    //关闭连接
                    if (client.Connected)
                    {
                        client.Shutdown(SocketShutdown.Both);
                        client.Close();
                    }
                    return;
                }

            }
        }

        //关闭连接
        private void closeConnect()
        {
            try
            {
                foreach (Socket sc in clientList)
                {
                    sc.Shutdown(SocketShutdown.Both);
                    sc.Close();
                }
            }
            catch { }
            try
            {
                server.Close();
            }
            catch { }
        }

        //更换wifi名
        private void cb_name_SelectedIndexChanged(object sender, EventArgs e)
        {
            tb_psw.Text = password[cb_name.SelectedIndex];
        }

        //保存wifi名和密码
        public bool SaveToDat()
        {
            //空
            if (MyDevice.userDAT == null)
            {
                return false;
            }
            //创建新路径
            else if (!Directory.Exists(MyDevice.userDAT))
            {
                Directory.CreateDirectory(MyDevice.userDAT);
            }

            //写入
            try
            {
                int Is_re = 0;
                if (File.Exists(mePath))
                {
                    System.IO.File.SetAttributes(mePath, FileAttributes.Normal);
                }
                for (int i = 0; i < n_wifi; i++)
                {
                    if (cb_name.Text == cb_name.Items[i].ToString())
                    {
                        if (password[i] == tb_psw.Text)
                        {
                            return true;
                        }
                        else
                        {
                            wifiFile[2 * i + 1] = tb_psw.Text;
                            Is_re = 1;
                            break;
                        }
                    }
                }
                if (Is_re == 0)
                {
                    wifiFile[n_wifi * 2] = cb_name.Text;
                    wifiFile[n_wifi * 2 + 1] = tb_psw.Text;
                    n_wifi++;
                }
                if (n_wifi > 0)
                {
                    string[] data = new string[n_wifi * 2];
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = wifiFile[i];
                    }
                    File.WriteAllLines(mePath, data);
                    System.IO.File.SetAttributes(mePath, FileAttributes.ReadOnly);

                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
