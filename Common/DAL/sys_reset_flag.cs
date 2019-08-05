using Common.DBUtility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DAL
{
    public class Sys_reset_flag
    {
        TempTest sqlconn = new TempTest();
        public string keyname { get; set; }
        public bool flag { get; set; }

        public DataTable getResetFlag()
        {
            string sql = "select * from sys_reset_flag";
            return sqlconn.Query(sql).Tables[0];
        }

        public bool updateResetFlag(string keyname,int flag)
        {
            string sql = "update sys_reset_flag set flag="+flag+" where keyname='"+keyname+"' ";
            return sqlconn.ExecuteSql(sql)>0?true:false;
        }
    }
}
