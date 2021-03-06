﻿using Common.DBUtility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DAL
{
    public class Productlog
    {
        TempTest sqlconn = new TempTest();
        public int id { get; set; }
        public string key_process { get; set; }
        public string equipment { get; set; }
        public string productno { get; set; }
        public string result { get; set; }
        public string contents { get; set; }
        public string createtime { get; set; }        

        /// <summary>
        /// 存储实时信息
        /// </summary>
        /// <param name="productlog"></param>
        /// <returns></returns>
        public bool InsertProductlog(Productlog productlog)
        {
            string sql = "insert into Productlog( key_process, equipment, productno, result, contents, createtime) values( @key_process,@equipment,@productno,@result,@contents,@createtime)";
            SqlParameter[] param = new SqlParameter[]
            {
                new SqlParameter("@key_process",productlog.key_process),
                new SqlParameter("@equipment",productlog.equipment),
                new SqlParameter("@productno",productlog.productno),
                new SqlParameter("@result",productlog.result),
                new SqlParameter("@contents",productlog.contents),
                new SqlParameter("@createtime",productlog.createtime),

            };
            return sqlconn.ExecuteSql(sql, param) > 0 ? true : false;
        }

        /// <summary>
        /// 实时信息记录
        /// </summary>
        /// <returns></returns>
        public DataTable GetInfo()
        {
            string sql = @"select top 10 key_process,equipment,productno,createtime 
                        from productlog
                        order by createtime desc";
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
            sql.Append(@" select productno,DATEDIFF(SECOND,min(createtime),max(createtime)) as cycletime 
                        from productlog 
                        where DATEDIFF(HOUR, createtime, GETDATE()) <= 1 
                        group by productno");
            //一天
            sql.Append(@" select productno,DATEDIFF(SECOND,min(createtime),max(createtime)) as cycletime 
                        from productlog
                        where DATEDIFF(DAY,createtime,GETDATE())<=1 
                        group by productno");
            //一周
            sql.Append(@" select productno, DATEDIFF(SECOND, min(createtime), max(createtime)) as cycletime
                        from productlog 
                        where DATEDIFF(DAY, createtime, GETDATE()) <= 7 
                        group by productno");
            //一个月
            sql.Append(@" select productno,DATEDIFF(SECOND,min(createtime),max(createtime)) as cycletime   
                        from productlog 
                        where DATEDIFF(DAY,createtime,GETDATE())<=30
                        group by productno");
            //一年
            sql.Append(@" select productno,DATEDIFF(SECOND,min(createtime),max(createtime)) as cycletime   
                        from productlog 
                        where DATEDIFF(YEAR,createtime,GETDATE())<=1 
                        group by productno");

            return sqlconn.Query(sql.ToString());
        }

        /// <summary>
        /// 获取当班整点的产量
        /// </summary>
        /// <returns></returns>
        public DataTable GetTodayData()
        {
            string sql = @"select hours,count(productno)as counts 
                    from(select distinct productno, DATEPART(hh, createtime) as hours
                    from productlog
                    where DATEDIFF(DAY, createtime, GETDATE()) = 0
                    ) as a
                    group by hours";
            return sqlconn.Query(sql).Tables[0];
        }

        /// <summary>
        /// 获取一年12个月的产量
        /// </summary>
        /// <returns></returns>
        public DataTable GetYearMonth()
        {
            string sql = @"select months,count(productno) as counts 
                        from(select distinct productno, DATEPART(MM, createtime) as months
                        from productlog
                        where DATEDIFF(YEAR, createtime, GETDATE()) = 0 ) as a
                        group by months";
            return sqlconn.Query(sql).Tables[0];
        }

        /// <summary>
        /// 获取每个站点的产品数
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public DataTable GetSiteCount(string site)
        {
            string sql = @"select '" + site + @"' as site,count(productno) as counts 
                        from(
                        select distinct productno from productlog
                        where equipment = '" + site + "') as a";
            return sqlconn.Query(sql).Tables[0];
        }

        /// <summary>
        /// 计算一年12月的FPY
        /// </summary>
        /// <returns></returns>
        public DataTable GetYearMonthFPY()
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(@"select A.*,B.fail_counts 
                        from (select months,count(productno)as fail_counts 
                        from (select distinct productno,result,DATEPART(MM,createtime)as months 
                        from productlog
                        where DATEDIFF(YEAR,createtime,GETDATE())=0 
                        and result='FAIL') as a
                        group by months) as B
                        left join (
                        select months,count(productno)as counts
                            from (select distinct productno,DATEPART(MM,createtime)as months
                        from productlog
                        where DATEDIFF(YEAR,createtime,GETDATE())=0 ) as a
                        group by months) as A on A.months=B.months ");
            return sqlconn.Query(sql.ToString()).Tables[0];
        }

        //public DataTable GetDayOfCountWithFPY()
        //{
            //string sql = @"select A.*,B.fail_counts 
            //                from(select days, count(productno) as fail_counts
            //                from(select distinct productno, result, DATEPART(DAY, testtime) as days
            //                from testvalue
            //                where DATEDIFF(DAY, testtime, GETDATE()) = 0 and active = 1
            //                and result = 'FAILED') as a
            //                group by days) as B
            //                left join(select days, count(productno)as counts
            //                from(select distinct productno, DATEPART(DAY, testtime) as days
            //                from testvalue
            //                where DATEDIFF(DAY, testtime, GETDATE()) = 0 and active = 1) as a
            //                group by days) as A on A.days = B.days";
            //return sqlconn.Query(sql).Tables[0];
       //}
    }
}
