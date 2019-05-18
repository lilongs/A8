using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DAL
{
    public class ErrorInfo
    {
        TempTest sqlconn = new TempTest();
        public int id { get; set; }
        public string site { get; set; }
        public string message { get; set; }
        public DateTime createtime { get; set; }

        public DataTable GetErrorInfo()
        {
            string sql = "select top 10 site,message,createtime from errorinfo order by createtime desc";
            return sqlconn.Query(sql).Tables[0];
        }

        public bool InsertErrorInfo()
        {
            string sql = "insert into errorinfo(site,message,createtime) values(@site,@message,@createtime)";
            return sqlconn.ExecuteSql(sql) > 0 ? true : false;
        }


    }
}
