using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Util
{
    public static class SysLog
    {

        public static void CreateLog(string errInfo)
        {
            DateTime now = DateTime.Now;
            FileOperate.CreateDirectory("E:\\log");
            FileOperate.CreateFile("E:\\log\\"+now.ToString("yyyyMMdd") + ".log",new List<string>() { now.ToString("yyyy-MM-dd HH:mm:ss") + "," + errInfo  });
        }
    }
}
