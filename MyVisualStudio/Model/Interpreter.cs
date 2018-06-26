using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MyParsr
{
    public class Function : Base
    {
        public string Name { get; set; }

        public bool IsReturnValue { get; set; }

        public bool Value { get; set; }

        public string Parameters { get; set; }
    }

    public class Condition : Base
    {

        public bool IsHaveElse { get; set; }

        public Else Else { get; set; }
    }

    public class Else : Base
    {

    }

    public class While : Base
    {

    }

    public class For : Base
    {

    }

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

    #region Variables

    class Variable
    {
        public string Name { get; set; }
    }

    class VariableInt : Variable
    {
        public int Value { get; set; }
    }

    class VariableString : Variable
    {
        public string Value { get; set; }
    }

    class VariableDouble : Variable
    {
        public double Value { get; set; }
    }

    class VariableBoolean : Variable
    {
        public bool Value { get; set; }
    }

    class VariableArray : Variable
    {
        public List<Variable> Value { get; set; }
    }

    #endregion

    class Expresion
    {
        public string Expresions { get; set; }

        public dynamic Result { get; set; }
    }

    public class Interpreter
    {
        #region Init

        public Interpreter()
        {
            variableNameRegex = new Regex(@"\w*\s*?=");
            variableValueRegex = new Regex(@"\=\s*?(.*);");
            // callFunctionRegex = new Regex(@"\s*\w*\(.*\);");
            callFunctionRegex = new Regex(@"[a-zA-Z]+\([^\)]*\)(\.[^\)]*\))?");
            //  variableCreteRegex = new Regex(@"var+.*;");
            updateVariableRegex = new Regex(@"\s?\w*\s*=\s*.*\s*;");
            exprecionRegex = new Regex(@"=.*[\+|\-|\*]+.*;");
            parametersRegex = new Regex(@"\((.*?)\)");
            //ifElseRegex = new Regex(@"(if\s*\([^()]*\)\p{Zs}*)\s*\{[.*\W*\S*]*}.{1}");
            ifRegex = new Regex(@"(if\s*\([^()]*\)\p{Zs}*)\s*{((?:\r?\n\p{Zs}+\p{L}.*)+?)+\s*?}");
            whileRegex = new Regex(@"(while\s*\([^()]*\)\p{Zs}*)\s*{((?:\r?\n?(\p{Zs}*)?\p{L}.*)+?)+\s*?}");
            elseRegex = new Regex(@"else\s*?{((?:\r?\n\p{Zs}+\p{L}.*)+?)+\s*?}");
            conditionCallRegex = new Regex("condition([0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12})$");
            whileCallRegex = new Regex("while([0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12})$");
            forCallRegex = new Regex("for([0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12})$");
            ifConditionRegex = new Regex(@"(?<=if).*\)");
            whileConditionRegex = new Regex(@"(?<=while).*\)");
            forConditionRegex = new Regex(@"(?<=for).*\)");
            functionNameRegex = new Regex(@"(?<=\bfunction\s)(\w+)");
            returnFunctionRegex = new Regex(@"\W*(return)\W*");
            param2 = new Regex(@"function+\s*\w+\((.*?)\)");
            arrByIndexRegex = new Regex(@"\w*\[");
            updateItemArrRegex = new Regex(@"\w*\[\w*\]\s*=.*;");
            forRegex = new Regex(@"\s*(for\s*\([^()]*\)\p{Zs}*)\s*{((?:\s*\r?\n?(\p{Zs}*)?\p{L}.*)+?)+\s*?}");
            returnRegex = new Regex(@"\s*return\s*.*;");
            retutnValue = new Regex(@"(?<=\breturn\s).*;");
            ifElseRegex = new Regex(@"\s*if\s*\(.*\)\s*\{?");
            //subCall = new Regex(@"\w*\(\w*\)");
        }

        #endregion

        #region Fields

        private IEnumerable<Function> functionList;
        private readonly Regex variableNameRegex;
        private readonly Regex variableValueRegex;
        private readonly Regex callFunctionRegex;
        //  private readonly Regex variableCreteRegex;
        private readonly Regex updateVariableRegex;
        private readonly Regex exprecionRegex;
        private readonly Regex parametersRegex;
        //private readonly Regex ifElseRegex;
        private readonly Regex ifRegex;
        private readonly Regex whileRegex;
        private readonly Regex elseRegex;
        private readonly Regex conditionCallRegex;
        private readonly Regex whileCallRegex;
        private readonly Regex forCallRegex;
        private readonly Regex ifConditionRegex;
        private readonly Regex whileConditionRegex;
        //private readonly Regex array;
        private readonly Regex functionNameRegex;
        private readonly Regex returnFunctionRegex;
        private readonly Regex param2;
        private readonly Regex arrByIndexRegex;
        private readonly Regex updateItemArrRegex;
        private readonly Regex forRegex;
        private readonly Regex forConditionRegex;
        private readonly Regex returnRegex;
        private readonly Regex retutnValue;
        private readonly Regex ifElseRegex;
        //private readonly Regex subCall;
        private string[] symbol = new string[] { "<=", ">=", "||", "&&", "!=", "==", ">", "<", "/", "*", "-", "+", "%" };

        #endregion

        public void Run(string code)
        {
            code = code.Replace("  ", " ");
            functionList = GetFunctionList(code);
            var mainFunction = functionList.FirstOrDefault(f => f.Name == "Main");
            CodeRun(mainFunction.Code, null, mainFunction);
            Console.WriteLine("END");
            Console.ReadKey();
        }

        private dynamic CodeRun(string code, List<Variable> variables, Base function = null)
        {
            if (variables == null)
                variables = new List<Variable>();
            if (function == null)
                function = new Base();
            dynamic retu = null;
            var lines = Regex.Split(code, "\r\n").ToList();

            for (int i = 0; i < lines.Count; i++)
            {
                var item = lines[i];
                if (string.IsNullOrWhiteSpace(item))
                    continue;

                var callMatch = callFunctionRegex.Matches(item);
                if (callMatch.Count > 0 && !item.Contains("function"))
                {
                    var call = callMatch[0].Value;
                    var contentBracketsArray = call.Skip(call.IndexOf("(")).Take(call.LastIndexOf(")") - call.IndexOf("(") + 1);
                    var contentBrackets = string.Concat(contentBracketsArray);
                    while (callFunctionRegex.Matches(contentBrackets).Count > 0)
                    {
                        var subCheck = callFunctionRegex.Matches(contentBrackets)[0].Value;
                        var restuir = RunFunk(subCheck, variables);
                        var fN = GetFunctionName(subCheck);
                        var f = GetFunctioByName(fN);
                        if (f != null && f.IsReturnValue)
                        {
                            var repleseItem = item.Replace(subCheck, restuir.ToString());
                            call = call.Replace(subCheck, restuir.ToString() + ")");
                            contentBrackets = contentBrackets.Replace(contentBrackets, restuir.ToString() + ")");
                        }
                    }
                    var restur = RunFunk(call, variables);
                    var functionName = GetFunctionName(call);
                    var funk = GetFunctioByName(functionName);
                    if (funk != null && funk.IsReturnValue)
                    {
                        var repleseItem = item.Replace(call, restur.ToString());
                        lines[i] = repleseItem;
                        --i;
                    }
                    continue;
                }

                var arrElemUpdate = updateItemArrRegex.Matches(item);
                if (arrElemUpdate.Count > 0)
                {
                    var match = arrElemUpdate[0].Value;
                    var variable = CreateVariables(match, variables);
                    var contentBracket = Regex.Match(match, @"\[([^)])\]").Groups[0].Value;
                    contentBracket = contentBracket.Substring(1, contentBracket.Length - 2);
                    int res = 0;
                    int.TryParse(contentBracket, out res);
                    if (variables != null && variables.Count > 0)
                    {
                        var fromVariabes = DoOperation(variables.FirstOrDefault(v => v.Name == contentBracket));
                        int.TryParse(fromVariabes.ToString(), out res);
                    }

                    var name = arrByIndexRegex.Matches(match);
                    var names = name[0].Value;
                    names = names.Substring(0, names.Length - 1);
                    var variabl = variables.FirstOrDefault(v => v.Name == names) as VariableArray;
                    var value = variabl.Value;
                    value.RemoveAt(res);
                    value.Insert(res, variable);
                }

                var forMatch = forCallRegex.Matches(item);
                if (forMatch.Count > 0)
                {
                    string id = string.Empty;
                    foreach (Match conditionId in forMatch)
                    {
                        id = conditionId.Value.Substring(3);
                    }
                    var fors = function.Bases.FirstOrDefault(c => c.Id.ToString() == id);
                    if (fors == null)
                        continue;
                    var arr = fors.Conditon.Substring(1, fors.Conditon.Length - 2).Split(';').ToList();
                    var variable = CreateVariables(arr[0] + ";", variables);
                    variables.Add(variable);
                    while (DoExpresion(arr[1], variables))
                    {
                        CodeRun(fors.Code, variables, function);
                        var valueVariable = DoExpresion(arr[2], variables);
                        var newVariable = GetVariableValue(valueVariable.ToString());
                        newVariable.Name = variable.Name;
                        int index = variables.IndexOf(variable);
                        if (index != -1)
                            variables[index] = newVariable;
                        variable = newVariable;
                    }
                    continue;
                }

                var updateVariableMatch = updateVariableRegex.Matches(item);
                if (updateVariableMatch.Count > 0)
                {
                    var match = updateVariableMatch[0].Value;
                    var variable = CreateVariables(match, variables);
                    var oldVariable = variables.FirstOrDefault(v => v.Name == variable.Name);
                    if (oldVariable != null)
                        variables.Remove(oldVariable);
                    variables.Add(variable);
                    continue;
                }

                var conditionMatch = conditionCallRegex.Matches(item);
                if (conditionMatch.Count > 0)
                {
                    string id = string.Empty;
                    foreach (Match conditionId in conditionMatch)
                    {
                        id = conditionId.Value.Substring(9);
                    }
                    var baseCondition = function.Bases.FirstOrDefault(c => c.Id.ToString() == id);

                    if (baseCondition == null)
                        continue;
                    else
                    {
                        var condition = baseCondition as Condition;
                        dynamic res = null;
                        var isTrue = IsIfConditionTrue(condition.Conditon, variables, ifConditionRegex);
                        if (isTrue)
                        {
                            res = CodeRun(condition.Code, variables, function);
                            if (res != null)
                            {
                                retu = res;
                                break;
                            }
                        }
                        else if (condition.Else != null && condition.IsHaveElse == true)
                        {
                            res = CodeRun(condition.Else.Code, variables, function);

                        }
                        if (res != null)
                        {
                            retu = res;
                            break;
                        }
                        continue;
                    }
                }

                var whileMatch = whileCallRegex.Matches(item);
                if (whileMatch.Count > 0)
                {
                    string id = string.Empty;
                    foreach (Match conditionId in whileMatch)
                    {
                        id = conditionId.Value.Substring(5);
                    }
                    var whiles = function.Bases.FirstOrDefault(c => c.Id.ToString() == id);
                    if (whiles == null)
                        continue;
                    var whileCode = whiles.Code;
                    if (whileCode.Contains("{") && whileCode.Contains("}"))
                    {
                        var expresionArr = whileCode.Skip(whileCode.IndexOf("{") + 1).Take(whileCode.LastIndexOf("}") - whileCode.IndexOf("({") - 1);
                        whileCode = string.Concat(expresionArr);
                    }
                    while (IsIfConditionTrue(whiles.Conditon, variables, whileConditionRegex))
                    {
                        CodeRun(whileCode, variables, function);
                    }
                    continue;
                }

                var returnMatch = returnRegex.Matches(item);
                if (returnMatch.Count > 0)
                {
                    var returnValue = returnMatch[0].Value;
                    var value = retutnValue.Matches(returnValue)[0].Value;
                    value = value.Substring(0, value.Length - 1);
                    var res = DoExpresion(value, variables);
                    if (res != null)
                    {
                        retu = res;
                        break;
                    }
                }
            }
            return retu;
        }

        private dynamic RunFunk(string call, List<Variable> variables)
        {
            var functionName = GetFunctionName(call);
            var funk = GetFunctioByName(functionName);
            var functionParams = GetParametersForCallFunction(call, variables, funk);
            if (functionName == "Print")
            {
                Print(functionParams.FirstOrDefault());
            }

            else if (IsExistFunction(functionName))
            {
                var res = CodeRun(funk.Code, functionParams, funk);
                if (funk.IsReturnValue)
                    return res;
            }

            return null;
        }

        private void Print(Variable variable)
        {
            Console.WriteLine(DoOperation(variable).ToString());
        }

        private List<Variable> GetParametersForCallFunction(string callFunction, List<Variable> variables, Function function)
        {
            List<string> functionNameParamArr = null;
            var paramsVariable = new List<Variable>();
            var parametersMatchCollection = parametersRegex.Matches(callFunction);
            var functionParam = string.Empty;
            foreach (Match item in parametersMatchCollection)
            {
                functionParam = item.Value;
            }

            var dict = new Dictionary<string, string>();
            var newV = DeepCopyList(variables);

            functionParam = functionParam.Substring(1, functionParam.Length - 2);
            var functionParamArr = functionParam.Split(',').ToList();

            if (function != null)
            {
                functionNameParamArr = function.Parameters.Split(',').ToList();
                for (int i = 0; i < functionNameParamArr.Count; i++)
                {
                    dict.Add(functionParamArr[i], functionNameParamArr[i]);
                }
            }

            foreach (var item in functionParamArr)
            {
                var variable = GetVariableWithValue(item, newV);
                if (function != null)
                    variable.Name = dict[item];
                paramsVariable.Add(variable);
            }
            return paramsVariable;
        }

        private List<Variable> DeepCopyList(List<Variable> variables)
        {
            var newV = new List<Variable>();
            foreach (var item in variables)
            {
                var value = DoOperation(item);
                Variable variable = null;
                if (!(item is VariableArray))
                {
                    variable = GetVariableValue(value.ToString());
                }
                else
                {
                    var array = item as VariableArray;
                    var list = DeepCopyList(array.Value);
                    array.Value = list;
                    variable = array;
                }

                if (item.Name != null)
                    variable.Name = item.Name;

                newV.Add(variable);
            }
            return newV;
        }

        private Variable GetVariableWithValue(string variabeValue, List<Variable> variables)
        {
            variabeValue = variabeValue.Trim();
            var variable = new Variable();
            if (variables != null && variables.Any(v => v.Name == variabeValue))
            {
                variable = variables.FirstOrDefault(v => v.Name == variabeValue);
                return variable;
            }
            if (IsOperation(variabeValue))
            {
                var operatingResult = DoExpresion(variabeValue, variables);
                variable = GetVariableValue(operatingResult.ToString(), variables);
            }
            else
            {
                variable = (GetVariableValue(variabeValue, variables));
            }
            return variable;
        }

        private bool IsOperation(string code)
        {
            for (int i = 0; i < code.Length; i++)
            {
                var sym = Char.ToString(code[i]);
                if ((symbol.Any(s => s == sym)) || (code.Length > i + 1 && symbol.Any(ex => ex == (sym + Char.ToString(code[i + 1])))))
                {
                    return true;
                }
            }
            return false;
        }

        private string GetFunctionName(string functionCall)
        {
            var functionNameRegex = new Regex(@"\w+\(");
            var functionNameMatch = functionNameRegex.Matches(functionCall);
            var functionName = string.Empty;

            foreach (Match name in functionNameMatch)
            {
                functionName = name.Value;
            }

            functionName = functionName.Substring(0, functionName.Length - 1);

            return functionName;
        }

        private bool IsExistFunction(string functionName)
        {
            var functions = functionList.Select(f => f.Name).Aggregate((left, right) => left + " " + right).ToString();
            if (functions.Contains(functionName))
                return true;
            return false;
        }

        private Function GetFunctioByName(string functionName)
        {
            var function = functionList.FirstOrDefault(f => f.Name == functionName);
            return function;
        }

        private Variable CreateVariables(string code, List<Variable> variables)
        {
            var newV = DeepCopyList(variables);
            var variable = new Variable();
            var variableValue = string.Empty;
            var variableValueMatch = variableValueRegex.Matches(code);
            var variableNameMatch = variableNameRegex.Matches(code);
            var variableName = variableNameMatch[0].Value;

            variableName = variableName.Substring(0, variableName.Length - 1).Trim();
            variableValue = variableValueMatch[0].Value;
            variableValue = variableValue.Substring(1, variableValue.Length - 2);
            variable = GetVariableWithValue(variableValue, newV);

            if (variableName != null)
                variable.Name = variableName;

            return variable;
        }

        private Variable GetVariableValue(string value, List<Variable> variables = null)
        {
            bool boolean;
            double doubl;
            int int32;
            Variable variable = null;

            value = value.Trim().ToString();

            if (bool.TryParse(value, out boolean))
            {
                variable = new VariableBoolean() { Value = boolean };
                return variable;
            }
            var strWithoutDash = string.Concat(value.Where(c => c != '_'));
            if (double.TryParse(strWithoutDash, out doubl))
            {
                variable = new VariableDouble() { Value = doubl };
                return variable;
            }
            if (int.TryParse(strWithoutDash, out int32))
            {
                variable = new VariableInt() { Value = int32 };
                return variable;
            }
            if (value.Contains("[") && value.Contains("]") && !IsOperation(value))
            {
                var contentBracket = Regex.Match(value, @"\[([^)]*)\]").Groups[0].Value;
                contentBracket = contentBracket.Substring(1, contentBracket.Length - 2);
                int res = 0;
                var fromVariabes = DoOperation(variables.FirstOrDefault(v => v.Name == contentBracket));
                if (fromVariabes != null)
                    fromVariabes = fromVariabes.ToString();
                if (int.TryParse(contentBracket, out res) || (variables != null && variables.Count > 0 && int.TryParse(fromVariabes, out res)))
                {
                    var name = arrByIndexRegex.Matches(value);
                    var names = name[0].Value;
                    names = names.Substring(0, names.Length - 1);
                    if (variables.Any(v => v.Name == names))
                    {
                        var variabl = variables.FirstOrDefault(v => v.Name == names);
                        var arr = variabl as VariableArray;
                        variable = arr.Value.ElementAtOrDefault(res);
                        return variable;
                    }
                }
            }

            if (value.Contains("[") && value.Contains("]") && !value.StartsWith("\""))
            {
                value = value.Substring(1, value.Length - 2);
                var variableList = new List<Variable>();
                var variableStrArr = value.Split(',').ToList();
                foreach (var item in variableStrArr)
                {
                    var variableRes = GetVariableWithValue(item, variables);
                    variableList.Add(variableRes);
                }
                variable = new VariableArray() { Value = variableList };
                return variable;
            }
            else
            {
                variable = new VariableString() { Value = value };
                return variable;
            }

        }

        private int GetIndex(string code, int startIndex = 0)
        {
            int open = 0;
            for (int i = startIndex; i < code.Length; i++)
            {
                if (code[i] == '{')
                    ++open;
                if (code[i] == '}')
                {
                    --open;
                    if (open == 0)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        private List<Function> GetFunctionList(string code)
        {
            var functionsList = new List<Function>();

            while (!string.IsNullOrWhiteSpace(code))
            {
                var function = new Function();
                var param = string.Empty;

                var index = GetIndex(code, code.IndexOf("function"));
                var functionCode = code.Substring(code.IndexOf("function"), code.Length - (Math.Abs(index - code.Length)) + 1 - code.IndexOf("function"));
                code = code.Substring(0, code.IndexOf("function")) + code.Substring(index + 1, Math.Abs(index - code.Length) - 1);
                function.Code = functionCode;

                var matchName = functionNameRegex.Matches(functionCode);
                var matchParam = parametersRegex.Matches(functionCode);

                if(matchName.Count>0)
                function.Name = matchName[0].Value;

                if (matchParam.Count > 0)
                    param = matchParam[0].Value;

                if (!string.IsNullOrWhiteSpace(param))
                    function.Parameters = param.Substring(param.IndexOf("(") + 1, Math.Abs(param.IndexOf("(") - param.Length) - 2);

                var matchReturn = returnRegex.Matches(functionCode);
                function.IsReturnValue = matchReturn.Count > 0 ? true : false;
                function = GetAttachments(function) as Function;
                functionsList.Add(function);
            }

            return functionsList;
        }

        private dynamic GetValueVariable(Variable variable)
        {
            dynamic value = null;

            if (variable is VariableInt)
                value = ((VariableInt)variable).Value;
            else if (variable is VariableBoolean)
                value = ((VariableBoolean)variable).Value;
            else if (variable is VariableDouble)
                value = ((VariableDouble)variable).Value;
            else if (variable is VariableString)
                value = ((VariableString)variable).Value;
            else if (variable is VariableArray)
                value = ((VariableArray)variable).Value;

            return value;
        }

        private dynamic DoOperation(Variable variable1, Variable variable2 = null, string operating = null)
        {
            var valueVariable1 = GetValueVariable(variable1);
            if (variable2 == null)
                return valueVariable1;

            var valueVariable2 = GetValueVariable(variable2);
            dynamic result = null;
            switch (operating)
            {
                case "+":
                    result = valueVariable1 + valueVariable2;
                    break;
                case "-":
                    result = valueVariable1 - valueVariable2;
                    break;
                case "*":
                    result = valueVariable1 * valueVariable2;
                    break;
                case "/":
                    result = valueVariable1 / valueVariable2;
                    break;
                case "<":
                    result = valueVariable1 < valueVariable2;
                    break;
                case ">":
                    result = valueVariable1 > valueVariable2;
                    break;
                case "==":
                    result = valueVariable1 == valueVariable2;
                    break;
                case "!=":
                    result = valueVariable1 != valueVariable2;
                    break;
                case "&&":
                    result = valueVariable1 && valueVariable2;
                    break;
                case "||":
                    result = valueVariable1 || valueVariable2;
                    break;
                case ">=":
                    result = valueVariable1 >= valueVariable2;
                    break;
                case "<=":
                    result = valueVariable1 <= valueVariable2;
                    break;
                case "%":
                    result = valueVariable1 % valueVariable2;
                    break;
            }
            return result;
        }

        private dynamic DoExpresion(string code, List<Variable> variables)
        {
            if (code.Contains("(") && code.Contains(")"))
            {
                var contentBracketsArray = code.Skip(code.IndexOf("(")).Take(code.LastIndexOf(")") - code.IndexOf("(") + 1);
                var contentBrackets = string.Concat(contentBracketsArray);
                var resultBrackets = DoExpresion(contentBrackets.Substring(1, contentBrackets.Length - 2), variables);
                code = code.Replace(contentBrackets, resultBrackets.ToString());
            }

            var operands = code.Split(symbol, StringSplitOptions.RemoveEmptyEntries).ToList();
            var actionList = new List<string>();

            for (int i = 0; i < code.Length; i++)
            {
                if (code.Length > i + 1 && symbol.Any(ex => ex == (Char.ToString(code[i]) + Char.ToString(code[i + 1]))))
                {
                    actionList.Add(Char.ToString(code[i]) + Char.ToString(code[i + 1]));
                    break;
                }
                if (symbol.Any(s => s == Char.ToString(code[i])))
                    actionList.Add(Char.ToString(code[i]));
            }

            var priorityAction = new List<string> { "*", "/" };
            for (int i = 0; i < operands.Count(); i++)
            {
                operands[i] = operands[i].Trim();

                if (variables.Any(v => v.Name == operands[i]))
                    operands[i] = DoOperation(variables.FirstOrDefault(v => v.Name == operands[i])).ToString();
            }

            if (operands.Count == 1)
            {
                return DoOperation(GetVariableValue(operands[0]));
            }

            var count = 0;
            dynamic res = null;

            for (int i = 0; i < actionList.Count; i++)
            {
                if (priorityAction.IndexOf(actionList[i]) != -1)
                {
                    var v1 = GetVariableValue(operands[i]);
                    var v2 = GetVariableValue(operands[i + 1]);
                    res = DoOperation(v1, v2, actionList[i].ToString());
                    operands.Remove(operands[i + 1]);
                    operands[i] = res.ToString();
                    actionList.RemoveAt(i);
                }
            }

            while (operands.Count() > 1)
            {
                if (1 <= operands.Count())
                {
                    var v1 = GetVariableValue(operands[0], variables);
                    var v2 = GetVariableValue(operands[1], variables);

                    res = DoOperation(v1, v2, actionList[count].ToString());
                    operands.RemoveAt(1);
                    operands[0] = res.ToString();
                    ++count;
                }
            }
            return res;
        }

        private (string, List<Condition>) GetIfElse(string code)
        {
            var list = new List<Condition>();
            while (ifElseRegex.Matches(code).Count > 0)
            {
                var coincidence = ifElseRegex.Matches(code)[0].Value;
                var result = GetIfElseCode(coincidence, code);
                code = result.Item1;
                list.Add(result.Item2);
            }
            return (code, list);
        }

        private (string, Condition) GetIfElseCode(string coincidence, string code)
        {
            string codeIfElse = string.Empty;
            string sub = string.Empty;
            int indexEndIf = -1;
            string ifCode = string.Empty;
            var condition = new Condition();
            condition.Conditon = ifConditionRegex.Matches(coincidence)[0].Value;
            if (coincidence[coincidence.Length - 1] == '{')
            {
                indexEndIf = GetIndex(code, code.IndexOf(coincidence));
                var contentBracketsArray = code.Skip(code.IndexOf(coincidence)).Take(indexEndIf - code.IndexOf(coincidence) + 1);
                ifCode = string.Concat(contentBracketsArray);
                sub = string.Concat(code.Substring(indexEndIf + 4).Trim().Where(c => c != '\n' && c != '\r' && c != ' '));
            }

            else
            {
                indexEndIf = code.IndexOf(";", code.IndexOf(coincidence));
                var elseCodeArr = code.Skip(code.IndexOf(coincidence)).Take(indexEndIf - code.IndexOf(coincidence) + 1);
                ifCode = string.Concat(elseCodeArr);
                sub = string.Concat(code.Substring(indexEndIf + 4).Trim().Where(c => c != '\n' && c != '\r' && c != ' '));
            }

            if (sub.StartsWith("else"))
            {
                condition.IsHaveElse = true;
                var ifelse = new Else();
                var elseResult = GetElseCode(code, indexEndIf);
                var array = code.Skip(code.IndexOf(coincidence)).Take(elseResult.Item2 - code.IndexOf(coincidence) + 1);
                codeIfElse = string.Concat(array);
                ifelse.Code = elseResult.Item1;
                condition.Else = ifelse;
            }

            var id = Guid.NewGuid();
            condition.Id = id;
            condition.Code = ifCode.Substring(ifCode.IndexOf(")") + 1);
            codeIfElse = codeIfElse == string.Empty ? ifCode : codeIfElse;
            code = code.Replace(codeIfElse, "\r\ncondition" + id.ToString());
            return (code, condition);
        }

        private (string, int) GetElseCode(string code, int index)
        {
            int endIndex = -1;
            var elseCode = string.Empty;
            var startIndexElse = code.IndexOf("else", index);
            var su2b = string.Concat(code.Substring(startIndexElse + 4).Trim().Where(c => c != '\n' && c != '\r' && c != ' '));

            if (su2b.StartsWith("{"))
            {
                endIndex = GetIndex(code, code.IndexOf("else"));
                var elseCodeArr = code.Skip(startIndexElse).Take(endIndex - startIndexElse + 1);
                elseCode = string.Concat(elseCodeArr);
            }

            else
            {
                endIndex = code.IndexOf(";", startIndexElse);
                var elseCodeArr = code.Skip(startIndexElse).Take(endIndex - startIndexElse + 1);
                elseCode = string.Concat(elseCodeArr);
            }

            return (elseCode, endIndex);
        }

        private (string, List<While>) GetWhiles(string code)
        {
            var whileList = new List<While>();
            var matchCollection = whileRegex.Matches(code);
            foreach (Match item in matchCollection)
            {
                var whileC = new While();
                whileC.Conditon = whileConditionRegex.Matches(code)[0].Value;
                var id = Guid.NewGuid();
                whileC.Code = item.Value;
                whileC.Id = id;
                code = code.Replace(item.Value, "while" + id.ToString());
                whileList.Add(whileC);
            }
            return (code, whileList);
        }

        private (string, List<For>) GetFor(string code)
        {
            var matchCollection = forRegex.Matches(code);
            var forList = new List<For>();
            foreach (Match item in matchCollection)
            {
                var whileC = new For();
                whileC.Conditon = forConditionRegex.Matches(code)[0].Value;
                var id = Guid.NewGuid();
                var endIndex = GetIndex(item.Value);
                whileC.Code = item.Value.Substring(item.Value.IndexOf("{"), Math.Min(item.Value.Length, endIndex) - item.Value.IndexOf("{") + 1);
                whileC.Id = id;
                code = code.Replace(item.Value, "\r\nfor" + id.ToString());
                forList.Add(whileC);
            }
            return (code, forList);
        }

        private bool IsIfConditionTrue(string code, List<Variable> variables, Regex regex)
        {
            dynamic result = null;
            string expresion = string.Empty;
            expresion = code;
            var expresionArr = expresion.Skip(expresion.IndexOf("(") + 1).Take(expresion.LastIndexOf(")") - expresion.IndexOf("(") - 1);
            expresion = string.Concat(expresionArr);
            result = DoExpresion(expresion, variables);
            return result;
        }

        private Base GetAttachments(Base bas)
        {
            if (bas == null)
                bas = new Base();
            if (bas.Bases == null)
                bas.Bases = new List<Base>();
            var getIfElseTuple = GetIfElse(bas.Code);
            bas.Code = getIfElseTuple.Item1;
            bas.Bases.AddRange(getIfElseTuple.Item2);

            var grtWhilesREsult = GetWhiles(bas.Code);
            bas.Code = grtWhilesREsult.Item1;
            bas.Bases.AddRange(grtWhilesREsult.Item2);

            var getFor = GetFor(bas.Code);
            bas.Code = getFor.Item1;
            bas.Bases.AddRange(getFor.Item2);

            for (int i = 0; i < bas.Bases.Count; i++)
            {
                var subBase = GetAttachments(bas.Bases[i]);
                bas.Bases.AddRange(subBase.Bases);
            }
            return bas;
        }
    }
}
