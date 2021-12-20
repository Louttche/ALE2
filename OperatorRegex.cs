using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ALE2
{
    public class OperatorRegex : INodeRegex
    {
        public int ID { get; set; }
        public char Value { get; set; }
        public int Parent_ID { get; set; }
        public INodeRegex Left_child { get; set; }
        public INodeRegex Right_child { get; set; }


        public OperatorRegex(int id, char value, int parent_id = -1)
        {
            this.ID = id;
            this.Value = value;
            this.Parent_ID = parent_id;
        }

        public void AddChild(INodeRegex child)
        {
            if (child != null)
            {
                if (this.Left_child == null)
                    this.Left_child = child;
                else if (this.Right_child == null)
                    this.Right_child = child;
            }
        }

        
        // TODO: Implement
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            // TODO: write your implementation of Equals() here
            throw new NotImplementedException();
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            // TODO: write your implementation of GetHashCode() here
            throw new NotImplementedException();
            return base.GetHashCode();
        }
    }
}
