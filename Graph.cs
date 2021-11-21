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

            this.states = new List<State>();

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
            string[] graph_contents_lines = file_contents;

            // Check if contents are in dot language and parse with the corresponding method
            if (!graph_contents.StartsWith("digraph") && !graph_contents.StartsWith("graph"))
                graph_contents = ParseFile(graph_contents_lines);
            else
                graph_contents = ParseDotFile(graph_contents);

            if (graph_contents != null)
                bm = Graph.Run(graph_contents);
            
            return bm;
        }

        private string ParseFile(string[] contents)
        {
            string dot_contents = "";

            int sfrom = 0;
            int sto = 0;

            // Recreates the string array without the commented lines
            foreach (string line in contents) //string line in contents)
            {
                if (line.StartsWith('#'))
                    continue;
                else if (line.StartsWith("alphabet"))
                {
                    sfrom = line.IndexOf("alphabet") + "alphabet:".Length;
                    this.alphabet = line.Substring(sfrom, line.Length - sfrom).Trim();
                    Debug.WriteLine($"Alphabet parsed: {this.alphabet}");
                }
                else if (line.StartsWith("states"))
                {
                    // Get states (name only)
                    sfrom = line.IndexOf("states") + "states:".Length;
                    string[] states = line.Substring(sfrom, line.Length - sfrom).Trim().Split(',');
                    foreach (string state in states)
                    {
                        this.states.Add(new State(state, false, null)); // A null transitions param will initialise a new List()
                        Debug.WriteLine($"State Name parsed: {state}");
                    }
                }
                else if (line.StartsWith("final"))
                {
                    // Get and set final state
                    sfrom = line.IndexOf("final") + "final:".Length;
                    string finalstate = line.Substring(sfrom, line.Length - sfrom).Trim();
                    this.states.FirstOrDefault(s => s.state_value == finalstate).isFinal = true;
                    Debug.WriteLine($"Set '{finalstate}' as final state.");
                }
                else if (line.StartsWith("transitions"))
                {
                    //// Get and set transitions
                    //sfrom = contents.IndexOf("transitions") + "transitions:".Length;
                    //sto = contents.IndexOf("end.");
                    //string transition_string = contents.Substring(sfrom, sto - sfrom).Trim();

                    //string trans_from = transition_string.Split(",")[0];
                    //string trans_value = transition_string.Split(",")[1].Split("-->")[0];
                    //string trans_to = transition_string.Split(",")[1].Split("-->")[1];

                    ////Debug.WriteLine($"\nFrom: {trans_from}\nTo {trans_to}\nValue {trans_value}");
                    //Debug.WriteLine(transition_string.Split(",")[6]);
                }
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