using BIL;
using Library;
using Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
//using System.Windows.Forms.DataVisualization.Charting;


//Ricardo 20230809
//Ziyun 20230810
//Ricardo 20231227
//Ricardo 20240410

namespace Base.UI.MenuDevices
{
    public partial class MenuDataForm : Form
    {
        private enum uiTask : Byte
        {
            task_NULL,
            task_READ_PARA,
            task_READ_HEART,
            task_READ_RECSIZE,
            task_READ_RECDAT,
            task_READ_RECIDX,
            task_CLEAR_RECSIZE,
        }

        private XET actXET;               //需操作的设备
        private volatile uiTask nextTask = uiTask.task_READ_HEART; //按键操作指令

        private Boolean isRec = false;   //数据表中是否有缓存

        private string oldActiveForm;    //判断页面

        private Boolean isScroll = false;//是否拉到最后一行
        private Int32 lines = 1;         //table表行数

        private Byte infoTick = 0;       //控制info显示时间
        private Int32 infoErr = 0;       //控制info显示时间
        private Int32 infoLevel = 0;     //控制info显示时间
        private Int32 comTick = 0;       //指令超时监控

        JDBC jdbc = new JDBC();          //数据库

        List<DataInfo> dataInfos = new List<DataInfo>();        //数据表
        List<DataInfo> dataInfosToDb = new List<DataInfo>();    //实际存储到数据库的数据表
        private List<Picture> myPictures = new List<Picture>(); //画图

        private List<double> torqueList = new List<double>();   //扭矩集合
        private List<double> angleList = new List<double>();    //角度集合
        private List<double> ResidualtorqueList = new List<double>();  //用于计算残余扭矩的扭矩集合
        private List<double> ResidualangleList = new List<double>();   //用于计算残余扭矩的扭矩集合
        private double OldResidualtorque = 0;                          //用于记录残余扭矩上一次的值

        private Int32 bemPick = 0;      //picturebox中选中数据对应表格索引
        private float torqueMax = 0.0f; //记录扭矩峰值

        public Dictionary<string, float> dicTorque = new Dictionary<string, float>();    //记录扭矩峰值
        private Dictionary<string, float> dicAngle = new Dictionary<string, float>();    //记录角度峰值
        private Dictionary<string, string> dicIndex = new Dictionary<string, string>();  //记录数据段（以流水号为界）

        private List<A1MXTable> a1MXTables = new List<A1MXTable>();
        private List<A2MXTable> a2MXTables = new List<A2MXTable>();

        private float anglePeak = 0f;         //记录扭矩峰值
        private float torquePeak = 0f;        //记录角度峰值
        private string torqueUnit;            //扭矩单位
        private string res = "no";            //结果
        private string torqueResult = "no";   //扭矩结果
        private string angleResult = "no";    //角度结果
        private string lastWorkVin = "";      //前一个结果的流水号

        private int table_index = 0;//读取表格下标
        private bool isZero = false;//是否归零
        private bool updateReccurve = true;//是否更新缓存曲线

        private Timer wakeTimer = new Timer();//唤醒定时器（防止扳手因长时间不用而休眠）

        //页面初始化
        public MenuDataForm()
        {
            //设置窗体的双缓冲
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();

            //
            InitializeComponent();

            //利用反射设置DataGridView的双缓冲
            Type myType = this.dataGridView1.GetType();
            PropertyInfo pi = myType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(this.dataGridView1, true, null);
        }

        //页面加载
        private void MenuDataForm_Load(object sender, System.EventArgs e)
        {
            //
            oldActiveForm = Main.ActiveForm;
            Main.ActiveForm = "DevicesData";

            //初始化设备数据
            MyDevice.protocol.addr = MyDevice.AddrList[0];
            if (MyDevice.protocol.type == COMP.NET)
            {
                MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
            }
            actXET = MyDevice.actDev;

            //已连接设备数量
            textBox1.Text = MyDevice.devSum.ToString();

            //事件委托
            MyDevice.myUpdate += new freshHandler(receiveData);

            //鼠标委托
            chart1.MouseWheel += new MouseEventHandler(chart_MouseWheel);

            //注册鼠标滚动事件
            pictureBox1.MouseWheel += new MouseEventHandler(PictureBox_Show_MouseWheel);

            //表格初始化
            dataGridView1.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Font = new System.Drawing.Font("Arial", 11);
            dataGridView1.Columns[1].Width = 190;
            dataGridView1.Columns[2].Width = 110;
            //
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.LightSkyBlue;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToOrderColumns = false;
            dataGridView1.AllowUserToResizeColumns = false;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.RowHeadersVisible = false;
            //
            Screen screen = Screen.PrimaryScreen;
            this.splitContainer1.SplitterDistance = Convert.ToInt32(screen.Bounds.Width * 0.4);

            //预设值更新
            switch (actXET.torqueUnit)
            {
                case UNIT.UNIT_nm: torqueUnit = "N·m"; break;
                case UNIT.UNIT_lbfin: torqueUnit = "lbf·in"; break;
                case UNIT.UNIT_lbfft: torqueUnit = "lbf·ft"; break;
                case UNIT.UNIT_kgcm: torqueUnit = "kgf·cm"; break;
                default: break;
            }

            //曲线数据选择初始化
            ucCombox1.Source = new List<KeyValuePair<string, string>>();
            for (int i = 0; i < MyDevice.AddrList.Count; i++)
            {
                ucCombox1.Source.Add(new KeyValuePair<string, string>(i.ToString(), MyDevice.AddrList[i].ToString()));
                Picture picture = new Picture();
                myPictures.Add(picture);
            }
            ucCombox1.SelectedIndex = 0;

            //曲线模式选择初始化
            ucCombox2.Source = new List<KeyValuePair<string, string>>();
            ucCombox2.Source.Add(new KeyValuePair<string, string>(0.ToString(), "常规曲线模式"));
            ucCombox2.Source.Add(new KeyValuePair<string, string>(1.ToString(), "残余扭矩检测"));
            ucCombox2.SelectedIndex = 0;

            //读心跳发生器
            comTick = 25;
            timer1.Enabled = true;

            //唤醒定时器初始化
            wakeTimer.Interval = 180000; //间隔三分钟发送一次唤醒
            wakeTimer.Tick += wakeTimer_Tick;
            wakeTimer.Enabled = true;
            wakeTimer.Start();
        }

        //页面关闭
        private void MenuDataForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Enabled = false;
            MyDevice.myUpdate -= new freshHandler(receiveData);
            Main.ActiveForm = oldActiveForm;

            if (MyDevice.protocol.type != COMP.NET)
            {
                //关闭串口防止软件卡顿
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

            try
            {
                //保存数据list到数据库
                if (dataInfosToDb.Count > 0)
                {
                    string datatime = MyDevice.GetTime(MyDevice.GetTimeStamp()).ToString("yyyy-MM-dd");
                    string dataInfoTableName = MyDevice.myMac + "_" + datatime;  //数据表表名

                    List<string> datasInfoName = new List<string>();             //数据统计表名称列
                    bool isTableNameExist = false;                               //是否存在对应数据表

                    // datasInfo = jdbc.GetListDatas();
                    datasInfoName = jdbc.GetListDatasName();//
                    jdbc.TableName = dataInfoTableName;
                    if (datasInfoName == null)
                    {
                        isTableNameExist = false;
                    }
                    else if (datasInfoName.Count > 0)
                    {
                        if (datasInfoName.FindIndex(item => item == dataInfoTableName) < 0)
                        {
                            isTableNameExist = false;
                        }
                        else
                        {
                            isTableNameExist = true;
                        }
                    }

                    //查找该对应的数据表是否存在
                    if (!isTableNameExist)
                    {
                        //不存在
                        //先在数据统计表增加记录
                        DatasInfo di = new DatasInfo()
                        {
                            time = datatime,
                            name = dataInfoTableName
                        };
                        jdbc.AddDatasInfoByName(di);

                        //再建新表
                        jdbc.CreateDataTable();
                    }

                    //保存数据
                    jdbc.AddDataInfoByID(dataInfosToDb);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("数据保存失败，请安装数据库");
                return;
            }

            //初始化
            dataInfos.Clear();
            dataInfosToDb.Clear();
            dicTorque.Clear();
            dicAngle.Clear();
            dicIndex.Clear();
            table_index = 0;     //读取表格下标
            isZero = false;      //是否归零

            if (MyDevice.protocol.type != COMP.NET)
            {
                //打开串口
                MyDevice.protocol.Protocol_PortOpen(MenuConnectForm.comPort, 115200);
            }
        }

        //委托
        private void receiveData()
        {
            if (Main.ActiveForm != "DevicesData") return;

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
                    //MessageBox.Show("MenuRunForm receiveData err 3");
                }
            }
            //本线程的操作请求
            else
            {
                if (actXET.isActive)
                {
                    if (actXET.modePt == 0)
                    {
                        actXET.num_clear = 1;
                    }

                    //单位更新
                    switch (actXET.torqueUnit)
                    {
                        case UNIT.UNIT_nm: torqueUnit = "N·m"; break;
                        case UNIT.UNIT_lbfin: torqueUnit = "lbf·in"; break;
                        case UNIT.UNIT_lbfft: torqueUnit = "lbf·ft"; break;
                        case UNIT.UNIT_kgcm: torqueUnit = "kgf·cm"; break;
                        default: break;
                    }

                    //A1Mx表更新和A2Mx表更新
                    a1MXTables = new List<A1MXTable> {
                        actXET.a1mxTable.A1M1,
                        actXET.a1mxTable.A1M2,
                        actXET.a1mxTable.A1M3,
                        actXET.a1mxTable.A1M4,
                        actXET.a1mxTable.A1M5,
                        actXET.a1mxTable.A1M6,
                        actXET.a1mxTable.A1M7,
                        actXET.a1mxTable.A1M8,
                        actXET.a1mxTable.A1M9
                    };

                    a2MXTables = new List<A2MXTable> {
                        actXET.a2mxTable.A2M1,
                        actXET.a2mxTable.A2M2,
                        actXET.a2mxTable.A2M3,
                        actXET.a2mxTable.A2M4,
                        actXET.a2mxTable.A2M5,
                        actXET.a2mxTable.A2M6,
                        actXET.a2mxTable.A2M7,
                        actXET.a2mxTable.A2M8,
                        actXET.a2mxTable.A2M9
                    };

                    //画坐标轴
                    pictureBoxScope_axis();
                    pictureBoxScope_draw();

                    if (actXET.devType == TYPE.XH06 && actXET.modePt == 1)
                    {
                        switch (nextTask)
                        {
                            case uiTask.task_READ_PARA:
                                //切换成读心跳
                                comTick = 0;
                                nextTask = uiTask.task_READ_HEART;
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.READ_HEART);
                                break;

                            case uiTask.task_READ_HEART:
                                //有缓存数据
                                if (actXET.isEmpty == false)
                                {
                                    comTick = 0;
                                    nextTask = uiTask.task_READ_RECSIZE;
                                    Console.WriteLine("正常" + dataGridView1.Rows.Count.ToString());
                                    MyDevice.protocol.Protocol_Read_SendCOM(TASKS.WRITE_RECSIZE);
                                }
                                //更新表格
                                else
                                {
                                    //更新实时数据曲线
                                    myPictures[ucCombox1.SelectedIndex].getPoint_pictureBox(actXET.REC);

                                    actXET.REC.Clear();

                                    //协航版本XH06缓存peak模式下
                                    //实时曲线已经包括了缓存峰值
                                    //无需更新
                                    updateReccurve = false;
                                }
                                break;

                            case uiTask.task_READ_RECSIZE:
                                //开始读
                                if (actXET.queueSize > 0)
                                {
                                    comTick = 0;
                                    nextTask = uiTask.task_READ_RECDAT;                           
                                    MyDevice.protocol.Protocol_Write_SendCOM(TASKS.WRITE_RECDAT);
                                }
                                //有数据但是size为0，重新读心跳
                                else
                                {
                                    comTick = 25;
                                    nextTask = uiTask.task_READ_HEART;
                                }
                                break;

                            case uiTask.task_READ_RECDAT:
                                //
                                comTick = 0;
                                //检测是否读完
                                if (actXET.queueArray[0].index == 0)
                                {
                                    //检测完整性
                                    for (UInt16 i = 1; i < actXET.queueSize; i++)
                                    {
                                        if (actXET.queueArray[i].index == 0)
                                        {
                                            comTick = 0;
                                            nextTask = uiTask.task_READ_RECIDX;
                                            MyDevice.protocol.Protocol_Write_SendCOM(TASKS.WRITE_RECDAT, i);
                                            return;
                                        }
                                    }
                                    //更新表格
                                    updateTableFromRecord();
                                    //开始读缓存
                                    isRec = true;
                                    //完成
                                    nextTask = uiTask.task_CLEAR_RECSIZE;
                                    MyDevice.protocol.Protocol_Write_SendCOM(TASKS.WRITE_RECSIZE);
                                }
                                break;

                            case uiTask.task_READ_RECIDX:
                                //检测完整性
                                for (UInt16 i = 1; i < actXET.queueSize; i++)
                                {
                                    if (actXET.queueArray[i].index == 0)
                                    {
                                        comTick = 0;
                                        nextTask = uiTask.task_READ_RECIDX;
                                        MyDevice.protocol.Protocol_Write_SendCOM(TASKS.WRITE_RECDAT, i);
                                        return;
                                    }
                                }
                                //完成
                                nextTask = uiTask.task_CLEAR_RECSIZE;
                                MyDevice.protocol.Protocol_Write_SendCOM(TASKS.WRITE_RECSIZE);
                                break;

                            case uiTask.task_CLEAR_RECSIZE:
                                //更新表格
                                //updateTableFromRecord();
                                //开始读缓存
                                //isRec = true;
                                //读完设备缓存之后继续读缓存
                                comTick = 25;
                                nextTask = uiTask.task_READ_HEART;
                                actXET.isEmpty = true;
                                break;

                            default:
                                break;
                        }
                    }
                    else
                    {
                        switch (nextTask)
                        {
                            case uiTask.task_READ_PARA:
                                //切换成读心跳
                                comTick = 0;
                                nextTask = uiTask.task_READ_HEART;
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.READ_HEART);
                                break;

                            case uiTask.task_READ_HEART:
                                //有缓存数据
                                if (actXET.isEmpty == false)
                                {
                                    comTick = 0;
                                    nextTask = uiTask.task_READ_RECSIZE;
                                    MyDevice.protocol.Protocol_Read_SendCOM(TASKS.WRITE_RECSIZE);
                                }
                                //更新表格
                                else
                                {
                                    updateTableFromHeart();
                                }
                                break;

                            case uiTask.task_READ_RECSIZE:
                                //开始读
                                if (actXET.queueSize > 0)
                                {
                                    comTick = 0;
                                    nextTask = uiTask.task_READ_RECDAT;
                                    MyDevice.protocol.Protocol_Write_SendCOM(TASKS.WRITE_RECDAT);
                                }
                                //有数据但是size为0，重新读心跳
                                else
                                {
                                    comTick = 25;
                                    nextTask = uiTask.task_READ_HEART;
                                }
                                break;

                            case uiTask.task_READ_RECDAT:
                                //
                                comTick = 0;
                                //检测是否读完
                                if (actXET.queueArray[0].index == 0)
                                {
                                    //检测完整性
                                    for (UInt16 i = 1; i < actXET.queueSize; i++)
                                    {
                                        if (actXET.queueArray[i].index == 0)
                                        {
                                            comTick = 0;
                                            nextTask = uiTask.task_READ_RECIDX;
                                            MyDevice.protocol.Protocol_Write_SendCOM(TASKS.WRITE_RECDAT, i);
                                            return;
                                        }
                                    }
                                    //更新表格
                                    updateTableFromRecord();
                                    //开始读缓存
                                    isRec = true;
                                    //完成
                                    nextTask = uiTask.task_CLEAR_RECSIZE;
                                    MyDevice.protocol.Protocol_Write_SendCOM(TASKS.WRITE_RECSIZE);
                                }
                                break;

                            case uiTask.task_READ_RECIDX:
                                //检测完整性
                                for (UInt16 i = 1; i < actXET.queueSize; i++)
                                {
                                    if (actXET.queueArray[i].index == 0)
                                    {
                                        comTick = 0;
                                        nextTask = uiTask.task_READ_RECIDX;
                                        MyDevice.protocol.Protocol_Write_SendCOM(TASKS.WRITE_RECDAT, i);
                                        return;
                                    }
                                }
                                //完成
                                nextTask = uiTask.task_CLEAR_RECSIZE;
                                MyDevice.protocol.Protocol_Write_SendCOM(TASKS.WRITE_RECSIZE);
                                break;

                            case uiTask.task_CLEAR_RECSIZE:
                                //读完设备缓存之后读心跳
                                comTick = 25;
                                nextTask = uiTask.task_READ_HEART;
                                actXET.isEmpty = true;
                                break;

                            default:
                                break;
                        }
                    }                   
                }
            }
        }

        //更新dataInfos表格
        private void updateTableFromHeart()
        {
            int k = dataInfos.Count;
            for (int i = 0; i < actXET.REC.Count; i++)
            {
                if (!lastWorkVin.Equals(""))
                {
                    //流水号更新重置扳手结果
                    if (!lastWorkVin.Equals(actXET.REC[i].opsn))
                    {
                        res = "no";            //结果
                        torqueResult = "no";   //扭矩结果
                        angleResult = "no";    //角度结果
                    }
                }

                if (actXET.REC[i].opsn.Length > 0)
                {
                    //表格数据
                    DataInfo di = new DataInfo()
                    {
                        id = lines,
                        time = MyDevice.GetMilTimeStamp(),
                        work_vin = actXET.addr.ToString() + "#" + actXET.REC[i].opsn,
                        screw_id = "0",
                        wrench_code = "0",
                        wrench_id = actXET.addr.ToString(),
                        actual_torque = (actXET.REC[i].torque / 100.0f).ToString() + " " + torqueUnit.ToString(),
                        actual_angle = (actXET.REC[i].angle / 10.0f).ToString() + " °",
                        torque_peak = "torquePeak" + " " + torqueUnit.ToString(),
                        angle_peak = "anglePeak" + " °",
                        result = res,
                        torque_result = torqueResult,
                        angle_result = angleResult
                    };

                    if (actXET.REC[i].modePt == 0)
                    {
                        dataInfos.Add(di);

                        isScroll = true;
                    }
                    else
                    {
                        dataInfos.Add(di);
                    }

                    lastWorkVin = dataInfos[k].work_vin;

                    //获取对应流水号的扭矩峰值、角度峰值
                    if (dicTorque.ContainsKey(dataInfos[k].work_vin))
                    {
                        dicTorque[dataInfos[k].work_vin] = dicTorque[dataInfos[k].work_vin] > float.Parse(dataInfos[k].actual_torque.Split(' ')[0]) ? dicTorque[dataInfos[k].work_vin] : float.Parse(dataInfos[k].actual_torque.Split(' ')[0]);
                        dicAngle[dataInfos[k].work_vin] = dicAngle[dataInfos[k].work_vin] > float.Parse(dataInfos[k].actual_angle.Split(' ')[0]) ? dicAngle[dataInfos[k].work_vin] : float.Parse(dataInfos[k].actual_angle.Split(' ')[0]);
                    }
                    else
                    {
                        dicTorque.Add(dataInfos[k].work_vin, float.Parse(dataInfos[k].actual_torque.Split(' ')[0]));
                        dicAngle.Add(dataInfos[k].work_vin, float.Parse(dataInfos[k].actual_angle.Split(' ')[0]));

                        //记录数据段
                        dicIndex.Add(dataInfos[k].work_vin, (k).ToString());
                        if (k - 1 >= 0 && dicIndex[dataInfos[k - 1].work_vin].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries).Length == 1)
                        {
                            dicIndex[dataInfos[k - 1].work_vin] += ":" + (k - 1).ToString();
                        }

                    }

                    float torpeak = actXET.REC[i].torquePeak / 100.0f;
                    float angpeak = actXET.REC[i].anglePeak / 10.0f;

                    //数据调整，调整数据峰值
                    if (dicTorque[dataInfos[k].work_vin] < torpeak)
                    {
                        dicTorque[dataInfos[k].work_vin] = torpeak;
                        dataInfos[k].actual_torque.Split(' ')[0] = torpeak.ToString();
                    }

                    //数据调整，调整数据峰值
                    if (dicAngle[dataInfos[k].work_vin] < angpeak)
                    {
                        dicAngle[dataInfos[k].work_vin] = angpeak;
                        dataInfos[k].actual_angle.Split(' ')[0] = angpeak.ToString();
                    }

                    switch (actXET.modeAx * 10 + actXET.modeMx)
                    {
                        default:
                            break;
                        //A1M0模式满足目标扭矩合格
                        case 0:
                            angleResult = "ok";
                            if (dicTorque[dataInfos[k].work_vin] >= actXET.a1mxTable.A1M0.torqueTarget)
                            {
                                torqueResult = "ok";
                            }
                            else
                            {
                                torqueResult = "no";
                            }
                            break;
                        //A1M1-A1M9模式满足扭矩上下限合格
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                        case 9:
                            angleResult = "ok";
                            if (dicTorque[dataInfos[k].work_vin] >= a1MXTables[actXET.modeMx - 1].torqueLow && dicTorque[dataInfos[k].work_vin] <= a1MXTables[actXET.modeMx - 1].torqueHigh)
                            {
                                torqueResult = "ok";
                            }
                            else
                            {
                                torqueResult = "no";
                            }
                            break;
                        //A2M0模式先满足目标扭矩再满足目标角度
                        case 10:
                            if (dicTorque[dataInfos[k].work_vin] >= actXET.a2mxTable.A2M0.torquePre)
                            {
                                torqueResult = "ok";
                            }
                            else
                            {
                                torqueResult = "no";
                            }

                            if (dicAngle[dataInfos[k].work_vin] >= actXET.a2mxTable.A2M0.angleTarget)
                            {
                                angleResult = "ok";
                            }
                            else
                            {
                                angleResult = "no";
                            }
                            break;
                        //A2M1 - A2M9模式先满足目标扭矩再满足角度上下限
                        case 11:
                        case 12:
                        case 13:
                        case 14:
                        case 15:
                        case 16:
                        case 17:
                        case 18:
                        case 19:
                            if (dicTorque[dataInfos[k].work_vin] >= a2MXTables[actXET.modeMx - 1].torquePre)
                            {
                                torqueResult = "ok";
                            }
                            else
                            {
                                torqueResult = "no";
                            }

                            if (dicAngle[dataInfos[k].work_vin] >= a2MXTables[actXET.modeMx - 1].angleLow && dicAngle[dataInfos[k].work_vin] <= a2MXTables[actXET.modeMx - 1].angleHigh)
                            {
                                angleResult = "ok";
                            }
                            else
                            {
                                angleResult = "no";
                            }
                            break;
                    }

                    //超出扭矩量程不合格
                    if (dicTorque[dataInfos[k].work_vin] >= actXET.torqueCapacity)
                    {
                        torqueResult = "no";
                    }

                    if (torqueResult.Equals("ok") && angleResult.Equals("ok"))
                    {
                        res = "ok";
                    }
                    else
                    {
                        res = "no";
                    }

                    k++;//更改dataInfos下标

                    lines++;
                }
            }

            //更新曲线计算
            myPictures[ucCombox1.SelectedIndex].getPoint_pictureBox(actXET.REC);

            actXET.REC.Clear();
        }

        //更新缓存数据
        private void updateTableFromRecord()
        {
            String opsn;
            int k = dataInfos.Count;//dataInfos 下标

            actXET.snDate = System.DateTime.Now.ToString("yyMMdd");
            actXET.snBat++;
            opsn = actXET.snDate + actXET.snBat.ToString("").PadLeft(4, '0');

            for (UInt16 i = actXET.queueSize; i > 0;)
            {
                //
                i--;

                DataInfo di = new DataInfo()
                {
                    id = lines,
                    time = actXET.queueArray[i].stamp,
                    work_vin = actXET.addr.ToString() + "_" + opsn,
                    screw_id = "0",
                    wrench_code = "0",
                    wrench_id = actXET.addr.ToString(),
                    actual_torque = (actXET.queueArray[i].torque / 100.0f).ToString() + " " + torqueUnit.ToString(),
                    actual_angle = (actXET.queueArray[i].angle / 10.0f).ToString() + " °",
                    torque_peak = "torquePeak" + " " + torqueUnit.ToString(),
                    angle_peak = "anglePeak" + " °",
                    result = "",
                    torque_result = "",
                    angle_result = ""
                };

                dataInfos.Add(di);

                //获取对应流水号的扭矩峰值、角度峰值
                if (dicTorque.ContainsKey(dataInfos[k].work_vin))
                {
                    dicTorque[dataInfos[k].work_vin] = dicTorque[dataInfos[k].work_vin] > float.Parse(dataInfos[k].actual_torque.Split(' ')[0]) ? dicTorque[dataInfos[k].work_vin] : float.Parse(dataInfos[k].actual_torque.Split(' ')[0]);
                    dicAngle[dataInfos[k].work_vin] = dicAngle[dataInfos[k].work_vin] > float.Parse(dataInfos[k].actual_angle.Split(' ')[0]) ? dicAngle[dataInfos[k].work_vin] : float.Parse(dataInfos[k].actual_angle.Split(' ')[0]);
                }
                else
                {
                    dicTorque.Add(dataInfos[k].work_vin, float.Parse(dataInfos[k].actual_torque.Split(' ')[0]));
                    dicAngle.Add(dataInfos[k].work_vin, float.Parse(dataInfos[k].actual_angle.Split(' ')[0]));

                    //记录数据段
                    dicIndex.Add(dataInfos[k].work_vin, (k).ToString());
                    if (k - 1 >= 0 && dicIndex[dataInfos[k - 1].work_vin].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries).Length == 1)
                    {
                        dicIndex[dataInfos[k - 1].work_vin] += ":" + (k - 1).ToString();
                    }

                }

                k++;//更改dataInfos下标

                isScroll = true;

                //PEAK更新流水号
                if (actXET.modePt == 1)
                {
                    actXET.snBat++;
                    opsn = actXET.snDate + actXET.snBat.ToString("").PadLeft(4, '0');
                    //isScroll = false;
                }

                //
                lines++;
            }

            //更新到曲线
            if (updateReccurve)
            {
                myPictures[ucCombox1.SelectedIndex].getPoint_pictureBox(actXET.queueArray);
            }

            //
            actXET.queueSize = 0;
            actXET.queueArray = null;
            actXET.queuePercent = 0;
        }

        //更新dataGridView1表格（该表格是实际存储到数据库的数据表）
        private void dataGridView_update()
        {
            Int32 idx;

            if (actXET.isActive == false)
            {
                return;
            }

            //加行
            while (table_index < dataInfos.Count)
            {
                //存储到数据库
                DataInfo diTodb = new DataInfo()
                {
                    id = dataInfos[table_index].id,
                    time = dataInfos[table_index].time,
                    work_vin = dataInfos[table_index].work_vin,
                    screw_id = "0",
                    wrench_code = "0",
                    wrench_id = dataInfos[table_index].wrench_id,
                    actual_torque = dataInfos[table_index].actual_torque,
                    actual_angle = dataInfos[table_index].actual_angle,
                    torque_peak = dicTorque[dataInfos[table_index].work_vin].ToString() + " " + torqueUnit.ToString(),
                    angle_peak = dicAngle[dataInfos[table_index].work_vin].ToString() + " °",
                    result = res,
                    torque_result = torqueResult,
                    angle_result = angleResult
                };

                dataInfosToDb.Add(diTodb);

                if (ucCombox2.SelectedIndex == 1) chart_Update();

                if (actXET.devType == TYPE.XH06 && actXET.modePt == 1)
                {
                    //track模式或者有缓存
                    if (isRec)
                    {
                        //行数
                        idx = dataGridView1.Rows.Add();

                        //数据
                        dataGridView1.Rows[idx].Cells[0].Value = dataInfos[table_index].id;
                        dataGridView1.Rows[idx].Cells[1].Value = MyDevice.GetMilTime(dataInfos[table_index].time).ToString("yyyy-MM-dd HH:mm:ss:fff");
                        dataGridView1.Rows[idx].Cells[2].Value = dataInfos[table_index].work_vin;
                        dataGridView1.Rows[idx].Cells[3].Value = dataInfos[table_index].wrench_id;
                        dataGridView1.Rows[idx].Cells[4].Value = dataInfos[table_index].actual_torque;
                        dataGridView1.Rows[idx].Cells[5].Value = dataInfos[table_index].actual_angle;
                        dataGridView1.Rows[idx].Cells[6].Value = dicTorque[dataInfos[table_index].work_vin].ToString() + " " + torqueUnit.ToString();
                        dataGridView1.Rows[idx].Cells[7].Value = dicAngle[dataInfos[table_index].work_vin].ToString() + " °";

                        table_index++;
                        actXET.num_clear++;

                        if (idx > 0 && dataGridView1.Rows[idx].Cells[0].Value.ToString() == dataGridView1.Rows[idx - 1].Cells[0].Value.ToString())
                        {
                            dataGridView1.Rows.RemoveAt(idx);
                        }
                    }
                    else
                    {
                        table_index++;
                    }
                }
                else
                {
                    //track模式或者有缓存
                    if (actXET.modePt == 0 || isRec)
                    {
                        //行数
                        idx = dataGridView1.Rows.Add();
                        //数据
                        dataGridView1.Rows[idx].Cells[0].Value = dataInfos[table_index].id;
                        dataGridView1.Rows[idx].Cells[1].Value = MyDevice.GetMilTime(dataInfos[table_index].time).ToString("yyyy-MM-dd HH:mm:ss:fff");
                        dataGridView1.Rows[idx].Cells[2].Value = dataInfos[table_index].work_vin;
                        dataGridView1.Rows[idx].Cells[3].Value = dataInfos[table_index].wrench_id;
                        dataGridView1.Rows[idx].Cells[4].Value = dataInfos[table_index].actual_torque;
                        dataGridView1.Rows[idx].Cells[5].Value = dataInfos[table_index].actual_angle;
                        dataGridView1.Rows[idx].Cells[6].Value = dicTorque[dataInfos[table_index].work_vin].ToString() + " " + torqueUnit.ToString();
                        dataGridView1.Rows[idx].Cells[7].Value = dicAngle[dataInfos[table_index].work_vin].ToString() + " °";

                        table_index++;
                    }

                    else if (isZero)
                    {
                        //行数
                        idx = dataGridView1.Rows.Add();
                        //数据
                        dataGridView1.Rows[idx].Cells[0].Value = idx + 1;
                        dataGridView1.Rows[idx].Cells[1].Value = MyDevice.GetMilTime(dataInfos[table_index].time).ToString("yyyy-MM-dd HH:mm:ss:fff");
                        dataGridView1.Rows[idx].Cells[2].Value = dataInfos[table_index].work_vin;
                        dataGridView1.Rows[idx].Cells[3].Value = dataInfos[table_index].wrench_id;
                        dataGridView1.Rows[idx].Cells[4].Value = dicTorque[dataInfos[table_index].work_vin].ToString() + " " + torqueUnit.ToString(); ;
                        dataGridView1.Rows[idx].Cells[5].Value = dicAngle[dataInfos[table_index].work_vin].ToString() + " °";
                        dataGridView1.Rows[idx].Cells[6].Value = dicTorque[dataInfos[table_index].work_vin].ToString() + " " + torqueUnit.ToString(); ;
                        dataGridView1.Rows[idx].Cells[7].Value = dicAngle[dataInfos[table_index].work_vin].ToString() + " °";

                        table_index++;
                        isZero = false;
                        actXET.num_clear++;

                        dataInfosToDb.RemoveAt(table_index);//用于删除peak模式下归零时多存d一条过程数据

                    }
                    else
                    {
                        table_index++;
                    }
                }            
            }
            //缓存读取完清除
            isRec = false;

            //超过5000行删除
            while (dataInfos.Count > 5000)
            {
                dataGridView1.Rows.RemoveAt(0);

                dataInfos.RemoveAt(0);

                table_index--;
            }

            //移到最后一行
            if (isScroll)
            {
                isScroll = false;

                if (dataGridView1.RowCount > 1)
                {
                    dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.RowCount - 1;
                }
            }
        }

        //定时器-页面加载触发
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Main.ActiveForm != "DevicesData") return;

            //提示信息10秒, torqueErr屏蔽angleLevel信息
            if (infoErr != actXET.torqueErr)
            {
                infoTick++;
                if (infoTick > 100)
                {
                    infoTick = 0;
                    infoErr = actXET.torqueErr;
                    infoLevel = actXET.angleLevel;
                }
            }
            //提示信息5秒
            else if (infoLevel != actXET.angleLevel)
            {
                infoTick++;
                if (infoTick > 50)
                {
                    infoTick = 0;
                    infoLevel = actXET.angleLevel;
                }
            }
            else
            {
                infoTick = 0;
            }

            //PEAK模式下写入表格
            if (actXET.num_clear % 2 == 0)
            {
                isZero = true;
                table_index--;
                if (table_index < 0)
                {
                    table_index = 0;
                }
            }

            dataGridView_update();

            //画曲线
            pictureBoxScope_draw();

            //通讯监控
            if (actXET.isActive)
            {
                switch (nextTask)
                {
                    case uiTask.task_READ_PARA:
                        if (++comTick > 10)
                        {
                            comTick = 0;
                            MyDevice.protocol.Protocol_Read_SendCOM(TASKS.READ_PARA);
                        }
                        break;

                    case uiTask.task_READ_HEART:
                        //读心跳回65帧数据，一帧50ms, 理论值总共3250ms（定时器触发时间100ms）
                        if (++comTick > 30)
                        {
                            comTick = 0;
                            MyDevice.protocol.Protocol_Read_SendCOM(TASKS.READ_HEART);
                        }
                        break;

                    case uiTask.task_READ_RECSIZE:
                        //读缓存
                        if (++comTick > 10)
                        {
                            comTick = 0;
                            MyDevice.protocol.Protocol_Read_SendCOM(TASKS.WRITE_RECSIZE);
                        }
                        break;

                    case uiTask.task_READ_RECDAT:
                        //读数据
                        if (++comTick > 10)
                        {
                            comTick = 0;
                            //MyDevice.protocol.Protocol_Write_SendCOM(TASKS.WRITE_RECDAT);
                            Console.WriteLine("超时");
                            nextTask = uiTask.task_READ_RECSIZE;
                            MyDevice.protocol.Protocol_Read_SendCOM(TASKS.WRITE_RECSIZE);
                        }
                        break;

                    case uiTask.task_READ_RECIDX:
                        //读数据
                        if (++comTick > 10)
                        {
                            comTick = 0;

                            //检测完整性
                            for (UInt16 i = 1; i < actXET.queueSize; i++)
                            {
                                if (actXET.queueArray[i].index == 0)
                                {
                                    MyDevice.protocol.Protocol_Write_SendCOM(TASKS.WRITE_RECDAT, i);
                                }
                            }
                        }
                        break;

                    case uiTask.task_CLEAR_RECSIZE:
                        //清缓存
                        if (++comTick > 10)
                        {
                            comTick = 0;
                            MyDevice.protocol.Protocol_Write_SendCOM(TASKS.WRITE_RECSIZE);
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        //画曲线
        private void pictureBoxScope_draw()
        {
            //铺图
            String myBatStr;
            String myLevStr;
            String myStr;
            SizeF mySize;

            MyDevice.protocol.addr = Convert.ToByte(ucCombox1.TextValue);
            if (MyDevice.protocol.type == COMP.NET)
            {
                MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
            }
            actXET = MyDevice.actDev;

            if (actXET.isActive == false)
            {
                return;
            }
            string battery = MyDevice.languageType == 0 ? "电量" : "Electricity";
            switch (actXET.battery)
            {
                default:
                //case 3: myBatStr = "电池 100%"; break;
                //case 2: myBatStr = "电池 60%"; break;
                //case 1: myBatStr = "电池 30%"; break;
                //case 0: myBatStr = "电池 0%"; break;
                case 3: myBatStr = battery + " III"; break;
                case 2: myBatStr = battery + " II"; break;
                case 1: myBatStr = battery + " I"; break;
                case 0: myBatStr = battery + " -"; break;
            }

            if (actXET.angleLevel > actXET.angleSpeed)
            {
                myLevStr = MyDevice.languageType == 0 ? "操作过快, 请提升角度档位" : "If the operation is too fast, please increase the angle gear";
            }
            else
            {
                string angleLevel = MyDevice.languageType == 0 ? "操作慢, 建议角度档位 " : "Slow operation, recommended angle gear";
                switch (actXET.angleLevel)
                {
                    default:
                    case 0: myLevStr = angleLevel + "15°/sec"; break;
                    case 1: myLevStr = angleLevel + "30°/sec"; break;
                    case 2: myLevStr = angleLevel + "60°/sec"; break;
                    case 3: myLevStr = angleLevel + "120°/sec"; break;
                    case 4: myLevStr = angleLevel + "250°/sec"; break;
                    case 5: myLevStr = angleLevel + "500°/sec"; break;
                    case 6: myLevStr = angleLevel + "1000°/sec"; break;
                    case 7: myLevStr = angleLevel + "2000°/sec"; break;
                }
            }

            //层图
            Bitmap img = new Bitmap(myPictures[ucCombox1.SelectedIndex].Width, myPictures[ucCombox1.SelectedIndex].Height);

            //绘制
            Graphics g = Graphics.FromImage(img);

            //不同模式下画曲线
            if (ucCombox2.SelectedIndex == 0)
            {
                //画线,选中的点
                if (myPictures[ucCombox1.SelectedIndex].xline_pick > 1)
                {
                    g.DrawLine(new Pen(myPictures[ucCombox1.SelectedIndex].color_info, 1.0f), new Point(myPictures[ucCombox1.SelectedIndex].xline_pick, 0), new Point(myPictures[ucCombox1.SelectedIndex].xline_pick, myPictures[ucCombox1.SelectedIndex].Height));
                }

                //画角度曲线
                if (myPictures[ucCombox1.SelectedIndex].angPoint.Count > 1)
                {
                    g.DrawCurve(new Pen(myPictures[ucCombox1.SelectedIndex].color_angle, 2.0f), myPictures[ucCombox1.SelectedIndex].angPoint.ToArray(), 0);
                }
                //画扭矩曲线
                if (myPictures[ucCombox1.SelectedIndex].torPoint.Count > 1)
                {
                    g.DrawCurve(new Pen(myPictures[ucCombox1.SelectedIndex].color_torque, 2.0f), myPictures[ucCombox1.SelectedIndex].torPoint.ToArray(), 0);
                }
            }
            else if (ucCombox2.SelectedIndex == 1)
            {
                try
                {
                    //剩余扭矩检测初始化
                    if (this.chart1 != null)
                    {
                        this.chart1.BackColor = Color.Black;
                        this.chart1.Series[0].BorderWidth = 3;
                        this.chart1.ChartAreas[0].BackColor = Color.Black;
                        this.chart1.Series[0].Color = Color.LightGreen;
                        this.chart1.ChartAreas[0].AxisY.LineColor = Color.LightGreen;
                        this.chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
                        this.chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = false;
                        this.chart1.ChartAreas[0].AxisX.LineColor = Color.White;
                        this.chart1.ChartAreas[0].AxisY.LineColor = Color.White;
                        this.chart1.ChartAreas[0].Axes[0].Title = "X - 角度";
                        this.chart1.ChartAreas[0].Axes[0].TitleForeColor = Color.White;
                        this.chart1.ChartAreas[0].Axes[1].Title = "Y - 扭矩";
                        this.chart1.ChartAreas[0].Axes[1].TitleForeColor = Color.White;
                        this.chart1.Series[0].IsVisibleInLegend = false;

                        //坐标轴初始化
                        chart1.ChartAreas[0].AxisX.Minimum = -System.Double.NaN;
                        chart1.ChartAreas[0].AxisX.Maximum = System.Double.NaN;
                        chart1.ChartAreas[0].AxisY.Minimum = -System.Double.NaN;
                        chart1.ChartAreas[0].AxisY.Maximum = System.Double.NaN;

                        // 设置轴显示数值 轴刻度间隔为10
                        chart1.ChartAreas[0].AxisX.Interval = 10;
                        chart1.ChartAreas[0].AxisX.LabelStyle.Format = "0";
                        chart1.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.White;
                        chart1.ChartAreas[0].AxisY.Interval = 10;
                        chart1.ChartAreas[0].AxisY.LabelStyle.Format = "0";
                        chart1.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.White;
                    }
                }
                catch
                {
                    //this.Refresh();
                    this.chart1 = null;
                }
            }

            //画角度仪表盘
            g.DrawLine(new Pen(myPictures[ucCombox1.SelectedIndex].color_angle, 5.0f), new Point(myPictures[ucCombox1.SelectedIndex].angCentreX, myPictures[ucCombox1.SelectedIndex].angCentreY), new Point((int)myPictures[ucCombox1.SelectedIndex].angArrowX, (int)myPictures[ucCombox1.SelectedIndex].angArrowY));
            //画扭矩仪表盘
            g.DrawArc(new Pen(myPictures[ucCombox1.SelectedIndex].color_torque, 5.0f), myPictures[ucCombox1.SelectedIndex].torCxStart, myPictures[ucCombox1.SelectedIndex].torCyStart, myPictures[ucCombox1.SelectedIndex].torDiameter, myPictures[ucCombox1.SelectedIndex].torDiameter, 90, myPictures[ucCombox1.SelectedIndex].torArcEnd);

            if (actXET.angleLevel != actXET.angleSpeed)
            {
                g.DrawString(myLevStr, new System.Drawing.Font("Courier New", 12), myPictures[ucCombox1.SelectedIndex].brush_info, myPictures[ucCombox1.SelectedIndex].xline_info, myPictures[ucCombox1.SelectedIndex].yline_angleLevel);
            }

            if (!(actXET.devType == TYPE.XH06 && actXET.modePt == 1))
            {
                if (actXET.queuePercent > 0)
            {
                if (MyDevice.languageType == 0)
                {
                    g.DrawString("读出缓存完成" + actXET.queuePercent.ToString() + "%", new System.Drawing.Font("Courier New", 20), myPictures[ucCombox1.SelectedIndex].brush_info, myPictures[ucCombox1.SelectedIndex].xline_info, myPictures[ucCombox1.SelectedIndex].yline_queueSize);
                }
                else
                {
                    g.DrawString("Read caching completes" + actXET.queuePercent.ToString() + "%", new System.Drawing.Font("Courier New", 20), myPictures[ucCombox1.SelectedIndex].brush_info, myPictures[ucCombox1.SelectedIndex].xline_info, myPictures[ucCombox1.SelectedIndex].yline_queueSize);
                }
            }
            }

            //文字角度
            MyDevice.protocol.addr = Convert.ToByte(ucCombox1.TextValue);
            if (MyDevice.protocol.type == COMP.NET)
            {
                MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
            }
            actXET = MyDevice.actDev;

            myStr = (actXET.angle / 10.0f).ToString("f1");
            mySize = g.MeasureString(myStr, new System.Drawing.Font("Courier New", 40, FontStyle.Bold));
            g.DrawString(myStr, new System.Drawing.Font("Courier New", 40, FontStyle.Bold), myPictures[ucCombox1.SelectedIndex].brush_angle, myPictures[ucCombox1.SelectedIndex].angCentreX - mySize.Width / 2, myPictures[ucCombox1.SelectedIndex].yline_angle);

            //文字扭矩
            myStr = (actXET.torque / 100.0f).ToString("f2");
            mySize = g.MeasureString(myStr, new System.Drawing.Font("Courier New", 40, FontStyle.Bold));
            g.DrawString(myStr, new System.Drawing.Font("Courier New", 40, FontStyle.Bold), myPictures[ucCombox1.SelectedIndex].brush_torque, myPictures[ucCombox1.SelectedIndex].torCentreX - mySize.Width / 2, myPictures[ucCombox1.SelectedIndex].yline_torque);

            //
            if (actXET.modePt == 0)
            {
                //文字角度max
                myStr = (myPictures[ucCombox1.SelectedIndex].angleMax / 10.0f).ToString("f1") + "(MAX)";
                mySize = g.MeasureString(myStr, new System.Drawing.Font("Courier New", 16, FontStyle.Bold));
                g.DrawString(myStr, new System.Drawing.Font("Courier New", 16, FontStyle.Bold), myPictures[ucCombox1.SelectedIndex].brush_angle, myPictures[ucCombox1.SelectedIndex].angCentreX - (mySize.Width / 2), myPictures[ucCombox1.SelectedIndex].yline_anglePeak);

                //文字扭矩max
                myStr = (myPictures[ucCombox1.SelectedIndex].torqueMax / 100.0f).ToString("f2") + "(MAX)";
                mySize = g.MeasureString(myStr, new System.Drawing.Font("Courier New", 16, FontStyle.Bold));
                g.DrawString(myStr, new System.Drawing.Font("Courier New", 16, FontStyle.Bold), myPictures[ucCombox1.SelectedIndex].brush_torque, myPictures[ucCombox1.SelectedIndex].torCentreX - mySize.Width / 2, myPictures[ucCombox1.SelectedIndex].yline_torquePeak);
            }
            else
            {
                //文字角度peak
                myStr = (actXET.anglePeak / 10.0f).ToString("f1") + "(PEAK)";
                mySize = g.MeasureString(myStr, new System.Drawing.Font("Courier New", 16, FontStyle.Bold));
                g.DrawString(myStr, new System.Drawing.Font("Courier New", 16, FontStyle.Bold), myPictures[ucCombox1.SelectedIndex].brush_angle, myPictures[ucCombox1.SelectedIndex].angCentreX - mySize.Width / 2, myPictures[ucCombox1.SelectedIndex].yline_anglePeak);

                //文字扭矩peak
                myStr = (actXET.torquePeak / 100.0f).ToString("f2") + "(PEAK)";
                mySize = g.MeasureString(myStr, new System.Drawing.Font("Courier New", 16, FontStyle.Bold));
                g.DrawString(myStr, new System.Drawing.Font("Courier New", 16, FontStyle.Bold), myPictures[ucCombox1.SelectedIndex].brush_torque, myPictures[ucCombox1.SelectedIndex].torCentreX - mySize.Width / 2, myPictures[ucCombox1.SelectedIndex].yline_torquePeak);
            }

            //文字电量
            mySize = g.MeasureString(myBatStr, new System.Drawing.Font("Courier New", 11));
            g.DrawString(myBatStr, new System.Drawing.Font("Courier New", 11), myPictures[ucCombox1.SelectedIndex].brush_torque_init, myPictures[ucCombox1.SelectedIndex].torCentreX - mySize.Width / 2, myPictures[ucCombox1.SelectedIndex].yline_battery);

            pictureBox1.Image = img;

            g.Dispose();

            //
        }

        //画坐标轴
        private void pictureBoxScope_axis()
        {
            String myAngStr;
            String myTorStr;
            SizeF mySize;

            int left;  //计算刻度线的左右坐标
            int right; //计算刻度线的左右坐标

            MyDevice.protocol.addr = Convert.ToByte(ucCombox1.TextValue);
            if (MyDevice.protocol.type == COMP.NET)
            {
                MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
            }
            actXET = MyDevice.actDev;

            if (actXET.isActive == false)
            {
                return;
            }
            string angle = MyDevice.languageType == 0 ? "角度" : "angle";
            switch (actXET.angleLevel)
            {
                default:
                case 0: myAngStr = angle + "(15°/sec)"; break;
                case 1: myAngStr = angle + "(30°/sec)"; break;
                case 2: myAngStr = angle + "(60°/sec)"; break;
                case 3: myAngStr = angle + "(120°/sec)"; break;
                case 4: myAngStr = angle + "(250°/sec)"; break;
                case 5: myAngStr = angle + "(500°/sec)"; break;
                case 6: myAngStr = angle + "(1000°/sec)"; break;
                case 7: myAngStr = angle + "(2000°/sec)"; break;
            }
            string torque = MyDevice.languageType == 0 ? "扭矩" : "torque";
            switch (actXET.torqueUnit)
            {
                default:
                case UNIT.UNIT_nm: myTorStr = torque + " (N·m)"; break;
                case UNIT.UNIT_lbfin: myTorStr = torque + "(lbf·in)"; break;
                case UNIT.UNIT_lbfft: myTorStr = torque + "(lbf·ft)"; break;
                case UNIT.UNIT_kgcm: myTorStr = torque + "(kgf·cm)"; break;
            }

            //层图
            Bitmap img = new Bitmap(myPictures[ucCombox1.SelectedIndex].Width, myPictures[ucCombox1.SelectedIndex].Height);

            //绘制
            Graphics g = Graphics.FromImage(img);

            //角度圆
            g.DrawEllipse(new Pen(myPictures[ucCombox1.SelectedIndex].color_axis, 4.0f), myPictures[ucCombox1.SelectedIndex].angCxStart, myPictures[ucCombox1.SelectedIndex].angCyStart, myPictures[ucCombox1.SelectedIndex].angDiameter, myPictures[ucCombox1.SelectedIndex].angDiameter);
            g.DrawEllipse(new Pen(myPictures[ucCombox1.SelectedIndex].color_angle_init, 6.0f), myPictures[ucCombox1.SelectedIndex].angCentreX - 3, myPictures[ucCombox1.SelectedIndex].angCentreY - 3, 6, 6);
            //扭矩圆
            g.DrawEllipse(new Pen(myPictures[ucCombox1.SelectedIndex].color_axis, 4.0f), myPictures[ucCombox1.SelectedIndex].torCxStart, myPictures[ucCombox1.SelectedIndex].torCyStart, myPictures[ucCombox1.SelectedIndex].torDiameter, myPictures[ucCombox1.SelectedIndex].torDiameter);
            g.DrawEllipse(new Pen(myPictures[ucCombox1.SelectedIndex].color_torque_init, 6.0f), myPictures[ucCombox1.SelectedIndex].torCentreX - 3, myPictures[ucCombox1.SelectedIndex].torAxisY - 3, 6, 6);

            //角轴轴线
            left = myPictures[ucCombox1.SelectedIndex].angAxisX - (myPictures[ucCombox1.SelectedIndex].boardGap / 4);
            right = myPictures[ucCombox1.SelectedIndex].angAxisX + (myPictures[ucCombox1.SelectedIndex].boardGap / 4);
            g.DrawLine(new Pen(myPictures[ucCombox1.SelectedIndex].color_axis, 1.0f), new Point(myPictures[ucCombox1.SelectedIndex].angAxStart, myPictures[ucCombox1.SelectedIndex].angAxisY), new Point(myPictures[ucCombox1.SelectedIndex].angAxStop, myPictures[ucCombox1.SelectedIndex].angAxisY));
            g.DrawLine(new Pen(myPictures[ucCombox1.SelectedIndex].color_axis, 1.0f), new Point(myPictures[ucCombox1.SelectedIndex].angAxisX, myPictures[ucCombox1.SelectedIndex].angAyStart), new Point(myPictures[ucCombox1.SelectedIndex].angAxisX, myPictures[ucCombox1.SelectedIndex].angAyStop));
            g.DrawLine(new Pen(myPictures[ucCombox1.SelectedIndex].color_axis, 1.0f), new Point(left, myPictures[ucCombox1.SelectedIndex].angLine0), new Point(right, myPictures[ucCombox1.SelectedIndex].angLine0));
            g.DrawLine(new Pen(myPictures[ucCombox1.SelectedIndex].color_axis, 1.0f), new Point(left, myPictures[ucCombox1.SelectedIndex].angLine1), new Point(right, myPictures[ucCombox1.SelectedIndex].angLine1));
            g.DrawLine(new Pen(myPictures[ucCombox1.SelectedIndex].color_axis, 1.0f), new Point(left, myPictures[ucCombox1.SelectedIndex].angLine2), new Point(right, myPictures[ucCombox1.SelectedIndex].angLine2));
            g.DrawLine(new Pen(myPictures[ucCombox1.SelectedIndex].color_axis, 1.0f), new Point(left, myPictures[ucCombox1.SelectedIndex].angLine3), new Point(right, myPictures[ucCombox1.SelectedIndex].angLine3));
            //扭矩轴线
            left = myPictures[ucCombox1.SelectedIndex].torAxisX - (myPictures[ucCombox1.SelectedIndex].boardGap / 4);
            right = myPictures[ucCombox1.SelectedIndex].torAxisX + (myPictures[ucCombox1.SelectedIndex].boardGap / 4);
            g.DrawLine(new Pen(myPictures[ucCombox1.SelectedIndex].color_axis, 1.0f), new Point(myPictures[ucCombox1.SelectedIndex].torAxStart, myPictures[ucCombox1.SelectedIndex].torAxisY), new Point(myPictures[ucCombox1.SelectedIndex].torAxStop, myPictures[ucCombox1.SelectedIndex].torAxisY));
            g.DrawLine(new Pen(myPictures[ucCombox1.SelectedIndex].color_axis, 1.0f), new Point(myPictures[ucCombox1.SelectedIndex].torAxisX, myPictures[ucCombox1.SelectedIndex].torAyStart), new Point(myPictures[ucCombox1.SelectedIndex].torAxisX, myPictures[ucCombox1.SelectedIndex].torAyStop));
            g.DrawLine(new Pen(myPictures[ucCombox1.SelectedIndex].color_axis, 1.0f), new Point(left, myPictures[ucCombox1.SelectedIndex].torLine0), new Point(right, myPictures[ucCombox1.SelectedIndex].torLine0));
            g.DrawLine(new Pen(myPictures[ucCombox1.SelectedIndex].color_axis, 1.0f), new Point(left, myPictures[ucCombox1.SelectedIndex].torLine1), new Point(right, myPictures[ucCombox1.SelectedIndex].torLine1));
            g.DrawLine(new Pen(myPictures[ucCombox1.SelectedIndex].color_axis, 1.0f), new Point(left, myPictures[ucCombox1.SelectedIndex].torLine2), new Point(right, myPictures[ucCombox1.SelectedIndex].torLine2));
            g.DrawLine(new Pen(myPictures[ucCombox1.SelectedIndex].color_axis, 1.0f), new Point(left, myPictures[ucCombox1.SelectedIndex].torLine3), new Point(right, myPictures[ucCombox1.SelectedIndex].torLine3));

            //文字角度文字
            mySize = g.MeasureString(myAngStr, new System.Drawing.Font("Courier New", 11));
            g.DrawString(myAngStr, new System.Drawing.Font("Courier New", 11), myPictures[ucCombox1.SelectedIndex].brush_angle_init, myPictures[ucCombox1.SelectedIndex].angCentreX - mySize.Width / 2, myPictures[ucCombox1.SelectedIndex].yline_angleSpeed);

            //文字扭矩
            mySize = g.MeasureString(myTorStr, new System.Drawing.Font("Courier New", 11));
            g.DrawString(myTorStr, new System.Drawing.Font("Courier New", 11), myPictures[ucCombox1.SelectedIndex].brush_torque_init, myPictures[ucCombox1.SelectedIndex].torCentreX - mySize.Width / 2, myPictures[ucCombox1.SelectedIndex].yline_torqueUnit);

            //铺图
            pictureBox1.BackgroundImage = img;

            //
            g.Dispose();
        }

        //残余扭矩曲线更新
        private void chart_Update()
        {
            chart1.Series[0].Points.Clear();
            torqueList.Clear();
            angleList.Clear();
            ResidualtorqueList.Clear();
            ResidualangleList.Clear();


            foreach (var item in dataInfosToDb)
            {
                if (item.wrench_id == ucCombox1.TextValue)
                {
                    //残余扭矩检测添加位点
                    if (Convert.ToDouble(item.actual_angle.Split(' ')[0]) != 0)
                    {
                        chart1.Series[0].Points.AddXY(Convert.ToDouble(item.actual_angle.Split(' ')[0]), Convert.ToDouble(item.actual_torque.Split(' ')[0]));
                    }
                    else
                    {
                        chart1.Series[0].Points.AddXY(Convert.ToDouble(0.01), Convert.ToDouble(item.actual_torque.Split(' ')[0]));
                    }

                    torqueList.Add(Convert.ToDouble(item.actual_torque.Split(' ')[0]));
                    angleList.Add(Convert.ToDouble(item.actual_angle.Split(' ')[0]));
                }
            }

            //存在一个0证明一次拧紧任务
            if (torqueList.Count(x => x == 0) == 1)
            {
                ResidualtorqueList = torqueList.ToList();
                ResidualangleList = angleList.ToList();
            }
            else if (torqueList.Count(x => x == 0) > 1)
            {
                ResidualtorqueList.Clear();
                ResidualangleList.Clear();

                //取最后一次拧紧任务
                for (int i = FindSecondLastIndex(torqueList, 0); i < torqueList.LastIndexOf(0); i++)
                {
                    ResidualtorqueList.Add(torqueList[i]);
                    ResidualangleList.Add(angleList[i]);
                }
            }
        }

        //显示残余扭矩中的再拧紧扭矩值
        private void chart1_Paint(object sender, PaintEventArgs e)
        {
            if (chart1.Series[0].Points.Count > 10)
            {
                // 获取 Chart 控件的 Graphics 对象
                Graphics g = e.Graphics;

                // 在 Chart 控件上绘制文本
                Font font = new Font("Arial", 12);
                string text = "残余扭矩值: ";
                PointF position = new PointF(pictureBox1.Width / 2, 50);
                if (GetResidualTorque(1, ResidualtorqueList, ResidualangleList) != 0)
                {
                    g.DrawString(text + GetResidualTorque(1, ResidualtorqueList, ResidualangleList), font, Brushes.Gold, position);
                }
                else
                {
                    //扳手拧完存在少量反弹数据（小于10）
                    if (OldResidualtorque != 0)
                    {
                        g.DrawString(text + OldResidualtorque, font, Brushes.Gold, position);
                    }
                }
            }
        }

        //计算再拧紧扭矩值
        private double GetResidualTorque(int mode, List<double> torqueList, List<double> angleList)
        {
            List <double> agvTorqueList = new List<double>();
            List<double> agvAngleList = new List<double>();  
            List<double> slopeList = new List<double>();      //斜率集合
            double avgTorque;
            double avgAngle;
            double slope;
            if (mode == 1)
            {
                //先求8个一组的平均值，再求斜率，最后滤波
                if (torqueList.Count > 10) { 

                    //求平均值
                    for (int i = 0; i < torqueList.Count - 8 + 1; i++)
                    {
                        // 使用 LINQ 的 Skip 和 Take 方法获取List中第 n 到第 m 个元素，并计算它们的平均值
                        // List.Skip(n).Take(m - n + 1).Average();
                        avgTorque = torqueList.Skip(i).Take(8).Average();
                        avgAngle = angleList.Skip(i).Take(8).Average();

                        agvTorqueList.Add(avgTorque);
                        agvAngleList.Add(avgAngle);
                    }
                    //求斜率 △t / △a
                    for (int i = 1; i < agvTorqueList.Count; i++)
                    {
                        slope = (agvTorqueList[i] - agvTorqueList[i - 1]) / (agvAngleList[i] - agvAngleList[i - 1]);
                        slopeList.Add(slope);
                    }
                    //滤波（正 -> 负 中的负值对应的avgTorque）
                    for (int i = 1; i < slopeList.Count; i++)
                    {
                        if (slopeList[i] < 0 && slopeList[i - 1] > 0) {

                            OldResidualtorque = agvTorqueList[i + 1];

                            //斜率集合的数量 = 平均扭矩集合 - 1
                            return agvTorqueList[i + 1];
                        }
                    }
                }
            }

            return 0;
        }

        //集合中指定元素的倒数第二次出现的下标
        public int FindSecondLastIndex(List<double> list, int target)
        {
            int lastIndex = -1;
            int count = 0;

            // 从列表末尾开始向前搜索
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i] == target)
                {
                    count++;
                    if (count == 2)
                    {
                        lastIndex = i;
                        break;
                    }
                }
            }

            return lastIndex;
        }


        //设备切换
        private void ucCombox1_SelectedChangedEvent(object sender, EventArgs e)
        {
            nextTask = uiTask.task_READ_PARA;
            MyDevice.protocol.Protocol_Read_SendCOM(TASKS.READ_PARA);//切换设备打断心跳防止后续读取

            MyDevice.protocol.addr = Convert.ToByte(ucCombox1.TextValue);
            if (MyDevice.protocol.type == COMP.NET)
            {
                MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
            }
            actXET = MyDevice.actDev;

            //曲线恢复
            chart1.ChartAreas[0].AxisX.ScaleView.ZoomReset(0);
            chart1.ChartAreas[0].AxisY.ScaleView.ZoomReset(0);

            //缩放恢复
            myPictures[ucCombox1.SelectedIndex].xline_pick = 0;
            myPictures[ucCombox1.SelectedIndex].getAxis_pictureBox(pictureBox1.Width, pictureBox1.Height);

            pictureBoxScope_axis();
            pictureBoxScope_draw();

            if (ucCombox2.SelectedIndex == 1) chart_Update();
        }

        //曲线模式选择
        private void ucCombox2_SelectedChangedEvent(object sender, EventArgs e)
        {
            this.chart1.Visible = ucCombox2.SelectedIndex == 0 ? false : true;

            //曲线恢复
            chart1.ChartAreas[0].AxisX.ScaleView.ZoomReset(0);
            chart1.ChartAreas[0].AxisY.ScaleView.ZoomReset(0);

            //缩放恢复
            myPictures[ucCombox1.SelectedIndex].xline_pick = 0;
            myPictures[ucCombox1.SelectedIndex].getAxis_pictureBox(pictureBox1.Width, pictureBox1.Height);

            pictureBoxScope_draw();

            if (ucCombox2.SelectedIndex == 1)
            {
                chart_Update();
            }
        }

        //清除数据
        private void bt_Clear_Click(object sender, EventArgs e)
        {
            //清除残余扭矩检测数据
            chart1.Series[0].Points.Clear();

            //标记线清除
            myPictures[ucCombox1.SelectedIndex].xline_pick = 0;

            //表格下标初始化
            table_index = 0;
            lines = 1;

            //初始化下标
            myPictures[ucCombox1.SelectedIndex].bemStart = 10000;
            myPictures[ucCombox1.SelectedIndex].bemStop = -10000;

            dataInfos.Clear();
            dicTorque.Clear();
            dicIndex.Clear();
            dicAngle.Clear();
            dataInfosToDb.Clear();
            dataGridView1.Rows.Clear();
            myPictures[ucCombox1.SelectedIndex].Clear();

            torqueList.Clear();
            angleList.Clear();
            ResidualtorqueList.Clear();
            ResidualangleList.Clear();
            OldResidualtorque = 0;
        }

        //鼠标悬停在残余扭矩曲线上显示坐标
        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            chart1.Series[0].ToolTip = "角度(X)：#VALX\n扭矩(Y)：#VALY";
        }

        //鼠标滚动控制放大缩小
        private void PictureBox_Show_MouseWheel(object sender, MouseEventArgs e)
        {
            myPictures[ucCombox1.SelectedIndex].old_rate = myPictures[ucCombox1.SelectedIndex].rate;
            int rate_x = e.X;
            int x_t;
            if (e.Delta > 0)
            {
                if (myPictures[ucCombox1.SelectedIndex].rate >= 1)
                {
                    myPictures[ucCombox1.SelectedIndex].rate *= 1.2f;
                    myPictures[ucCombox1.SelectedIndex].rate = (float)Math.Round(myPictures[ucCombox1.SelectedIndex].rate, 2);//四舍五入，保留两位小数
                    if (myPictures[ucCombox1.SelectedIndex].rate >= 5.18f)
                    {
                        myPictures[ucCombox1.SelectedIndex].rate = 5.18f;
                    }
                }
                else
                {
                    myPictures[ucCombox1.SelectedIndex].rate += 0.1f;
                }
            }
            if (e.Delta < 0)
            {
                if (myPictures[ucCombox1.SelectedIndex].rate > 1)
                {
                    myPictures[ucCombox1.SelectedIndex].rate /= 1.2f;
                }
                else
                {
                    myPictures[ucCombox1.SelectedIndex].rate -= 0.1f;
                    if (myPictures[ucCombox1.SelectedIndex].rate <= 0.5f)
                    {
                        myPictures[ucCombox1.SelectedIndex].rate = 0.5f;
                    }
                }
                myPictures[ucCombox1.SelectedIndex].rate = (float)Math.Round(myPictures[ucCombox1.SelectedIndex].rate, 2);
            }
            //清除轴线
            myPictures[ucCombox1.SelectedIndex].xline_pick = 0;

            //计算放大位置
            x_t = (int)Math.Round((rate_x - myPictures[ucCombox1.SelectedIndex].pointStart) / myPictures[ucCombox1.SelectedIndex].old_rate, 0) + myPictures[ucCombox1.SelectedIndex].start_x;
            x_t = x_t > 0 ? x_t - (int)Math.Round((rate_x - myPictures[ucCombox1.SelectedIndex].pointStart) / myPictures[ucCombox1.SelectedIndex].rate, 0) : 0;
            x_t = x_t > 0 ? x_t : 0;
            myPictures[ucCombox1.SelectedIndex].start_x = x_t;

            //更新曲线计算
            myPictures[ucCombox1.SelectedIndex].getPoint_pictureBox(actXET.REC);
        }

        //点击图片，显示轴线
        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            //取消选中
            for (int j = 0; j < dataGridView1.SelectedRows.Count; j++)
            {
                dataGridView1.SelectedRows[j].Selected = false;
            }
            //获取y轴坐标
            myPictures[ucCombox1.SelectedIndex].yline_pick = e.Y;
            //获取选中的时间戳
            string str = myPictures[ucCombox1.SelectedIndex].getViewIdx_pictureBox(e.X);

            //设置是否选中的数据在表中是没有的
            if (str == null || str == "")
            {
                myPictures[ucCombox1.SelectedIndex].xline_pick = 0;
            }
            else
            {
                bool b_axis = false;
                if (actXET.modePt == 1)
                {
                    for (int i = 0; i < dataGridView1.RowCount; i++)
                    {
                        if (str.Equals(dataGridView1.Rows[i].Cells[1].Value.ToString()))
                        {
                            //获得峰值
                            torqueMax = dicTorque[dataGridView1.Rows[i].Cells[2].Value.ToString()];
                            //选中表格
                            dataGridView1.Rows[i].Selected = true;
                            b_axis = true;
                            bemPick = i;
                            //移到表格
                            if (i > 15)
                            {
                                dataGridView1.FirstDisplayedScrollingRowIndex = i - 15;
                            }
                            else
                            {
                                dataGridView1.FirstDisplayedScrollingRowIndex = 0;
                            }
                            break;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < dataInfos.Count; i++)
                    {
                        if (str.Equals(dataInfos[i].time))
                        {
                            torqueMax = dicTorque[dataInfos[i].work_vin];
                            bemPick = i;
                            b_axis = true;
                            break;
                        }
                    }
                }
                if (!b_axis)
                {
                    myPictures[ucCombox1.SelectedIndex].xline_pick = 0;
                }
            }
            //画顶层
            pictureBoxScope_draw();
        }

        //窗口缩放重绘
        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            if (ucCombox1.SelectedIndex != -1)
            {
                myPictures[ucCombox1.SelectedIndex].xline_pick = 0;
                myPictures[ucCombox1.SelectedIndex].getAxis_pictureBox(pictureBox1.Width, pictureBox1.Height);

                pictureBoxScope_axis();
                pictureBoxScope_draw();
            }
        }

        //曲线缩放
        private void chart_MouseWheel(object sender, MouseEventArgs e)
        {
            //Chart chart1 = sender as Chart;
            //try
            //{
            //    //鼠标向上滚的Delta值是大于0，向下是小于0
            //    if (e.Delta > 0)
            //    {
            //        if (chart1.ChartAreas[0].AxisX.ScaleView.Size > 0 && chart1.ChartAreas[0].AxisY.ScaleView.Size > 0)
            //        {
            //            chart1.ChartAreas[0].AxisX.ScaleView.Size /= 2;//每页显示的点数除以2实现放大效果
            //            chart1.ChartAreas[0].AxisY.ScaleView.Size /= 2;
            //        }
            //        else
            //        {
            //            chart1.ChartAreas[0].AxisX.ScaleView.Size = chart1.Series[0].Points.Count / 2;//首次滚动时size为NaN
            //            chart1.ChartAreas[0].AxisY.ScaleView.Size = chart1.Series[0].Points.Count / 2;
            //        }
            //    }
            //    else if (e.Delta < 0)
            //    {
            //        if (chart1.ChartAreas[0].AxisX.ScaleView.Size > 0 && chart1.ChartAreas[0].AxisY.ScaleView.Size > 0)
            //        {
            //            chart1.ChartAreas[0].AxisX.ScaleView.Size *= 2;
            //            chart1.ChartAreas[0].AxisY.ScaleView.Size *= 2;
            //        }
            //        else
            //        {
            //            chart1.ChartAreas[0].AxisX.ScaleView.Size = chart1.Series[0].Points.Count * 2;
            //            chart1.ChartAreas[0].AxisY.ScaleView.Size = chart1.Series[0].Points.Count * 2;
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}
        }

        //曲线缩放恢复
        private void chart1_MouseClick(object sender, MouseEventArgs e)
        {
            //右键恢复事件
            if (e.Button == MouseButtons.Right)
            {
                chart1.ChartAreas[0].AxisX.ScaleView.ZoomReset(0);
                chart1.ChartAreas[0].AxisY.ScaleView.ZoomReset(0);
            }
        }

        //唤醒定时器
        private void wakeTimer_Tick(object sender, EventArgs e)
        {
            //关闭正在执行的定时器
            timer1.Enabled = false;

            //向已连接设备轮流发送readPara
            if (ucCombox1.Source.Count > 1)
            {
                foreach (var item in ucCombox1.Source)
                {
                    MyDevice.protocol.addr = Convert.ToByte(item.Value); if (MyDevice.protocol.type == COMP.NET)
                    {
                        MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
                    }
                    MyDevice.protocol.Protocol_Read_SendCOM(TASKS.READ_PARA);
                }
            }
            //重启读心跳
            timer1.Enabled = true;
        }


        //鼠标右击选择生成csv文件
        private void dataGridView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && dataGridView1.Rows.Count > 0)
            {
                DialogResult result = MessageBox.Show($"是否保存数据导出?", "确认保存", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    string datFolderPath = Application.StartupPath + @"\dat"; //导出目录

                    if (!Directory.Exists(datFolderPath))
                    {
                        Directory.CreateDirectory(datFolderPath);
                    }

                    System.Windows.Forms.SaveFileDialog DialogSave = new System.Windows.Forms.SaveFileDialog();
                    DialogSave.Filter = "Excel(*.csv)|*.csv";
                    DialogSave.InitialDirectory = datFolderPath;     //默认路径
                    DialogSave.FileName = DateTime.Now.ToString("yyyyMMddHHmmss");
                    
                    if (DialogSave.ShowDialog() == DialogResult.OK)
                    {
                        string myExcel = DialogSave.FileName;  //导出文件名
                        if (saveActualDataToExcel(myExcel))
                        {
                            if (MyDevice.languageType == 0)
                            {
                                MessageBox.Show("导出数据成功！");
                            }
                            else
                            {
                                MessageBoxEX.Show("Export  successfully！", "Hint", MessageBoxButtons.OK, new string[] { "OK" });
                            }
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                else
                    return;
            }
        }

        //数据表保存为csv文件
        public bool saveActualDataToExcel(string mePath)
        {
            //空
            if (mePath == null)
            {
                return false;
            }

            //写入
            try
            {
                //excel的每一行
                var lines = new List<string>();

                lines.Add("序号,时间,作业号VIN,扳手ID,实时扭矩,实时角度,扭矩峰值,角度峰值");
                DataInfo dataInfoToExcel = new DataInfo();

                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    dataInfoToExcel = new DataInfo();

                    dataInfoToExcel.id = Convert.ToInt32(dataGridView1.Rows[i].Cells[0].Value.ToString());
                    dataInfoToExcel.time = dataGridView1.Rows[i].Cells[1].Value.ToString();
                    dataInfoToExcel.work_vin = dataGridView1.Rows[i].Cells[2].Value.ToString();
                    dataInfoToExcel.wrench_id = dataGridView1.Rows[i].Cells[3].Value.ToString();
                    dataInfoToExcel.actual_torque = dataGridView1.Rows[i].Cells[4].Value.ToString();
                    dataInfoToExcel.actual_angle = dataGridView1.Rows[i].Cells[5].Value.ToString();
                    dataInfoToExcel.torque_peak = dataGridView1.Rows[i].Cells[6].Value.ToString();
                    dataInfoToExcel.angle_peak = dataGridView1.Rows[i].Cells[7].Value.ToString();

                    //加=\，使csv文件用excel打开时能正常显示数据
                    lines.Add($"{dataInfoToExcel.id},=\"{dataInfoToExcel.time}\",=\"{dataInfoToExcel.work_vin}\",=\"{dataInfoToExcel.wrench_id}\"," +
                              $"{dataInfoToExcel.actual_torque},{dataInfoToExcel.actual_angle},{dataInfoToExcel.torque_peak},{dataInfoToExcel.angle_peak},");
                }

                File.WriteAllLines(mePath, lines, System.Text.Encoding.Default);
                System.IO.File.SetAttributes(mePath, FileAttributes.ReadOnly);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}