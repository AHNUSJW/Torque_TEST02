using Library;
using Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;

namespace Base.UI.MenuTools
{
    public partial class MenuSetReceiverForm : Form
    {
        private XET actXET;     //需操作的设备
        private string oldActiveForm;

        public MenuSetReceiverForm()
        {
            InitializeComponent();

            //
            tb_wifiAddr.KeyPress += new KeyPressEventHandler(BoxRestrict.KeyPress_IntegerPositive_len3);
            tb_wifiAddr.KeyUp += new KeyEventHandler(BoxRestrict.KeyUp_ControlXCV_IntegerPositive_len3);
        }

        private void MenuSetReceiverForm_Load(object sender, EventArgs e)
        {
            //初始化数据
            actXET = MyDevice.mXF[0];

            //
            oldActiveForm = Main.ActiveForm;
            Main.ActiveForm = oldActiveForm;

            MyDevice.myUpdate += new freshHandler(receiveReconfig);

            //刷新串口
            button3_Click(sender, e);

            button1.Enabled = false;
        }

        private void MenuSetReceiverForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ucProcessLineExt1.Value != 0)
            {
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("正在配置接收器，请稍等...");
                }
                else
                {
                    MessageBox.Show("Configuring receiver, please wait...");
                }
                e.Cancel = true;
            }
            else
            {
                MyDevice.myUpdate -= new freshHandler(receiveReconfig);
                timer1.Enabled = false;
            }
            Main.ActiveForm = oldActiveForm;
        }

        //刷新串口
        private void button3_Click(object sender, EventArgs e)
        {
            //刷串口
            ucCombox1.Source = new List<KeyValuePair<string, string>>();
            String[] ports = SerialPort.GetPortNames();
            for (int i = 0; i < ports.Length; i++)
            {
                ucCombox1.Source.Add(new KeyValuePair<string, string>(i.ToString(), ports[i]));
            }

            if (ucCombox1.Source.Count > 0)
            {
                ucCombox1.SelectedIndex = 0;
            }
        }

        //打开串口
        private void button4_Click(object sender, EventArgs e)
        {
            if (ucProcessLineExt1.Value != 0)
            {
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("正在配置接收器，请稍等...");
                }
                else
                {
                    MessageBox.Show("Configuring receiver, please wait...");
                }
                return;
            }

            if (ucCombox1.TextValue != null)
            {
                //打开串口
                MyDevice.myXFUART.Protocol_PortOpen(ucCombox1.TextValue, 115200);

                button4.BackColor = Color.Firebrick;

                //串口发送
                if (MyDevice.myXFUART.IsOpen)
                {
                    button4.BackColor = Color.Green;
                }
                else
                {
                    if (MyDevice.languageType == 0)
                    {
                        MessageBox.Show("串口打开失败");
                    }
                    else
                    {
                        MessageBox.Show("The serial port failed to open");
                    }
                }
            }
            else
            {
                //
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("未找到串口");
                }
                else
                {
                    MessageBox.Show("Serial port not found");
                }
                //刷新
                button3_Click(null, null);
            }
        }

        //连接
        private void button2_Click(object sender, EventArgs e)
        {
            if (ucProcessLineExt1.Value != 0)
            {
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("正在配置接收器，请稍等...");
                }
                else
                {
                    MessageBox.Show("Configuring receiver, please wait...");
                }
                return;
            }

            if (MyDevice.myXFUART.IsOpen)
            {
                button2.BackColor = Color.Firebrick;
                //发送读接收器设置
                MyDevice.myXFUART.Protocol_ClearState();
                MyDevice.myXFUART.Protocol_Read_SendCOM(TASKS.READ_RECONFIG);
            }
            else
            {
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("请先打开串口...");
                }
                else
                {
                    MessageBox.Show("Please open the serial port first...");
                }
            }
        }

        //委托
        private void receiveReconfig()
        {
            //其它线程的操作请求
            if (this.InvokeRequired)
            {
                try
                {
                    freshHandler meDelegate = new freshHandler(receiveReconfig);
                    this.Invoke(meDelegate, new object[] { });
                }
                catch
                {
                }
            }
            //本线程的操作请求
            else
            {
                switch (MyDevice.myXFUART.trTASK)
                {
                    case TASKS.READ_RECONFIG:
                        UpdateForm();
                        button2.BackColor = Color.Green;
                        //确认权限
                        if (ucCombox1.TextValue != "" &&
                            tb_wifiAddr.Text != "" &&
                            cb_rfOption.TextValue != "" &&
                            cb_rfRate.TextValue != "" &&
                            cb_rfCheck.TextValue != "" &&
                            cb_rfBaud.TextValue != "")
                        {
                            button1.Enabled = true;
                        }
                        else
                        {
                            button1.Enabled = false;
                        }
                        break;

                    case TASKS.WRITE_RECONFIG:
                        button1.BackColor = Color.Green;
                        timer1.Enabled = true;
                        ucProcessLineExt1.Visible = true;
                        break;
                }
            }
        }

        //更新界面参数
        private void UpdateForm()
        {
            //校验位
            cb_rfCheck.Source = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("00", "8N1"),
                new KeyValuePair<string, string>("01", "8O1"),
                new KeyValuePair<string, string>("10", "8E1")
            };

            //串口速率（波特率）
            cb_rfBaud.Source = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("000", "1200"),
                new KeyValuePair<string, string>("001", "2400"),
                new KeyValuePair<string, string>("010", "4800"),
                new KeyValuePair<string, string>("011", "9600"),
                new KeyValuePair<string, string>("100", "19200"),
                new KeyValuePair<string, string>("101", "38400"),
                new KeyValuePair<string, string>("110", "57600"),
                new KeyValuePair<string, string>("111", "115200")
            };

            //空中速率
            cb_rfRate.Source = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("000", "1.2k"),
                new KeyValuePair<string, string>("001", "2.4k"),
                new KeyValuePair<string, string>("010", "4.8k"),
                new KeyValuePair<string, string>("011", "9.6k"),
                new KeyValuePair<string, string>("100", "19.2k"),
                new KeyValuePair<string, string>("101", "50k"),
                new KeyValuePair<string, string>("110", "100k"),
                new KeyValuePair<string, string>("111", "200k")
            };

            //发射功率
            cb_rfOption.Source = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("00", "30dBm"),
                new KeyValuePair<string, string>("01", "27dBm"),
                new KeyValuePair<string, string>("10", "24dBm"),
                new KeyValuePair<string, string>("11", "20dBm")
            };

            tb_wifiAddr.Text = actXET.meReconfig.wifiAddr.ToString();

            for (int i = 0; i < cb_rfCheck.Source.Count; i++)
            {
                if (cb_rfCheck.Source[i].Key == actXET.GetRfCheck())
                {
                    cb_rfCheck.SelectedIndex = i;
                    break;
                }
                else
                {
                    cb_rfCheck.SelectedIndex = 0;
                }
            }

            for (int i = 0; i < cb_rfBaud.Source.Count; i++)
            {
                if (cb_rfBaud.Source[i].Key == actXET.GetRfBaud())
                {
                    cb_rfBaud.SelectedIndex = i;
                    break;
                }
                else
                {
                    cb_rfBaud.SelectedIndex = 0;
                }
            }

            for (int i = 0; i < cb_rfRate.Source.Count; i++)
            {
                if (cb_rfRate.Source[i].Key == actXET.GetRfRate())
                {
                    cb_rfRate.SelectedIndex = i;
                    break;
                }
                else
                {
                    cb_rfRate.SelectedIndex = 0;
                }
            }

            for (int i = 0; i < cb_rfOption.Source.Count; i++)
            {
                if (Convert.ToByte(cb_rfOption.Source[i].Key) == actXET.meReconfig.rfOption)
                {
                    cb_rfOption.SelectedIndex = i;
                    break;
                }
                else
                {
                    cb_rfOption.SelectedIndex = 0;
                }
            }
        }

        //发送
        private void button1_Click(object sender, EventArgs e)
        {
            if (ucProcessLineExt1.Value != 0)
            {
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("正在配置接收器，请稍等...");
                }
                else
                {
                    MessageBox.Show("Configuring receiver, please wait...");
                }
                return;
            }

            Boolean change = false;
            Byte wifiAddr = Convert.ToByte(tb_wifiAddr.Text);
            Byte rfAddr = Convert.ToByte(tb_wifiAddr.Text);
            string str = cb_rfCheck.Source[cb_rfCheck.SelectedIndex].Key + cb_rfBaud.Source[cb_rfBaud.SelectedIndex].Key + cb_rfRate.Source[cb_rfRate.SelectedIndex].Key;
            Byte rfSped = actXET.SetSped(str);
            Byte rfChan = Convert.ToByte(tb_wifiAddr.Text);
            Byte rfOption = Convert.ToByte(cb_rfOption.Source[cb_rfOption.SelectedIndex].Key);

            if (actXET.meReconfig.wifiAddr != wifiAddr)
            {
                actXET.meReconfig.wifiAddr = wifiAddr;
                actXET.meReconfig.rfAddr = rfAddr;
                actXET.meReconfig.rfChan = rfChan;
                change = true;
            }

            if (change)
            {
                //发送写接收器设置
                button1.BackColor = Color.Red;
                MyDevice.myXFUART.Protocol_ClearState();
                MyDevice.myXFUART.Protocol_Write_SendCOM(TASKS.WRITE_RECONFIG);
            }
            else
            {
                button1.BackColor = Color.Green;
            }

        }

        //进度条
        private void timer1_Tick(object sender, EventArgs e)
        {
            ucProcessLineExt1.Value += 2;

            if (ucProcessLineExt1.Value == ucProcessLineExt1.MaxValue)
            {
                ucProcessLineExt1.Visible = false;
                ucProcessLineExt1.Value = 0;
                Main.ActiveForm = oldActiveForm;
                this.Close();
            }
        }


        private void tb_WIFIAddr_Leave(object sender, EventArgs e)
        {
            if (tb_wifiAddr.Text == "")
            {
                tb_wifiAddr.Text = actXET.meReconfig.wifiAddr.ToString();
                return;
            }

            if (Convert.ToUInt16(tb_wifiAddr.Text) > 0 && Convert.ToUInt16(tb_wifiAddr.Text) < 256)
            {
                byte b = Convert.ToByte(tb_wifiAddr.Text);
                if (b > 200)
                {
                    tb_wifiAddr.Text = "200";
                }
            }
            else
            {
                MessageBox.Show("接收器ID不得超出 1 - 255 的范围");
                return;
            }
        }
    }
}
