using BIL;
using HZH_Controls;
using Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;

//Lumi 20230816
//Junzhe 20230906
//Ricardo 20231205
//Ricardo 20240108

namespace Base.UI.MenuTools
{
    public partial class MenuShowStrategyForm : Form
    {
        #region 定义变量

        private List<Color> colorOptions = new List<Color>()
        {
            Color.IndianRed,
            Color.Orange,
            Color.MediumSeaGreen,
            Color.MediumBlue,
            Color.MediumVioletRed,
            Color.MediumTurquoise,
            Color.MediumPurple,
            Color.LightPink,
            Color.Silver,
            Color.CadetBlue,
        };

        Font fontTitle = new Font("微软雅黑", 18, FontStyle.Bold, GraphicsUnit.Point);
        Font fontTitleLen3 = new Font("微软雅黑", 16, FontStyle.Bold, GraphicsUnit.Point);
        Font fontBubble = new Font("微软雅黑", 15, FontStyle.Regular, GraphicsUnit.Point);
        Font fontBigTitle = new Font("微软雅黑", 58, FontStyle.Bold, GraphicsUnit.Point);
        Font fontBigTitleLen3 = new Font("微软雅黑", 47, FontStyle.Bold, GraphicsUnit.Point);

        private Size currentSize;                                     //窗口大小
        private string mePicLocation = "";                            //图片路径
        private string importWorkOrderId = "";                        //要导入的工单表表名

        int selectIndex = -1;                                         //选择的位点下标
        int selectNum = -1;                                           //选择的拧紧序号

        private List<Tuple<int, Point>> pointList = new List<Tuple<int, Point>>();                //Item1:拧紧序号, Item2:选点位置
        private List<Tuple<int, Rectangle>> bubbleList = new List<Tuple<int, Rectangle>>();       //Item1:拧紧序号, Item2:气泡范围
        private List<KeyValuePair<int, Color>> colorList = new List<KeyValuePair<int, Color>>();  //Item1:拧紧序号, Item2:气泡颜色

        //Item1:拧紧序号, Item2:扳手编码，Item3:扳手ID
        private List<Tuple<string, string, string>> meWrenchCodeAndID = new List<Tuple<string, string, string>>();
        //Item1:拧紧序号, Item2:A1A2模式, Item3:PT模式, Item4:单位, Item5:M0~M9模式, Item6:角度挡位
        private List<Tuple<string, string, string, string, string, string>> meWrenchMessage = new List<Tuple<string, string, string, string, string, string>>();
        //Item1:拧紧序号, Item2:目标力矩, Item3:目标角度, Item4:数据记录模式
        private List<Tuple<string, string, string, string>> meTorqueAngleAndSetMode = new List<Tuple<string, string, string, string>>();

        JDBC jdbc = new JDBC();

        private XET actXET;              //需操作的设备
        private string oldActiveForm;
        private volatile TASKS nextTask; //按键操作指令
        private byte meWrenchID;         //选择的位点的扳手id

        private byte comTicker = 0;      //发送指令间隔
        private byte scanIndex = 0;      //扫描扳手指针

        List<DataInfo> dataInfos = new List<DataInfo>();      //数据表
        List<DataInfo> dataInfosToDb = new List<DataInfo>();  //实际存储到数据库的数据表

        private Dictionary<string, float> dicTorque = new Dictionary<string, float>();  //记录扭矩峰值
        private Dictionary<string, float> dicAngle = new Dictionary<string, float>();   //记录角度峰值
        private Dictionary<string, string> dicIndex = new Dictionary<string, string>(); //记录数据段（以流水号为界）

        private int tickIndex = 0;            //为1时画"√"
        private int fockIndex = 0;            //为1时画"×"
        private int normalIndex = 0;          //为1时画序号
        private int table_index = 0;          //读取表格下标
        private bool isZero = false;          //是否归零
        private bool scanOver = false;        //是否扫描结束
        private bool isBatAlarm = false;      //是否电量报警
        private bool isOvertake = false;      //是否超出上限
        private bool isAutoSwitch = false;    //是否自动切换位点

        private float anglePeak = 0f;         //记录扭矩峰值
        private float torquePeak = 0f;        //记录角度峰值
        private string torqueUnit;            //扭矩单位

        private string res = "no";            //结果
        private string torqueResult = "no";   //扭矩结果
        private string angleResult = "no";    //角度结果
        private string lastWorkVin = "";      //前一个结果的流水号

        private List<int> screwIDList = new List<int>();   //该工单的拧紧序号
        private int autoSelectedNum = 0;                   //当前自动选择的拧紧序号
        private List<string> addressList = new List<string>(); //该工单的扳手站点集合

        private Timer wakeTimer = new Timer();             //唤醒定时器（防止扳手因长时间不用而休眠）

        private Int16 readParaCnt = 0;                     //读参数指令发送次数（次数过大证明是掉线而非指令回复慢）
        private Int16 writeParaCnt = 0;                    //写参数指令发送次数
        private Int16 readHeartCnt = 0;                    //读心跳指令发送次数

        #endregion

        public MenuShowStrategyForm()
        {
            InitializeComponent();
        }

        private void MenuShowStrategyForm_Load(object sender, EventArgs e)
        {
            #region 定义下拉框
            //数据记录模式
            List<KeyValuePair<string, string>> setMode = new List<KeyValuePair<string, string>>();
            setMode.Add(new KeyValuePair<string, string>("0", "不缓存"));
            setMode.Add(new KeyValuePair<string, string>("1", "缓存TRACK"));
            setMode.Add(new KeyValuePair<string, string>("2", "缓存PEAK"));

            ucCombox1.Source = setMode;
            ucCombox1.SelectedIndex = 0;
            ucCombox1.ConerRadius = 2;
            ucCombox1.RectColor = SystemColors.GradientActiveCaption;

            //A1A2模式
            List<KeyValuePair<string, string>> A1A2Mode = new List<KeyValuePair<string, string>>();
            A1A2Mode.Add(new KeyValuePair<string, string>("0", "A1"));
            A1A2Mode.Add(new KeyValuePair<string, string>("1", "A2"));

            ucCombox2.Source = A1A2Mode;
            ucCombox2.SelectedIndex = 0;
            ucCombox2.ConerRadius = 2;
            ucCombox2.RectColor = SystemColors.GradientActiveCaption;

            //PT模式
            List<KeyValuePair<string, string>> PTMode = new List<KeyValuePair<string, string>>();
            PTMode.Add(new KeyValuePair<string, string>("0", "TRACK"));
            PTMode.Add(new KeyValuePair<string, string>("1", "PEAK"));

            ucCombox3.Source = PTMode;
            ucCombox3.SelectedIndex = 0;
            ucCombox3.ConerRadius = 2;
            ucCombox3.RectColor = SystemColors.GradientActiveCaption;

            //单位
            List<KeyValuePair<string, string>> unit = new List<KeyValuePair<string, string>>();
            unit.Add(new KeyValuePair<string, string>("0", "N·m"));
            unit.Add(new KeyValuePair<string, string>("1", "lbf·in"));
            unit.Add(new KeyValuePair<string, string>("2", "lbf·ft"));
            unit.Add(new KeyValuePair<string, string>("3", "kgf·cm"));

            ucCombox4.Source = unit;
            ucCombox4.SelectedIndex = 0;
            ucCombox4.ConerRadius = 2;
            ucCombox4.RectColor = SystemColors.GradientActiveCaption;

            //M0~M9模式
            List<KeyValuePair<string, string>> M0M9Mode = new List<KeyValuePair<string, string>>();
            M0M9Mode.Add(new KeyValuePair<string, string>("0", "M0"));
            M0M9Mode.Add(new KeyValuePair<string, string>("1", "M1"));
            M0M9Mode.Add(new KeyValuePair<string, string>("2", "M2"));
            M0M9Mode.Add(new KeyValuePair<string, string>("3", "M3"));
            M0M9Mode.Add(new KeyValuePair<string, string>("4", "M4"));
            M0M9Mode.Add(new KeyValuePair<string, string>("5", "M5"));
            M0M9Mode.Add(new KeyValuePair<string, string>("6", "M6"));
            M0M9Mode.Add(new KeyValuePair<string, string>("7", "M7"));
            M0M9Mode.Add(new KeyValuePair<string, string>("8", "M8"));
            M0M9Mode.Add(new KeyValuePair<string, string>("9", "M9"));

            ucCombox5.Source = M0M9Mode;
            ucCombox5.SelectedIndex = 0;
            ucCombox5.ConerRadius = 2;
            ucCombox5.RectColor = SystemColors.GradientActiveCaption;

            //角度挡位
            List<KeyValuePair<string, string>> angle = new List<KeyValuePair<string, string>>();
            angle.Add(new KeyValuePair<string, string>("0", "15°/sec"));
            angle.Add(new KeyValuePair<string, string>("1", "30°/sec"));
            angle.Add(new KeyValuePair<string, string>("2", "60°/sec"));
            angle.Add(new KeyValuePair<string, string>("3", "120°/sec"));
            angle.Add(new KeyValuePair<string, string>("4", "250°/sec"));
            angle.Add(new KeyValuePair<string, string>("5", "500°/sec"));
            angle.Add(new KeyValuePair<string, string>("6", "1000°/sec"));
            angle.Add(new KeyValuePair<string, string>("7", "2000°/sec"));

            ucCombox6.Source = angle;
            ucCombox6.SelectedIndex = 0;
            ucCombox6.ConerRadius = 2;
            ucCombox6.RectColor = SystemColors.GradientActiveCaption;

            #endregion

            #region  定义TextBox

            textBox2.ReadOnly = true;
            textBox1.ReadOnly = true;
            textBox5.ReadOnly = true;
            textBox6.ReadOnly = true;
            textBox7.ReadOnly = true;

            #endregion

            oldActiveForm = Main.ActiveForm;
            Main.ActiveForm = "MenuShowStrategyForm";

            actXET = MyDevice.actDev;

            MyDevice.myUpdate += new freshHandler(receiveData);   //事件委托

            //唤醒定时器初始化
            wakeTimer.Interval = 180000; //间隔三分钟发送一次唤醒
            wakeTimer.Tick += wakeTimer_Tick;
            wakeTimer.Enabled = true;
            wakeTimer.Start();
        }

        //界面关闭
        private void MenuShowStrategyForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (timer1.Enabled == true)
            {
                DialogResult result = MyDevice.languageType == 0 ?
                    MessageBox.Show("工单操作中, 确认退出？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question):
                    MessageBox.Show("During the ticket operation, confirm the exit？", "prompt", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }

            this.ucSignalLamp1.LampColor = new System.Drawing.Color[] { System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(59))))) };

            timer1.Enabled = false;

            wakeTimer.Enabled = false;

            MyDevice.myUpdate -= new freshHandler(receiveData);

            Main.ActiveForm = oldActiveForm;
        }

        //界面大小变化事件
        private void MenuSetWorkOrderForm_SizeChanged(object sender, EventArgs e)
        {
            // 获取新大小
            Size newSize = pictureBox1.Size;

            // 如果是第一次调用SizeChanged事件，将旧大小设置为新大小
            if (currentSize == Size.Empty)
            {
                currentSize = newSize;
                return;
            }

            updatePictureBox1(newSize, currentSize);

            // 更新旧大小为新大小
            currentSize = newSize;
        }

        //调整位点位置
        private void updatePictureBox1(Size newSize, Size oldSize)
        {
            label15.SendToBack();
            label16.SendToBack();

            if (newSize != oldSize)
            {
                // 计算宽度和高度的缩放比例
                float widthScale = (float)newSize.Width / oldSize.Width;
                float heightScale = (float)newSize.Height / oldSize.Height;

                // 遍历pointList中的每个元素，将每个点的坐标乘以缩放比例
                for (int i = 0; i < pointList.Count; i++)
                {
                    Point oldPoint = pointList[i].Item2;
                    Point newPoint = new Point((int)(oldPoint.X * widthScale), (int)(oldPoint.Y * heightScale));
                    pointList[i] = new Tuple<int, Point>(pointList[i].Item1, newPoint);
                }
            }

            //重绘
            bubbleList.Clear();

            Bitmap bubbles = new Bitmap(pictureBox1.ClientSize.Width, pictureBox1.ClientSize.Height);
            Graphics graphics = Graphics.FromImage(bubbles);
            for (int i = 0; i < pointList.Count; i++)
            {
                //绘制气泡框形状的图形、圆形和序号
                DrawBubbleAndCircle(graphics, pointList[i].Item1, pointList[i].Item2);

                //在气泡上绘制文字
                DrawStrOnBubble(graphics, pointList[i], meTorqueAngleAndSetMode);
            }
            //画左上角大圈
            if (selectIndex > -1 && selectIndex < bubbleList.Count)
            {
                DrawBubbleAndCircle_Max(graphics, pointList[selectIndex].Item1);
            }
            graphics.Save();
            graphics.Dispose();
            pictureBox1.Image = bubbles;
        }

        //导入工单
        private void buttonX1_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled == true)
            {
                DialogResult result = MyDevice.languageType == 0 ?
                    MessageBox.Show("工单操作中, 确认导入工单？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question):
                    MessageBox.Show("During the ticket operation, confirm the import of the ticket？", "prompt", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                {
                    return;
                }
            }

            //导入工单弹窗
            MenuImportWorkOrderForm myMenuImportWorkOrderForm = new MenuImportWorkOrderForm();
            myMenuImportWorkOrderForm.StartPosition = FormStartPosition.CenterParent;
            this.BringToFront();

            if (myMenuImportWorkOrderForm.ShowDialog() == DialogResult.OK)
            {
                importWorkOrderId = myMenuImportWorkOrderForm.TextBox1Value;
            }
            else
            {
                return;
            }
            if (importWorkOrderId.Equals("")) return;

            jdbc.TableName = importWorkOrderId;
            List<WorkInfo> workInfoList = jdbc.QueryFullWorkInfo();   //从数据库读取工单表
            if (workInfoList == null)
            {
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("该工单未存储位点信息", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("The ticket does not store site information", "System prompt", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return;
            }

            dataTuplesClear();                                        //清除各tuple list的值
            pointList.Clear();
            meWrenchCodeAndID.Clear();
            meTorqueAngleAndSetMode.Clear();
            meWrenchMessage.Clear();
            screwIDList.Clear();

            selectIndex = -1;                                         //选择的位点下标
            selectNum = -1;                                           //选择的拧紧序号

            currentSize = pictureBox1.Size;
            mePicLocation = workInfoList[0].pic_location;
            Size dbSize = Size.Empty;                                 //数据库存储的picturebox大小
            string sizeStr = workInfoList[0].pic_news.Split('|')[1];
            if (sizeStr.StartsWith("{") && sizeStr.EndsWith("}"))
            {
                string[] coordinates = sizeStr.Trim('{', '}').Split(',');
                if (coordinates.Length == 2)
                {
                    int x = int.Parse(coordinates[0].Split('=')[1]);
                    int y = int.Parse(coordinates[1].Split('=')[1]);
                    dbSize = new Size(x, y);
                }
            }

            foreach (WorkInfo wi in workInfoList)
            {
                Point dbPoint = Point.Empty;
                string pointStr = wi.pic_news.Split('|')[0];
                string[] wrenchMessageStrs = wi.wrench_message.Split('|');

                //获取点
                if (pointStr.StartsWith("{") && pointStr.EndsWith("}"))
                {
                    string[] coordinates = pointStr.Trim('{', '}').Split(',');
                    if (coordinates.Length == 2)
                    {
                        int x = int.Parse(coordinates[0].Split('=')[1]);
                        int y = int.Parse(coordinates[1].Split('=')[1]);
                        dbPoint = new Point(x, y);
                    }
                }

                Tuple<int, Point> pointTuple = new Tuple<int, Point>(int.Parse(wi.screw_id), dbPoint);
                Tuple<string, string, string> meWrenchCodeAndIDTuple = new Tuple<string, string, string>(wi.screw_id, wi.wrench_code, wi.wrench_id);
                Tuple<string, string, string, string> torqueAngleSetModeTuple = new Tuple<string, string, string, string>(wi.screw_id, wi.target_torque, wi.target_angle, wi.set_mode);
                Tuple<string, string, string, string, string, string> wrenchMessageTuple = new Tuple<string, string, string, string, string, string>
                                                                                           (wi.screw_id, wrenchMessageStrs[0], wrenchMessageStrs[1], wrenchMessageStrs[2], wrenchMessageStrs[3], wrenchMessageStrs[4]);

                pointList.Add(pointTuple);
                meWrenchCodeAndID.Add(meWrenchCodeAndIDTuple);
                meTorqueAngleAndSetMode.Add(torqueAngleSetModeTuple);
                meWrenchMessage.Add(wrenchMessageTuple);
                updateColorList(int.Parse(wi.screw_id));
            }

            //获取所有拧紧序号
            foreach (var tuple in meWrenchCodeAndID)
            {
                if (int.TryParse(tuple.Item1, out int result))
                {
                    screwIDList.Add(result);
                }
                else
                {
                    continue;
                }
            }
            screwIDList.Sort();    //升序排序

            //获取所有扳手站点ID
            foreach(var item in meWrenchCodeAndID)
            {
                if (!addressList.Contains(item.Item3))
                {
                    addressList.Add(item.Item3);
                }
            }

            string picFolderPath = Application.StartupPath + @"\pic";
            if (!Directory.Exists(picFolderPath))
            {
                Directory.CreateDirectory(picFolderPath);
            }

            //画图
            try
            {
                pictureBox1.BackgroundImage = Image.FromFile(Application.StartupPath + mePicLocation);
            }
            catch (Exception ex)
            {
                MessageBox.Show("当前工单所需图片缺少，请重新建立工单");
                return;
            }
            pictureBox1.BackgroundImageLayout = ImageLayout.Zoom;
            updatePictureBox1(currentSize, dbSize);

            textBox3.Visible = true;
            if (MyDevice.languageType == 0)
            {
                textBox3.Text = "工单号：" + importWorkOrderId;
            }
            else
            {
                textBox3.Left = 435;
                textBox3.Text = "Work Order ID:" + importWorkOrderId;
            }

            //将工单中所有扳手状态初始化
            if (MyDevice.protocol.type == COMP.XF)
            {
                for (int i = 0; i < meWrenchCodeAndID.Count; i++)
                {
                    MyDevice.mXF[Convert.ToInt32(meWrenchCodeAndID[i].Item3)].sTATE = STATE.OFFLINE;
                }
            }
            else if (MyDevice.protocol.type == COMP.NET)
            {
                for (int i = 0; i < meWrenchCodeAndID.Count; i++)
                {
                    MyDevice.mNET[Convert.ToInt32(meWrenchCodeAndID[i].Item3)].sTATE = STATE.OFFLINE;
                }
            }

            //扫描工单中所有扳手是否在线
            scanOver = false;
            timer2.Enabled = true;
            timer1.Enabled = false;
            nextTask = TASKS.READ_PARA;
        }

        //操作完成
        private void buttonX2_Click(object sender, EventArgs e)
        {
            if (pictureBox1.BackgroundImage == null) return;
            if (meWrenchCodeAndID == null) return;
            if (!textBox5.Text.Equals(meWrenchCodeAndID[meWrenchCodeAndID.Count - 1].Item1)) return;

            this.ucSignalLamp1.LampColor = new System.Drawing.Color[] { System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(59))))) };

            timer1.Enabled = false;

            MyDevice.myUpdate -= new freshHandler(receiveData);   //事件委托

            //初始化各list、dic
            dataInfos.Clear();
            dataInfosToDb.Clear();
            dicTorque.Clear();
            dicAngle.Clear();
            dicIndex.Clear();
            table_index = 0;     //读取表格下标
            isZero = false;      //是否归零
        }

        //左键选位点
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            //设备扫描未完成直接点击选位点
            if (MyDevice.devSum < addressList.Count)
            {
                return;
            }

            if (pictureBox1.BackgroundImage == null) return;

            selectIndex = -1;                                 //选择的位点下标
            selectNum = -1;                                   //选择的拧紧序号

            MouseEventArgs mouseEventArgs = e as MouseEventArgs;
            Point clickPoint = mouseEventArgs.Location;       //鼠标点击的点位置

            if (mouseEventArgs.Button == MouseButtons.Right) return;

            //判断鼠标点击的位置是否在气泡和圆形的区域内
            for (int i = bubbleList.Count - 1; i > -1; i--)
            {
                if (bubbleList[i].Item2.Contains(clickPoint) || IsPointInCircle(clickPoint, bubbleList[i].Item2.X, bubbleList[i].Item2.Y))
                {
                    selectIndex = i;
                    selectNum = bubbleList[selectIndex].Item1;
                    break;
                }
            }
            if (selectIndex == -1) return;

            //将选择的位点气泡置于最前
            if (bubbleList.Count > 1)
            {
                bubbleList = MoveElementToEnd(bubbleList, selectIndex);
                pointList = MoveElementToEnd(pointList, selectIndex);
                selectIndex = pointList.Count - 1;
            }

            // 重新绘制所有圆形
            updatePictureBox1(currentSize, currentSize);

            //更新左侧边栏
            updateLeftInfo(selectNum);

            //点击其他位点时，上一次操作结束
            this.ucSignalLamp1.LampColor = new System.Drawing.Color[] { System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(59))))) };
            if (timer1.Enabled == true)
            {
                timer1.Enabled = false;
            }

            //更新参数
            timer1.Enabled = true;
            updatePara();
        }

        //扫描扳手是否全部在线
        private void scanStatus() 
        {
            nextTask = TASKS.NULL;
            timer2.Enabled = false;
        
            //存储掉线设备的地址
            List<string> offAddr = new List<string>();

            //获取掉线设备的地址
            if (MyDevice.protocol.type == COMP.XF)
            {
                for (Byte i = 0; i < meWrenchCodeAndID.Count; i++)
                {
                    if (MyDevice.mXF[Convert.ToInt32(meWrenchCodeAndID[i].Item3)].sTATE != STATE.CONNECTED)
                    {
                        offAddr.Add(meWrenchCodeAndID[i].Item3);
                    }
                }
            }
            else if (MyDevice.protocol.type == COMP.NET)
            {
                for (Byte i = 0; i < meWrenchCodeAndID.Count; i++)
                {
                    if (MyDevice.mNET[Convert.ToInt32(meWrenchCodeAndID[i].Item3)].sTATE != STATE.CONNECTED)
                    {
                        offAddr.Add(meWrenchCodeAndID[i].Item3);
                    }
                }
            }

            //有掉线设备
            if (offAddr.Count > 0)
            {
                offAddr.Sort();
                var result = String.Join("，", offAddr.Distinct().ToArray());

                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("设备" + result + "已掉线，请检查连接后重新导入工单！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("Device" + result + "has been dropped, please check the connection and re-import the ticket!", "Hint", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else if (offAddr.Count == 0)
            {
                //扫描结束
                scanOver = true;

                //导入工单后直接开始第一个点位
                workOrder_Start();
            }
        }

        //导入工单直接开始第一个点位
        private void workOrder_Start()
        {
            if (pictureBox1.Image == null) return;

            selectIndex = pointList.FindIndex(item => item.Item1 == screwIDList[0]); //选择的位点下标
            selectNum = screwIDList[0]; //选择的拧紧序号

            //将选择的位点气泡置于最前
            if (bubbleList.Count > 1)
            {
                bubbleList = MoveElementToEnd(bubbleList, selectIndex);
                pointList = MoveElementToEnd(pointList, selectIndex);
                selectIndex = pointList.Count - 1;
            }

            // 清空PictureBox并重新绘制所有圆形
            updatePictureBox1(currentSize, currentSize);

            //更新左侧边栏
            updateLeftInfo(selectNum);

            //点击其他位点时，上一次操作结束
            this.ucSignalLamp1.LampColor = new System.Drawing.Color[] { System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(59))))) };
            if (timer1.Enabled == true)
            {
                timer1.Enabled = false;
            }

            //更新参数
            timer1.Enabled = true;
            updatePara();
        }

        #region 定时器、receiveData委托

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
                    //MessageBox.Show("MenuRunForm receiveData err 3");
                }
            }
            //本线程的操作请求
            else
            {             
                switch (nextTask)
                {
                    case TASKS.READ_PARA:
                        comTicker = 0;

                        if (scanOver)
                        {
                            //归零初始化
                            fockIndex = 0;
                            anglePeak = 0;
                            torquePeak = 0;

                            if (isAutoSwitch && isOvertake == false)
                            {
                                //满足自动切换条件
                                AutoSwitch();
                            }
                            else
                            {
                                //继续读心跳
                                isAutoSwitch = false;
                                //timer1.Enabled = true;
                                nextTask = TASKS.READ_HEART;
                            }
                        }
                        else
                        {
                            //防止指令回复较慢，导致连续发送两条相同指令，正确的发送指令收到的是上一条指令回复
                            //新增地址校验，仅指令回复地址之后设备站点才会正常更新
                            if (MyDevice.protocol.addr == Convert.ToByte(addressList[scanIndex]))
                            {
                                scanIndex++;
                                readParaCnt = 0;
                            }

                            //轮询
                            if (scanIndex == addressList.Count)
                            {
                                scanIndex = 0;
                                scanStatus();

                                //timer1.Enabled = true;
                            }
                        }
                        break;

                    case TASKS.WRITE_A1M01DAT:
                        this.ucSignalLamp1.LampColor = new Color[] { Color.Green };
                        comTicker = 0;

                        nextTask = TASKS.WRITE_PARA;
                        MyDevice.protocol.Protocol_ClearState();
                        MyDevice.protocol.Protocol_Write_SendCOM(TASKS.WRITE_PARA);

                        actXET.oldAx = 0xFF;
                        actXET.oldMx = 0xFF;
                        actXET.oldTU = 0x00;
                        break;
                    case TASKS.WRITE_PARA:
                        comTicker = 0;
                        writeParaCnt = 0;

                        nextTask = TASKS.READ_HEART;

                        timer1.Enabled = true;
                        break;
                    case TASKS.READ_HEART:
                        if (actXET.isActive && actXET.count > 1)
                        {
                            if (actXET.modePt == 0)
                            {
                                actXET.num_clear = 1;
                            }

                            updateDataFromHeart();

                            readHeartCnt = 0;
                        }

                        //检测是否满足自动切换点位条件
                        checkAutoSwitch();

                        //检测到手动归零
                        if (actXET.isKeyZero)
                        {
                            //timer1.Enabled = false;
                            nextTask = TASKS.READ_PARA;
                            MyDevice.protocol.Protocol_ClearState();
                            MyDevice.protocol.Protocol_Read_SendCOM(TASKS.READ_PARA);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        //心跳定时器
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Main.ActiveForm != "MenuShowStrategyForm") return;

            switch(nextTask)
            {
                default:
                    break;
                case TASKS.READ_HEART:
                    //PEAK模式下写入dataInfoListToDb
                    if (actXET.num_clear % 2 == 0)
                    {
                        isZero = true;
                        table_index--;
                        if (table_index < 0)
                        {
                            table_index = 0;
                        }
                    }

                    updateDataInfosToDb();

                    //通讯监控
                    if (actXET.isActive)
                    {
                        if (actXET.elapse > 1)//0.1秒-----用于缓冲，防止指令发送太快
                        {
                            MyDevice.protocol.addr = meWrenchID;
                            //net模式下ip地址需要更换，即需要更换port
                            if (MyDevice.protocol.type == COMP.NET)
                            {
                                if (MyDevice.addr_ip.ContainsKey(MyDevice.protocol.addr.ToString()) == true)
                                {
                                    MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
                                }
                            }
                            actXET = MyDevice.actDev;
                            nextTask = TASKS.READ_HEART;
                            MyDevice.protocol.Protocol_ClearState();
                            MyDevice.protocol.Protocol_Read_SendCOM(TASKS.READ_HEART);
                            readHeartCnt++;
                        }
                        else
                        {
                            actXET.elapse++;
                        }
                    }
                    break;

                case TASKS.WRITE_PARA:
                    if (++comTicker > 5)
                    {
                        comTicker = 0;

                        MyDevice.protocol.Protocol_ClearState();
                        MyDevice.protocol.Protocol_Write_SendCOM(TASKS.WRITE_PARA);
                        writeParaCnt++;

                        //actXET.oldAx = 0xFF;
                        //actXET.oldMx = 0xFF;
                        //actXET.oldTU = 0x00;
                    }
                    break;

                case TASKS.WRITE_A1M01DAT:
                    if (++comTicker > 5)
                    {
                        comTicker = 0;
                        MyDevice.protocol.Protocol_ClearState();
                        MyDevice.protocol.Protocol_WriteAXMXTasks(actXET.modeAx * 10 + actXET.modeMx);
                    }
                    break;

                case TASKS.READ_PARA:
                    if (++comTicker > 5)
                    {
                        comTicker = 0;
                        MyDevice.protocol.Protocol_ClearState();
                        MyDevice.protocol.Protocol_Read_SendCOM(TASKS.READ_PARA);
                    }
                    break;
            }

            if (readHeartCnt > 30 || writeParaCnt > 30)
            {
                readHeartCnt = 0;
                writeParaCnt = 0;
                timer1.Enabled = false;

                //先判断当前工单设置的设备站点是否属于已连接设备集合
                if (!MyDevice.AddrList.Contains(MyDevice.protocol.addr))
                {
                    if (MyDevice.languageType == 0)
                    {
                        MessageBox.Show("设备" + MyDevice.protocol.addr + "未连接，请重新连接！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        MessageBox.Show("Device" + MyDevice.protocol.addr + " is not connected, , please reconnect!", "Hint", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                //设备已连接，属于掉线情况
                else
                {              
                    if (MyDevice.languageType == 0)
                    {
                        MessageBox.Show("设备" + MyDevice.protocol.addr + "已掉线，请检查连接后重新导入工单！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        MessageBox.Show("Device" + MyDevice.protocol.addr + "has been dropped, please check the connection and re-import the ticket!", "Hint", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        //扫描定时器
        private void timer2_Tick(object sender, EventArgs e)
        {
            if (Main.ActiveForm != "MenuShowStrategyForm") return;

            if (++comTicker > 5 && nextTask == TASKS.READ_PARA)
            {
                comTicker = 0;
                MyDevice.protocol.addr = Convert.ToByte(addressList[scanIndex]);
                //net模式下ip地址需要更换，即需要更换port
                if (MyDevice.protocol.type == COMP.NET)
                {
                    if (MyDevice.addr_ip.ContainsKey(MyDevice.protocol.addr.ToString()) == true)
                    {
                        MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
                    }
                }
                actXET = MyDevice.actDev;
                MyDevice.protocol.Protocol_ClearState();
                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.READ_PARA);
                readParaCnt++;
            }

            //发送读指令超过3次表示是 未连接状态
            if (readParaCnt > 3)
            {
                readParaCnt = 0;
                timer2.Enabled = false;
                //先判断当前工单设置的设备站点是否属于已连接设备集合
                if (!MyDevice.AddrList.Contains(MyDevice.protocol.addr))
                {
                    if (MyDevice.languageType == 0)
                    {
                        MessageBox.Show("设备" + MyDevice.protocol.addr + "未连接，请重新连接！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        MessageBox.Show("Device" + MyDevice.protocol.addr + " is not connected, , please reconnect!", "Hint", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                //设备已连接，属于掉线情况
                else
                {
                    scanStatus();
                }
            }
        }

        //唤醒定时器
        private void wakeTimer_Tick(object sender, EventArgs e)
        {
            //关闭正在执行的定时器
            timer1.Enabled = false;
            timer2.Enabled = false;

            //向已连接设备轮流发送readPara
            if (MyDevice.AddrList.Count > 1)
            {
                foreach (var item in MyDevice.AddrList)
                {
                    MyDevice.protocol.addr = item; 
                    if (MyDevice.protocol.type == COMP.NET)
                    {
                        MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
                    }
                    MyDevice.protocol.Protocol_Read_SendCOM(TASKS.READ_PARA);
                }
            }

            //重启读心跳
            timer1.Enabled = true;
        }

        //更新参数
        private void updatePara()
        {
            this.ucSignalLamp1.LampColor = new Color[] { Color.Red };

            try
            {
                //保存数据list到数据库
                if (dataInfosToDb.Count > 0)
                {
                    string datatime = ConvertTimestampToStr(MyDevice.GetTimeStamp());
                    string dataInfoTableName = MyDevice.myMac + "_" + datatime;  //数据表表名
                    List<string> datasInfoName = new List<string>();             //数据统计表名称列
                    bool isTableNameExist = false;                               //是否存在对应数据表

                    datasInfoName = jdbc.GetListDatasName();
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
            catch (Exception ex)
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

            //连接选择的扳手
            if (textBox2.Text.Equals("")) return;

            int wrenchIDint = int.Parse(textBox2.Text);
            meWrenchID = Convert.ToByte(wrenchIDint);             //要连接的扳手ID

            //获取已选设备地址
            if (MyDevice.protocol.type != COMP.SelfUART)
            {
                MyDevice.protocol.addr = meWrenchID;
                //net模式下ip地址需要更换，即需要更换port
                if (MyDevice.protocol.type == COMP.NET)
                {
                    if (MyDevice.addr_ip.ContainsKey(MyDevice.protocol.addr.ToString()) == true)
                    {
                        MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
                    }
                }
                actXET = MyDevice.actDev;
            }

            actXET.modeRec = (byte)ucCombox1.SelectedIndex;
            actXET.modeAx = (byte)ucCombox2.SelectedIndex;
            actXET.modePt = (byte)ucCombox3.SelectedIndex;
            actXET.torqueUnit = (UNIT)ucCombox4.SelectedIndex;
            actXET.modeMx = (byte)ucCombox5.SelectedIndex;
            actXET.angleSpeed = ucCombox6.SelectedIndex;
            actXET.screwNum = Convert.ToByte(textBox5.Text);
            actXET.isKeyLock = 1;

            switch (actXET.modeAx * 10 + actXET.modeMx)
            {
                default:
                case 0:
                    actXET.a1mxTable.A1M0.torqueTarget = (uint)(Convert.ToDouble(textBox6.Text) * 100 + 0.5);
                    break;
                case 1:
                    if (textBox6.Text.Split(',').Length == 2)
                    {
                        actXET.a1mxTable.A1M1.torqueLow = (uint)(Convert.ToDouble(textBox6.Text.Split(',')[0]) * 100 + 0.5);
                        actXET.a1mxTable.A1M1.torqueHigh = (uint)(Convert.ToDouble(textBox6.Text.Split(',')[1]) * 100 + 0.5);
                    }
                    break;
                case 2:
                    if (textBox6.Text.Split(',').Length == 2)
                    {
                        actXET.a1mxTable.A1M2.torqueLow = (uint)(Convert.ToDouble(textBox6.Text.Split(',')[0]) * 100 + 0.5);
                        actXET.a1mxTable.A1M2.torqueHigh = (uint)(Convert.ToDouble(textBox6.Text.Split(',')[1]) * 100 + 0.5);
                    }
                    break;
                case 3:
                    if (textBox6.Text.Split(',').Length == 2)
                    {
                        actXET.a1mxTable.A1M3.torqueLow = (uint)(Convert.ToDouble(textBox6.Text.Split(',')[0]) * 100 + 0.5);
                        actXET.a1mxTable.A1M3.torqueHigh = (uint)(Convert.ToDouble(textBox6.Text.Split(',')[1]) * 100 + 0.5);
                    }
                    break;
                case 4:
                    if (textBox6.Text.Split(',').Length == 2)
                    {
                        actXET.a1mxTable.A1M4.torqueLow = (uint)(Convert.ToDouble(textBox6.Text.Split(',')[0]) * 100 + 0.5);
                        actXET.a1mxTable.A1M4.torqueHigh = (uint)(Convert.ToDouble(textBox6.Text.Split(',')[1]) * 100 + 0.5);
                    }
                    break;
                case 5:
                    if (textBox6.Text.Split(',').Length == 2)
                    {
                        actXET.a1mxTable.A1M5.torqueLow = (uint)(Convert.ToDouble(textBox6.Text.Split(',')[0]) * 100 + 0.5);
                        actXET.a1mxTable.A1M5.torqueHigh = (uint)(Convert.ToDouble(textBox6.Text.Split(',')[1]) * 100 + 0.5);
                    }
                    break;
                case 6:
                    if (textBox6.Text.Split(',').Length == 2)
                    {
                        actXET.a1mxTable.A1M6.torqueLow = (uint)(Convert.ToDouble(textBox6.Text.Split(',')[0]) * 100 + 0.5);
                        actXET.a1mxTable.A1M6.torqueHigh = (uint)(Convert.ToDouble(textBox6.Text.Split(',')[1]) * 100 + 0.5);
                    }
                    break;
                case 7:
                    if (textBox6.Text.Split(',').Length == 2)
                    {
                        actXET.a1mxTable.A1M7.torqueLow = (uint)(Convert.ToDouble(textBox6.Text.Split(',')[0]) * 100 + 0.5);
                        actXET.a1mxTable.A1M7.torqueHigh = (uint)(Convert.ToDouble(textBox6.Text.Split(',')[1]) * 100 + 0.5);
                    }
                    break;
                case 8:
                    if (textBox6.Text.Split(',').Length == 2)
                    {
                        actXET.a1mxTable.A1M8.torqueLow = (uint)(Convert.ToDouble(textBox6.Text.Split(',')[0]) * 100 + 0.5);
                        actXET.a1mxTable.A1M8.torqueHigh = (uint)(Convert.ToDouble(textBox6.Text.Split(',')[1]) * 100 + 0.5);
                    }
                    break;
                case 9:
                    if (textBox6.Text.Split(',').Length == 2)
                    {
                        actXET.a1mxTable.A1M9.torqueLow = (uint)(Convert.ToDouble(textBox6.Text.Split(',')[0]) * 100 + 0.5);
                        actXET.a1mxTable.A1M9.torqueHigh = (uint)(Convert.ToDouble(textBox6.Text.Split(',')[1]) * 100 + 0.5);
                    }
                    break;
                case 10:
                    actXET.a2mxTable.A2M0.torquePre = (uint)(Convert.ToDouble(textBox6.Text) * 100 + 0.5);
                    actXET.a2mxTable.A2M0.angleTarget = (ushort)(Convert.ToDouble(textBox7.Text) * 10 + 0.5);
                    break;
                case 11:
                    actXET.a2mxTable.A2M1.torquePre = (uint)(Convert.ToDouble(textBox6.Text) * 100 + 0.5);
                    if (textBox7.Text.Split(',').Length == 2)
                    {
                        actXET.a2mxTable.A2M1.angleLow = (ushort)(Convert.ToDouble(textBox7.Text.Split(',')[0]) * 10 + 0.5);
                        actXET.a2mxTable.A2M1.angleHigh = (ushort)(Convert.ToDouble(textBox7.Text.Split(',')[1]) * 10 + 0.5);
                    }
                    break;
                case 12:
                    actXET.a2mxTable.A2M2.torquePre = (uint)(Convert.ToDouble(textBox6.Text) * 100 + 0.5);
                    if (textBox7.Text.Split(',').Length == 2)
                    {
                        actXET.a2mxTable.A2M2.angleLow = (ushort)(Convert.ToDouble(textBox7.Text.Split(',')[0]) * 10 + 0.5);
                        actXET.a2mxTable.A2M2.angleHigh = (ushort)(Convert.ToDouble(textBox7.Text.Split(',')[1]) * 10 + 0.5);
                    }
                    break;
                case 13:
                    actXET.a2mxTable.A2M3.torquePre = (uint)(Convert.ToDouble(textBox6.Text) * 100 + 0.5);
                    if (textBox7.Text.Split(',').Length == 2)
                    {
                        actXET.a2mxTable.A2M3.angleLow = (ushort)(Convert.ToDouble(textBox7.Text.Split(',')[0]) * 10 + 0.5);
                        actXET.a2mxTable.A2M3.angleHigh = (ushort)(Convert.ToDouble(textBox7.Text.Split(',')[1]) * 10 + 0.5);
                    }
                    break;
                case 14:
                    actXET.a2mxTable.A2M4.torquePre = (uint)(Convert.ToDouble(textBox6.Text) * 100 + 0.5);
                    if (textBox7.Text.Split(',').Length == 2)
                    {
                        actXET.a2mxTable.A2M4.angleLow = (ushort)(Convert.ToDouble(textBox7.Text.Split(',')[0]) * 10 + 0.5);
                        actXET.a2mxTable.A2M4.angleHigh = (ushort)(Convert.ToDouble(textBox7.Text.Split(',')[1]) * 10 + 0.5);
                    }
                    break;
                case 15:
                    actXET.a2mxTable.A2M5.torquePre = (uint)(Convert.ToDouble(textBox6.Text) * 100 + 0.5);
                    if (textBox7.Text.Split(',').Length == 2)
                    {
                        actXET.a2mxTable.A2M5.angleLow = (ushort)(Convert.ToDouble(textBox7.Text.Split(',')[0]) * 10 + 0.5);
                        actXET.a2mxTable.A2M5.angleHigh = (ushort)(Convert.ToDouble(textBox7.Text.Split(',')[1]) * 10 + 0.5);
                    }
                    break;
                case 16:
                    actXET.a2mxTable.A2M6.torquePre = (uint)(Convert.ToDouble(textBox6.Text) * 100 + 0.5);
                    if (textBox7.Text.Split(',').Length == 2)
                    {
                        actXET.a2mxTable.A2M6.angleLow = (ushort)(Convert.ToDouble(textBox7.Text.Split(',')[0]) * 10 + 0.5);
                        actXET.a2mxTable.A2M6.angleHigh = (ushort)(Convert.ToDouble(textBox7.Text.Split(',')[1]) * 10 + 0.5);
                    }
                    break;
                case 17:
                    actXET.a2mxTable.A2M7.torquePre = (uint)(Convert.ToDouble(textBox6.Text) * 100 + 0.5);
                    if (textBox7.Text.Split(',').Length == 2)
                    {
                        actXET.a2mxTable.A2M7.angleLow = (ushort)(Convert.ToDouble(textBox7.Text.Split(',')[0]) * 10 + 0.5);
                        actXET.a2mxTable.A2M7.angleHigh = (ushort)(Convert.ToDouble(textBox7.Text.Split(',')[1]) * 10 + 0.5);
                    }
                    break;
                case 18:
                    actXET.a2mxTable.A2M8.torquePre = (uint)(Convert.ToDouble(textBox6.Text) * 100 + 0.5);
                    if (textBox7.Text.Split(',').Length == 2)
                    {
                        actXET.a2mxTable.A2M8.angleLow = (ushort)(Convert.ToDouble(textBox7.Text.Split(',')[0]) * 10 + 0.5);
                        actXET.a2mxTable.A2M8.angleHigh = (ushort)(Convert.ToDouble(textBox7.Text.Split(',')[1]) * 10 + 0.5);
                    }
                    break;
                case 19:
                    actXET.a2mxTable.A2M9.torquePre = (uint)(Convert.ToDouble(textBox6.Text) * 100 + 0.5);
                    if (textBox7.Text.Split(',').Length == 2)
                    {
                        actXET.a2mxTable.A2M9.angleLow = (ushort)(Convert.ToDouble(textBox7.Text.Split(',')[0]) * 10 + 0.5);
                        actXET.a2mxTable.A2M9.angleHigh = (ushort)(Convert.ToDouble(textBox7.Text.Split(',')[1]) * 10 + 0.5);
                    }
                    break;
            }

            //参数初始化
            isBatAlarm = true;
            isOvertake = false;
            torquePeak = float.MinValue;
            anglePeak = float.MinValue;

            //写入参数
            //timer1.Enabled = false;
            nextTask = TASKS.WRITE_A1M01DAT;
            MyDevice.protocol.Protocol_ClearState();
            MyDevice.protocol.Protocol_WriteAXMXTasks(actXET.modeAx * 10 + actXET.modeMx);
        }

        //更新数据
        private void updateDataFromHeart()
        {
            int k = dataInfos.Count;

            //更新电量显示
            switch (actXET.battery)
            {
                case 0:
                    pictureBox2.Image = Properties.Resources.Bat_0;
                    if (isBatAlarm)
                    {
                        isBatAlarm = false;
                        MessageBox.Show("扳手" + actXET.addr.ToString() + "电量不足，请更换电池！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    break;
                case 1:
                    pictureBox2.Image = Properties.Resources.Bat_1;
                    break;
                case 2:
                    pictureBox2.Image = Properties.Resources.Bat_2;
                    break;
                case 3:
                    pictureBox2.Image = Properties.Resources.Bat_3;
                    break;
                default:
                    break;
            }

            for (int i = 0; i < actXET.REC.Count; i++)
            {
                if (actXET.REC[i].opsn.Length > 0)
                {
                    if (!lastWorkVin.Equals(""))
                    {
                        if (!lastWorkVin.Equals(actXET.REC[i].opsn))
                        {
                            res = "no";            //结果
                            torqueResult = "no";   //扭矩结果
                            angleResult = "no";    //角度结果
                        }
                    }

                    //peak模式峰值
                    if (actXET.modePt == 1)
                    {
                        if ((actXET.REC[i].torquePeak / 100.0f) > torquePeak)
                        {
                            torquePeak = actXET.REC[i].torquePeak / 100.0f;
                        }
                        if (Math.Abs(actXET.REC[i].anglePeak / 10.0f) > anglePeak)
                        {
                            anglePeak = Math.Abs(actXET.REC[i].anglePeak / 10.0f);
                        }
                    }
                    //track模式峰值
                    else if (actXET.modePt == 0)
                    {
                        if ((actXET.REC[i].torque / 100.0f) > torquePeak)
                        {
                            torquePeak = actXET.REC[i].torque / 100.0f;
                        }
                        if (Math.Abs(actXET.REC[i].angle / 10.0f) > anglePeak)
                        {
                            anglePeak = Math.Abs(actXET.REC[i].angle / 10.0f);
                        }
                    }

                    //更新单位
                    switch (actXET.torqueUnit)
                    {
                        case UNIT.UNIT_nm: torqueUnit = "N·m"; break;
                        case UNIT.UNIT_lbfin: torqueUnit = "lbf·in"; break;
                        case UNIT.UNIT_lbfft: torqueUnit = "lbf·ft"; break;
                        case UNIT.UNIT_kgcm: torqueUnit = "kgf·cm"; break;
                        default: break;
                    }

                    int index = meTorqueAngleAndSetMode.FindIndex(item => item.Item1 == textBox5.Text);
                    //表格数据
                    DataInfo di = new DataInfo()
                    {
                        time = MyDevice.GetMilTimeStamp(),
                        work_vin = actXET.REC[i].opsn,
                        work_order_id = importWorkOrderId,
                        screw_id = meWrenchCodeAndID[index].Item1,
                        wrench_code = meWrenchCodeAndID[index].Item2,
                        wrench_id = meWrenchCodeAndID[index].Item3,
                        actual_torque = (actXET.REC[i].torque / 100.0f).ToString() + " " + torqueUnit.ToString(),
                        actual_angle = Math.Abs(actXET.REC[i].angle / 10.0f).ToString() + " °",
                        torque_peak = torquePeak.ToString()+ " " + torqueUnit.ToString(),
                        angle_peak = Math.Abs(anglePeak).ToString() + " °",
                        result = res,
                        torque_result = torqueResult,
                        angle_result = angleResult
                    };
                    dataInfos.Add(di);

                    if (dataInfos.Count > 0)
                    {
                        lastWorkVin = dataInfos[k].work_vin;
                    }

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

                    //数据调整，调整数据峰值
                    float torpeak = actXET.REC[i].torquePeak / 100.0f;
                    float angpeak = actXET.REC[i].anglePeak / 10.0f;
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
                            if (dicTorque[dataInfos[k].work_vin] >= float.Parse(meTorqueAngleAndSetMode[index].Item2))
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
                            if (textBox6.Text.Split(',').Length == 2)
                            {
                                if (dicTorque[dataInfos[k].work_vin] >= (uint)Convert.ToDouble(textBox6.Text.Split(',')[0]) && dicTorque[dataInfos[k].work_vin] <= (uint)Convert.ToDouble(textBox6.Text.Split(',')[1]))
                                {
                                    torqueResult = "ok";
                                }
                                else
                                {
                                    torqueResult = "no";

                                    //超出上限打 X 
                                    if (dicTorque[dataInfos[k].work_vin] > (uint)Convert.ToDouble(textBox6.Text.Split(',')[1]))
                                    {
                                        fockIndex++;
                                    }
                                }
                            }
                            break;
                        //A2M0模式先满足目标扭矩再满足目标角度
                        case 10:
                            if (dicTorque[dataInfos[k].work_vin] >= float.Parse(meTorqueAngleAndSetMode[index].Item2))
                            {
                                torqueResult = "ok";
                            }
                            else
                            {
                                torqueResult = "no";
                            }
                            if (dicAngle[dataInfos[k].work_vin] >= float.Parse(meTorqueAngleAndSetMode[index].Item3))
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
                            if (dicTorque[dataInfos[k].work_vin] >= (uint)(Convert.ToDouble(textBox6.Text)))
                            {
                                torqueResult = "ok";
                            }
                            else
                            {
                                torqueResult = "no";
                            }
                            if (textBox7.Text.Split(',').Length == 2)
                            {
                                if (dicAngle[dataInfos[k].work_vin] >= (ushort)Convert.ToDouble(textBox7.Text.Split(',')[0]) && dicAngle[dataInfos[k].work_vin] <= (ushort)Convert.ToDouble(textBox7.Text.Split(',')[1]))
                                {
                                    angleResult = "ok";
                                }
                                else
                                {
                                    angleResult = "no";

                                    if (dicAngle[dataInfos[k].work_vin] > (ushort)Convert.ToDouble(textBox7.Text.Split(',')[1]))
                                    {
                                        fockIndex++;
                                    }
                                }
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
                        tickIndex++;
                    }
                    else
                    {
                        res = "no";
                        tickIndex = 0;
                        //normalIndex++;
                    }

                    //pictureBox1.Refresh();
                    Graphics graphics = pictureBox1.CreateGraphics();
                    Color color = colorList.FindLast(item => item.Key == int.Parse(meTorqueAngleAndSetMode[index].Item1)).Value;
                    string torqueText = MyDevice.languageType == 0 ? ("实时(峰值)力矩: " + (actXET.torque / 100.0f).ToString() + "(" + torquePeak.ToString() + ")" + torqueUnit.ToString()) : ("Real-time(Peak) torque: " + (actXET.torque / 100.0f).ToString() + "(" + torquePeak.ToString() + ")" + torqueUnit.ToString());
                    string angleText = MyDevice.languageType == 0 ? ("实时(峰值)角度: " + (Math.Abs(actXET.angle) / 10.0f).ToString() + "(" + Math.Abs(anglePeak).ToString() + ")" + "°") : ("Real-time(Peak) angle: " + (Math.Abs(actXET.angle) / 10.0f).ToString() + "°" + Math.Abs(anglePeak).ToString() + "°");

                    //画左上角大圈和√
                    if (tickIndex == 1 && res == "ok") 
                    {
                        isOvertake = false;
                        DrawBubbleAndTick_Max(graphics, color);
                    }
                    //超上限画左上角气泡和×
                    else if (fockIndex == 1)
                    {
                        isOvertake = true;
                        DrawBubbleAndFork_Max(graphics, color);
                    }
                    //画左上角气泡和圆形序号
                    //else if (normalIndex == 1)
                    //{
                    //    DrawBubbleAndCircle_Max(graphics, pointList[selectIndex].Item1);
                    //}
                    graphics.Dispose();

                    //显示实时数据
                    label15.Text = torqueText;
                    label16.Text = angleText;
                    label15.BackColor = color;
                    label16.BackColor = color;
                    label15.BringToFront();
                    label16.BringToFront();

                    k++;//更改mTable下标
                }
            }
            actXET.REC.Clear();
        }

        //检测是否自动切换点位
        private void checkAutoSwitch()
        {
            float threshold = 0;

            //自动切换
            //比较峰值是否达到百分之40
            float torqueValue;
            if (dataInfos.Count < 1)
            {
                torqueValue = 0;
            }
            else
            {
                torqueValue = dicTorque[dataInfos[dataInfos.Count - 1].work_vin];
            }
            int index = meTorqueAngleAndSetMode.FindIndex(item => item.Item1 == textBox5.Text);
            switch (actXET.modeAx * 10 + actXET.modeMx)
            {
                default:
                case 0:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                case 17:
                case 18:
                case 19:
                    threshold = float.Parse(meTorqueAngleAndSetMode[index].Item2) * 0.4f; // 计算阈值
                    break;
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                    if (textBox6.Text.Split(',').Length == 2)
                    {
                        threshold = (float)Convert.ToDouble(textBox6.Text.Split(',')[0]) * 0.4f; // 计算阈值
                    }
                    break;
            }

            //满足自动切换条件
            if (Math.Abs(torqueValue) > threshold && isOvertake == false)
            {
                isAutoSwitch = true;
            }
        }

        //自动切换点位
        private void AutoSwitch()
        {
            isAutoSwitch = false;

            autoSelectedNum = int.Parse(textBox5.Text);
            int indexNow = screwIDList.IndexOf(autoSelectedNum);
            if (indexNow != -1)
            {
                // 如果 autoSelectedNum 存在于列表中
                if (indexNow == screwIDList.Count - 1)
                {
                    // 如果 autoSelectedNum 已经是列表的最后一个元素，更新为列表的第一个元素
                    autoSelectedNum = screwIDList[0];
                }
                else
                {
                    // 否则，更新为列表中的下一个元素
                    autoSelectedNum = screwIDList[indexNow + 1];
                }
            }

            selectIndex = bubbleList.FindIndex(item => item.Item1 == autoSelectedNum);

            //画左上角的大圈
            updatePictureBox1(currentSize, currentSize);

            //更新左侧边栏
            updateLeftInfo(autoSelectedNum);

            //更新参数
            updatePara();
        }

        //更新实际存储到数据库的数据表list
        private void updateDataInfosToDb()
        {
            if (actXET.isActive == false)
            {
                return;
            }

            //加行
            while (table_index < dataInfos.Count)
            {
                if (actXET.modePt == 0 || actXET.modePt == 1)  //track模式或者peak模式
                {
                    //数据
                    DataInfo di = new DataInfo()
                    {
                        time = dataInfos[table_index].time,
                        work_vin = actXET.addr.ToString() + "W" + dataInfos[table_index].work_vin,
                        work_order_id = dataInfos[table_index].work_order_id,
                        screw_id = dataInfos[table_index].screw_id,
                        wrench_code = dataInfos[table_index].wrench_code,
                        wrench_id = dataInfos[table_index].wrench_id,
                        actual_torque = dataInfos[table_index].actual_torque,
                        actual_angle = dataInfos[table_index].actual_angle,
                        torque_peak = dicTorque[dataInfos[table_index].work_vin].ToString(),
                        angle_peak = dicAngle[dataInfos[table_index].work_vin].ToString(),
                        result = res,
                        torque_result = torqueResult,
                        angle_result = angleResult
                    };
                    dataInfosToDb.Add(di);
                    table_index++;
                }
                else if (isZero)
                {
                    //数据
                    DataInfo di = new DataInfo()
                    {
                        time = dataInfos[table_index].time,
                        work_vin = actXET.addr.ToString() + "W" + dataInfos[table_index].work_vin,
                        work_order_id = dataInfos[table_index].work_order_id,
                        screw_id = dataInfos[table_index].screw_id,
                        wrench_code = dataInfos[table_index].wrench_code,
                        wrench_id = dataInfos[table_index].wrench_id,
                        actual_torque = dicTorque[dataInfos[table_index].work_vin].ToString(),
                        actual_angle = dicAngle[dataInfos[table_index].work_vin].ToString(),
                        torque_peak = dicTorque[dataInfos[table_index].work_vin].ToString(),
                        angle_peak = dicAngle[dataInfos[table_index].work_vin].ToString(),
                        result = res,
                        torque_result = torqueResult,
                        angle_result = angleResult
                    };
                    dataInfosToDb.Add(di);

                    table_index++;
                    isZero = false;
                    actXET.num_clear++;
                }
                else
                {
                    table_index++;
                }
            }
        }

        #endregion

        #region pictureBox1绘图

        //更新气泡颜色list
        private void updateColorList(int newNum, int oldNum = -1)
        {
            //依据位点添加的顺序分配颜色
            //当拧紧序号改变时，顺序不变，故颜色也不改变
            //删除点时，将删除的点的键值对的key改为（-2），点顺序也不改变
            if (oldNum == -1)
            {
                int index = colorList.FindIndex(item => item.Key == newNum);
                if (index == -1)
                {
                    colorList.Add(new KeyValuePair<int, Color>(newNum, colorOptions[(colorList.Count + 1) % 10]));
                }
                else
                {
                    return;
                }
            }
            else
            {
                int index = colorList.FindIndex(item => item.Key == oldNum);
                if (index != -1)
                {
                    KeyValuePair<int, Color> item = colorList[index];
                    colorList.RemoveAt(index);
                    colorList.Insert(index, new KeyValuePair<int, Color>(newNum, item.Value));
                }
            }
        }

        //画位点气泡和圆形序号
        private void DrawBubbleAndCircle(Graphics graphics, int num, Point point)
        {
            DrawBubble(graphics, num, point);
            DrawCircleAndIndex(graphics, num, point);
        }

        // 绘制气泡框形状的图形
        private void DrawBubble(Graphics graphics, int num, Point point)
        {
            int width = 200;  // 气泡框的宽度
            int height = 47;  // 气泡框的高度
            int radius = 20;  // 气泡框的圆角半径
            int X = point.X;
            int Y = point.Y;
            int indexPoint = meWrenchMessage.FindIndex(item => item.Item1 == num.ToString());

            //根据不同模式画不同大小的框
            if (meWrenchMessage[indexPoint].Item2 == "A1" && meWrenchMessage[indexPoint].Item5 == "M0")
            {
                width = 200;
                height = 47;
            }
            else if (meWrenchMessage[indexPoint].Item2 == "A1" && meWrenchMessage[indexPoint].Item5 != "M0")
            {
                width = 250;
                height = 47;
            }
            else if (meWrenchMessage[indexPoint].Item2 == "A2" && meWrenchMessage[indexPoint].Item5 == "M0")
            {
                width = 200;
                height = 76;
            }
            else if (meWrenchMessage[indexPoint].Item2 == "A2" && meWrenchMessage[indexPoint].Item5 != "M0")
            {
                width = 250;
                height = 76;
            }

            // 创建一个GraphicsPath对象，用于绘制气泡框形状
            GraphicsPath path = new GraphicsPath();
            path.AddArc(X, Y, radius, radius, 180, 76);                                   // 左上角圆角
            path.AddArc(X + width - radius, Y, radius, radius, 270, 76);                  // 右上角圆角
            path.AddArc(X + width - radius, Y + height - radius, radius, radius, 0, 76);  // 右下角圆角
            path.AddArc(X, Y + height - radius, radius, radius, 90, 76);                  // 左下角圆角
            path.CloseFigure();

            // 绘制气泡框形状
            Color color = colorList[colorList.FindIndex(item => item.Key == num)].Value;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            graphics.FillPath(new SolidBrush(color), path);
            graphics.DrawPath(new Pen(color), path);

            bubbleList.Add(new Tuple<int, Rectangle>(num, new Rectangle(X, Y, width, height)));
        }

        //画圆形和序号
        private void DrawCircleAndIndex(Graphics graphics, int num, Point point)
        {
            int radius = 20;
            int diameter = radius * 2;    //直径
            Color textColor = Color.White;
            Color color = colorList[colorList.FindIndex(item => item.Key == num)].Value;

            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;                                          //抗锯齿
            graphics.FillEllipse(new SolidBrush(color), point.X - radius, point.Y - radius, diameter, diameter);                //画圆
            if (num < 10)
            {
                graphics.DrawString(num.ToString(), fontTitle, new SolidBrush(textColor), point.X - 11, point.Y - 14);          //拧紧序号
            }
            else if (num >= 10 && num <= 99)
            {
                graphics.DrawString(num.ToString(), fontTitle, new SolidBrush(textColor), point.X - 19, point.Y - 14);          //拧紧序号
            }
            else
            {
                graphics.DrawString(num.ToString(), fontTitleLen3, new SolidBrush(textColor), point.X - 24, point.Y - 14);      //拧紧序号
            }
        }

        //画气泡上的目标力矩，目标角度
        private void DrawStrOnBubble(Graphics graphics, Tuple<int, Point> pointList, List<Tuple<string, string, string, string>> meTorqueAngleAndSetMode)
        {
            int indexPoint = meTorqueAngleAndSetMode.FindIndex(item => item.Item1 == pointList.Item1.ToString());
            string torque = "";
            string angle = "";

            if (indexPoint != -1)
            {
                torque = meTorqueAngleAndSetMode[indexPoint].Item2;
                angle = meTorqueAngleAndSetMode[indexPoint].Item3;
            }

            if (meWrenchMessage[indexPoint].Item2 == "A1" && meWrenchMessage[indexPoint].Item5 == "M0")
            {
                string bubbletext = MyDevice.languageType == 0 ? ("目标力矩: " + torque) : ("Target torque: " + torque);
                graphics.DrawString(bubbletext, fontBubble, new SolidBrush(Color.White), pointList.Item2.X + 10, pointList.Item2.Y + 10);
            }
            else if (meWrenchMessage[indexPoint].Item2 == "A1" && meWrenchMessage[indexPoint].Item5 != "M0")
            {
                string bubbletext = MyDevice.languageType == 0 ? ("力矩下限,上限: " + torque) : ("Torque lower,upper: " + torque);
                graphics.DrawString(bubbletext, fontBubble, new SolidBrush(Color.White), pointList.Item2.X + 10, pointList.Item2.Y + 10);
            }
            else if (meWrenchMessage[indexPoint].Item2 == "A2" && meWrenchMessage[indexPoint].Item5 == "M0")
            {
                string bubbletext = MyDevice.languageType == 0 ? ("目标力矩: " + torque + "\n目标角度: " + angle) : ("Target torque: " + torque + "\nTarget angle: " + angle);
                graphics.DrawString(bubbletext, fontBubble, new SolidBrush(Color.White), pointList.Item2.X + 10, pointList.Item2.Y + 10);
            }
            else if (meWrenchMessage[indexPoint].Item2 == "A2" && meWrenchMessage[indexPoint].Item5 != "M0")
            {
                string bubbletext = MyDevice.languageType == 0 ? ("目标力矩: " + torque + "\n角度下限,上限: " + angle) : ("Target torque: " + torque + "\nAngle lower,upper: " + angle);
                graphics.DrawString(bubbletext, fontBubble, new SolidBrush(Color.White), pointList.Item2.X + 10, pointList.Item2.Y + 10);
            }
        }

        //画左上角大圈和序号
        private void DrawBigCircleAndIndex(Graphics graphics, int num)
        {
            Point point = new Point(0, 0);
            int radius = 60;
            int diameter = radius * 2;    //直径
            Color textColor = Color.White;
            Color color = colorList[colorList.FindIndex(item => item.Key == num)].Value;

            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;                         //抗锯齿
            graphics.FillEllipse(new SolidBrush(color), point.X, point.Y, diameter, diameter);                 //画圆
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            if (num < 10)
            {
                graphics.DrawString(num.ToString(), fontBigTitle, new SolidBrush(textColor), 23, 11);          //拧紧序号
            }
            else if (num >= 10 && num < 99)
            {
                graphics.DrawString(num.ToString(), fontBigTitle, new SolidBrush(textColor), -4, 11);          //拧紧序号
            }
            else
            {
                graphics.DrawString(num.ToString(), fontBigTitleLen3, new SolidBrush(textColor), -10, 18);     //拧紧序号
            }
        }

        //画左上角大圈和√
        private void DrawBigCircleAndTick(Graphics graphics, Color circleColor)
        {
            Point point = new Point(0, 0);
            int radius = 60;
            int diameter = radius * 2;    //直径
            Color textColor = Color.White;
            string tick = "√";
            Font fontTick = new Font("隶书", 61, FontStyle.Bold, GraphicsUnit.Point);

            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;                    //抗锯齿
            graphics.FillEllipse(new SolidBrush(circleColor), point.X, point.Y, diameter, diameter);      //画圆
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            graphics.DrawString(tick, fontTick, new SolidBrush(textColor), 4, 19);                        //√

            graphics.Dispose();
        }

        //画左上角气泡和圆形序号
        private void DrawBubbleAndCircle_Max(Graphics graphics, int num)
        {
            int width = 350;  // 气泡框的宽度
            int height = 76;  // 气泡框的高度
            int radius = 20;  // 气泡框的圆角半径
            int X = 98;
            int Y = 20;

            // 创建一个GraphicsPath对象，用于绘制气泡框形状
            GraphicsPath path = new GraphicsPath();
            path.AddArc(X, Y, radius, radius, 180, 76);                                   // 左上角圆角
            path.AddArc(X + width - radius, Y, radius, radius, 270, 76);                  // 右上角圆角
            path.AddArc(X + width - radius, Y + height - radius, radius, radius, 0, 76);  // 右下角圆角
            path.AddArc(X, Y + height - radius, radius, radius, 90, 76);                  // 左下角圆角
            path.CloseFigure();

            // 绘制气泡框形状
            Color color = colorList[colorList.FindIndex(item => item.Key == num)].Value;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            graphics.FillPath(new SolidBrush(color), path);
            graphics.DrawPath(new Pen(color), path);
            bubbleList.Add(new Tuple<int, Rectangle>(num, new Rectangle(X, Y, width, height)));

            int diameter = 120;    //直径
            Color textColor = Color.White;

            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;      //抗锯齿
            graphics.FillEllipse(new SolidBrush(color), 0, 0, diameter, diameter);          //画圆
            if (num < 10)
            {
                graphics.DrawString(num.ToString(), fontBigTitle, new SolidBrush(textColor), 23, 9);        //拧紧序号
            }
            else if (num >= 10 && num < 99)
            {
                graphics.DrawString(num.ToString(), fontBigTitle, new SolidBrush(textColor), -4, 9);        //拧紧序号
            }
            else
            {
                graphics.DrawString(num.ToString(), fontBigTitleLen3, new SolidBrush(textColor), -10, 18);   //拧紧序号
            }
        }

        //画左上角气泡和√
        private void DrawBubbleAndTick_Max(Graphics graphics, Color circleColor)
        {
            int width = 350;  // 气泡框的宽度
            int height = 76;  // 气泡框的高度
            int radius = 20;  // 气泡框的圆角半径
            int X = 98;
            int Y = 20;

            // 创建一个GraphicsPath对象，用于绘制气泡框形状
            GraphicsPath path = new GraphicsPath();
            path.AddArc(X, Y, radius, radius, 180, 76);                                   // 左上角圆角
            path.AddArc(X + width - radius, Y, radius, radius, 270, 76);                  // 右上角圆角
            path.AddArc(X + width - radius, Y + height - radius, radius, radius, 0, 76);  // 右下角圆角
            path.AddArc(X, Y + height - radius, radius, radius, 90, 76);                  // 左下角圆角
            path.CloseFigure();

            // 绘制气泡框形状
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            graphics.FillPath(new SolidBrush(circleColor), path);
            graphics.DrawPath(new Pen(circleColor), path);

            int diameter = 120;    //直径
            Color textColor = Color.White;
            string tick = "√";
            Font fontTick = new Font("隶书", 61, FontStyle.Bold, GraphicsUnit.Point);

            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;   //抗锯齿
            graphics.FillEllipse(new SolidBrush(circleColor), 0, 0, diameter, diameter); //画圆
            graphics.DrawString(tick, fontTick, new SolidBrush(textColor), 4, 19);       //√
        }

        //画左上角气泡和×
        private void DrawBubbleAndFork_Max(Graphics graphics, Color circleColor)
        {
            int width = 350;  // 气泡框的宽度
            int height = 76;  // 气泡框的高度
            int radius = 20;  // 气泡框的圆角半径
            int X = 98;
            int Y = 20;

            // 创建一个GraphicsPath对象，用于绘制气泡框形状
            GraphicsPath path = new GraphicsPath();
            path.AddArc(X, Y, radius, radius, 180, 76);                                   // 左上角圆角
            path.AddArc(X + width - radius, Y, radius, radius, 270, 76);                  // 右上角圆角
            path.AddArc(X + width - radius, Y + height - radius, radius, radius, 0, 76);  // 右下角圆角
            path.AddArc(X, Y + height - radius, radius, radius, 90, 76);                  // 左下角圆角
            path.CloseFigure();

            // 绘制气泡框形状
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            graphics.FillPath(new SolidBrush(circleColor), path);
            graphics.DrawPath(new Pen(circleColor), path);

            int diameter = 120;    //直径
            Color textColor = Color.White;
            string tick = "×";
            Font fontTick = new Font("隶书", 61, FontStyle.Bold, GraphicsUnit.Point);

            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;   //抗锯齿
            graphics.FillEllipse(new SolidBrush(circleColor), 0, 0, diameter, diameter); //画圆
            graphics.DrawString(tick, fontTick, new SolidBrush(textColor), 4, 19);       //√
        }
        #endregion

        #region 设定左侧边栏的值

        //更新左侧边栏
        private void updateLeftInfo(int selectNum)
        {
            if (selectIndex > -1)
            {
                textBox5.Text = selectNum.ToString();
                int index = meTorqueAngleAndSetMode.FindIndex(item => item.Item1 == textBox5.Text);
                if (index != -1)
                {
                    textBox1.Text = meWrenchCodeAndID[index].Item2;         //扳手编码
                    textBox2.Text = meWrenchCodeAndID[index].Item3;         //扳手ID
                    textBox6.Text = meTorqueAngleAndSetMode[index].Item2;   //目标力矩
                    textBox7.Text = meTorqueAngleAndSetMode[index].Item3;   //目标角度

                    switch (meTorqueAngleAndSetMode[index].Item4)
                    {
                        case "不缓存":
                            ucCombox1.SelectedIndex = 0;
                            break;
                        case "缓存TRACK":
                            ucCombox1.SelectedIndex = 1;
                            break;
                        case "缓存PEAK":
                            ucCombox1.SelectedIndex = 2;
                            break;
                        default:
                            break;
                    }

                    switch (meWrenchMessage[index].Item2)
                    {
                        case "A1":
                            ucCombox2.SelectedIndex = 0;
                            break;
                        case "A2":
                            ucCombox2.SelectedIndex = 1;
                            break;
                        default:
                            break;
                    }

                    switch (meWrenchMessage[index].Item3)
                    {
                        case "TRACK":
                            ucCombox3.SelectedIndex = 0;
                            break;
                        case "PEAK":
                            ucCombox3.SelectedIndex = 1;
                            break;
                        default:
                            break;
                    }

                    switch (meWrenchMessage[index].Item4)
                    {
                        case "N·m":
                            ucCombox4.SelectedIndex = 0;
                            break;
                        case "lbf·in":
                            ucCombox4.SelectedIndex = 1;
                            break;
                        case "lbf·ft":
                            ucCombox4.SelectedIndex = 2;
                            break;
                        case "kgf·cm":
                            ucCombox4.SelectedIndex = 3;
                            break;
                        default:
                            break;
                    }

                    switch (meWrenchMessage[index].Item5)
                    {
                        case "M0":
                            ucCombox5.SelectedIndex = 0;
                            break;
                        case "M1":
                            ucCombox5.SelectedIndex = 1;
                            break;
                        case "M2":
                            ucCombox5.SelectedIndex = 2;
                            break;
                        case "M3":
                            ucCombox5.SelectedIndex = 3;
                            break;
                        case "M4":
                            ucCombox5.SelectedIndex = 4;
                            break;
                        case "M5":
                            ucCombox5.SelectedIndex = 5;
                            break;
                        case "M6":
                            ucCombox5.SelectedIndex = 6;
                            break;
                        case "M7":
                            ucCombox5.SelectedIndex = 7;
                            break;
                        case "M8":
                            ucCombox5.SelectedIndex = 8;
                            break;
                        case "M9":
                            ucCombox5.SelectedIndex = 9;
                            break;
                        default:
                            break;
                    }

                    switch (meWrenchMessage[index].Item6)
                    {
                        case "15°/sec":
                            ucCombox6.SelectedIndex = 0;
                            break;
                        case "30°/sec":
                            ucCombox6.SelectedIndex = 1;
                            break;
                        case "60°/sec":
                            ucCombox6.SelectedIndex = 2;
                            break;
                        case "120°/sec":
                            ucCombox6.SelectedIndex = 3;
                            break;
                        case "250°/sec":
                            ucCombox6.SelectedIndex = 4;
                            break;
                        case "500°/sec":
                            ucCombox6.SelectedIndex = 5;
                            break;
                        case "1000°/sec":
                            ucCombox6.SelectedIndex = 6;
                            break;
                        case "2000":
                            ucCombox6.SelectedIndex = 7;
                            break;
                        default:
                            break;
                    }
                }
            }
            else if (selectIndex == -1 && textBox5.Text.Equals(""))
            {
                textBox5.Text = "";
                textBox6.Text = "";
                textBox7.Text = "";
            }
        }

        #endregion

        //清除各元组list的值
        private void dataTuplesClear()
        {
            pointList.Clear();
            bubbleList.Clear();
            meWrenchCodeAndID.Clear();
            meTorqueAngleAndSetMode.Clear();
            meWrenchMessage.Clear();
        }

        // 判断点是否在圆内
        private bool IsPointInCircle(Point point, int circleX, int circleY, int radius = 20)
        {
            // 计算鼠标点击的位置与圆心的距离
            double distance = Math.Sqrt(Math.Pow(point.X - circleX, 2) + Math.Pow(point.Y - circleY, 2));

            // 判断距离是否小于等于圆的半径
            return distance <= radius;
        }

        //将特定元素加到末尾
        List<T> MoveElementToEnd<T>(List<T> list, int index)
        {
            if (list.Count == 1) return list;

            if (list.Count >= index + 1)
            {
                T element = list[index];
                list.RemoveAt(index);
                list.Add(element);
            }

            return list;
        }

        //将时间戳转换为时间（日期）
        private string ConvertTimestampToStr(string timestamp)
        {
            DateTime dateTime = MyDevice.GetTime(timestamp);
            return dateTime.ToString("yyyy-MM-dd");
        }
    }
}
