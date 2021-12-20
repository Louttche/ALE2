using System;
using System.Collections.Generic;
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
        public Boolean isEmpty { get; set; }

        public Transition(State startsfrom, State pointsto, string label)
        {
            if (startsfrom == null || pointsto == null)
                return;

            this.startsFrom = startsfrom;
            this.pointsTo = pointsto;

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
            return String.Format($"Transition {this.label}: {this.startsFrom.state_value} --> {this.pointsTo.state_value}");
        }
    }
}
