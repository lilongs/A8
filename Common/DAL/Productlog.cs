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
            string sql = "insert into Productlog( key_process, equipment, productno, result, contents, createtime) " +
                "values( '"+productlog.key_process+"','"+productlog.equipment + "','" + productlog.productno + "','" + productlog.result + "','" + productlog.contents + "','" + productlog.createtime + "')";            
            return sqlconn.ExecuteSql(sql) > 0 ? true : false;
        }

        /// <summary>
        /// 实时信息记录
        /// </summary>
        /// <returns></returns>
        public DataTable GetInfo()
        {
            string sql = @"select top 20 key_process,contents ,equipment,productno,createtime 
                        from productlog
                        where result='FAIL'
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
            sql.Append(@" select a.productno,DATEDIFF(SECOND,starttime,endtime)as cycletime 
                        from (
	                        select productno,min(createtime)as starttime
	                        from  (select equipment,productno,key_process,createtime  
			                        from productlog
			                        where DATEDIFF(HOUR, createtime, GETDATE()) <= 1 and DATEDIFF(HOUR, createtime, GETDATE()) >=0
			                        and equipment in ('AC','CC','FC01','FC02','Run-In','WS1','WS2','WS3','WS4'))as temp
	                        where equipment='WS1' and  key_process='Process_IN'
	                        group by productno)as a
                        left join (
	                        select productno,min(createtime)as endtime 
	                        from (select equipment,productno,key_process,createtime  
			                        from productlog
			                        where DATEDIFF(HOUR, createtime, GETDATE()) <= 1 and DATEDIFF(HOUR, createtime, GETDATE()) >=0
			                        and equipment in ('AC','CC','FC01','FC02','Run-In','WS1','WS2','WS3','WS4'))as temp 
	                        where equipment='CC' and  key_process='START_OUT'
	                        group by productno
	                        ) as b on a.productno=b.productno");
            //一天
            sql.Append(@" select a.productno,DATEDIFF(SECOND,starttime,endtime)as cycletime 
                        from (
	                        select productno,min(createtime)as starttime
	                        from  (select equipment,productno,key_process,createtime  
			                        from productlog
			                        where DATEDIFF(DAY,createtime,GETDATE())<=1 and DATEDIFF(DAY,createtime,GETDATE())>=0
			                        and equipment in ('AC','CC','FC01','FC02','Run-In','WS1','WS2','WS3','WS4'))as temp
	                        where equipment='WS1' and  key_process='Process_IN'
	                        group by productno)as a
                        left join (
	                        select productno,min(createtime)as endtime 
	                        from (select equipment,productno,key_process,createtime  
			                        from productlog
			                        where DATEDIFF(DAY,createtime,GETDATE())<=1 and DATEDIFF(DAY,createtime,GETDATE())>=0
			                        and equipment in ('AC','CC','FC01','FC02','Run-In','WS1','WS2','WS3','WS4'))as temp 
	                        where equipment='CC' and  key_process='START_OUT'
	                        group by productno
	                        ) as b on a.productno=b.productno");
            //一周
            sql.Append(@" select a.productno,DATEDIFF(SECOND,starttime,endtime)as cycletime 
                        from (
	                        select productno,min(createtime)as starttime
	                        from (select equipment,productno,key_process,createtime  
			                        from productlog
			                        where DATEDIFF(DAY, createtime, GETDATE()) <= 7 and DATEDIFF(DAY, createtime, GETDATE()) >=0
			                        and equipment in ('AC','CC','FC01','FC02','Run-In','WS1','WS2','WS3','WS4'))as temp
	                        where equipment='WS1' and  key_process='Process_IN'
	                        group by productno)as a
                        left join (
	                        select productno,min(createtime)as endtime 
	                        from (
		                        select equipment,productno,key_process,createtime  
			                        from productlog
			                        where DATEDIFF(DAY, createtime, GETDATE()) <= 7 and DATEDIFF(DAY, createtime, GETDATE()) >=0
			                        and equipment in ('AC','CC','FC01','FC02','Run-In','WS1','WS2','WS3','WS4'))as temp
	                        where equipment='CC' and  key_process='START_OUT'
	                        group by productno
	                        ) as b on a.productno=b.productno");
            //一个月
            sql.Append(@"  select a.productno,DATEDIFF(SECOND,starttime,endtime)as cycletime 
                            from (
	                            select productno,min(createtime)as starttime
	                            from  (select equipment,productno,key_process,createtime  
			                            from productlog
			                            where DATEDIFF(DAY,createtime,GETDATE())<=30 and DATEDIFF(DAY,createtime,GETDATE())>=0
			                            and equipment in ('AC','CC','FC01','FC02','Run-In','WS1','WS2','WS3','WS4'))as temp
	                            where equipment='WS1' and  key_process='Process_IN'
	                            group by productno)as a
                            left join (
	                            select productno,min(createtime)as endtime 
	                            from (select equipment,productno,key_process,createtime  
			                            from productlog
			                            where DATEDIFF(DAY,createtime,GETDATE())<=30 and DATEDIFF(DAY,createtime,GETDATE())>=0
			                            and equipment in ('AC','CC','FC01','FC02','Run-In','WS1','WS2','WS3','WS4'))as temp 
	                            where equipment='CC' and  key_process='START_OUT'
	                            group by productno
	                            ) as b on a.productno=b.productno");
            //一年
            sql.Append(@" select a.productno,DATEDIFF(SECOND,starttime,endtime)as cycletime 
                            from (
	                            select productno,min(createtime)as starttime
	                            from  (select equipment,productno,key_process,createtime  
			                            from productlog
			                            where DATEDIFF(YEAR,createtime,GETDATE())<=1 and DATEDIFF(YEAR,createtime,GETDATE())>=0
			                            and equipment in ('AC','CC','FC01','FC02','Run-In','WS1','WS2','WS3','WS4'))as temp
	                            where equipment='WS1' and  key_process='Process_IN'
	                            group by productno)as a
                            left join (
	                            select productno,min(createtime)as endtime 
	                            from (select equipment,productno,key_process,createtime  
			                            from productlog
			                            where DATEDIFF(YEAR,createtime,GETDATE())<=1 and DATEDIFF(YEAR,createtime,GETDATE())>=0
			                            and equipment in ('AC','CC','FC01','FC02','Run-In','WS1','WS2','WS3','WS4'))as temp 
	                            where equipment='CC' and  key_process='START_OUT'
	                            group by productno
	                            ) as b on a.productno=b.productno");

            return sqlconn.Query(sql.ToString());
        }

        /// <summary>
        /// 获取当班整点的产量,以CC站点作为最后一个站点，用于统计产量
        /// </summary>
        /// <returns></returns>
        public DataTable GetTodayData()
        {
            //string sql = @"select DATEPART(hh, createtime) as hours,count(productno)as counts  
            //            from productlog
            //            where DATEDIFF(DAY, createtime, GETDATE()) = 0
            //            and equipment='CC' and key_process='START_OUT'
            //            group by DATEPART(hh, createtime)";

            string sql = @"select FORMAT(endtime, 'HH:mm') as mins,count(distinct productno)as counts  
                        FROM(
                        select productno,min(createtime)as endtime 
                        from (select equipment,productno,key_process,createtime  
		                        from productlog
		                        where DATEDIFF(DAY,createtime,GETDATE())=0 
		                        and equipment in ('AC','CC','FC01','FC02','Run-In','WS1','WS2','WS3','WS4'))as temp 
                        where equipment='CC' and  key_process='START_OUT'
                        group by productno)AS B
                        group by FORMAT(endtime, 'HH:mm')
                         order by mins";
            return sqlconn.Query(sql).Tables[0];
        }

        /// <summary>
        /// 获取一年12个月的产量,以CC站点作为最后一个站点，用于统计产量
        /// </summary>
        /// <returns></returns>
        public DataTable GetYearMonth()
        {
            string sql = @"select DATEPART(MM, endtime) as months,count(productno) as counts 
                        from (
                            select productno,min(createtime)as endtime 
                            from (select equipment,productno,key_process,createtime  
		                            from productlog
		                            where DATEDIFF(YEAR,createtime,GETDATE())=0 
		                            and equipment in ('AC','CC','FC01','FC02','Run-In','WS1','WS2','WS3','WS4')
                                )as temp 
                            where equipment='CC' and  key_process='START_OUT'
                            group by productno)as b
                        group by DATEPART(MM, endtime)";
            return sqlconn.Query(sql).Tables[0];
        }

        /// <summary>
        /// 获取每个站点Process_IN的产品数的次数
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public DataTable GetSiteCount(string site)
        {
            string sql = @"select '" + site + @"' as site,count(productno) as counts 
                        from productlog
                        where equipment like '%" + site + "%' and key_process='Process_IN'";
            return sqlconn.Query(sql).Tables[0];
        }

        /// <summary>
        /// 计算一年12月的FPY，FPY=AC_FPY*CC_FPY*FC_FPY
        /// </summary>
        /// <returns></returns>
        public DataSet GetYearMonthFPY()
        {
            StringBuilder sql = new StringBuilder();
            //AC_FPY
            sql.Append(@"select A.*,isnull(B.fail_counts,0) as fail_counts from (
	                    select DATEPART(MM,createtime)as months,count(productno) as counts 
	                    from productlog
	                    where equipment='AC' 
	                    and key_process='START_OUT'
	                    group by DATEPART(MM,createtime)
                    )as a
                    left join (
	                    select DATEPART(MM,createtime)as months,count(productno) as fail_counts 
	                    from productlog
	                    where equipment='AC' 
	                    and (key_process='START_OUT' or key_process='Process_OUT') and result='FAIL'
	                    group by DATEPART(MM,createtime)
                    )as b on a.months=b.months");


                //CC_FPY
                sql.Append(@" select A.*,isnull(B.fail_counts,0) as fail_counts from (
	                        select DATEPART(MM,createtime)as months,count(productno) as counts 
	                        from productlog
	                        where equipment='CC'and key_process='START_OUT'
	                        group by DATEPART(MM,createtime)
                        )as a
                        left join (
	                        select DATEPART(MM,createtime)as months,count(productno) as fail_counts 
	                        from productlog
	                        where equipment='CC'
							and (key_process='START_OUT' or key_process='Process_OUT') and result='FAIL'
	                        group by DATEPART(MM,createtime)
                        )as b on a.months=b.months");

                //FC01_FPY
                 sql.Append(@" select A.*,isnull(B.fail_counts,0) as fail_counts from (
	                select DATEPART(MM,createtime)as months,count(productno) as counts 
	                from productlog
	                where equipment='FC01' 
	                and key_process='START_OUT'
	                group by DATEPART(MM,createtime)
                )as a
                left join (
	                select DATEPART(MM,createtime)as months,count(productno) as fail_counts 
	                from productlog
	                where equipment='FC01'
	                and (key_process='START_OUT' or key_process='Process_OUT') and result='FAIL'
	                group by DATEPART(MM,createtime)
                )as b on a.months=b.months ");

            //FC02_FPY
            sql.Append(@" select A.*,isnull(B.fail_counts,0) as fail_counts from (
	                select DATEPART(MM,createtime)as months,count(productno) as counts 
	                from productlog
	                where equipment='FC02' 
	                and key_process='START_OUT'
	                group by DATEPART(MM,createtime)
                )as a
                left join (
	                select DATEPART(MM,createtime)as months,count(productno) as fail_counts 
	                from productlog
	                where equipment='FC02'
	                and (key_process='START_OUT' or key_process='Process_OUT') and result='FAIL'
	                group by DATEPART(MM,createtime)
                )as b on a.months=b.months ");
            return sqlconn.Query(sql.ToString());
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

        /// <summary>
        /// 确认当前站点该产品是否已进站
        /// </summary>
        /// <param name="equipment"></param>
        /// <param name="productno"></param>visteon0000Process_OUT
        /// <returns></returns>
        public Boolean CheckProcess(string equipment,string productno)
        {
            string sql = @"select count(id) 
                        from productlog
                        where key_process='Process_IN' and equipment='" + equipment + "' and productno='"+ productno + "'";
            return sqlconn.Exists(sql);
        }
    }
}
