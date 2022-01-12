using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ALE2
{
    public class State
    {
        public string state_label { get; set; }
        public bool isFinal { get; set; }
        public List<Transition> transitions { get; set; }

        public State(string label, bool isFinal, List<Transition> transitions)
        {
            this.state_label = label;
            this.isFinal = isFinal;

            if (transitions != null)
                this.transitions = transitions;
            else
                this.transitions = new List<Transition>();
        }

        public void AddTransition(Transition transition)
        {
            try {
                this.transitions.Add(transition);
            }
            catch (NullReferenceException ne) {
                Debug.WriteLine("Adding Transition gave a NullReferenceException - " + ne);
            }
        }

        public List<Transition> FindTransitionsByValue(string label_value, bool onlyOut = false, bool empty_closure = false)
        {
            List<Transition> result = transitions.Where(t => t.label == label_value).ToList();

            // If checked outside of method, transitions with different from-state than initial will fail for word checking
            if (onlyOut)
                result = result.Where(t => t.startsFrom.Equals(this)).ToList();

            // If including empty closures,
            if (empty_closure) {
                // return transitions belonging to states that the empty transition is pointing to
                foreach (Transition t in transitions.Where(t => t.isEmpty && t.startsFrom.Equals(this)))
                {
                    Debug.WriteLine($"{this.state_label} has empty transition, checking follow-up transitions...");
                    result.AddRange(t.pointsTo.FindTransitionsByValue(label_value, true, true));
                }
            }

            Debug.WriteLine($"Found {result.Count()} transitions with value {label_value}");
            return result;
        }

        public override bool Equals(object obj)
        {
            return obj is State state &&
                   state_label == state.state_label;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(state_label);
        }
    }
}
