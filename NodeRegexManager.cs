using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ALE2
{
    public class NodeRegexManager
    {
        public string formula = "";
        public List<INodeRegex> nodes = new List<INodeRegex>();
        public List<OperatorRegex> operators = new List<OperatorRegex>();
        public List<OperantRegex> operants = new List<OperantRegex>();

        private int formula_index = 0;
        private Stack<OperatorRegex> parent_stack = new Stack<OperatorRegex>();

        public Dictionary<char, string> infix_notations = new Dictionary<char, string> { // ascii : notation
            { '*', Char.ConvertFromUtf32(8902) },
            { '|', Char.ConvertFromUtf32(8744) },
            { '.', Char.ConvertFromUtf32(8743) }
        };

        public NodeRegexManager()
        {
            this.operants.Clear();
            this.operators.Clear();
            this.nodes.Clear();
            this.parent_stack.Clear();
            formula_index = 0;
        }

        public void AddNode(string formula)
        {
            INodeRegex n;
            int id_index = this.nodes.Count + 1;

            for (int i = formula_index; i < formula.Length; i = formula_index)
            {
                id_index = this.nodes.Count + 1;
                char c = formula[i];

                switch (c)
                {
                    case '(': // Add children
                        formula_index++;
                        AddNode(formula);
                        break;
                    case ')':
                        // Last parent has no more children to add so remove from stack
                        if (this.parent_stack.Count > 0)
                            this.parent_stack.Pop();
                        return; // to exit from child method (of recursion)
                    case ',':
                        break;
                    default: // Operants/Operators

                        // Add the nodes to their respective list
                        if (this.infix_notations.Keys.Contains(c))
                        {
                            n = new OperatorRegex(id_index, c, this.parent_stack.Count > 0 ? this.parent_stack.Peek().ID : -1);
                            this.operators.Add((OperatorRegex)n);
                        }
                        else
                        {
                            n = new OperantRegex(id_index, c, this.parent_stack.Count > 0 ? this.parent_stack.Peek().ID : -1);

                            // Don't add if same operant already exists.
                            if (this.operants.Any(x => x.Value == n.Value) == false)
                                this.operants.Add((OperantRegex)n);
                        }

                        // Add this node as child to parent in stack
                        if (this.parent_stack.Count > 0)
                        {
                            foreach (OperatorRegex o in this.nodes.Where(x => x.GetType() == typeof(OperatorRegex)))
                            {
                                if (o.ID == this.parent_stack.Peek().ID)
                                    o.AddChild(n);
                            }
                        }

                        // Add this in parent stack if operator
                        if (n.GetType() == typeof(OperatorRegex))
                            this.parent_stack.Push((OperatorRegex)n);

                        // Add it to the general list of nodes
                        if (n.GetType() != typeof(INodeRegex))
                            this.nodes.Add(n);

                        break;
                }

                formula_index++;
            }
        }

        public string debugString()
        {
            string result = "";
            foreach (OperatorRegex node in operators)
            {
                result += "Node " + node.Value;

                if (node.Left_child != null)
                    result += "\n\tLeft - " + node.Left_child.Value;
                if (node.Right_child != null)
                    result += "\n\tRight - " + node.Right_child.Value;
            }

            return result;
        }
    }
}
