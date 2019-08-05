using Common.DAL;
using Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.BLL
{
    public class BUSys_userinfo
    {
        Sys_userinfo userinfo = new Sys_userinfo();

        public bool login(string username,string password)
        {
            string newPassword = MD5.MD5Encrypt(password);
            return userinfo.login(username, newPassword);
        }

        public bool changePassword(string username, string password)
        {
            string newPassword = MD5.MD5Encrypt(password);
            return userinfo.changePassword(username, newPassword);
        }
    }
}
