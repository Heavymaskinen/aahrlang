using System;
using System.Collections.Generic;
using System.Linq;

namespace ArrhLang
{
    public class Parser
    {
        public static string BlockCommentToken = "||";
        public static string CommentToken = ">>";
        private List<EntryType> entries = new();
        private Dictionary<string, string> constants = new() { { "MAIN", "666" } };

        public ArrhProgram ParseIt(string code)
        {
            entries.Clear();
            constants = new() { { "MAIN", "666" } };
            var newline = Environment.NewLine;
            code = code.Replace(newline+newline, newline+"§"+newline);
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
            bool isBlockCommment = false;
            for (var lineNo = 0; lineNo < lines.Length; lineNo++)
            {
                var currentLine = lines[lineNo].Trim();
                
                if (currentLine.StartsWith(BlockCommentToken))
                {
                    isBlockCommment = !isBlockCommment;
                    continue;
                }
                if (isBlockCommment)
                {
                    continue;
                };

                if (isPreamble)
                {
                    if (currentLine.StartsWith("["))
                    {
                        isPreamble = false;
                    }
                    else
                    {
                        var parts = currentLine.Split(" ");
                        constants.Add(parts[1], parts[2]);
                    }

                    continue;
                }

                if (currentLine.Contains("=>"))
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
            var value = leftPart.Replace("'", "").Replace("¨", "\n").Trim(' ');
            return new ValueType { Index = index, Value = value };
        }

        private EntryType ParseFunctionEntry(int index, string leftPart, string[] lines, int lineNo)
        {
            var argList = leftPart.Split("(")[1].Split(")")[0];
            var argParts = argList.Split(",");

            var statementLines = new List<string>();
            var currentLine = lines[lineNo].Trim();

            var isBlockComment = false;
            while (IsStatement(currentLine))
            {
                if (currentLine.StartsWith(BlockCommentToken))
                {
                    isBlockComment = !isBlockComment;
                }
                else if (currentLine.StartsWith(CommentToken) || isBlockComment)
                {
                }
                else
                {
                    statementLines.Add(currentLine);
                }
                lineNo++;
                currentLine = lines[lineNo].Trim();
            }

            var expressions = new List<Expression>();
            IfExpression ifExpression = null;
            ForExpression forExpression = null;

            var collectionStack = new Stack<List<Expression>>();
            collectionStack.Push(expressions);
            
            foreach (var l in statementLines)
            {
                var line = l.Replace("¨", "\n").Trim();
                
                if (line.StartsWith("for"))
                {
                    var parts = line.Split("(");
                    var inner = parts[1].Split(")")[0];
                    var segments = inner.Split(";");
                    var clause = ParseToExpression(segments[0]);
                    var increment = ParseToExpression(segments[1]);
                    forExpression = new ForExpression { Clause = clause, Increment = increment };
                    collectionStack.Push(forExpression.Expressions);

                } 
                else if (line.StartsWith("if"))
                {
                    ifExpression = new IfExpression();
                    var clauseStr = line.Substring(2).Trim();
                    var clauseExpression = ParseToExpression(clauseStr);
                    ifExpression.Clause = clauseExpression;
                    collectionStack.Push(ifExpression.Expressions);
                }
                else
                {
                    if (line == "§" && forExpression != null)
                    {
                        collectionStack.Pop();
                        collectionStack.Peek().Add(forExpression);
                        forExpression = null;
                        continue;
                    }
                    
                    if (line== "§" && ifExpression != null)
                    {
                        collectionStack.Pop();
                        collectionStack.Peek().Add(ifExpression);
                        ifExpression = null;
                        continue;
                    }

                    var item = ParseToExpression(line);
                    collectionStack.Peek().Add(item);
                }
            }

            if (ifExpression != null || forExpression != null)
            {
                throw new Exception("Infinite If or For-statement detected");
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
            return linePart != "}";
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
            if (string.IsNullOrEmpty(line))
            {
                throw new Exception("White space overflow");
            }
            
            if (IsAssignment(line))
            {
                var assignmentParts = line.Split(" = ", StringSplitOptions.TrimEntries);
                var rightSide = ParseToExpression(assignmentParts[1].Trim(' '));
                var left = assignmentParts[0];

                if (left.StartsWith("$"))
                {
                    throw new Exception("Parameters are read-only");
                }
                
                if (FirstElementIsLocal(left))
                {
                    var splitted = left.Split("[here]");
                    var localIndex = GetIndexFromSquares(splitted[1].Trim(' '));
                    return new LocalValueAssign { Index = localIndex, Value = rightSide };
                }

                var index = GetIndexFromSquares(assignmentParts[0].Trim(' '));
                return new ValueAssign { Index = index, Value = rightSide };
            }
            
            if (line.Contains("+"))
            {
                var parts = line.Split("+", StringSplitOptions.TrimEntries);
                var left = ParseToExpression(parts[0]);
                var right = ParseToExpression(parts[1]);
                return new SumExpression { Left = left, Right = right };
            }

            if (line.Contains("<="))
            {
                return CreateBooleanExpression(line, "<=");
            }

            if (line.Contains(">="))
            {
                return CreateBooleanExpression(line, ">=");
            }

            if (line.Contains("<"))
            {
                return CreateBooleanExpression(line, "<");
            }

            if (line.Contains(">"))
            {
                return CreateBooleanExpression(line, ">");
            }

            if (line.Contains("=="))
            {
                return CreateBooleanExpression(line, "==");
            }

            if (line.Contains("^"))
            {
                var parts = line.Split("^", StringSplitOptions.TrimEntries);
                var left = ParseToExpression(parts[0]);
                var right = ParseToExpression(parts[1]);
                return new AppendExpression { Left = left, Right = right };
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
            
            if (line == "§")
            {
                throw new Exception("Whitespace overflow");
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

            throw new Exception("Insufficient indexing " + line);
        }

        private Expression CreateBooleanExpression(string line, string sign)
        {
            var parts = line.Split(sign, StringSplitOptions.TrimEntries);
            var left = ParseToExpression(parts[0]);
            var right = ParseToExpression(parts[1]);
            return new BoolExpression { Left = left, Right = right, Sign = sign };
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
            var isFunctionCall = line.Contains("(");
            return isFunctionCall;
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
            var firstIndex = line.IndexOf('(');
            var first = line.Substring(firstIndex+1);
            var lastindex = first.LastIndexOf(')');
            if (lastindex <= 0)
            {
                lastindex = first.Length > 1? first.Length-1: first.Length;
            }
            
            var second = first.Substring(0, lastindex);
            if (second.Length <= 1)
            {
                return Array.Empty<string>();
            }

            var paramLine = second;
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
            else if (stmt is BoolExpression boolExp)
            {
                var left = ParseExpressionToFunction(boolExp.Left, program);
                var right = ParseExpressionToFunction(boolExp.Right, program);
                func = program.BoolFunc(left, right, boolExp.Sign);
            }
            else if (stmt is IfExpression ifExp)
            {
                var clause = ParseExpressionToFunction(ifExp.Clause, program);
                var inner = new List<Func<string>>();
                foreach (var exp in ifExp.Expressions)
                {
                    inner.Add(ParseExpressionToFunction(exp, program));
                }

                func = program.IfFunc(clause, inner);
            }
            else if (stmt is ForExpression forExp)
            {
                var clause = ParseExpressionToFunction(forExp.Clause, program);
                var increment = ParseExpressionToFunction(forExp.Increment, program);
                var inner = new List<Func<string>>();
                foreach (var exp in forExp.Expressions)
                {
                    inner.Add(ParseExpressionToFunction(exp, program));
                }

                func = program.ForFunc(clause, increment, inner);
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