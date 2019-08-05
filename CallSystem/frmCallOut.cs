﻿using Common.Util;
using DevExpress.XtraBars;
using HPSocketCS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CallSystem
{
    public enum AppState
    {
        Starting, Started, Stopping, Stoped, Error
    }
    public partial class frmCallOut : Form
    {
        private AppState appState = AppState.Stoped;

        //PACK模型，应用程序不必处理分包与数据抓取，HP-Socket组件保证每个OnReceive事件都向应用程序提供一个完整的数据包
        //PACK模型组件会对应用程序发送的每个数据包自动加上4字节（32位）的包头，组件接收到数据时根据包头信息自动分包，每个完整数据包通过OnReceive事件发送给应用程序
        //private HPSocketCS.TcpPackClient client = new HPSocketCS.TcpPackClient();
        private HPSocketCS.TcpClient client = new HPSocketCS.TcpClient();
        public frmCallOut()
        {
            InitializeComponent();
        }

        private void frmCallOut_Load(object sender, EventArgs e)
        {
            appState = AppState.Stoped;
            barStaticItem2.Caption = this.Tag.ToString();
            //绑定事件
            //开始连接前触发
            client.OnPrepareConnect += new TcpClientEvent.OnPrepareConnectEventHandler(client_OnPrepareConnect);
            //连接成功后触发
            client.OnConnect += new TcpClientEvent.OnConnectEventHandler(client_OnConnect);
            //发送消息后触发
            client.OnSend += new TcpClientEvent.OnSendEventHandler(client_OnSend);
            //收到消息后触发
            client.OnReceive += new TcpClientEvent.OnReceiveEventHandler(client_OnReceive);
            //连接关闭后触发
            client.OnClose += new TcpClientEvent.OnCloseEventHandler(client_OnClose);

            //PACK模型包头格式
            //XXXXXXXXXXXXX YYYYYYYYYYYYYYYYYYY
            //前13位为包头标识，用于数据包校验，取值范围为0-8191（ox1FFF）,当包头标识为0时不校验包头
            //后19位为长度，记录包体长度。有效数据包最大长度不能超过524287（ox7FFFF）字节，默认长度限制为262144（ox40000）字节
            //设置包头标识，客户端与服务端的包头标识一致才能通信
            //client.PackHeaderFlag = 0xFF;
            //client.PackHeaderFlag = 0;
            //设置包体长度
            //client.MaxPackSize = 0x1000;
            ConnectServer();

            foreach (Control control in this.Controls)
            {
                if (control.GetType().Name == "Button")
                {
                    ControlStatus.Add((Button)control,0);
                }
            }
        }

        #region Socket连接处理方法
        private void ConnectServer()
        {
            try
            {
                string ip = ConfigurationManager.AppSettings["socketSeverIP"];
                ushort port = ushort.Parse(ConfigurationManager.AppSettings["socketSeverPort"]);
                appState = AppState.Starting;

                //Client组件发起连接的过程可以是同步或异步的
                //同步是指组件的连接方法等到连接成功或失败了再返回（返回true或false）
                //异步是指组件的连接方法会立即返回，如果返回值为false则表示连接失败，如果连接成功则稍后会触发OnConnect事件
                if (client.Connect(ip, port, true))
                {
                    appState = AppState.Started;
                    this.BeginInvoke((MethodInvoker)delegate
                    {
                        barStaticItem4.Caption = "服务器已连接";
                    });
                }
                else
                {
                    appState = AppState.Stoped;
                    SysLog.CreateLog(string.Format("无法建立连接：{0}，{1}", client.ErrorMessage, client.ErrorCode));
                    this.BeginInvoke((MethodInvoker)delegate
                    {
                        barStaticItem4.Caption = "服务器连接已关闭";
                    });
                }
            }
            catch (Exception exc)
            {
                SysLog.CreateLog(exc.Message);
            }
        }

        private HandleResult client_OnPrepareConnect(TcpClient sender, IntPtr socket)
        {
            SysLog.CreateLog(string.Format("正在连接服务端，socket句柄为：{0}", socket.ToString()));

            return HandleResult.Ok;
        }

        private HandleResult client_OnConnect(TcpClient sender)
        {
            //如果是异步连接，更新控件状态
            appState = AppState.Started;

            SysLog.CreateLog("连接服务端成功");

            return HandleResult.Ok;
        }

        private HandleResult client_OnSend(TcpClient sender, byte[] bytes)
        {
            SysLog.CreateLog(string.Format("信息发送成功，长度：{0}", bytes.Length));

            return HandleResult.Ok;
        }

        private HandleResult client_OnReceive(TcpClient sender, byte[] bytes)
        {
            string recievedStr = Encoding.GetEncoding("UTF-8").GetString(bytes);

            SysLog.CreateLog(string.Format("收到信息，内容：{0}，长度：{1}", recievedStr, bytes.Length));

            return HandleResult.Ok;
        }

        //当触发了OnClose事件时，表示连接已经被关闭，并且OnClose事件只会被触发一次
        //通过errorCode参数判断是正常关闭还是异常关闭，0表示正常关闭
        private HandleResult client_OnClose(TcpClient sender, SocketOperation enOperation, int errorCode)
        {
            appState = AppState.Stoped;

            if (errorCode == 0)
            {
                SysLog.CreateLog("连接已关闭");
                this.BeginInvoke((MethodInvoker)delegate
                {
                    barStaticItem4.Caption = "服务器连接已关闭";
                });
            }
            else
            {
                SysLog.CreateLog(string.Format("连接异常关闭：{0}，{1}", client.ErrorMessage, client.ErrorCode));
                this.BeginInvoke((MethodInvoker)delegate
                {
                    barStaticItem4.Caption = "服务器连接已关闭";
                });
            }

            return HandleResult.Ok;
        }

        #endregion 事件处理方法

        //控件，状态标识
        public Dictionary<Button, int> ControlStatus = new Dictionary<Button, int>();

        private void button_Click(object sender, EventArgs e)
        {
            try
            {
                if (ControlStatus[((Button)sender)] == 0)
                {
                    //闪红
                    ControlStatus[((Button)sender)] = 1;
                    if (((Button)sender).Text.Contains("物料呼叫"))
                    {                        
                        ((Button)sender).Text = ((Button)sender).Text.Replace("物料呼叫", "物料待补充");
                    }
                    else
                    {
                        ((Button)sender).Text = ((Button)sender).Text.Replace("维修呼叫", "待维修");
                    }
                }
                else if (ControlStatus[((Button)sender)] == 1)
                {
                    //红
                    ControlStatus[((Button)sender)] = 2;
                    if (((Button)sender).Text.Contains("物料待补充"))
                    {
                        ((Button)sender).Text = ((Button)sender).Text.Replace("物料待补充", "物料补充中");
                    }
                    else
                    {
                        ((Button)sender).Text = ((Button)sender).Text.Replace("待维修", "维修中");
                    }
                }
                else if (ControlStatus[((Button)sender)] == 2)
                {
                    //闪绿
                    ControlStatus[((Button)sender)] = 3;
                    if (((Button)sender).Text.Contains("物料补充中"))
                    {
                        ((Button)sender).Text = ((Button)sender).Text.Replace("物料补充中", "物料待确认");
                    }
                    else
                    {
                        ((Button)sender).Text = ((Button)sender).Text.Replace("维修中", "维修待确认");
                    }
                }
                else
                {
                    //绿色
                    ControlStatus[((Button)sender)] = 0;
                    if (((Button)sender).Text.Contains("物料待确认"))
                    {
                        ((Button)sender).Text = ((Button)sender).Text.Replace("物料待确认", "物料呼叫");
                    }
                    else
                    {
                        ((Button)sender).Text = ((Button)sender).Text.Replace("维修待确认", "维修呼叫");
                    }
                }
                //操作记录
                SysLog.OperateLog(((Button)sender).Text);
                //与服务器通讯并发送信息到移动手表
                string msg = "<STX>Call_OUT," + ((Button)sender).Text + ","+ ControlStatus[((Button)sender)] + "<ETX>";
                byte[] sendBytes = Encoding.GetEncoding("UTF-8").GetBytes(msg);
                client.Send(sendBytes, sendBytes.Length);
                Com_Msg.SendData(((Button)sender).Text);
                
            }
            catch (Exception exc)
            {
                SysLog.CreateLog(string.Format("发送失败：{0}", exc.Message));
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            foreach (var item in ControlStatus)
            {
                if (item.Value == 0)
                {
                    //绿
                    item.Key.BackColor = Color.Green;
                }
                else if (item.Value == 1)
                {
                    //闪红
                    if (item.Key.BackColor == Color.Green || item.Key.BackColor == this.BackColor)
                        item.Key.BackColor = Color.Red;
                    else
                        item.Key.BackColor = this.BackColor;
                }
                else if (item.Value == 2)
                {
                    //红
                    item.Key.BackColor = Color.Red;
                }
                else if (item.Value == 3)
                {
                    //闪绿
                    if (item.Key.BackColor == Color.Red || item.Key.BackColor == this.BackColor)
                        item.Key.BackColor = Color.Green;
                    else
                        item.Key.BackColor = this.BackColor;

                }
            }
        }

        private void frmCallOut_FormClosing(object sender, FormClosingEventArgs e)
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

        private void frmCallOut_FormClosed(object sender, FormClosedEventArgs e)
        {
            appState = AppState.Stopping;
            client.Stop();
            Application.Exit();
        }

        private void SubItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                string formname = e.Item.Caption.ToString();

                if (!String.IsNullOrEmpty(formname))
                {
                    switch (formname)
                    {
                        case "修改密码":
                            frmChangePassword frmChange = new frmChangePassword();
                            frmChange.username = this.Tag.ToString();
                            frmChange.ShowDialog();
                            break;
                        case "耗材重置":
                            frmReset frmReset = new frmReset();
                            frmReset.ShowDialog();
                            return;
                        default:
                            break;
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
