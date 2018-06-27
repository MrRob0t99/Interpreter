using MyVisualStudio.Model.Parser.Variables;
using System.Collections.Generic;

namespace MyVisualStudio.Model.Parser
{
    class Struct
    {
        public string Name { get; set; }

        public string Code { get; set; }

        public List<Function> Functions { get; set; }

        public List<Variable> Fields { get; set; }
    }
}
