using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Linq;

namespace ALE2
{
    public class Graph
    {
        //private static string graphviz_path = "C:/Program Files/Graphviz/bin";
        private Form1 form;

        public Graph(Form1 form)
        {
            Debug.WriteLine("Initializing Graphviz...");
            this.form = form;

            //TODO: Check if dot.exe file is detectable, if not ask user to find it
        }

        public static Bitmap Run(string dot)
        {
            Debug.WriteLine("Running Graphviz...");

            string executable = @".\external\dot.exe";
            string output = @".\external\tempgraph";
            File.WriteAllText(output, dot);

            Process process = new Process();

            // Stop the process from opening a new window
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            // Setup executable and parameters
            process.StartInfo.FileName = executable;
            process.StartInfo.Arguments = string.Format(@"{0} -Tjpg -O", output);

            // Go
            process.Start();
            // and wait dot.exe to complete and exit
            process.WaitForExit();
            Bitmap bitmap = null; ;
            using (Stream bmpStream = File.Open(output + ".jpg", FileMode.Open))
            {
                Image image = Image.FromStream(bmpStream);
                bitmap = new Bitmap(image);
            }
            File.Delete(output);
            File.Delete(output + ".jpg");
            return bitmap;
        }

        public Bitmap GetGraphFromFile(string path)
        {
            Bitmap bm = null;
            string[] file_contents = File.ReadAllLines(@path);

            // Display the contents in the ui text box
            this.form.ui_rtb_filecontents.Lines = file_contents;

            // Do not read any commented lines
            string graph_contents = string.Join("", file_contents.Where(line => !line.StartsWith('#')));

            // TODO: Check if contents are in dot language and if not convert
            //if (!graph_contents.StartsWith("digraph") || !graph_contents.StartsWith("graph"))
            //    graph_contents = ConvertToDot(graph_contents);

            if (graph_contents != null)
                bm = Graph.Run(graph_contents);
            
            return bm;
        }

        public string ConvertToDot(string contents)
        {
            // TODO: Parse non-graphviz files into dot format

            return null;
        }
    }
}