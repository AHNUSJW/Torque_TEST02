using BIL;
using Microsoft.VisualBasic;
using Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

//Lumi 20230809
//Lumi 20231025

namespace Base.UI.MenuTools
{
    public partial class MenuDealWorkOrderForm : Form
    {
        #region 定义参数

        private int index = 0;  //要删除的行

        JDBC jdbc = new JDBC();
        List<WorksInfo> worksInfoList = new List<WorksInfo>();

        #endregion

        public MenuDealWorkOrderForm()
        {
            InitializeComponent();

            //设置窗体的双缓冲
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
            //利用反射设置DataGridView的双缓冲
            Type dgvType = this.dataGridView1.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(this.dataGridView1, true, null);
        }

        private void MenuDealWorkOrderForm_Load(object sender, EventArgs e)
        {
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;    //选行

            refreshTable();   //刷新表
        }

        //新建工单
        private void buttonX2_Click(object sender, EventArgs e)
        {
            String str = Interaction.InputBox("请先设定工单号：", "新建工单", "", -1, -1);
            if (!String.IsNullOrEmpty(str))
            {
                Regex regex = new Regex(@"^[a-zA-Z0-9_]+$");
                if (!regex.IsMatch(str))
                {
                    if (MyDevice.languageType == 0)
                    {
                        MessageBox.Show("工单号只能包含数字、字母、下划线！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Ticket numbers can only contain numbers, letters, underscores！", "prompt", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    return;
                }

                if (str.Length > 20)
                {
                    if (MyDevice.languageType == 0)
                    {
                        MessageBox.Show("工单号长度不能大于20", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("The ticket number length cannot be greater than 20！", "prompt", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    return;
                }

                if (IsValueDuplicateInColumn(dataGridView1, 2, str))
                {
                    if (MyDevice.languageType == 0)
                    {
                        MessageBox.Show("设定的工单号不能与已有工单重复！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("The set ticket number cannot be the same as an existing ticket！", "prompt", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    return;
                }

                //更新数据库
                WorksInfo myWorksInfo = new WorksInfo
                {
                    time = MyDevice.GetTimeStamp(),
                    work_order_id = str,
                    mac = MyDevice.myMac
                };

                try
                {
                    jdbc.AddWorksInfoByWorkOrderId(myWorksInfo);  //更新工单统计表
                    jdbc.TableName = str;
                    jdbc.CreateWorkTable();                       //新建工单表
                }
                catch
                {
                    MessageBox.Show("工单创建失败，请安装数据库");
                    return;
                }

                refreshTable();   //刷新表

                MenuSetWorkOrderForm myMenuSetWorkOrderForm = new MenuSetWorkOrderForm();
                myMenuSetWorkOrderForm.MeWorksInfo = myWorksInfo;
                myMenuSetWorkOrderForm.IsNewWorkInfo = true;

                //跳转到工单编辑界面
                foreach (Form form in this.MdiChildren)
                {
                    if (form.GetType().Name == "MenuSetWorkOrderForm")
                    {
                        form.BringToFront();
                        return;
                    }
                }

                myMenuSetWorkOrderForm.MdiParent = this.MdiParent;
                myMenuSetWorkOrderForm.Show();
                myMenuSetWorkOrderForm.WindowState = FormWindowState.Maximized;
            }

        }

        //左键双击编辑
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            //跳转到工单编辑界面
            MenuSetWorkOrderForm myMenuSetWorkOrderForm = new MenuSetWorkOrderForm();
            WorksInfo selectedWorksInfo = worksInfoList[dataGridView1.CurrentRow.Index];
            myMenuSetWorkOrderForm.MeWorksInfo = selectedWorksInfo;
            myMenuSetWorkOrderForm.IsNewWorkInfo = false;

            foreach (Form form in this.MdiChildren)
            {
                if (form.GetType().Name == "MenuSetWorkOrderForm")
                {
                    form.BringToFront();
                    return;
                }
            }

            myMenuSetWorkOrderForm.MdiParent = this.MdiParent;
            myMenuSetWorkOrderForm.Show();
            myMenuSetWorkOrderForm.WindowState = FormWindowState.Maximized;
        }

        //右键菜单
        private void dataGridView1_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                if (e.RowIndex == -1) return;

                this.dataGridView1.Rows[e.RowIndex].Selected = true;  //是否选中当前行
                index = e.RowIndex;
                this.dataGridView1.CurrentCell = this.dataGridView1.Rows[e.RowIndex].Cells[2];

                //每次选中行都刷新到datagridview中的活动单元格
                this.contextMenuStrip1.Show(this.dataGridView1, e.Location);
                //指定控件（DataGridView），指定位置（鼠标指定位置）
                this.contextMenuStrip1.Show(Cursor.Position);        //锁定右键列表出现的位置
            }
        }

        //删除工单
        private void DelWorkOrderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string delWorkID = this.dataGridView1.CurrentCell.Value.ToString();  //要删除的工单ID

            DialogResult result = MyDevice.languageType == 0 ?
                MessageBox.Show("是否删除工单" + delWorkID + "？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) :
                MessageBox.Show("Whether to delete the ticket" + delWorkID + "？", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
            {
                return;
            }

            if (!this.dataGridView1.Rows[index].IsNewRow)   //判断是否为新行
            {
                try
                {
                    //数据库操作
                    jdbc.DelWorksInfoByWorkOrderId(delWorkID);  //删除工单统计表的记录
                    jdbc.TableName = delWorkID;
                    jdbc.DeleteWorkTable();                     //删除工单表
                }
                catch
                {
                    MessageBox.Show("工单删除失败，请安装数据库");
                    return;
                }

                this.dataGridView1.Rows.RemoveAt(index);    //从集合中移除指定的行
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("删除成功" + delWorkID);
                }
                else
                {
                    MessageBox.Show("The deletion was successful" + delWorkID);
                }
                //必须数据库先删除后再刷新Ui界面的数据，因为CurrentCell会因为UI界面刷新删除数据后发生单元格的变动，
                //跳到下一行数据，从而导致数据库删除失败。
            }
        }

        //编辑备注
        private void editNoteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!this.dataGridView1.Rows[index].IsNewRow)//判断是否为新行
            {
                WorksInfo selectedWorksInfo = worksInfoList[dataGridView1.CurrentRow.Index];

                string inputNote = Interaction.InputBox("请输入备注内容:", "编辑备注", selectedWorksInfo.note);
                Regex meRgx = new Regex(@"[\\/:*""<>\|`]");
                if (meRgx.IsMatch(inputNote))
                {
                    if (MyDevice.languageType == 0)
                    {
                        MessageBox.Show("备注不能包含特殊符号");
                    }
                    else
                    {
                        MessageBox.Show("Notes cannot contain special symbols");
                    }
                    return;
                }
                selectedWorksInfo.note = inputNote;

                jdbc.UpdateWorksInfoNoteByWorkOrderId(selectedWorksInfo.work_order_id, selectedWorksInfo.note);  //数据库操作

                dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells["备注"].Value = inputNote;              //更新dataGridView1
            }
        }

        //复制工单号
        private void 复制工单号ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                // 获取=列名为"工单号"的单元格的值
                string value = this.dataGridView1.CurrentCell.Value.ToString();

                // 将值复制到剪贴板
                Clipboard.SetText(value);
            }
        }

        //条形码
        private void 生成条形码ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string codeWorkID = this.dataGridView1.CurrentCell.Value.ToString();  //要生成条形码的工单ID

            Bitmap barcodeBitmap = GetBarcodeBitmap(codeWorkID, 200, 100);

            string folderPath = AppDomain.CurrentDomain.BaseDirectory + @"\out";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fileName = $"{codeWorkID}_{System.DateTime.Now.ToString("yyyyMMddHHmmss")}_Barcode.png";    //文件名
            string filePath = Path.Combine(folderPath, fileName);                                              //条形码图片保存路径

            barcodeBitmap.Save(filePath, ImageFormat.Png);

            if (File.Exists(filePath))
            {
                Process.Start(folderPath);
            }
        }

        //二维码
        private void 生成二维码ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string codeWorkID = this.dataGridView1.CurrentCell.Value.ToString();  //要生成二维码的工单ID

            Bitmap barcodeBitmap = GetQRCodeBitmap(codeWorkID, 300, 300);

            string folderPath = AppDomain.CurrentDomain.BaseDirectory + @"\out";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fileName = $"{codeWorkID}_{System.DateTime.Now.ToString("yyyyMMddHHmmss")}_QRCode.png";    //文件名
            string filePath = Path.Combine(folderPath, fileName);                                              //条形码图片保存路径

            barcodeBitmap.Save(filePath, ImageFormat.Png);

            if (File.Exists(filePath))
            {
                Process.Start(folderPath);
            }
        }

        // 生成条形码
        private Bitmap GetBarcodeBitmap(string barcodeContent, int barcodeWidth, int barcodeHeight)
        {
            BarcodeWriter barcodeWriter = new BarcodeWriter();
            barcodeWriter.Format = BarcodeFormat.CODE_128;            //设置编码格式
            EncodingOptions encodingOptions = new EncodingOptions();
            encodingOptions.Width = barcodeWidth;                     //设置宽度
            encodingOptions.Height = barcodeHeight;                   //设置长度
            encodingOptions.Margin = 2;                               //设置边距
            barcodeWriter.Options = encodingOptions;
            Bitmap bitmap = barcodeWriter.Write(barcodeContent);
            return bitmap;
        }

        // 生成二维码
        public static Bitmap GetQRCodeBitmap(string qrCodeContent, int qrCodeWidth, int qrCodeHeight)
        {
            BarcodeWriter barcodeWriter = new BarcodeWriter();
            barcodeWriter.Format = BarcodeFormat.QR_CODE;
            QrCodeEncodingOptions qrCodeEncodingOptions = new QrCodeEncodingOptions();
            qrCodeEncodingOptions.DisableECI = true;
            qrCodeEncodingOptions.CharacterSet = "UTF-8";             //设置编码
            qrCodeEncodingOptions.Width = qrCodeWidth;                //设置二维码宽度
            qrCodeEncodingOptions.Height = qrCodeHeight;              //设置二维码高度
            qrCodeEncodingOptions.Margin = 1;                         //设置二维码边距

            barcodeWriter.Options = qrCodeEncodingOptions;
            Bitmap bitmap = barcodeWriter.Write(qrCodeContent);       //写入内容
            return bitmap;
        }

        //刷新表格
        private void refreshTable()
        {
            dataGridView1.Rows.Clear();
            dataGridView1.ClearSelection();

            worksInfoList = jdbc.GetListWork();
            if (worksInfoList == null) return;
            foreach (WorksInfo worksInfo in worksInfoList)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGridView1, worksInfo.id, ConvertTimestampToStr(worksInfo.time), worksInfo.work_order_id, worksInfo.note);
                dataGridView1.Rows.Add(row);
            }
        }

        //将时间戳转换为时间（秒）
        private string ConvertTimestampToStr(string timestamp)
        {
            DateTime dateTime = MyDevice.GetTime(timestamp);
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        //判断是否存在重复值
        private bool IsValueDuplicateInColumn(DataGridView dataGridView, int columnIndex, string value)
        {
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (row.Cells[columnIndex].Value != null && row.Cells[columnIndex].Value.ToString() == value)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
