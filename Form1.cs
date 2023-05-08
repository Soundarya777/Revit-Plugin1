using System;
using System.Linq;
using System.Windows.Forms;

namespace S3KeystoRVT
{
    public partial class Form1 : Form
    {
        private string folderPath = string.Empty;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                folderPath = folderBrowserDialog1.SelectedPath;
                textBox1.Text = KeystoRVT();
            }
        }

        private string KeystoRVT()
        {
            string rvtFilePath = string.Empty;
            try
            {
                string rvtName = folderPath.Split('\\').Last();
                string rvtFileName = string.Empty;

                if (rvtName.Contains("_backup"))
                    rvtFileName = rvtName.Replace("_backup", "");
                else
                    rvtFileName = rvtName;

                rvtFilePath = DataBinding.CreateLatestCentralFile(folderPath, rvtFileName + ".rvt");
            }
            catch (Exception ex)
            {
            }
            return rvtFilePath;

        }
    }
}
