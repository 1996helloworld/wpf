using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Ezhu.AutoUpdater
{
    public class XMLHelper
    {
        public static string Read(string key)
        {
            try
            {
                XmlDocument doc = new XmlDocument();

                // 获得配置文件的全路径　　  
                string strFileName = "Config.xml";
                doc.Load(strFileName);
                XmlNode node = doc.SelectSingleNode("Config");
                XmlNodeList nodes = node.ChildNodes;
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (nodes[i].Name == key)
                    {
                        return nodes[i].InnerText;
                    }
                }
                return "";
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        /// <summary>
        /// 更新XML中指定节点的值
        /// </summary>
        /// <param name="NodeName">需要更改的节点</param>
        /// <param name="NodeValue">需要更新的节点值</param>
        public static void Update(string NodeName, string NodeValue,string url)
        {
            try
            {
                XmlDocument doc = new XmlDocument();

                // 获得配置文件的全路径　　  
                string strFileName = "Config.xml";
                doc.Load(url);
                XmlNode node = doc.SelectSingleNode("Config");
                XmlNodeList nodes = node.ChildNodes;
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (nodes[i].Name == NodeName)
                    {
                        nodes[i].InnerText = NodeValue;
                        doc.Save(strFileName);
                    }
                }
                doc.Save(strFileName);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
