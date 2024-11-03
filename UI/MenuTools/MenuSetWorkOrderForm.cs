using BIL;
using Library;
using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;

//Lumi 20230811
//Junzhe 20230906
//Ricardo 20231205

namespace Base.UI.MenuTools
{
    public partial class MenuSetWorkOrderForm : Form
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

        int selectIndex = -1;                                         //选择的位点下标
        int selectNum = -1;                                           //选择的拧紧序号

        private List<Tuple<int, Point>> pointList = new List<Tuple<int, Point>>();           //Item1:拧紧序号, Item2:选点位置
        private List<Tuple<int, Rectangle>> bubbleList = new List<Tuple<int, Rectangle>>();  //Item1:拧紧序号, Item2:气泡范围
        private List<KeyValuePair<int, Color>> colorList = new List<KeyValuePair<int, Color>>();           //Item1:拧紧序号, Item2:气泡颜色

        //Item1:拧紧序号, Item2:扳手编码，Item3:扳手ID
        private List<Tuple<string, string, string>> meWrenchCodeAndID = new List<Tuple<string, string, string>>();
        //Item1:拧紧序号, Item2:A1A2模式, Item3:PT模式, Item4:单位, Item5:M0~M9模式, Item6:角度挡位
        private List<Tuple<string, string, string, string, string, string>> meWrenchMessage = new List<Tuple<string, string, string, string, string, string>>();
        //Item1:拧紧序号, Item2:目标力矩, Item3:目标角度, Item4:数据记录模式
        private List<Tuple<string, string, string, string>> meTorqueAngleAndSetMode = new List<Tuple<string, string, string, string>>();

        JDBC jdbc = new JDBC();

        private WorksInfo meWorksInfo = new WorksInfo();              //工单处理界面新建或选择的工单
        private bool isNewWorkInfo = false;                           //是否为新建的工单
        private string previousAxValue;                               //Ax切换前的值
        private string previousMxValue;                               //Mx切换前的值

        public WorksInfo MeWorksInfo { get => meWorksInfo; set => meWorksInfo = value; }
        public bool IsNewWorkInfo { get => isNewWorkInfo; set => isNewWorkInfo = value; }

        #endregion

        public MenuSetWorkOrderForm()
        {
            InitializeComponent();
        }

        //界面初始化
        private void MenuSetWorkOrderForm_Load(object sender, EventArgs e)
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

            jdbc.TableName = meWorksInfo.work_order_id;           //对应的工单号
            if (MyDevice.languageType == 0)
            {
                textBox3.Text = "工单号：" + meWorksInfo.work_order_id.ToString();
            }
            else
            {
                textBox3.Left = 370;
                textBox3.Text = "Work Order ID:" + meWorksInfo.work_order_id.ToString();
            }
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

        //页面加载稳定之后自动导入工单
        private void MenuSetWorkOrderForm_Shown(object sender, EventArgs e)
        {
            currentSize = pictureBox1.Size;

            //不是新建工单表时，读取已有数据
            if (!isNewWorkInfo)
            {
                dataTuplesClear();                                        //清除所有data元组list的值
                selectIndex = -1;                                         //选择的位点下标
                selectNum = -1;                                           //选择的拧紧序号

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
            }
        }

        //调整位点位置
        private void updatePictureBox1(Size newSize, Size oldSize)
        {
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
                DrawBigCircleAndIndex(graphics, pointList[selectIndex].Item1);
            }
            graphics.Save();
            graphics.Dispose();
            pictureBox1.Image = bubbles;
        }

        //导入工单
        private void buttonX1_Click(object sender, EventArgs e)
        {
            //currentSize = pictureBox1.Size;

            ////不是新建工单表时，读取已有数据
            //if (!isNewWorkInfo)
            //{
            //    dataTuplesClear();                                        //清除所有data元组list的值
            //    selectIndex = -1;                                         //选择的位点下标
            //    selectNum = -1;                                           //选择的拧紧序号

            //    List<WorkInfo> workInfoList = jdbc.QueryFullWorkInfo();   //从数据库读取工单表

            //    if (workInfoList == null)
            //    {
            //        if (MyDevice.languageType == 0)
            //        {
            //            MessageBox.Show("该工单未存储位点信息", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //        }
            //        else
            //        {
            //            MessageBox.Show("The ticket does not store site information", "System prompt", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //        }
            //        return;
            //    }
            //    mePicLocation = workInfoList[0].pic_location;
            //    Size dbSize = Size.Empty;                                 //数据库存储的picturebox大小
            //    string sizeStr = workInfoList[0].pic_news.Split('|')[1];
            //    if (sizeStr.StartsWith("{") && sizeStr.EndsWith("}"))
            //    {
            //        string[] coordinates = sizeStr.Trim('{', '}').Split(',');
            //        if (coordinates.Length == 2)
            //        {
            //            int x = int.Parse(coordinates[0].Split('=')[1]);
            //            int y = int.Parse(coordinates[1].Split('=')[1]);
            //            dbSize = new Size(x, y);
            //        }
            //    }

            //    foreach (WorkInfo wi in workInfoList)
            //    {
            //        Point dbPoint = Point.Empty;
            //        string pointStr = wi.pic_news.Split('|')[0];
            //        string[] wrenchMessageStrs = wi.wrench_message.Split('|');

            //        //获取点
            //        if (pointStr.StartsWith("{") && pointStr.EndsWith("}"))
            //        {
            //            string[] coordinates = pointStr.Trim('{', '}').Split(',');
            //            if (coordinates.Length == 2)
            //            {
            //                int x = int.Parse(coordinates[0].Split('=')[1]);
            //                int y = int.Parse(coordinates[1].Split('=')[1]);
            //                dbPoint = new Point(x, y);
            //            }
            //        }

            //        Tuple<int, Point> pointTuple = new Tuple<int, Point>(int.Parse(wi.screw_id), dbPoint);
            //        Tuple<string, string, string> meWrenchCodeAndIDTuple = new Tuple<string, string, string>(wi.screw_id, wi.wrench_code, wi.wrench_id);
            //        Tuple<string, string, string, string> torqueAngleSetModeTuple = new Tuple<string, string, string, string>(wi.screw_id, wi.target_torque, wi.target_angle, wi.set_mode);
            //        Tuple<string, string, string, string, string, string> wrenchMessageTuple = new Tuple<string, string, string, string, string, string>
            //                                                                                   (wi.screw_id, wrenchMessageStrs[0], wrenchMessageStrs[1], wrenchMessageStrs[2], wrenchMessageStrs[3], wrenchMessageStrs[4]);
            //        pointList.Add(pointTuple);
            //        meWrenchCodeAndID.Add(meWrenchCodeAndIDTuple);
            //        meTorqueAngleAndSetMode.Add(torqueAngleSetModeTuple);
            //        meWrenchMessage.Add(wrenchMessageTuple);
            //        updateColorList(int.Parse(wi.screw_id));
            //    }

            //    string picFolderPath = Application.StartupPath + @"\pic";
            //    if (!Directory.Exists(picFolderPath))
            //    {
            //        Directory.CreateDirectory(picFolderPath);
            //    }

            //    //画图
            //    pictureBox1.BackgroundImage = Image.FromFile(Application.StartupPath + mePicLocation);
            //    pictureBox1.BackgroundImageLayout = ImageLayout.Zoom;
            //    updatePictureBox1(currentSize, dbSize);
            //}
        }

        //保存工单
        private void buttonX2_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null) return;

            #region 报错弹窗
            foreach (Tuple<string, string, string> wrenchCodeAndIDTuple in meWrenchCodeAndID)
            {
                if (wrenchCodeAndIDTuple.Item3 == "")
                {
                    if (MyDevice.languageType == 0)
                    {
                        MessageBox.Show("位点 " + wrenchCodeAndIDTuple.Item1 + " 的扳手ID未填写，不能保存", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("The wrench ID for point " + wrenchCodeAndIDTuple.Item1 + " is not filled in, cannot save", "System prompt", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    return;
                }
                if (int.Parse(wrenchCodeAndIDTuple.Item3) < 1 || int.Parse(wrenchCodeAndIDTuple.Item3) > 255)
                {
                    if (MyDevice.languageType == 0)
                    {
                        MessageBox.Show("位点 " + wrenchCodeAndIDTuple.Item1 + " 的扳手ID不在规定范围内（1-255），不能保存", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("The wrench ID for point " + wrenchCodeAndIDTuple.Item1 + " is not within the specified range (1-255) and cannot be saved", "System prompt", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    return;
                }

                string screwId = wrenchCodeAndIDTuple.Item1;
                Tuple<string, string, string, string> torqueAngleSetModeTuple = meTorqueAngleAndSetMode.FirstOrDefault(item => item.Item1 == screwId);
                Tuple<string, string, string, string, string, string> WrenchMessageTuple = meWrenchMessage.FirstOrDefault(item => item.Item1 == screwId);
                if (WrenchMessageTuple.Item2 == "A1")
                {
                    if (torqueAngleSetModeTuple.Item2.Equals(""))
                    {
                        if (MyDevice.languageType == 0)
                        {
                            MessageBox.Show("位点 " + wrenchCodeAndIDTuple.Item1 + " 的目标力矩未填写，不能保存", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("The target torque at point " + wrenchCodeAndIDTuple.Item1 + " is not filled in, cannot save", "System prompt", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        return;
                    }
                }
                else if (WrenchMessageTuple.Item2 == "A2")
                {
                    if (torqueAngleSetModeTuple.Item2.Equals(""))
                    {
                        if (MyDevice.languageType == 0)
                        {
                            MessageBox.Show("位点 " + wrenchCodeAndIDTuple.Item1 + " 的目标力矩未填写，不能保存", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("The target torque at point " + wrenchCodeAndIDTuple.Item1 + " is not filled in, cannot save", "System prompt", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        return;
                    }
                    if (torqueAngleSetModeTuple.Item3.Equals(""))
                    {
                        if (MyDevice.languageType == 0)
                        {
                            MessageBox.Show("位点 " + wrenchCodeAndIDTuple.Item1 + " 的目标角度未填写，不能保存", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("The target angle at point " + wrenchCodeAndIDTuple.Item1 + " is not filled in, cannot save", "System prompt", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        return;
                    }
                }
                if (WrenchMessageTuple.Item2 == "A1" && WrenchMessageTuple.Item5 != "M0")
                {
                    if (torqueAngleSetModeTuple.Item2.IndexOf(',') == -1 || torqueAngleSetModeTuple.Item2.IndexOf(',') == torqueAngleSetModeTuple.Item2.Length - 1)
                    {
                        if (MyDevice.languageType == 0)
                        {
                            MessageBox.Show("位点 " + wrenchCodeAndIDTuple.Item1 + ": 力矩下限,上限需用逗号隔开\n请输入正确的格式\n例如：20,30", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        }
                        else
                        {
                            MessageBox.Show("The lower torque limit, the upper limit of point " + wrenchCodeAndIDTuple.Item1 + " needs to be separated by commas\nPlease enter the correct format\nFor example: 20, 30", "prompt", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        }
                        return;
                    }
                }
                else if (WrenchMessageTuple.Item2 == "A2" && WrenchMessageTuple.Item5 != "M0")
                {
                    if (torqueAngleSetModeTuple.Item3.IndexOf(',') == -1 || torqueAngleSetModeTuple.Item3.IndexOf(',') == torqueAngleSetModeTuple.Item3.Length - 1)
                    {
                        if (MyDevice.languageType == 0)
                        {
                            MessageBox.Show("位点 " + wrenchCodeAndIDTuple.Item1 + ": 角度下限,上限需用逗号隔开\n请输入正确的格式\n例如：20,30", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        }
                        else
                        {
                            MessageBox.Show("The lower angle limit, the upper limit of point " + wrenchCodeAndIDTuple.Item1 + " needs to be separated by commas\nPlease enter the correct format\nFor example: 20, 30", "prompt", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        }
                        return;
                    }
                }
            }
            #endregion

            List<WorkInfo> workInfoList = new List<WorkInfo>();   //工单表

            foreach (Tuple<string, string, string> wrenchCodeAndIDTuple in meWrenchCodeAndID)
            {
                string screwId = wrenchCodeAndIDTuple.Item1;
                string wrenchCode = wrenchCodeAndIDTuple.Item2;
                string wrenchID = wrenchCodeAndIDTuple.Item3;
                Point point = pointList.FirstOrDefault(item => item.Item1 == int.Parse(screwId)).Item2;

                Tuple<string, string, string, string> torqueAngleSetModeTuple = meTorqueAngleAndSetMode.FirstOrDefault(item => item.Item1 == screwId);
                Tuple<string, string, string, string, string, string> WrenchMessageTuple = meWrenchMessage.FirstOrDefault(item => item.Item1 == screwId);
                string targetTorque = torqueAngleSetModeTuple.Item2;
                string targetAngle = torqueAngleSetModeTuple.Item3;
                string setMode = torqueAngleSetModeTuple.Item4;
                string wrenchMessage = string.Join("|", WrenchMessageTuple.Item2, WrenchMessageTuple.Item3, WrenchMessageTuple.Item4, WrenchMessageTuple.Item5, WrenchMessageTuple.Item6);

                WorkInfo workInfo = new WorkInfo()
                {
                    time = MyDevice.GetTimeStamp(),
                    screw_id = screwId.ToString(),
                    wrench_code = wrenchCode,
                    wrench_id = wrenchID,
                    pic_location = mePicLocation,
                    pic_news = point.ToString() + "|" + currentSize.ToString(),
                    target_torque = targetTorque,
                    target_angle = targetAngle,
                    mac = MyDevice.myMac,
                    set_mode = setMode,
                    wrench_message = wrenchMessage
                };
                workInfoList.Add(workInfo);
            }

            jdbc.DelFullWorkInfo();                   //删除原有工单表的所有记录
            foreach (WorkInfo wi in workInfoList)
            {
                jdbc.AddWorkInfoByScrewIDType2(wi);   //添加记录
            }

            if (MyDevice.languageType == 0)
            {
                MessageBox.Show("工单保存成功", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("The ticket is saved", "System prompt", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        //插入图片
        private void buttonX3_Click(object sender, EventArgs e)
        {
            currentSize = pictureBox1.Size;

            string picFolderPath = Application.StartupPath + @"\pic";

            if (!Directory.Exists(picFolderPath))
            {
                Directory.CreateDirectory(picFolderPath);
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "图像文件|*.jpg;*.jpeg;*.png;*.bmp|所有文件|*.*";
            openFileDialog.Title = "选择要导入的图像文件";
            openFileDialog.InitialDirectory = picFolderPath;                                        //默认图片目录

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string imagePath = openFileDialog.FileName;                                         //真正选择的目录
                string imageName = Path.GetFileNameWithoutExtension(imagePath);                     //导入图片的文件名
                string imageExtension = Path.GetExtension(imagePath);                               //文件类型后缀
                string destinationPath = Path.Combine(picFolderPath, imageName + imageExtension);   //目标文件路径

                //如果pic文件夹内没有导入的图片，复制图像文件到pic文件夹
                if (!File.Exists(destinationPath))
                {
                    File.Copy(imagePath, destinationPath, true);
                }
                //导入图片重名，判断是否是同一张图
                else
                {
                    Bitmap importedImage = new Bitmap(imagePath);
                    Bitmap existingImage = new Bitmap(destinationPath);
                    bool isSameImage = ImageCompareArray(importedImage, existingImage);

                    if (!isSameImage)
                    {
                        //生成时间戳后缀直到不再有同名文件
                        string timeStampSuffix = MyDevice.GetMilTimeStamp();
                        imageName = imageName + "_" + timeStampSuffix;
                        destinationPath = Path.Combine(picFolderPath, imageName + imageExtension);
                        File.Copy(imagePath, destinationPath, true);
                    }
                }

                //保存到数据库的图片路径
                mePicLocation = @"\\pic\\" + imageName + imageExtension;
                mePicLocation = mePicLocation.Replace("\\", "/");

                //显示图片
                pictureBox1.BackgroundImageLayout = ImageLayout.Zoom;
                pictureBox1.BackgroundImage = Image.FromFile(imagePath);
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;

                //清除所有data元组list的值
                dataTuplesClear();
            }
        }

        //左键选位点，右键删除位点
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (pictureBox1.BackgroundImage == null) return;

            selectIndex = -1;                                 //选择的位点下标
            selectNum = -1;                                   //选择的拧紧序号

            MouseEventArgs mouseEventArgs = e as MouseEventArgs;
            Point clickPoint = mouseEventArgs.Location;       //鼠标点击的点位置

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

            //绘图
            if (selectIndex > -1)
            {
                //将选择的位点气泡置于最前
                if (bubbleList.Count > 1)
                {
                    bubbleList = MoveElementToEnd(bubbleList, selectIndex);
                    pointList = MoveElementToEnd(pointList, selectIndex);
                    selectIndex = pointList.Count - 1;
                }

                // 重新绘制所有圆形
                updatePictureBox1(currentSize, currentSize);
            }

            //更新左侧边栏
            updateLeftInfo();

            //右键删除选择的位点
            if (mouseEventArgs.Button == MouseButtons.Right)
            {
                if (pointList.Count < 1) return;
                if (selectNum == -1) return;

                DialogResult result = MyDevice.languageType == 0 ?
                    MessageBox.Show("是否删除位点" + selectNum + "？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) :
                    MessageBox.Show("Whether to delete the site" + selectNum + "？", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    //删除点
                    pointList.RemoveAll(item => item.Item1 == selectNum);
                    meWrenchCodeAndID.RemoveAll(item => item.Item1 == selectNum.ToString());
                    meTorqueAngleAndSetMode.RemoveAll(item => item.Item1 == selectNum.ToString());
                    meWrenchMessage.RemoveAll(item => item.Item1 == selectNum.ToString());

                    //删除点时，将删除的点的键值对的key改为（-2），点顺序也不改变
                    int index = colorList.FindIndex(item => item.Key == selectNum);
                    if (index != -1)
                    {
                        KeyValuePair<int, Color> item = colorList[index];
                        colorList.RemoveAt(index);
                        colorList.Insert(index, new KeyValuePair<int, Color>(-2, item.Value));
                    }

                    //删除点后，不选择任何点
                    textBox1.Text = "";
                    textBox2.Text = "";
                    textBox5.Text = "";
                    textBox6.Text = "";
                    textBox7.Text = "";
                    selectIndex = -1;
                    selectNum = -1;

                    // 清空PictureBox并重新绘制所有圆形
                    updatePictureBox1(currentSize, currentSize);
                }
            }
        }

        //左键双击加位点
        private void pictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (pictureBox1.Image == null) return;

            Point clickPoint = e.Location;           //鼠标点击的点位置
            if (e.Button == MouseButtons.Left)
            {
                DialogResult result = MyDevice.languageType == 0 ?
                    MessageBox.Show("是否在此处添加位点？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) :
                    MessageBox.Show("Whether to add sites here？", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    int num = 1;  //拧紧序号
                    if (pointList.Count > 0)
                    {
                        //优先将缺失的拧紧序号赋给新位点，如已有拧紧序号1,3,4,则新增的位点的拧紧序号为2
                        List<int> tempNums = pointList.Select(tuple => tuple.Item1).ToList();
                        List<int> missingNums = FindMissingNumbers(tempNums);

                        if (missingNums.Count == 0)
                        {
                            num = pointList.Count + 1;
                        }
                        else
                        {
                            List<int> sortedList = missingNums.OrderBy(number => number).ToList();
                            num = sortedList[0];
                        }
                    }
                    pointList.Add(new Tuple<int, Point>(num, clickPoint));

                    textBox5.Text = num.ToString();
                    meWrenchCodeAndID.Add(new Tuple<string, string, string>(num.ToString(), textBox1.Text, textBox2.Text));
                    meTorqueAngleAndSetMode.Add(new Tuple<string, string, string, string>
                                               (num.ToString(), textBox6.Text, textBox7.Text, ucCombox1.SelectedText));
                    meWrenchMessage.Add(new Tuple<string, string, string, string, string, string>
                                       (num.ToString(), ucCombox2.SelectedText, ucCombox3.SelectedText, ucCombox4.SelectedText, ucCombox5.SelectedText, ucCombox6.SelectedText));

                    //绘图
                    updateColorList(num);
                    selectIndex = bubbleList.Count;
                    selectNum = num;
                    updatePictureBox1(currentSize, currentSize);
                }
            }
        }

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

        #endregion

        #region 设定左侧边栏的值

        //更新左侧边栏
        private void updateLeftInfo()
        {
            if (selectIndex > -1)
            {
                textBox5.Text = selectNum.ToString();
                int index = meTorqueAngleAndSetMode.FindIndex(item => item.Item1 == textBox5.Text);
                if (index != -1)
                {
                    textBox1.Text = meWrenchCodeAndID[index].Item2;                //扳手编码
                    textBox2.Text = meWrenchCodeAndID[index].Item3;                //扳手ID
                    textBox6.Text = meTorqueAngleAndSetMode[index].Item2;          //目标力矩
                    textBox7.Text = meTorqueAngleAndSetMode[index].Item3;          //目标角度

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

        #region 获取左侧边栏的值

        //根据AxMx改变所需设置的参数
        private void updateCalRequest()
        {
            if (ucCombox2.SelectedIndex == 0 && ucCombox5.SelectedIndex == 0)
            {
                label6.Visible = true;
                label7.Visible = false;
                label6.Text = MyDevice.languageType == 0 ? "目标力矩：" : "Target torque:";
                label7.Text = "";
                textBox6.Visible = true;
                textBox7.Visible = false;
                textBox7.Text = "";
            }
            else if (ucCombox2.SelectedIndex == 0 && ucCombox5.SelectedIndex > 0)
            {
                label6.Visible = true;
                label7.Visible = false;
                label6.Text = MyDevice.languageType == 0 ? "力矩下限,上限：" : "Lower torque limit, upper limit:";
                label7.Text = "";
                textBox6.Visible = true;
                textBox7.Visible = false;
                textBox7.Text = "";
            }
            else if (ucCombox2.SelectedIndex == 1 && ucCombox5.SelectedIndex == 0)
            {
                label6.Visible = true;
                label7.Visible = true;
                label6.Text = MyDevice.languageType == 0 ? "目标力矩：" : "Target torque:";
                label7.Text = MyDevice.languageType == 0 ? "目标角度：" : "Target angle:";
                textBox6.Visible = true;
                textBox7.Visible = true;
            }
            else if (ucCombox2.SelectedIndex == 1 && ucCombox5.SelectedIndex > 0)
            {
                label6.Visible = true;
                label7.Visible = true;
                label6.Text = MyDevice.languageType == 0 ? "目标力矩：" : "Target torque:"; ;
                label7.Text = MyDevice.languageType == 0 ? "角度下限,上限：" : "Lower angle limit, upper limit:";
                textBox6.Visible = true;
                textBox7.Visible = true;
            }
        }

        //依据textbox的值更新位点信息
        private void updateWorkOrderInfo(System.Windows.Forms.TextBox textBox)
        {
            if (textBox5.Text.Equals("")) return;
            if (mePicLocation.Equals("")) return;

            int index = meWrenchCodeAndID.FindIndex(item => item.Item1 == textBox5.Text);
            if (index == -1) return;
            switch (textBox.Tag.ToString())
            {
                case "wrenchCode":
                    meWrenchCodeAndID[index] = new Tuple<string, string, string>(meWrenchCodeAndID[index].Item1, textBox.Text, meWrenchCodeAndID[index].Item3);
                    break;
                case "wrenchID":
                    meWrenchCodeAndID[index] = new Tuple<string, string, string>(meWrenchCodeAndID[index].Item1, meWrenchCodeAndID[index].Item2, textBox.Text);
                    break;
                case "targetTorque":
                    meTorqueAngleAndSetMode[index] = new Tuple<string, string, string, string>
                                                 (meTorqueAngleAndSetMode[index].Item1, textBox.Text, meTorqueAngleAndSetMode[index].Item3, meTorqueAngleAndSetMode[index].Item4);
                    break;
                case "targetAngle":
                    meTorqueAngleAndSetMode[index] = new Tuple<string, string, string, string>
                                                     (meTorqueAngleAndSetMode[index].Item1, meTorqueAngleAndSetMode[index].Item2, textBox.Text, meTorqueAngleAndSetMode[index].Item4);
                    break;
                default:
                    break;
            }
        }

        //依据combox的值更新位点信息
        private void updateWorkOrderInfo(HZH_Controls.Controls.UCCombox ucCombox)
        {
            if (textBox5.Text.Equals("")) return;
            if (mePicLocation.Equals("")) return;

            int index = meWrenchCodeAndID.FindIndex(item => item.Item1 == textBox5.Text);
            if (index == -1) return;
            switch (ucCombox.Tag.ToString())
            {
                case "angleMode":
                    meWrenchMessage[index] = new Tuple<string, string, string, string, string, string>
                                             (meWrenchMessage[index].Item1, meWrenchMessage[index].Item2, meWrenchMessage[index].Item3, meWrenchMessage[index].Item4, meWrenchMessage[index].Item5, ucCombox.SelectedText);
                    break;
                case "recordMode":
                    meTorqueAngleAndSetMode[index] = new Tuple<string, string, string, string>
                                 (meTorqueAngleAndSetMode[index].Item1, meTorqueAngleAndSetMode[index].Item2, meTorqueAngleAndSetMode[index].Item3, ucCombox.SelectedText);
                    break;
                case "unit":
                    meWrenchMessage[index] = new Tuple<string, string, string, string, string, string>
                                             (meWrenchMessage[index].Item1, meWrenchMessage[index].Item2, meWrenchMessage[index].Item3, ucCombox.SelectedText, meWrenchMessage[index].Item5, meWrenchMessage[index].Item6);
                    break;
                case "PTMode":
                    meWrenchMessage[index] = new Tuple<string, string, string, string, string, string>
                                             (meWrenchMessage[index].Item1, meWrenchMessage[index].Item2, ucCombox.SelectedText, meWrenchMessage[index].Item4, meWrenchMessage[index].Item5, meWrenchMessage[index].Item6);
                    break;
                case "A1A2Mode":
                    meWrenchMessage[index] = new Tuple<string, string, string, string, string, string>
                         (meWrenchMessage[index].Item1, ucCombox.SelectedText, meWrenchMessage[index].Item3, meWrenchMessage[index].Item4, meWrenchMessage[index].Item5, meWrenchMessage[index].Item6);
                    break;
                case "M09Mode":
                    meWrenchMessage[index] = new Tuple<string, string, string, string, string, string>
                                             (meWrenchMessage[index].Item1, meWrenchMessage[index].Item2, meWrenchMessage[index].Item3, meWrenchMessage[index].Item4, ucCombox.SelectedText, meWrenchMessage[index].Item6);
                    break;
                default:
                    break;
            }
        }

        //拧紧序号
        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            label13.Text = "";
            if (selectIndex == -1 || selectIndex >= pointList.Count) return;  //新增位点或删除位点

            int oldID = pointList[pointList.Count - 1].Item1;   //修改前的拧紧序号
            int textBox5Value;                                  //修改后的拧紧序号
            if (!int.TryParse(textBox5.Text, out textBox5Value))//修改后的拧紧序号不为正整数（为空）
            {
                label13.Text = MyDevice.languageType == 0 ? "拧紧序号不能为空" : "Screw number cannot be null";
                return;
            }
            if (textBox5Value == oldID) return;                 //重复点击位点
            if (pointList.FindIndex(item => item.Item1 == textBox5Value) != -1)   //修改后的序号与之前已有序号重复
            {
                label13.Text = MyDevice.languageType == 0 ? "已存在拧紧序号为 " + textBox5.Text + " 的位点" : "There is a duplicate point " + textBox5.Text;
                return;
            }
            label13.Text = "";

            //修改拧紧序号
            pointList[pointList.Count - 1] = new Tuple<int, Point>(textBox5Value, pointList[pointList.Count - 1].Item2);

            int lastIndex = meWrenchCodeAndID.FindLastIndex(item => item.Item1 == oldID.ToString());
            bubbleList[lastIndex] = new Tuple<int, Rectangle>(textBox5Value, bubbleList[lastIndex].Item2);
            meWrenchCodeAndID[lastIndex] = new Tuple<string, string, string>(textBox5.Text, meWrenchCodeAndID[lastIndex].Item2, meWrenchCodeAndID[lastIndex].Item3);
            meWrenchMessage[lastIndex] = new Tuple<string, string, string, string, string, string>
                                        (textBox5.Text, meWrenchMessage[lastIndex].Item2, meWrenchMessage[lastIndex].Item3, meWrenchMessage[lastIndex].Item4, meWrenchMessage[lastIndex].Item5, meWrenchMessage[lastIndex].Item6);
            meTorqueAngleAndSetMode[lastIndex] = new Tuple<string, string, string, string>
                            (textBox5.Text, meTorqueAngleAndSetMode[lastIndex].Item2, meTorqueAngleAndSetMode[lastIndex].Item3, meTorqueAngleAndSetMode[lastIndex].Item4);

            updateColorList(textBox5Value, oldID);
            updatePictureBox1(currentSize, currentSize);
        }

        //扳手编码
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            updateWorkOrderInfo(textBox1);
        }

        //扳手id
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            updateWorkOrderInfo(textBox2);
        }

        //目标力矩
        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            updateWorkOrderInfo(textBox6);
        }

        //目标角度
        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            updateWorkOrderInfo(textBox7);
        }

        //角度挡位
        private void ucCombox6_SelectedChangedEvent(object sender, EventArgs e)
        {
            updateWorkOrderInfo(ucCombox6);
        }

        //数据记录模式
        private void ucCombox1_SelectedChangedEvent(object sender, EventArgs e)
        {
            updateWorkOrderInfo(ucCombox1);
        }

        //单位
        private void ucCombox4_SelectedChangedEvent(object sender, EventArgs e)
        {
            updateWorkOrderInfo(ucCombox4);
        }

        //PT模式
        private void ucCombox3_SelectedChangedEvent(object sender, EventArgs e)
        {
            updateWorkOrderInfo(ucCombox3);
        }

        //A1A2模式
        private void ucCombox2_SelectedChangedEvent(object sender, EventArgs e)
        {
            updateWorkOrderInfo(ucCombox2);

            //根据AxMx改变所需设置的参数
            updateCalRequest();

            if (ucCombox2.TextValue != previousAxValue)
            {
                // ComboBox的Text值已经改变
                AxMxChanged();
            }
            previousAxValue = ucCombox2.TextValue;
        }

        //M0-M9模式
        private void ucCombox5_SelectedChangedEvent(object sender, EventArgs e)
        {
            updateWorkOrderInfo(ucCombox5);

            //根据AxMx改变所需设置的参数
            updateCalRequest();

            if (ucCombox5.TextValue != previousMxValue)
            {
                // ComboBox的Text值已经改变
                AxMxChanged();
            }
            previousMxValue = ucCombox5.TextValue;
        }

        //MxAx更改左边栏参数改变
        private void AxMxChanged()
        {
            textBox5.Text = selectNum.ToString();
            int index = meTorqueAngleAndSetMode.FindIndex(item => item.Item1 == textBox5.Text);
            if (index != -1)
            {
                if (ucCombox2.TextValue == "A1")
                {
                    //A1M0模式
                    if (ucCombox5.TextValue == "M0")
                    {
                        //目标力矩
                        if (meTorqueAngleAndSetMode[index].Item2.IndexOf(",") != -1)
                        {
                            textBox6.Text = meTorqueAngleAndSetMode[index].Item2.Split(',')[0];
                        }
                        else
                        {
                            textBox6.Text = meTorqueAngleAndSetMode[index].Item2;
                        }
                    }
                    //A1M1 - A1M9模式
                    else
                    {
                        //力矩上下限
                        if (meTorqueAngleAndSetMode[index].Item2.IndexOf(",") == -1)
                        {
                            if (textBox6.Text != "")
                            {
                                textBox6.Text = meTorqueAngleAndSetMode[index].Item2 + ",";
                            }
                        }
                        else
                        {
                            textBox6.Text = meTorqueAngleAndSetMode[index].Item2;
                        }
                    }
                }
                else
                {
                    //A2M0模式
                    if (ucCombox5.TextValue == "M0")
                    {
                        //目标力矩
                        if (meTorqueAngleAndSetMode[index].Item2.IndexOf(",") != -1)
                        {
                            textBox6.Text = meTorqueAngleAndSetMode[index].Item2.Split(',')[0];
                        }
                        else
                        {
                            textBox6.Text = meTorqueAngleAndSetMode[index].Item2;
                        }

                        //目标角度
                        if (meTorqueAngleAndSetMode[index].Item3.IndexOf(",") != -1)
                        {
                            textBox7.Text = meTorqueAngleAndSetMode[index].Item3.Split(',')[0];
                        }
                        else
                        {
                            textBox7.Text = meTorqueAngleAndSetMode[index].Item3;
                        }
                    }
                    //A2M1 - A2M9模式
                    else
                    {
                        //目标力矩
                        if (meTorqueAngleAndSetMode[index].Item2.IndexOf(",") != -1)
                        {
                            textBox6.Text = meTorqueAngleAndSetMode[index].Item2.Split(',')[0];
                        }
                        else
                        {
                            textBox6.Text = meTorqueAngleAndSetMode[index].Item2;
                        }

                        //角度上下限
                        if (meTorqueAngleAndSetMode[index].Item3.IndexOf(",") == -1)
                        {
                            if (meTorqueAngleAndSetMode[index].Item3 != "")
                            {
                                textBox7.Text = meTorqueAngleAndSetMode[index].Item3 + ",";
                            }
                        }
                        else
                        {
                            textBox7.Text = meTorqueAngleAndSetMode[index].Item3;
                        }
                    }
                }
            }
        }

        #endregion

        #region 文本框输入限制

        //拧紧序号
        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 只允许输入正整数,长度限制3
            BoxRestrict.KeyPress_IntegerPositive_len3(sender, e);
        }

        //扳手编码
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            //没有位点被选择时，不能输入
            if (textBox5.Text == "")
            {
                e.Handled = true;
                return;
            }

            //不可以有以下特殊字符
            // \/:*?"<>|
            // \\
            // \|
            // ""
            BoxRestrict.KeyPress_FileName(sender, e);
        }

        //扳手id
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            //没有位点被选择时，不能输入
            if (textBox5.Text == "")
            {
                e.Handled = true;
                return;
            }

            // 只允许输入正整数,长度限制3
            BoxRestrict.KeyPress_IntegerPositive_len3(sender, e);
        }

        //目标力矩
        private void textBox6_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 只允许输入有理数和逗号
            Restrict_KeyPress_RationalNumber(sender, e);
        }

        //目标角度
        private void textBox7_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 只允许输入有理数和逗号
            Restrict_KeyPress_RationalNumber(sender, e);
        }

        //力矩上下限输入限制
        private void textBox6_MouseLeave(object sender, EventArgs e)
        {
            if (label6.Text == "力矩下限,上限：" || label6.Text == "Lower torque limit, upper limit:")
            {
                if (textBox6.Text.IndexOf(',') == -1 || textBox6.Text.IndexOf(',') == textBox6.Text.Length - 1)
                {
                    if (MyDevice.languageType == 0)
                    {
                        MessageBox.Show("力矩下限,上限需用逗号隔开\n请输入正确的格式\n例如：20,30", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    else
                    {
                        MessageBox.Show("The lower torque limit, the upper limit needs to be separated by commas\nPlease enter the correct format\nFor example: 20, 30", "prompt", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    
                    //自动纠正错误格式
                    if (textBox6.Text != "")
                    {
                        if (textBox6.Text.IndexOf(',') == -1)
                        {
                            textBox6.Text = textBox6.Text + "," + (Convert.ToInt32(textBox6.Text) + 1).ToString();
                        }
                        else
                        {
                            if (textBox6.Text != ",")
                            {
                                textBox6.Text = textBox6.Text + (Convert.ToInt32(textBox6.Text.Split(',')[0]) + 1).ToString();
                            }
                            else
                            {
                                textBox6.Text = "1,2";
                            }
                        }
                    }
                    //空值则自动赋值
                    else
                    {
                        textBox6.Text = "1,2";
                    }

                    return;
                }
            }
            else
            {
                if (textBox6.Text.IndexOf(',') != -1)
                {
                    if (MyDevice.languageType == 0)
                    {
                        MessageBox.Show("输入格式有误！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    else
                    {
                        MessageBox.Show("The input format is incorrect！", "prompt", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    return;
                }
            }
        }

        //角度上下限输入限制
        private void textBox7_MouseLeave(object sender, EventArgs e)
        {
            if (label7.Text == "角度下限,上限：" || label7.Text == "Lower angle limit, upper limit:")
            {
                if (textBox7.Text.IndexOf(',') == -1 || textBox7.Text.IndexOf(',') == textBox7.Text.Length - 1)
                {
                    if (MyDevice.languageType == 0)
                    {
                        MessageBox.Show("角度下限,上限需用逗号隔开\n请输入正确的格式\n例如：20,30", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    else
                    {
                        MessageBox.Show("Lower angle, upper limit separated by commas\nPlease enter the correct format\nExample: 20,30", "prompt", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }

                    //自动纠正错误格式
                    if (textBox7.Text != "")
                    {
                        if (textBox7.Text.IndexOf(',') == -1)
                        {
                            textBox7.Text = textBox7.Text + "," + (Convert.ToInt32(textBox7.Text) + 1).ToString();
                        }
                        else
                        {
                            if (textBox7.Text != ",")
                            {
                                textBox7.Text = textBox7.Text + (Convert.ToInt32(textBox7.Text.Split(',')[0]) + 10).ToString();
                            }
                            else
                            {
                                textBox7.Text = "10,20";
                            }
                        }
                    }
                    else
                    {
                        textBox7.Text = "10,20";
                    }

                    return;
                }
            }
            else
            {
                if (textBox7.Text.IndexOf(',') != -1)
                {
                    if (MyDevice.languageType == 0)
                    {
                        MessageBox.Show("输入格式有误！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    else
                    {
                        MessageBox.Show("The input format is incorrect！", "prompt", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    return;
                }
            }
        }

        //只允许输入有理数和逗号
        private void Restrict_KeyPress_RationalNumber(object sender, KeyPressEventArgs e)
        {
            //没有位点被选择时，不能输入
            if (textBox5.Text == "")
            {
                e.Handled = true;
                return;
            }

            // 只允许输入有理数和逗号
            BoxRestrict.KeyPress_RationalNumber(sender, e);
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

        //比较两张图片是否完全一样
        private bool ImageCompareArray(Bitmap firstImage, Bitmap secondImage)
        {
            MemoryStream ms = new MemoryStream();
            firstImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            String firstBitmap = Convert.ToBase64String(ms.ToArray());
            ms.Position = 0;
            secondImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            String secondBitmap = Convert.ToBase64String(ms.ToArray());
            if (firstBitmap.Equals(secondBitmap))
            {
                return true;
            }
            else
            {
                return false;
            }
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

        //找缺失的序号
        List<int> FindMissingNumbers(List<int> numbers)
        {
            int maxNumber = numbers.Max();
            List<int> missingNumbers = new List<int>();

            for (int i = 1; i <= maxNumber; i++)
            {
                if (!numbers.Contains(i))
                {
                    missingNumbers.Add(i);
                }
            }

            return missingNumbers;
        }
    }
}
