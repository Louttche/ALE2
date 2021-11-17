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
        private MainForm form;

        public string alphabet { get; set; }    // alphabet: wo
        public List<State> states { get; set; } // states: 1,2,3
        public State final_state { get; set; }  // final: 3
        public bool isDFA { get; set; } // dfa: n
        public bool isFinite { get; set; }  // finite: n
        public Dictionary<string, bool> words { get; set; } // words: word, y

        public Graph(MainForm form)
        {
            Debug.WriteLine("Initializing Graphviz...");
            this.form = form;

            //TODO: Check if dot.exe file exists, if not browse to find it
        }

        public static Bitmap Run(string dot)
        {
            Debug.WriteLine("Running Graphviz...");

            // Gets the program from graphviz to parse and draw based on dot language
            string executable = @".\external\dot.exe";
            // Temporary file to be used to create graph
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
            if (!graph_contents.StartsWith("digraph") && !graph_contents.StartsWith("graph"))
                graph_contents = ParseFile(graph_contents);
            else
                graph_contents = ParseDotFile(graph_contents);

            if (graph_contents != null)
                bm = Graph.Run(graph_contents);
            
            return bm;
        }

        private string ParseFile(string contents)
        {
            string dot_contents = "";

            this.alphabet = contents.Substring(contents.IndexOf("alphabet"), contents.IndexOf("states"))
                .Remove(contents.IndexOf("alphabet"), "alphabet:".Length).Trim();

            string[] states = contents.Substring(contents.IndexOf("states"), contents.IndexOf("final"))
                .Trim().Split(',');

            foreach (string state in states)
            {
                Debug.WriteLine(state);
            }
            // TODO: add states to objects

            return ConvertToDot(dot_contents);
        }

        public string ConvertToDot(string contents)
        {
            // TODO: Parse non-graphviz files into dot format

            return null;
        }

        private string ParseDotFile(string contents)
        {
            // TODO: Add info to objects
            return contents;
        }
    }
}