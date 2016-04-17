using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string cid = "OCBC";
            string strId = "admin";
            string strPassword = "ocbcwh56";

            string postData = "<?xml version='1.0' encoding='iso-8859-1'?><!DOCTYPE jds SYSTEM '/home/httpd/html/dtd/jds2.dtd'><jds><account acid='" + cid + "'loginid='" + strId + "' passwd='" + strPassword + "'>{0}</account></jds>";
            //string postData = "<account acid='" + cid + "'loginid='" + strId + "' passwd='" + strPassword + "'><change_pwd>NEW_PASSWORD</change_pwd></account>";
            Encoding encoding = Encoding.UTF8;
            byte[] data = encoding.GetBytes(postData);
            System.Net.HttpWebRequest req = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create("https://smsmgr.three.com.mo/servlet/corpsms.jdsXMLClient2");
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentType = "text/XML";//SOAP
            req.ContentLength = data.Length;
            System.IO.Stream newStream = req.GetRequestStream();
            //   发送数据   
            newStream.Write(data, 0, data.Length);
            System.Net.HttpWebResponse res = (System.Net.HttpWebResponse)req.GetResponse();
            MessageBox.Show(res.StatusDescription.ToString());
            newStream.Close();

            //// 获取响应
            HttpWebResponse myResponse = (HttpWebResponse)req.GetResponse();
            StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);
            string content = reader.ReadToEnd();
        }
    }
}
