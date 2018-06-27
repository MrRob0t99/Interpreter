using MyParsr;

namespace MyVisualStudio.Model.Parser
{
    public class Function : Base
    {
        public string Name { get; set; }

        public bool IsReturnValue { get; set; }

        public bool Value { get; set; }

        public string Parameters { get; set; }
    }
}
