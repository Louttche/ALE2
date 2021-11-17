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
    public partial class Form1 : Form
    {
        public Graph graph;
        private string graphVizString = @" digraph g{ label=""Graph""; labelloc=top;labeljust=left;}";

        // UI Elements
        public RichTextBox ui_rtb_filecontents;
        public PictureBox ui_pb_graph;

        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Initialize global ui
            ui_rtb_filecontents = rtb_filecontents;
            ui_pb_graph = pb_graph;

            this.graph = new Graph(this);
            lbl_fileloaded.Text = "No graph loaded.";
            lbl_status.Text = "Open a file to start drawing!";
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
                    lbl_status.Text = "Parsing not implemented yet";
                    lbl_fileloaded.Text = "No graph loaded.";
                }
            }
        }
    }
}
