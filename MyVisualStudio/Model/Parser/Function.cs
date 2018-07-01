using System;

namespace MyVisualStudio.Model.Parser
{
    public class Function : Base
    {
        public string Name { get; set; }

        public bool IsReturnValue { get; set; }

        public string Parameters { get; set; }

        public Type ReturnType { get; set; }

        public bool IsConstructor { get; set; }
    }
}
