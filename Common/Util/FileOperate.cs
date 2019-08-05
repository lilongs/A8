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
        public static void createFile(string folder,string filename,string info) { 
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string filepath = folder + "/" + filename;
            if (File.Exists(filepath))
            {
                FileStream fs = new FileStream(filename, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                StreamWriter sr = new StreamWriter(fs);
                sr.WriteLine(info);
                sr.Close();
                fs.Close();
            }
            else
            {
                FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                StreamWriter sr = new StreamWriter(fs);
                sr.WriteLine(info);
                sr.Close();
                fs.Close();
            }
        }

        public static string ReadFile(string path) { 
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader sr = new StreamReader(fs, System.Text.Encoding.UTF8);
            StringBuilder sb = new StringBuilder();
            while (!sr.EndOfStream)
            {
                sb.AppendLine(sr.ReadLine());
            }
            return sb.ToString();
        }
    }
}
