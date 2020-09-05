using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckPDFSharp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string checkDir = @"C:\Users\wangwei\Desktop\TEMP";
            PDFSharpHelper pdfhelper = new PDFSharpHelper();
            pdfhelper.Combine0(checkDir);
            pdfhelper.Combine1(checkDir);
            pdfhelper.Concatenate0(checkDir);
            pdfhelper.Concatenate1(checkDir);
            pdfhelper.Concatenate2(checkDir);
            pdfhelper.Concatenate3(checkDir);
            Console.WriteLine("Finished.");
            Console.ReadKey();
        }
    }
}