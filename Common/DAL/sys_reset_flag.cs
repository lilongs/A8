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

        public DataTable GetResetInfo()
        {
            string sql = "select * from sys_reset_flag";
            return sqlconn.Query(sql).Tables[0];
        }
        /// <summary>
        /// 计数归零
        /// </summary>
        /// <param name="keyname"></param>
        /// <param name="editor"></param>
        /// <returns></returns>
        public bool updateResetFlag(string keyname,string editor)
        {
            string sql = "update sys_reset_flag set Process_IN_Counts=0,editor='" + editor+"',last_edit_time=getdate() where keyname='"+keyname+"' ";
            return sqlconn.ExecuteSql(sql)>0?true:false;
        }

        public bool Process_IN_CountIncrease(string keyname,int times)
        {
            string sql =@"update sys_reset_flag set Process_IN_Counts = Process_IN_Counts + "+times+@"
                            where keyname = '"+ keyname +"'";
            return sqlconn.ExecuteSql(sql) > 0 ? true : false;
        }
    }
}
