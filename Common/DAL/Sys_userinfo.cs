using Common.DBUtility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DAL
{
    public class Sys_userinfo
    {
        TempTest sqlconn = new TempTest();

        public bool login(string username,string password)
        {
            string sql = "select count(username) from sys_userinfo where username=@username and password=@password";
            SqlParameter[] param = new SqlParameter[] {
                new SqlParameter("@username",username),
                new SqlParameter("@password",password)
            };
            return sqlconn.Exists(sql, param);
        }

        public bool changePassword(string username, string password)
        {
            string sql = @"update sys_userinfo set password='"+password+"',last_edit_time=getdate() " +
                "where username='"+username+"'";
            return sqlconn.ExecuteSql(sql) > 0 ? true : false;
        }
    }
}
