using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace PhotoManage
{
    public partial class frmMain : Form
    {
        private string m_rootDist = @"Z:\Share4VM_C\Temp";
        private string rootDisk
        {
            get
            {
                if (string.IsNullOrEmpty(this.m_rootDist))
                {
                    if (Path.IsPathRooted(this.txtDir.Text))
                        this.m_rootDist = Path.Combine(Path.GetPathRoot(this.txtDir.Text), "DuoDuo");
                    else
                        this.m_rootDist = @"Z:\";
                }

                return this.m_rootDist;
            }
        }

        public frmMain()
        {
            InitializeComponent();
        }

        private List<EntPicM> listPicM = new List<EntPicM>();
        private List<EntVedioM> listVedioM = new List<EntVedioM>();
        private void txtDir_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.dgvResult.DataSource = null;
                if (this.chkPic.Checked)
                {
                    #region Picture

                    string strDir = this.txtDir.Text.Trim();
                    Dictionary<string, DateTime> dicFiles = new Dictionary<string, DateTime>();
                    if (Directory.Exists(strDir))
                    {
                        listPicM.Clear();
                        string[] strFiles = Directory.GetFiles(strDir, "*.jp*g", SearchOption.AllDirectories);
                        foreach (string itemFile in strFiles)
                        {
                            if (!itemFile.Contains("/2008/") && !itemFile.Contains("/2009/")
                                && !itemFile.Contains("/2010/") && !itemFile.Contains("/2011/")
                                && !itemFile.Contains("/BackUp/") && !itemFile.Contains("/Other/")
                                && !itemFile.Contains("NoTime") && !itemFile.Contains("幼儿园"))
                            {
                                EntPicM entPM = new EntPicM();
                                entPM.FileFullName = itemFile;

                                using (FileStream stream = new FileStream(itemFile, FileMode.Open, FileAccess.Read))
                                {
                                    using (System.Drawing.Image image = System.Drawing.Image.FromStream(stream, true, false))
                                    {
                                        entPM.Width = image.Width;
                                        entPM.Higth = image.Height;
                                        StringBuilder sbOther = new StringBuilder();
                                        Encoding ascii = Encoding.ASCII;
                                        foreach (PropertyItem p in image.PropertyItems)
                                        {
                                            switch (p.Id)
                                            {
                                                case 0x9004:
                                                    entPM.TakeTime1 = ascii.GetString(p.Value);
                                                    break;
                                                case 0x132:
                                                    entPM.TakeTime = ascii.GetString(p.Value);
                                                    break;
                                                case 0x9003:
                                                    entPM.TakeTime2 = ascii.GetString(p.Value);
                                                    break;
                                                //case 0x100:
                                                //    entPM.ImageWidth = p.Type.ToString();
                                                //    break;
                                                //case 0x101:
                                                //    entPM.ImageHigth = p.Type.ToString();
                                                //    break;
                                                case 0x110:
                                                    entPM.EquipModel = ascii.GetString(p.Value);
                                                    break;
                                                //case 0xa002:
                                                //    entPM.ExifImageWidth = convertToInt16U(p.Value);
                                                //    break;
                                                //case 0xa003:
                                                //    entPM.ExifImageHeight = convertToInt16U(p.Value);
                                                //    break;
                                                default:
                                                    if (p.Type == 0x2)
                                                    {
                                                        sbOther.AppendFormat("{0}:{1};", p.Id.ToString("x"), ascii.GetString(p.Value));
                                                    }
                                                    else if (p.Type == 0x3)
                                                    {
                                                        switch (p.Id)
                                                        {
                                                            case 0x8827:
                                                            case 0xA217:
                                                            case 0x8822:
                                                            case 0x9207:
                                                            case 0x9208:
                                                            case 0x9209:
                                                                break;
                                                            default:
                                                                sbOther.AppendFormat("{0}:{1}||", p.Id.ToString("x"), convertToInt16U(p.Value));
                                                                break;
                                                        }
                                                    }
                                                    break;
                                            }
                                        }

                                        entPM.Other = sbOther.ToString();
                                    }
                                }

                                if (entPM.TakeTime2 == null || string.IsNullOrEmpty(entPM.TakeTime2.ToString()))
                                {
                                    if (!(entPM.TakeTime1 == null || string.IsNullOrEmpty(entPM.TakeTime1.ToString())))
                                        entPM.TakeTime2 = entPM.TakeTime1;
                                }

                                if (!(entPM.TakeTime2 == null || string.IsNullOrEmpty(entPM.TakeTime2.ToString())))
                                    entPM.TakeTime = entPM.TakeTime2;

                                if (entPM.TakeTime == null || string.IsNullOrEmpty(entPM.TakeTime.ToString())
                                    || entPM.TakeTime.ToString().Equals(@"\0") || entPM.TakeTime.ToString().Length < 4)
                                {
                                    FileInfo fi = new FileInfo(itemFile);
                                    File.Copy(itemFile, Path.Combine(this.rootDisk, @"NoTime\" + fi.Name), true);

                                    entPM.TakeTime = File.GetLastWriteTime(itemFile).ToString("yyyyMMddHHmmss");
                                }

                                string strTakeTime = entPM.TakeTime.ToString().Replace(":", "");
                                if (entPM.TakeTime.ToString().Length > 10)
                                    strTakeTime = entPM.TakeTime.ToString().Substring(0, 10).Replace(":", "");
                                if (strTakeTime.CompareTo("20121001") > 0)
                                    listPicM.Add(entPM);
                            }
                        }
                    }

                    foreach (KeyValuePair<string, DateTime> kvp in dicFiles)
                    {
                        //File.SetLastWriteTime(kvp.Key, kvp.Value);
                    }

                    this.dgvResult.DataSource = listPicM;
                    #endregion
                }
                else
                {
                    string strDir = this.txtDir.Text.Trim();
                    if (Directory.Exists(strDir))
                    {
                        listVedioM.Clear();
                        string[] strFiles = Directory.GetFiles(strDir, "*.*", SearchOption.AllDirectories);
                        foreach (string itemFile in strFiles)
                        {
                            if (itemFile.ToUpper().Contains(".JPG") || itemFile.Contains("幼儿园") || itemFile.Contains("Thumbs.db"))
                            {
                                continue;
                            }

                            EntVedioM entVM = new EntVedioM();
                            entVM.FileFullName = itemFile;
                            entVM.LastModifyDT = File.GetLastWriteTime(itemFile);
                            string dateStr = entVM.LastModifyDT.ToString("yyyyMMddHHmmss");
                            if (entVM.FileName.Replace("VID_", "").Replace("meipai_", string.Empty).Replace("_", "").StartsWith(dateStr.Substring(0, 11)))
                            {
                                dateStr = entVM.FileName.Replace("VID_", "").Replace("(", "_").Replace(")", "");
                                if (dateStr.Substring(8, 1).Equals("_"))
                                {
                                    dateStr = dateStr.Substring(0, 8) + dateStr.Substring(9);
                                }
                            }
                            else if (entVM.FileName.StartsWith("meipai_"))
                            {
                                dateStr = entVM.FileName.Replace("meipai_", string.Empty);
                            }

                            entVM.FileNameDate = dateStr;
                            if (dateStr.CompareTo("20121001") > 0)
                                listVedioM.Add(entVM);
                        }
                    }

                    this.dgvResult.DataSource = listVedioM;
                }
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            this.txtDir.Text = @"Z:\Share4VM_C\Temp";

            //this.chkPic.Checked = false;
            //this.txtDir_KeyDown(this.txtDir, new KeyEventArgs(Keys.Enter));
        }

        private uint convertToInt16U(byte[] arr)
        {
            if (arr.Length != 2)
                return 0;
            else
                return Convert.ToUInt16(arr[1] << 8 | arr[0]);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (this.chkPic.Checked)
            {
                foreach (var currEnt in this.listPicM)
                {
                    if (!string.IsNullOrEmpty(currEnt.FileFullName))
                    {
                        string dateStr = currEnt.TakeTime.Trim(new char[] { '\0' }).Replace(" ", "").Replace(":", "");
                        if (currEnt.FileName.Replace("IMG_", "").Replace("_", "").StartsWith(dateStr.Substring(0, 11)))
                        {
                            dateStr = currEnt.FileName.Replace("IMG_", "").Replace("(", "_").Replace(")", "").Replace("_bestshot", "").Replace("_HDR", "").Replace("DSC_", "").Replace("_LLS", "");
                            if (dateStr.Substring(8, 1).Equals("_"))
                            {
                                dateStr = dateStr.Substring(0, 8) + dateStr.Substring(9);
                            }
                        }


                        string destDir = Path.Combine(Path.Combine(this.rootDisk, @"Renamed"), dateStr.Substring(0, 4));
                        string backDir = Path.Combine(Path.Combine(this.rootDisk, @"Backup"), currEnt.ParentDir);
                        if (!Directory.Exists(destDir))
                            Directory.CreateDirectory(destDir);
                        if (!Directory.Exists(backDir))
                            Directory.CreateDirectory(backDir);

                        string destFileName = Path.Combine(destDir, dateStr + currEnt.FileExt);
                        File.Copy(currEnt.FileFullName, destFileName, true);
                        File.Copy(currEnt.FileFullName, Path.Combine(backDir, currEnt.FileName + currEnt.FileExt), true);
                        try
                        {
                            if (File.Exists(destFileName))
                                File.Delete(currEnt.FileFullName);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }

                this.listPicM.Clear();
                this.dgvResult.DataSource = null;
                MessageBox.Show("OK");
            }
            else
            {
                string strDir = Path.Combine(this.rootDisk, "Renamed");
                foreach (var currEnt in this.listVedioM)
                {
                    if (!string.IsNullOrEmpty(currEnt.FileFullName))
                    {
                        string strDateFileName = currEnt.FileNameDate;
                        string destName = Path.Combine(strDir, strDateFileName + currEnt.FileExt);
                        //if (fi.Length > 1000000000)
                        //{
                        //    System.Diagnostics.Debug.WriteLine(string.Format("SizeContinue:Index,{0};Name,{1};Date,{2};Len,{3}", row.Index, row.Cells[2].Value, row.Cells[3].Value, fi.Length));
                        //    continue;
                        //}

                        if (!File.Exists(destName))
                        {
                            File.Copy(currEnt.FileFullName, destName, false);
                        }
                        else
                        {
                            MessageBox.Show(string.Format("FileExists:Name,{0};Date,{1};", currEnt.FileName, currEnt.FileNameDate));
                            System.Diagnostics.Debug.WriteLine(string.Format("FileExists:Name,{0};Date,{1};", currEnt.FileName, currEnt.FileNameDate));
                        }
                    }
                }

                this.listVedioM.Clear();
                this.dgvResult.DataSource = null;
                MessageBox.Show("OK");
            }
        }

        private void btnMove_Click(object sender, EventArgs e)
        {
            string strDest = @"Z:\TEMP\tmp\xz";
            string strSrc = this.txtDir.Text.Trim();
            if (Directory.Exists(strSrc))
            {
                string[] strFiles = Directory.GetFiles(strSrc, "*.*", SearchOption.AllDirectories);
                foreach (string itemFile in strFiles)
                {
                    FileInfo fi = new FileInfo(itemFile);
                    string dirName = fi.Directory.Name;
                    string destName = Path.Combine(strDest, string.Format("{0}_{1}", dirName, fi.Name));
                    File.Move(itemFile, destName);
                }
            }
        }

        private void btnBrowser_Click(object sender, EventArgs e)
        {
            string setDir = this.txtDir.Text.Trim();
            if (Directory.Exists(setDir))
            {
                this.fbdSrc.SelectedPath = setDir;
                if (this.fbdSrc.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string selPath = this.fbdSrc.SelectedPath;
                    if (Directory.Exists(selPath))
                    {
                        this.txtDir.Text = selPath;
                        this.txtDir_KeyDown(this.txtDir, new KeyEventArgs(Keys.Enter));
                    }
                }
            }
        }
    }
}
