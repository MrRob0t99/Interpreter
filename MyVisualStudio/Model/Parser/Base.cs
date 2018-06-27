using MyParsr;
using System;
using System.Collections.Generic;

namespace MyVisualStudio.Model.Parser
{
    public class Base
    {
        public Guid Id { get; set; }

        public string Code { get; set; }

        public string Conditon { get; set; }

        public List<Condition> Codnditions { get; set; }

        public List<While> While { get; set; }

        public List<For> Fors { get; set; }

        public List<Base> Bases { get; set; }
    }
}
