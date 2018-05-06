using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace PhotoManage
{
    public class EntPicM : IEquatable<EntPicM>
    {
        public string FileFullName { get; set; }
        private string mFileName = string.Empty;
        public string FileName
        {
            get
            {
                return this.mFileName;
            }
        }

        public string FileNameWithEquip
        {
            get
            {
                return string.Format("{0}_{1}", this.mFileName, this.EquipModel ?? "1");
            }
        }

        public string FileExt { get { return Path.GetExtension(FileFullName); } }
        public string ParentDir { get { return Directory.GetParent(FileFullName).Name; } }
        public int Width { get; set; }
        public int Higth { get; set; }
        private string mTakeTime = string.Empty;
        public string TakeTime
        {
            get
            {
                return this.mTakeTime;
            }
            set
            {
                this.mTakeTime = value;
                string dateStr = this.TakeTime.Trim(new char[] { '\0' }).Replace(" ", "").Replace(":", "");
                if (this.FileName.Replace("IMG_", "").Replace("_", "").StartsWith(dateStr.Substring(0, 11)))
                {
                    dateStr = this.FileName.Replace("IMG_", "").Replace("(", "_").Replace(")", "").Replace("_bestshot", "").Replace("_HDR", "").Replace("DSC_", "").Replace("_LLS", "");
                    if (dateStr.Substring(8, 1).Equals("_"))
                    {
                        dateStr = dateStr.Substring(0, 8) + dateStr.Substring(9);
                    }

                    if (dateStr.Length > 14)
                    {
                        if (string.IsNullOrEmpty(this.EquipModel) || !this.EquipModel.Equals("SCH-N719"))
                            dateStr = dateStr.Substring(0, 14);
                    }
                }
                this.mFileName = dateStr;
            }
        }

        public string TakeTime1 { get; set; }
        public string TakeTime2 { get; set; }
        public string EquipModel { get; set; }

        //public uint ExifImageWidth { get; set; }
        //public uint ExifImageHeight { get; set; }
        //public string ImageWidth { get; set; }
        //public string ImageHigth { get; set; }
        public string Other { get; set; }

        public string TakeTime_string { get; set; }

        public bool CheckEqualEquipModel(EntPicM other)
        {
            if (other == null)
                return false;

            if (this.TakeTime_string.Equals(other.TakeTime_string)
                && (this.EquipModel ?? string.Empty).Equals(other.EquipModel ?? string.Empty)
                && this.Width.Equals(other.Width)
                && this.Higth.Equals(other.Higth))
                return true;
            else
                return false;
        }

        public bool Equals(EntPicM other)
        {
            if (other == null)
                return false;

            if (this.TakeTime_string.Equals(other.TakeTime_string)
                && (this.EquipModel ?? string.Empty).Equals(other.EquipModel ?? string.Empty)
                && this.Width.Equals(other.Width)
                && this.Higth.Equals(other.Higth)
                && this.FileHash.Equals(other.FileHash))
                return true;
            else
                return false;
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;

            EntPicM checkObj = obj as EntPicM;
            if (checkObj == null)
                return false;
            else
                return Equals(checkObj);
        }

        public override int GetHashCode()
        {
            return string.Format("{0}-{1}-{2}-{3}-{4}", this.TakeTime_string, this.EquipModel, this.Width, this.Higth, this.FileHash).GetHashCode();
        }

        private string mHash = string.Empty;
        public string FileHash
        {
            get
            {
                if (string.IsNullOrEmpty(this.mHash))
                {
                    using (var ha = HashAlgorithm.Create())
                    {
                        using (var stream = File.OpenRead(this.FileFullName))
                        {
                            var hash = ha.ComputeHash(stream);
                            this.mHash = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                        }
                    }

                }

                return this.mHash;
            }
        }
    }

    public class EntVedioM
    {
        public string FileFullName { get; set; }
        public string FileName { get { return Path.GetFileNameWithoutExtension(FileFullName); } }
        public string FileExt { get { return Path.GetExtension(FileFullName); } }
        public DateTime LastModifyDT { get; set; }
        public string FileNameDate { get; set; }

        public bool Equals(EntVedioM other)
        {
            if (other == null)
                return false;

            if (this.FileNameDate.Equals(other.FileNameDate)
                && this.FileHash.Equals(other.FileHash))
                return true;
            else
                return false;
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;

            EntVedioM checkObj = obj as EntVedioM;
            if (checkObj == null)
                return false;
            else
                return Equals(checkObj);
        }

        public override int GetHashCode()
        {
            return string.Format("{0}-{1}", this.FileNameDate, this.FileHash).GetHashCode();
        }

        private string mHash = string.Empty;
        public string FileHash
        {
            get
            {
                if (string.IsNullOrEmpty(this.mHash))
                {
                    using (var ha = HashAlgorithm.Create())
                    {
                        using (var stream = File.OpenRead(this.FileFullName))
                        {
                            var hash = ha.ComputeHash(stream);
                            this.mHash = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                        }
                    }

                }

                return this.mHash;
            }
        }
    }
}
