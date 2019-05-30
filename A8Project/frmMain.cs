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

namespace A8Project
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }
        ErrorInfo errorInfo = new ErrorInfo();
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
            //DealTestValue();
        }

        #region Socket通讯
        /// <summary>
        /// 接收信息
        /// </summary>
        /// <param name="ip"></param>
        public void OnReceiveMsg(string ip)
        {
            byte[] buffer = _sm._listSocketInfo[ip].msgBuffer;
            string msg = Encoding.UTF8.GetString(buffer).Replace("\0", "");
            if (msg.Length > 0)
            {
                string[] temp = msg.Split(',');
                GetCommunication(temp[0], temp[1], temp[2]);
            }
        }

        /// <summary>
        /// 客户端连接
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

        //文档处理，工控机给一个完成信息然后进行文档处理,一次处理一个文件
        private void DealTestValue()
        {
            try
            {
                string dealPath = ConfigurationManager.AppSettings["dealpath"];
                string[] files = File.ReadAllLines(dealPath);
                string[] testInfo = new string[] { };
                List<TestValue> listValues = new List<TestValue>();
                TestValue testValue1 = new TestValue();
                foreach (string str in files)
                {
                    TestValue testValue = new TestValue();
                    testInfo = str.Split(',');
                    testValue.path = dealPath;
                    testValue.productno = "test1";
                    testValue.testItem = testInfo[0];
                    testValue.testValue = testInfo[1];
                    testValue.testTime = Convert.ToDateTime(testInfo[2]);
                    listValues.Add(testValue);
                }
                testValue1.InsertTestValue(listValues);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 处理存储errorinfo实时信息
        /// </summary>
        /// <param name="site"></param>
        /// <param name="message"></param>
        private void GetCommunication(string createtime, string site, string message)
        {
            ErrorInfo errorInfo = new ErrorInfo();
            errorInfo.createtime = Convert.ToDateTime(createtime);
            errorInfo.site = site;
            errorInfo.message = message;
            errorInfo.InsertErrorInfo(errorInfo);
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
            //加载错误信息
            DataTable dt = new DataTable();
            dt = errorInfo.GetErrorInfo();
            this.gdcErrorInfo.DataSource = dt;
        }

        private void LoadConsumables()
        {
            int consumable1 = buTestValue.GetSiteCount("RunIn", 10)%100000;
            int consumable2 = buTestValue.GetSiteCount("FC", 20) % 100000;
            int consumable3 = (buTestValue.GetSiteCount("RunIn", 5) + buTestValue.GetSiteCount("FC", 5))%50000;

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
            DataTable dt= buTestValue.GetDayOfCountWithFPY();
            this.chartControl4.Series.Clear();
            this.chartControl4.DataSource = dt;
            Series mySeries = new Series("Series1", ViewType.Pie);  // 这是图形类型
            chartControl4.Series.Add(mySeries);
            mySeries.ArgumentDataMember = "name";   // 绑定参数
            mySeries.ValueDataMembers.AddRange(new string[] { "value" });   // 绑定值
            mySeries.Label.PointOptions.PointView = PointView.ArgumentAndValues;   // 设置Label显示方式
            mySeries.ToolTipEnabled = DevExpress.Utils.DefaultBoolean.True;  // 设置鼠标悬浮显示toolTip
            
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