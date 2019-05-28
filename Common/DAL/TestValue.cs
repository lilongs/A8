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

        /// <summary>
        /// 保存测试数据
        /// </summary>
        /// <param name="listValues"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 获取当班整点的产量
        /// </summary>
        /// <returns></returns>
        public DataTable GetTodayData()
        {
            string sql = @"select hours,count(productno)as counts 
                    from(select distinct productno, DATEPART(hh, testtime) as hours
                    from testValue
                    where DATEDIFF(DAY, testtime, GETDATE()) = 0
                    ) as a
                    group by hours";
            return sqlconn.Query(sql).Tables[0];
        }

        /// <summary>
        /// 获取各个时间段内的Cycletime
        /// </summary>
        /// <returns></returns>
        public DataSet GetCycleTime()
        {
            StringBuilder sql = new StringBuilder();
            //一小时
            sql.Append(@" select productno,DATEDIFF(SECOND,min(testtime),max(testtime)) as cycletime 
                         from testValue where DATEDIFF(HOUR, createtime, GETDATE()) <= 1 and active = 1
                         group by productno");
            //一天
            sql.Append(@" select productno,DATEDIFF(SECOND,min(testtime),max(testtime)) as cycletime 
                         from testValue where DATEDIFF(DAY,createtime,GETDATE())<=1 and active=1
                         group by productno");
            //一周
            sql.Append(@" select productno, DATEDIFF(SECOND, min(testtime), max(testtime)) as cycletime
                          from testValue where DATEDIFF(DAY, createtime, GETDATE()) <= 7 and active = 1
                          group by productno");
            //一个月
            sql.Append(@" select productno,DATEDIFF(SECOND,min(testtime),max(testtime)) as cycletime   
                          from testValue where DATEDIFF(DAY,createtime,GETDATE())<=30 and active=1 
                          group by productno");
            //一年
            sql.Append(@" select productno,DATEDIFF(SECOND,min(testtime),max(testtime)) as cycletime   
                          from testValue where DATEDIFF(YEAR,createtime,GETDATE())<=1 and active=1
                          group by productno");

            return sqlconn.Query(sql.ToString());
        }
    }
}
