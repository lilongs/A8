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
    public partial class frmLogin : Form
    {
        public frmLogin()
        {
            InitializeComponent();
        }
        BUSys_userinfo userinfo = new BUSys_userinfo();
        private void btnLogin_Click(object sender, EventArgs e)
        {
            #region 检验输入不能为空
            if(String.IsNullOrEmpty(txtUsername.Text))
            {
                MessageBox.Show("请输入用户名！");
                return;
            }
            if (String.IsNullOrEmpty(txtPassword.Text))
            {
                MessageBox.Show("请输入用户密码！");
                return;
            }
            #endregion

           if(userinfo.login(txtUsername.Text.Trim(), txtPassword.Text.Trim()))
            {
                frmCallOut frm = new frmCallOut();
                frm.Tag = txtUsername.Text.Trim();
                frm.Show();                
                this.Hide();
            }
            else
            {
                MessageBox.Show("登录失败，用户信息错误");
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
                this.btnLogin_Click(sender, e);
        }
    }
}
