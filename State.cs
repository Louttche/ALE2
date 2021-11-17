using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ALE2
{
    public class State
    {
        public string state_value { get; set; }
        public bool isFinal { get; set; }
        public Dictionary<State, string> transitions { get; set; }

        public State(string value, bool isFinal, Dictionary<State,string> transitions)
        {
            this.state_value = value;
            this.isFinal = isFinal;
            this.transitions = transitions;
        }

    }
}
