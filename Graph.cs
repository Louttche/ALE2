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
        private State open_state;

        // NFA --> DFA
        private List<string> states2calculate = new List<string>(); // state labels generated through table
        private Dictionary<char, List<string>> letters_states = new Dictionary<char, List<string>>(); // letter | state label --> for table
        private int tableIndex = 0;
        public bool convertedDFA = false;

        // PDA
        private Stack<char> pdaStack = new Stack<char>();

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
            this.open_state = new State(null, false, null);
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
            else // Default format (Given by ale2 course)
                graph_contents = ParseDefaultFile(file_contents); // pass content as multi-line

            if (graph_contents != null)
                bm = Graph.Run(graph_contents);

            // Store current graph dot contents (for edit + refresh)
            this.form.graph_dot = graph_contents;
            return bm;
        }

        public string ParseRegexFile(string[] contents)
        {
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

            switch (parent_node.Value)
            {
                case '.':
                    // Create needed states
                    State s1_concat = new State($"q{state_counter++}", false, null);
                    State s2_concat = new State($"q{state_counter++}", false, null);

                    if (parent_node.Left_child != null)
                    {
                        if (parent_node.Left_child is OperantRegex)
                        {
                            // Create transition from last open state to new s1 state
                            this.open_state = states.Last();
                            
                            Transition l_transition = new Transition(this.open_state, s1_concat, parent_node.Left_child.Value.ToString());
                            this.open_state.AddTransition(l_transition);
                            s1_concat.AddTransition(l_transition);
                            all_transitions.Add(l_transition);

                            states.Add(s1_concat);
                        }
                        else
                            ParseRegexContents((OperatorRegex) parent_node.Left_child);
                    }

                    if (parent_node.Right_child != null)
                    {
                        if (parent_node.Right_child is OperantRegex)
                        {
                            // Create transition from last open state to new s1 state
                            this.open_state = states.Last();

                            Transition r_transition = new Transition(this.open_state, s2_concat, parent_node.Right_child.Value.ToString());
                            this.open_state.AddTransition(r_transition);
                            s2_concat.AddTransition(r_transition);
                            all_transitions.Add(r_transition);

                            states.Add(s2_concat);
                        }
                        else
                            ParseRegexContents((OperatorRegex)parent_node.Right_child);
                    }

                    break;
                case '|':

                    // Get state to grow from
                    if (o_state == null)
                        this.open_state = states.Last();
                    else
                        this.open_state = o_state;

                    // final state of Choice + its empties
                    State s5_choice = new State($"q{state_counter++}", false, null);
                    Transition e2_left = null;
                    Transition e2_right = null;

                    // Add empty transition connecting final stages (if recursive '|')
                    if (f_state != null)
                    {
                        Transition f_empty = new Transition(s5_choice, f_state, "_");
                        f_state.AddTransition(f_empty);
                        s5_choice.AddTransition(f_empty);
                        all_transitions.Add(f_empty);
                    }

                    // Set up both left and right empty transitions
                    //left
                    State s1_choice = new State($"q{state_counter++}", false, null);
                    Transition e1_left = new Transition(this.open_state, s1_choice, "_");
                    this.open_state.AddTransition(e1_left);
                    s1_choice.AddTransition(e1_left);

                    //right
                    State s2_choice = new State($"q{state_counter++}", false, null);
                    Transition e1_right = new Transition(this.open_state, s2_choice, "_");
                    this.open_state.AddTransition(e1_right);
                    s2_choice.AddTransition(e1_right);

                    // Complete left side
                    if (parent_node.Left_child != null)
                    {
                        if (parent_node.Left_child is OperantRegex)
                        {
                            State s3_choice = new State($"q{state_counter++}", false, null);
                            Transition l1_choice = new Transition(s1_choice, s3_choice, parent_node.Left_child.Value.ToString());
                            s1_choice.AddTransition(l1_choice);
                            s3_choice.AddTransition(l1_choice);
                            all_transitions.Add(l1_choice);

                            e2_left = new Transition(s3_choice, s5_choice, "_");
                            s5_choice.AddTransition(e2_left);
                            s3_choice.AddTransition(e2_left);
                            this.states.Add(s3_choice);
                        }
                        else
                            ParseRegexContents((OperatorRegex)parent_node.Left_child, s1_choice, s5_choice);

                        this.states.Add(s1_choice);
                    }

                    // Complete right side
                    if (parent_node.Right_child != null)
                    {
                        if (parent_node.Right_child is OperantRegex)
                        {
                            State s4_choice = new State($"q{state_counter++}", false, null);
                            Transition r1_choice = new Transition(s2_choice, s4_choice, parent_node.Right_child.Value.ToString());
                            s2_choice.AddTransition(r1_choice);
                            s4_choice.AddTransition(r1_choice);
                            all_transitions.Add(r1_choice);

                            e2_right = new Transition(s4_choice, s5_choice, "_");
                            s5_choice.AddTransition(e2_right);
                            s4_choice.AddTransition(e2_right);
                            this.states.Add(s4_choice);
                        }
                        else
                            ParseRegexContents((OperatorRegex)parent_node.Right_child, s2_choice, s5_choice);

                        this.states.Add(s2_choice);
                    }

                    // Add all transitions to list (for graph dot conversion)
                    Transition[] trans_temp = { e1_left, e1_right, e2_left, e2_right };
                    all_transitions.AddRange(trans_temp);
                    this.states.Add(s5_choice);

                    break;
                case '*':
                    // TODO: Draw * static layout
                    break;
                default:
                    break;
            }
        }

        public string ParseDefaultFile(string[] contents)
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

        private string CreateDotFromParsedFile(string name = "name")
        {
            char comma_replacement = 'n';
            Debug.WriteLine("\nCreating Dot file from parsed values...");

            // Create dot file from parsed values
            string dot_contents = $"digraph {name} " + "{rankdir=LR;\n\n";

            // set final state in drawing
            foreach (State finalstate in this.final_states) {
                Debug.WriteLine($"Setting final state: {finalstate.state_label}");

                if (finalstate.state_label.Contains(','))
                    finalstate.state_label = finalstate.state_label.Replace(',', comma_replacement);

                dot_contents += "node [shape = doublecircle]; " + finalstate.state_label + ";\n";
            }

            // set shape for the rest of the states
            dot_contents += "node [shape = circle];\n";

            // set transitions
            foreach (Transition tr in this.all_transitions) {
                if (tr != null) {
                    Debug.WriteLine($"Setting transition: {tr.label}: {tr.startsFrom.state_label} --> {tr.pointsTo.state_label}");

                    if (tr.startsFrom.state_label.Contains(','))
                        tr.startsFrom.state_label = tr.startsFrom.state_label.Replace(',', comma_replacement);

                    if (tr.pointsTo.state_label.Contains(','))
                        tr.pointsTo.state_label = tr.pointsTo.state_label.Replace(',', comma_replacement);

                    dot_contents += $"\t{tr.startsFrom.state_label} -> {tr.pointsTo.state_label} [label = \"{tr.label}\"];\n";
                }
            }

            // set starting arrow to initial state
            Debug.WriteLine($"Setting initial state: {this.states[0].state_label}");

            dot_contents += "\nnode [shape=point,label=\"\"]ENTRY;\n";

            if (this.states[0].state_label.Contains(','))
                this.states[0].state_label = this.states[0].state_label.Replace(',', comma_replacement);

            dot_contents += $"ENTRY->{this.states[0].state_label} [label=\"Start\"];\n";

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
            // Reform states if epsilon transitions
            CheckForEpsilonClosures();

            tableIndex = 0;
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

                // Reparse values
                this.states.Clear();
                this.all_transitions.Clear();
                this.final_states.Clear();

                // Create all states available
                foreach (string l_state in states2calculate) {
                    // isFinal if is the last state
                    State state_temp = new State(l_state, states2calculate.Last().Equals(l_state) ? true : false, null);
                    this.states.Add(state_temp); }

                // Create/Add appropriate transitions between the states
                int letter_states_index = 0;
                foreach (char letter in this.alphabet.ToCharArray())
                {
                    foreach (State st_from in this.states)
                    {
                        State st_to = this.states.First(s => s.state_label == this.letters_states[letter][letter_states_index]);
                        Transition transition_temp = new Transition(st_from, st_to, letter.ToString());

                        st_from.AddTransition(transition_temp);
                        st_to.AddTransition(transition_temp);
                        this.all_transitions.Add(transition_temp);
                        letter_states_index++;
                    }
                    letter_states_index = 0;
                }

                // Add last found state as final state
                this.final_states.Add(this.states.Last());

                DebugParsedValues();
                
                // Return a dot format using the new values
                return CreateDotFromParsedFile();
            }

            return null;
        }

        private bool NDFA2DFA_CalculateState(State curr_state)
        {
            try
            {
                Debug.WriteLine($"Calculating state {curr_state.state_label}...");

                // Go through every letter
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

                        Debug.WriteLine($"Final new state label: {out_label}");
                        // Add state to letters_states under the letter
                        //this.letters_states[letter] = out_label;
                        this.letters_states[letter].Add(out_label);
                    }
                    // If no possible transitions with that letter, create a sink state to transition to
                    else
                    {
                        Debug.WriteLine($"Creating SINK state for letter {letter} from {curr_state.state_label}");
                        State sink_state = new State("SINK", false, null);
                        Transition sink_trans = new Transition(curr_state, sink_state, letter.ToString());
                        sink_state.AddTransition(sink_trans);
                        curr_state.AddTransition(sink_trans);
                        this.states.Add(sink_state);
                        this.all_transitions.Add(sink_trans);
                        this.letters_states[letter].Add("NaN");
                    }
                }

                Debug.WriteLine($"Number of states under letters: {this.letters_states.Count()}");

                Debug.WriteLine($"\nChecking for new formed states...");

                // Check if any new states in table
                foreach (KeyValuePair<char, List<string>> l_state in this.letters_states)
                {
                    if (l_state.Value.Count > tableIndex && l_state.Value[tableIndex] != "NaN")
                    {
                        Debug.WriteLine($"{tableIndex}: Letter {l_state.Key} now points to state '{l_state.Value[tableIndex]}'");

                        // If there are no states with the same label
                        if (this.states.All(s => s.state_label != l_state.Value[tableIndex]))
                        {
                            // Create new state
                            State new_state = new State(l_state.Value[tableIndex], false, null); // Transitions added later
                            this.states.Add(new_state);

                            Debug.WriteLine($"New state found: {l_state.Value[tableIndex]}");

                            // Add new state to table (states2calculate)
                            this.states2calculate.Add(l_state.Value[tableIndex]);

                            // call this method again with new state
                            NDFA2DFA_CalculateState(new_state);
                                                        
                            tableIndex++; // Increase the index that refers to which table row we are currently focused on
                        }
                    }
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
            Debug.WriteLine("Epsilon Enclosure");
            bool hasEpsilonClosures = false;
            foreach (State st in this.states)
            {
                Debug.Write($"State {st.state_label} turned to: ");
                List<Transition> outEpsilonTransitions = st.FindTransitionsByValue(this.form.epsilon.ToString(), true, true); // true for outgoing only, true for recursive epsilon transitions

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
            foreach(State st in this.states)
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
                    // If current state's transition points to a final state or has path to final state, return true
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

                // Check if file is wrong
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

            if (word.Length == 0)
                word = "_";

            // Check transitions from current state (state on top of stack)
            List<Transition> possibleTransitions = this.stateStack.Peek().
                FindTransitionsByValue(word[letter_index].ToString(), true, true).ToList();

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
                        Debug.WriteLine($"\t\t {tr.label}: {tr.startsFrom.state_label} --> {tr.pointsTo.state_label}");
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