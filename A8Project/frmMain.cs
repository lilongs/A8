using Common.DAL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
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
            this.gdcHistory.DataSource = dtHis;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            LoadHistory();
        }
    }
}
