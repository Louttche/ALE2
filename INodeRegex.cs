using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ALE2
{
    public interface INodeRegex
    {
        int ID { get; set; }
        char Value { get; set; }
        int Parent_ID { get; set; }
    }
}
