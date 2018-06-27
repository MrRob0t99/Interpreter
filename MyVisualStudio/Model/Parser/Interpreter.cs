using MyVisualStudio.Model.Parser;
using MyVisualStudio.Model.Parser.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MyParsr
{
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
            functionNameRegex = new Regex(@"(?<=\bfunction\s)(\w+)");
            returnFunctionRegex = new Regex(@"\W*(return)\W*");
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
        private readonly Regex createInstanseStructRegex;
        private readonly Regex structNameRegex;
        private readonly Regex createStructNameRegex;
        private readonly Regex callStructFunctionRegex;
        private string[] symbol;

        #endregion

        #region Public Methods

        public void Run(string code)
        {
            code = code.Replace("  ", " ");
            var res = GetStructList(code);
            code = res.Item1;
            structList = res.Item2;
            functionList = GetFunctionList(code);
            var mainFunction = functionList.FirstOrDefault(f => f.Name == "Main");
            CodeRun(mainFunction.Code, null, mainFunction);
            Console.WriteLine("END");
            Console.ReadKey();
        }

        #endregion

        #region Private Methods for RunCode

        //This method will be divided into parts
        private dynamic CodeRun(string code, List<Variable> variables, Base function = null)
        {
            if (variables == null)
                variables = new List<Variable>();
            var lines = Regex.Split(code, "\r\n").ToList();

            for (int i = 0; i < lines.Count; i++)
            {
                var item = lines[i];
                if (string.IsNullOrWhiteSpace(item))
                    continue;

                var callStructFunctionMatch = callStructFunctionRegex.Matches(item);
                if (callStructFunctionMatch.Count > 0)
                {
                    var callFunction = callStructFunctionMatch[0].Value.Trim();
                    var nameVariable = string.Concat(callFunction.Take(callFunction.IndexOf(".")));
                    if (!variables.Any(v => v.Name == nameVariable))
                        continue;
                    var variable = (variables.FirstOrDefault(v => v.Name == nameVariable)) as VariableStruct;
                    var methodName = string.Concat(callFunction.Skip(callFunction.IndexOf(".") + 1).Take(callFunction.LastIndexOf("(") - callFunction.IndexOf(".") - 1));
                    var methodCall = string.Concat(callFunction.Skip(callFunction.IndexOf(".") + 1).Take(callFunction.LastIndexOf(")") - callFunction.IndexOf(".")));
                    var struc = structList.FirstOrDefault(s => s.Name == variable.StructName);
                    if (!struc.Functions.Any(f => f.Name == methodName))
                        continue;
                    var funk = struc.Functions.FirstOrDefault(f => f.Name == methodName);
                    var result = RunFunk(methodCall, variables, funk);
                }

                var createInstanseStructMatch = createInstanseStructRegex.Matches(item);
                if (createInstanseStructMatch.Count > 0)
                {
                    var createInstanseStruct = createInstanseStructMatch[0].Value;
                    var nameStruct = createStructNameRegex.Matches(createInstanseStruct)[0].Value;
                    if (!structList.Any(s => s.Name == nameStruct))
                        continue;
                    var structVariable = new VariableStruct();
                    var variableNameMatch = variableNameRegex.Matches(code);
                    structVariable.Name = string.Concat(variableNameMatch[0].Value.Substring(0, variableNameMatch[0].Value.Length - 1).Where(c => c != ' '));
                    structVariable.StructName = nameStruct;
                    variables.Add(structVariable);
                    continue;
                }

                var callMatch = callFunctionRegex.Matches(item);
                if (callMatch.Count > 0 && !item.Contains("function"))
                {
                    var call = callMatch[0].Value;
                    call = ReplaceSubCallFunction(call, variables);
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
                    var r = UpdateArrItem(match, variables);
                    var name = arrByIndexRegex.Matches(match);
                    var names = name[0].Value;
                    names = names.Substring(0, names.Length - 1);
                    var value = (variables.FirstOrDefault(v => v.Name == names) as VariableArray).Value;
                    value.RemoveAt(r.Item1);
                    value.Insert(r.Item1, r.Item2);
                }

                var forMatch = forCallRegex.Matches(item);
                if (forMatch.Count > 0)
                {
                    string id = string.Empty;
                    foreach (Match conditionId in forMatch)
                    {
                        id = conditionId.Value.Substring(3);
                    }
                    var res = RunFor(id, variables, function);

                    if (res != null)
                        return res;
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
                    var id = conditionMatch[0].Value.Substring(9);
                    var res = RunIfElse(id, variables, function);
                    if (res != null)
                        return res;
                    continue;
                }

                var whileMatch = whileCallRegex.Matches(item);
                if (whileMatch.Count > 0)
                {
                    var id = whileMatch[0].Value.Substring(5);
                    var res = RunWhile(id, variables, function);
                    if (res != null)
                        return res;
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
                        return res;
                    continue;
                }
            }

            return null;
        }

        private dynamic RunFunk(string call, List<Variable> variables, Function function = null)
        {
            var functionName = GetFunctionName(call);
            if (function == null)
                function = GetFunctioByName(functionName);

            var functionParams = GetParametersForCallFunction(call, variables, function);

            if (functionName == "Print")
            {
                Print(functionParams.FirstOrDefault());
            }

            else if (IsExistFunction(functionName)|| function!=null)
            {
                var res = CodeRun(function.Code, functionParams, function);
                if (function.IsReturnValue)
                    return res;
            }

            return null;
        }

        private dynamic RunFor(string id, List<Variable> variables, Base bases)
        {
            var fors = bases.Bases.FirstOrDefault(c => c.Id.ToString() == id);
            if (fors == null)
                return null;
            var arr = fors.Conditon.Substring(1, fors.Conditon.Length - 2).Split(';').ToList();
            var variable = CreateVariables(arr[0] + ";", variables);
            variables.Add(variable);
            while (DoExpresion(arr[1], variables))
            {
                var res = CodeRun(fors.Code, variables, bases);
                if (res != null)
                    return res;

                var valueVariable = DoExpresion(arr[2], variables);
                var newVariable = GetVariable(valueVariable.ToString());
                newVariable.Name = variable.Name;
                int index = variables.IndexOf(variable);
                if (index != -1)
                    variables[index] = newVariable;
                variable = newVariable;
            }
            return null;
        }

        private dynamic RunWhile(string id, List<Variable> variables, Base bases)
        {
            var whiles = bases.Bases.FirstOrDefault(c => c.Id.ToString() == id);
            if (whiles == null)
                return null;
            while (IsIfConditionTrue(whiles.Conditon, variables, whileConditionRegex))
            {
                var res = CodeRun(whiles.Code, variables, bases);
                if (res != null)
                    return res;
            }

            return null;
        }

        private dynamic RunIfElse(string id, List<Variable> variables, Base bases)
        {
            var baseCondition = bases.Bases.FirstOrDefault(c => c.Id.ToString() == id);

            if (baseCondition == null)
            {
                return null;
            }
            else
            {
                var condition = baseCondition as Condition;
                dynamic res = null;
                var isTrue = IsIfConditionTrue(condition.Conditon, variables, ifConditionRegex);
                if (isTrue)
                    res = CodeRun(condition.Code, variables, bases);
                else if (condition.IsHaveElse == true && condition.Else != null)
                    res = CodeRun(condition.Else.Code, variables, bases);

                if (res != null)
                    return res;
            }
            return null;
        }

        private string ReplaceSubCallFunction(string call, List<Variable> variables)
        {
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
                    call = call.Replace(subCheck, restuir.ToString() + ")");
                    contentBrackets = contentBrackets.Replace(contentBrackets, restuir.ToString() + ")");
                }
            }

            return call;
        }

        private (int, Variable) UpdateArrItem(string code, List<Variable> variables)
        {
            var variable = CreateVariables(code, variables);
            var contentBracket = Regex.Match(code, @"\[([^)])\]").Groups[0].Value;
            contentBracket = contentBracket.Substring(1, contentBracket.Length - 2);

            int index = 0;
            int.TryParse(contentBracket, out index);

            if (variables != null && variables.Count > 0)
            {
                var fromVariabes = DoOperation(variables.FirstOrDefault(v => v.Name == contentBracket));
                int.TryParse(fromVariabes.ToString(), out index);
            }

            return (index, variable);
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
                if (item is VariableStruct)
                {
                    var struc = item as VariableStruct;
                    var newStruct = new VariableStruct();
                    newStruct.Name = struc.Name;
                    newStruct.StructName = struc.StructName;
                    if (struc.Value != null)
                        newStruct.Value = struc.Value;
                    variable = newStruct;
                }
                else if (!(item is VariableArray))
                {
                    variable = GetVariable(value.ToString());
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
                variable = GetVariable(operatingResult.ToString(), variables);
            }
            else
            {
                variable = (GetVariable(variabeValue, variables));
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

        private Variable GetVariable(string value, List<Variable> variables = null)
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

            if (operands.Count == 1)
                return DoOperation(GetVariable(operands[0]));

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

            var count = 0;
            dynamic res = null;

            for (int i = 0; i < actionList.Count; i++)
            {
                if (priorityAction.IndexOf(actionList[i]) != -1)
                {
                    var v1 = GetVariable(operands[i]);
                    var v2 = GetVariable(operands[i + 1]);
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
                    var v1 = GetVariable(operands[0], variables);
                    var v2 = GetVariable(operands[1], variables);

                    res = DoOperation(v1, v2, actionList[count].ToString());
                    operands.RemoveAt(1);
                    operands[0] = res.ToString();
                    ++count;
                }
            }
            return res;
        }

        #endregion

        #region Private Methods for FormationCode 

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
            var regex = new Regex(@"\s*function\s*\w*\(");
            while (regex.Matches(code).Count > 0)
            {
                var function = new Function();
                var param = string.Empty;

                var index = GetIndex(code, code.IndexOf("function"));
                var functionCode = code.Substring(code.IndexOf("function"), code.Length - (Math.Abs(index - code.Length)) + 1 - code.IndexOf("function"));
                code = code.Substring(0, code.IndexOf("function")) + code.Substring(index + 1, Math.Abs(index - code.Length) - 1);
                function.Code = functionCode;

                var matchName = functionNameRegex.Matches(functionCode);
                var matchParam = parametersRegex.Matches(functionCode);

                if (matchName.Count > 0)
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
                var endIndex = GetIndex(item.Value);
                whileC.Code = item.Value.Substring(item.Value.IndexOf("{"), Math.Min(item.Value.Length, endIndex) - item.Value.IndexOf("{") + 1);
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
                var fors = new For();
                fors.Conditon = forConditionRegex.Matches(code)[0].Value;
                var id = Guid.NewGuid();
                var endIndex = GetIndex(item.Value);
                fors.Code = item.Value.Substring(item.Value.IndexOf("{"), Math.Min(item.Value.Length, endIndex) - item.Value.IndexOf("{") + 1);
                fors.Id = id;
                code = code.Replace(item.Value, "\r\nfor" + id.ToString());
                forList.Add(fors);
            }
            return (code, forList);
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

        private (string, List<Struct>) GetStructList(string code)
        {
            var structList = new List<Struct>();
            var regex = new Regex(@"\s*struct\s*\w*\s*{");

            while (regex.Matches(code).Count > 0)
            {
                var struc = new Struct();
                var index = GetIndex(code, code.IndexOf("struct"));
                var structCode = code.Substring(code.IndexOf("struct"), code.Length - (Math.Abs(index - code.Length)) + 1 - code.IndexOf("struct"));
                struc.Code = structCode;
                var matchName = structNameRegex.Matches(structCode);
                struc.Name = matchName[0].Value;
                struc.Functions = GetFunctionList(structCode);
                code = code.Replace(structCode, "\r\n");
                structList.Add(struc);
            }

            return (code, structList);
        }

        #endregion
    }
}
