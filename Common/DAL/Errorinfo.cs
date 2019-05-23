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
    public class ErrorInfo
    {
        TempTest sqlconn = new TempTest();
        public int id { get; set; }
        public string site { get; set; }
        public string message { get; set; }
        public DateTime createtime { get; set; }

        /// <summary>
        /// 获取最新10条实时信息
        /// </summary>
        /// <returns></returns>
        public DataTable GetErrorInfo()
        {
            string sql = "select top 10 site,message,createtime from errorinfo order by createtime desc";
            return sqlconn.Query(sql).Tables[0];
        }

        /// <summary>
        /// 存储实时信息
        /// </summary>
        /// <param name="errorInfo"></param>
        /// <returns></returns>
        public bool InsertErrorInfo(ErrorInfo errorInfo)
        {
            string sql = "insert into errorinfo(site,message,createtime) values(@site,@message,@createtime)";
            SqlParameter[] param = new SqlParameter[]
            {
                new SqlParameter("@site",errorInfo.site),
                 new SqlParameter("@message",errorInfo.message),
                  new SqlParameter("@createtime",errorInfo.createtime),
            };
            return sqlconn.ExecuteSql(sql,param) > 0 ? true : false;
        }
    }
}