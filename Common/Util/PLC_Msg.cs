using Common.DBUtility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Util
{
    public class PLC_Msg
    {
        public static string Msg(string site,int bytes,int bit)
        {
            TempTest sqlconn = new TempTest();
            DataTable dt = sqlconn.Query("select * from PLC_Msg where site='"+site+"',byte="+bytes+",bit="+bit+"").Tables[0];
            if (dt.Rows.Count >0)
            {
                return dt.Rows[0]["message"].ToString();
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
