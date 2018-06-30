using Microsoft.CSharp;
using MyVisualStudio.Model.Parser;
using MyVisualStudio.Model.Parser.Variables;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MyParsr
{
    public partial class Interpreter
    {
        #region Public Methods

        public string Code;


        public event Action<string> PrintEvent;

        public dynamic Run(string code)
        {
            var res = GetStructList(code);
            code = res.Item1;
            structList = res.Item2;
            functionList = GetFunctionList(code).Item2;
            var mainFunction = functionList.FirstOrDefault(f => f.Name == "Main");
            if (mainFunction == null)
                return mainFunction;
            var result = CodeRun(mainFunction.Code, null, mainFunction);
            Console.WriteLine("END");
            return result;
        }

        public string LoadFile(string path)
        {
            using (var streamReader = new StreamReader(string.Concat(path)))
            {
                Code = streamReader.ReadToEnd();
            }
            return Code;
        }

        public dynamic RunFile()
        {
            return Run(Code);
        }

        public dynamic RunFunction(string call)
        {
            return RunFunk(call, null);
        }

        public dynamic RunFunction(string nameFunction, params dynamic[] param)
        {
            if (!IsExistFunction(nameFunction))
            {
                Console.WriteLine("Function not Found");
                return null;
            }
            var func = GetFunctioByName(nameFunction);
            nameFunction += "(";
            var functionNameParamArr = func.Parameters.Split(',').ToList();
            for (int i = 0; i < functionNameParamArr.Count; i++)
            {
                nameFunction += param[i].ToString();
                if (i + 1 < functionNameParamArr.Count)
                    nameFunction += ",";
            }
            nameFunction += ")";
            return RunFunk(nameFunction, null);
        }

        #endregion

        #region Private Methods for RunCode

        //This method will be divided into parts
        private dynamic CodeRun(string code, List<Variable> variables, Base bas = null)
        {
            if (variables == null)
                variables = new List<Variable>();
            var lines = Regex.Split(code, "\r\n").ToList();

            for (int i = 0; i < lines.Count; i++)
            {
                var item = lines[i];
                if (string.IsNullOrWhiteSpace(item))
                    continue;

                var setFieldStructMatch = setStructFieldRegex.Matches(item);
                if (setFieldStructMatch.Count > 0)
                {
                    var setFieldStruct = setFieldStructMatch[0].Value.Trim();
                    SetFieldStruct(setFieldStruct, variables);
                    continue;
                }

                var callStructFunctionMatch = callStructFunctionRegex.Matches(item);
                if (callStructFunctionMatch.Count > 0)
                {
                    var callFunction = callStructFunctionMatch[0].Value.Trim();
                    var result = CallStructFunction(callFunction, variables);
                    if (result != null)
                    {
                        var repleseItem = item.Replace(callFunction, result.ToString() + ";");
                        lines[i] = repleseItem;
                        --i;
                    }
                    continue;
                }

                var createInstanseStructMatch = createInstanseStructRegex.Matches(item);
                if (createInstanseStructMatch.Count > 0)
                {
                    var createInstanseStruct = createInstanseStructMatch[0].Value;
                    CreateInstanceStruct(createInstanseStruct, variables);
                    continue;
                }

                var re = new Regex(@"\s*\w*\.\w*");
                var getStructFieldValue = re.Matches(item);
                if (getStructFieldValue.Count > 0)
                {
                    var getStructField = getStructFieldValue[0].Value;
                    var res = GetStructFieldValue(getStructField, variables);
                    var repleseItem = item.Replace(getStructField, res.ToString());
                    lines[i] = repleseItem;
                    --i;
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
                    if ((funk != null && funk.IsReturnValue || functionName == "RunSharp") && restur != null)
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
                    var id = forMatch[0].Value.Substring(3);
                    var res = RunFor(id, variables, bas);
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
                    var res = RunIfElse(id, variables, bas);
                    if (res != null)
                        return res;
                    continue;
                }

                var whileMatch = whileCallRegex.Matches(item);
                if (whileMatch.Count > 0)
                {
                    var id = whileMatch[0].Value.Substring(5);
                    var res = RunWhile(id, variables, bas);
                    if (res != null)
                        return res;
                    continue;
                }

                var returnMatch = returnRegex.Matches(item);
                if (returnMatch.Count > 0)
                {
                    var function = bas as Function;
                    if (!function.IsReturnValue)
                        continue;
                    var returnValue = returnMatch[0].Value;
                    var value = retutnValue.Matches(returnValue)[0].Value;
                    value = value.Substring(0, value.Length - 1);
                    var variable = GetVariableWithValue(value, variables);
                    var typeVariable = GetTypeVariable(variable);
                    if (typeVariable != function.ReturnType)
                        return null;
                    var rValue = GetValueVariable(variable);
                    if (rValue != null)
                        return rValue;
                    continue;
                }
            }

            return null;
        }

        private dynamic CallStructFunction(string code, List<Variable> variables)
        {
            var nameVariable = string.Concat(code.Take(code.IndexOf(".")));
            if (!variables.Any(v => v.Name == nameVariable))
                return null;
            var variable = (variables.FirstOrDefault(v => v.Name == nameVariable)) as VariableStruct;
            var methodName = string.Concat(code.Skip(code.IndexOf(".") + 1).Take(code.LastIndexOf("(") - code.IndexOf(".") - 1));
            var methodCall = string.Concat(code.Skip(code.IndexOf(".") + 1).Take(code.LastIndexOf(")") - code.IndexOf(".")));
            var struc = structList.FirstOrDefault(s => s.Name == variable.StructName);
            if (!struc.Functions.Any(f => f.Name == methodName))
                return null;
            var funk = struc.Functions.FirstOrDefault(f => f.Name == methodName);
            var result = RunFunk(methodCall, variables, funk);
            return result;
        }

        private dynamic GetStructFieldValue(string code, List<Variable> variables)
        {
            var regex = new Regex(@"\.\w*");

            var nameVariable = string.Concat(code.Take(code.IndexOf(".")));
            if (!variables.Any(v => v.Name == nameVariable))
                return null;
            var variable = variables.FirstOrDefault(v => v.Name == nameVariable);
            if (!(variable is VariableStruct))
                return null;
            var nameFiled = regex.Matches(code)[0].Value.Substring(1);
            var struc = variable as VariableStruct;
            if (!struc.Value.Any(s => s.Name == nameFiled))
                return null;

            var oldVariable = struc.Value.FirstOrDefault(s => s.Name == nameFiled);
            var res = GetValueVariable(oldVariable);
            return res;
        }

        private void CreateInstanceStruct(string code, List<Variable> variables)
        {
            var nameStruct = createStructNameRegex.Matches(code)[0].Value;
            if (!structList.Any(s => s.Name == nameStruct))
                return;
            var struc = structList.FirstOrDefault(s => s.Name == nameStruct);
            var structVariable = new VariableStruct();
            var variableNameMatch = variableNameRegex.Matches(code);
            structVariable.Name = string.Concat(variableNameMatch[0].Value.Substring(0, variableNameMatch[0].Value.Length - 1).Where(c => c != ' '));
            structVariable.StructName = nameStruct;
            structVariable.Value = struc.Fields;
            variables.Add(structVariable);
        }

        private void SetFieldStruct(string code, List<Variable> variables)
        {
            var regex = new Regex(@"\.\w*");

            var nameVariable = string.Concat(code.Take(code.IndexOf(".")));
            if (!variables.Any(v => v.Name == nameVariable))
                return;
            var variable = variables.FirstOrDefault(v => v.Name == nameVariable);
            if (!(variable is VariableStruct))
                return;

            var nameFiled = regex.Matches(code)[0].Value.Substring(1);
            var struc = variable as VariableStruct;
            if (!struc.Value.Any(s => s.Name == nameFiled))
                return;

            var fieldValue = variableValueRegex.Matches(code)[0].Value;
            fieldValue = fieldValue.Substring(1, fieldValue.Length - 2).Trim();
            var newFieldVariable = GetVariableWithValue(fieldValue, variables);
            var oldVariable = struc.Value.FirstOrDefault(s => s.Name == nameFiled);
            if (GetTypeVariable(oldVariable) != GetTypeVariable(newFieldVariable))
                return;

            newFieldVariable.Name = oldVariable.Name;
            int index = struc.Value.IndexOf(oldVariable);
            if (index != -1)
                struc.Value[index] = newFieldVariable;
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

            if (functionName == "RunSharp")
            {
                var paramCall = string.Concat(call.Skip(call.IndexOf("(") + 1).Take(call.LastIndexOf(")") - call.IndexOf("(") - 1)).Split(',');
                var listParam = new List<dynamic>();
                for (int i = 0; i < paramCall.Length; i++)
                {
                    if (i > 3)
                        listParam.Add(DoExpresion(paramCall[i], variables));
                    else { paramCall[i] = DoExpresion(paramCall[i], variables).ToString(); }
                }
                var res = RunSharpCode(paramCall[0], paramCall[1], paramCall[2], paramCall[3], listParam.ToArray());
                return res;
            }

            else if (IsExistFunction(functionName) || function != null)
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
            var strOut = (DoOperation(variable)).ToString();
            PrintEvent(strOut.ToString());
            //Console.WriteLine(strOut);
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

            if (variables == null)
                variables = new List<Variable>();

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

            if (int.TryParse(strWithoutDash, out int32))
            {
                variable = new VariableInt() { Value = int32 };
                return variable;
            }

            if (double.TryParse(strWithoutDash, out doubl))
            {
                variable = new VariableDouble() { Value = doubl };
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
                return DoOperation(GetVariable(operands[0]));


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

        private dynamic RunSharpCode(string path, string nameSpace, string className, string methodName, params dynamic[] param)
        {
            if (param == null)
                param = new dynamic[0];

            var code = string.Empty;
            path = CutQuotes(path);
            nameSpace = CutQuotes(nameSpace);
            className = CutQuotes(className);
            methodName = CutQuotes(methodName);
            using (var reader = new StreamReader(path))
            {
                code = reader.ReadToEnd();
            }
            var result = CompileSharpCode(code, nameSpace, className, methodName, param);
            return result;
        }

        private string CutQuotes(string str)
        {
            return str.Substring(1, str.Length - 2);
        }

        private dynamic CompileSharpCode(string code, string nameSpace, string className, string methodName, params dynamic[] param)
        {
            var providerOptions = new Dictionary<string, string> { { "CompilerVersion", "v3.5" } };

            var provider = new CSharpCodeProvider(providerOptions);

            var compilerParams = new CompilerParameters
            {
                GenerateInMemory = true,
                GenerateExecutable = false
            };

            var results = provider.CompileAssemblyFromSource(compilerParams, code);

            if (results.Errors.Count != 0)
                throw new Exception("Mission failed!");

            var obj = results.CompiledAssembly.CreateInstance(nameSpace + "." + className);
            var mi = obj.GetType().GetMethod(methodName);
            var res = mi.Invoke(obj, param);
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

        private (string, List<Function>) GetFunctionList(string code)
        {
            var functionsList = new List<Function>();
            var regex = new Regex(@"\s*function\s*\w*\s*\w*\s*\(");
            while (regex.Matches(code).Count > 0)
            {
                var function = new Function();
                var param = string.Empty;

                var index = GetIndex(code, code.IndexOf("function"));
                var functionCode = code.Substring(code.IndexOf("function"), code.Length - (Math.Abs(index - code.Length)) + 1 - code.IndexOf("function"));
                code = code.Substring(0, code.IndexOf("function")) + code.Substring(index + 1, Math.Abs(index - code.Length) - 1);
                function.Code = functionCode;

                var matchReturnType = functionReturnTypeRegex.Matches(functionCode);
                var matchParam = parametersRegex.Matches(functionCode);

                if (matchReturnType.Count > 0)
                    function.ReturnType = GetReturnType(matchReturnType[0].Value);

                var refexNameFunction = new Regex(string.Concat(@"(?<=\b", matchReturnType[0].Value, @"\s)(\w+)"));
                var matchName = refexNameFunction.Matches(functionCode);

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
            return (code, functionsList);
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
            var regex = new Regex(@"\s*struct\s*\w*\s*\(");

            while (regex.Matches(code).Count > 0)
            {
                var struc = new Struct();
                var index = GetIndex(code, code.IndexOf("struct"));
                var structCode = code.Substring(code.IndexOf("struct"), code.Length - (Math.Abs(index - code.Length)) + 1 - code.IndexOf("struct"));
                struc.Code = structCode;
                var matchName = structNameRegex.Matches(structCode);
                struc.Name = matchName[0].Value;
                var resultFunction = GetFunctionList(structCode);
                struc.Functions = resultFunction.Item2;
                struc.Fields = GetStructFields(resultFunction.Item1);
                code = code.Replace(structCode, "\r\n");
                structList.Add(struc);
            }
            return (code, structList);
        }

        private Type GetReturnType(string code)
        {
            Type type = null;
            if (code == "int")
                type = typeof(VariableInt);
            else if (code == "double")
                type = typeof(VariableDouble);
            else if (code == "bool")
                type = typeof(VariableBoolean);
            else if (code == "string")
                type = typeof(VariableString);
            else if (code == "array")
                type = typeof(VariableArray);

            return type;
        }

        private Type GetTypeVariable(Variable variable)
        {
            Type type = null;
            if (variable is VariableInt)
                type = typeof(VariableInt);
            else if (variable is VariableDouble)
                type = typeof(VariableDouble);
            else if (variable is VariableString)
                type = typeof(VariableString);
            else if (variable is VariableArray)
                type = typeof(VariableArray);

            return type;
        }

        private (string, List<Struct>) GetInterfaceList(string code)
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
                // struc.Functions = GetFunctionList(structCode);
                code = code.Replace(structCode, "\r\n");
                structList.Add(struc);
            }

            return (code, structList);
        }

        private List<Variable> GetStructFields(string code)
        {
            var regex = new Regex(@"(int|string|double|bool|array)\s*.*;");
            var regex2 = new Regex(@"(int|string|double|bool|array)\s*\w*;");
            var regex3 = new Regex(@"(int|string|double|bool|array)\s*\w*\s*=\s*\w*;");
            var variables = new List<Variable>();
            while (regex.Matches(code).Count > 0)
            {
                var field = regex.Matches(code)[0].Value;

                var wordArr = regex.Matches(field)[0].Value.Split(' ').Where(w => !string.IsNullOrWhiteSpace(w) || w != "=").ToArray();
                var type = GetReturnType(wordArr[0]);
                var instanse = Activator.CreateInstance(type);
                if (wordArr[1].Contains(";"))
                    wordArr[1] = wordArr[1].Substring(0, wordArr[1].Length - 1);

                if (regex2.Matches(field).Count > 0)
                {
                    var var = instanse as Variable;
                    var.Name = wordArr[1];
                    variables.Add(var);
                }
                else if (regex3.Matches(field).Count > 0)
                {
                    if (wordArr[3].Contains(";"))
                        wordArr[3] = wordArr[3].Substring(0, wordArr[3].Length - 1);
                    if (wordArr[3].StartsWith("="))
                        wordArr[3] = wordArr[3].Substring(1, wordArr[3].Length - 1);
                    var variable = GetVariableWithValue(wordArr[3], variables);
                    variable.Name = wordArr[1];
                    if (type == GetTypeVariable(variable))
                        variables.Add(variable);
                }
                code = code.Replace(field, "\r\n");
            }
            return variables;
        }

        #endregion

    }
}

