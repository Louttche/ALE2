﻿using System;
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
        public Bitmap bm_graph;

        public string alphabet { get; set; }    // alphabet: wo
        public List<State> states { get; set; } // states: 1,2,3
        public List<State> final_states { get; set; }  // final: 3
        public bool isDFA { get; set; } // dfa: n
        public bool isFinite { get; set; }  // finite: n
        public bool isDFA_file { get; set; } // what does the file say
        public bool isFinite_file { get; set; }  // what does the file say
        public Dictionary<string, bool> words { get; set; } // words: word, y
        public List<Transition> all_transitions { get; set; } // all transitions

        private Stack<State> stateStack = new Stack<State>();

        public Graph(MainForm form)
        {
            Debug.WriteLine("Initializing Graphviz...");
            this.form = form;

            this.states = new List<State>();
            this.words = new Dictionary<string, bool>();
            this.all_transitions = new List<Transition>();
            this.final_states = new List<State>();

            this.bm_graph = null;

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

            // Do not read any commented lines + join all lines in 1 string
            string graph_contents = string.Join("", file_contents.Where(line => !line.StartsWith('#')));

            // Check if contents are in dot language and parse with the corresponding method
            if (!graph_contents.StartsWith("digraph") && !graph_contents.StartsWith("graph"))
                graph_contents = ParseContent(file_contents); // pass content as multi-line
            else
                graph_contents = ParseDotFile(graph_contents);

            if (graph_contents != null)
                bm = Graph.Run(graph_contents);

            // Store current graph dot contents (for edit + refresh)
            this.form.graph_dot = graph_contents;
            return bm;
        }

        public string ParseContent(string[] contents)
        {
            //string name_of_file = "";
            int sfrom = 0;
            int line_index = -1;
            foreach (string line in contents)
            {
                line_index++;
                if (line.StartsWith('#'))
                    continue;

                // Get alphabet
                else if (line.StartsWith("alphabet"))
                {
                    sfrom = line.IndexOf("alphabet") + "alphabet:".Length;
                    this.alphabet = line.Substring(sfrom, line.Length - sfrom).Trim();
                    Debug.WriteLine($"Alphabet parsed: {this.alphabet}");
                }
                // Get states (name only)
                else if (line.StartsWith("states"))
                {
                    sfrom = line.IndexOf("states") + "states:".Length;
                    string[] states = line.Substring(sfrom, line.Length - sfrom).Trim().Split(',');
                    foreach (string state in states)
                    {
                        this.states.Add(new State(state, false, null)); // A null transitions param will initialise a new List()
                        Debug.WriteLine($"State Name parsed: {state}");
                    }
                }
                // Get and set final state
                else if (line.StartsWith("final"))
                {//final: q2,q4
                    sfrom = line.IndexOf("final") + "final:".Length;
                    string[] finalstates = line.Substring(sfrom, line.Length - sfrom).Trim().Split(',');

                    foreach (State state in this.states.Where(s => finalstates.Contains(s.state_value)))
                    {
                        state.isFinal = true;
                        this.final_states.Add(state);
                        Debug.WriteLine($"Set '{state.state_value}' as final state.");
                    }
                }
                // Get and set transitions
                else if (line.StartsWith("transitions"))
                {
                    for (int i = line_index + 1; i < contents.Length; i++)
                    {
                        if (contents[i] == "end.")
                            break;

                        // Separate the line's contents
                        string trans_from = contents[i].Split(",")[0].Trim();
                        string trans_value = contents[i].Split(",")[1].Split("-->")[0].Trim();
                        string trans_to = contents[i].Split(",")[1].Split("-->")[1].Trim();

                        // Add them to the states + new transition class objects
                        State tfrom = this.states.FirstOrDefault(s => s.state_value == trans_from);
                        State tTo = this.states.FirstOrDefault(s => s.state_value == trans_to);

                        Transition transition = new Transition(tfrom, tTo, trans_value);

                        if (transition != null)
                        {
                            tfrom.AddTransition(transition);
                            // Only add to the destination state if its not a self-loop transition
                            // (or else same state gets duplicate transition in list)
                            if (!tfrom.Equals(tTo))
                                tTo.AddTransition(transition);

                            all_transitions.Add(transition);
                        }
                    }
                }
                // Get whether or not the file says the graph is a dfa
                else if (line.StartsWith("dfa"))
                {
                    line.Trim();
                    this.isDFA_file = line.Split(":")[1].Trim() == "y" ? true : false;
                    this.isDFA = CheckDFA();

                    if (this.isDFA != this.isDFA_file)
                    {
                        this.form.ui_pb_dfa.BackColor = Color.Gray;
                        this.form.ui_tooltip_info.SetToolTip(this.form.ui_pb_dfa, "File is wrong.");
                    }
                    else
                    {
                        this.form.ui_pb_dfa.BackColor = Color.Transparent;
                        this.form.ui_tooltip_info.SetToolTip(this.form.ui_pb_dfa, "");
                    }
                }
                // Get whether or not the file says the graph is finite
                else if (line.StartsWith("finite"))
                {
                    this.isFinite_file = line.Split(":")[1].Trim() == "y" ? true : false;
                    this.isFinite = CheckFinite();

                    if (this.isFinite != this.isFinite_file)
                    {
                        this.form.ui_pb_finite.BackColor = Color.Gray;
                        this.form.ui_tooltip_info.SetToolTip(this.form.ui_pb_finite, "File is wrong.");
                    }
                    else
                    {
                        this.form.ui_pb_finite.BackColor = Color.Transparent;
                        this.form.ui_tooltip_info.SetToolTip(this.form.ui_pb_finite, "");
                    }
                }
                // Get words
                else if (line.StartsWith("words"))
                {
                    for (int i = line_index + 1; i < contents.Length; i++)
                    {
                        if (contents[i] == "end.")
                            break;

                        this.words[contents[i].Split(",")[0].Trim()] = contents[i].Split(",")[1].Trim() == "y" ? true: false;
                    }
                }
            }

            // TODO: Assume name will always be in the first line of the file (miuns the '#' character in the start)
            return CreateDotFromParsedFile();
        }

        private string CreateDotFromParsedFile(string name = "example_name")
        {
            // TODO: Create dot file from parsed values
            string dot_contents = $"digraph {name}" + "{rankdir=LR;";

            // set final state in drawing
            foreach (State finalstate in this.final_states)
                dot_contents += "node [shape = doublecircle]; " + finalstate.state_value + ";";

            dot_contents += "node [shape = circle];";

            // set transitions
            foreach (Transition tr in this.all_transitions)
                dot_contents += $"{tr.startsFrom.state_value} -> {tr.pointsTo.state_value} [label = \"{tr.label}\"];";

            // set starting arrow to initial state
            dot_contents += "node [shape=point,label=\"\"]ENTRY;";
            dot_contents += $"ENTRY->{this.states[0].state_value} [label=\"Start\"];";

            dot_contents += "}";

            return dot_contents;
        }

        private string ParseDotFile(string contents)
        {
            // TODO: Add info to objects
            return contents;
        }

        private bool CheckDFA()
        {
            Debug.WriteLine("Checking DFA...");
            // Check if graph is DFA
            // For DFA there are no empty/e characters
            bool hasEmptyChar = this.all_transitions.Any(t => t.label.Length == 0 || t.label == "_");
            // For DFA each state should have transitions going out from itself for each  unique letter from the alphabet
            var nr_unique_letters = new HashSet<char>(this.alphabet);
            bool hasNDFATransition = this.states.Any(s => s.transitions.Where(t => t.startsFrom == s).Count() != nr_unique_letters.Count);
            
            if (hasEmptyChar || hasNDFATransition)
                this.isDFA = false;
            else
                this.isDFA = true;

            // Set tick/x image based on truth value
            this.form.ui_pb_dfa.Image = this.isDFA ? new Bitmap(Properties.Resources.tick) : new Bitmap(Properties.Resources.x);

            return this.isDFA;
        }

        private bool CheckFinite()
        {
            Debug.WriteLine("Checking Finite...");
            // Check if graph is Finite

            // Set tick/x image based on truth value
            this.form.ui_pb_finite.Image = this.isFinite ? new Bitmap(Properties.Resources.tick) : new Bitmap(Properties.Resources.x);
            return false;
        }

        public void CheckWords()
        {
            int rtb_index = 0;
            foreach (KeyValuePair<string, bool> word in this.words)
            {
                form.ui_rtb_words.AppendText(word.Key + " : " + word.Value + "\n");
                form.ui_rtb_words.SelectionStart = rtb_index;
                form.ui_rtb_words.SelectionLength = word.Key.Length;

                // Check if word is accepted
                bool accepted = CheckWord(word.Key, 0);
                if (accepted)
                    form.ui_rtb_words.SelectionColor = Color.Green;
                else
                    form.ui_rtb_words.SelectionColor = Color.Red;

                rtb_index = form.ui_rtb_words.Text.Length;

                // TODO: Check if file is wrong
                if (accepted != word.Value) {
                    form.ui_rtb_words.SelectionStart = rtb_index - (word.Value.ToString().Length + 1);
                    form.ui_rtb_words.SelectionLength = word.Value.ToString().Length;
                    form.ui_rtb_words.SelectionColor = Color.Red;
                }
            }
        }

        public bool CheckWord(string word, int letter_index = 0)
        {
            // If starting new word, clear stack and push first state
            if (letter_index == 0)
            {
                this.stateStack.Clear();
                this.stateStack.Push(this.states[0]);
            }

            // Check transitions from current state (state on top of stack)
            List<Transition> possibleTransitions = this.stateStack.Peek().FindTransitionsByValue(word[letter_index].ToString());

            if (possibleTransitions != null && possibleTransitions.Count > 0)
            {
                foreach (Transition pt in possibleTransitions)
                {
                    this.stateStack.Push(pt.pointsTo);

                    // Check if last letter
                    if (letter_index == word.Length - 1) {
                        if (this.stateStack.Peek().isFinal)
                            return true; // If current state is final, accept word
                        else
                        {
                            this.stateStack.Pop(); // pop stack to go back to previous parent
                            continue; // check other possible transitions
                        }
                    }
                    else
                        return CheckWord(word, ++letter_index);
                }
            }

            return false;
        }
    }
}