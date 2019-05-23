using Common.DBUtility;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DAL
{
    public class TestValue
    {
        TempTest sqlconn = new TempTest();
        public int id { get; set; }
        public string path { get; set; }
        public string productno { get; set; }
        public string testItem { get; set; }
        public string testValue { get; set; }
        public DateTime testTime { get; set; }
        public DateTime createTime { get; set; }

        public bool InsertTestValue(List<TestValue> listValues)
        {
            StringBuilder sql = new StringBuilder();
            string temp = string.Empty;
            foreach (TestValue testValue in listValues)
            {
                temp=@"select count(id) from testValue where path='"+ testValue.path + "' and productno='"+testValue.productno+"' and testItem='"+testValue.testItem+"' and active=1";
                if(sqlconn.Exists(temp))
                {
                    sql.Append("update testValue set active=0 where path='" + testValue.path + "' and productno='" + testValue.productno + "' and testItem='" + testValue.testItem + "' ");
                }
                sql.Append(@"insert into testValue (path, productno, testItem, testValue, testTime, createtime,active) 
                    values('" + testValue.path + "', '" + testValue.productno + "', '" + testValue.testItem + "', '" + testValue.testValue + "', '" + testValue.testTime + "', getdate(),1)");
            }
            return sqlconn.ExecuteSql(sql.ToString()) > 0 ? true : false;
        }
    }
}
