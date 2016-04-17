using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace ReadRTF
{
    public partial class frmByFolder : Form
    {
        public frmByFolder()
        {
            InitializeComponent();
        }

        private void frmByFolder_Load(object sender, EventArgs e)
        {
#if DEBUG
            this.txtFFN.Text = @"C:\Users\WangWei\Documents\UMAC\Thesis\2012\女性";
            this.InitListBox(this.txtFFN.Text);
#endif
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            if (this.fbdRTF.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.txtFFN.Text = this.fbdRTF.SelectedPath;
                this.InitListBox(this.txtFFN.Text);
            }
        }

        private void InitListBox(string strFolder)
        {
            List<string> listFiles = new List<string>();
            listFiles.AddRange(Directory.GetFiles(strFolder, "*.rtf", SearchOption.TopDirectoryOnly));
            foreach (var item in listFiles)
            {
                this.listboxFile.Items.Add(item);
                //if (this.listboxFile.Items.Count > 100)
                //    break;
            }

#if DEBUG
            foreach (var item in listFiles)
            {
                this.ConvertFromRTF(item);
            }
#endif
        }

        private void ConvertFromRTF(string strFFN)
        {
            // Get the contents of the RTF file. When the contents of the file are   
            // stored in the string (rtfText), the contents are encoded as UTF-16.  
            string rtfText = System.IO.File.ReadAllText(strFFN);

            // Display the RTF text. This should look like the contents of your file.
            //    System.Windows.Forms.MessageBox.Show(rtfText);

            // Use the RichTextBox to convert the RTF code to plain text.
            this.rtbResult.Rtf = rtfText;
            List<string> listContent = new List<string>();
            listContent.AddRange(this.rtbResult.Lines);
            string xmlFile = Path.Combine(Path.GetDirectoryName(strFFN), "00output.xml");
            string itemName = Path.GetFileNameWithoutExtension(strFFN);
            using (Sys.Common.XmlHelper xmlout = new Sys.Common.XmlHelper(xmlFile, true))
            {
                XmlNode curNode = xmlout.SetAttrValueByXpath("ITEMS/ITEM", "FILENAME", itemName);
                List<string> lstBaseInfo = new List<string>();
                List<string> lstProcess = new List<string>();
                List<string> lstSymptom = new List<string>();
                List<string> lstWesternDiagnosis = new List<string>();
                List<string> lstChineseDiagnosis = new List<string>();
                List<string> lstTreat = new List<string>();
                List<string> lstWesternMedicine = new List<string>();
                List<string> lstChineseMedicine = new List<string>();

                #region lstBaseInfo
                int lineNo = 0;
                foreach (var currentLine in listContent)
                {
                    lineNo++;
                    if (string.IsNullOrEmpty(currentLine.Trim()))
                        continue;

                    lstBaseInfo.Add(currentLine);
                    if (currentLine.StartsWith("地址"))
                    {
                        break;
                    }
                }
                #endregion

                #region lstProcess
                listContent.RemoveRange(0, lineNo);
                lineNo = 0;
                foreach (var currentLine in listContent)
                {
                    if (!string.IsNullOrEmpty(currentLine.Trim()))
                    {
                        if (currentLine.StartsWith("现"))
                        {
                            break;
                        }

                        lstProcess.Add(currentLine);
                    }

                    lineNo++;
                }
                #endregion

                #region lstSymptom
                listContent.RemoveRange(0, lineNo);
                lineNo = 0;
                foreach (var currentLine in listContent)
                {
                    if (!string.IsNullOrEmpty(currentLine.Trim()))
                    {
                        if (currentLine.Contains("西医诊断："))
                        {
                            break;
                        }

                        lstSymptom.Add(currentLine);
                    }

                    lineNo++;
                }
                #endregion

                #region lstWesternDiagnosis
                listContent.RemoveRange(0, lineNo);
                lineNo = 0;
                foreach (var currentLine in listContent)
                {
                    if (!string.IsNullOrEmpty(currentLine.Trim()))
                    {
                        if (currentLine.Contains("中医诊断："))
                        {
                            break;
                        }

                        lstWesternDiagnosis.Add(currentLine);
                    }

                    lineNo++;
                }
                #endregion

                #region lstChineseDiagnosis
                listContent.RemoveRange(0, lineNo);
                lineNo = 0;
                foreach (var currentLine in listContent)
                {
                    if (!string.IsNullOrEmpty(currentLine.Trim()))
                    {
                        if (currentLine.StartsWith("处理："))
                        {
                            break;
                        }

                        lstChineseDiagnosis.Add(currentLine);
                    }

                    lineNo++;
                }
                #endregion

                #region lstTreat
                listContent.RemoveRange(0, lineNo);
                lineNo = 0;
                foreach (var currentLine in listContent)
                {
                    if (!string.IsNullOrEmpty(currentLine.Trim()))
                    {
                        if (currentLine.Contains("西药处方："))
                        {
                            break;
                        }

                        lstTreat.Add(currentLine);
                    }

                    lineNo++;
                }
                #endregion

                #region lstWesternMedicine
                listContent.RemoveRange(0, lineNo);
                lineNo = 0;
                foreach (var currentLine in listContent)
                {
                    if (!string.IsNullOrEmpty(currentLine.Trim()))
                    {
                        if (currentLine.Contains("中药处方："))
                        {
                            break;
                        }

                        lstWesternMedicine.Add(currentLine);
                    }

                    lineNo++;
                }
                #endregion

                #region lstChineseMedicine
                listContent.RemoveRange(0, lineNo);
                lineNo = 0;
                foreach (var currentLine in listContent)
                {
                    if (!string.IsNullOrEmpty(currentLine.Trim()))
                    {
                        if (currentLine.EndsWith("医师：吴万垠"))
                        {
                            break;
                        }

                        lstChineseMedicine.Add(currentLine);
                    }

                    lineNo++;
                }
                #endregion

                xmlout.SetChildTextByNode(curNode, "BaseInfo", this.RemoveWord(string.Join(Environment.NewLine, lstBaseInfo)));
                xmlout.SetChildTextByNode(curNode, "Process", this.RemoveWord(string.Join(Environment.NewLine, lstProcess)));
                xmlout.SetChildTextByNode(curNode, "Symptom", this.RemoveWord(string.Join(Environment.NewLine, lstSymptom)));
                xmlout.SetChildTextByNode(curNode, "WesternDiagnosis", this.RemoveWord(string.Join(Environment.NewLine, lstWesternDiagnosis)));
                xmlout.SetChildTextByNode(curNode, "ChineseDiagnosis", this.RemoveWord(string.Join(Environment.NewLine, lstChineseDiagnosis)));
                xmlout.SetChildTextByNode(curNode, "Treat", this.RemoveWord(string.Join(Environment.NewLine, lstTreat)));
                xmlout.SetChildTextByNode(curNode, "WesternMedicine", this.RemoveWord(string.Join(Environment.NewLine, lstWesternMedicine)));
                xmlout.SetChildTextByNode(curNode, "ChineseMedicine", this.RemoveWord(string.Join(Environment.NewLine, lstChineseMedicine)));

                xmlout.SaveConfig();
            }

            #region Save to text file
            //string plainText = this.rtbResult.Text;
            //FileInfo fi = new FileInfo(strFFN);
            //string outFFN = Path.Combine(Path.GetDirectoryName(strFFN), Path.GetFileNameWithoutExtension(strFFN) + "output.txt");
            //// Output the plain text to a file, encoded as UTF-8. 
            //System.IO.File.WriteAllText(outFFN, plainText);

            #endregion

        }

        private string RemoveWord(string strInput)
        {
            string strResult = strInput;
            string[] replaceStrs = new string[] { "西医诊断：", "中医诊断：", "处理：", "西药处方：", "中药处方："
                , "建议浏览博客网站：", "吴万垠教授博客：blog.sina.com.cn/drwuwanyin", "咨询网站：wuwanyin.haodf.com" };
            foreach (var item in replaceStrs)
            {
                strResult = strResult.Replace(item, string.Empty);
            }

            //delete multiple lines
            strResult = Regex.Replace(strResult, @"^\s+${2,}", Environment.NewLine, RegexOptions.Multiline).Trim();
            return strResult;
        }
        private void listboxFile_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listboxFile.SelectedIndex >= 0)
            {
                this.ConvertFromRTF(this.listboxFile.SelectedItem.ToString());
            }
        }

    }
}
