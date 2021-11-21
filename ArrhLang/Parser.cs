using System;
using System.Collections.Generic;

namespace ArrhLang
{
    public class Parser
    {
        private List<EntryType> entries = new();
        private Dictionary<string, string> constants = new() { { "MAIN", "666" } };

        public ArrhProgram ParseIt(string code)
        {
            entries.Clear();
            constants = new() { { "MAIN", "666" } };
            var lines = code.Split("\n", StringSplitOptions.TrimEntries);


            ParseEntriesFromCode(lines);

            var program = new ArrhProgram();
            foreach (var entry in entries)
            {
                FunctionCreator.AddEntryFunctionsToProgram(program, entry);
            }

            return program;
        }

        private void ParseEntriesFromCode(string[] lines)
        {
            bool isPreamble = true;
            for (var lineNo = 0; lineNo < lines.Length; lineNo++)
            {
                if (isPreamble)
                {
                    if (lines[lineNo].StartsWith("["))
                    {
                        isPreamble = false;
                    }
                    else
                    {
                        var parts = lines[lineNo].Split(" ");
                        constants.Add(parts[1], parts[2]);
                    }

                    continue;
                }

                if (lines[lineNo].Contains("=>"))
                {
                    lineNo = ParseEntry(lines, lineNo);
                }
            }
        }

        private int ParseEntry(string[] lines, int lineNo)
        {
            var entryParts = lines[lineNo].Split("=>");
            var indexStr = entryParts[0].Trim();
            if (constants.ContainsKey(indexStr))
            {
                indexStr = constants[indexStr];
            }


            var index = int.Parse(indexStr);
            var rightPart = entryParts[1].Trim();
            EntryType entryType;
            if (rightPart.Contains("{"))
            {
                lineNo++;
                entryType = ParseFunctionEntry(index, rightPart, lines, lineNo);
            }
            else
            {
                entryType = ParseValueEntry(rightPart, index);
            }

            if (entryType != null)
            {
                entries.Add(entryType);
            }

            return lineNo;
        }

        private static EntryType ParseValueEntry(string leftPart, int index)
        {
            var value = leftPart.Replace("'", "").Trim(' ');
            return new ValueType { Index = index, Value = value };
        }

        private EntryType ParseFunctionEntry(int index, string leftPart, string[] lines, int lineNo)
        {
            var argList = leftPart.Split("(")[1].Split(")")[0];
            var argParts = argList.Split(",");

            var statementLines = new List<string>();
            var linePart = lines[lineNo].Trim();
            while (IsStatement(linePart))
            {
                statementLines.Add(linePart);
                lineNo++;
                linePart = lines[lineNo].Trim();
            }

            var expressions = new List<Expression>();
            foreach (var line in statementLines)
            {
                expressions.Add(ParseToExpression(line));
            }

            var entryType = new FunctionType
            {
                Index = index,
                Expressions = expressions,
                Parameters = argParts
            };

            return entryType;
        }

        private static bool IsStatement(string linePart)
        {
            return linePart != "}" && linePart != "";
        }

        private List<Expression> ParseParametersAsExpressions(string line)
        {
            var paramParts = GetParamParts(line);
            var expressions = new List<Expression>();
            foreach (var part in paramParts)
            {
                var e = ParseToExpression(part.Trim(' '));
                expressions.Add(e);
            }

            return expressions;
        }

        private Expression ParseToExpression(string line)
        {
            if (line.Contains("+"))
            {
                var parts = line.Split("+", StringSplitOptions.TrimEntries);
                var left = ParseToExpression(parts[0]);
                var right = ParseToExpression(parts[1]);
                return new SumExpression { Left = left, Right = right };
            }

            if (line.Contains("^"))
            {
                var parts = line.Split("^", StringSplitOptions.TrimEntries);
                var left = ParseToExpression(parts[0]);
                var right = ParseToExpression(parts[1]);
                return new AppendExpression { Left = left, Right = right };
            }

            if (IsAssignment(line))
            {
                var parts = line.Split("=");
                
                var assignmentParts = line.Split(" = ");
                var rightSide = ParseToExpression(assignmentParts[1].Trim(' '));
                var left = parts[0];
                if (FirstElementIsLocal(left))
                {
                    var splitted = left.Split("[here]");
                    var localIndex = GetIndexFromSquares(splitted[1].Trim(' '));
                    return new LocalValueAssign { Index = localIndex, Value = rightSide };
                }
                var index = GetIndexFromSquares(parts[0].Trim(' '));
                return new ValueAssign { Index = index, Value = rightSide };
            }

            if (FirstElementIsLocal(line))
            {
                var parts = line.Split("[here]");
                var index = GetIndexFromSquares(parts[1].Trim(' '));

                return new LocalValueRead { Index = index };
            }

            if (IsParameter(line))
            {
                return new ParameterRead { Name = line.Substring(1).Trim() };
            }

            if (IsFunctionCall(line))
            {
                if (IsUsingEntry(line))
                {
                    var i = GetIndexFromSquares(line);
                    var parametersAsExpressions = ParseParametersAsExpressions(line);

                    return new FunctionCall
                    {
                        FunctionIndex = i,
                        Parameters = parametersAsExpressions
                    };
                }

                //Built in function
                var parts = line.Trim().Split("(");
                var name = parts[0];
                var expressions = ParseParametersAsExpressions(line);

                return new BuiltInFunctionCall
                {
                    Name = name,
                    Parameters = expressions
                };
            }

            

            if (IsUsingEntry(line))
            {
                var i = GetIndexFromSquares(line);
                var stmt = new ValueRead { Index = i };
                return stmt;
            }

            if (IsStringLiteral(line))
            {
                var val = line.Trim().Replace("'", "");
                var stmt = new Literal { Value = val };
                return stmt;
            }

            if (IsNumeric(line))
            {
                var stmt = new Literal { Value = line };
                return stmt;
            }

            throw new Exception("Insufficient indexing, "+line);
        }

        private static bool IsParameter(string line)
        {
            return line.StartsWith("$");
        }

        private static bool IsNumeric(string line)
        {
            var isInt = int.TryParse(line, out int tmp);
            return isInt;
        }

        private static bool IsStringLiteral(string line)
        {
            return line.StartsWith("'");
        }

        private static bool IsUsingEntry(string line)
        {
            return line.StartsWith("[");
        }

        private static bool IsFunctionCall(string line)
        {
            return line.Contains("(");
        }

        private static bool IsAssignment(string line)
        {
            return line.Contains(" = ");
        }

        private static bool FirstElementIsLocal(string line)
        {
            return line.StartsWith("[here]");
        }

        private static string[] GetParamParts(string line)
        {
            var first = line.Split("(");
            var second = first[1].Split(")");
            if (second[0].Length <= 1)
            {
                return Array.Empty<string>();
            }

            var paramLine = second[0];
            var paramParts = paramLine.Split(",");
            return paramParts;
        }

        private int GetIndexFromSquares(string line)
        {
            var endIndex = line.IndexOf("]");
            var i = line.Substring(1, endIndex - 1).Trim();
            if (constants.ContainsKey(i))
            {
                i = constants[i];
            }

            return int.Parse(i);
        }

    }

    class FunctionCreator
    {
        public static void AddEntryFunctionsToProgram(ArrhProgram program, EntryType entry)
        {
            if (entry is ValueType valEntry)
            {
                program.SetData(entry.Index, valEntry.Value);
            }
            else if (entry is FunctionType funcEntry)
            {
                AddFunctionTypeToProgram(program, funcEntry);
            }
        }

        private static void AddFunctionTypeToProgram(ArrhProgram program, FunctionType entry)
        {
            List<Func<string>> funcParts = BuildFunctionContent(program, entry.Expressions);

            var func = program.FullFunc(funcParts, entry.Parameters);
            program.SetFunction(entry.Index, func);
        }

        private static List<Func<string>> BuildFunctionContent(ArrhProgram program, List<Expression> statements)
        {
            var funcParts = new List<Func<string>>();
            foreach (var stmt in statements)
            {
                var exFunc = ParseExpressionToFunction(stmt, program);
                funcParts.Add(exFunc);
            }

            return funcParts;
        }

        private static Func<string> ParseExpressionToFunction(Expression stmt, ArrhProgram program)
        {
            Func<string> func = null;
            if (stmt is ValueRead read)
            {
                func = program.ValueReadFunc(read.Index);
            }
            else if (stmt is LocalValueRead localRead)
            {
                func = program.LocalReadFunc(localRead.Index);
            }
            else if (stmt is ParameterRead param)
            {
                func = program.ScopedReadFunc(param.Name);
            }
            else if (stmt is LocalValueAssign localAss)
            {
                var val = ParseExpressionToFunction(localAss.Value, program);
                func = program.LocalAssignFunc(localAss.Index, val);
            }
            else if (stmt is ValueAssign ass)
            {
                var val = ParseExpressionToFunction(ass.Value, program);
                func = program.AssignFunc(ass.Index, val);
            }
            else if (stmt is FunctionCall call)
            {
                var parFuncs = CreateParameterExpressions(program, call);
                func = program.FunctionCallFunc(call.FunctionIndex, parFuncs.ToArray());
            }
            else if (stmt is BuiltInFunctionCall builtIn)
            {
                var parFuncs = CreateParameterExpressions(program, builtIn);
                func = program.InternalCallFunc(builtIn.Name, parFuncs.ToArray());
            }
            else if (stmt is Literal literal)
            {
                func = program.ValueFunc(literal.Value);
            }
            else if (stmt is SumExpression sum)
            {
                var left = ParseExpressionToFunction(sum.Left, program);
                var right = ParseExpressionToFunction(sum.Right, program);

                func = program.SumFunc(left, right);
            }
            else if (stmt is AppendExpression append)
            {
                var left = ParseExpressionToFunction(append.Left, program);
                var right = ParseExpressionToFunction(append.Right, program);

                func = program.AppendFunc(left, right);
            }

            return func;
        }

        private static List<Func<string>> CreateParameterExpressions(ArrhProgram program, FunctionExpression call)
        {
            var parFuncs = new List<Func<string>>();
            foreach (var param in call.Parameters)
            {
                parFuncs.Add(ParseExpressionToFunction(param, program));
            }

            return parFuncs;
        }

    }
}