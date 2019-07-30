using Common.DBUtility;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DAL
{
    public class CommunicationLogs
    {
        TempTest sqlconn = new TempTest();

        public int id { get; set; }
        public string rec_message { get; set; }
        public string rec_time { get; set; }
        public string res_time { get; set; }
        public string res_message { get; set; }

        public bool InsertCommunicationLog(CommunicationLogs communicationLogs)
        {
            string sql = "insert into CommunicationLogs( rec_message, rec_time,res_message) values( '"+communicationLogs.rec_message+ "','" + communicationLogs.rec_time + "', '" + communicationLogs.res_message + "')";
            return sqlconn.ExecuteSql(sql) > 0 ? true : false;
        }
    }
}
