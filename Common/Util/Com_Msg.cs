using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Util
{
    public static class Com_Msg
    {
        /// <summary>
        /// 发送串口信息
        /// </summary>
        /// <param name="msg"></param>
        public static void SendData(string msg)
        {
            SerialClass sc = new SerialClass("COM3", 9600, Parity.None, 8, StopBits.One);
            sc.openPort();
            sc.SendData(Com_Msg.byte_Hex(msg), 0);
            sc.closePort();
        }
        /// <summary>
        /// 将字符串组成byte数组
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] byte_Hex(string str)
        {
            StringBuilder result = new StringBuilder();
            StringBuilder strBuider = new StringBuilder();
            byte[] bt = Encoding.GetEncoding("GBK").GetBytes(str);
            result.Append("5A 00 FF FF FF FF E1 3A 00 00 00 00 0A 00 00 0B 00 00 ");
            //最多支持48个字节的数据信息
            for (int i = 0; i < 48; i++)
            {
                if (i <= bt.Length - 1)
                {
                    result.Append(Convert.ToString(bt[i], 16).ToUpper() + " ");
                }
                else
                {
                    result.Append("00" + " ");
                }
            }
            byte[] data = HexStrTobyte(result.ToString());            
            return data;
        }

        /// <summary>
        /// 将16进制的字符根据迅捷手表通讯规则组装称byte数组
        /// </summary>
        /// <param name="hs"></param>
        /// <returns></returns>
        public static byte[] HexStrTobyte(string hs)
        {
            string[] strArr = hs.Trim().Split(' ');
            byte[] b = new byte[strArr.Length + 1];
            //逐个字符变为16进制字节数据
            int sum = 0;
            for (int i = 0; i < strArr.Length; i++)
            {
                b[i] = Convert.ToByte(strArr[i], 16);
                sum = sum + Convert.ToInt32(strArr[i], 16);
            }
            //校验位规则：取前面字节的和，取低2位的值
            string check = Convert.ToString(sum, 16);
            string ch= check.Substring(check.Length-2, 2);
            b[strArr.Length] = Convert.ToByte(ch, 16);
            //按照指定编码将字节数组变为字符串
            return b;
        }

    }
}
