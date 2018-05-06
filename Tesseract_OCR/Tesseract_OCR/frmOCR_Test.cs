using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
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
            DirectoryInfo di = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            this.txtFile.Text = Path.Combine(di.Parent.Parent.Parent.FullName, "phototest.tif");
        }

        private void btnResize_Click(object sender, EventArgs e)
        {
            string strSrcImage = this.txtFile.Text;

            this.Resize(strSrcImage, 1.5);
            this.Resize(strSrcImage, 2);

            Bitmap bmp = new Bitmap(strSrcImage);
            double adjustPercent = 1.5;
            Bitmap newImage = this.Resize(bmp, adjustPercent);
            string saveFFN = Path.Combine(Path.GetDirectoryName(strSrcImage), string.Format("{0}_{1}_1.tiff",
                           Path.GetFileNameWithoutExtension(strSrcImage), adjustPercent.ToString()));

            if (File.Exists(saveFFN))
                File.Delete(saveFFN);

            newImage.Save(saveFFN, System.Drawing.Imaging.ImageFormat.Tiff);

            adjustPercent = 2;
            newImage = this.Resize(bmp, adjustPercent);
            saveFFN = Path.Combine(Path.GetDirectoryName(strSrcImage), string.Format("{0}_{1}_1.tiff",
                           Path.GetFileNameWithoutExtension(strSrcImage), adjustPercent.ToString()));

            if (File.Exists(saveFFN))
                File.Delete(saveFFN);

            newImage.Save(saveFFN, System.Drawing.Imaging.ImageFormat.Tiff);
        }

        private void Resize(string srcImageFile, double adjustPercent)
        {
            using (Image curImg = Image.FromFile(srcImageFile))
            {
                int newwidth = (int)(curImg.Width * adjustPercent);
                int newHeight = (int)(curImg.Height * adjustPercent);
                var destRect = new Rectangle(0, 0, newwidth, newHeight);
                //float AspectRatio = (float)curImg.Size.Width / (float)curImg.Size.Height;
                Bitmap newImage = new Bitmap(newwidth, newHeight);
                newImage.SetResolution(curImg.HorizontalResolution, curImg.VerticalResolution);
                using (Graphics gr = Graphics.FromImage(newImage))
                {
                    gr.CompositingMode = CompositingMode.SourceCopy;
                    gr.CompositingQuality = CompositingQuality.HighQuality;
                    gr.SmoothingMode = SmoothingMode.HighQuality;
                    gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    //gr.DrawImage(curImg, new Rectangle(0, 0, newwidth, newHeight));
                    using (var wrapMode = new System.Drawing.Imaging.ImageAttributes())
                    {
                        wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                        gr.DrawImage(curImg, destRect, 0, 0, curImg.Width, curImg.Height, GraphicsUnit.Pixel, wrapMode);
                    }
                }

                string saveFFN = Path.Combine(Path.GetDirectoryName(srcImageFile), string.Format("{0}_{1}.tiff",
                            Path.GetFileNameWithoutExtension(srcImageFile), adjustPercent.ToString()));

                if (File.Exists(saveFFN))
                    File.Delete(saveFFN);

                newImage.Save(saveFFN, System.Drawing.Imaging.ImageFormat.Tiff);
            }
        }

        public Bitmap Resize(Bitmap bmp, double adjustPercent)
        {
            int newWidth = (int)(bmp.Width * adjustPercent);
            int newHeight = (int)(bmp.Height * adjustPercent);
            Bitmap temp = (Bitmap)bmp;
            Bitmap bmap = new Bitmap(newWidth, newHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            double nWidthFactor = (double)temp.Width / (double)newWidth;
            double nHeightFactor = (double)temp.Height / (double)newHeight;

            double fx, fy, nx, ny;
            int cx, cy, fr_x, fr_y;
            Color color1 = new Color();
            Color color2 = new Color();
            Color color3 = new Color();
            Color color4 = new Color();
            byte nRed, nGreen, nBlue;

            byte bp1, bp2;

            for (int x = 0; x < bmap.Width; ++x)
            {
                for (int y = 0; y < bmap.Height; ++y)
                {
                    fr_x = (int)Math.Floor(x * nWidthFactor);
                    fr_y = (int)Math.Floor(y * nHeightFactor);
                    cx = fr_x + 1;
                    if (cx >= temp.Width) cx = fr_x;
                    cy = fr_y + 1;
                    if (cy >= temp.Height) cy = fr_y;
                    fx = x * nWidthFactor - fr_x;
                    fy = y * nHeightFactor - fr_y;
                    nx = 1.0 - fx;
                    ny = 1.0 - fy;

                    color1 = temp.GetPixel(fr_x, fr_y);
                    color2 = temp.GetPixel(cx, fr_y);
                    color3 = temp.GetPixel(fr_x, cy);
                    color4 = temp.GetPixel(cx, cy);

                    // Blue
                    bp1 = (byte)(nx * color1.B + fx * color2.B);

                    bp2 = (byte)(nx * color3.B + fx * color4.B);

                    nBlue = (byte)(ny * (double)(bp1) + fy * (double)(bp2));

                    // Green
                    bp1 = (byte)(nx * color1.G + fx * color2.G);

                    bp2 = (byte)(nx * color3.G + fx * color4.G);

                    nGreen = (byte)(ny * (double)(bp1) + fy * (double)(bp2));

                    // Red
                    bp1 = (byte)(nx * color1.R + fx * color2.R);

                    bp2 = (byte)(nx * color3.R + fx * color4.R);

                    nRed = (byte)(ny * (double)(bp1) + fy * (double)(bp2));

                    bmap.SetPixel(x, y,Color.FromArgb(255, nRed, nGreen, nBlue));
                }
            }

            bmap = SetGrayscale(bmap);
            bmap = RemoveNoise(bmap);

            return bmap;
        }

        public Bitmap SetGrayscale(Bitmap img)
        {
            Bitmap temp = (Bitmap)img;
            Bitmap bmap = (Bitmap)temp.Clone();
            Color c;
            for (int i = 0; i < bmap.Width; i++)
            {
                for (int j = 0; j < bmap.Height; j++)
                {
                    c = bmap.GetPixel(i, j);
                    byte gray = (byte)(.299 * c.R + .587 * c.G + .114 * c.B);

                    bmap.SetPixel(i, j, Color.FromArgb(gray, gray, gray));
                }
            }
            return (Bitmap)bmap.Clone();
        }

   
        public Bitmap RemoveNoise(Bitmap bmap)
        {
            for (var x = 0; x < bmap.Width; x++)
            {
                for (var y = 0; y < bmap.Height; y++)
                {
                    var pixel = bmap.GetPixel(x, y);
                    if (pixel.R < 162 && pixel.G < 162 && pixel.B < 162)
                        bmap.SetPixel(x, y, Color.Black);
                }
            }

            for (var x = 0; x < bmap.Width; x++)
            {
                for (var y = 0; y < bmap.Height; y++)
                {
                    var pixel = bmap.GetPixel(x, y);
                    if (pixel.R > 162 && pixel.G > 162 && pixel.B > 162)
                        bmap.SetPixel(x, y, Color.White);
                }
            }

            return bmap;
        }

        /// <summary>
        /// Rescales an image.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="dpiX"></param>
        /// <param name="dpiY"></param>
        /// <returns></returns>
        private Image Rescale(Image image, int dpiX, int dpiY)
        {
            Bitmap bm = new Bitmap((int)(image.Width * dpiX / image.HorizontalResolution), (int)(image.Height * dpiY / image.VerticalResolution));
            bm.SetResolution(dpiX, dpiY);
            Graphics g = Graphics.FromImage(bm);
            g.InterpolationMode = InterpolationMode.Bicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.DrawImage(image, 0, 0);
            g.Dispose();

            return bm;
        }
    }
}
