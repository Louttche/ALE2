using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace ALE2
{
    public partial class MainForm : Form
    {
        public Graph graph;

        // UI Elements
        public RichTextBox ui_rtb_filecontents;
        public PictureBox ui_pb_graph;

        public MainForm()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Initialize global ui
            ui_rtb_filecontents = rtb_filecontents;
            ui_pb_graph = pb_graph;

            this.graph = new Graph(this);
            lbl_fileloaded.Text = "No graph loaded.";
        }

        private void btn_browse_Click(object sender, EventArgs e)
        {
            string init_dir = Directory.GetCurrentDirectory() + "/files"; //Environment.CurrentDirectory;

            string path;
            OpenFileDialog file = new OpenFileDialog();
            file.InitialDirectory = init_dir;
            if (file.ShowDialog() == DialogResult.OK)
            {
                path = file.FileName;
                Bitmap bm = this.graph.GetGraphFromFile(path);

                if (bm != null)
                {
                    pb_graph.Image = bm;
                    lbl_fileloaded.Text = "graph loaded.";
                }
                else
                {
                    pb_graph.Image = null;
                    lbl_fileloaded.Text = "No graph loaded.";
                }
            }
        }
    }
}
