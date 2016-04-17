namespace Sys.Common
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Xml;

    public class XmlHelper : IDisposable
    {
        private readonly string rootName = "Root";
        private string xmlFilePath;
        private XmlDocument xmlDoc;
        public XmlHelper(string pfilePath)
            : this(pfilePath, false)
        {
        }

        public XmlHelper(string pfilePath, bool createFile)
        {
            if (!File.Exists(pfilePath) && createFile)
            {
                this.CreateXmlFile(pfilePath);
            }

            if (!File.Exists(pfilePath))
            {
                throw new Exception(string.Format("xml file ({0}) don't exists.Please check.", pfilePath));
            }

            this.xmlFilePath = pfilePath;
            this.xmlDoc = new XmlDocument();
            this.xmlDoc.Load(pfilePath);
        }

        #region Get
        public bool CheckNodesByXpath(string xpath)
        {
            XmlNode aNode = this.GetXmlNodeByXpath(xpath);
            if (aNode != null)
                return aNode.HasChildNodes;
            else
                return false;
        }

        public string ParseByXpath(string xpath)
        {
            return this.ParseByXpath(xpath, string.Empty);
        }

        public string ParseByXpath(string xpath, string aElment)
        {
            XmlNode aNode = this.GetXmlNodeByXpath(xpath);
            return XmlNodeHelper.ParseByNode(aNode, aElment);
        }

        public XmlNodeList GetXmlNodeListByXpath(string xpath)
        {
            if (xpath.StartsWith("/"))
                xpath = xpath.Substring(1);
            return this.xmlDoc.DocumentElement.SelectNodes(xpath);
        }

        public XmlNode GetXmlNodeByXpath(string xpath)
        {
            if (xpath.StartsWith("/"))
                xpath = xpath.Substring(1);
            return this.xmlDoc.DocumentElement.SelectSingleNode(xpath);
        }

        #endregion

        #region Set

        public XmlNode SetInnerTextByXpath(string xpath, string strValue)
        {
            XmlNode aNode = this.CreateXmlNode(xpath);
            if (aNode != null)
            {
                aNode.InnerText = strValue;
                return aNode;
            }
            else
            {
                throw new Exception(string.Format("The xpath <{0}> don't exists.Please check.", xpath));
            }
        }

        public XmlNode SetAttrValueByXpath(string xpath, string attrName, string strValue)
        {
            XmlNode aNode = this.CreateXmlNodeWithAttr(xpath, attrName, strValue);
            if (aNode != null && !string.IsNullOrEmpty(attrName))
            {
                if (aNode.Attributes[attrName] != null)
                    aNode.Attributes[attrName].Value = strValue;
                else
                {
                    XmlAttribute attr = this.xmlDoc.CreateAttribute(attrName);
                    attr.Value = strValue;
                    aNode.Attributes.Append(attr);
                }
            }
            else
            {
                throw new Exception(string.Format("The xpath <{0}> don't exists.Please check.", xpath));
            }

            return aNode;
        }

        public void SetInnerTextByNode(XmlNode aNode, string strValue)
        {
            if (aNode != null)
            {
                aNode.InnerText = strValue;
            }
            else
            {
                throw new Exception(string.Format("The aNode <{0}> don't exists.Please check.", aNode.Name));
            }
        }

        public void SetAttrValueByNode(XmlNode aNode, string attrName, string strValue)
        {
            if (aNode != null && !string.IsNullOrEmpty(attrName))
            {
                if (aNode.Attributes[attrName] != null)
                    aNode.Attributes[attrName].Value = strValue;
                else
                {
                    XmlAttribute attr = this.xmlDoc.CreateAttribute(attrName);
                    attr.Value = strValue;
                    aNode.Attributes.Append(attr);
                }
            }
            else
            {
                throw new Exception(string.Format("The aNode <{0}> don't exists.Please check.", aNode.Name));
            }
        }

        public void SetChildTextByNode(XmlNode aNode, string childNodePath, string strValue)
        {
            if (aNode != null)
            {
                XmlNode childNode = this.CreateChildXmlNode(aNode, childNodePath);
                childNode.InnerText = strValue;
            }
            else
            {
                throw new Exception(string.Format("The aNode <{0}> don't exists.Please check.", aNode.Name));
            }
        }

        public XmlNode SettChildAttrValueByNode(XmlNode aNode, string childNodePath, string attrName, string strValue)
        {
            if (aNode != null)
            {
                XmlNode childNode = this.CreateChildXmlNodeWithAttr(aNode, childNodePath, attrName, strValue);
                if (!string.IsNullOrEmpty(attrName))
                {
                    if (childNode.Attributes[attrName] != null)
                        childNode.Attributes[attrName].Value = strValue;
                    else
                    {
                        XmlAttribute attr = this.xmlDoc.CreateAttribute(attrName);
                        attr.Value = strValue;
                        childNode.Attributes.Append(attr);
                    }
                }

                return childNode;
            }
            else
            {
                throw new Exception(string.Format("The aNode <{0}> don't exists.Please check.", aNode.Name));
            }
        }

        public void SaveConfig()
        {
            this.xmlDoc.Save(this.xmlFilePath);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (this.xmlDoc != null)
            {
                this.xmlDoc = null;
            }
        }

        #endregion

        #region CreateXmlNode
        private XmlNode CreateXmlNode(string xpath)
        {
            return this.CreateXmlNodeWithAttr(xpath, string.Empty, string.Empty);
        }

        private XmlNode CreateXmlNodeWithAttr(string xpath, string attrName, string strValue)
        {
            if (!xpath.StartsWith("/"))
                xpath = "/" + xpath;

            string searchXpath = string.Format("{0}[@{1}='{2}']", xpath, attrName, strValue);

            if (string.IsNullOrEmpty(attrName))
            {
                searchXpath = xpath;
            }

            XmlNode resultNode = this.GetXmlNodeByXpath(searchXpath);
            if (resultNode == null)
            {
                List<string> listPath = new List<string>();
                string tmpXpath = xpath;

                while (!string.IsNullOrEmpty(tmpXpath))
                {
                    listPath.Insert(0, tmpXpath);
                    tmpXpath = tmpXpath.Substring(0, tmpXpath.LastIndexOf("/"));
                }

                XmlNode firstNode = this.GetXmlNodeByXpath(listPath[0]);
                if (firstNode == null)
                {
                    XmlElement rootNode = this.xmlDoc.CreateElement(listPath[0].Substring(1));
                    this.xmlDoc.DocumentElement.AppendChild(rootNode);
                }
                
                if (listPath.Count == 1)
                {
                    firstNode = this.GetXmlNodeByXpath(listPath[0]);
                    XmlAttribute attr = this.xmlDoc.CreateAttribute(attrName);
                    attr.Value = strValue;
                    firstNode.Attributes.Append(attr);
                }

                for (int i = 1; i < listPath.Count; i++)
                {
                    string item = listPath[i];
                    XmlNode midNode = this.GetXmlNodeByXpath(item);
                    if (midNode == null)
                    {
                        string parentPath = listPath[i - 1];
                        XmlNode parentNode = this.GetXmlNodeByXpath(parentPath);
                        XmlElement tagChild = this.xmlDoc.CreateElement(item.Substring(parentPath.Length + 1));
                        if (!string.IsNullOrEmpty(attrName) && i == listPath.Count - 1)
                        {
                            XmlAttribute attr = this.xmlDoc.CreateAttribute(attrName);
                            attr.Value = strValue;
                            tagChild.Attributes.Append(attr);
                        }

                        parentNode.AppendChild(tagChild);
                    }
                    else if (i == listPath.Count - 1)
                    {
                        if (!string.IsNullOrEmpty(attrName))
                        {
                            string parentPath = listPath[i - 1];
                            XmlNode parentNode = this.GetXmlNodeByXpath(parentPath);
                            XmlElement tagChild = this.xmlDoc.CreateElement(item.Substring(parentPath.Length + 1));
                            XmlAttribute attr = this.xmlDoc.CreateAttribute(attrName);
                            attr.Value = strValue;
                            tagChild.Attributes.Append(attr);
                            parentNode.AppendChild(tagChild);
                        }
                    }
                }

                resultNode = this.GetXmlNodeByXpath(searchXpath);
            }

            return resultNode;
        }

        public XmlNode CreateChildXmlNode(XmlNode aNode, string childNodePath)
        {
            return this.CreateChildXmlNodeWithAttr(aNode, childNodePath, string.Empty, string.Empty);
        }

        public XmlNode CreateChildXmlNodeWithAttr(XmlNode aNode, string childNodePath, string attrName, string strValue)
        {
            if (aNode != null)
            {
                if (!childNodePath.StartsWith("/"))
                    childNodePath = "/" + childNodePath;

                string searchXpath = string.Format("{0}[@{1}='{2}']", childNodePath, attrName, strValue);

                if (string.IsNullOrEmpty(attrName))
                {
                    searchXpath = childNodePath;
                }

                XmlNode resultNode = XmlNodeHelper.GetXmlNodeByNode(aNode, searchXpath);
                if (resultNode == null)
                {
                    List<string> listPath = new List<string>();
                    string tmpXpath = childNodePath;

                    while (!string.IsNullOrEmpty(tmpXpath))
                    {
                        listPath.Insert(0, tmpXpath);
                        tmpXpath = tmpXpath.Substring(0, tmpXpath.LastIndexOf("/"));
                    }

                    for (int i = 0; i < listPath.Count; i++)
                    {
                        string item = listPath[i];
                        XmlNode midNode;
                        if (!string.IsNullOrEmpty(attrName) && i == listPath.Count - 1)
                            midNode = XmlNodeHelper.GetXmlNodeByNode(aNode, string.Format("{0}[@{1}='{2}']", item, attrName, strValue));
                        else
                            midNode = XmlNodeHelper.GetXmlNodeByNode(aNode, item);

                        if (midNode == null)
                        {
                            string parentPath = string.Empty;
                            XmlNode parentNode = aNode;
                            if (i > 0)
                            {
                                parentPath = listPath[i - 1];
                                parentNode = XmlNodeHelper.GetXmlNodeByNode(aNode, parentPath);
                            }

                            XmlElement tagChild = this.xmlDoc.CreateElement(item.Substring(parentPath.Length + 1));
                            if (!string.IsNullOrEmpty(attrName) && i == listPath.Count - 1)
                            {
                                XmlAttribute attr = this.xmlDoc.CreateAttribute(attrName);
                                attr.Value = strValue;
                                tagChild.Attributes.Append(attr);
                            }

                            parentNode.AppendChild(tagChild);
                        }
                    }

                    resultNode = XmlNodeHelper.GetXmlNodeByNode(aNode, searchXpath);
                }

                return resultNode;
            }
            else
                throw new Exception(string.Format("The aNode <{0}> don't exists.Please check.", aNode.Name));
        }
        #endregion

        private void CreateXmlFile(string pFileName)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
            XmlElement rootNode = xmlDoc.CreateElement(this.rootName);
            xmlDoc.InsertBefore(xmlDeclaration, xmlDoc.DocumentElement);
            xmlDoc.AppendChild(rootNode);
            xmlDoc.Save(pFileName);
        }
    }

    public sealed class XmlNodeHelper
    {
        public static string ParseByNode(XmlNode aNode, string aElment)
        {
            string result = null;
            if (aNode != null)
            {
                if (!string.IsNullOrEmpty(aElment))
                {
                    if (aNode[aElment] != null)
                        result = aNode[aElment].InnerText;
                    else if (aNode.Attributes[aElment] != null)
                        result = aNode.Attributes[aElment].Value;
                }
                else
                {
                    result = aNode.Value;
                    if (string.IsNullOrEmpty(result))
                    {
                        result = aNode.InnerText;
                    }
                }
            }

            return result;
        }

        public static XmlNode GetXmlNodeByNode(XmlNode aNode, string childNodePath)
        {
            if (aNode != null && !string.IsNullOrEmpty(childNodePath))
            {
                if (childNodePath.StartsWith("/"))
                    childNodePath = childNodePath.Substring(1);
                return aNode.SelectSingleNode(childNodePath);
            }
            else
                return aNode;
        }
    }
}
