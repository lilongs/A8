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
using HPSocketCS;
using static DevExpress.Utils.Drawing.Helpers.NativeMethods;

namespace A8Project
{
    public enum AppState
    {
        Starting, Started, Stopping, Stoped, Error
    }

    public partial class frmMain : Form
    {

        public frmMain()
        {
            InitializeComponent();
        }
        Productlog productlog = new Productlog();
        BUTestValue buTestValue = new BUTestValue();
        string ip = string.Empty;
        ushort port = 102;
        private HPSocketCS.TcpPackServer server = new HPSocketCS.TcpPackServer();
        private AppState appState = AppState.Stoped;
        List<string> checkedListBoxClientList = new List<string>();
        public Dictionary<string, string> EquipmentInfo = new Dictionary<string, string>()
        {
            { "A8001","WS1"},
            { "A8002","WS2"},
            { "A8003","WS3"},
            { "A8004","WS4"},
            { "A8005","Run-In"},
            { "A8006","AC01"},
            { "A8007","AC02"},
            { "A8008","FC01"},
            { "A8009","FC02"},
            { "A8010","CC"}
        };


        private void frmMain_Load(object sender, EventArgs e)
        {
            #region Socket通讯服务
            //_sm = new SocketManager(ip, port);
            //_sm.OnReceiveMsg += OnReceiveMsg;
            //_sm.OnConnected += OnConnected;
            //_sm.OnDisConnected += OnDisConnected;
            //_sm.Start();

            //绑定监听地址前触发
            server.OnPrepareListen += new TcpServerEvent.OnPrepareListenEventHandler(server_OnPrepareListen);
            //客户端连接请求被接受后触发
            server.OnAccept += new TcpServerEvent.OnAcceptEventHandler(server_OnAccept);
            //发送消息后触发
            server.OnSend += new TcpServerEvent.OnSendEventHandler(server_OnSend);
            //收到消息后触发
            server.OnReceive += new TcpServerEvent.OnReceiveEventHandler(server_OnReceive);
            //连接关闭后触发（服务端的连接通常是多个，只要某一个连接关闭了都会触发）
            server.OnClose += new TcpServerEvent.OnCloseEventHandler(server_OnClose);
            //组件停止后触发
            server.OnShutdown += new TcpServerEvent.OnShutdownEventHandler(server_OnShutdown);
            server.PackHeaderFlag = 0xff;
            //设置包体长度
            server.MaxPackSize = 0x1000;
            //启动server
            SocketStart();
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

        #region 事件处理方法
        private void SocketStart()
        {
            this.ip = ConfigurationManager.AppSettings["socketSeverIP"];
            this.port = ushort.Parse(ConfigurationManager.AppSettings["socketSeverPort"]);
            server.IpAddress = ip;
            server.Port = port;
            if (server.Start())
            {
                appState = AppState.Started;
                SysLog.CreateLog("服务端启动");
            }
            else
            {
                appState = AppState.Stoped;
                SysLog.CreateLog(string.Format("服务端启动失败：{0}，{1}", server.ErrorMessage, server.ErrorCode));
            }
        }
        private HandleResult server_OnPrepareListen(IntPtr soListen)
        {
            SysLog.CreateLog("开始监听");
            return HandleResult.Ok;
        }

        private HandleResult server_OnAccept(IntPtr connId, IntPtr pClient)
        {
            SysLog.CreateLog(string.Format("接受客户端连接请求，连接ID：{0}", connId));
            string strConnID = connId.ToString();
            if (checkedListBoxClientList.Contains(strConnID) == false)
            {
                CheckedListBoxOperation(connId.ToString(), "add");
            }

            return HandleResult.Ok;
        }

        private HandleResult server_OnSend(IntPtr connId, byte[] bytes)
        {
            return HandleResult.Ok;
        }

        private HandleResult server_OnReceive(IntPtr connId, byte[] bytes)
        {
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string msg = Encoding.GetEncoding("UTF-8").GetString(bytes).Replace("<STX>", "");
            msg = msg.Replace("<ETX>", "");
            string sendContent = string.Empty;
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
                            sendContent = "<STX>" + msg + ",pass<ETX>";
                            break;
                        case "Process_IN":
                            sendContent = "<STX>" + msg + ",pass<ETX>";
                            GetProductLog(temp[0], EquipmentInfo[temp[1]], temp[3], "", "", now);
                            break;
                        case "Process_OUT":
                            sendContent = "<STX>" + temp[0] + "," + temp[1] + "," + temp[2] + "," + temp[3] + ",pass<ETX>";
                            List<string> list = temp.ToList();
                            list.RemoveRange(0, 5);
                            string contents = String.Join(",", list.ToArray());
                            GetProductLog(temp[0], EquipmentInfo[temp[1]], temp[3], temp[4], contents, now);
                            break;
                        case "START_OUT":
                            sendContent = "<STX>" + temp[0] + "," + temp[1] + "," + temp[2] + "," + temp[3] + ",pass<ETX>";
                            GetProductLog(temp[0], EquipmentInfo[temp[1]], temp[3], temp[4], temp[5], now);
                            break;
                        case "Call_OUT":
                            string title = temp[1];
                            GetCommunicationLogs("<STX>" + temp[0] + "," + temp[1] + ",pass<ETX>", now);
                            Thread th;//添加线程 
                            
                            if (title.Contains("物料呼叫"))
                            {
                                this.BeginInvoke((MethodInvoker)delegate
                                {
                                    label4.Text = title;
                                });
                                th = new Thread(run2);
                                th.IsBackground = true;
                                th.Start();
                            }
                            else
                            {
                                this.BeginInvoke((MethodInvoker)delegate
                                {
                                    label3.Text = title;
                                });
                                th = new Thread(run1);
                                th.IsBackground = true;
                                th.Start();
                            }
                            break;
                        default:
                            break;
                    }
                    if (sendContent.Length > 0)
                    { 
                        byte[] sendBytes = Encoding.GetEncoding("UTF-8").GetBytes(sendContent);
                        server.Send(connId, sendBytes, sendBytes.Length);
                        GetCommunicationLogs(sendContent, now);
                    }
                }
            }
            catch
            {
                byte[] sendBytes = Encoding.GetEncoding("UTF-8").GetBytes("<STX>" + msg + ",error<ETX>");
                server.Send(connId, sendBytes, sendBytes.Length);
                GetCommunicationLogs("<STX>" + msg + ",error<ETX>", now);
            }
            return HandleResult.Ok;
        }

        private void run1()
        {
            int n = 10;
            while (n > 0)
            {
                if (n % 2 == 0)
                {
                    this.BeginInvoke((MethodInvoker)delegate
                    {
                        label1.ForeColor = Color.Red;
                    });                    
                }
                else
                {
                    this.BeginInvoke((MethodInvoker)delegate
                    {
                        label1.ForeColor = this.BackColor;
                    });                    
                    if (n == 1)
                    {
                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            label1.ForeColor = Color.Green;
                        });                        
                    }
                }
                n--;
                Thread.Sleep(500);
            }
        }

        private void run2()
        {
            int n = 10;
            while (n > 0)
            {
                if (n % 2 == 0)
                {
                    this.BeginInvoke((MethodInvoker)delegate
                    {
                        label2.ForeColor = Color.Red;
                    });
                }
                else
                {
                    this.BeginInvoke((MethodInvoker)delegate
                    {
                        label2.ForeColor = this.BackColor;
                    });                    
                    if (n == 1)
                    {
                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            label2.ForeColor = Color.Green;
                        });                        
                    }
                }
                n--;
                Thread.Sleep(500);
            }
        }

        //当触发了OnClose事件时，表示连接已经被关闭，并且OnClose事件只会被触发一次
        //通过errorCode参数判断是正常关闭还是异常关闭，0表示正常关闭
        private HandleResult server_OnClose(IntPtr connId, SocketOperation enOperation, int errorCode)
        {
            try
            {
                CheckedListBoxOperation(connId.ToString(), "remove");
                if (errorCode == 0)
                {
                    SysLog.CreateLog(string.Format("连接已断开，连接ID：{0}", connId));
                }
                else
                {
                    SysLog.CreateLog(string.Format("客户端连接发生异常，已经断开连接，连接ID：{0}，错误代码：{1}", connId, errorCode));
                }

                return HandleResult.Ok;
            }
            catch (Exception ex)
            {
                SysLog.CreateLog(ex.Message);
                return HandleResult.Error;
            }
        }

        private HandleResult server_OnShutdown()
        {
            appState = AppState.Stoped;
            SysLog.CreateLog("服务端已经停止服务");
            return HandleResult.Ok;
        }
        private void CheckedListBoxOperation(string connId, string operationType)
        {

            switch (operationType)
            {
                case "add":
                    {
                        checkedListBoxClientList.Add(connId);
                    }
                    break;

                case "remove":
                    {
                        checkedListBoxClientList.Remove(connId);
                    }
                    break;
            }
        }

        #endregion 事件处理方法
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
            //查询获得AC、CC、FC等站点Process_IN实际过站次数
            int consumable1 = buTestValue.GetSiteCount("AC") ;
            int consumable2 = buTestValue.GetSiteCount("FC") ;
            int consumable3 = buTestValue.GetSiteCount("CC") ;

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
            DataTable dt = buTestValue.GetTodayData();
            DataTable dt2 = buTestValue.GetTodayTarget();

            this.chartControl1.Series.Clear();
            #region 方式一，传统的数据绑定，无需精确控制每一个Bar
            //Series series1 = new Series("产量", ViewType.Bar);
            //series1.DataSource = dt;
            //series1.ArgumentScaleType = ScaleType.Qualitative;
            //series1.LabelsVisibility = DevExpress.Utils.DefaultBoolean.True;

            //// 以哪个字段进行显示 
            //series1.ArgumentDataMember = "hours";
            //series1.ValueScaleType = ScaleType.Numerical;

            //// 柱状图里的柱的取值字段
            //series1.ValueDataMembers.AddRange(new string[] { "counts" });

            ////BarSeriesView barSeriesView = (BarSeriesView)series1.View;
            ////barSeriesView.Color = Color.Red;
            //chartControl1.Series.Add(series1);
            #endregion

            #region 方式二，使用Points的方式进行数据赋值，适用于控制每个柱子的颜色
            Series series1 = new Series("产量", ViewType.Bar);
            SeriesPoint point = null;
            foreach (DataRow row in dt.Rows)
            {
                if (row["hours"] != null)
                {
                    point = new SeriesPoint(row["hours"].ToString());
                    double[] vals = { Convert.ToDouble(row["counts"]) };
                    point.Values = vals;
                    if (Convert.ToDouble(row["counts"]) < Convert.ToDouble(ConfigurationManager.AppSettings["OneHourProductionTarget"]))
                        point.Color = Color.Red;
                    else
                        point.Color = Color.Blue;
                    series1.Points.Add(point);
                }
            }
            this.chartControl1.Series.Add(series1);
            #endregion

            Series series2 = new Series("Target", ViewType.Line);
            series2.DataSource = dt2;
            series2.ArgumentScaleType = ScaleType.Qualitative;

            // 以哪个字段进行显示 
            series2.ArgumentDataMember = "hours";
            series2.ValueScaleType = ScaleType.Numerical;
            //series2.LabelsVisibility = DevExpress.Utils.DefaultBoolean.True;//标签

            // 柱状图里的柱的取值字段
            series2.ValueDataMembers.AddRange(new string[] { "counts" });
            //绑定Series
            chartControl1.Series.Add(series2);

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
            DataTable dt2 = buTestValue.GetYearMonthTarget();

            this.chartControl2.Series.Clear();
            #region 方式一，传统的数据绑定，无需精确控制每一个Bar
            //Series series1 = new Series("历史产量", ViewType.Bar);
            //series1.DataSource = dt;
            //series1.ArgumentScaleType = ScaleType.Qualitative;
            //series1.LabelsVisibility = DevExpress.Utils.DefaultBoolean.True;

            //// 以哪个字段进行显示 
            //series1.ArgumentDataMember = "months";
            //series1.ValueScaleType = ScaleType.Numerical;

            //// 柱状图里的柱的取值字段
            //series1.ValueDataMembers.AddRange(new string[] { "counts" });
            ////绑定Series
            //chartControl2.Series.Add(series1);
            #endregion

            #region 方式二，使用Points的方式进行数据赋值，适用于控制每个柱子的颜色
            Series series1 = new Series("历史产量", ViewType.Bar);
            SeriesPoint point = null;
            foreach (DataRow row in dt.Rows)
            {
                if (row["months"] != null)
                {
                    point = new SeriesPoint(row["months"].ToString());
                    double[] vals = { Convert.ToDouble(row["counts"]) };
                    point.Values = vals;
                    if (Convert.ToDouble(row["counts"]) < Convert.ToDouble(ConfigurationManager.AppSettings["YearMonthTarget"]))
                        point.Color = Color.Red;
                    else
                        point.Color = Color.Blue;
                    series1.Points.Add(point);
                }
            }
            this.chartControl2.Series.Add(series1);
            #endregion

            Series series2 = new Series("Target", ViewType.Line);
            series2.DataSource = dt2;
            series2.ArgumentScaleType = ScaleType.Qualitative;

            // 以哪个字段进行显示 
            series2.ArgumentDataMember = "months";
            series2.ValueScaleType = ScaleType.Numerical;

            // 柱状图里的柱的取值字段
            series2.ValueDataMembers.AddRange(new string[] { "counts" });
            //绑定Series
            this.chartControl2.Series.Add(series2);

            XYDiagram diagram = (XYDiagram)chartControl2.Diagram;
            diagram.AxisX.GridSpacingAuto = false;
            diagram.AxisX.GridSpacing = 1;
        }

        private void LoadYearMonthFPY()
        {
            DataTable dt = buTestValue.GetYearMonthFPY();
            DataTable dt2 = buTestValue.GetYearMonthFPYTarget();
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

            Series series2 = new Series("Target", ViewType.Line);
            series2.DataSource = dt2;
            series2.ArgumentScaleType = ScaleType.Qualitative;

            // 以哪个字段进行显示 
            series2.ArgumentDataMember = "months";
            series2.ValueScaleType = ScaleType.Numerical;

            // 柱状图里的柱的取值字段
            series2.ValueDataMembers.AddRange(new string[] { "ratio" });
            //绑定Series
            this.chartControl3.Series.Add(series2);
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
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}