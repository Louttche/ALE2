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
        public string[] graph_file_contents;
        public string graph_dot;

        public NodeRegexManager regexManager;

        // UI Elements
        public RichTextBox ui_rtb_filecontents;
        public RichTextBox ui_rtb_words;
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
            ui_rtb_words = rtb_words;
            ui_pb_graph = pb_graph;
            ui_pb_dfa = pb_dfa;
            ui_pb_finite = pb_finite;                                                                                                                                                                                              

            ui_tooltip_info = toolTip_info;

            pb_wordinput.Visible = false;
            tb_wordinput.Enabled = false;
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
                
                this.graph.DebugParsedValues();

                if (this.graph.bm_graph != null)
                {
                    pb_graph.Image = this.graph.bm_graph;
                    DisplayWords(this.graph.words);
                }
            }
        }

        private void btn_refresh_Click(object sender, EventArgs e)
        {
            this.graph = new Graph(this);

            if (rtb_filecontents.Text.StartsWith("digraph") || rtb_filecontents.Text.StartsWith("graph"))
                this.graph_dot = rtb_filecontents.Text;
            else if (rtb_filecontents.Text.StartsWith("regex") || rtb_filecontents.Text.StartsWith("# Regex"))
                this.graph_dot = this.graph.ParseRegexFile(rtb_filecontents.Lines);
            else
                this.graph_dot = this.graph.ParseDefaultFile(rtb_filecontents.Lines);

            pb_graph.Image = Graph.Run(this.graph_dot);
            if (this.graph.words != null)
                DisplayWords(this.graph.words);

            this.graph.DebugParsedValues();
        }

        private void DisplayWords(Dictionary<string, bool> words)
        {
            // Clear textbox with words
            rtb_words.Text = "";
            // Check words + display them
            this.graph.CheckWords();
            // Make manual word input available
            tb_wordinput.Enabled = true;
        }

        private void tb_wordinput_TextChanged(object sender, EventArgs e)
        {
            if (tb_wordinput.Text.Length > 0)
            {
                pb_wordinput.Visible = true;
                // Check inputed word
                if (this.graph.CheckWord(tb_wordinput.Text))
                    pb_wordinput.Image = new Bitmap(Properties.Resources.tick);
                else
                    pb_wordinput.Image = new Bitmap(Properties.Resources.x);
            } else
                pb_wordinput.Visible = false;
        }

        private void cb_filecontents_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_filecontents.Checked)
                rtb_filecontents.Text = this.graph_dot;
            else
                rtb_filecontents.Text = String.Join("\n", this.graph_file_contents);
        }
    }
}
