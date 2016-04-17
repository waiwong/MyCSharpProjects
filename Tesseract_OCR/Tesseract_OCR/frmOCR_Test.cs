using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tesseract;

namespace Tesseract_OCR
{
    public partial class frmOCR_Test : Form
    {
        public frmOCR_Test()
        {
            InitializeComponent();
        }

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "tif files (*.tif)|*.tif|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.txtFile.Text = openFileDialog1.FileName;
            }
        }

        private void btnOCR_Click(object sender, EventArgs e)
        {
            try
            {
                string testImagePath = this.txtFile.Text.Trim();
                using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
                {                    
                    using (var img = Pix.LoadFromFile(testImagePath))
                    {
                        System.Diagnostics.Debug.WriteLine("Process image");

                        var i = 1;
                        using (var page = engine.Process(img))
                        {
                            var text = page.GetText();
                            System.Diagnostics.Debug.WriteLine("Text: {0}", text);
                            System.Diagnostics.Debug.WriteLine("Mean confidence: {0}", page.GetMeanConfidence());

                            using (var iter = page.GetIterator())
                            {
                                iter.Begin();
                                do
                                {
                                    if (i % 2 == 0)
                                    {
                                        System.Diagnostics.Debug.WriteLine("Line {0}", i);
                                        do
                                        {
                                            System.Diagnostics.Debug.WriteLine("Word Iteration");

                                            if (iter.IsAtBeginningOf(PageIteratorLevel.Block))
                                            {
                                                System.Diagnostics.Debug.WriteLine("New block");
                                            }
                                            if (iter.IsAtBeginningOf(PageIteratorLevel.Para))
                                            {
                                                System.Diagnostics.Debug.WriteLine("New paragraph");
                                            }
                                            if (iter.IsAtBeginningOf(PageIteratorLevel.TextLine))
                                            {
                                                System.Diagnostics.Debug.WriteLine("New line");
                                            }
                                            System.Diagnostics.Debug.WriteLine("word: " + iter.GetText(PageIteratorLevel.Word));
                                        } while (iter.Next(PageIteratorLevel.TextLine, PageIteratorLevel.Word));
                                    }
                                    i++;
                                } while (iter.Next(PageIteratorLevel.Para, PageIteratorLevel.TextLine));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected Error: " + ex.Message);
                Console.WriteLine("Details: ");
                Console.WriteLine(ex.ToString());
            }
        }

        private void frmOCR_Test_Load(object sender, EventArgs e)
        {
            this.txtFile.Text = @"C:\SVN\github\MyCSharpProjects\Tesseract_OCR\Tesseract_OCR\phototest.tif";
        }
    }
}
