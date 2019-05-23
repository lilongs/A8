using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Util
{
    public static class FileOperate
    {
        /// <summary>
        /// 读取指定路径下的文本
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string[] RealFile(string path)
        {
            if (File.Exists(path))
            {
                return File.ReadAllLines(path);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 向指定的路径生成文本文件，并写入指定内容
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filename"></param>
        /// <param name="list"></param>
        public static void CreateFile(string path, List<string> list)
        {
            if (!File.Exists(path))
            {
                FileStream fs = File.Create(path);
                fs.Close();
            }
            
            File.AppendAllLines(path, list);
        }

        /// <summary>
        /// 删除指定路径下的文件
        /// </summary>
        /// <param name="path"></param>
        public static void DeleteFile(string path)
        {
            File.Delete(path);
        }
    }
}
