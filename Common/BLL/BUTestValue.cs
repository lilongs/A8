using Common.DAL;
using Common.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.BLL
{
    public class BUTestValue
    {
        Productlog productlog = new Productlog();

        /// <summary>
        /// 填充当班数据到0-24小时的表中
        /// </summary>
        /// <returns></returns>
        public DataTable GetTodayData()
        {
            DataTable dtData = productlog.GetTodayData();
            DataTable dt = new DataTable();
            dt.Columns.Add("hours", Type.GetType("System.String"));
            dt.Columns.Add("counts", Type.GetType("System.Int32"));
            for (int i = 1; i <= 24; i++)
            {
                DataRow dr = dt.NewRow();
                dr["hours"] = i + " O'clock";
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

        public DataTable GetTodayTarget()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("hours", Type.GetType("System.String"));
            dt.Columns.Add("counts", Type.GetType("System.Int32"));
            for (int i = 1; i <= 24; i++)
            {
                DataRow dr = dt.NewRow();
                dr["hours"] = i + " O'clock";
                dr["counts"] = ConfigurationManager.AppSettings["OneHourProductionTarget"];
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
            DataSet ds = productlog.GetCycleTime();

            for (int i = 0; i < ds.Tables.Count; i++)
            {
                DataRow dr = dtHis.NewRow();
                switch (i)
                {
                    case 0:
                        if (ds.Tables[i].Rows.Count > 0)
                        {
                            dr["cycletime"] = "One Hour";
                            dr["second"] = (int)(Convert.ToDouble(ds.Tables[i].Compute("Sum(cycletime)", "")) / ds.Tables[i].Rows.Count);
                            dtHis.Rows.Add(dr);
                        }
                        else
                        {
                            dr["cycletime"] = "One Hour";
                            dr["second"] = "null";
                            dtHis.Rows.Add(dr);
                        }
                        break;
                    case 1:
                        if (ds.Tables[i].Rows.Count > 0)
                        {
                            dr["cycletime"] = "One Day";
                            dr["second"] = (int)(Convert.ToDouble(ds.Tables[i].Compute("Sum(cycletime)", "")) / ds.Tables[i].Rows.Count);
                            dtHis.Rows.Add(dr);
                        }
                        else
                        {
                            dr["cycletime"] = "One Day";
                            dr["second"] = "null";
                            dtHis.Rows.Add(dr);
                        }
                        break;
                    case 2:
                        if (ds.Tables[i].Rows.Count > 0)
                        {
                            dr["cycletime"] = "One Week";
                            dr["second"] = (int)(Convert.ToDouble(ds.Tables[i].Compute("Sum(cycletime)", "")) / ds.Tables[i].Rows.Count);
                            dtHis.Rows.Add(dr);
                        }
                        else
                        {
                            dr["cycletime"] = "One Week";
                            dr["second"] = "null";
                            dtHis.Rows.Add(dr);
                        }
                        break;
                    case 3:
                        if (ds.Tables[i].Rows.Count > 0)
                        {
                            dr["cycletime"] = "One Month";
                            dr["second"] = (int)(Convert.ToDouble(ds.Tables[i].Compute("Sum(cycletime)", "")) / ds.Tables[i].Rows.Count);
                            dtHis.Rows.Add(dr);
                        }
                        else
                        {
                            dr["cycletime"] = "One Month";
                            dr["second"] = "null";
                            dtHis.Rows.Add(dr);
                        }
                        break;
                    case 4:
                        if (ds.Tables[i].Rows.Count > 0)
                        {
                            dr["cycletime"] = "One Year";
                            dr["second"] = (int)(Convert.ToDouble(ds.Tables[i].Compute("Sum(cycletime)", "")) / ds.Tables[i].Rows.Count);
                            dtHis.Rows.Add(dr);
                        }
                        else
                        {
                            dr["cycletime"] = "One Year";
                            dr["second"] = "null";
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
            string strParaMonthn = "Jan_Feb_Mar_Apr_May_Jun_Jul_Aug_Sep_Oct_Nov_Dec";
            string[] mon = strParaMonthn.Split('_');
            DataTable dt = new DataTable();
            dt.Columns.Add("months", Type.GetType("System.String"));
            dt.Columns.Add("counts", Type.GetType("System.Int32"));
            DataTable dtData = productlog.GetYearMonth();
            //创建1-12月表样式结构
            for (int i = 1; i <= 12; i++)
            {
                DataRow dr = dt.NewRow();
                dr["months"] = mon[i - 1];
                DataRow[] dataRows = dtData.Select("months=" + i + "");
                if (dataRows.Length > 0)
                    dr["counts"] = dataRows[0]["counts"].ToString();
                else
                    dr["counts"] = 0;
                dt.Rows.Add(dr);
            }
            return dt;
        }

        public DataTable GetYearMonthTarget()
        {
            string strParaMonthn = "Jan_Feb_Mar_Apr_May_Jun_Jul_Aug_Sep_Oct_Nov_Dec";
            string[] mon = strParaMonthn.Split('_');
            DataTable dt = new DataTable();
            dt.Columns.Add("months", Type.GetType("System.String"));
            dt.Columns.Add("counts", Type.GetType("System.Int32"));
            for (int i = 1; i <= 12; i++)
            {
                DataRow dr = dt.NewRow();
                dr["months"] = mon[i - 1];
                dr["counts"] = ConfigurationManager.AppSettings["YearMonthTarget"];
                dt.Rows.Add(dr);
            }
            return dt;
        }

        /// <summary>
        /// 获取每个站点的Process_IN次数
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public int GetSiteCount(string site)
        {
            int result = 0;
            DataTable dt = productlog.GetSiteCount(site);
            if (dt.Rows.Count > 0)
            {
                return result = Convert.ToInt32(dt.Rows[0]["counts"]);
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
            string strParaMonthn = "Jan_Feb_Mar_Apr_May_Jun_Jul_Aug_Sep_Oct_Nov_Dec";
            string[] mon = strParaMonthn.Split('_');
            DataSet ds = productlog.GetYearMonthFPY();
            DataTable dt = new DataTable();
            dt.Columns.Add("months", Type.GetType("System.String"));
            dt.Columns.Add("ratio", Type.GetType("System.Double"));
            for (int i = 1; i <= 12; i++)
            {
                DataRow dr = dt.NewRow();
                dr["months"] = mon[i - 1];
                DataRow[] dataRows = ds.Tables[0].Select("months=" + i + "");//AC_FPY
                DataRow[] dataRows2 = ds.Tables[1].Select("months=" + i + "");//CC_FPY
                DataRow[] dataRows3 = ds.Tables[2].Select("months=" + i + "");//FC_FPY
                if (dataRows.Length > 0 && dataRows2.Length > 0 && dataRows3.Length > 0)
                {
                    int pass_counts1 = Convert.ToInt32(dataRows[0]["pass_counts"]);
                    int counts1 = Convert.ToInt32(dataRows[0]["counts"]);
                    double resutl = (Double)pass_counts1 / counts1;

                    int pass_counts2 = Convert.ToInt32(dataRows2[0]["pass_counts"]);
                    int counts2 = Convert.ToInt32(dataRows2[0]["counts"]);
                    double resut2 = (Double)pass_counts2 / counts2;

                    int pass_counts3 = Convert.ToInt32(dataRows3[0]["pass_counts"]);
                    int counts3 = Convert.ToInt32(dataRows3[0]["counts"]);
                    double resut3 = (Double)pass_counts3 / counts3;

                    dr["ratio"] = (resutl * resut2 * resut3).ToString("0.00");
                }
                else
                {
                    dr["ratio"] = 0.00d;
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }

        public DataTable GetYearMonthFPYTarget()
        {
            string strParaMonthn = "Jan_Feb_Mar_Apr_May_Jun_Jul_Aug_Sep_Oct_Nov_Dec";
            string[] mon = strParaMonthn.Split('_');
            DataTable dt = new DataTable();
            dt.Columns.Add("months", Type.GetType("System.String"));
            dt.Columns.Add("ratio", Type.GetType("System.Double"));
            for (int i = 1; i <= 12; i++)
            {
                DataRow dr = dt.NewRow();
                dr["months"] = mon[i - 1];
                dr["ratio"] = ConfigurationManager.AppSettings["YearMonthFPYTarget"];
                dt.Rows.Add(dr);
            }
            return dt;
        }

        //public DataTable GetDayOfCountWithFPY()
        //{
        //DataTable dt = new DataTable();
        //dt.Columns.Add("name",Type.GetType("System.String"));
        //dt.Columns.Add("value", Type.GetType("System.Double"));
        //DataTable dtData = productlog.GetDayOfCountWithFPY();
        //if (dtData.Rows.Count > 0)
        //{
        //    DataRow dr = dt.NewRow();
        //    dr["name"] = "合格率";
        //    dr["value"] = 1 - (Double)Convert.ToInt32(dtData.Rows[0]["fail_counts"]) / Convert.ToInt32(dtData.Rows[0]["counts"]);
        //    dt.Rows.Add(dr);

        //    DataRow dr2 = dt.NewRow();
        //    dr2["name"] = "不良率";
        //    dr2["value"] = (Double)Convert.ToInt32(dtData.Rows[0]["fail_counts"]) / Convert.ToInt32(dtData.Rows[0]["counts"]);
        //    dt.Rows.Add(dr2);
        //}
        //else
        //{
        //    DataRow dr = dt.NewRow();
        //    dr["name"] = "合格率";
        //    dr["value"] = 1;
        //    dt.Rows.Add(dr);
        //}
        //return dt;
        //}

        /// <summary>
        /// 读取xml文件中TestSult=0的StepName
        /// </summary>
        /// <returns></returns>
        public DataTable DealErrorInfo()
        {
            DataTable dt = new DataTable();
            dt = productlog.GetInfo();
            foreach(DataRow dr in dt.Rows)
            {
                if (dr["key_process"].ToString() == "START_OUT")
                {
                    string xml = dr["contents"].ToString();
                    IList<Teststep> resultList = ReadXml.ReadXMLFroPath(xml);
                    IEnumerator enumerator = resultList.GetEnumerator();
                    List<string> list = new List<string>();
                    while (enumerator.MoveNext())
                    {
                        Teststep teststep = enumerator.Current as Teststep;
                        if (teststep.TestResult == "0")
                        {
                            list.Add(teststep.StepName);
                        }
                    }
                    if(list.Count>0)
                    dr["contents"] = String.Join(",", list.ToArray());
                }
            }
            return dt;
        }
    }
}
