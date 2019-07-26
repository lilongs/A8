using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Common.Util
{
    public static class ReadXml
    {
        public static IList<Teststep> ReadXMLFroPath(string str)
        {
            IList<Teststep> resultList = new List<Teststep>();

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(str);

            XmlNodeList xmlNodeList = xmlDocument.SelectSingleNode("Cluster").ChildNodes;
            foreach (XmlNode list in xmlNodeList)
            {
                Teststep teststep = new Teststep
                (
                    list.Attributes["StepNo"].InnerText,
                    list.Attributes["StepName"].InnerText,
                    list.Attributes["Value"].InnerText,
                    list.Attributes["UPPERLIMIT"].InnerText,
                    list.Attributes["LOWERLIMIT"].InnerText,
                    list.Attributes["Unit"].InnerText,
                    list.Attributes["Duration"].InnerText,
                    list.Attributes["TestResult"].InnerText
                );
                resultList.Add(teststep);
            }
            return resultList;
        }

        public static string ReadValueByXPath(string xml,string XPath,string targetAttributes)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            var node = doc.SelectSingleNode(XPath);
            if (node != null && node.Attributes != null)
            {
                return node.Attributes[targetAttributes].Value;
            }
            else
            {
                return string.Empty;
            }
        }
    }

    public class Teststep
    {
        public string StepNo { get; set; }
        public string StepName { get; set; }
        public string Value { get; set; }
        public string UPPERLIMIT { get; set; }
        public string LOWERLIMIT { get; set; }
        public string Unit { get; set; }
        public string Duration { get; set; }
        public string TestResult { get; set; }

        public Teststep(string stepno,string stepname,string value,string upperlimit,string lowerlimit,string unit,string duration,string testresult)
        {
            this.StepNo = stepno;
            this.StepName = stepname;
            this.Value = value;
            this.UPPERLIMIT = upperlimit;
            this.LOWERLIMIT = lowerlimit;
            this.Unit = unit;
            this.Duration = duration;
            this.TestResult = testresult;
        }
    }
}
