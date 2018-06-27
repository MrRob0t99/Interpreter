using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyVisualStudio.Model.Parser.Variables
{
    class VariableArray : Variable
    {
        public List<Variable> Value { get; set; }
    }
}
