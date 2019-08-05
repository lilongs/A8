using Common.BLL;
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
    public partial class frmChangePassword : Form
    {
        public frmChangePassword()
        {
            InitializeComponent();
        }
        public string username = string.Empty;
        BUSys_userinfo userinfo = new BUSys_userinfo();

        private void frmChangePassword_Load(object sender, EventArgs e)
        {
            this.txtusername.Text = username;
        }

        private void btnSure_Click(object sender, EventArgs e)
        {
            #region 检验输入
            if (String.IsNullOrEmpty(txtoldpassword.Text.Trim()))
            {
                MessageBox.Show("请输入当前密码！");
                return;
            }
            if (String.IsNullOrEmpty(txtnewpassword.Text.Trim()))
            {
                MessageBox.Show("请输入新密码！");
                return;
            }
            if (String.IsNullOrEmpty(txtDoubleSure.Text.Trim()))
            {
                MessageBox.Show("请再次输入新密码！");
                return;
            }
            #endregion
            #region 校验两次新密码是否相同
            if (!String.Equals(txtnewpassword.Text.Trim(), txtDoubleSure.Text.Trim()))
            {
                MessageBox.Show("两次输入新密码不一致 ，请确认！");
                return;
            }
            #endregion
            #region 校验当前用户密码
            if (!userinfo.login(txtusername.Text.Trim(), txtoldpassword.Text.Trim()))
            {
                MessageBox.Show("当前密码错误，请确认！");
                return;
            }
            #endregion
            if (userinfo.changePassword(txtusername.Text.Trim(), txtDoubleSure.Text.Trim()))
            {
                MessageBox.Show("修改成功！");
            }
            else
            {
                MessageBox.Show("修改失败！");
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
