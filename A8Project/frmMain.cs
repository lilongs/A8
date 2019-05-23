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

namespace A8Project
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }
        ErrorInfo errorInfo = new ErrorInfo();
        private void err_timer1_Tick(object sender, EventArgs e)
        {
            //在家ErrorInfo信息
            DataTable dt = new DataTable();
            dt = errorInfo.GetErrorInfo();
            this.gdcErrorInfo.DataSource = dt;
        }

        private void LoadHistory()
        {
            DataTable dtHis = new DataTable();
            dtHis.Columns.Add("cycletime");
            dtHis.Columns.Add("second");

            DataRow dr = dtHis.NewRow();
            dr["cycletime"] = "1小时";
            dr["second"] = 8.2;
            dtHis.Rows.Add(dr);

            DataRow dr1 = dtHis.NewRow();
            dr1["cycletime"] = "1天";
            dr1["second"] = 8.3;
            dtHis.Rows.Add(dr1);

            DataRow dr2 = dtHis.NewRow();
            dr2["cycletime"] = "1周";
            dr2["second"] = 8.5;
            dtHis.Rows.Add(dr2);

            DataRow dr3 = dtHis.NewRow();
            dr3["cycletime"] = "1个月";
            dr3["second"] = 9.0;
            dtHis.Rows.Add(dr3);

            DataRow dr4 = dtHis.NewRow();
            dr4["cycletime"] = "1年";
            dr4["second"] = 9.5;
            dtHis.Rows.Add(dr4);

            this.gdcHistory.DataSource = dtHis;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            LoadHistory();
            //DealTestValue();
        }

        //文档处理，工控机给一个完成信息然后进行文档处理,一次处理一个文件
        private void DealTestValue()
        {
            try
            {
                string dealPath = FileOperate.RealFile(Application.StartupPath + "\\dealpath.dat")[0];
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
        private void GetCommunication(string site, string message)
        {
            ErrorInfo errorInfo = new ErrorInfo();
            errorInfo.createtime = DateTime.Now;
            errorInfo.site = site;
            errorInfo.message = message;
            errorInfo.InsertErrorInfo(errorInfo);
        }
    }
}