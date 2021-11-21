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

        public Transition(State startsfrom, State pointsto, string label)
        {
            this.startsFrom = startsfrom;
            this.pointsTo = pointsto;
            this.label = label;
        }
    }
}
