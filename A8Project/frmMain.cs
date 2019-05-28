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
        SocketManager _sm = null;
        string ip = "192.168.0.105";
        int port = 102;

        /// <summary>
        /// 加载错误信息，每隔6秒定时刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void err_timer1_Tick(object sender, EventArgs e)
        {
            //在家ErrorInfo信息
            DataTable dt = new DataTable();
            dt = errorInfo.GetErrorInfo();
            this.gdcErrorInfo.DataSource = dt;
        }

        /// <summary>
        /// 加载各个时间间隔的Cycletime
        /// </summary>
        private void LoadHistory()
        {
            DataTable dtHis = new DataTable();
            BUTestValue bUTestValue = new BUTestValue();
            dtHis = bUTestValue.GetCycleTime();
            this.gdcHistory.DataSource = dtHis;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            #region 读取Socket配置信息
            this.ip = ConfigurationManager.AppSettings["socketSeverIP"];
            this.port= Convert.ToInt32(ConfigurationManager.AppSettings["socketSeverPort"]);
            #endregion
            #region Socket通讯服务
            _sm = new SocketManager(ip, port);
            _sm.OnReceiveMsg += OnReceiveMsg;
            _sm.OnConnected += OnConnected;
            _sm.OnDisConnected += OnDisConnected;
            _sm.Start();
            #endregion

            LoadHistory();

            LoadTodayData();

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
                GetCommunication(temp[0], temp[1],temp[2]);
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
                string dealPath= ConfigurationManager.AppSettings["dealpath"];
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
        private void GetCommunication(string createtime,string site, string message)
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
        private void Today_timer_Tick(object sender, EventArgs e)
        {
            LoadTodayData();
        }

        /// <summary>
        /// 加载当班数据
        /// </summary>
        private void LoadTodayData()
        {
            TestValue testValue = new TestValue();
            DataTable dtData = testValue.GetTodayData();
            DataTable dt = FillTable(dtData);

            this.chartControl1.Series.Clear();
            Series series1 = new Series("产量", ViewType.Bar);
            series1.DataSource = dt;
            series1.ArgumentScaleType = ScaleType.Qualitative;

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

        /// <summary>
        /// 填充当班数据到0-24小时的表中
        /// </summary>
        /// <param name="dtData"></param>
        /// <returns></returns>
        private DataTable FillTable(DataTable dtData)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("hours",Type.GetType("System.Int32"));
            dt.Columns.Add("counts", Type.GetType("System.Int32"));
            for (int i = 1; i <= 24; i++)
            {
                DataRow dr = dt.NewRow();
                dr["hours"] = i;
                DataRow[] dataRows = dtData.Select("hours=" + i + "");
                if (dataRows.Length > 0)
                {
                    dr["counts"] = dataRows[0]["counts"].ToString();
                }
                else
                {
                    dr["counts"] = 0;
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }

        
    }
}