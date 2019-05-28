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
    }
}
