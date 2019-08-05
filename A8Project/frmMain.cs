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
        //private HPSocketCS.TcpPackServer server = new HPSocketCS.TcpPackServer();
        private HPSocketCS.TcpServer server = new HPSocketCS.TcpServer();
        private AppState appState = AppState.Stoped;
        List<string> checkedListBoxClientList = new List<string>();
        public Dictionary<string, string> EquipmentInfo = new Dictionary<string, string>()
        {
            { "A8001","WS1"},
            { "A8002","WS2"},
            { "A8003","WS3"},
            { "A8004","WS4"},
            { "A8005","Run-In"},
            { "A8006","AC"},
            { "A8007","FC01"},
            { "A8008","FC02"},
            { "A8009","CC"}
        };

        public Dictionary<IntPtr, bool> checkInfo = new Dictionary<IntPtr, bool>();//检验当前客户端是否已进行握手校验
        public Dictionary<Label, int> ControlStatus = new Dictionary<Label, int>();//图标状态

        private void frmMain_Load(object sender, EventArgs e)
        {
            try
            {
                #region Socket通讯服务
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
                //server.PackHeaderFlag = 0xFF;
                //设置包体长度
                //server.MaxPackSize = 0x1000;
                //启动server
                SocketStart();
                #endregion
                //为label41-label53赋值，默认状态0：常绿
                for (int i = 41; i < 53; i++)
                {
                    ControlStatus.Add(((Label)this.Controls.Find("label" + i.ToString(), true)[0]), 0);
                }

                LoadErrorInfo();
                LoadConsumables();

                LoadCycleTime();
                LoadTodayData();
                LoadYearMonth();
                LoadYearMonthFPY();
            }
            catch(Exception ex)
            {
                SysLog.CreateLog(ex.Message);
            }

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
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string msg = Encoding.GetEncoding("UTF-8").GetString(bytes);
            List<string> list = new List<string>();
            byte[] sendBytes = new byte[] { };
            try
            {
                string sendContent = string.Empty;
                string doneMsg = msg.Replace("<STX>", "").Replace("<ETX>", "");
                list = doneMsg.Split(',').ToList();
                int listCount = list.Count();
                if (listCount > 0)
                {
                    //区分出当前接收到的信息到底是哪个阶段：Process_ID、Process_IN、Process_OUT、START_OUT
                    switch (list[0])
                    {
                        case "Process_ID":
                            //握手
                            //接收信息:msg
                            //返回信息:sendContent
                            //接收时间:now                                                   
                            if (!checkInfo.ContainsKey(connId))
                                checkInfo.Add(connId, true);
                            else
                                checkInfo[connId] = true;

                            list.Add("pass");
                            sendContent = "<STX>" + String.Join(",", list.ToArray()) + "<ETX>";
                            sendBytes = Encoding.GetEncoding("UTF-8").GetBytes(sendContent);
                            server.Send(connId, sendBytes, sendBytes.Length);
                            GetCommunicationLogs(msg, now, sendContent);
                            sendContent = string.Empty;
                            break;
                        case "Process_IN":
                            if (checkInfo[connId])
                            {
                                list.Add("pass");
                                sendContent = "<STX>" + String.Join(",", list.ToArray()) + "<ETX>";
                                GetProductLog(list[0], EquipmentInfo[list[1]], list[3], "", "", now);
                            }
                            else
                            {
                                list.Add("error");
                                sendContent = "<STX>" + String.Join(",", list.ToArray()) + "<ETX>";
                            }
                            sendBytes = Encoding.GetEncoding("UTF-8").GetBytes(sendContent);
                            server.Send(connId, sendBytes, sendBytes.Length);
                            GetCommunicationLogs(msg, now, sendContent);
                            sendContent = string.Empty;
                            break;
                        case "Process_OUT":
                            if (checkInfo[connId])
                            {
                                if (productlog.CheckProcess(EquipmentInfo[list[1]], list[3]))//校验当前产品是否已进站
                                {
                                    List<string> tempList = new List<string>();
                                    foreach (string str in list)
                                    {
                                        tempList.Add(str);
                                    }
                                    tempList.RemoveRange(0, 5);//取得Item项目和校验位
                                    //判断Item项目与数量是否一致，一致则返回pass,否则为error
                                    int count = tempList.Count;
                                    if (count - 1 == Convert.ToInt32(tempList[count - 1]))
                                    {
                                        string contents = String.Join(",", tempList.ToArray());
                                        GetProductLog(list[0], EquipmentInfo[list[1]], list[3], list[4], contents, now);
                                        list.RemoveRange(4, listCount - 4);
                                        list.Add("pass");
                                        sendContent = "<STX>" + String.Join(",", list.ToArray()) + "<ETX>";
                                    }
                                    else
                                    {
                                        list.RemoveRange(4, listCount - 4);
                                        list.Add("error");
                                        sendContent = "<STX>" + String.Join(",", list.ToArray()) + "<ETX>";
                                    }
                                }
                                else
                                {
                                    list.RemoveRange(4, listCount - 4);
                                    list.Add("error");
                                    sendContent = "<STX>" + String.Join(",", list.ToArray()) + "<ETX>";
                                }
                            }
                            else
                            {
                                list.RemoveRange(4, listCount - 4);
                                list.Add("error");
                                sendContent = "<STX>" + String.Join(",", list.ToArray()) + "<ETX>";
                            }
                            sendBytes = Encoding.GetEncoding("UTF-8").GetBytes(sendContent);
                            server.Send(connId, sendBytes, sendBytes.Length);
                            GetCommunicationLogs(msg, now, sendContent);
                            sendContent = string.Empty;
                            break;
                        case "START_OUT":
                            if (checkInfo[connId])
                            {
                                //校验当前产品是否已进站
                                if (productlog.CheckProcess(EquipmentInfo[list[1]], list[3]))
                                {
                                    string filename = "E:\\EquimentTestXML\\" + EquipmentInfo[list[1]]+"\\"+list[5];
                                    string contents = FileOperate.ReadFile(filename);
                                    GetProductLog(list[0], EquipmentInfo[list[1]], list[3], list[4], contents, now);

                                    list.RemoveRange(4, listCount - 4);
                                    list.Add("pass");
                                    sendContent = "<STX>" + String.Join(",", list.ToArray()) + "<ETX>";
                                }
                                else
                                {
                                    list.RemoveRange(4, listCount - 4);
                                    list.Add("error");
                                    sendContent = "<STX>" + String.Join(",", list.ToArray()) + "<ETX>";
                                }

                            }
                            else
                            {
                                list.RemoveRange(4, listCount - 4);
                                list.Add("error");
                                sendContent = "<STX>" + String.Join(",", list.ToArray()) + "<ETX>";
                            }
                            sendBytes = Encoding.GetEncoding("UTF-8").GetBytes(sendContent);
                            server.Send(connId, sendBytes, sendBytes.Length);
                            GetCommunicationLogs(msg, now, sendContent);
                            sendContent = string.Empty;
                            break;
                        case "Call_OUT":
                            string title = list[1];
                            int flag = Convert.ToInt32(list[2]);

                            list.Add("pass");
                            sendContent = "<STX>" + String.Join(",", list.ToArray()) + "<ETX>";
                            GetCommunicationLogs(msg, now, sendContent);

                            #region 呼叫系统
                            if (title.Contains("WS1"))
                            {
                                this.BeginInvoke((MethodInvoker)delegate
                                {
                                    if (flag == 0)
                                    {
                                        label31.Visible = false;
                                    }
                                    else
                                    {
                                        label31.Visible = true;
                                    }
                                    label31.Text = title;
                                    ControlStatus[label41] = flag;
                                });
                            }
                            else if (title.Contains("WS2"))
                            {
                                this.BeginInvoke((MethodInvoker)delegate
                                {
                                    if (flag == 0)
                                    {
                                        label32.Visible = false;
                                    }
                                    else
                                    {
                                        label32.Visible = true;
                                    }
                                    label32.Text = title;
                                    ControlStatus[label42] = flag;
                                });
                            }
                            else if (title.Contains("WS3"))
                            {
                                this.BeginInvoke((MethodInvoker)delegate
                                {
                                    if (flag == 0)
                                    {
                                        label33.Visible = false;
                                    }
                                    else
                                    {
                                        label33.Visible = true;
                                    }
                                    label33.Text = title;
                                    ControlStatus[label43] = flag;
                                });
                            }
                            else if (title.Contains("WS4"))
                            {
                                this.BeginInvoke((MethodInvoker)delegate
                                {
                                    if (flag == 0)
                                    {
                                        label34.Visible = false;
                                    }
                                    else
                                    {
                                        label34.Visible = true;
                                    }
                                    label34.Text = title;
                                    ControlStatus[label44] = flag;
                                });
                            }
                            else if (title.Contains("Run-In"))
                            {
                                this.BeginInvoke((MethodInvoker)delegate
                                {
                                    if (flag == 0)
                                    {
                                        label35.Visible = false;
                                    }
                                    else
                                    {
                                        label35.Visible = true;
                                    }
                                    label35.Text = title;
                                    ControlStatus[label45] = flag;
                                });
                            }
                            else if (title.Contains("AC"))
                            {
                                this.BeginInvoke((MethodInvoker)delegate
                                {
                                    if (flag == 0)
                                    {
                                        label36.Visible = false;
                                    }
                                    else
                                    {
                                        label36.Visible = true;
                                    }
                                    label36.Text = title;
                                    ControlStatus[label46] = flag;
                                });
                            }
                            else if (title.Contains("CC"))
                            {
                                this.BeginInvoke((MethodInvoker)delegate
                                {
                                    if (flag == 0)
                                    {
                                        label37.Visible = false;
                                    }
                                    else
                                    {
                                        label37.Visible = true;
                                    }
                                    label37.Text = title;
                                    ControlStatus[label47] = flag;
                                });
                            }
                            else
                            {
                                this.BeginInvoke((MethodInvoker)delegate
                                {
                                    if (flag == 0)
                                    {
                                        label38.Visible = false;
                                    }
                                    else
                                    {
                                        label38.Visible = true;
                                    }
                                    label38.Text = title;
                                    ControlStatus[label48] = flag;
                                });
                            }
                            break;
                        #endregion
                        default:
                            list.Add("error");
                            sendContent = "<STX>" + String.Join(",", list.ToArray()) + "<ETX>";
                            sendBytes = Encoding.GetEncoding("UTF-8").GetBytes(sendContent);
                            server.Send(connId, sendBytes, sendBytes.Length);
                            GetCommunicationLogs(msg, now, sendContent);
                            sendContent = string.Empty;
                            break;
                    }
                }
            }
            catch
            {
                sendBytes = Encoding.GetEncoding("UTF-8").GetBytes("<STX>" + String.Join(",", list.ToArray()) + ",error<ETX>");
                server.Send(connId, sendBytes, sendBytes.Length);
                GetCommunicationLogs(msg, now, "<STX>" + String.Join(",", list.ToArray()) + ",error<ETX>");
            }
            return HandleResult.Ok;
        }

        /// <summary>
        /// 用于控制灯具显示状态，0：常绿、1：闪红、2：红、3：闪绿
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            foreach (var item in ControlStatus)
            {
                if (item.Value == 0)
                {
                    //绿
                    item.Key.ForeColor = Color.Green;
                }
                else if (item.Value == 1)
                {
                    //闪红
                    if (item.Key.ForeColor == Color.Green || item.Key.ForeColor == this.BackColor)
                        item.Key.ForeColor = Color.Red;
                    else
                        item.Key.ForeColor = this.BackColor;
                }
                else if (item.Value == 2)
                {
                    //红
                    item.Key.ForeColor = Color.Red;
                }
                else if (item.Value == 3)
                {
                    //闪绿
                    if (item.Key.ForeColor == Color.Red || item.Key.ForeColor == this.BackColor)
                        item.Key.ForeColor = Color.Green;
                    else
                        item.Key.ForeColor = this.BackColor;

                }
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

        private void GetCommunicationLogs(string msg, string rec_time, string rebackMsg)
        {
            CommunicationLogs communicationLogs = new CommunicationLogs();
            communicationLogs.rec_message = msg;
            communicationLogs.rec_time = rec_time;
            communicationLogs.res_message = rebackMsg;
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
        /// 定时刷新，时间间隔6s
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Six_timer_Tick(object sender, EventArgs e)
        {
            LoadCycleTime();
            LoadTodayData();
            LoadYearMonth();
            LoadYearMonthFPY();
        }
        /// <summary>   
        /// 加载错误信息，每隔3秒定时刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Three_timer1_Tick(object sender, EventArgs e)
        {
            LoadErrorInfo();
            LoadConsumables();
        }

        private void LoadErrorInfo()
        {
            try
            {
                //加载信息
                DataTable dt = new DataTable();
                dt = buTestValue.DealErrorInfo();
                this.gdcErrorInfo.DataSource = dt;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void LoadConsumables()
        {
            try
            {
                //查询获得AC、CC、FC等站点Process_IN实际过站次数
                float consumable1 = (float)(buTestValue.GetSiteCount("AC") / 1000.0) % 50;
            float consumable2 = (float)(buTestValue.GetSiteCount("CC") / 1000.0) % 50;
            float consumable3 = (float)(buTestValue.GetSiteCount("FC01") / 1000.0) % 50;
            float consumable4 = (float)(buTestValue.GetSiteCount("FC02") / 1000.0) % 50;
            float consumable5 = (float)(buTestValue.GetSiteCount("WS3") / 1000.0) % 50;

            Sys_reset_flag flag = new Sys_reset_flag();
            if (consumable1 == 47.5)
            {
                flag.updateResetFlag("AC", 1);
            }
            if (consumable2 == 47.5)
            {
                flag.updateResetFlag("CC", 1);
                flag.updateResetFlag("CC_Print", 1);
            }
            if (consumable3 == 47.5)
            {
                flag.updateResetFlag("FC01", 1);
            }
            if (consumable4 == 47.5)
            {
                flag.updateResetFlag("FC02", 1);
            }
            if (consumable5 == 47.5)
            {
                flag.updateResetFlag("WS3_Print", 1);
            }

            DataTable dtFlag = flag.getResetFlag();
            bool AC_flag = Convert.ToBoolean(dtFlag.Select("keyname='AC'")[0]["flag"]);
            bool CC_flag = Convert.ToBoolean(dtFlag.Select("keyname='CC'")[0]["flag"]);
            bool FC01_flag = Convert.ToBoolean(dtFlag.Select("keyname='FC01'")[0]["flag"]);
            bool FC02_flag = Convert.ToBoolean(dtFlag.Select("keyname='FC02'")[0]["flag"]);
            bool WS3_Print_flag = Convert.ToBoolean(dtFlag.Select("keyname='WS3_Print'")[0]["flag"]);
            bool CC_Print_flag = Convert.ToBoolean(dtFlag.Select("keyname='CC_Print'")[0]["flag"]);

            //bool开关量用来作重置标识
            //AC
            if (consumable1 > 47.5 && AC_flag)
            {
                ControlStatus[label49] = 1;
                label19.Visible = true;
            }
            else
            {
                ControlStatus[label49] = 0;
                label19.Visible = false;
            }
            //CC
            if (consumable2 > 47.5 && CC_flag)
            {
                ControlStatus[label50] = 1;

                label19.Visible = true;
            }
            else
            {
                ControlStatus[label50] = 0;
                ControlStatus[label53] = 0;
                label19.Visible = false;
            }
            //FC01
            if (consumable3 > 47.5 && FC01_flag)
            {
                ControlStatus[label51] = 1;
                label19.Visible = true;
            }
            else
            {
                ControlStatus[label51] = 0;
                label19.Visible = false;
            }
            //FC02
            if (consumable4 > 47.5 && FC02_flag)
            {
                ControlStatus[label52] = 1;
                label19.Visible = true;
            }
            else
            {
                ControlStatus[label52] = 0;
                label19.Visible = false;
            }
            //CC_Print
            if (consumable2 > 47.5 && CC_Print_flag)
            {
                ControlStatus[label53] = 1;
                label19.Visible = true;
            }
            else
            {
                ControlStatus[label53] = 0;
                label19.Visible = false;
            }
            //WS3_Print
            if (consumable5 > 47.5 && WS3_Print_flag)
            {
                ControlStatus[label54] = 1;
                label19.Visible = true;
            }
            else
            {
                ControlStatus[label54] = 0;
                label19.Visible = false;
            }


            this.arcScaleComponent1.Value = consumable1;
            this.label5.Text = consumable1.ToString() + " K";

            this.arcScaleComponent2.Value = consumable2;
            this.label6.Text = consumable2.ToString() + " K";

            this.arcScaleComponent3.Value = consumable3;
            this.label7.Text = consumable3.ToString() + " K";

            this.arcScaleComponent4.Value = consumable4;
            this.label8.Text = consumable3.ToString() + " K";

            this.arcScaleComponent5.Value = consumable2;
            this.label10.Text = consumable2.ToString() + " K";

            this.arcScaleComponent6.Value = consumable5;
            this.label12.Text = consumable5.ToString() + " K";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 加载各个时间间隔的Cycletime
        /// </summary>
        private void LoadCycleTime()
        {
            try
            {
                DataTable dtHis = new DataTable();
                dtHis = buTestValue.GetCycleTime();
                this.gdcHistory.DataSource = dtHis;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 加载当班数据
        /// </summary>
        private void LoadTodayData()
        {
            try
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
                //Series series1 = new Series("Output", ViewType.Bar);
                //SeriesPoint point = null;
                //foreach (DataRow row in dt.Rows)
                //{
                //    if (row["hours"] != null)
                //    {
                //        point = new SeriesPoint(row["hours"].ToString());
                //        double[] vals = { Convert.ToDouble(row["counts"]) };
                //        point.Values = vals;
                //        double counts = Convert.ToDouble(row["counts"]);
                //        double target = Convert.ToDouble(ConfigurationManager.AppSettings["OneHourProductionTarget"]);

                //        if (counts >= target)
                //            point.Color = Color.Green;
                //        else if ((target - 0.1 * target) <= counts && counts < target)
                //            point.Color = Color.Yellow;
                //        else
                //            point.Color = Color.Red;
                //        series1.Points.Add(point);
                //    }
                //}
                //this.chartControl1.Series.Add(series1);
                #endregion
                Series series1 = new Series("Output", ViewType.Point);
                series1.DataSource = dt;
                series1.ArgumentScaleType = ScaleType.Qualitative;
                // 以哪个字段进行显示 
                series1.ArgumentDataMember = "hours";
                series1.ArgumentScaleType = ScaleType.DateTime;
                series1.ValueScaleType = ScaleType.Numerical;
                // 柱状图里的柱的取值字段
                series1.ValueDataMembers.AddRange(new string[] { "counts" });
                // 点系列视图属性设置
                PointSeriesView myView1 = (PointSeriesView)series1.View;
                myView1.PointMarkerOptions.Size = 4;//标记大小
                //绑定Series
                chartControl1.Series.Add(series1);


                


                Series series2 = new Series("Target", ViewType.Line);
                series2.DataSource = dt2;
                series2.ArgumentScaleType = ScaleType.Qualitative;
                // 以哪个字段进行显示 
                series2.ArgumentDataMember = "hours";
                series2.ArgumentScaleType = ScaleType.DateTime;
                series2.ValueScaleType = ScaleType.Numerical;
                // 柱状图里的柱的取值字段
                series2.ValueDataMembers.AddRange(new string[] { "counts" });
                PointSeriesView myView2 = (PointSeriesView)series2.View;
                myView2.PointMarkerOptions.Size = 4;//标记大小
                //绑定Series
                chartControl1.Series.Add(series2);

                XYDiagram diagram = (XYDiagram)chartControl1.Diagram;
                diagram.AxisX.QualitativeScaleOptions.AutoGrid = false;
                diagram.AxisX.DateTimeScaleOptions.ScaleMode = ScaleMode.Manual;//x轴是扫描轴，时间类型
                diagram.AxisX.DateTimeScaleOptions.MeasureUnit = DateTimeMeasureUnit.Minute;//测量单位是秒这样才能显示到秒
                diagram.AxisX.DateTimeScaleOptions.GridAlignment = DateTimeGridAlignment.Hour;
                diagram.AxisX.DateTimeScaleOptions.GridSpacing = 1;
                diagram.AxisX.Label.Angle = 30;
                diagram.AxisX.WholeRange.AutoSideMargins = false;
                diagram.AxisX.WholeRange.SideMarginsValue = 0;

                diagram.AxisY.Label.Font = new Font("Arial", 9F);
                chartControl1.Legend.Visibility = DevExpress.Utils.DefaultBoolean.False;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void LoadYearMonth()
        {
            try
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
                Series series1 = new Series("HistoryOutput", ViewType.Bar);
                SeriesPoint point = null;
                foreach (DataRow row in dt.Rows)
                {
                    if (row["months"] != null)
                    {
                        point = new SeriesPoint(row["months"].ToString());
                        double[] vals = { Convert.ToDouble(row["counts"]) };
                        point.Values = vals;

                        double counts = Convert.ToDouble(row["counts"]);
                        double target = Convert.ToDouble(ConfigurationManager.AppSettings["YearMonthTarget"]);

                        if (counts >= target)
                            point.Color = Color.Green;
                        else if ((target - 0.1 * target) <= counts && counts < target)
                            point.Color = Color.Yellow;
                        else
                            point.Color = Color.Red;
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
                diagram.AxisX.QualitativeScaleOptions.AutoGrid = false;
                diagram.AxisX.DateTimeScaleOptions.GridSpacing = 1;
                diagram.AxisX.Label.Angle = 20;
                diagram.AxisX.Label.Font = new Font("Arial", 9F);
                diagram.AxisY.Label.Font = new Font("Arial", 9F);                
                chartControl2.Legend.Visibility = DevExpress.Utils.DefaultBoolean.False;
            }catch(Exception ex)
            {
                throw ex;
            }
        }

        private void LoadYearMonthFPY()
        {
            try
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
                diagram.AxisX.QualitativeScaleOptions.AutoGrid = false;
                diagram.AxisX.DateTimeScaleOptions.GridSpacing = 1;
                diagram.AxisX.Label.Angle = 20;
                diagram.AxisY.Label.TextPattern = "{v:0.00%}";
                diagram.AxisY.Label.Font = new Font("Arial", 9F);
                diagram.AxisX.Label.Font = new Font("Arial", 9F);
                //设置Y轴区间范围0%-100%
                diagram.AxisY.WholeRange.Auto = false;
                diagram.AxisY.WholeRange.AutoSideMargins = false;
                diagram.AxisY.WholeRange.SideMarginsValue = 0;
                diagram.AxisY.WholeRange.SetMinMaxValues(0, 1);
                chartControl3.Legend.Visibility = DevExpress.Utils.DefaultBoolean.False;
            }
            catch(Exception ex)
            {
                throw ex;
            }
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
            appState = AppState.Stopping;
            server.Stop();
            Application.Exit();
        }

        private void gdvHistory_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName == "second")
            {
                int Value = e.CellValue.ToString() == "null" ? 0 : Convert.ToInt32(e.CellValue);
                double target = double.Parse(ConfigurationManager.AppSettings["CycleTimeTarget"]);
                double ratio = (Value / target) - 1;

                if (Value <= target)
                {
                    e.Appearance.BackColor = Color.Green;
                }
                else if (Value > target && Value <= target + target * 0.1)
                {
                    e.Appearance.BackColor = Color.Yellow;
                }
                else
                {
                    e.Appearance.BackColor = Color.Red;
                }
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            try
            {
                if (progressBarControl1.Position > 0)
                {
                    progressBarControl1.Position += -1;
                    labelControl2.Text = progressBarControl1.Position.ToString() + "S";
                    if (progressBarControl1.Position == 0)
                    {
                        LoadCycleTime();
                        LoadTodayData();
                        LoadYearMonth();
                        LoadYearMonthFPY();
                        LoadErrorInfo();
                        LoadConsumables();

                        progressBarControl1.Position = 60;
                        labelControl2.Text = progressBarControl1.Position.ToString() + "S";
                    }
                }
            }
            catch (Exception ex)
            {
                SysLog.CreateLog(ex.Message);
            }
        }
    }
}