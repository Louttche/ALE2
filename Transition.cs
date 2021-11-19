using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ALE2
{
    public class Transition
    {
        public State startsFrom;
        public State pointsTo;
        public string label;

        public Transition(State startsfrom, State pointsto, string label)
        {
            this.startsFrom = startsfrom;
            this.pointsTo = pointsto;
            this.label = label;
        }        
    }
}
