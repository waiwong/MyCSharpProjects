using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace PhotoManage
{
    public class EntPicM
    {
        public string FileFullName { get; set; }
        public string FileName { get { return Path.GetFileNameWithoutExtension(FileFullName); } }
        public string FileExt { get { return Path.GetExtension(FileFullName); } }
        public string ParentDir { get { return Directory.GetParent(FileFullName).Name; } }
        public int Width { get; set; }
        public int Higth { get; set; }
        public string TakeTime { get; set; }
        public string TakeTime1 { get; set; }
        public string TakeTime2 { get; set; }
        public string EquipModel { get; set; }

        //public uint ExifImageWidth { get; set; }
        //public uint ExifImageHeight { get; set; }
        //public string ImageWidth { get; set; }
        //public string ImageHigth { get; set; }
        public string Other { get; set; }
    }

    public class EntVedioM
    {
        public string FileFullName { get; set; }
        public string FileName { get { return Path.GetFileNameWithoutExtension(FileFullName); } }
        public string FileExt { get { return Path.GetExtension(FileFullName); } }
        public DateTime LastModifyDT { get; set; }
        public string FileNameDate { get; set; }
    }
}
