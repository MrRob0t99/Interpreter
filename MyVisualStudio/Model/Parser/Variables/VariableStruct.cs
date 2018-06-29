using System.Collections.Generic;

namespace MyVisualStudio.Model.Parser.Variables
{
    class VariableStruct : Variable
    {
        public List<Variable> Value { get; set; }

        public string StructName { get; set; }
    }
}
