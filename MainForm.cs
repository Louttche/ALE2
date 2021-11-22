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
        private string graph_path;
        public string graph_dot;

        // UI Elements
        public RichTextBox ui_rtb_filecontents;
        public PictureBox ui_pb_graph;
        public PictureBox ui_pb_dfa;
        public PictureBox ui_pb_finite;

        public ToolTip ui_tooltip_info;

        public MainForm()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Initialize global ui
            ui_rtb_filecontents = rtb_filecontents;
            ui_pb_graph = pb_graph;
            ui_pb_dfa = pb_dfa;
            ui_pb_finite = pb_finite;

            ui_tooltip_info = toolTip_info;
        }

        private void btn_browse_Click(object sender, EventArgs e)
        {
            string init_dir = Directory.GetCurrentDirectory() + "/files"; //Environment.CurrentDirectory;

            OpenFileDialog file = new OpenFileDialog();
            file.InitialDirectory = init_dir;
            if (file.ShowDialog() == DialogResult.OK)
            {
                this.graph = new Graph(this);
                this.graph_path = file.FileName;
                this.graph.bm_graph = this.graph.GetGraphFromFile(this.graph_path);

                if (this.graph.bm_graph != null)
                    pb_graph.Image = this.graph.bm_graph;
            }
        }

        private void btn_refresh_Click(object sender, EventArgs e)
        {
            this.graph = new Graph(this);

            if (rtb_filecontents.Text.StartsWith("digraph") || rtb_filecontents.Text.StartsWith("graph"))
                this.graph_dot = rtb_filecontents.Text;
            else
                this.graph_dot = this.graph.ParseContent(rtb_filecontents.Lines);

            pb_graph.Image = Graph.Run(this.graph_dot);
        }
    }
}
