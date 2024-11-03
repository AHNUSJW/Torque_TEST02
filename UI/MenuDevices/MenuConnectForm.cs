using HZH_Controls.Controls;
using Library;
using Model;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Forms;

//Ricardo 20230803
//Ziyun 20230807
//Ziyun 20230905
//Ricardo 20230921

namespace Base.UI.MenuDevices
{
    public partial class MenuConnectForm : Form
    {
        //变量初始化
        private String myCOM = "COM1";
        private Boolean activeCom = false;
        private Boolean isConnectClose = false;

        private string oldActiveForm;
        public static string comPort;
        public static string ipPort;

        //单设备连接TASKS.READ_HEART
        //扫描单设备连接TASKS.READ_A1M01DAT
        //扫描连接TASKS.READ_PARA
        private TASKS meTask = TASKS.NULL;

        Byte addr = 1;                      //扫描站点1-255
        volatile bool isXFScan = false;     //是否点击接收器扫描
        volatile bool isNETScan = false;    //是否点击wifi扫描
        private volatile int connectID = 0;

        //构造函数
        public MenuConnectForm()
        {
            InitializeComponent();

            //文本输入限制
            textBox3.KeyPress += new KeyPressEventHandler(BoxRestrict.KeyPress_IntegerPositive_len3);
            textBox3.KeyUp += new KeyEventHandler(BoxRestrict.KeyUp_ControlXCV_IntegerPositive_len3);

            textBox5.KeyPress += new KeyPressEventHandler(BoxRestrict.KeyPress_IntegerPositive_len3);
            textBox5.KeyUp += new KeyEventHandler(BoxRestrict.KeyUp_ControlXCV_IntegerPositive_len3);
        }

        // 导航选择
        private void tvMenu_AfterSelect(object sender, TreeViewEventArgs e)
        {
            //
            switch (e.Node.Text.Trim())
            {
                case "有线连接":
                    groupBox1.Text = "有线连接";
                    groupBox2.Text = "";
                    groupBox3.Text = "";
                    groupBox4.Text = "";
                    groupBox1.Visible = true;
                    groupBox2.Visible = false;
                    groupBox3.Visible = false;
                    groupBox4.Visible = false;
                    button7.Visible = false;
                    label8.Visible = false;
                    textBox2.Width = 395;
                    textBox2.Height = 280;
                    textBox2.Location = new Point(31, 86);
                    break;

                case "Wired connection":
                    groupBox1.Text = "Wired connection";
                    groupBox2.Text = "";
                    groupBox3.Text = "";
                    groupBox4.Text = "";
                    groupBox1.Visible = true;
                    groupBox2.Visible = false;
                    groupBox3.Visible = false;
                    groupBox4.Visible = false;
                    button7.Visible = false;
                    label8.Visible = false;
                    textBox2.Width = 445;
                    textBox2.Height = 280;
                    textBox2.Location = new Point(31, 86);
                    break;

                case "蓝牙连接":
                    groupBox1.Text = "蓝牙连接";
                    groupBox2.Text = "";
                    groupBox3.Text = "";
                    groupBox4.Text = "";
                    groupBox1.Visible = true;
                    groupBox2.Visible = false;
                    groupBox3.Visible = false;
                    groupBox4.Visible = false;
                    button7.Visible = true;
                    label8.Visible = true;
                    textBox2.Width = 395;
                    textBox2.Height = 223;
                    textBox2.Location = new Point(31, 143);
                    break;

                case "Bluetooth connection":
                    groupBox1.Text = "Bluetooth connection";
                    groupBox2.Text = "";
                    groupBox3.Text = "";
                    groupBox4.Text = "";
                    groupBox1.Visible = true;
                    groupBox2.Visible = false;
                    groupBox3.Visible = false;
                    groupBox4.Visible = false;
                    button7.Visible = true;
                    label8.Visible = true;
                    textBox2.Width = 445;
                    textBox2.Height = 223;
                    textBox2.Location = new Point(31, 143);
                    break;

                case "接收器连接":
                    groupBox1.Text = "";
                    groupBox2.Text = "接收器连接";
                    groupBox3.Text = "";
                    groupBox4.Text = "";
                    groupBox1.Visible = false;
                    groupBox2.Visible = true;
                    groupBox3.Visible = false;
                    groupBox4.Visible = false;
                    groupBox2.Location = new Point(groupBox1.Location.X, groupBox1.Location.Y);
                    button6_Click(null, null);
                    break;

                case "Receiver connection":
                    groupBox1.Text = "";
                    groupBox2.Text = "Receiver connection";
                    groupBox3.Text = "";
                    groupBox4.Text = "";
                    groupBox1.Visible = false;
                    groupBox2.Visible = true;
                    groupBox3.Visible = false;
                    groupBox4.Visible = false;
                    groupBox2.Location = new Point(groupBox1.Location.X, groupBox1.Location.Y);
                    button6_Click(null, null);
                    break;

                case "路由器WiFi连接":
                    groupBox1.Text = "";
                    groupBox2.Text = "";
                    groupBox3.Text = "路由器WiFi连接";
                    groupBox4.Text = "";
                    panel1.Visible = false;
                    label2.Visible = true;
                    groupBox1.Visible = false;
                    groupBox2.Visible = false;
                    groupBox3.Visible = true;
                    groupBox4.Visible = false;
                    groupBox3.Location = new Point(groupBox1.Location.X, groupBox1.Location.Y);
                    break;

                case "WiFi connection":
                    groupBox1.Text = "";
                    groupBox2.Text = "";
                    groupBox3.Text = "WiFi connection";
                    groupBox4.Text = "";
                    panel1.Visible = false;
                    label2.Visible = true;
                    groupBox1.Visible = false;
                    groupBox2.Visible = false;
                    groupBox3.Visible = true;
                    groupBox4.Visible = false;
                    groupBox3.Location = new Point(groupBox1.Location.X, groupBox1.Location.Y);
                    break;

                case "① TCP连接":
                    groupBox1.Text = "";
                    groupBox2.Text = "";
                    groupBox3.Text = "① TCP连接";
                    groupBox4.Text = "";
                    panel1.Visible = true;
                    label2.Visible = false;
                    groupBox1.Visible = false;
                    groupBox2.Visible = false;
                    groupBox3.Visible = true;
                    groupBox4.Visible = false;
                    groupBox3.Location = new Point(groupBox1.Location.X, groupBox1.Location.Y);
                    button1_Click(null, null);
                    break;

                case "① TCP connection":
                    groupBox1.Text = "";
                    groupBox2.Text = "";
                    groupBox3.Text = "① TCP connection";
                    groupBox4.Text = "";
                    panel1.Visible = true;
                    label2.Visible = false;
                    groupBox1.Visible = false;
                    groupBox2.Visible = false;
                    groupBox3.Visible = true;
                    groupBox4.Visible = false;
                    groupBox3.Location = new Point(groupBox1.Location.X, groupBox1.Location.Y);
                    button1_Click(null, null);
                    break;

                case "② 建立通讯":
                    groupBox1.Text = "";
                    groupBox2.Text = "";
                    groupBox3.Text = "";
                    groupBox4.Text = "② 建立通讯";
                    groupBox1.Visible = false;
                    groupBox2.Visible = false;
                    groupBox3.Visible = false;
                    groupBox4.Visible = true;
                    groupBox4.Location = new Point(groupBox1.Location.X, groupBox1.Location.Y);
                    break;

                case "② Communications":
                    groupBox1.Text = "";
                    groupBox2.Text = "";
                    groupBox3.Text = "";
                    groupBox4.Text = "② Communications";
                    groupBox1.Visible = false;
                    groupBox2.Visible = false;
                    groupBox3.Visible = false;
                    groupBox4.Visible = true;
                    groupBox4.Location = new Point(groupBox1.Location.X, groupBox1.Location.Y);
                    break;
            }
        }

        //有线连接/蓝牙连接组合框
        private void comboBox0_port_SelectedIndexChanged(object sender, EventArgs e)
        {
            //
            if (myCOM != comboBox0_port.Text)
            {
                MyDevice.protocol.Protocol_PortClose();
                myCOM = comboBox0_port.Text;
            }
        }

        //接收器组合框
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //
            if (myCOM != comboBox1_port.Text)
            {
                MyDevice.myXFUART.Protocol_PortClose();
                myCOM = comboBox1_port.Text;
            }
        }

        //串口刷新
        private void button3_Click(object sender, EventArgs e)
        {
            if (MyDevice.protocol.IsOpen)
            {
                if (MyDevice.mSUT.isActive)
                {
                    textBox2.Text = MyDevice.languageType == 0 ? "设备已连接" : "The device is connected.";
                }
            }
            else
            {
                //刷串口
                comboBox0_port.Items.Clear();
                comboBox0_port.Items.AddRange(SerialPort.GetPortNames());

                //无串口
                if (comboBox0_port.Items.Count == 0)
                {
                    activeCom = false;
                    comboBox0_port.Text = null;
                    myCOM = null;
                }
                //有可用串口
                else
                {
                    comboBox0_port.Text = MyDevice.protocol.portName;
                    //
                    if (comboBox0_port.SelectedIndex < 0)
                    {
                        comboBox0_port.SelectedIndex = 0;
                    }
                    myCOM = comboBox0_port.Text;
                }
                bt_send1.Enabled = true;
            }
        }

        //接收器连接-刷新
        private void button6_Click(object sender, EventArgs e)
        {
            if (isConnectClose)
            {
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("设备正在连接中，请稍等...");
                }
                else
                {
                    MessageBox.Show("The device is connecting, please wait.");
                }
                return;
            }

            if (MyDevice.protocol.IsOpen)
            {
                if (MyDevice.devSum != 0)
                {
                    textBox4.Text = MyDevice.languageType == 0 ? "设备已连接" : "The device is connected.";
                }
            }
            else
            {
                //刷串口
                comboBox1_port.Items.Clear();
                comboBox1_port.Items.AddRange(SerialPort.GetPortNames());

                //无串口
                if (comboBox1_port.Items.Count == 0)
                {
                    activeCom = false;
                    comboBox1_port.Text = null;
                    myCOM = null;
                }
                //有可用串口
                else
                {
                    comboBox1_port.Text = MyDevice.myXFUART.portName;
                    //
                    if (comboBox1_port.SelectedIndex < 0)
                    {
                        comboBox1_port.SelectedIndex = 0;
                    }
                    myCOM = comboBox1_port.Text;
                }
                bt_send2.Enabled = true;
            }
        }

        //有线连接/蓝牙连接-串口连接
        private void bt_send1_Click(object sender, EventArgs e)
        {
            if (MyDevice.protocol.IsOpen)
            {
                if (MyDevice.mSUT.isActive)
                {
                    textBox2.Text = MyDevice.languageType == 0 ? "设备已连接" : "The device is connected.";
                }
                else
                {
                    //其他连接进行中，再点击有线/蓝牙连接
                    timer2.Enabled = false;
                    timer3.Enabled = false;
                    MessageBox.Show("串口被占用，请重新点击连接");
                    MyDevice.mePort_SetProtocol(COMP.SelfUART);
                }
            }
            else
            {
                if (myCOM != null)
                {
                    //切换自定义通讯
                    MyDevice.mePort_SetProtocol(COMP.SelfUART);

                    //打开串口
                    comPort = comboBox0_port.Text;
                    MyDevice.protocol.Protocol_PortOpen(comPort, 115200);

                    //串口发送
                    if (MyDevice.protocol.IsOpen)
                    {
                        textBox2.Text = MyDevice.languageType == 0 ? "适配器已打开\r\n搜索中 ." : "Adapter turned on. \r\nIn the search .";
                        bt_send1.BackColor = Color.OrangeRed;
                        timer1.Enabled = true;

                        //修改任务机状态
                        meTask = TASKS.READ_HEART;
                    }
                    else
                    {
                        textBox2.Text = MyDevice.languageType == 0 ? "适配器打开失败\r\n" : "Adapter opening failed. \r\n";
                        bt_send1.BackColor = Color.Firebrick;
                    }
                }
                else
                {
                    //
                    textBox2.Text = MyDevice.languageType == 0 ? "未找到适配,刷新中...\r\n" : "Adaptation not found, refreshing...\r\n";
                    bt_send1.BackColor = Color.Firebrick;
                    //刷新
                    button3_Click(null, null);
                }
            }
        }

        //接收器连接-串口连接
        private void bt_send2_Click(object sender, EventArgs e)
        {
            if (isConnectClose)
            {
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("设备正在连接中，请稍等...");
                }
                else
                {
                    MessageBox.Show("The device is connecting, please wait.");
                }
                return;
            }

            if (myCOM != null)
            {
                //不用扫描
                isXFScan = false;

                //切换XF通讯
                MyDevice.mePort_SetProtocol(COMP.XF);

                //打开串口
                    comPort = comboBox1_port.Text;
                    MyDevice.protocol.Protocol_PortOpen(comPort, 115200);

                //串口发送
                if (MyDevice.protocol.IsOpen)
                {
                    //初始化设备连接状态
                    for (int i = 0; i < 255; i++)
                    {
                        MyDevice.mXF[i].sTATE = STATE.INVALID;
                    }

                    //
                    ui_Connect_XF();
                    meTask = TASKS.READ_A1M01DAT;

                    //站点地址
                    if (textBox3.Text == "")
                    {
                        MessageBox.Show("设备ID不得为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }

                    if (Convert.ToUInt16(textBox3.Text) > 0 && Convert.ToUInt16(textBox3.Text) < 256)
                    {
                        MyDevice.protocol.addr = Convert.ToByte(textBox3.Text);
                    }
                    else
                    {
                        MessageBox.Show("设备ID不得超出 1 - 255 的范围");
                        return;
                    }

                    //读出设备信息
                    MyDevice.protocol.Protocol_ClearState();
                    MyDevice.protocol.Protocol_ReadTasks();

                    //清空显示框历史数据
                    textBox4.Clear();
                }
                else
                {
                    if (MyDevice.languageType == 0)
                    {
                        MessageBox.Show("串口未打开，检查串口是否被占用");
                    }
                    else
                    {
                        MessageBox.Show("The serial port is not enabled. Check whether the serial port is occupied.");
                    }
                }
            }
            else
            {
                //
                textBox4.Text = MyDevice.languageType == 0 ? "未找到适配,刷新中...\r\n" : "Adaptation not found, refreshing...\r\n";
                bt_send2.BackColor = Color.Firebrick;
                //刷新
                button6_Click(null, null);
            }
        }

        //有线连接/蓝牙连接-连接触发器
        private void timer1_Tick(object sender, EventArgs e)
        {
            textBox2.Text += ".";

            //读出设备信息
            if (MyDevice.protocol.trTASK != TASKS.READ_PARA)
            {
                MyDevice.protocol.Protocol_ClearState();
            }
            MyDevice.protocol.Protocol_ReadTasks();
        }

        //接收器扫描
        private void bt_scan_Click(object sender, EventArgs e)
        {
            //点击扫描标志
            isXFScan = true;

            //扫描之后连接状态初始化
            bt_send2.BackColor = Color.White;

            //扫描中
            if (timer2.Enabled)
            {
                ui_Stop_XFScan();
            }
            //停止中
            else
            {
                //清空显示框历史数据
                textBox4.Clear();

                if (comboBox1_port.Text != null)
                {
                    //切换XF通讯协议
                    MyDevice.mePort_SetProtocol(COMP.XF);

                    //打开串口
                    comPort = comboBox1_port.Text;
                    MyDevice.protocol.Protocol_PortOpen(comPort, 115200);

                    //串口有效
                    if (MyDevice.myXFUART.IsOpen)
                    {
                        //初始化设备连接状态
                        for (int i = 0; i < 255; i++)
                        {
                            MyDevice.mXF[i].sTATE = STATE.INVALID;
                        }

                        //扫描初始化
                        addr = 0;
                        meTask = TASKS.READ_PARA;

                        //启动扫描
                        ui_Start_XFScan();
                    }
                    else
                    {
                        if (MyDevice.languageType == 0)
                        {
                            MessageBox.Show("串口未打开，检查串口是否被占用");
                        }
                        else
                        {
                            MessageBox.Show("The serial port is not enabled. Check whether the serial port is occupied.");
                        }
                    }
                }
            }
        }

        //接收器连接-扫描触发器
        private void timer2_Tick(object sender, EventArgs e)
        {
            if (!isXFScan) return;

            if (MyDevice.myXFUART.IsOpen)
            {
                //扫描地址1-255
                if ((++addr) != 0)
                {
                    //

                    textBox3.Text = addr.ToString();
                    MyDevice.protocol.addr = addr;

                    //清除串口任务
                    //扫描-先发送A1M01,
                    //没回复直接扫描下一个站点,
                    //回复了继续发送剩余的A1M23-89,发完读取指令，继续下一个站点
                    MyDevice.protocol.Protocol_ClearState();
                    MyDevice.protocol.Protocol_ReadTasks();
                }
                else
                {
                    ui_Finish_XFScan();
                }
            }
            else
            {
                //停止扫描
                ui_Stop_XFScan();

                //
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("串口未打开，检查串口是否被占用");
                }
                else
                {
                    MessageBox.Show("The serial port is not enabled. Check whether the serial port is occupied.");
                }
            }
        }

        //加载
        private void MenuConnectForm_Load(object sender, EventArgs e)
        {
            oldActiveForm = Main.ActiveForm;
            Main.ActiveForm = "ConnectDevices";

            if (MyDevice.protocol.IsOpen)
            {
                if (MyDevice.mSUT.isActive)
                {
                    activeCom = true;
                    textBox2.Text = MyDevice.languageType == 0 ? "设备已通过串口连接" : "The device has been connected through the serial port.";
                    button3.Enabled = true;
                    bt_send1.Enabled = false;
                }
                //串口打开之后迅速切换到有效串口
                if (MyDevice.protocol.portName != null)
                {
                    comboBox0_port.Items.Add(MyDevice.protocol.portName);
                    comboBox0_port.SelectedIndex = 0;
                }
            }
            else
            {
                textBox3.Text = MyDevice.myXFUART.addr.ToString();
                button3_Click(null, null);
                button6_Click(null, null);
            }
            MyDevice.myUpdate += new freshHandler(receiveData);
        }

        //关闭窗口
        private void MenuConnectForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isConnectClose)
            {
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("设备正在连接中，请稍等...");
                }
                else
                {
                    MessageBox.Show("The device is connecting, please wait.");
                }
                e.Cancel = true;
            }
            else
            {
                timer1.Enabled = false;
                timer2.Enabled = false;
                timer3.Enabled = false;
                MyDevice.myUpdate -= new freshHandler(receiveData);

                if (activeCom)
                {
                    if (MyDevice.protocol.IsOpen == false)
                    {
                        try
                        {
                            comPort = comboBox0_port.Text;
                            MyDevice.protocol.Protocol_PortOpen(comPort, 115200);
                        }
                        catch
                        {
                            MyDevice.protocol.Protocol_PortClose();
                        }
                    }
                }
                else
                {
                    //打开端口之后未扫描连接关闭窗口重新连接
                    if (MyDevice.devSum == 0 && MyDevice.protocol == MyDevice.myWIFIUART)
                    {
                        MyDevice.protocol.Protocol_PortClose();
                        MyDevice.clientConnectionItems.Clear();
                    }
                }
            }
            Main.ActiveForm = oldActiveForm;
        }

        //委托
        private void receiveData()
        {
            //其它线程的操作请求
            if (this.InvokeRequired)
            {
                try
                {
                    freshHandler meDelegate = new freshHandler(receiveData);
                    this.Invoke(meDelegate, new object[] { });
                }
                catch
                {
                    //MessageBox.Show("MenuConnectForm receiveData err 1");
                }
            }
            //本线程的操作请求
            else
            {
                //XH扳手无法使用AiTorque软件
                if (Process.GetCurrentProcess().MainModule.FileName.Contains("AiTorque.exe") && MyDevice.actDev.devType == TYPE.XH06)
                {
                    textBox1.Text = "";
                    textBox2.Text = "";
                    textBox4.Text = "";
                    timer1.Enabled = false;
                    timer2.Enabled = false;
                    timer3.Enabled = false;

                    return;
                }

                isConnectClose = true;
                string str = MyDevice.languageType == 0 ? "读取" : "Read ";

                switch (meTask)
                {
                    //自定义连接
                    case TASKS.READ_HEART:
                        timer1.Enabled = false;
                        //刷新界面
                        textBox2.Text += "\r\n" + str + MyDevice.protocol.trTASK.ToString();
                        MyDevice.protocol.Protocol_ReadTasks();

                        if (MyDevice.protocol.trTASK == TASKS.NULL)
                        {
                            meTask = TASKS.NULL;
                            MyDevice.mSUT.isActive = true;
                            isConnectClose = false;
                            bt_send1.BackColor = Color.Green;
                            textBox2.Text += "\r\n" + str + "完成";
                        }
                        break;

                    //XF/wifi 单设备连接
                    case TASKS.READ_A1M01DAT:
                        //刷新界面
                        if (MyDevice.protocol == MyDevice.myXFUART)
                        {
                            textBox4.Text += "\r\n" + str + MyDevice.protocol.trTASK.ToString();
                        }
                        else
                        {
                            textBox1.Text += "\r\n" + str + MyDevice.protocol.trTASK.ToString();
                        }
                        MyDevice.protocol.Protocol_ReadTasks();

                        if (MyDevice.protocol.trTASK == TASKS.NULL)
                        {
                            meTask = TASKS.NULL;
                            isConnectClose = false;
                            if (MyDevice.protocol == MyDevice.myXFUART)
                            {
                                MyDevice.mXF[MyDevice.protocol.addr].isActive = true;
                                bt_send2.BackColor = Color.Green;
                                textBox4.Text += MyDevice.languageType == 0 ? "已连接设备" + MyDevice.protocol.addr : "Connected Devices." + MyDevice.protocol.addr;
                            }
                            else
                            {
                                MyDevice.mNET[MyDevice.protocol.addr].isActive = true;
                                button5.BackColor = Color.Green;
                                textBox1.Text += MyDevice.languageType == 0 ? "已连接设备" + MyDevice.protocol.addr : "Connected Devices." + MyDevice.protocol.addr;
                            }
                        }
                        break;

                    case TASKS.READ_PARA:
                        isXFScan = false;
                        isNETScan = false;
                        //刷新界面
                        if (MyDevice.protocol == MyDevice.myXFUART)
                        {
                            textBox4.Text += "\r\n" + str + MyDevice.protocol.trTASK.ToString();
                        }
                        else
                        {
                            textBox1.Text += "\r\n" + str + MyDevice.protocol.trTASK.ToString();
                        }
                        MyDevice.protocol.Protocol_ReadTasks();

                        if (MyDevice.protocol.trTASK == TASKS.NULL)
                        {
                            isConnectClose = false;
                            if (MyDevice.protocol == MyDevice.myXFUART)
                            {
                                isXFScan = true;
                                MyDevice.mXF[MyDevice.protocol.addr].isActive = true;
                                textBox4.Text += MyDevice.languageType == 0 ? "已连接设备" + MyDevice.protocol.addr : "Connected Devices." + MyDevice.protocol.addr;
                            }
                            else
                            {
                                MyDevice.mNET[MyDevice.protocol.addr].isActive = true;
                                if (connectID < MyDevice.clientConnectionItems.Count - 1)
                                {
                                    connectID++;
                                    addr = 0;
                                }
                                textBox1.Text += MyDevice.languageType == 0 ? "已连接设备" + MyDevice.protocol.addr : "Connected Devices." + MyDevice.protocol.addr;

                                isNETScan = true;
                            }
                        }
                        break;

                    case TASKS.SET_RECONFIG:
                        isConnectClose = false;
                        button7.BackColor = Color.Green;
                        if (MyDevice.mSUT.IsUnbind())
                        {
                            textBox2.Text += "\r\n" + "解绑成功";
                        }
                        else
                        {
                            textBox2.Text += "\r\n" + "解绑失败，请断开设备连接后重试";
                        }
                        break;
                }
            }
        }

        private void ui_Connect_XF()
        {
            timer2.Enabled = false;//关闭扫描

            bt_scan.Text = MyDevice.languageType == 0 ? "扫 描" : "Scan";
            bt_send2.Text = MyDevice.languageType == 0 ? "连 接" : "Connect";
        }

        private void ui_Stop_XFScan()
        {
            timer2.Enabled = false;//关闭扫描
            meTask = TASKS.NULL;

            bt_scan.BackColor = label1.BackColor;

            bt_scan.Text = MyDevice.languageType == 0 ? "扫 描": "Scan";
            bt_send2.Text = MyDevice.languageType == 0 ? "连 接" : "Connect";
        }

        private void ui_Start_XFScan()
        {
            timer2.Enabled = true;//启动扫描

            bt_scan.BackColor = Color.Firebrick;

            bt_scan.Text = MyDevice.languageType == 0 ? "停 止" : "Stop";
            bt_send2.Text = MyDevice.languageType == 0 ? "连 接" : "Connect";
        }

        private void ui_Finish_XFScan()
        {
            timer2.Enabled = false;//关闭扫描
            meTask = TASKS.NULL;

            bt_scan.BackColor = Color.Green;
            isConnectClose = false;

            bt_scan.Text = MyDevice.languageType == 0 ? "扫描完毕" : "Complete";
            bt_send2.Text = MyDevice.languageType == 0 ? "连 接" : "Connect";
        }

        private void ui_Connect_NET()
        {
            timer3.Enabled = false;//关闭扫描

            //button5.BackColor = Color.Green;

            button4.Text = MyDevice.languageType == 0 ? "扫 描" : "Scan";
            button5.Text = MyDevice.languageType == 0 ? "连 接" : "Connect";
        }

        private void ui_Stop_NETScan()
        {
            timer3.Enabled = false;//关闭扫描
            meTask = TASKS.NULL;

            button5.BackColor = label1.BackColor;

            button4.Text = MyDevice.languageType == 0 ? "扫 描" : "Scan";
            button5.Text = MyDevice.languageType == 0 ? "连 接" : "Connect";
        }

        private void ui_Start_NETScan()
        {
            timer3.Enabled = true;//启动扫描

            //button5.BackColor = Color.Firebrick;

            button4.Text = MyDevice.languageType == 0 ? "停 止" : "Stop";
            button5.Text = MyDevice.languageType == 0 ? "连 接" : "Connect";
        }

        private void ui_Finish_NETScan()
        {
            if (connectID < MyDevice.clientConnectionItems.Count - 1)
            {
                connectID++;
            }
            else
            {
                timer3.Enabled = false;//关闭扫描
                meTask = TASKS.NULL;

                button5.BackColor = Color.Green;
                isConnectClose = false;

                button4.Text = MyDevice.languageType == 0 ? "扫描完毕" : "Complete";
                button5.Text = MyDevice.languageType == 0 ? "连 接" : "Connect";
            }
        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            //自动滚屏
            textBox1.SelectionStart = textBox1.Text.Length;
            textBox1.ScrollToCaret();
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            //自动滚屏
            textBox4.SelectionStart = textBox4.Text.Length;
            textBox4.ScrollToCaret();
        }

        //wifi连接刷新
        private void button1_Click(object sender, EventArgs e)
        {
            comboBox2_ip.Items.Clear();
            //获取本地的ip
            string str = comboBox2_ip.Text;
            getIP();
            if (str != comboBox2_ip.Text)
            {
                MyDevice.protocol.Protocol_PortClose();
                button2.Enabled = true;
            }
        }

        //获取本地的ip
        public void getIP()
        {
            //获取本地的IP地址
            string AddressIP = string.Empty;
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    AddressIP = _IPAddress.ToString();
                    if (!comboBox2_ip.Items.Contains(AddressIP))
                    {
                        comboBox2_ip.Items.Add(AddressIP);
                    }
                }
            }
            comboBox2_ip.SelectedIndex = 0;
        }

        //网口测试
        private void button2_Click(object sender, EventArgs e)
        {
            if (comboBox2_ip.Text == "")
            {
                MessageBox.Show("ip地址不得为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            //检测并关闭串口
            if (MyDevice.protocol.IsOpen)
            {
                try
                {
                    MyDevice.protocol.Protocol_PortClose();
                    MyDevice.clientConnectionItems.Clear();
                    button2.BackColor = Color.Red;
                    button2.Text = "关闭端口";
                    comboBox2_ip.Enabled = true;
                    button1.Enabled = true;
                }
                catch
                {
                    textBox1.Text = MyDevice.languageType == 0 ? "未能正确关闭串口" : "Failed to properly close the serial port";
                }
            }
            else
            {
                //切换WF通讯协议
                MyDevice.mePort_SetProtocol(COMP.NET);

                ipPort = comboBox2_ip.Text;
                MyDevice.protocol.Protocol_PortOpen(ipPort, 5678);
                button2.BackColor = Color.Green;
                button2.Text = "打开端口";
                comboBox2_ip.Enabled = false;
                button1.Enabled = false;
            }
        }

        //wifi通讯连接单设备
        private void button5_Click(object sender, EventArgs e)
        {
            if (isConnectClose)
            {
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("设备正在连接中，请稍等...");
                }
                else
                {
                    MessageBox.Show("The device is connecting, please wait.");
                }
                return;
            }
            if (comboBox2_ip.Text != "")
            {
                //重复点击刷新重置按钮颜色
                button5.BackColor = label1.BackColor;

                //不用扫描
                isNETScan = false;

                //切换XF通讯
                MyDevice.mePort_SetProtocol(COMP.NET);

                //串口发送
                if (MyDevice.protocol.IsOpen)
                {
                    //初始化设备连接状态
                    for (int i = 0; i < 255; i++)
                    {
                        MyDevice.mNET[i].sTATE =  STATE.INVALID;
                    }

                    //
                    meTask = TASKS.READ_A1M01DAT;
                    ui_Connect_NET();

                    //站点地址
                    if (textBox5.Text == "")
                    {
                        MessageBox.Show("设备ID不得为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }

                    if (Convert.ToUInt16(textBox5.Text) > 0 && Convert.ToUInt16(textBox5.Text) < 256)
                    {
                        MyDevice.protocol.addr = Convert.ToByte(textBox5.Text);
                    }
                    else
                    {
                        MessageBox.Show("设备ID不得超出 1 - 255 的范围");
                        return;
                    }

                    if (MyDevice.clientConnectionItems.Count > 0)
                    {
                        //未扫描，addr与ip未绑定时取第一个值
                        MyDevice.protocol.port = MyDevice.clientConnectionItems.First().Value;

                        //扫描后，取对应的绑定值
                        if (MyDevice.addr_ip.ContainsKey(MyDevice.protocol.addr.ToString()))
                        {
                            MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
                        }
                    }

                    //读出设备信息
                    MyDevice.protocol.Protocol_ClearState();
                    MyDevice.protocol.Protocol_ReadTasks();

                    //清空显示框历史数据
                    textBox1.Clear();
                }
                else
                {
                    if (MyDevice.languageType == 0)
                    {
                        MessageBox.Show("串口未打开，检查串口是否被占用");
                    }
                    else
                    {
                        MessageBox.Show("The serial port is not enabled. Check whether the serial port is occupied.");
                    }
                }
            }
            else
            {
                //
                textBox1.Text = MyDevice.languageType == 0 ? "未找到适配,刷新中...\r\n" : "Adaptation not found, refreshing...\r\n";
                button4.BackColor = Color.Firebrick;
                //刷新
                button1_Click(null, null);
            }
        }

        //wifi通讯扫描多设备
        private void button4_Click(object sender, EventArgs e)
        {
            //点击扫描标志
            isNETScan = true;

            //扫描中
            if (timer3.Enabled)
            {
                ui_Stop_NETScan();
            }
            //停止中
            else
            {
                //清空显示框历史数据
                textBox1.Clear();

                if (comboBox2_ip.Text != null)
                {
                    //切换XF通讯协议
                    MyDevice.mePort_SetProtocol(COMP.NET);

                    //串口有效
                    if (MyDevice.myWIFIUART.IsOpen)
                    {
                        //初始化设备连接状态
                        for (int i = 0; i < 255; i++)
                        {
                            MyDevice.mNET[i].sTATE = STATE.INVALID;
                        }

                        //扫描初始化
                        addr = 0;
                        connectID = 0;
                        meTask = TASKS.READ_PARA;

                        //启动扫描
                        ui_Start_NETScan();
                    }
                    else
                    {
                        if (MyDevice.languageType == 0)
                        {
                            MessageBox.Show("串口未打开，检查串口是否被占用");
                        }
                        else
                        {
                            MessageBox.Show("The serial port is not enabled. Check whether the serial port is occupied.");
                        }
                    }
                }
            }
        }

        //wifi连接-扫描触发器
        private void timer3_Tick(object sender, EventArgs e)
        {
            if (!isNETScan) return;
            
            if (MyDevice.clientConnectionItems.Count != 0)
            {
                MyDevice.protocol.port = MyDevice.clientConnectionItems.Values.ElementAt(connectID);
            }

            if (MyDevice.myWIFIUART.IsOpen)
            {
                //扫描地址1-255
                if ((++addr) != 0)
                {
                    //
                    textBox5.Text = addr.ToString();
                    MyDevice.protocol.addr = addr;

                    //清除串口任务
                    //扫描-先发送A1M01,
                    //没回复直接扫描下一个站点,
                    //回复了继续发送剩余的A1M23-89,发完读取指令，继续下一个站点
                    MyDevice.protocol.Protocol_ClearState();
                    MyDevice.protocol.Protocol_ReadTasks();
                }
                else
                {
                    ui_Finish_NETScan();
                }
            }
            else
            {
                //停止扫描
                ui_Stop_NETScan();

                //
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("串口未打开，检查串口是否被占用");
                }
                else
                {
                    MessageBox.Show("The serial port is not enabled. Check whether the serial port is occupied.");
                }
            }
        }

        //接收器解绑
        private void button7_Click(object sender, EventArgs e)
        {
            if (MyDevice.protocol.IsOpen)
            {
                meTask = TASKS.SET_RECONFIG;
                button7.BackColor = Color.Red;
                MyDevice.protocol.Protocol_ClearState();
                MyDevice.protocol.Protocol_Write_SendCOM(TASKS.SET_RECONFIG);
            }
            else
            {
                if (myCOM != null)
                {
                    //切换自定义通讯
                    MyDevice.mePort_SetProtocol(COMP.SelfUART);

                    //打开串口
                    comPort = comboBox0_port.Text;
                    MyDevice.protocol.Protocol_PortOpen(comPort, 115200);

                    //串口发送
                    if (MyDevice.protocol.IsOpen)
                    {
                        meTask = TASKS.SET_RECONFIG;
                        button7.BackColor = Color.Red;
                        MyDevice.protocol.Protocol_ClearState();
                        MyDevice.protocol.Protocol_Write_SendCOM(TASKS.SET_RECONFIG);
                    }
                }
            }
        }
    }
}

