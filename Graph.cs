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

        // For Finite Testing
        private State initState = new State(null, false, new List<Transition>());
        private List<Transition> cycle = new List<Transition>();
        //private Dictionary<int, List<State>> cycles = new Dictionary<int, List<State>>(); // cycle ID : list of states part of the cycle

        // For Regex
        private int state_counter;

        // NFA --> DFA
        private List<string> states2calculate = new List<string>(); // state labels generated through table
        private Dictionary<char, List<string>> letters_states = new Dictionary<char, List<string>>(); // letter | state label --> for table
        public bool convertedDFA = false;

        // PDA
        public bool isPDA = false;
        private Stack<string> pdaStack = new Stack<string>();
        private string stack_val = "";

        public Graph(MainForm form)
        {
            Debug.WriteLine("Initializing Graphviz...");
            this.form = form;

            this.states = new List<State>();
            this.words = new Dictionary<string, bool>();
            this.all_transitions = new List<Transition>();
            this.final_states = new List<State>();
            this.alphabet = "";

            this.bm_graph = null;

            this.state_counter = 0;
            // TODO: Use list or stack
            //this.open_state = new State(null, false, null);
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
            this.isPDA = false;
            Bitmap bm = null;
            string[] file_contents = File.ReadAllLines(@path);
            // Do not read any commented lines + join all lines in 1 string
            string graph_contents = string.Join("", file_contents.Where(line => !line.StartsWith('#')));

            // Display the contents in the ui text box
            this.form.ui_rtb_filecontents.Lines = file_contents;
            // Store them to be able to switch back to it in file contents textbox
            this.form.graph_file_contents = file_contents;

            // Check what type of file it is (default, dot, regex)

            if (graph_contents.StartsWith("regex"))
                graph_contents = ParseRegexFile(file_contents); // pass content as multi-line
            else if (graph_contents.StartsWith("digraph") || graph_contents.StartsWith("graph"))
                graph_contents = ParseDotFile(graph_contents);
            else if (graph_contents.Contains("stack"))
                graph_contents = ParsePDAFile(file_contents);
            else // Default format (Given by ale2 course)
                graph_contents = ParseDefaultFile(file_contents); // pass content as multi-line

            if (graph_contents != null)
                bm = Graph.Run(graph_contents);

            // Store current graph dot contents (for edit + refresh)
            this.form.graph_dot = graph_contents;
            return bm;
        }

        public string ParseDefaultFile(string[] contents)
        {
            Debug.WriteLine($"Parsing Default file...");
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

                    foreach (State state in this.states.Where(s => finalstates.Contains(s.state_label)))
                    {
                        state.isFinal = true;
                        this.final_states.Add(state);
                        Debug.WriteLine($"Set '{state.state_label}' as final state.");
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
                        State tfrom = this.states.FirstOrDefault(s => s.state_label == trans_from);
                        State tTo = this.states.FirstOrDefault(s => s.state_label == trans_to);

                        Transition transition = new Transition(tfrom, tTo, trans_value);

                        if (transition != null)
                            all_transitions.Add(transition);
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

                        this.words[contents[i].Split(",")[0].Trim()] = contents[i].Split(",")[1].Trim() == "y" ? true : false;
                    }
                }
            }

            // TODO: Assume name will always be in the first line of the file (miuns the '#' character in the start)
            return CreateDotFromParsedFile();
        }

        public string ParseRegexFile(string[] contents)
        {
            Debug.WriteLine($"Parsing Regex file...");

            form.regexManager = new NodeRegexManager();

            int line_index = -1;
            foreach (string line in contents)
            {
                line_index++;
                if (line.StartsWith('#'))
                    continue;
                // Parse regex expression
                else if (line.StartsWith("regex"))
                {
                    line.Trim();
                    form.regexManager.formula = line.Split(":")[1].Trim();
                    // Parse nodes with their children
                    form.regexManager.AddNode(form.regexManager.formula);

                    // parse to graph properties + create dot
                    state_counter = 0;
                    ParseRegexContents(form.regexManager.operators[0]); // sets states + transitions

                    // In regex, last state is always final state
                    this.final_states.Add(states.Last());
                    // Set the alphabet (operants)
                    foreach (OperantRegex letter in form.regexManager.operants)
                    { this.alphabet += letter.Value; }
                }
                //Get whether or not the file says the graph is a dfa
                else if (line.StartsWith("dfa"))
                {
                    line.Trim();
                    isDFA_file = line.Split(":")[1].Trim() == "y" ? true : false;
                    isDFA = CheckDFA();

                    if (isDFA != isDFA_file)
                                            {
                        form.ui_pb_dfa.BackColor = Color.Gray;
                        form.ui_tooltip_info.SetToolTip(form.ui_pb_dfa, "File is wrong.");
                    }
                    else
                    {
                        form.ui_pb_dfa.BackColor = Color.Transparent;
                        form.ui_tooltip_info.SetToolTip(form.ui_pb_dfa, "");
                    }
                }
                // Get whether or not the file says the graph is finite
                else if (line.StartsWith("finite"))
                {
                    isFinite_file = line.Split(":")[1].Trim() == "y" ? true : false;
                    isFinite = CheckFinite();

                    if (isFinite != isFinite_file)
                    {
                        form.ui_pb_finite.BackColor = Color.Gray;
                        form.ui_tooltip_info.SetToolTip(form.ui_pb_finite, "File is wrong.");
                    }
                    else
                    {
                        form.ui_pb_finite.BackColor = Color.Transparent;
                        form.ui_tooltip_info.SetToolTip(form.ui_pb_finite, "");
                    }
                }
                // Get words
                else if (line.StartsWith("words"))
                {
                    for (int i = line_index + 1; i < contents.Length; i++)
                    {
                        if (contents[i] == "end.")
                            break;

                        if (contents[i].Length > 0)
                            words[contents[i].Split(",")[0].Trim()] = contents[i].Split(",")[1].Trim() == "y" ? true : false;
                    }
                }
            }

            return CreateDotFromParsedFile(); //form.regexManager.formula.Trim()
        }

        private void ParseRegexContents(OperatorRegex parent_node, State o_state = null, State f_state = null)
        {
            // If root node
            if (form.regexManager.operators[0].ID == parent_node.ID)
            {
                // Add starting state
                State s0 = new State($"q{state_counter++}", false, null);
                states.Add(s0);
            }

            // Get state to grow from
            if (o_state == null) {
                o_state = states.Last();
                Debug.WriteLine($"Open state: {o_state.state_label}");
            }

            // Set state to end to
            if (f_state == null) {
                f_state = new State($"q{state_counter++}", false, null);
                Debug.WriteLine($"Final state: {f_state.state_label}");
            }

            Debug.WriteLine($"Operator: '{parent_node.Value}'");
            switch (parent_node.Value)
            {
                case '.':
                    // Create needed states
                    State s1_concat = null;
                    State s2_concat = null;

                    if (parent_node.Left_child != null)
                    {
                        Debug.Write($"\tLeft child");
                        if (parent_node.Left_child is OperantRegex)
                        {
                            Debug.WriteLine($" - Operant");
                            s1_concat = new State($"q{state_counter++}", false, null);

                            // Create transition from last open state to new s1 state                            
                            Transition l_transition = new Transition(o_state, s1_concat, parent_node.Left_child.Value.ToString()); //this.open_state
                            all_transitions.Add(l_transition);
                            states.Add(s1_concat);
                        }
                        else {
                            Debug.WriteLine($" - Operator");
                            ParseRegexContents((OperatorRegex)parent_node.Left_child, o_state, f_state);
                        }
                    }

                    // Change open state for right child
                    o_state = states.Last();
                    Debug.WriteLine($"Open State changed to: {o_state.state_label}");
                    if (parent_node.Right_child != null)
                    {
                        Debug.Write($"\tRight child");
                        if (parent_node.Right_child is OperantRegex)
                        {
                            Debug.WriteLine($" - Operant");
                            s2_concat = new State($"q{state_counter++}", false, null);

                            // Create transition from last open state to new s1 state
                            Transition r_transition = new Transition(o_state, s2_concat, parent_node.Right_child.Value.ToString()); //this.open_state
                            all_transitions.Add(r_transition);
                            states.Add(s2_concat);
                        }
                        else {
                            Debug.WriteLine($" - Operator");
                            ParseRegexContents((OperatorRegex)parent_node.Right_child, o_state, f_state);
                        }
                    }

                    break;
                case '|':
                    // Initialize both left and right empty transitions to initial choice states
                    //left
                    State s1_choice = new State($"q{state_counter++}", false, null);
                    Transition e1_left = new Transition(o_state, s1_choice, "_");

                    //right
                    State s2_choice = new State($"q{state_counter++}", false, null);
                    Transition e1_right = new Transition(o_state, s2_choice, "_");

                    // Initialize final choice states for e transitions | (top) s1,a --> s3,e --> | (bottom) s2,b --> s4,e --> |
                    State s3_choice = null;
                    State s4_choice = null;
                    Transition e2_left = null;
                    Transition e2_right = null;

                    // Complete left side
                    if (parent_node.Left_child != null)
                    {
                        Debug.Write($"\tLeft child");
                        if (parent_node.Left_child is OperantRegex)
                        {
                            Debug.WriteLine($" - Operant");
                            s3_choice = new State($"q{state_counter++}", false, null);
                            Transition l1_choice = new Transition(s1_choice, s3_choice, parent_node.Left_child.Value.ToString());
                            all_transitions.Add(l1_choice);
                        }
                        else {
                            Debug.WriteLine($" - Operator");
                            ParseRegexContents((OperatorRegex)parent_node.Left_child, s1_choice, f_state);
                            
                            // previous f_state becomes s3_choice state
                            s3_choice = f_state;
                            f_state = new State($"q{state_counter++}", false, null);
                        }

                        this.states.Add(s1_choice);
                    }

                    // Complete right side
                    if (parent_node.Right_child != null)
                    {
                        Debug.Write($"\tRight child");
                        if (parent_node.Right_child is OperantRegex)
                        {
                            Debug.WriteLine($" - Operant");
                            s4_choice = new State($"q{state_counter++}", false, null);
                            Transition r1_choice = new Transition(s2_choice, s4_choice, parent_node.Right_child.Value.ToString());
                            all_transitions.Add(r1_choice);
                        }
                        else {
                            Debug.WriteLine($" - Operator");
                            ParseRegexContents((OperatorRegex)parent_node.Right_child, s2_choice, f_state);

                            // previous f_state becomes s4_choice state
                            s4_choice = f_state;
                            f_state = new State($"q{state_counter++}", false, null);
                        }

                        this.states.Add(s2_choice);
                    }

                    // Create 2 empty transitions to the final state
                    e2_left = new Transition(s3_choice, f_state, "_");
                    this.states.Add(s3_choice);
                    e2_right = new Transition(s4_choice, f_state, "_");
                    this.states.Add(s4_choice);

                    // Add all transitions to list (for graph dot conversion)
                    Transition[] trans_temp = { e1_left, e1_right, e2_left, e2_right };
                    all_transitions.AddRange(trans_temp);
                    this.states.Add(f_state); //s5

                    break;
                case '*':
                    // Creating needed states
                    State s1 = new State($"q{state_counter++}", false, null);
                    // e Transition from open_state to next state
                    Transition e_mid = new Transition(o_state, s1, this.form.epsilon); // open_state

                    State s2 = new State($"q{state_counter++}", false, null);
                    // Check if child is an operant
                    if (parent_node.Left_child != null)
                    {
                        if (parent_node.Left_child is OperantRegex) {
                            Transition l_mid = new Transition(s1, s2, parent_node.Left_child.Value.ToString());
                            all_transitions.Add(l_mid);
                        }
                        else
                            ParseRegexContents((OperatorRegex)parent_node.Left_child, s1, s2);
                    }

                    Transition e_top = new Transition(s2, o_state, this.form.epsilon); // open_state

                    //State s3 = new State($"q{state_counter++}", false, null);
                    Transition e_final = new Transition(s2, f_state, this.form.epsilon);
                    Transition e_bot = new Transition(o_state, f_state, this.form.epsilon); // open_state

                    Transition[] loop_trans = { e_mid, e_top, e_final, e_bot };
                    State[] statesToAdd = { s1, s2, f_state }; // s3
                    this.all_transitions.AddRange(loop_trans);
                    this.states.AddRange(statesToAdd);

                    break;
                default:
                    break;
            }
        }

        public string ParsePDAFile(string[] contents)
        {
            Debug.WriteLine($"Parsing PDA file...");
            this.isPDA = true;
            try
            {
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
                    // Get stack values
                    else if (line.StartsWith("stack"))
                    {
                        sfrom = line.IndexOf("stack") + "stack:".Length;
                        this.stack_val = line.Substring(sfrom, line.Length - sfrom).Trim();
                        Debug.WriteLine($"Stack parsed: {this.stack_val}");
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

                        foreach (State state in this.states.Where(s => finalstates.Contains(s.state_label)))
                        {
                            state.isFinal = true;
                            this.final_states.Add(state);
                            Debug.WriteLine($"Set '{state.state_label}' as final state.");
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
                            // TODO: Find how to split
                            string[] comma_split = contents[i].Split(",");

                            string trans_from = "";
                            string trans_value = "";
                            string pop_val = "";
                            string push_val = "";
                            string trans_to = "";

                            if (comma_split.Length == 2) // 1 comma --> 2 elements | eg., A,b --> A == { A },{ b --> A }
                            {
                                // Separate the line's contents
                                trans_from = contents[i].Split(",")[0].Trim();

                                trans_value = contents[i].Split(",")[1].Split("-->")[0].Trim();
                                trans_to = contents[i].Split(",")[1].Split("-->")[1].Trim();
                            }
                            else // Most likely 2 commas --> 3 elements | eg., A,b [_,x] --> A == { A },{ b [_ },{ x] --> A }
                            {
                                // { A }
                                trans_from = contents[i].Split(",")[0].Trim();

                                // { b [_ } : Split at [
                                trans_value = contents[i].Split(",")[1].Split("[")[0].Trim();
                                pop_val = contents[i].Split(",")[1].Split("[")[1].Trim();

                                //{ x] --> A } : Split at ] for push val and --> for state-to val
                                push_val = contents[i].Split(",")[2].Split("]")[0].Trim();
                                trans_to = contents[i].Split(",")[2].Split("-->")[1].Trim();
                            }

                            // Add them to the states + new transition class objects
                            State tfrom = this.states.FirstOrDefault(s => s.state_label == trans_from);
                            State tTo = this.states.FirstOrDefault(s => s.state_label == trans_to);

                            Transition transition = new Transition(tfrom, tTo, trans_value);

                            if (transition != null)
                            {
                                transition.AddStack(pop_val, push_val);
                                all_transitions.Add(transition);
                            }
                        }
                    }
                    // Get words
                    else if (line.StartsWith("words"))
                    {
                        for (int i = line_index + 1; i < contents.Length; i++)
                        {
                            if (contents[i] == "end.")
                                break;

                            // If line is empty
                            if (contents[i] == "")
                                continue;

                            this.words[contents[i].Split(",")[0].Trim()] = contents[i].Split(",")[1].Trim() == "y" ? true : false;
                        }
                    }
                }

                return this.CreateDotFromParsedFile("PDA2Dot");
            }
            catch (Exception ex) {
                Debug.WriteLine("Could not parse PDA file.\n\t-" + ex.ToString());
                return null;
            }
        }

        private string CreateDotFromParsedFile(string name = "name")
        {
            Debug.WriteLine("\nCreating Dot file from parsed values...");

            // Create dot file from parsed values
            string dot_contents = $"digraph {name} " + "{rankdir=LR;\n\n";

            // set final state in drawing
            foreach (State finalstate in this.final_states) {
                Debug.WriteLine($"Setting final state: {finalstate.state_label}");

                dot_contents += "node [shape = doublecircle]; \"" + finalstate.state_label + "\";\n";
            }

            // set shape for the rest of the states
            dot_contents += "node [shape = circle];\n";

            // set transitions
            foreach (Transition tr in this.all_transitions) {
                if (tr != null) {
                    Debug.WriteLine("Setting transition:\t" + tr.ToString());
                    dot_contents += $"\t\"{tr.startsFrom.state_label}\" -> \"{tr.pointsTo.state_label}\"";
                    dot_contents += $"[label = \"{tr.GetFullLabel()}\"];\n";
                }
            }

            // set starting arrow to initial state
            Debug.WriteLine($"Setting initial state: {this.states[0].state_label}");

            dot_contents += "\nnode [shape=point,label=\"\"]ENTRY;\n";

            dot_contents += $"ENTRY->\"{this.states[0].state_label}\" [label=\"Start\"];\n";

            dot_contents += "}";

            return dot_contents;
        }

        private string ParseDotFile(string contents)
        {
            // TODO: Add info to objects
            return contents;
        }

        public bool CheckDFA()
        {
            Debug.WriteLine("Checking DFA...");
            // Check if graph is DFA

            // For DFA there are no empty/e characters
            bool hasEmptyChar = this.all_transitions.Any(t => t.label == this.form.epsilon.ToString());

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

        public string NDFA2DFA()
        {
            Debug.WriteLine("\n\nConverting NDFA to a DFA...");

            // Store a copy of the final states temp
            List<State> prev_final_states = new List<State>(this.final_states);

            // Reform states if epsilon transitions
            CheckForEpsilonClosures();

            this.letters_states.Clear();
            // Initialise groups dictionary (based on letters from alphabet)
            foreach (char lt in this.alphabet.ToCharArray())
                this.letters_states.Add(lt, new List<string>());

            states2calculate.Add(this.states[0].state_label); // Add the initial state to start the table (label)
            bool isDFAGraphReady = NDFA2DFA_CalculateState(this.states[0]);

            if (isDFAGraphReady)
            {
                DebugNDFA2DFA();
                convertedDFA = true;

                this.form.ui_btn_ndfa2dfa.Enabled = false;
                this.form.ui_btn_ndfa2dfa.Text = "Refresh";

                try
                {
                    // Reparse values
                    this.states.Clear();
                    this.all_transitions.Clear();
                    this.final_states.Clear();

                    // Create all states available
                    foreach (string l_state in states2calculate)
                    {
                        // Set states to final if they contain a state in their label that was previously a final state
                        bool s_final = prev_final_states.Any(os => l_state.Contains(os.state_label));
                        foreach (State os in prev_final_states) {
                            Debug.WriteLine($"{os.state_label} is final: {os.isFinal}");
                        }

                        State state_temp = null;
                        if (l_state == "SINK")
                        {
                            state_temp = new State("SINK", false, null);
                            // Create self-loop transitions with each letter of the alphabet to this state
                            foreach (char lttr in this.alphabet.ToCharArray()) {
                                Transition tr_sink = new Transition(state_temp, state_temp, lttr.ToString());
                            }
                        }
                        else
                            state_temp = new State(l_state, s_final, null);

                        if (state_temp != null)
                        {
                            if (s_final) {
                                this.final_states.Add(state_temp);
                                Debug.WriteLine($"{state_temp.state_label} added to final states list.");
                            }
                            this.states.Add(state_temp);
                        }
                    }

                    // Create/Add appropriate transitions between the states
                    int letter_states_index = 0;
                    foreach (char letter in this.alphabet.ToCharArray())
                    {
                        foreach (State st_from in this.states)
                        {
                            Debug.WriteLine($"index: {letter_states_index} - {this.letters_states[letter][letter_states_index]}");
                            State st_to = this.states.FirstOrDefault(s => s.state_label.Equals(this.letters_states[letter][letter_states_index]));
                            if (st_to != null)
                            {
                                Transition transition_temp = new Transition(st_from, st_to, letter.ToString());
                                this.all_transitions.Add(transition_temp);
                            }
                            letter_states_index++;
                        }
                        letter_states_index = 0;
                    }

                    DebugParsedValues();
                    // Return a dot format using the new values
                    return CreateDotFromParsedFile();
                }
                catch (Exception e)
                {
                    this.form.ui_btn_ndfa2dfa.Text = "To DFA";
                    Debug.WriteLine($"Could not reparse values. - {e.ToString()}");
                }
            }

            return null;
        }

        private bool NDFA2DFA_CalculateState(State curr_state, int tb_index = 0)
        {
            try
            {
                Debug.WriteLine($"Calculating state {curr_state.state_label}...");

                // Go through every letter and fill in the table row for the current state
                foreach (char letter in this.alphabet.ToCharArray())
                {
                    Debug.WriteLine($"Processing {letter}...");

                    List<Transition> poss_transitions = new List<Transition>();

                    // Check what state the letter might have a transition to 
                    string[] curr_states = Array.Empty<string>();
                    if (curr_state.state_label.Contains(',')) // If state has multiple state values, check transitions from all of them
                    {
                        curr_states = curr_state.state_label.Split(',');

                        foreach (string cs in curr_states)
                        {
                            Debug.WriteLine($"Checking transitions for {cs}...");
                            State sep_state = this.states.Find(s => s.state_label == cs);
                            if (sep_state != null)
                                poss_transitions.AddRange(sep_state.FindTransitionsByValue(letter.ToString(), true));
                        }
                    }
                    else // Has single state value
                        // find outgoing transitions with the same letter
                        poss_transitions = curr_state.FindTransitionsByValue(letter.ToString(), true);

                    // Once we know all possible transitions, form the states as needed
                    Debug.WriteLine($"{poss_transitions.Count()} possible transitions");
                    if (poss_transitions != null && poss_transitions.Count > 0)
                    {
                        string out_label = "";
                        foreach (Transition poss_tr in poss_transitions)
                        {
                            // Check if state was already written (avoid duplicates)
                            if (out_label.Contains(poss_tr.pointsTo.state_label))
                                break;

                            Debug.WriteLine($"Transition {poss_tr.label} points to {poss_tr.pointsTo.state_label}");

                            // Add the state the transition is pointing to to the new state's label
                            out_label += poss_tr.pointsTo.state_label + ",";
                        }

                        // delete extra comma at the end
                        out_label = out_label.Substring(0, out_label.Length - 1);

                        Debug.WriteLine($"Final new state label: {out_label} under '{letter}'");
                        // Add state to letters_states under the letter
                        this.letters_states[letter].Add(out_label);
                    }
                    // If no possible transitions with that letter, create a sink state to transition to
                    else
                    {
                        Debug.WriteLine($"No transitions with {letter} from {curr_state.state_label}, marking as 'SINK'");
                        this.letters_states[letter].Add("SINK");
                    }
                }

                Debug.WriteLine($"\nChecking for new formed states...");
                foreach (KeyValuePair<char, List<string>> l_state in this.letters_states)
                {
                    string focusedState = l_state.Value[tb_index];
                    Debug.WriteLine($"{tb_index}: Checking if {focusedState} is a new state...");

                    // If there are no states with the same label as the state we're checking add new state
                    if (this.states2calculate.All(s => s != focusedState)) //this.states
                    {
                        Debug.WriteLine($"{focusedState} IS new.");
                        // Create new state (for future checking)
                        State new_state = new State(focusedState, false, null);
                        this.states.Add(new_state);
                        // Add new state to table (states2calculate)
                        this.states2calculate.Add(focusedState);

                        // call this method again with new state looking at next table index
                        NDFA2DFA_CalculateState(new_state, this.states2calculate.Count() - 1);
                    } else
                        Debug.WriteLine($"{focusedState} is NOT new.");
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Could not convert graph to DFA - " + e.Message + " - " + e.ToString());
                return false;
            }
        }

        private bool CheckForEpsilonClosures()
        {
            Debug.WriteLine("\nEpsilon Enclosure");
            bool hasEpsilonClosures = false;
            foreach (State st in this.states)
            {
                Debug.Write($"State {st.state_label} turned to: ");

                // true for outgoing only, true for recursive epsilon transitions
                List<Transition> outEpsilonTransitions = st.FindTransitionsByValue(this.form.epsilon.ToString(), true, true);

                // If there are outgoing epsilon transitions to other states from current state, create a closure of both
                if (outEpsilonTransitions != null) {

                    hasEpsilonClosures = true;
                    foreach (Transition e_tr in outEpsilonTransitions) {
                        if (!st.state_label.Contains(e_tr.pointsTo.state_label))
                            st.state_label += "," + e_tr.pointsTo.state_label;
                    }
                }

                Debug.WriteLine($"\t{st.state_label}");
            }

            return hasEpsilonClosures;
        }

        public bool CheckFinite()
        {
            // Check if graph is Finite
            Debug.WriteLine("Checking if Finite...");

            try
            {
                this.isFinite = true;

                // Check for self-loop
                foreach (State st in this.states)
                {
                    List<Transition> selfLoops = st.transitions.Where((t) => t.isSelfLoop()).ToList();

                    // Check if there are paths from that loop/state to the final state
                    foreach (Transition loop in selfLoops)
                    {
                        this.isFinite = !HasPathToFinal(st); // If there is path, graph is infinite

                        // if there is a single self-loop that is connected to a final state, it settles it
                        if (this.isFinite == false)
                            break;

                    }
                }

                // Check for Cycle
                foreach (State st in this.states)
                {
                    // Only check if any FINAL states belong to a cycle
                    if (st.isFinal)
                    {
                        initState = st;
                        // Check if is part of cycle AND that NOT ALL transitions in cycle are empty
                        if (IsPartOfCycle(initState) && !cycle.All(t => t.isEmpty))
                        {
                            Debug.WriteLine("NOT FINITE: final state part of cycle");
                            this.isFinite = false;
                            break;
                        }
                    }
                }

                // Set tick/x image based on truth value
                this.form.ui_pb_finite.Image = this.isFinite ? new Bitmap(Properties.Resources.tick) : new Bitmap(Properties.Resources.x);
                return this.isFinite;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Couldn't check if graph is finite: - {ex.ToString()}");
            }

            return false;
        }

        private bool HasPathToFinal(State st)
        {
            if (st.isFinal)
                return true;

            // Get possible transitions that are not self-loops
            List<Transition> possibleTransitions = st.transitions
                .Where(t => !t.isSelfLoop() && t.startsFrom.Equals(st)).ToList();

            if (possibleTransitions != null && possibleTransitions.Count > 0)
            {
                foreach (Transition pt in possibleTransitions)
                {
                    // If current state's transition points to a final state or
                    // has path to final state, return true
                    if (pt.pointsTo.isFinal || HasPathToFinal(pt.pointsTo))
                        return true;
                }
            }

            return false;
        }

        private bool IsPartOfCycle(State st)
        {
            // Add state to cycle list
            this.cycle.AddRange(st.transitions);

            // Get possible transitions that are not self-loops starting from the specified state
            List<Transition> possibleTransitions = st.transitions
                .Where(t => !t.isSelfLoop() && t.startsFrom.Equals(st)).ToList();

            if (possibleTransitions != null && possibleTransitions.Count > 0)
            {
                foreach (Transition pt in possibleTransitions)
                {
                    // If current transition points to the initial state we started with
                    // OR current transition points to a state that is part of the cycle
                    // it IS part of cycle
                    if (pt.pointsTo.Equals(this.initState) || IsPartOfCycle(pt.pointsTo))
                    {
                        return true;
                    }
                }
            }

            // Remove state from cycle list (since it had to go recursively back when no true value was returned)
            this.cycle.RemoveRange(this.cycle.Count - st.transitions.Count, st.transitions.Count());
            return false;
        }

        public void CheckWords(bool isPDA = false)
        {
            Debug.WriteLine($"Checking words - is PDA: {isPDA}");
            int rtb_index = 0;
            foreach (KeyValuePair<string, bool> word in this.words)
            {
                form.ui_rtb_words.AppendText(word.Key + " : " + word.Value + "\n");
                form.ui_rtb_words.SelectionStart = rtb_index;
                form.ui_rtb_words.SelectionLength = word.Key.Length;

                // Check if word is accepted
                bool accepted = CheckWord(word.Key, 0, isPDA);
                if (accepted) {
                    Debug.WriteLine($"Word '{word.Key}' accepted.");
                    form.ui_rtb_words.SelectionColor = Color.Green;
                }
                else
                    form.ui_rtb_words.SelectionColor = Color.Red;

                rtb_index = form.ui_rtb_words.Text.Length;

                // If file is wrong, color the truth value red
                if (!accepted.Equals(word.Value)) {
                    form.ui_rtb_words.SelectionStart = rtb_index - (word.Value.ToString().Length + 1);
                    form.ui_rtb_words.SelectionLength = word.Value.ToString().Length;
                    form.ui_rtb_words.SelectionColor = Color.Red;
                }
            }
        }

        public bool CheckWord(string word, int letter_index = 0, bool isPDA = false)
        {
            // If starting new word, clear stack and push first state
            if (letter_index == 0)
            {
                Debug.WriteLine($"\nChecking word '{word}'\n\tis PDA: {isPDA}\n\tStarting state: {this.states[0].state_label}");
                this.pdaStack.Clear();
                this.stateStack.Clear();
                this.stateStack.Push(this.states[0]);
            }

            if (word.Length == 0)
                word = "_";

            // Check transitions from current state (state on top of stack)
            List<Transition> possibleTransitions = this.stateStack.Peek().
                FindTransitionsByValue(word[letter_index].ToString(), true, true).ToList();

            if (possibleTransitions != null && possibleTransitions.Count > 0)
            {
                Debug.WriteLine($"Found {possibleTransitions.Count} from state '{this.stateStack.Peek().state_label}'");
                foreach (Transition pt in possibleTransitions)
                {
                    this.stateStack.Push(pt.pointsTo);
                    Debug.WriteLine($"Checking transition [{pt.ToString()}]");
                    
                    // If PDA and pop/push vals are initialized
                    if (isPDA && pt.popValue != null && pt.pushValue != null)
                    {
                        Debug.WriteLine($"Checking PDA Conditions...");
                        // has something to pop (not equals to epsilon) 
                        if (!pt.popValue.Equals(this.form.epsilon))
                        {
                            Debug.WriteLine($"Trying to pop {pt.popValue}...");

                            // if pdaStack is not empty and pop val is equal to the top of the stack
                            if (this.pdaStack.Count > 0 && this.pdaStack.Peek() == pt.popValue)
                            {
                                // pop stack
                                this.pdaStack.Pop();
                                Debug.WriteLine($"Successfully popped {pt.popValue}. stack count = {this.pdaStack.Count()}");
                            }
                            // if stack is empty OR what we want to pop is not at the top of the stack, check other possibilities
                            else
                            {
                                Debug.WriteLine($"Could not pop {pt.popValue}. stack count = {this.pdaStack.Count()}");
                                continue;
                            }
                        }

                        // if it has nothing to pop, just push if there is something to push
                        if (!pt.pushValue.Equals(this.form.epsilon))
                        {
                            this.pdaStack.Push(pt.pushValue);
                            Debug.WriteLine($"Pushed {pt.pushValue} to stack. stack count = {this.pdaStack.Count()}");
                        }
                    }

                    // Check if last letter
                    if (letter_index == word.Length - 1)
                    {
                        Debug.WriteLine($"'{word[letter_index]}' is the last letter. Checking if '{this.stateStack.Peek().state_label}' is final state...");
                        // Is state final?
                        if (this.stateStack.Peek().isFinal)
                        {
                            Debug.WriteLine($"'{this.stateStack.Peek().state_label}' IS a final state");
                            // If PDA, accept word only if stack is empty
                            if (isPDA && pt.popValue != null && pt.pushValue != null)
                            {
                                if (this.pdaStack.Count == 0)
                                    return true;
                            }
                            // If not PDA just accept word
                            else
                                return true;
                        }
                        else
                        {
                            Debug.WriteLine($"'{this.stateStack.Peek().state_label}' is NOT a final state. Checking other transitions...");
                            this.stateStack.Pop(); // pop stack to go back to previous parent
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"'{word[letter_index]}' is not the last letter\nChecking next letter...");
                        if (CheckWord(word, ++letter_index, isPDA))
                            return true;
                        else {
                            // Check other paths from previous state
                            this.stateStack.Pop();
                            // Point back to the previous letter
                            letter_index--;
                        }
                    }
                }
                Debug.WriteLine($"No more possible transitions with '{word[letter_index]}' from state '{this.stateStack.Peek().state_label}'");
            }
            else
                Debug.WriteLine($"Couldn't find transitions with letter {word[letter_index]} from {this.stateStack.Peek().state_label}");
            return false;
        }

        public void DebugParsedValues()
        {
            Debug.WriteLine("\n\n-- DEBUG - Parsed Values --\n");

            Debug.WriteLine($"Alphabet: {this.alphabet}");

            Debug.WriteLine("# of states: " + this.states.Count().ToString());
            foreach (State state in this.states)
            {
                Debug.WriteLine($"\n\t- {state.state_label}");

                Debug.WriteLine("\tTransitions:");
                foreach (Transition tr in state.transitions)
                {
                    if (tr != null)
                        Debug.WriteLine($"\t\t {tr.ToString()}");
                    else
                        Debug.WriteLine($"\t\t State '{state.state_label}' has null transitions");
                }                    
            }

            Debug.WriteLine("\nWords:");
            foreach (KeyValuePair<string, Boolean> word in this.words)
            {
                Debug.WriteLine($"\t- {word.Key} : {word.Value}");
            }
        }

        private void DebugNDFA2DFA()
        {
            int pad = 20;
            // DEBUG results
            Debug.WriteLine("");
            foreach (char lt in this.alphabet.ToCharArray())
                Debug.Write($"|{lt}".PadLeft(pad));

            for (int row = 0; row < states2calculate.Count(); row++)
            {
                Debug.WriteLine("");
                for (int col = 0; col <= this.alphabet.ToCharArray().Count(); col++)
                {
                    if (col == 0)
                        Debug.Write($"{states2calculate[row]}");
                    else
                        Debug.Write($"|{this.letters_states[this.alphabet.ToCharArray()[col - 1]][row]}".PadLeft(pad));
                }
            }

            Debug.WriteLine($"\n\ntotal # states: {states2calculate.Count()}");
            Debug.WriteLine($"total # states under letters: {this.letters_states.Values.Count()}");
        }
    }
}