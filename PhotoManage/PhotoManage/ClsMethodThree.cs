using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace PhotoManage
{
    public class ClsMethodThree
    {
        public void AddWaterMark(MemoryStream ms, string watermarkText, MemoryStream outputStream)
        {
            System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
            Graphics gr = Graphics.FromImage(img);
            Font font = new Font("Tahoma", (float)40);
            Color color = Color.FromArgb(50, 241, 235, 105);
            double tangent = (double)img.Height / (double)img.Width;
            double angle = Math.Atan(tangent) * (180 / Math.PI);
            double halfHypotenuse = Math.Sqrt((img.Height * img.Height) + (img.Width * img.Width)) / 2;
            double sin, cos, opp1, adj1, opp2, adj2;

            for (int i = 100; i > 0; i--)
            {
                font = new Font("Tahoma", i, FontStyle.Bold);
                SizeF sizef = gr.MeasureString(watermarkText, font, int.MaxValue);

                sin = Math.Sin(angle * (Math.PI / 180));
                cos = Math.Cos(angle * (Math.PI / 180));
                opp1 = sin * sizef.Width;
                adj1 = cos * sizef.Height;
                opp2 = sin * sizef.Height;
                adj2 = cos * sizef.Width;

                if (opp1 + adj1 < img.Height && opp2 + adj2 < img.Width)
                    break;
                //
            }

            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;

            gr.SmoothingMode = SmoothingMode.AntiAlias;
            gr.RotateTransform((float)angle);
            gr.DrawString(watermarkText, font, new SolidBrush(color), new Point((int)halfHypotenuse, 0), stringFormat);

            img.Save(outputStream, ImageFormat.Jpeg);
        }

    }
}
