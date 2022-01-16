using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ALE2
{
    public class Transition
    {
        public State startsFrom { get; set; }
        public State pointsTo { get; set; }
        public string label { get; set; }
        public bool isEmpty { get; set; }
        public string popValue { get; set; }
        public string pushValue { get; set; }
        //public Dictionary<string, string> stack { get; set; } // what is being popped | what is being pushed

        public Transition(State startsfrom, State pointsto, string label)
        {
            if (startsfrom == null || pointsto == null)
                return;

            this.startsFrom = startsfrom;
            this.pointsTo = pointsto;

            // Add transition to the states too
            this.startsFrom.AddTransition(this);
            this.pointsTo.AddTransition(this);

            if (label == "_" || label == "" || label == " ")
            {
                isEmpty = true;
                this.label = Char.ConvertFromUtf32(949); // epsilon;
            }
            else
            {
                isEmpty = false;
                this.label = label;
            }

            this.popValue = null;
            this.pushValue = null;

            Debug.WriteLine($"Transition Created - {this.ToString()}");
        }

        public void AddStack(string pop_value, string push_value)
        {
            if (pop_value == "_" || pop_value == "")
                pop_value = Char.ConvertFromUtf32(949);
            if (push_value == "_" || push_value == "")
                push_value = Char.ConvertFromUtf32(949);

            //this.stack.Add(pop_value, push_value);
            this.popValue = pop_value;
            this.pushValue = push_value;
            Debug.WriteLine($"Added pop and push val (resp): {pop_value}, {push_value} to Transition:\n{this.ToString()}\n");
        }

        public string GetFullLabel()
        {
            //if (this.stack.Count > 0)

            // Only show stack if only 1 or neither values are epsilon/empty
            if (this.popValue != null && this.pushValue != null && !(this.popValue.Equals(Char.ConvertFromUtf32(949)) && this.pushValue.Equals(Char.ConvertFromUtf32(949))))
                return this.label + $", {this.popValue} {Char.ConvertFromUtf32(8594)} {this.pushValue}";

            //Debug.WriteLine("Could not get stack label because transition does not have one.");
            return this.label;
        }

        public bool isSelfLoop()
        {
            if (this.startsFrom.Equals(this.pointsTo))
                return true;

            return false;
        }

        public override bool Equals(Object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
                return false;
            else
            {
                Transition t = (Transition)obj;
                return (this.startsFrom == t.startsFrom) && (this.pointsTo == t.pointsTo) && (this.label == t.label);
            }
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(startsFrom, pointsTo, label);
        }

        public override string ToString()
        {
            return String.Format($"Transition {this.GetFullLabel()}: {this.startsFrom.state_label} --> {this.pointsTo.state_label}");
        }
    }
}
