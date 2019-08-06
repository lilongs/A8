using Common.DAL;
using Common.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CallSystem
{
    public partial class frmReset : Form
    {
        public frmReset()
        {
            InitializeComponent();
        }

        Sys_reset_flag sys_Reset_ = new Sys_reset_flag();
        public string username = string.Empty;

        private void btnAC_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult dr = MessageBox.Show("确认重置？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (dr == DialogResult.OK)
                {
                    bool result = sys_Reset_.updateResetFlag("AC", username);
                    MessageBox.Show(result ? "重置成功" : "重置失败");
                }
            }
            catch (Exception ex)
            {
                SysLog.CreateLog(ex.Message);
            }
        }

        private void btnCC_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult dr = MessageBox.Show("确认重置？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (dr == DialogResult.OK)
                {
                    bool result = sys_Reset_.updateResetFlag("CC", username);
                    MessageBox.Show(result ? "重置成功" : "重置失败");
                }
            }
            catch (Exception ex)
            {
                SysLog.CreateLog(ex.Message);
            }
        }

        private void btnFC01_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult dr = MessageBox.Show("确认重置？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (dr == DialogResult.OK)
                {
                    bool result = sys_Reset_.updateResetFlag("FC01", username);
                    MessageBox.Show(result ? "重置成功" : "重置失败");
                }
            }
            catch (Exception ex)
            {
                SysLog.CreateLog(ex.Message);
            }
        }

        private void btnFC02_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult dr = MessageBox.Show("确认重置？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (dr == DialogResult.OK)
                {
                    bool result = sys_Reset_.updateResetFlag("FC02", username);
                    MessageBox.Show(result ? "重置成功" : "重置失败");
                }
            }
            catch (Exception ex)
            {
                SysLog.CreateLog(ex.Message);
            }
        }

        private void btnCCPrint_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult dr = MessageBox.Show("确认重置？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (dr == DialogResult.OK)
                {
                    bool result = sys_Reset_.updateResetFlag("CC_Print", username);
                    MessageBox.Show(result ? "重置成功" : "重置失败");
                }
            }
            catch (Exception ex)
            {
                SysLog.CreateLog(ex.Message);
            }
        }

        private void btnWS3Print_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult dr = MessageBox.Show("确认重置？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (dr == DialogResult.OK)
                {
                    bool result = sys_Reset_.updateResetFlag("WS3_Print", username);
                    MessageBox.Show(result ? "重置成功" : "重置失败");
                }
            }
            catch (Exception ex)
            {
                SysLog.CreateLog(ex.Message);
            }
        }
    }
}
