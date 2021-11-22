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
        public string state_value { get; set; }
        public bool isFinal { get; set; }
        public List<Transition> transitions { get; set; }

        public State(string value, bool isFinal, List<Transition> transitions)
        {
            this.state_value = value;
            this.isFinal = isFinal;

            if (transitions != null)
                this.transitions = transitions;
            else
                this.transitions = new List<Transition>();
        }

        public void AddTransition(Transition transition)
        {
            try
            {
                this.transitions.Add(transition);
            }
            catch (NullReferenceException ne)
            {
                Debug.WriteLine("Adding Transition gave a NullReferenceException - " + ne);
            }
            finally
            {
                //Debug.WriteLine($"{this.state_value}: Transition added.\t{transition.label}: {transition.startsFrom.state_value} --> {transition.pointsTo.state_value}");
            }
        }

        public override bool Equals(object obj)
        {
            return obj is State state &&
                   state_value == state.state_value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(state_value);
        }
    }
}
