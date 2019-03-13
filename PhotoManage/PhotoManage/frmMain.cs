using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace PhotoManage
{
    public partial class frmMain : Form
    {
        private string finalDirectory = @"E:\Duoduo\PictureAndVideos";
        private string m_rootDisk = @"E:\Duoduo\Temp";
        private string rootDisk
        {
            get
            {
                if (string.IsNullOrEmpty(this.m_rootDisk))
                {
                    if (Path.IsPathRooted(this.txtDir.Text))
                        this.m_rootDisk = Path.Combine(Path.GetPathRoot(this.txtDir.Text), "DuoDuo");
                    else
                        this.m_rootDisk = @"E:\Duoduo\Temp";
                }

                return this.m_rootDisk;
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
                bool showInfo = this.ckbShowInfo.Checked;
                this.dgvResult.DataSource = null;
                string strDir = this.txtDir.Text.Trim();
                if (!Directory.Exists(strDir))
                {
                    MessageBox.Show("Dir not exist:" + strDir);
                    return;
                }

                if (this.chkPic.Checked)
                {
                    #region Picture
                    List<EntPicM> lstPicM = new List<EntPicM>();
                    //Dictionary<string, DateTime> dicFiles = new Dictionary<string, DateTime>();
                    listPicM.Clear();
                    string[] strFiles = Directory.GetFiles(strDir, "*.jp*g", SearchOption.AllDirectories);
                    int cnt = 0;
                    foreach (string itemFile in strFiles)
                    {
                        if (!itemFile.Contains("/2008/") && !itemFile.Contains("/2009/")
                            && !itemFile.Contains("/2010/") && !itemFile.Contains("/2011/")
                            && !itemFile.Contains("/BackUp/") && !itemFile.Contains("/Other/")
                            && !itemFile.Contains("NoTime") && !itemFile.Contains("幼儿园"))
                        {
                            EntPicM entPM = this.Init_fr_file(itemFile);
                            //if (entPM.TakeTime_string.CompareTo("20121001") > 0)
                            lstPicM.Add(entPM);
                        }

                        if (lstPicM.Count > 100)
                        {
                            if (showInfo)
                            {
                                this.listPicM.AddRange(lstPicM);
                                break;
                            }
                            else
                            {
                                cnt += lstPicM.Count;
                                this.DoSavePic(lstPicM);
                                System.Diagnostics.Debug.WriteLine(string.Format("process:{0}", cnt));
                                lstPicM.Clear();
                            }
                        }
                    }

                    //foreach (KeyValuePair<string, DateTime> kvp in dicFiles)
                    //{
                    //    //File.SetLastWriteTime(kvp.Key, kvp.Value);
                    //}

                    if (showInfo)
                    {
                        this.dgvResult.DataSource = listPicM;
                    }
                    else
                    {
                        this.DoSavePic(lstPicM);
                        MessageBox.Show("OK");
                    }

                    #endregion
                }
                else
                {
                    listVedioM.Clear();
                    List<EntVedioM> lstVedioM = new List<EntVedioM>();
                    string[] strFiles = Directory.GetFiles(strDir, "*.*", SearchOption.AllDirectories);
                    foreach (string itemFile in strFiles)
                    {
                        if (itemFile.ToUpper().Contains(".JPG") || itemFile.ToUpper().Contains(".PNG") || itemFile.Contains("幼儿园") || itemFile.Contains("Thumbs.db"))
                        {
                            continue;
                        }

                        if (!itemFile.ToUpper().Contains(".MP4") && !itemFile.ToUpper().Contains(".MOV"))
                            continue;

                        EntVedioM entVM = Vedio_Init_fr_File(itemFile);
                        //if (dateStr.CompareTo("20121001") > 0)
                        lstVedioM.Add(entVM);
                        if (lstVedioM.Count > 100)
                        {
                            if (showInfo)
                            {
                                this.listVedioM.AddRange(lstVedioM);
                                break;
                            }
                            else
                            {
                                this.DoSaveVideo(lstVedioM);
                                System.Diagnostics.Debug.WriteLine(string.Format("process:{0}", lstVedioM.Count));
                                lstVedioM.Clear();
                            }
                        }
                    }

                    if (ckbShowInfo.Checked)
                    {
                        this.dgvResult.DataSource = lstVedioM;
                    }
                    else
                    {
                        this.DoSaveVideo(lstVedioM);
                        MessageBox.Show("OK");
                    }
                }
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            this.txtDir.Text = @"E:\Duoduo\Temp\Temp";

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

        private EntPicM Init_fr_file(string fileFullName)
        {
            EntPicM entPM = new EntPicM();
            entPM.FileFullName = fileFullName;

            using (FileStream stream = new FileStream(fileFullName, FileMode.Open, FileAccess.Read))
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
                                entPM.EquipModel = ascii.GetString(p.Value).Replace("\0", string.Empty);
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
                FileInfo fi = new FileInfo(fileFullName);
                entPM.TakeTime = File.GetLastWriteTime(fileFullName).ToString("yyyyMMddHHmmss");
            }

            entPM.TakeTime_string = entPM.TakeTime.ToString().Replace(":", "");
            if (entPM.TakeTime.ToString().Length > 10)
                entPM.TakeTime_string = entPM.TakeTime.ToString().Substring(0, 10).Replace(":", "");

            return entPM;
        }

        private EntVedioM Vedio_Init_fr_File(string fileFullName)
        {
            EntVedioM entVM = new EntVedioM();
            entVM.FileFullName = fileFullName;
            entVM.LastModifyDT = File.GetLastWriteTime(fileFullName);
            string dateStr = entVM.LastModifyDT.ToString("yyyyMMddHHmmss");
            //if (entVM.FileName.Replace("VID_", "").Replace("meipai_", string.Empty).Replace("_", "").StartsWith(dateStr.Substring(0, 11)))
            //{
            //    dateStr = entVM.FileName.Replace("VID_", "").Replace("(", "_").Replace(")", "");
            //    if (dateStr.Substring(8, 1).Equals("_"))
            //    {
            //        dateStr = dateStr.Substring(0, 8) + dateStr.Substring(9);
            //    }
            //}
            //else if (entVM.FileName.StartsWith("meipai_"))
            //{
            //    dateStr = entVM.FileName.Replace("meipai_", string.Empty);
            //}

            entVM.FileNameDate = dateStr;
            return entVM;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (this.chkPic.Checked)
            {
                this.DoSavePic(this.listPicM);
                this.listPicM.Clear();
                this.dgvResult.DataSource = null;
                MessageBox.Show("OK");
            }
            else
            {
                this.DoSaveVideo(this.listVedioM);
                this.listVedioM.Clear();
                this.dgvResult.DataSource = null;
                MessageBox.Show("OK");
            }
        }

        private void DoSavePic(List<EntPicM> lstPicM, bool checkDuplicate = false)
        {
            if (lstPicM.Count == 0)
                return;
            System.Diagnostics.Debug.WriteLine(lstPicM.FirstOrDefault().FileFullName);
            string strDir = Path.Combine(this.rootDisk, "Renamed");
            string strBackup = Path.Combine(this.rootDisk, "Backup");
            if (!Directory.Exists(strBackup))
                Directory.CreateDirectory(strBackup);
            string doubleCheckDir = Path.Combine(this.rootDisk, "DoubleCheck");
            if (!Directory.Exists(doubleCheckDir))
                Directory.CreateDirectory(doubleCheckDir);

            foreach (var currEnt in lstPicM)
            {
                if (File.Exists(currEnt.FileFullName))
                {
                    string finalDirWithYear = Path.Combine(this.finalDirectory, currEnt.FileName.Substring(0, 4));
                    if (!Directory.Exists(finalDirWithYear))
                        Directory.CreateDirectory(finalDirWithYear);

                    string renameDirWithYear = Path.Combine(strDir, currEnt.FileName.Substring(0, 4));
                    if (!Directory.Exists(renameDirWithYear))
                        Directory.CreateDirectory(renameDirWithYear);

                    string ffnFinal = Path.Combine(finalDirWithYear, currEnt.FileName + currEnt.FileExt);
                    string ffnRename = Path.Combine(renameDirWithYear, currEnt.FileName + currEnt.FileExt);
                    string ffnDoubleCheck = Path.Combine(doubleCheckDir, currEnt.FileName + currEnt.FileExt);

                    if (!File.Exists(ffnFinal) && !File.Exists(ffnDoubleCheck) && !File.Exists(ffnRename))
                    {
                        if (!checkDuplicate && File.Exists(currEnt.FileFullName))
                            File.Move(currEnt.FileFullName, ffnRename);
                    }
                    else
                    {
                        //if (File.Exists(ffnFinal))
                        //{
                        //    EntPicM checkFinalPM = this.Init_fr_file(ffnFinal);
                        //    if (!currEnt.CheckEqualEquipModel(checkFinalPM))
                        //    {
                        //        File.Move(currEnt.FileFullName, ffnRename);
                        //        continue;
                        //    }
                        //}
                        List<EntPicM> lstCheckPM = new List<EntPicM>();
                        //Directory.GetFiles(doubleCheckDir, currEnt.FileName + "*").Select(ff => this.Init_fr_file(ff)).ToList<EntPicM>();
                        lstCheckPM.AddRange(Directory.GetFiles(doubleCheckDir, currEnt.FileName + "*").Select(ff => this.Init_fr_file(ff)));
                        lstCheckPM.AddRange(Directory.GetFiles(strBackup, currEnt.FileName + "*").Select(ff => this.Init_fr_file(ff)));
                        //if (File.Exists(ffnDoubleCheck))
                        //    lstCheckPM.Add(this.Init_fr_file(ffnDoubleCheck));
                        if (File.Exists(ffnFinal))
                            lstCheckPM.Add(this.Init_fr_file(ffnFinal));
                        if (File.Exists(ffnRename))
                            lstCheckPM.Add(this.Init_fr_file(ffnRename));

                        bool withSameItem = false;

                        string backupName = Path.Combine(strBackup, currEnt.FileName + currEnt.FileExt);
                        foreach (var checkPM in lstCheckPM)
                        {
                            if (checkPM.Equals(currEnt))
                            {
                                bool existBackupOrDuplicate = File.Exists(backupName);
                                if (!existBackupOrDuplicate)
                                    existBackupOrDuplicate = Directory.GetFiles(doubleCheckDir, currEnt.FileName + "*").Length > 1;

                                if (existBackupOrDuplicate)
                                {
                                    System.Diagnostics.Debug.WriteLine("delete:" + currEnt.FileFullName);
                                    File.Delete(currEnt.FileFullName);
                                    withSameItem = true;
                                    break;
                                }
                            }
                        }

                        if (!withSameItem && File.Exists(currEnt.FileFullName))
                        {
                            int i = 0;
                            while (File.Exists(backupName))
                            {
                                i++;
                                backupName = Path.Combine(strBackup, string.Format("{0}_{1}{2}", currEnt.FileName, i, currEnt.FileExt));
                            }

                            //System.Diagnostics.Debug.WriteLine(string.Format("Move: {0} to {1}", currEnt.FileFullName, backupName));
                            File.Move(currEnt.FileFullName, backupName);
                        }

                        if (!checkDuplicate && !withSameItem)
                        {
                            int begIndex = Directory.GetFiles(doubleCheckDir, currEnt.FileName + "*").Length;
                            List<string> lstMoveFile = new List<string>();
                            //move the final file to double check dir
                            if (File.Exists(ffnFinal))
                                lstMoveFile.Add(ffnFinal);
                            if (File.Exists(backupName))
                                lstMoveFile.Add(backupName);
                            //move the rename file to double check dir
                            if (File.Exists(ffnRename))
                                lstMoveFile.Add(ffnRename);
                            //move current file double check dir
                            if (File.Exists(currEnt.FileFullName))
                                lstMoveFile.Add(currEnt.FileFullName);

                            foreach (var itmMoveFile in lstMoveFile)
                            {
                                string tmpFileName = Path.Combine(doubleCheckDir, string.Format("{0}{1}", currEnt.FileName, currEnt.FileExt));
                                while (File.Exists(tmpFileName))
                                {
                                    tmpFileName = Path.Combine(doubleCheckDir, string.Format("{0}_{1}{2}", currEnt.FileName, ++begIndex, currEnt.FileExt));
                                }

                                File.Move(itmMoveFile, tmpFileName);
                            }
                        }
                    }
                }
            }
        }

        private void DoSaveVideo(List<EntVedioM> lstVedioM, bool checkDuplicate = false)
        {
            if (lstVedioM.Count == 0)
                return;
            System.Diagnostics.Debug.WriteLine(lstVedioM.FirstOrDefault().FileFullName);
            string strDir = Path.Combine(this.rootDisk, @"Video\Renamed");
            string strBackup = Path.Combine(this.rootDisk, @"Video\Backup");
            if (!Directory.Exists(strBackup))
                Directory.CreateDirectory(strBackup);
            string doubleCheckDir = Path.Combine(this.rootDisk, @"Video\DoubleCheck");
            if (!Directory.Exists(doubleCheckDir))
                Directory.CreateDirectory(doubleCheckDir);

            foreach (var currEnt in lstVedioM)
            {
                if (File.Exists(currEnt.FileFullName))
                {
                    string finalDirWithYear = Path.Combine(this.finalDirectory, @"Videos\" + currEnt.FileNameDate.Substring(0, 4));
                    if (!Directory.Exists(finalDirWithYear))
                        Directory.CreateDirectory(finalDirWithYear);

                    string renameDirWithYear = Path.Combine(strDir, currEnt.FileNameDate.Substring(0, 4));
                    if (!Directory.Exists(renameDirWithYear))
                        Directory.CreateDirectory(renameDirWithYear);

                    string ffnFinal = Path.Combine(finalDirWithYear, currEnt.FileNameDate + currEnt.FileExt);
                    string ffnRename = Path.Combine(renameDirWithYear, currEnt.FileNameDate + currEnt.FileExt);
                    string ffnDoubleCheck = Path.Combine(doubleCheckDir, currEnt.FileNameDate + currEnt.FileExt);

                    if (!File.Exists(ffnFinal) && !File.Exists(ffnDoubleCheck) && !File.Exists(ffnRename))
                    {
                        //File.Copy(currEnt.FileFullName, destName, false);
                        if (!checkDuplicate && File.Exists(currEnt.FileFullName))
                            File.Move(currEnt.FileFullName, ffnRename);
                    }
                    else
                    {
                        EntVedioM check = new EntVedioM();
                        if (File.Exists(ffnDoubleCheck))
                            check = Vedio_Init_fr_File(ffnDoubleCheck);
                        else if (File.Exists(ffnFinal))
                            check = Vedio_Init_fr_File(ffnFinal);
                        else if (File.Exists(ffnRename))
                            check = Vedio_Init_fr_File(ffnRename);

                        if (check.Equals(currEnt))
                        {
                            string backupName = Path.Combine(strBackup, currEnt.FileNameDate + currEnt.FileExt);
                            if (File.Exists(backupName))
                            {
                                System.Diagnostics.Debug.WriteLine("delete:" + currEnt.FileFullName);
                                File.Delete(currEnt.FileFullName);
                            }
                            else
                            {
                                int i = 0;
                                while (File.Exists(backupName))
                                {
                                    i++;
                                    backupName = Path.Combine(strBackup, string.Format("{0}_{1}{2}", currEnt.FileNameDate, i, currEnt.FileExt));
                                }

                                System.Diagnostics.Debug.WriteLine(string.Format("Move: {0} to {1}", currEnt.FileFullName, backupName));
                                File.Move(currEnt.FileFullName, backupName);
                            }
                        }
                        else if (!checkDuplicate)
                        {
                            int begIndex = Directory.GetFiles(doubleCheckDir, currEnt.FileNameDate + "*").Length;
                            List<string> lstMoveFile = new List<string>();
                            //move the final file to double check dir
                            if (File.Exists(ffnFinal))
                                lstMoveFile.Add(ffnFinal);
                            //move the rename file to double check dir
                            if (File.Exists(ffnRename))
                                lstMoveFile.Add(ffnRename);
                            //move current file double check dir
                            lstMoveFile.Add(currEnt.FileFullName);

                            foreach (var itmMoveFile in lstMoveFile)
                            {
                                string tmpFileName = Path.Combine(doubleCheckDir, string.Format("{0}{1}", currEnt.FileNameDate, currEnt.FileExt));
                                while (File.Exists(tmpFileName))
                                {
                                    tmpFileName = Path.Combine(doubleCheckDir, string.Format("{0}_{1}{2}", currEnt.FileNameDate, ++begIndex, currEnt.FileExt));
                                }

                                File.Move(itmMoveFile, tmpFileName);
                            }
                        }
                    }
                }
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

        private void btnCheck_Click(object sender, EventArgs e)
        {
            string strCheckDir = this.txtDir.Text.Trim();
            if (!Directory.Exists(strCheckDir))
            {
                MessageBox.Show("Check Dir not exist:" + strCheckDir);
                return;
            }

            List<EntPicM> lstPicM = new List<EntPicM>();
            List<EntVedioM> lstVedioM = new List<EntVedioM>();

            List<string> lstFiles = new List<string>();
            lstFiles.AddRange(Directory.GetFiles(strCheckDir, "*.*", SearchOption.AllDirectories));
            //for temp debug
            //lstFiles.AddRange(Directory.GetFiles(Path.Combine(strCheckDir, "mi"), "*.*", SearchOption.AllDirectories));
            //lstFiles.AddRange(Directory.GetFiles(Path.Combine(strCheckDir, "mi note"), "*.*", SearchOption.AllDirectories));
            //lstFiles.AddRange(Directory.GetFiles(Path.Combine(strCheckDir, "嫲嫲"), "*.*", SearchOption.AllDirectories));

            //string[] strFiles = Directory.GetFiles(strCheckDir, "*.*", SearchOption.AllDirectories);
            int cntPic = 0;
            int cntVedio = 0;
            foreach (string itemFile in lstFiles)
            {
                if (itemFile.ToUpper().Contains(".JPG") || itemFile.ToUpper().Contains(".PNG"))
                {
                    EntPicM entPM = this.Init_fr_file(itemFile);
                    lstPicM.Add(entPM);

                    if (lstPicM.Count > 100)
                    {
                        cntPic += lstPicM.Count;
                        this.DoSavePic(lstPicM, true);
                        System.Diagnostics.Debug.WriteLine(string.Format("process picture:{0}", cntPic));
                        lstPicM.Clear();
                    }
                }
                else if (itemFile.ToUpper().Contains(".MP4") || itemFile.ToUpper().Contains(".MOV"))
                {
                    EntVedioM entVM = Vedio_Init_fr_File(itemFile);
                    lstVedioM.Add(entVM);

                    if (lstVedioM.Count > 100)
                    {
                        cntVedio += lstVedioM.Count;
                        this.DoSaveVideo(lstVedioM, true);
                        System.Diagnostics.Debug.WriteLine(string.Format("process vedio:{0}", cntVedio));
                        lstVedioM.Clear();
                    }
                }
            }

            this.DoSavePic(lstPicM, true);
            this.DoSaveVideo(lstVedioM, true);
            MessageBox.Show("OK");
        }

        private void btnTempAction_Click(object sender, EventArgs e)
        {
            string strCheckDir = this.txtDir.Text.Trim();
            if (!Directory.Exists(strCheckDir))
            {
                MessageBox.Show("Check Dir not exist:" + strCheckDir);
                return;
            }

            string strDest = @"E:\photo\嫲嫲\renamed";
            List<string> lstFiles = new List<string>();
            int cntPic = 0;
            lstFiles.AddRange(Directory.GetFiles(strCheckDir, "*.*", SearchOption.AllDirectories));
            foreach (string itemFile in lstFiles)
            {
                if (itemFile.ToUpper().Contains(".JPG") || itemFile.ToUpper().Contains(".PNG"))
                {
                    EntPicM entPM = this.Init_fr_file(itemFile);
                    if (entPM.EquipModel == "MI NOTE Pro" || entPM.EquipModel == "SCH-N719" || entPM.EquipModel == "OPPO R11")
                    {
                        cntPic++;
                        FileInfo fi = new FileInfo(itemFile);
                        string dirName = fi.Directory.Name;
                        string subFolder = Path.Combine(strDest, entPM.EquipModel);
                        if (!Directory.Exists(subFolder))
                            Directory.CreateDirectory(subFolder);
                        string destName = Path.Combine(subFolder, fi.Name);
                        File.Move(itemFile, destName);

                        if (cntPic % 100 == 0)
                            System.Diagnostics.Debug.WriteLine(string.Format("process picture:{0}", cntPic));
                    }

                }
            }

            MessageBox.Show("OK");
        }
    }
}
