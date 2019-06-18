using Common.DAL;
using Common.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using DevExpress.XtraCharts;
using Common.BLL;
using System.IO.Ports;

namespace A8Project
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }
        Productlog productlog = new Productlog();
        BUTestValue buTestValue = new BUTestValue();
        SocketManager _sm = null;
        string ip = string.Empty;
        int port = 102;


        private void frmMain_Load(object sender, EventArgs e)
        {
            #region 读取Socket配置信息
            this.ip = ConfigurationManager.AppSettings["socketSeverIP"];
            this.port = Convert.ToInt32(ConfigurationManager.AppSettings["socketSeverPort"]);
            #endregion
            #region Socket通讯服务
            _sm = new SocketManager(ip, port);
            _sm.OnReceiveMsg += OnReceiveMsg;
            _sm.OnConnected += OnConnected;
            _sm.OnDisConnected += OnDisConnected;
            _sm.Start();
            #endregion

            LoadErrorInfo();
            LoadConsumables();

            LoadCycleTime();
            LoadTodayData();
            LoadDayProductRatio();
            LoadYearMonth();
            LoadYearMonthFPY();

        }

        #region Socket通讯
        /// <summary>
        /// 接收信息
        /// </summary>
        /// <param name="ip"></param>
        public void OnReceiveMsg(string ip)
        {
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            byte[] buffer = _sm._listSocketInfo[ip].msgBuffer;
            string msg = Encoding.UTF8.GetString(buffer).Replace("<STX>", "");
            msg = msg.Replace("<ETX>", "");
            try
            {               
                //当前为处理工控机段socket通讯方法，后期根据具体的PLC通讯协议来变更               
                if (msg.Length > 0)
                {
                    string[] temp = msg.Split(',');
                    //区分出当前接收到的信息到底是哪个阶段：Process_ID、Process_IN、Process_OUT
                    switch (temp[0])
                    {
                        case "Process_ID":
                            _sm.SendMsg("<STX>" + msg + ",pass<ETX>", ip);
                            GetCommunicationLogs("<STX>" + msg + ",pass<ETX>", now);
                            break;
                        case "Process_IN":
                            _sm.SendMsg("<STX>" + msg + ",pass<ETX>", ip);
                            GetCommunicationLogs("<STX>" + msg + ",pass<ETX>", now);
                            GetProductLog(temp[0],temp[1],temp[3],"","",now);    
                            break;
                        case "Process_OUT":
                            _sm.SendMsg("<STX>" + temp[0] + "," + temp[1] + "," + temp[2] + "," + temp[3] + ",pass<ETX>", ip);
                            GetCommunicationLogs("<STX>" + temp[0] + "," + temp[1] + "," + temp[2] + "," + temp[3] + ",pass<ETX>", now);
                            List<string> list = temp.ToList();
                            list.RemoveRange(0,5);
                            string contents=String.Join(",", list.ToArray());
                            GetProductLog(temp[0], temp[1], temp[3], temp[4], contents, now);
                            break;
                        case "START_OUT":
                            _sm.SendMsg("<STX>" + temp[0] + "," + temp[1] + "," + temp[2] + "," + temp[3] + ",pass<ETX>", ip);
                            GetCommunicationLogs("<STX>" + temp[0] + "," + temp[1] + "," + temp[2] + "," + temp[3] + ",pass<ETX>", now);
                            GetProductLog(temp[0], temp[1], temp[3], temp[4], temp[5], now);
                            break;
                        default:
                            break;
                    }                    
                }
            }
            catch
            {
                _sm.SendMsg("<STX>" + msg + ",error<ETX>", ip);
                GetCommunicationLogs("<STX>" + msg + ",error<ETX>", now);
            }
        }


        /// <summary>
        /// 客户端连接，并将协议中的信息返回给客户端，并追加pass/error
        /// </summary>
        /// <param name="clientIP"></param>
        public void OnConnected(string clientIP)
        {
            string ipstr = clientIP.Split(':')[0];
            string portstr = clientIP.Split(':')[1];
        }

        /// <summary>
        /// Socket客户端断开连接保存日志到指定目录下的
        /// </summary>
        /// <param name="clientIp"></param>
        public void OnDisConnected(string clientIp)
        {
            SysLog.CreateLog(clientIp + ",通讯中断");
        }
        #endregion

        private void GetCommunicationLogs(string msg, string rec_time)
        {
            CommunicationLogs communicationLogs = new CommunicationLogs();
            if (msg.Contains(",error"))
                communicationLogs.rec_message = msg.Replace(",error", "");
            else
                communicationLogs.rec_message = msg.Replace(",pass", "");
            communicationLogs.rec_time = rec_time;
            communicationLogs.res_message = msg;
            communicationLogs.InsertCommunicationLog(communicationLogs);
        }

        /// <summary>
        /// 处理存储测试信息
        /// </summary>
        /// <param name="site"></param>
        /// <param name="message"></param>        
        private void GetProductLog(string key_process, string equipment, string productno, string result, string contents, string createtime)
        {
            Productlog productlog = new Productlog();
            productlog.key_process = key_process;
            productlog.equipment = equipment;
            productlog.productno = productno;
            productlog.result = result;
            productlog.contents = contents;
            productlog.createtime = createtime;
            productlog.InsertProductlog(productlog);
        }

        /// <summary>
        /// 定时刷新，时间间隔1小时=3600000ms
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OneHour_timer_Tick(object sender, EventArgs e)
        {
            LoadCycleTime();
            LoadTodayData();
            LoadYearMonth();
            LoadYearMonthFPY();
        }
        /// <summary>   
        /// 加载错误信息，每隔6秒定时刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Six_timer1_Tick(object sender, EventArgs e)
        {
            LoadErrorInfo();
            LoadConsumables();
            LoadDayProductRatio();
        }

        private void LoadErrorInfo()
        {
            //加载信息
            DataTable dt = new DataTable();
            dt = productlog.GetInfo();
            this.gdcErrorInfo.DataSource = dt;
        }

        private void LoadConsumables()
        {
            int consumable1 = buTestValue.GetSiteCount("A8001", 10) % 100000;
            int consumable2 = buTestValue.GetSiteCount("A8002", 20) % 100000;
            int consumable3 = (buTestValue.GetSiteCount("A8001", 5) + buTestValue.GetSiteCount("A8002", 5)) % 50000;

            this.arcScaleComponent1.Value = consumable1;
            this.labelControl4.Text = consumable1.ToString();
            this.arcScaleComponent2.Value = consumable2;
            this.labelControl5.Text = consumable2.ToString();
            this.arcScaleComponent3.Value = consumable3;
            this.labelControl6.Text = consumable3.ToString();
        }

        /// <summary>
        /// 加载各个时间间隔的Cycletime
        /// </summary>
        private void LoadCycleTime()
        {
            DataTable dtHis = new DataTable();
            dtHis = buTestValue.GetCycleTime();
            this.gdcHistory.DataSource = dtHis;
        }

        /// <summary>
        /// 加载当班数据
        /// </summary>
        private void LoadTodayData()
        {
            DataTable dt = buTestValue.GetTodayData(); ;

            this.chartControl1.Series.Clear();
            Series series1 = new Series("产量", ViewType.Bar);
            series1.DataSource = dt;
            series1.ArgumentScaleType = ScaleType.Qualitative;
            series1.LabelsVisibility = DevExpress.Utils.DefaultBoolean.True;

            // 以哪个字段进行显示 
            series1.ArgumentDataMember = "hours";
            series1.ValueScaleType = ScaleType.Numerical;

            // 柱状图里的柱的取值字段
            series1.ValueDataMembers.AddRange(new string[] { "counts" });
            //绑定Series
            chartControl1.Series.Add(series1);
            XYDiagram diagram = (XYDiagram)chartControl1.Diagram;
            diagram.AxisX.GridSpacingAuto = false;
            diagram.AxisX.GridSpacing = 1;
        }

        private void LoadDayProductRatio()
        {
            //DataTable dt = buTestValue.GetDayOfCountWithFPY();
            //this.chartControl4.Series.Clear();
            //this.chartControl4.DataSource = dt;
            //Series mySeries = new Series("Series1", ViewType.Pie);  // 这是图形类型
            //chartControl4.Series.Add(mySeries);
            //mySeries.ArgumentDataMember = "name";   // 绑定参数
            //mySeries.ValueDataMembers.AddRange(new string[] { "value" });   // 绑定值
            //mySeries.Label.PointOptions.PointView = PointView.ArgumentAndValues;   // 设置Label显示方式
            //mySeries.ToolTipEnabled = DevExpress.Utils.DefaultBoolean.True;  // 设置鼠标悬浮显示toolTip

        }

        private void LoadYearMonth()
        {
            DataTable dt = buTestValue.GetYearMonth();
            this.chartControl2.Series.Clear();
            Series series1 = new Series("历史产量", ViewType.Bar);
            series1.DataSource = dt;
            series1.ArgumentScaleType = ScaleType.Qualitative;
            series1.LabelsVisibility = DevExpress.Utils.DefaultBoolean.True;

            // 以哪个字段进行显示 
            series1.ArgumentDataMember = "months";
            series1.ValueScaleType = ScaleType.Numerical;

            // 柱状图里的柱的取值字段
            series1.ValueDataMembers.AddRange(new string[] { "counts" });
            //绑定Series
            chartControl2.Series.Add(series1);
            XYDiagram diagram = (XYDiagram)chartControl2.Diagram;
            diagram.AxisX.GridSpacingAuto = false;
            diagram.AxisX.GridSpacing = 1;
        }

        private void LoadYearMonthFPY()
        {
            DataTable dt = buTestValue.GetYearMonthFPY();
            this.chartControl3.Series.Clear();
            Series series1 = new Series("FPY", ViewType.Line);
            series1.DataSource = dt;
            series1.ArgumentScaleType = ScaleType.Qualitative;

            // 以哪个字段进行显示 
            series1.ArgumentDataMember = "months";
            series1.ValueScaleType = ScaleType.Numerical;
            series1.Label.TextPattern = "{v:0.00%}";
            series1.LabelsVisibility = DevExpress.Utils.DefaultBoolean.True;

            // 柱状图里的柱的取值字段
            series1.ValueDataMembers.AddRange(new string[] { "ratio" });
            //绑定Series
            chartControl3.Series.Add(series1);
            XYDiagram diagram = (XYDiagram)chartControl3.Diagram;
            diagram.AxisX.GridSpacingAuto = false;
            diagram.AxisX.GridSpacing = 1;

            diagram.AxisY.Label.TextPattern = "{v:0.00%}";

        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult dr = MessageBox.Show("确定退出系统？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (dr == DialogResult.OK)
            {
                //需要先释放资源，否则会重复执行该事件
                Dispose();
                Application.Exit();
            }
            else
            {
                e.Cancel = true;
            }
        }
    }
}