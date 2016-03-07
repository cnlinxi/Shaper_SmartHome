using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WebApiSample.Common
{
    public class ToastHelper
    {
        /// <summary>
        /// 从toast中取出内容使用的xml工具
        /// </summary>
        /// <param name="xmlContent">xml</param>
        /// <returns>toast中的内容</returns>
        public static string ParseXml(string xmlContent)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlContent);
            XmlNode toast = doc.FirstChild;
            XmlNode visual = toast.FirstChild;
            XmlNode binding = visual.FirstChild;
            foreach (XmlNode node in binding.ChildNodes)
            {
                if (node.Name == "text" && node.InnerText != "Shaper智能家居")
                    return node.InnerText;
            }
            return string.Empty;
        }
    }
}
