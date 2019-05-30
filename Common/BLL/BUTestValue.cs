using Common.DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.BLL
{
    public class BUTestValue
    {
        TestValue testValue = new TestValue();

        /// <summary>
        /// 填充当班数据到0-24小时的表中
        /// </summary>
        /// <returns></returns>
        public DataTable GetTodayData()
        {
            DataTable dtData = testValue.GetTodayData();
            DataTable dt = new DataTable();
            dt.Columns.Add("hours", Type.GetType("System.Int32"));
            dt.Columns.Add("counts", Type.GetType("System.Int32"));
            for (int i = 1; i <= 24; i++)
            {
                DataRow dr = dt.NewRow();
                dr["hours"] = i;
                DataRow[] dataRows = dtData.Select("hours=" + i + "");
                if (dataRows.Length > 0)
                {
                    dr["counts"] = dataRows[0]["counts"].ToString();
                }
                else
                {
                    dr["counts"] = 0;
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }

        /// <summary>
        /// 获取各个时间段内的Cycletime
        /// </summary>
        /// <returns></returns>
        public DataTable GetCycleTime()
        {
            DataTable dtHis = new DataTable();
            dtHis.Columns.Add("cycletime");
            dtHis.Columns.Add("second");
            DataSet ds = testValue.GetCycleTime();

            for (int i = 0; i < ds.Tables.Count; i++)
            {
                DataRow dr = dtHis.NewRow();
                switch (i)
                {
                    case 0:
                        if (ds.Tables[i].Rows.Count > 0)
                        {
                            dr["cycletime"] = "1小时";
                            dr["second"] = Convert.ToDouble(ds.Tables[i].Compute("Sum(cycletime)", "")) / ds.Tables[i].Rows.Count;
                            dtHis.Rows.Add(dr);
                        }
                        else
                        {
                            dr["cycletime"] = "1小时";
                            dr["second"] = "无数据";
                            dtHis.Rows.Add(dr);
                        }
                        break;
                    case 1:
                        if (ds.Tables[i].Rows.Count > 0)
                        {
                            dr["cycletime"] = "1天";
                            dr["second"] = Convert.ToDouble(ds.Tables[i].Compute("Sum(cycletime)", "")) / ds.Tables[i].Rows.Count;
                            dtHis.Rows.Add(dr);
                        }
                        else
                        {
                            dr["cycletime"] = "1天";
                            dr["second"] = "无数据";
                            dtHis.Rows.Add(dr);
                        }
                        break;
                    case 2:
                        if (ds.Tables[i].Rows.Count > 0)
                        {
                            dr["cycletime"] = "1周";
                            dr["second"] = Convert.ToDouble(ds.Tables[i].Compute("Sum(cycletime)", "")) / ds.Tables[i].Rows.Count;
                            dtHis.Rows.Add(dr);
                        }
                        else
                        {
                            dr["cycletime"] = "1周";
                            dr["second"] = "无数据";
                            dtHis.Rows.Add(dr);
                        }
                        break;
                    case 3:
                        if (ds.Tables[i].Rows.Count > 0)
                        {
                            dr["cycletime"] = "1月";
                            dr["second"] = Convert.ToDouble(ds.Tables[i].Compute("Sum(cycletime)", "")) / ds.Tables[i].Rows.Count;
                            dtHis.Rows.Add(dr);
                        }
                        else
                        {
                            dr["cycletime"] = "1月";
                            dr["second"] = "无数据";
                            dtHis.Rows.Add(dr);
                        }
                        break;
                    case 4:
                        if (ds.Tables[i].Rows.Count > 0)
                        {
                            dr["cycletime"] = "1年";
                            dr["second"] = Convert.ToDouble(ds.Tables[i].Compute("Sum(cycletime)", "")) / ds.Tables[i].Rows.Count;
                            dtHis.Rows.Add(dr);
                        }
                        else
                        {
                            dr["cycletime"] = "1年";
                            dr["second"] = "无数据";
                            dtHis.Rows.Add(dr);
                        }
                        break;
                    default:
                        break;
                }
            }
            return dtHis;
        }

        /// <summary>
        /// 获取一年12个月的各自产量
        /// </summary>
        /// <returns></returns>
        public DataTable GetYearMonth()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("months", Type.GetType("System.Int32"));
            dt.Columns.Add("counts", Type.GetType("System.Int32"));
            DataTable dtData = testValue.GetYearMonth();
            //创建1-12月表样式结构
            for (int i = 1; i <= 12; i++)
            {
                DataRow dr = dt.NewRow();
                dr["months"] = i;
                DataRow[] dataRows = dtData.Select("months=" + i + "");
                if (dataRows.Length > 0)
                    dr["counts"] = dataRows[0]["counts"].ToString();
                else
                    dr["counts"] = 0;
                dt.Rows.Add(dr);
            }
            return dt;
        }

        /// <summary>
        /// 获取每个站点的产品数,并计算出对应耗材的使用次数
        /// </summary>
        /// <param name="site"></param>
        /// <param name="ratio"></param>
        /// <returns></returns>
        public int GetSiteCount(string site, int ratio)
        {
            int result = 0;
            DataTable dt = testValue.GetSiteCount(site);
            if (dt.Rows.Count > 0)
            {
                return result = Convert.ToInt32(dt.Rows[0]["counts"]) * ratio;
            }
            else
            {
                return result;
            }
        }

        /// <summary>
        /// 计算一年12月的FPY
        /// </summary>
        /// <returns></returns>
        public DataTable GetYearMonthFPY()
        {
            DataTable dtData = testValue.GetYearMonthFPY();
            DataTable dt = new DataTable();
            dt.Columns.Add("months", Type.GetType("System.Int32"));
            dt.Columns.Add("ratio", Type.GetType("System.Double"));
            for (int i = 1; i <= 12; i++)
            {
                DataRow dr = dt.NewRow();
                dr["months"] = i;
                DataRow[] dataRows = dtData.Select("months=" + i + "");
                if (dataRows.Length > 0)
                {
                    int fail_counts = Convert.ToInt32(dataRows[0]["fail_counts"]);
                    int counts = Convert.ToInt32(dataRows[0]["counts"]);
                    double resutl = (Double)fail_counts / counts;
                    dr["ratio"] = resutl.ToString("0.00");
                }
                else
                {
                    dr["ratio"] = 0.00d;
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }

        public DataTable GetDayOfCountWithFPY()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("name",Type.GetType("System.String"));
            dt.Columns.Add("value", Type.GetType("System.Double"));
            DataTable dtData = testValue.GetDayOfCountWithFPY();
            if (dtData.Rows.Count > 0)
            {
                DataRow dr = dt.NewRow();
                dr["name"] = "合格率";
                dr["value"] = 1 - (Double)Convert.ToInt32(dtData.Rows[0]["fail_counts"]) / Convert.ToInt32(dtData.Rows[0]["counts"]);
                dt.Rows.Add(dr);

                DataRow dr2 = dt.NewRow();
                dr2["name"] = "不良率";
                dr2["value"] = (Double)Convert.ToInt32(dtData.Rows[0]["fail_counts"]) / Convert.ToInt32(dtData.Rows[0]["counts"]);
                dt.Rows.Add(dr2);
            }
            else
            {
                DataRow dr = dt.NewRow();
                dr["name"] = "合格率";
                dr["value"] = 1;
                dt.Rows.Add(dr);
            }
            return dt;
        }
    }
}
