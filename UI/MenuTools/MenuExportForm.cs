using Model;
using System;
using System.IO;
using System.Windows.Forms;

//Ricardo 20230927

namespace Base.UI.MenuTools
{
    public partial class MenuExportForm : Form
    {
        public string reportFileName;
        public string reportCompany;
        public string reportLoad;
        public string reportCommodity;
        public string reportStandard;
        public string reportOpsn;
        public string reportDate;

        public bool report = false;  //是否点击生成报告

        public String reportInfoPath;//报告信息文件路径

        public MenuExportForm()
        {
            InitializeComponent();
        }

        //窗口加载
        private void MenuExportForm_Load(object sender, EventArgs e)
        {
            reportInfoPath = MyDevice.userCFG + @"\user.ifo";

            //读取历史报告记录快速填写文本内容
            GetReprotInfo();
        }

        //保存报告记录
        private void SaveReprotInfo()
        {
            if (!Directory.Exists(MyDevice.userCFG))
            {
                Directory.CreateDirectory(MyDevice.userCFG);
            }

            if (!File.Exists(reportInfoPath))
            {
                //创建文件（close的目的是为了关闭进程，防止重新打开软件出现进程被占用的问题）
                File.Create(reportInfoPath).Close();
            }

            // 设置文件属性为正常
            File.SetAttributes(reportInfoPath, FileAttributes.Normal);

            FileStream meFS = new FileStream(reportInfoPath, FileMode.Create, FileAccess.Write);
            TextWriter meWrite = new StreamWriter(meFS);
            if (tb_fileName.TextLength > 0)
            {
                meWrite.WriteLine("reportFileName=" + tb_fileName.Text);
            }
            if (tb_company.TextLength > 0)
            {
                meWrite.WriteLine("reportCompany=" + tb_company.Text);
            }
            if (tb_load.TextLength > 0)
            {
                meWrite.WriteLine("reportLoad=" + tb_load.Text);
            }
            if (tb_commodity.TextLength > 0)
            {
                meWrite.WriteLine("reportCommodity=" + tb_commodity.Text);
            }
            if (tb_standard.TextLength > 0)
            {
                meWrite.WriteLine("reportStandard=" + tb_standard.Text);
            }
            meWrite.Close();
            meFS.Close();
            File.SetAttributes(reportInfoPath, FileAttributes.ReadOnly);
        }

        //读取历史报告记录
        private void GetReprotInfo()
        {
            if (File.Exists(reportInfoPath))
            {
                String[] meLines = File.ReadAllLines(reportInfoPath);

                foreach (String line in meLines)
                {
                    switch (line.Substring(0, line.IndexOf('=')))
                    {
                        case "reportFileName": tb_fileName.Text = line.Substring(line.IndexOf('=') + 1); break;
                        case "reportCompany": tb_company.Text = line.Substring(line.IndexOf('=') + 1); break;
                        case "reportLoad": tb_load.Text = line.Substring(line.IndexOf('=') + 1); break;
                        case "reportCommodity": tb_commodity.Text = line.Substring(line.IndexOf('=') + 1); break;
                        case "reportStandard": tb_standard.Text = line.Substring(line.IndexOf('=') + 1); break;
                        case "reportOpsn": tb_opsn.Text = ""; break;
                        default: break;
                    }
                }
            }

            //初始化
            string msg = MyDevice.languageType == 0 ? "扭力测试曲线报告" : "TorqueTestCurveReport";
            tb_fileName.Text = tb_fileName.Text == "" ? msg + DateTime.Now.ToString("yyMMddHHmmssfff") : tb_fileName.Text;
            tb_company.Text = tb_company.Text == "" ? "芜湖艾瑞特机电设备有限公司" : tb_company.Text;
            tb_load.Text = tb_load.Text == "" ? MyDevice.userOut : tb_load.Text;
            tb_commodity.Text = tb_commodity.Text == "" ? "智能扭力扳手" : tb_commodity.Text;
            tb_standard.Text = tb_standard.Text == "" ? "YS//T276-2011" : tb_standard.Text;
            tb_opsn.Text = reportOpsn;
            tb_time.Text = System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        }

        //生成报告
        private void bt_report_Click(object sender, EventArgs e)
        {
            reportFileName = tb_fileName.Text;
            reportCompany = tb_company.Text;
            reportLoad = tb_load.Text;
            reportCommodity = tb_commodity.Text;
            reportStandard = tb_standard.Text;
            reportOpsn = tb_opsn.Text;
            reportDate = tb_time.Text;

            if (reportFileName != "")
            {
                report = true;
            }
            else
            {
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("文件名不能为空，请重新输入", "警告", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else
                {
                    MessageBox.Show("The file name cannot be empty, please re-enter it", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                return;
            }

            //保存报告记录
            SaveReprotInfo();

            this.Hide();
        }

        //设置保存路径
        private void bt_load_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = MyDevice.languageType == 0 ? "请选择文件保存路径" : "Please select a path to save the file.";
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                reportLoad = folderBrowserDialog.SelectedPath;
            }
            tb_load.Text = reportLoad;
        }
    }
}
