using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CheckOpenXML
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ExcelReplace excelReplace = new ExcelReplace();
            excelReplace.DoReplaceAndInsert();
        }
    }
}