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

namespace ALE2
{
    public partial class Form1 : Form
    {
        public Graph graph;

        public Form1()
        {
            InitializeComponent();

            this.graph = new Graph();
            Debug.WriteLine("Graph created.");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            String graphVizString = @" digraph g{ label=""Graph""; labelloc=top;labeljust=left;}";
            Bitmap bm = Graph.Run(graphVizString);

            pb_graph.Image = bm;
        }
    }
}
