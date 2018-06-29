using MyVisualStudio.Model.Parser;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MyParsr
{
    public partial class Interpreter
    {
        #region Init

        public Interpreter()
        {
            variableNameRegex = new Regex(@"\w*\s*?=");
            variableValueRegex = new Regex(@"\=\s*?(.*);");
            callFunctionRegex = new Regex(@"[a-zA-Z]+\([^\)]*\)(\.[^\)]*\))?");
            updateVariableRegex = new Regex(@"\s?\w*\s*=\s*.*\s*;");
            exprecionRegex = new Regex(@"=.*[\+|\-|\*]+.*;");
            parametersRegex = new Regex(@"\((.*?)\)");
            ifRegex = new Regex(@"(if\s*\([^()]*\)\p{Zs}*)\s*{((?:\r?\n\p{Zs}+\p{L}.*)+?)+\s*?}");
            whileRegex = new Regex(@"(while\s*\([^()]*\)\p{Zs}*)\s*{((?:\r?\n?(\p{Zs}*)?\p{L}.*)+?)+\s*?}");
            elseRegex = new Regex(@"else\s*?{((?:\r?\n\p{Zs}+\p{L}.*)+?)+\s*?}");
            conditionCallRegex = new Regex("condition([0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12})$");
            whileCallRegex = new Regex("while([0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12})$");
            forCallRegex = new Regex("for([0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12})$");
            ifConditionRegex = new Regex(@"(?<=if).*\)");
            whileConditionRegex = new Regex(@"(?<=while).*\)");
            forConditionRegex = new Regex(@"(?<=for).*\)");
            functionReturnTypeRegex = new Regex(@"(?<=\bfunction\s)(\w+)");
            param2 = new Regex(@"function+\s*\w+\((.*?)\)");
            arrByIndexRegex = new Regex(@"\w*\[");
            updateItemArrRegex = new Regex(@"\w*\[\w*\]\s*=.*;");
            forRegex = new Regex(@"\s*(for\s*\([^()]*\)\p{Zs}*)\s*{((?:\s*\r?\n?(\p{Zs}*)?\p{L}.*)+?)+\s*?}");
            returnRegex = new Regex(@"\s*return\s*.*;");
            retutnValue = new Regex(@"(?<=\breturn\s).*;");
            ifElseRegex = new Regex(@"\s*if\s*\(.*\)\s*\{?");
            createInstanseStructRegex = new Regex(@"\s*\w*\s*=\s*new\s*\w*\(.*\);");
            structNameRegex = new Regex(@"(?<=\bstruct\s)(\w+)");
            createStructNameRegex = new Regex(@"(?<=\bnew\s)(\w+)");
            callStructFunctionRegex = new Regex(@"\s*\w*\.\w*\(.*\);");
            setStructFieldRegex = new Regex(@"\s*\w*\.\w*\s*=.*;");
            symbol = new string[] { "<=", ">=", "||", "&&", "!=", "==", ">", "<", "/", "*", "-", "+", "%" };
        }

        #endregion

        #region Fields

        private IEnumerable<Function> functionList;
        private IEnumerable<Struct> structList;
        private readonly Regex variableNameRegex;
        private readonly Regex variableValueRegex;
        private readonly Regex callFunctionRegex;
        private readonly Regex updateVariableRegex;
        private readonly Regex exprecionRegex;
        private readonly Regex parametersRegex;
        private readonly Regex ifRegex;
        private readonly Regex whileRegex;
        private readonly Regex elseRegex;
        private readonly Regex conditionCallRegex;
        private readonly Regex whileCallRegex;
        private readonly Regex forCallRegex;
        private readonly Regex ifConditionRegex;
        private readonly Regex whileConditionRegex;
        private readonly Regex functionReturnTypeRegex;
        private readonly Regex param2;
        private readonly Regex arrByIndexRegex;
        private readonly Regex updateItemArrRegex;
        private readonly Regex forRegex;
        private readonly Regex forConditionRegex;
        private readonly Regex returnRegex;
        private readonly Regex retutnValue;
        private readonly Regex ifElseRegex;
        private readonly Regex createInstanseStructRegex;
        private readonly Regex structNameRegex;
        private readonly Regex createStructNameRegex;
        private readonly Regex callStructFunctionRegex;
        private readonly Regex setStructFieldRegex;
        private string[] symbol;

        #endregion
    }
}
