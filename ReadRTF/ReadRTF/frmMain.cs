using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ReadRTF
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            this.ofdRTF.InitialDirectory = Environment.SpecialFolder.Desktop.ToString();
            if (this.ofdRTF.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.txtFFN.Text = this.ofdRTF.FileName;
                this.ConvertFromRTF(this.txtFFN.Text);
            }
        }

        private void ConvertFromRTF(string strFFN)
        {
            // Get the contents of the RTF file. When the contents of the file are   
            // stored in the string (rtfText), the contents are encoded as UTF-16.  
            string rtfText = System.IO.File.ReadAllText(strFFN);

            // Display the RTF text. This should look like the contents of your file.
        //    System.Windows.Forms.MessageBox.Show(rtfText);

            // Use the RichTextBox to convert the RTF code to plain text.
            this.rtbResult.Rtf = rtfText;
            string plainText = this.rtbResult.Text;

            FileInfo fi = new FileInfo(strFFN);
            string outFFN = Path.Combine(Path.GetDirectoryName(strFFN), Path.GetFileNameWithoutExtension(strFFN) + "output.txt");
            // Output the plain text to a file, encoded as UTF-8. 
            System.IO.File.WriteAllText(outFFN, plainText);

            
        }
    }
}
