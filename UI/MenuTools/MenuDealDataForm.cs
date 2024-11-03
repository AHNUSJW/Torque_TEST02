using BIL;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Library;
using Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

//Junzhe 20230925
//Ricardo 20231128
//Ricardo 20240110
//Ricardo 20240410

namespace Base.UI.MenuTools
{
    public partial class MenuDealDataForm : Form
    {
        JDBC jdbc = new JDBC();//数据库方法
        List<DatasInfo> DatasInfoList = new List<DatasInfo>();     //数据总表
        List<DataInfo> dataInfos = new List<DataInfo>();           //数据详细信息

        private string oldVin = "";                //加载出来的上一条VIN
        private string click_VIN = "";             //双击的作业号VIN
        private string firstClick_tableName = "";  //第一次双击获取的表格名

        private string reportFileName;
        private string reportCompany;
        private string reportLoad;
        private string reportCommodity;
        private string reportStandard;
        private string reportOpsn;
        private string reportDate;

        private string reportTorquePeak;
        private string reportAnglePeak;
        private string reportAngleSpeed;
        private string reportTorqueUnit;
        private string reportResidualTorque;
        private string reportStamp;
        private string reportWorkOrder;
        private string reportScrewID;
        private string reportDeviceID;

        private List<double> torqueList = new List<double>();   //扭矩集合
        private List<double> angleList = new List<double>();    //角度集合
        private List<double> ResidualtorqueList = new List<double>();  //用于计算残余扭矩的扭矩集合
        private List<double> ResidualangleList = new List<double>();   //用于计算残余扭矩的扭矩集合

        public MenuDealDataForm()
        {
            InitializeComponent();
        }

        //页面加载
        private void MenuDealDataForm_Load(object sender, EventArgs e)
        {
            //加载数据总表
            showDatasInfo();

            //绘图初始化
            picture_Draw();

            //鼠标委托(滚轮缩放，右键恢复)
            chart1.MouseWheel += new MouseEventHandler(chart_MouseWheel);
            chart2.MouseWheel += new MouseEventHandler(chart_MouseWheel);
            chart1.MouseClick += new MouseEventHandler(chart_MouseClick);
            chart2.MouseClick += new MouseEventHandler(chart_MouseClick);
        }

        //加载数据总表
        private void showDatasInfo()
        {
            //隐藏返回按钮、生成报告按钮
            bt_Return.Visible = false;
            bt_Report.Visible = false;

            //隐藏曲线数据选择下拉框
            label2.Visible = false;
            ucCombox1.Visible = false;

            //表格初始化
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();
            dataGridView1.ClearSelection();

            //加列
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());

            //列名
            dataGridView1.Columns[0].HeaderText = MyDevice.languageType == 0 ? "序号" : "lines";
            dataGridView1.Columns[1].HeaderText = MyDevice.languageType == 0 ? "日期" : "time";
            dataGridView1.Columns[2].HeaderText = MyDevice.languageType == 0 ? "数据表名称" : "Data table name";
            dataGridView1.Columns[3].HeaderText = MyDevice.languageType == 0 ? "备注" : "notes";

            //获取数据总表
            DatasInfoList = jdbc.GetListDatas();
            if (DatasInfoList == null) return;

            //填入数据总表信息
            foreach (DatasInfo datasInfo in DatasInfoList)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGridView1, datasInfo.id, datasInfo.time, datasInfo.name, datasInfo.note);
                dataGridView1.Rows.Add(row);
            }
        }

        //返回 数据总表/作业号数据表
        private void bt_Return_Click(object sender, EventArgs e)
        {
            //绘图初始化
            chart1.Visible = true;
            chart2.Visible = true;
            label2.Visible = false;
            ucCombox1.Visible = false;

            picture_Draw();

            //根据生成报告按钮是否出现加载不同表格数据
            if (!bt_Report.Visible)
            {
                //加载数据总表
                showDatasInfo();
            }
            else
            {
                //加载作业号对应数据段
                showVINDatasInfo();

                //清除曲线数据
                this.chart1.Series[0].Points.Clear();
                this.chart2.Series[0].Points.Clear();

                //清除曲线缩放
                chart1.ChartAreas[0].AxisX.ScaleView.ZoomReset(0);
                chart1.ChartAreas[0].AxisY.ScaleView.ZoomReset(0);
                chart2.ChartAreas[0].AxisX.ScaleView.ZoomReset(0);
                chart2.ChartAreas[0].AxisY.ScaleView.ZoomReset(0);
            }
        }

        //加载作业号数据
        private void showVINDatasInfo()
        {
            //隐藏返回按钮、生成报告按钮
            bt_Return.Visible = true;
            bt_Report.Visible = false;

            //获取双击的数据表名
            jdbc.TableName = firstClick_tableName;

            //表格初始化
            unit_dataGridView();

            //获取详细数据
            dataInfos = jdbc.GetListData();
            if (dataInfos == null) return;

            UInt32 lines = 1;

            oldVin = dataInfos[0].work_vin;//初始化oldVin

            //数据填入表格
            for (int i = 0; i < dataInfos.Count; i++)
            {
                if (oldVin != dataInfos[i].work_vin)
                {
                    oldVin = dataInfos[i].work_vin;
                    DataGridViewRow row = new DataGridViewRow();
                    row.CreateCells(dataGridView1,
                                    lines,
                                    ConvertTimestampToStr(dataInfos[i - 1].time),
                                    dataInfos[i - 1].work_vin,
                                    dataInfos[i - 1].work_order_id,
                                    dataInfos[i - 1].screw_id,
                                    dataInfos[i - 1].wrench_code,
                                    dataInfos[i - 1].wrench_id,
                                    "",
                                    "",
                                    dataInfos[i - 1].torque_peak,
                                    dataInfos[i - 1].angle_peak,
                                    dataInfos[i - 1].result,
                                    dataInfos[i - 1].torque_result,
                                    dataInfos[i - 1].angle_result);
                    dataGridView1.Rows.Add(row);
                    lines++;
                }
                //添加最后一条作业号
                if (i == dataInfos.Count - 1)
                {
                    DataGridViewRow rowLast = new DataGridViewRow();
                    rowLast.CreateCells(dataGridView1,
                                        lines,
                                        ConvertTimestampToStr(dataInfos[i].time),
                                        dataInfos[i].work_vin,
                                        dataInfos[i].work_order_id,
                                        dataInfos[i].screw_id,
                                        dataInfos[i].wrench_code,
                                        dataInfos[i].wrench_id,
                                        "",
                                        "",
                                        dataInfos[i].torque_peak,
                                        dataInfos[i].angle_peak,
                                        dataInfos[i].result,
                                        dataInfos[i].torque_result,
                                        dataInfos[i].angle_result);
                    dataGridView1.Rows.Add(rowLast);
                }
            }
        }

        //左键双击查看作业号数据
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.Rows.Count != 0)
            {
                //显示某天汇总数据表
                if (bt_Return.Visible == false)
                {
                    //显示返回按钮
                    bt_Return.Visible = true;
                    bt_Report.Visible = false;

                    //隐藏曲线数据选择下拉框
                    label2.Visible = false;
                    ucCombox1.Visible = false;

                    //第一次双击存储的表名，便于后续用于返回
                    firstClick_tableName = DatasInfoList[dataGridView1.CurrentRow.Index].name;

                    //获取双击的数据表名
                    jdbc.TableName = firstClick_tableName;

                    //表格初始化
                    unit_dataGridView();

                    //获取详细数据
                    dataInfos = jdbc.GetListData();
                    if (dataInfos == null) return;

                    UInt32 lines = 1;
                    oldVin = dataInfos[0].work_vin;
                    //数据填入表格
                    for (int i = 0; i < dataInfos.Count; i++)
                    {
                        if (oldVin != dataInfos[i].work_vin)
                        {
                            oldVin = dataInfos[i].work_vin;
                            DataGridViewRow row = new DataGridViewRow();
                            row.CreateCells(dataGridView1,
                                            lines,
                                            ConvertTimestampToStr(dataInfos[i - 1].time),
                                            dataInfos[i - 1].work_vin,
                                            dataInfos[i - 1].work_order_id,
                                            dataInfos[i - 1].screw_id,
                                            dataInfos[i - 1].wrench_code,
                                            dataInfos[i - 1].wrench_id,
                                            "",
                                            "",
                                            dataInfos[i - 1].torque_peak,
                                            dataInfos[i - 1].angle_peak,
                                            dataInfos[i - 1].result,
                                            dataInfos[i - 1].torque_result,
                                            dataInfos[i - 1].angle_result);
                            dataGridView1.Rows.Add(row);
                            lines++;
                        }
                        //添加最后一条作业号
                        if (i == dataInfos.Count - 1)
                        {
                            DataGridViewRow rowLast = new DataGridViewRow();
                            rowLast.CreateCells(dataGridView1,
                                                lines,
                                                ConvertTimestampToStr(dataInfos[i].time),
                                                dataInfos[i].work_vin,
                                                dataInfos[i].work_order_id,
                                                dataInfos[i].screw_id,
                                                dataInfos[i].wrench_code,
                                                dataInfos[i].wrench_id,
                                                "",
                                                "",
                                                dataInfos[i].torque_peak,
                                                dataInfos[i].angle_peak,
                                                dataInfos[i].result,
                                                dataInfos[i].torque_result,
                                                dataInfos[i].angle_result);
                            dataGridView1.Rows.Add(rowLast);
                        }
                    }
                }
                //显示流水号数据汇总
                else if (bt_Report.Visible == false)
                {
                    //显示返回按钮
                    bt_Return.Visible = true;
                    bt_Report.Visible = true;

                    //显示曲线模式选择下拉框
                    label2.Visible = true;
                    ucCombox1.Visible = true;

                    //曲线模式选择初始化
                    ucCombox1.Source = new List<KeyValuePair<string, string>>();
                    ucCombox1.Source.Add(new KeyValuePair<string, string>(0.ToString(), "常规曲线模式"));
                    ucCombox1.Source.Add(new KeyValuePair<string, string>(1.ToString(), "残余扭矩检测"));
                    ucCombox1.SelectedIndex = 0;

                    //ucCombox1_SelectedChangedEvent(null, null);
                }
            }
            else
                return;
        }

        //曲线模式切换绘图
        private void ucCombox1_SelectedChangedEvent(object sender, EventArgs e)
        {
            //清空数据
            chart1.Series[0].Points.Clear();
            chart2.Series[0].Points.Clear();

            chart1.ChartAreas[0].AxisX.ScaleView.ZoomReset(0);
            chart1.ChartAreas[0].AxisY.ScaleView.ZoomReset(0);
            chart2.ChartAreas[0].AxisX.ScaleView.ZoomReset(0);
            chart2.ChartAreas[0].AxisY.ScaleView.ZoomReset(0);

            torqueList.Clear();
            angleList.Clear();
            ResidualtorqueList.Clear();
            ResidualangleList.Clear();

            foreach (DatasInfo datasInfo in DatasInfoList)
            {
                if (dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[1].Value.ToString().Split(' ')[0] == datasInfo.time)
                {
                    //获取双击的数据表名
                    jdbc.TableName = datasInfo.name;
                }
            }

            //获取双击的作业号VIN
            click_VIN = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[2].Value.ToString();

            //表格初始化
            unit_dataGridView();

            //获取详细数据
            dataInfos = jdbc.GetListData();
            if (dataInfos == null) return;

            UInt32 lines = 1;   //序列号
            Double X_Axis = 1;  //X轴横坐标

            //数据填入表格
            foreach (DataInfo dataInfo in dataInfos)
            {
                if (click_VIN == dataInfo.work_vin)
                {
                    DataGridViewRow row = new DataGridViewRow();
                    row.CreateCells(dataGridView1,
                                    lines,
                                    ConvertTimestampToStr(dataInfo.time),
                                    dataInfo.work_vin,
                                    dataInfo.work_order_id,
                                    dataInfo.screw_id,
                                    dataInfo.wrench_code,
                                    dataInfo.wrench_id,
                                    dataInfo.actual_torque,
                                    dataInfo.actual_angle,
                                    dataInfo.torque_peak,
                                    dataInfo.angle_peak,
                                    dataInfo.result,
                                    dataInfo.torque_result,
                                    dataInfo.angle_result);
                    dataGridView1.Rows.Add(row);

                    //常规模式分别画角度曲线和扭矩曲线
                    if (ucCombox1.SelectedIndex == 0)
                    {
                        chart1.Visible = true;
                        chart2.Visible = true;
                        chart1.Series[0].LegendText = "角度值";

                        //不需要X轴刻度
                        chart1.ChartAreas[0].AxisX.LabelStyle.Enabled = false;
                        chart2.ChartAreas[0].AxisX.LabelStyle.Enabled = false;

                        //更新实时曲线
                        chart1.Series[0].Points.AddXY(X_Axis, Convert.ToDouble(dataInfo.actual_angle.ToString().Split(' ')[0]));
                        chart2.Series[0].Points.AddXY(X_Axis, Convert.ToDouble(dataInfo.actual_torque.ToString().Split(' ')[0]));

                        lines++;
                        X_Axis++;
                    }
                    //残余扭矩检测模式下以angle为X轴, torque为Y轴
                    else
                    {
                        chart2.Visible = false;
                        chart1.Series[0].LegendText = "A - T 值";

                        //X轴刻度
                        chart1.ChartAreas[0].AxisX.LabelStyle.Enabled = true;

                        // 设置轴显示数值 轴刻度间隔为10
                        chart1.ChartAreas[0].AxisX.Interval = 2;
                        chart1.ChartAreas[0].AxisX.LabelStyle.Format = "0";
                        chart1.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.Black;
                        //chart1.ChartAreas[0].AxisY.Interval = 2;
                        //chart1.ChartAreas[0].AxisY.LabelStyle.Format = "0";

                        //更新A - T 曲线
                        //chart1.Series[0].Points.AddXY(Convert.ToDouble(dataInfo.actual_angle.ToString().Split(' ')[0]), Convert.ToDouble(dataInfo.actual_torque.ToString().Split(' ')[0]));

                        if (Convert.ToDouble(dataInfo.actual_angle.ToString().Split(' ')[0]) != 0)
                        {
                            chart1.Series[0].Points.AddXY(Convert.ToDouble(dataInfo.actual_angle.ToString().Split(' ')[0]), Convert.ToDouble(dataInfo.actual_torque.ToString().Split(' ')[0]));
                        }
                        else
                        {
                            chart1.Series[0].Points.AddXY(Convert.ToDouble(0.01), Convert.ToDouble(dataInfo.actual_torque.ToString().Split(' ')[0]));
                        }
                        lines++;

                    }

                    torqueList.Add(Convert.ToDouble(dataInfo.actual_torque.Split(' ')[0]));
                    angleList.Add(Convert.ToDouble(dataInfo.actual_angle.Split(' ')[0]));
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
                System.Drawing.Font font = new System.Drawing.Font("Arial", 12);
                string text = "残余扭矩值: ";
                PointF position = new PointF(chart1.Width / 2, 15);
                if (GetResidualTorque(1, ResidualtorqueList, ResidualangleList) != 0)
                {
                    reportResidualTorque = GetResidualTorque(1, ResidualtorqueList, ResidualangleList).ToString();//有效残余扭矩
                    if (ucCombox1.SelectedIndex == 1)//在残余扭矩曲线上显示残余扭矩值
                    {
                        g.DrawString(text + GetResidualTorque(1, ResidualtorqueList, ResidualangleList), font, Brushes.Blue, position);
                    }
                }
            }
            else
            {
                reportResidualTorque = "";//数据量小于10不计算残余扭矩
            }
        }

        //计算再拧紧扭矩值
        private double GetResidualTorque(int mode, List<double> torqueList, List<double> angleList)
        {
            List<double> agvTorqueList = new List<double>();
            List<double> agvAngleList = new List<double>();
            List<double> slopeList = new List<double>();      //斜率集合
            double avgTorque;
            double avgAngle;
            double slope;
            if (mode == 1)
            {
                //先求8个一组的平均值，再求斜率，最后滤波
                if (torqueList.Count > 10)
                {

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
                        if (slopeList[i] < 0 && slopeList[i - 1] > 0)
                        {
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

        //扳手数据表格初始化
        private void unit_dataGridView()
        {
            //表格初始化
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();
            dataGridView1.ClearSelection();

            //加列
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());

            //列名
            dataGridView1.Columns[0].HeaderText = MyDevice.languageType == 0 ? "序号" : "lines";
            dataGridView1.Columns[1].HeaderText = MyDevice.languageType == 0 ? "时间" : "time";
            dataGridView1.Columns[2].HeaderText = MyDevice.languageType == 0 ? "作业号" : "VIN";
            dataGridView1.Columns[3].HeaderText = MyDevice.languageType == 0 ? "工单号" : "Workoder ID";
            dataGridView1.Columns[4].HeaderText = MyDevice.languageType == 0 ? "拧紧序号" : "Screw ID";
            dataGridView1.Columns[5].HeaderText = MyDevice.languageType == 0 ? "设备编码" : "Device encoding";
            dataGridView1.Columns[6].HeaderText = MyDevice.languageType == 0 ? "设备ID" : "Device ID";
            dataGridView1.Columns[7].HeaderText = MyDevice.languageType == 0 ? "实时扭矩" : "Real-time torque";
            dataGridView1.Columns[8].HeaderText = MyDevice.languageType == 0 ? "实时角度" : "Real-time angle";
            dataGridView1.Columns[9].HeaderText = MyDevice.languageType == 0 ? "峰值扭矩" : "torque peak";
            dataGridView1.Columns[10].HeaderText = MyDevice.languageType == 0 ? "峰值角度" : "angle peak";
            dataGridView1.Columns[11].HeaderText = MyDevice.languageType == 0 ? "结果" : "result";
            dataGridView1.Columns[12].HeaderText = MyDevice.languageType == 0 ? "扭矩结果" : "torque result";
            dataGridView1.Columns[13].HeaderText = MyDevice.languageType == 0 ? "角度结果" : "angle result";
        }

        //绘图初始化
        private void picture_Draw()
        {
            // chart属性
            Series series1 = chart1.Series[0];
            series1.ChartType = SeriesChartType.Spline;
            series1.BorderWidth = 3;
            series1.Color = System.Drawing.Color.Green;  //曲线颜色
            series1.LegendText = "角度值";

            // chart属性
            Series series2 = chart2.Series[0];
            series2.ChartType = SeriesChartType.Spline;
            series2.BorderWidth = 3;
            series2.Color = System.Drawing.Color.DarkCyan;   //曲线颜色
            series2.LegendText = "扭矩值";

            // 设置坐标轴和背景
            chart1.BackColor = System.Drawing.Color.Honeydew;
            ChartArea chartAreaAngle = chart1.ChartAreas[0];
            chartAreaAngle.BackColor = System.Drawing.Color.Honeydew;
            chartAreaAngle.AxisX.Minimum = -System.Double.NaN;
            chartAreaAngle.AxisX.Maximum = System.Double.NaN;
            chartAreaAngle.AxisY.Minimum = -System.Double.NaN;
            chartAreaAngle.AxisY.Maximum = System.Double.NaN;
            chartAreaAngle.AxisX.MajorGrid.Enabled = false;
            chartAreaAngle.AxisY.MajorGrid.Enabled = false;
            //chartAreaAngle.AxisX.LabelStyle.Enabled = false;
            chartAreaAngle.AxisY.LineColor = System.Drawing.Color.Gray;
            chartAreaAngle.AxisX.MajorGrid.LineColor = System.Drawing.Color.Gray;
            chartAreaAngle.AxisY.MajorGrid.LineColor = System.Drawing.Color.Gray;
            chartAreaAngle.AxisX.MajorTickMark.Size = 0;

            chart2.BackColor = System.Drawing.Color.Ivory;
            ChartArea chartAreaTorque = chart2.ChartAreas[0];
            chartAreaTorque.BackColor = System.Drawing.Color.Ivory;
            chartAreaTorque.AxisX.Minimum = 0;
            chartAreaTorque.AxisX.Maximum = System.Double.NaN;
            chartAreaTorque.AxisY.Minimum = 0d;
            chartAreaTorque.AxisY.Maximum = System.Double.NaN;
            chartAreaTorque.AxisX.MajorGrid.Enabled = false;
            chartAreaTorque.AxisY.MajorGrid.Enabled = false;
            //chartAreaTorque.AxisX.LabelStyle.Enabled = false;
            chartAreaTorque.AxisY.LineColor = System.Drawing.Color.Gray;
            chartAreaTorque.AxisX.MajorGrid.LineColor = System.Drawing.Color.Gray;
            chartAreaTorque.AxisY.MajorGrid.LineColor = System.Drawing.Color.Gray;
            chartAreaTorque.AxisX.MajorTickMark.Size = 0;
        }

        //将时间戳转换为时间（秒）
        private string ConvertTimestampToStr(string timestamp)
        {
            DateTime dateTime = MyDevice.GetMilTime(timestamp);
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss:fff");
        }

        //生成报告
        private void bt_Report_Click(object sender, EventArgs e)
        {
            //获取当前运行的exe完整路径
            string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

            //根据不同exe环境来运行不同功能
            if (exePath.IndexOf("AiTorque.exe") != -1)
            {
                MenuExportForm menuExportForm = new MenuExportForm();

                PrepReport(menuExportForm);
            }
            else if (exePath.IndexOf("XHTorque.exe") != -1)
            {
                MessageBox.Show("开发中...");
            }
        }

        //生成报告准备工作
        private void PrepReport(MenuExportForm menuExportForm)
        {
            //打开报告弹窗
            menuExportForm.reportOpsn = click_VIN;
            menuExportForm.ShowDialog();        
            this.BringToFront();

            if (menuExportForm.report)
            {
                this.reportFileName = menuExportForm.reportFileName;
                this.reportCompany = menuExportForm.reportCompany;
                this.reportLoad = menuExportForm.reportLoad;
                this.reportCommodity = menuExportForm.reportCommodity;
                this.reportStandard = menuExportForm.reportStandard;
                this.reportOpsn = menuExportForm.reportOpsn;
                this.reportDate = menuExportForm.reportDate;

                CreatDataPdf();
            }
        }

        //生成pdf
        private void CreatDataPdf()
        {
            int page = 0;//记录页数
            const int LENL = 24;//设置数据占位长度
            int myMax = 55; ;//获取最大长度
            String blankLine = " ";

            List<String> myLS = new List<String>();

            //保存报告路径和文件名
            if (!Directory.Exists(MyDevice.userOut))
            {
                Directory.CreateDirectory(MyDevice.userOut);
            }

            //保存报告路径和文件名
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.FileName = reportLoad + @"\" + reportFileName + ".pdf";
            if (File.Exists(fileDialog.FileName))
            {
                DialogResult dr = (MyDevice.languageType == 0 ?
                    MessageBox.Show("存在同名文件，确定要覆盖它吗？", "提示", MessageBoxButtons.YesNo) :
                    MessageBoxEX.Show("A file with the same name exists. Are you sure you want to overwrite it?", "Hint", MessageBoxButtons.YesNo, new string[] { "Yes", "NO" }));
                if (dr == DialogResult.No)
                {
                    return;
                }
            }
            //创建新文档对象,页边距(X,X,Y,Y)
            Document document = new Document(PageSize.A4, 48, 16, 16, 0);

            PdfWriter writer;
            try
            {
                //路径设置; FileMode.Create文档不在会创建，存在会覆盖
                writer = PdfWriter.GetInstance(document, new FileStream(fileDialog.FileName, FileMode.Create));
            }
            catch
            {
                if (MyDevice.languageType == 0)//语言选择为中文
                {
                    MessageBox.Show("请先关闭该文档", "提示");
                }
                else
                {
                    MessageBox.Show("Please close this document first", "Hint");
                }
                return;
            }

            //添加信息
            document.AddTitle("芜湖艾瑞特机电设备有限公司");
            document.AddSubject(" 扭力测试曲线报告");
            document.AddKeywords("report");

            //创建字体，STSONG.TTF空格不等宽
            iTextSharp.text.Font fontTitle = new iTextSharp.text.Font(BaseFont.CreateFont(@".\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 14.0f, iTextSharp.text.Font.BOLD);
            iTextSharp.text.Font fontItem = new iTextSharp.text.Font(BaseFont.CreateFont(@".\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 10.0f, iTextSharp.text.Font.BOLD);
            iTextSharp.text.Font fontContent = new iTextSharp.text.Font(BaseFont.CreateFont(@".\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 10.0f, iTextSharp.text.Font.NORMAL);
            iTextSharp.text.Font fontMessage = new iTextSharp.text.Font(BaseFont.CreateFont(@".\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 8.0f, iTextSharp.text.Font.NORMAL);
            iTextSharp.text.Font fontJingdu = new iTextSharp.text.Font(BaseFont.CreateFont(@".\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 7.0f, iTextSharp.text.Font.NORMAL);

            //页眉页脚水印
            writer.PageEvent = new IsHandF();

            //将pdf变量设置为只读选项
            writer.SetEncryption(null, null, PdfWriter.AllowPrinting, PdfWriter.STANDARD_ENCRYPTION_128);

            //打开
            document.Open();

            document.Add(CreateParagraph("Page" + (++page), fontMessage, Element.ALIGN_RIGHT));
            //document.Add(new Paragraph(blankLine, fontItem));

            document.Add(CreateParagraph(reportCompany, fontTitle, Element.ALIGN_CENTER));//标题（公司名称）
            document.Add(new Paragraph(blankLine, fontItem));
            document.Add(CreateParagraph("扭力测试曲线报告", fontItem, Element.ALIGN_CENTER));//副标题（报告名称）
            document.Add(CreateParagraph("Torque test curve Report", fontItem, Element.ALIGN_CENTER));//副标题（报告名称）

            for (int j = 0; j < 3; j++)
            {
                document.Add(new Paragraph(blankLine, fontItem));
            }

            //设置峰值、流水号、扭矩单位的数据
            reportOpsn = dataGridView1.Rows[0].Cells[2].Value.ToString().Split(' ')[0];
            //reportTorqueUnit = dataGridView1.Rows[0].Cells[9].Value.ToString().Split(' ')[1];
            reportWorkOrder = dataGridView1.Rows[0].Cells[3].Value.ToString();
            reportScrewID = dataGridView1.Rows[0].Cells[5].Value.ToString();
            reportDeviceID = dataGridView1.Rows[0].Cells[6].Value.ToString();

            double torquevalue;
            double anglevalue;
            double torquepeak = int.MinValue;
            double anglepeak = int.MinValue;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                torquevalue = Convert.ToDouble(row.Cells[9].Value.ToString().Split(' ')[0]);
                anglevalue = Convert.ToDouble(row.Cells[10].Value.ToString().Split(' ')[0]);
                if (torquevalue > torquepeak)
                {
                    torquepeak = torquevalue;
                }
                if (anglevalue > anglepeak)
                {
                    anglepeak = anglevalue;
                }
            }

            reportAnglePeak = anglepeak.ToString();
            reportTorquePeak = torquepeak.ToString();

            myLS.Clear();
            myLS.Add("Base information".PadRight(LENL, ' '));
            myLS.Add("COMMODITY".PadRight(LENL, ' '));
            myLS.Add("STANDRAD".PadRight(LENL, ' '));
            myLS.Add("THE NO".PadRight(LENL, ' '));
            myLS.Add("DATE".PadRight(LENL, ' '));
            myLS[1] = myLS[1] + ":  " + reportCommodity;
            myLS[2] = myLS[2] + ":  " + reportStandard;
            myLS[3] = myLS[3] + ":  " + reportOpsn;
            myLS[4] = myLS[4] + ":  " + reportDate;
            //myMax = mdp.GetJoinLen(myLS, LENL, 24);
            myLS[0] = myLS[0].PadRight(myMax + myLS[0].Length - Encoding.Default.GetBytes(myLS[0]).Length, ' ') + "(基本信息)";
            myLS[1] = myLS[1].PadRight(myMax + myLS[1].Length - Encoding.Default.GetBytes(myLS[1]).Length, ' ') + "(品    名)";
            myLS[2] = myLS[2].PadRight(myMax + myLS[2].Length - Encoding.Default.GetBytes(myLS[2]).Length, ' ') + "(执行标准)";
            myLS[3] = myLS[3].PadRight(myMax + myLS[3].Length - Encoding.Default.GetBytes(myLS[3]).Length, ' ') + "(序 列 号)";
            myLS[4] = myLS[4].PadRight(myMax + myLS[4].Length - Encoding.Default.GetBytes(myLS[4]).Length, ' ') + "(报告日期)";

            document.Add(new Paragraph(myLS[0], fontItem));
            document.Add(new Paragraph(blankLine, fontItem));
            document.Add(new Paragraph(myLS[1], fontContent));
            document.Add(new Paragraph(myLS[2], fontContent));
            document.Add(new Paragraph(myLS[3], fontContent));
            document.Add(new Paragraph(myLS[4], fontContent));

            document.Add(new Paragraph(blankLine, fontItem));
            document.Add(new Paragraph(blankLine, fontItem));
            ////////////////////////////////////////////////////////////////////

            myLS.Clear();
            myLS.Add("General information".PadRight(LENL, ' '));
            myLS.Add("扭矩峰值(MAX)".PadRight(LENL - 4, ' '));
            myLS.Add("角度峰值(MAX)".PadRight(LENL - 4, ' '));
            myLS.Add("扭矩单位(UNIT)".PadRight(LENL - 4, ' '));
            myLS.Add("残余扭矩(RT)".PadRight(LENL - 4, ' '));
            myLS[1] = myLS[1] + ":  " + reportTorquePeak;
            myLS[2] = myLS[2] + ":  " + reportAnglePeak;
            myLS[3] = myLS[3] + ":  " + reportTorqueUnit;
            myLS[4] = myLS[4] + ":  " + reportResidualTorque;
            //myMax = mdp.GetJoinLen(myLS, LENL, 30);
            myLS[0] = myLS[0].PadRight(myMax + myLS[0].Length - Encoding.Default.GetBytes(myLS[0]).Length, ' ') + "(检测信息)";
            myLS[1] = myLS[1].PadRight(myMax + myLS[1].Length - Encoding.Default.GetBytes(myLS[1]).Length, ' ') + "工 单 号(WorkOrder)".PadRight(17, ' ') + ":  " + reportWorkOrder;
            myLS[2] = myLS[2].PadRight(myMax + myLS[2].Length - Encoding.Default.GetBytes(myLS[2]).Length, ' ') + "拧紧序号(ScrewID)".PadRight(16, ' ') + ":  " + reportWorkOrder;
            myLS[3] = myLS[3].PadRight(myMax + myLS[3].Length - Encoding.Default.GetBytes(myLS[3]).Length, ' ') + "设 备 ID(DeviceID)".PadRight(18, ' ') + ":  " + reportDeviceID;

            document.Add(new Paragraph(myLS[0], fontItem));
            document.Add(new Paragraph(blankLine, fontItem));
            document.Add(new Paragraph(myLS[1], fontContent));
            document.Add(new Paragraph(myLS[2], fontContent));
            document.Add(new Paragraph(myLS[3], fontContent));
            document.Add(new Paragraph(myLS[4], fontContent));
            ///////////////////////////////////////////////////////////////////

            //document.Add(new Paragraph(blankLine, fontItem));
            document.Add(new Paragraph(blankLine, fontItem));
            document.Add(CreateParagraph("扭矩角度曲线图", fontItem, Element.ALIGN_CENTER));
            document.Add(CreateParagraph("Torque angle graph", fontItem, Element.ALIGN_CENTER));

            //增加画图
            Bitmap bmp = new Bitmap(splitContainer1.Panel2.Width, splitContainer1.Panel2.Height);
            splitContainer1.Panel2.DrawToBitmap(bmp, splitContainer1.Panel2.ClientRectangle);
            bmp.Save(MyDevice.userDAT + @"\" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpg");

            // 读取图片
            iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(MyDevice.userDAT + @"\" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpg");
            
            // 设置图片位置和大小
            image.SetAbsolutePosition(100, 35);
            image.ScaleToFit(600, 350);

            // 将图片添加到 PDF 文档
            document.Add(image);

            document.Close();
            writer.Close();

            if (System.IO.File.Exists(MyDevice.userDAT + @"\" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpg"))
            {
                System.IO.File.Delete(MyDevice.userDAT + @"\" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpg");
            }

            Process.Start(fileDialog.FileName);
        }

        //创建段落
        public Paragraph CreateParagraph(string str, iTextSharp.text.Font font, int align)
        {
            Paragraph mp = new Paragraph(str, font);

            mp.Alignment = align;
            mp.SpacingBefore = 5.0f;
            mp.SpacingAfter = 5.0f;

            return mp;
        }

        // 页眉页脚水印
        public class IsHandF : PdfPageEventHelper, IPdfPageEvent
        {
            //页事件
            public override void OnEndPage(PdfWriter writer, Document document)
            {
                base.OnEndPage(writer, document);

                //页眉页脚使用字体
                iTextSharp.text.Font fontJingdu = new iTextSharp.text.Font(BaseFont.CreateFont(@".\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 7.0f, iTextSharp.text.Font.NORMAL);

                //页眉 Top - y
                //页脚 Bottom + y
                PdfContentByte myIfo = writer.DirectContent;
                Phrase footer3 = new Phrase("芜湖艾瑞特机电设备有限公司", fontJingdu);
                ColumnText.ShowTextAligned(myIfo, Element.ALIGN_CENTER, footer3, document.Right / 2, document.Bottom + 12, 0);

                #region 水印
                try
                {
                    PdfContentByte myPic = writer.DirectContentUnder;//水印在内容下方添加
                    PdfGState myGS = new PdfGState();
                    iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(MyDevice.userPIC + @"\aitorque.jpg");//水印图片
                    image.RotationDegrees = 30;//旋转角度
                    myGS.FillOpacity = 0.1f;//透明度
                    myPic.SetGState(myGS);

                    float width = document.Right + document.RightMargin; //pdf宽
                    float height = document.Top + document.TopMargin; //pdf高
                    float xnum = 3; //一行3个logo
                    float ynum = 5; //一列5个logo
                    float xspace = (width - (xnum * image.Right)) / xnum; //logo间距
                    float yspace = (height - (ynum * image.Top)) / ynum; //logo间距
                    for (int x = 0; x < xnum; x++)
                    {
                        for (int y = 0; y < ynum; y++)
                        {
                            image.SetAbsolutePosition(0.5f * xspace + x * (xspace + image.Right), 0.5f * yspace + y * (yspace + image.Top));
                            myPic.AddImage(image);
                        }
                    }
                }
                catch
                {

                }
                #endregion
            }
        }

        //鼠标悬停在曲线上显示坐标
        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            if (ucCombox1.SelectedIndex == 0)
            {
                chart1.Series[0].ToolTip = "角度(Y)：#VALY";
            }
            else
            {
                chart1.Series[0].ToolTip = "角度(X)：#VALX\n扭矩(Y)：#VALY";
            }
        }

        //鼠标悬停在曲线上显示坐标
        private void chart2_MouseMove(object sender, MouseEventArgs e)
        {
            chart2.Series[0].ToolTip = "扭矩(Y)：#VALY";
        }

        //曲线缩放
        private void chart_MouseWheel(object sender, MouseEventArgs e)
        {
            Chart chart1 = sender as Chart;
            try
            {
                //鼠标向上滚的Delta值是大于0，向下是小于0
                if (e.Delta > 0)
                {
                    if (chart1.ChartAreas[0].AxisX.ScaleView.Size > 0 && chart1.ChartAreas[0].AxisY.ScaleView.Size > 0)
                    {
                        chart1.ChartAreas[0].AxisX.ScaleView.Size /= 2;//每页显示的点数除以2实现放大效果
                        chart1.ChartAreas[0].AxisY.ScaleView.Size /= 2;
                    }
                    else
                    {
                        chart1.ChartAreas[0].AxisX.ScaleView.Size = chart1.Series[0].Points.Count / 2;//首次滚动时size为NaN
                        chart1.ChartAreas[0].AxisY.ScaleView.Size = chart1.Series[0].Points.Count / 2;
                    }
                }
                else if (e.Delta < 0)
                {
                    if (chart1.ChartAreas[0].AxisX.ScaleView.Size > 0 && chart1.ChartAreas[0].AxisY.ScaleView.Size > 0)
                    {
                        chart1.ChartAreas[0].AxisX.ScaleView.Size *= 2;
                        chart1.ChartAreas[0].AxisY.ScaleView.Size *= 2;
                    }
                    else
                    {
                        chart1.ChartAreas[0].AxisX.ScaleView.Size = chart1.Series[0].Points.Count * 2;
                        chart1.ChartAreas[0].AxisY.ScaleView.Size = chart1.Series[0].Points.Count * 2;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //右键恢复缩放
        private void chart_MouseClick(object sender, MouseEventArgs e)
        {
            Chart chart1 = sender as Chart;
            //右键恢复事件
            if (e.Button == MouseButtons.Right)
            {
                chart1.ChartAreas[0].AxisX.ScaleView.ZoomReset(0);
                chart1.ChartAreas[0].AxisY.ScaleView.ZoomReset(0);
            }
        }

        //鼠标右击选择生成csv文件
        private void dataGridView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && dataGridView1.Rows.Count > 0  && bt_Return.Visible)
            {
                DialogResult result = ucCombox1.Visible ? MessageBox.Show($"是否将数据全部导出?", "确认保存", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                                                        : MessageBox.Show($"是否将作业号汇总表数据导出?", "确认保存", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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
                    DialogSave.FileName = ucCombox1.Visible ? "数据表" + DateTime.Now.ToString("yyyyMMddHHmmss")
                                                            : "作业号汇总表" + DateTime.Now.ToString("yyyyMMddHHmmss");

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

                lines.Add("序号,时间,作业号,工单号,设备编码,设备ID,实时扭矩,实时角度,峰值扭矩,峰值角度,结果,扭矩结果,角度结果");
                DataInfo dataInfoToExcel = new DataInfo();

                if (bt_Return.Visible)
                {
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        dataInfoToExcel = new DataInfo();

                        dataInfoToExcel.id = Convert.ToInt32(dataGridView1.Rows[i].Cells[0].Value.ToString());
                        dataInfoToExcel.time = dataGridView1.Rows[i].Cells[1].Value.ToString();
                        dataInfoToExcel.work_vin = dataGridView1.Rows[i].Cells[2].Value.ToString();
                        dataInfoToExcel.work_order_id = dataGridView1.Rows[i].Cells[3].Value.ToString();
                        dataInfoToExcel.wrench_code = dataGridView1.Rows[i].Cells[5].Value.ToString();
                        dataInfoToExcel.wrench_id = dataGridView1.Rows[i].Cells[6].Value.ToString();
                        dataInfoToExcel.actual_torque = dataGridView1.Rows[i].Cells[7].Value.ToString();
                        dataInfoToExcel.actual_angle = dataGridView1.Rows[i].Cells[8].Value.ToString();
                        dataInfoToExcel.torque_peak = dataGridView1.Rows[i].Cells[9].Value.ToString();
                        dataInfoToExcel.angle_peak = dataGridView1.Rows[i].Cells[10].Value.ToString();
                        dataInfoToExcel.result = dataGridView1.Rows[i].Cells[11].Value.ToString();
                        dataInfoToExcel.torque_result = dataGridView1.Rows[i].Cells[12].Value.ToString();
                        dataInfoToExcel.angle_result = dataGridView1.Rows[i].Cells[13].Value.ToString();

                        //使csv文件用excel打开时能正常显示数据
                        lines.Add($"{dataInfoToExcel.id},{dataInfoToExcel.time},{dataInfoToExcel.work_vin},{dataInfoToExcel.work_order_id}," +
                                  $"{dataInfoToExcel.wrench_code},{dataInfoToExcel.wrench_id}," +
                                  $"{dataInfoToExcel.actual_torque},{dataInfoToExcel.actual_angle},{dataInfoToExcel.torque_peak},{dataInfoToExcel.angle_peak}," +
                                  $"{dataInfoToExcel.result},{dataInfoToExcel.torque_result},{dataInfoToExcel.angle_result},"
                                  );
                    }
                }

                else
                {
                    for (int i = 0; i < dataInfos.Count; i++)
                    {
                        dataInfoToExcel = new DataInfo();

                        dataInfoToExcel.id = dataInfos[i].id;
                        dataInfoToExcel.time = ConvertTimestampToStr(dataInfos[i].time);
                        dataInfoToExcel.work_vin = dataInfos[i].work_vin;
                        dataInfoToExcel.work_order_id = dataInfos[i].work_order_id;
                        dataInfoToExcel.wrench_code = dataInfos[i].wrench_code;
                        dataInfoToExcel.wrench_id = dataInfos[i].wrench_id;
                        dataInfoToExcel.actual_torque = dataInfos[i].actual_torque;
                        dataInfoToExcel.actual_angle = dataInfos[i].actual_angle;
                        dataInfoToExcel.torque_peak = dataInfos[i].torque_peak;
                        dataInfoToExcel.angle_peak = dataInfos[i].angle_peak;
                        dataInfoToExcel.result = dataInfos[i].result;
                        dataInfoToExcel.torque_result = dataInfos[i].torque_result;
                        dataInfoToExcel.angle_result = dataInfos[i].angle_result;

                        //使csv文件用excel打开时能正常显示数据
                        lines.Add($"{dataInfoToExcel.id},{dataInfoToExcel.time},{dataInfoToExcel.work_vin},{dataInfoToExcel.work_order_id}," +
                                  $"{dataInfoToExcel.wrench_code},{dataInfoToExcel.wrench_id}," +
                                  $"{dataInfoToExcel.actual_torque},{dataInfoToExcel.actual_angle},{dataInfoToExcel.torque_peak},{dataInfoToExcel.angle_peak}," +
                                  $"{dataInfoToExcel.result},{dataInfoToExcel.torque_result},{dataInfoToExcel.angle_result},"
                                  );
                    }
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

        
        //鼠标选中单元格右击进行针对性保存数据为csv
        private void dataGridView1_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && dataGridView1.Rows.Count > 0 && !bt_Return.Visible)
            {
                if (e.RowIndex == -1) return;

                this.dataGridView1.Rows[e.RowIndex].Selected = true;  //是否选中当前行

                if (bt_Return.Visible == false)
                {
                    //获取详细数据
                    jdbc.TableName = DatasInfoList[e.RowIndex].name;
                    dataInfos = jdbc.GetListData();
                    if (dataInfos == null) return;
                    DialogResult result = MessageBox.Show($"是否将{DatasInfoList[e.RowIndex].time}当天所有数据导出?", "确认保存", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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
                        DialogSave.FileName = DatasInfoList[dataGridView1.CurrentRow.Index].time + "数据总表" + DateTime.Now.ToString("yyyyMMddHHmmss");

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
                }
            }
        }
    }
}
