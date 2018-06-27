using MyParsr;

namespace MyVisualStudio.Model.Parser
{
    public class Condition : Base
    {
        public bool IsHaveElse { get; set; }

        public Else Else { get; set; }
    }
}
