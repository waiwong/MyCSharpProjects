using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace CheckPDFSharp
{
    public class PDFSharpHelper
    {
        #region Combine

        /*
         * This samples shows how to create a new document from two existing PDF files.
         * The pages are inserted alternately from two documents. This may be useful for visual comparision
         */

        /// <summary>
        /// Imports pages from external documents.
        /// Note that this technique imports the whole page including the hyperlinks.
        /// </summary>
        public void Combine0(string checkDir)
        {
            if (!Directory.Exists(checkDir))
                return;

            string outputDir = Path.Combine(checkDir, "result");
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            string[] files = Directory.GetFiles(checkDir, "*.pdf", SearchOption.TopDirectoryOnly);

            if (files.Length < 2)
            {
                Console.WriteLine("at least two pdf file in the folder.");
                return;
            }

            // Create the output document
            using (PdfDocument outputDocument = new PdfDocument())
            {
                // Show consecutive pages facing. Requires Acrobat 5 or higher.
                outputDocument.PageLayout = PdfPageLayout.TwoColumnLeft;

                XFont font = new XFont("Verdana", 10, XFontStyle.Bold);
                XStringFormat format = new XStringFormat();
                format.Alignment = XStringAlignment.Center;
                format.LineAlignment = XLineAlignment.Far;
                XGraphics gfx;
                XRect box;

                using (PdfDocument inputDocument1 = PdfReader.Open(files[0], PdfDocumentOpenMode.Import))
                {
                    using (PdfDocument inputDocument2 = PdfReader.Open(files[1], PdfDocumentOpenMode.Import))
                    {
                        string filename1 = Path.GetFileNameWithoutExtension(files[0]);
                        string filename2 = Path.GetFileNameWithoutExtension(files[1]);
                        int page1Cnt = inputDocument1.PageCount;
                        int page2Cnt = inputDocument2.PageCount;
                        int count = Math.Max(inputDocument1.PageCount, inputDocument2.PageCount);
                        for (int idx = 0; idx < count; idx++)
                        {
                            // Get page from 1st document
                            PdfPage page1 = inputDocument1.PageCount > idx ? inputDocument1.Pages[idx] : new PdfPage();

                            // Get page from 2nd document
                            PdfPage page2 = inputDocument2.PageCount > idx ? inputDocument2.Pages[idx] : new PdfPage();

                            // Add both pages to the output document
                            page1 = outputDocument.AddPage(page1);
                            page2 = outputDocument.AddPage(page2);

                            // Write document file name and page number on each page
                            gfx = XGraphics.FromPdfPage(page1);
                            box = page1.MediaBox.ToXRect();
                            box.Inflate(0, -10);
                            gfx.DrawString(string.Format("{0} • {1} of {2}", filename1, idx + 1, page1Cnt), font, XBrushes.Red, box, format);

                            gfx = XGraphics.FromPdfPage(page2);
                            box = page2.MediaBox.ToXRect();
                            box.Inflate(0, -10);
                            gfx.DrawString(string.Format("{0} • {1}  of {2}", filename2, idx + 1, page2Cnt), font, XBrushes.Red, box, format);
                        }

                        // Save the document...
                        string filename = Path.Combine(outputDir, "CompareDocument1.pdf");
                        outputDocument.Save(filename);
                    }
                }
            }
        }

        /// <summary>
        /// Imports the pages as form X objects.
        /// Note that this technique copies only the visual content and the hyperlinks do not work.
        /// </summary>
        public void Combine1(string checkDir)
        {
            if (!Directory.Exists(checkDir))
                return;

            string outputDir = Path.Combine(checkDir, "result");
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            string[] files = Directory.GetFiles(checkDir, "*.pdf", SearchOption.TopDirectoryOnly);

            if (files.Length < 2)
            {
                Console.WriteLine("at least two pdf file in the folder.");
                return;
            }

            // Create the output document
            // Create the output document
            using (PdfDocument outputDocument = new PdfDocument())
            {
                using (PdfDocument inputDocument1 = PdfReader.Open(files[0], PdfDocumentOpenMode.Import))
                {
                    using (PdfDocument inputDocument2 = PdfReader.Open(files[1], PdfDocumentOpenMode.Import))
                    {
                        string filename1 = Path.GetFileNameWithoutExtension(files[0]);
                        string filename2 = Path.GetFileNameWithoutExtension(files[1]);

                        // Show consecutive pages facing
                        outputDocument.PageLayout = PdfPageLayout.TwoPageLeft;

                        XFont font = new XFont("Verdana", 10, XFontStyle.Bold);
                        XStringFormat format = new XStringFormat();
                        format.Alignment = XStringAlignment.Center;
                        format.LineAlignment = XLineAlignment.Far;
                        XGraphics gfx;
                        XRect box;

                        // Open the external documents as XPdfForm objects. Such objects are
                        // treated like images. By default the first page of the document is
                        // referenced by a new XPdfForm.
                        XPdfForm form1 = XPdfForm.FromFile(files[0]);
                        XPdfForm form2 = XPdfForm.FromFile(files[1]);

                        int count = Math.Max(form1.PageCount, form2.PageCount);
                        for (int idx = 0; idx < count; idx++)
                        {
                            // Add two new pages to the output document
                            PdfPage page1 = outputDocument.AddPage();
                            PdfPage page2 = outputDocument.AddPage();

                            if (form1.PageCount > idx)
                            {
                                // Get a graphics object for page1
                                gfx = XGraphics.FromPdfPage(page1);

                                // Set page number (which is one-based)
                                form1.PageNumber = idx + 1;

                                // Draw the page identified by the page number like an image
                                gfx.DrawImage(form1, new XRect(0, 0, form1.PointWidth, form1.PointHeight));

                                // Write document file name and page number on each page
                                box = page1.MediaBox.ToXRect();
                                box.Inflate(0, -10);
                                gfx.DrawString(String.Format("{0} • {1}", filename1, idx + 1), font, XBrushes.Red, box, format);
                            }

                            // Same as above for second page
                            if (form2.PageCount > idx)
                            {
                                gfx = XGraphics.FromPdfPage(page2);

                                form2.PageNumber = idx + 1;
                                gfx.DrawImage(form2, new XRect(0, 0, form2.PointWidth, form2.PointHeight));

                                box = page2.MediaBox.ToXRect();
                                box.Inflate(0, -10);
                                gfx.DrawString(String.Format("{0} • {1}", filename2, idx + 1), font, XBrushes.Red, box, format);
                            }
                        }

                        // Save the document...
                        string filename = Path.Combine(outputDir, "CompareDocument2.pdf");
                        outputDocument.Save(filename);
                    }
                }
            }
        }

        #endregion

        #region Concatenate

        /// <summary>
        /// This sample adds a consecutive number in the middle of each page.
        /// It shows how you can add graphics to an imported page.
        /// </summary>
        public void Concatenate0(string checkDir)
        {
            if (!Directory.Exists(checkDir))
                return;

            string outputDir = Path.Combine(checkDir, "result");
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            string[] files = Directory.GetFiles(checkDir, "*.pdf", SearchOption.TopDirectoryOnly);

            //get total pages
            Stopwatch sw = new Stopwatch();
            sw.Start();
            int totalNoPages = 0;
            foreach (string file in files)
            {
                using (PdfDocument inputDocument = PdfReader.Open(file, PdfDocumentOpenMode.ReadOnly))
                {
                    totalNoPages += inputDocument.PageCount;
                }
            }

            sw.Stop();
            TimeSpan ts = sw.Elapsed;
            string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            Console.WriteLine("RunTime of get total no pages : " + elapsedTime);

            // Open the output document
            using (PdfDocument outputDocument = new PdfDocument())
            {
                // This is because adding graphics to an imported page causes the
                // uncompression of its content if it was compressed in the external document.
                // To compare file sizes you should either run the sample as Release build
                // or uncomment the following line.
                //outputDocument.Options.CompressContentStreams = true;

                int number = 0;

                foreach (string file in files)
                {
                    sw.Start();
                    string fileName = Path.GetFileNameWithoutExtension(file);

                    // Open the document to import pages from it.
                    using (PdfDocument inputDocument = PdfReader.Open(file, PdfDocumentOpenMode.Import))
                    {
                        int count = inputDocument.PageCount;
                        for (int idx = 0; idx < count; idx++)
                        {
                            // Get the page from the external document
                            PdfPage page = inputDocument.Pages[idx];
                            // and add it to the output document.
                            // Note that the PdfPage instance returned by AddPage is a different object.
                            page = outputDocument.AddPage(page);

                            // Create a graphics object for this page. To draw beneath the existing content set 'Append' to 'Prepend'.
                            XGraphics gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);
                            string pageInfo = string.Format("Pg.{1}/{2} of {0} • Pg.{3}/{4}", fileName, idx + 1, count, ++number, totalNoPages);
                            DrawPageIndex(gfx, pageInfo);
                        }

                        sw.Stop();
                        Console.WriteLine(string.Format("RunTime of file name : {0} - {1}s", fileName, sw.ElapsedMilliseconds / 1000.0));
                    }
                }

                string filename = Path.Combine(outputDir, "ConcatenatedDocument0.pdf");
                outputDocument.Save(filename);
            }
        }

        /// <summary>
        /// It shows that add external pages with two pages.
        /// File by file, if page number is odd, with blank page.
        /// </summary>
        public void Concatenate1(string checkDir)
        {
            if (!Directory.Exists(checkDir))
                return;

            string outputDir = Path.Combine(checkDir, "result");
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            string[] files = Directory.GetFiles(checkDir, "*.pdf", SearchOption.TopDirectoryOnly);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            //get total pages
            int totalNoPages = 0;
            foreach (string file in files)
            {
                using (PdfDocument inputDocument = PdfReader.Open(file, PdfDocumentOpenMode.ReadOnly))
                {
                    totalNoPages += inputDocument.PageCount;
                }
            }

            sw.Stop();
            Console.WriteLine(string.Format("RunTime of get total no pages : {0}s", sw.ElapsedMilliseconds / 1000.0));

            // Open the output document
            using (PdfDocument outputDocument = new PdfDocument())
            {
                outputDocument.PageLayout = PdfPageLayout.SinglePage;
                // For checking the file size uncomment next line.
                outputDocument.Options.CompressContentStreams = true;

                XRect box;
                XGraphics gfx;

                int number = 0;

                // Iterate files
                foreach (string file in files)
                {
                    sw.Start();

                    string fileName = Path.GetFileNameWithoutExtension(file);

                    // Open the external document as XPdfForm object
                    using (XPdfForm form = XPdfForm.FromFile(file))
                    {
                        for (int idx = 0; idx < form.PageCount; idx += 2)
                        {
                            // Add a new page to the output document
                            PdfPage page = outputDocument.AddPage();
                            page.Orientation = PageOrientation.Landscape;
                            double width = page.Width;
                            double height = page.Height;

                            //int rotate = page.Elements.GetInteger("/Rotate");

                            gfx = XGraphics.FromPdfPage(page);
                            // Set page number (which is one-based)
                            form.PageNumber = idx + 1;
                            box = new XRect(0, 0, width / 2, height);
                            // Draw the page identified by the page number like an image
                            gfx.DrawImage(form, box);

                            // Write document file name and page number on each page
                            DrawPageIndexBottomCenter(gfx, box, string.Format("- {0} - {1} -", fileName, idx + 1));

                            if (idx + 1 < form.PageCount)
                            {
                                // Set page number (which is one-based)
                                form.PageNumber = idx + 2;

                                box = new XRect(width / 2, 0, width / 2, height);
                                // Draw the page identified by the page number like an image
                                gfx.DrawImage(form, box);

                                // Write document file name and page number on each page
                                DrawPageIndexBottomCenter(gfx, box, string.Format("- {0} - {1} -", fileName, idx + 2));
                            }

                            string pageInfo = string.Format("Pg.{1}/{2} of {0} • Pg.{3}/{4}", fileName, idx + 1, form.PageCount, ++number, totalNoPages);
                            DrawPageIndex(gfx, pageInfo);
                        }

                        sw.Stop();
                        Console.WriteLine(string.Format("RunTime of file name : {0} - {1}s", fileName, sw.ElapsedMilliseconds / 1000.0));
                    }
                }

                // Save the document...
                string filename = Path.Combine(outputDir, "ConcatenatedDocument1.pdf");
                outputDocument.Save(filename);
            }
        }

        /// <summary>
        /// It shows that add external pages with two pages.
        /// First, concatenate all page into one, then plance two pages into one page.
        /// </summary>
        public void Concatenate2(string checkDir)
        {
            if (!Directory.Exists(checkDir))
                return;

            string outputDir = Path.Combine(checkDir, "result");
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            string[] files = Directory.GetFiles(checkDir, "*.pdf", SearchOption.TopDirectoryOnly);

            if (files.Length == 0)
            {
                Console.WriteLine("No pdf files.");
                return;
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();

            //get total pages
            int totalNoPages = 0;
            foreach (string file in files)
            {
                using (PdfDocument inputDocument = PdfReader.Open(file, PdfDocumentOpenMode.ReadOnly))
                {
                    totalNoPages += inputDocument.PageCount;
                }
            }

            sw.Stop();
            Console.WriteLine(string.Format("RunTime of get total no pages : {0}s", sw.ElapsedMilliseconds / 1000.0));

            using (PdfDocument outputDocMid = new PdfDocument())
            {
                foreach (string file in files)
                {
                    sw.Start();
                    string fileName = Path.GetFileNameWithoutExtension(file);

                    // Open the document to import pages from it.
                    using (PdfDocument inputDocument = PdfReader.Open(file, PdfDocumentOpenMode.Import))
                    {
                        // Iterate pages
                        int count = inputDocument.PageCount;
                        for (int idx = 0; idx < count; idx++)
                        {
                            // Get the page from the external document
                            PdfPage page = inputDocument.Pages[idx];
                            // and add it to the output document.
                            // Note that the PdfPage instance returned by AddPage is a different object.
                            page = outputDocMid.AddPage(page);

                            // Create a graphics object for this page. To draw beneath the existing content set 'Append' to 'Prepend'.
                            XGraphics gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);
                            string pageInfo = string.Format("Pg.{1}/{2} of {0}", fileName, idx + 1, count);
                            DrawPageIndexBottomCenter(gfx, pageInfo);
                        }

                        sw.Stop();
                        Console.WriteLine(string.Format("RunTime of file name : {0} - {1}s", fileName, sw.ElapsedMilliseconds / 1000.0));
                    }
                }

                // Save the document...
                string filename = Path.Combine(outputDir, "ConcatenatedDocument2_1.pdf");
                outputDocMid.Save(filename);

                //Two Pages on One
                int newTotalNoPages = totalNoPages / 2 + (totalNoPages % 2 == 0 ? 0 : 1);

                using (MemoryStream stream = new MemoryStream())
                {
                    outputDocMid.Save(stream, false);
                    using (PdfDocument mergedOutputDoc = new PdfDocument())
                    {
                        mergedOutputDoc.PageLayout = PdfPageLayout.SinglePage;
                        // For checking the file size uncomment next line.
                        mergedOutputDoc.Options.CompressContentStreams = true;
                        XRect box;
                        XGraphics gfx;
                        int number = 0;
                        sw.Start();
                        using (XPdfForm form = XPdfForm.FromStream(stream))
                        {
                            for (int idx = 0; idx < form.PageCount; idx += 2)
                            {
                                // Add a new page to the output document
                                PdfPage page = mergedOutputDoc.AddPage();
                                page.Orientation = PageOrientation.Landscape;
                                double width = page.Width;
                                double height = page.Height;

                                gfx = XGraphics.FromPdfPage(page);
                                // Set page number (which is one-based)
                                form.PageNumber = idx + 1;
                                box = new XRect(0, 0, width / 2, height);
                                // Draw the page identified by the page number like an image
                                gfx.DrawImage(form, box);

                                if (idx + 1 < form.PageCount)
                                {
                                    // Set page number (which is one-based)
                                    form.PageNumber = idx + 2;

                                    box = new XRect(width / 2, 0, width / 2, height);
                                    // Draw the page identified by the page number like an image
                                    gfx.DrawImage(form, box);
                                }

                                string pageInfo = string.Format("Pg.{0}/{1}", ++number, newTotalNoPages);
                                DrawPageIndex(gfx, pageInfo);
                            }

                            sw.Stop();
                            Console.WriteLine(string.Format("RunTime of merge : {0}s", sw.ElapsedMilliseconds / 1000.0));
                        }

                        mergedOutputDoc.Save(Path.Combine(outputDir, "ConcatenatedDocument2.pdf"));
                    }
                }
            }
        }

        /// <summary>
        /// It shows that add external pages with two pages.
        /// </summary>
        public void Concatenate3(string checkDir)
        {
            if (!Directory.Exists(checkDir))
                return;

            string outputDir = Path.Combine(checkDir, "result");
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            string[] files = Directory.GetFiles(checkDir, "*.pdf", SearchOption.TopDirectoryOnly);
            if (files.Length == 0)
            {
                Console.WriteLine("no pdf files.");
                return;
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();

            //get total pages
            int totalNoPages = 0;
            foreach (string file in files)
            {
                using (PdfDocument inputDocument = PdfReader.Open(file, PdfDocumentOpenMode.ReadOnly))
                {
                    totalNoPages += inputDocument.PageCount;
                }
            }

            sw.Stop();
            Console.WriteLine(string.Format("RunTime of get total no pages : {0}s", sw.ElapsedMilliseconds / 1000.0));

            //Two Pages on One
            int newTotalNoPages = totalNoPages / 2 + (totalNoPages % 2 == 0 ? 0 : 1);
            int pageIdx = 0;
            // Open the output document
            using (PdfDocument outputDocument = new PdfDocument())
            {
                outputDocument.PageLayout = PdfPageLayout.SinglePage;
                // For checking the file size uncomment next line.
                outputDocument.Options.CompressContentStreams = true;

                XRect box;
                XGraphics gfx;

                bool nextFile = false;
                for (int fileIdx = 0; fileIdx < files.Length; fileIdx++)
                {
                    sw.Start();
                    string file = files[fileIdx];
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    // Open the external document as XPdfForm object
                    using (XPdfForm form = XPdfForm.FromFile(file))
                    {
                        int startIdx = nextFile ? 1 : 0;
                        nextFile = false;
                        int pgCount = form.PageCount;
                        for (int idx = startIdx; idx < form.PageCount; idx += 2)
                        {
                            // Add a new page to the output document
                            PdfPage page = outputDocument.AddPage();
                            page.Orientation = PageOrientation.Landscape;
                            double width = page.Width;
                            double height = page.Height;

                            gfx = XGraphics.FromPdfPage(page);
                            // Set page number (which is one-based)
                            form.PageNumber = idx + 1;
                            box = new XRect(0, 0, width / 2, height);
                            // Draw the page identified by the page number like an image
                            gfx.DrawImage(form, box);

                            // Write document file name and page number on each page
                            DrawPageIndexBottomCenter(gfx, box, string.Format("Pg.{1}/{2} of {0}", fileName, idx + 1, pgCount));

                            if (idx + 1 < form.PageCount)
                            {
                                // Set page number (which is one-based)
                                form.PageNumber = idx + 2;

                                box = new XRect(width / 2, 0, width / 2, height);
                                // Draw the page identified by the page number like an image
                                gfx.DrawImage(form, box);

                                // Write document file name and page number on each page
                                DrawPageIndexBottomCenter(gfx, box, string.Format("Pg.{1}/{2} of {0}", fileName, idx + 2, pgCount));
                            }
                            else
                            {
                                //last page.
                                nextFile = true;
                                if (fileIdx + 1 < files.Length)
                                {
                                    string nextFileName = Path.GetFileNameWithoutExtension(files[fileIdx + 1]);
                                    using (XPdfForm nextPdfFileForm = XPdfForm.FromFile(files[fileIdx + 1]))
                                    {
                                        nextPdfFileForm.PageNumber = 1;
                                        box = new XRect(width / 2, 0, width / 2, height);
                                        // Draw the page identified by the page number like an image
                                        gfx.DrawImage(nextPdfFileForm, box);

                                        // Write document file name and page number on each page
                                        DrawPageIndexBottomCenter(gfx, box, string.Format("Pg.{1}/{2} of {0}", nextFileName, 1, nextPdfFileForm.PageCount));
                                    }
                                }
                            }

                            string pageInfo = string.Format("Pg.{0}/{1}", ++pageIdx, newTotalNoPages);
                            DrawPageIndex(gfx, pageInfo);
                        }

                        sw.Stop();
                        Console.WriteLine(string.Format("RunTime of file name : {0} - {1}s", fileName, sw.ElapsedMilliseconds / 1000.0));
                    }
                }

                // Save the document...
                string filename = Path.Combine(outputDir, "ConcatenatedDocument3.pdf");
                outputDocument.Save(filename);
            }
        }

        #endregion

        #region Draw Page Index

        /// <summary>
        /// Draws the number and time in the top of the page.
        /// </summary>
        private static void DrawPageIndex(XGraphics gfx, string pageIndex, bool printTime = false)
        {
            XFont font = new XFont("Verdana", 10, XFontStyle.Italic);
            XStringFormat format = new XStringFormat();
            format.Alignment = XStringAlignment.Center;
            // Draw the text
            XRect rect = new XRect(10, 5, gfx.PageSize.Width - 10, gfx.PageSize.Height - 5);
            gfx.DrawString(pageIndex, font, XBrushes.Black, rect, XStringFormats.TopLeft);
            if (printTime)
            {
                gfx.DrawString(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), font, XBrushes.Black, rect, XStringFormats.TopRight);
            }

            //XRect rect0 = new XRect(10, 5, gfx.PageSize.Width - 10, 30);
            //gfx.DrawString(numberString + "-CenterLeft", font, XBrushes.Black, rect0, XStringFormats.CenterLeft);
            //gfx.DrawString(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-CenterLeft", font, XBrushes.Black, rect0, XStringFormats.CenterRight);
        }

        /// <summary>
        /// Draws the string in the bottom of the page.
        /// </summary>
        private static void DrawPageIndexBottomCenter(XGraphics gfx, string pageIndex, bool printTime = false)
        {
            XFont font = new XFont("Verdana", 8, XFontStyle.Bold);
            XStringFormat format = new XStringFormat();
            format.Alignment = XStringAlignment.Near;
            format.LineAlignment = XLineAlignment.Far;

            XRect box = new XRect(0, 0, gfx.PageSize.Width, gfx.PageSize.Height);
            box.Inflate(0, -10);

            gfx.DrawString(pageIndex, font, XBrushes.Red, box, format);
            if (printTime)
            {
                gfx.DrawString(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), font, XBrushes.Blue, box, XStringFormats.BottomRight);
            }
            //draw space use XBrushes.Black, some time the next page color will same as the last draw font.
            gfx.DrawString(" ", font, XBrushes.Black, box, XStringFormats.BottomCenter);
        }

        /// <summary>
        /// Draws the string in the bottom of the page.
        /// </summary>
        private static void DrawPageIndexBottomCenter(XGraphics gfx, XRect box, string pageIndex, bool printTime = false)
        {
            XFont font = new XFont("Verdana", 8, XFontStyle.Bold);
            XStringFormat format = new XStringFormat();
            format.Alignment = XStringAlignment.Center;
            format.LineAlignment = XLineAlignment.Far;

            box.Inflate(0, -10);

            gfx.DrawString(pageIndex, font, XBrushes.Red, box, format);
            if (printTime)
            {
                gfx.DrawString(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), font, XBrushes.Blue, box, XStringFormats.BottomRight);
            }
            //draw space use XBrushes.Black, some time the next page color will same as the last draw font.
            gfx.DrawString(" ", font, XBrushes.Black, box, XStringFormats.BottomCenter);
        }

        #endregion
    }
}