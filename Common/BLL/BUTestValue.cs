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
            dt.Columns.Add("hours", Type.GetType("System.DateTime"));
            dt.Columns.Add("counts", Type.GetType("System.Int32"));
            double sum = 0d;
            foreach(DataRow drs in dtData.Rows)
            {
                DataRow dr = dt.NewRow();
                sum += Convert.ToDouble(drs["counts"]);
                string min =drs["mins"].ToString();
                dr["hours"] = Convert.ToDateTime(min);
                dr["counts"] = sum;
                dt.Rows.Add(dr);
            }
            //for (int i = 1; i <= 24; i++)
            //{
            //    DataRow dr = dt.NewRow();
            //    dr["hours"] = i + " O'clock";
            //    DataRow[] dataRows = dtData.Select("hours=" + i + "");
            //    if (dataRows.Length > 0)
            //    {
            //        dr["counts"] = dataRows[0]["counts"].ToString();
            //    }
            //    else
            //    {
            //        dr["counts"] = 0;
            //    }
            //    dt.Rows.Add(dr);
            //}
            return dt;
        }

        public double ConvertHourFromMin(string time)
        {
            double result = 0d;
            if (!String.IsNullOrEmpty(time))
            {
                string[] arr=time.Split(':');                
                result =  Convert.ToDouble(arr[0])+Convert.ToDouble(arr[1])/60;
            }
            return result;
        }

        public DataTable GetTodayTarget()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("hours", Type.GetType("System.DateTime"));
            dt.Columns.Add("counts", Type.GetType("System.Int32"));
            for (int i = 0; i < 24; i++)
            {
                DataRow dr = dt.NewRow();
                dr["hours"] = Convert.ToDateTime(i + ":00");
                dr["counts"] = Convert.ToDouble(ConfigurationManager.AppSettings["OneHourProductionTarget"])*i;
                dt.Rows.Add(dr);
            }
            DataRow dr2 = dt.NewRow();
            dr2["hours"] = Convert.ToDateTime("23:59:59.999");
            dr2["counts"] = Convert.ToDouble(ConfigurationManager.AppSettings["OneHourProductionTarget"]) * 24;
            dt.Rows.Add(dr2);

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
            dtHis.Columns.Add("targetCT");            
            dtHis.Columns.Add("deviation");  
            
            DataSet ds = productlog.GetCycleTime();
            int targetCT = Convert.ToInt32(ConfigurationManager.AppSettings["CycleTimeTarget"]);

            for (int i = 0; i < ds.Tables.Count; i++)
            {
                DataRow dr = dtHis.NewRow();
                dr["targetCT"] = targetCT;
                switch (i)
                {
                    case 0:
                        dr["cycletime"] = "One Hour";
                        if (ds.Tables[i].Rows.Count > 0)
                        {
                            dr["second"] = (int)(Convert.ToDouble(ds.Tables[i].Compute("Sum(cycletime)", "")) / ds.Tables[i].Rows.Count);
                            dr["deviation"]= (int)(Convert.ToDouble(ds.Tables[i].Compute("Sum(cycletime)", "")) / ds.Tables[i].Rows.Count) - targetCT;
                        }
                        else
                        {
                            dr["second"] = "null";
                            dr["deviation"] =  -targetCT;
                        }
                        break;
                    case 1:
                        dr["cycletime"] = "One Day";
                        if (ds.Tables[i].Rows.Count > 0)
                        {
                            dr["second"] = (int)(Convert.ToDouble(ds.Tables[i].Compute("Sum(cycletime)", "")) / ds.Tables[i].Rows.Count);
                            dr["deviation"] = (int)(Convert.ToDouble(ds.Tables[i].Compute("Sum(cycletime)", "")) / ds.Tables[i].Rows.Count) - targetCT;
                        }
                        else
                        {
                            dr["second"] = "null";
                            dr["deviation"] = -targetCT;
                        }
                        break;
                    case 2:
                        dr["cycletime"] = "One Week";
                        if (ds.Tables[i].Rows.Count > 0)
                        {
                            dr["second"] = (int)(Convert.ToDouble(ds.Tables[i].Compute("Sum(cycletime)", "")) / ds.Tables[i].Rows.Count);
                            dr["deviation"] = (int)(Convert.ToDouble(ds.Tables[i].Compute("Sum(cycletime)", "")) / ds.Tables[i].Rows.Count) - targetCT;
                        }
                        else
                        {
                            dr["second"] = "null";
                            dr["deviation"] = -targetCT;
                        }
                        break;
                    case 3:
                        dr["cycletime"] = "One Month";
                        if (ds.Tables[i].Rows.Count > 0)
                        {
                            dr["second"] = (int)(Convert.ToDouble(ds.Tables[i].Compute("Sum(cycletime)", "")) / ds.Tables[i].Rows.Count);
                            dr["deviation"] = (int)(Convert.ToDouble(ds.Tables[i].Compute("Sum(cycletime)", "")) / ds.Tables[i].Rows.Count) - targetCT;
                        }
                        else
                        {
                            dr["second"] = "null";
                            dr["deviation"] = -targetCT;
                        }
                        break;
                    case 4:
                        dr["cycletime"] = "One Year";
                        if (ds.Tables[i].Rows.Count > 0)
                        {
                            dr["second"] = (int)(Convert.ToDouble(ds.Tables[i].Compute("Sum(cycletime)", "")) / ds.Tables[i].Rows.Count);
                            dr["deviation"] = (int)(Convert.ToDouble(ds.Tables[i].Compute("Sum(cycletime)", "")) / ds.Tables[i].Rows.Count) - targetCT;
                        }
                        else
                        {
                            dr["second"] = "null";
                            dr["deviation"] = -targetCT;
                        }
                        break;
                    default:
                        break;
                }
                dtHis.Rows.Add(dr);
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
                DataRow[] dataRows3 = ds.Tables[2].Select("months=" + i + "");//FC01_FPY
                DataRow[] dataRows4 = ds.Tables[2].Select("months=" + i + "");//FC02_FPY
                if (dataRows.Length > 0 && dataRows2.Length > 0 && dataRows3.Length > 0 && dataRows4.Length > 0)
                {
                    int pass_counts1 = Convert.ToInt32(dataRows[0]["fail_counts"]);
                    int counts1 = Convert.ToInt32(dataRows[0]["counts"]);
                    double resutl =1- (Double)pass_counts1 / counts1;

                    int pass_counts2 = Convert.ToInt32(dataRows2[0]["fail_counts"]);
                    int counts2 = Convert.ToInt32(dataRows2[0]["counts"]);
                    double resut2 = 1-(Double)pass_counts2 / counts2;

                    int pass_counts3 = Convert.ToInt32(dataRows3[0]["fail_counts"]);
                    int counts3 = Convert.ToInt32(dataRows3[0]["counts"]);
                    double resut3 =1- (Double)pass_counts3 / counts3;

                    int pass_counts4 = Convert.ToInt32(dataRows4[0]["fail_counts"]);
                    int counts4 = Convert.ToInt32(dataRows4[0]["counts"]);
                    double resut4 = 1-(Double)pass_counts4 / counts4;

                    dr["ratio"] = (resutl * resut2 * resut3).ToString("0.000");
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
            try
            {
                DataTable dt = new DataTable();
                dt = productlog.GetInfo();
                foreach (DataRow dr in dt.Rows)
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
                            if (teststep.TestResult == "2")
                            {
                                list.Add(teststep.StepName);
                            }
                        }
                        if (list.Count > 0)
                            dr["contents"] = String.Join(",", list.ToArray());
                    }
                }
                return dt;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}
