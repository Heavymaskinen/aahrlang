using System;
using System.Collections.Generic;
using System.IO;

namespace ArrhLang
{
    class EntryType
    {
        public int Index;
    }

    class ValueType : EntryType
    {
        public string Value;
    }

    class FunctionType : EntryType
    {
        public List<Expression> Expressions;
    }

    class Expression
    {
    }

    class FunctionCall : Expression
    {
        public int FunctionIndex;
        public List<Expression> Parameters;
    }

    class BuiltInFunctionCall : Expression
    {
        public string Name;
        public Expression[] Parameters;
    }

    class ValueRead : Expression
    {
        public int Index;
    }

    class ValueAssign : Expression
    {
        public int Index;
        public string Value;
    }

    class LocalValueRead : Expression
    {
        public int Index;
    }

    class LocalValueAssign : Expression
    {
        public int Index;
        public Expression Value;
    }

    class Literal : Expression
    {
        public string Value;
    }

    public class Parser
    {
        private List<EntryType> entries = new();

        public ArrhProgram ParseIt(string code)
        {
            entries.Clear();
            var lines = code.Split("\n");
            for (var lineNo = 0; lineNo < lines.Length; lineNo++)
            {
                if (lines[lineNo].Contains("=>"))
                {
                    lineNo = ParseEntry(lines, lineNo);
                }
            }

            var program = new ArrhProgram();
            foreach (var entry in entries)
            {
                if (entry is ValueType)
                {
                    program.SetData(entry.Index, (entry as ValueType).Value);
                }
                else if (entry is FunctionType)
                {
                    var statements = (entry as FunctionType).Expressions;
                    var funcParts = new List<Func<string>>();
                    foreach (var stmt in statements)
                    {
                        var exFunc = ParseExpression(stmt, program);

                        funcParts.Add(exFunc);
                    }

                    var func = program.FullFunc(funcParts);
                    program.SetFunction(entry.Index, func);
                }
            }

            return program;
        }

        private static Func<string> ParseExpression(Expression stmt, ArrhProgram program)
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
            else if (stmt is LocalValueAssign localAss)
            {
                var val = ParseExpression(localAss.Value, program);
                func = program.LocalAssignFunc(localAss.Index, val);
            }
            else if (stmt is FunctionCall call)
            {
                var parFuncs = new List<Func<string>>();
                foreach (var param in call.Parameters)
                {
                    parFuncs.Add(ParseExpression(param, program));
                }

                func = program.FunctionCallFunc(call.FunctionIndex, parFuncs.ToArray());
            }
            else if (stmt is BuiltInFunctionCall builtIn)
            {
                var parFuncs = new List<Func<string>>();
                foreach (var param in builtIn.Parameters)
                {
                    parFuncs.Add(ParseExpression(param, program));
                }

                func = program.InternalCallFunc(builtIn.Name, parFuncs.ToArray());
            }
            else if (stmt is Literal literal)
            {
                func = program.ValueFunc(literal.Value);
            }

            return func;
        }

        private int ParseEntry(string[] lines, int lineNo)
        {
            var entryParts = lines[lineNo].Split("=>");
            var indexStr = entryParts[0].Trim();
            if (indexStr == "MAIN")
            {
                indexStr = "666";
            }

            var index = int.Parse(indexStr);
            EntryType entryType = null;
            var rightPart = entryParts[1].Trim();
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
            var value = leftPart.Replace("'", "").Trim();
            return new ValueType { Index = index, Value = value };
        }

        private EntryType ParseFunctionEntry(int index, string leftPart, string[] lines, int lineNo)
        {
            var argList = leftPart.Split("(");
            var statementLines = new List<string>();
            var linePart = lines[lineNo].Trim();
            while (linePart != "}" && linePart != "")
            {
                statementLines.Add(linePart);
                lineNo++;
                linePart = lines[lineNo].Trim();
            }

            var expressions = new List<Expression>();
            foreach (var line in statementLines)
            {
                expressions.Add(ParseExpression(line));
            }

            var entryType = new FunctionType
            {
                Index = index,
                Expressions = expressions
            };

            return entryType;
        }

        private Expression ParseExpression(string line)
        {
            if (line.StartsWith("[here]"))
            {
                var parts = line.Split("[here]");
                var index = GetIndexFromSquares(parts[1].Trim());
                
                if (line.Contains(" = "))
                {
                    var assignmentParts = line.Split(" = ");
                    var rightSide = ParseExpression(assignmentParts[1].Trim());
                    var stmt = new LocalValueAssign { Index = index, Value = rightSide };
                    return stmt;
                }

                return new LocalValueRead { Index = index };
            }
            if (line.Contains("("))
            {
                if (line.StartsWith("["))
                {
                    //Function call
                    var i = GetIndexFromSquares(line);

                    var parametersAsExpressions = ParseParametersAsExpressions(line);

                    var stmt = new FunctionCall
                    {
                        FunctionIndex = i,
                        Parameters = parametersAsExpressions
                    };

                    return stmt;
                }

                //Built in function
                var parts = line.Trim().Split("(");
                var name = parts[0];
                var expressions = ParseParametersAsExpressions(line);
                var call = new BuiltInFunctionCall
                {
                    Name = name,
                    Parameters = expressions.ToArray()
                };

                return call;
            }

            if (line.StartsWith("["))
            {
                var i = GetIndexFromSquares(line);
                var stmt = new ValueRead { Index = i };
                return stmt;
            }

            if (line.StartsWith("'"))
            {
                var val = line.Trim().Replace("'", "");
                var stmt = new Literal { Value = val };
                return stmt;
            }

            int tmp = 0;
            var isInt = int.TryParse(line, out tmp);
            if (isInt)
            {
                var stmt = new Literal { Value = line };
                return stmt;
            }

            throw new Exception("Insufficient indexing");
        }

        private List<Expression> ParseParametersAsExpressions(string line)
        {
            var paramParts = GetParamParts(line);
            var expressions = new List<Expression>();
            foreach (var part in paramParts)
            {
                var e = ParseExpression(part.Trim());
                expressions.Add(e);
            }

            return expressions;
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

        private static int GetIndexFromSquares(string line)
        {
            var endIndex = line.IndexOf("]");
            var i = line.Substring(1, endIndex - 1).Trim();
            return int.Parse(i);
        }
    }

    public class ArrhInterpreter
    {
        public void RunFile(string filename)
        {
            if (!File.Exists(filename))
            {
                throw new Exception("Can't do it, mate! " + filename);
            }

            try
            {
                var code = File.ReadAllText(filename);
                var parser = new Parser();
                var program = parser.ParseIt(code);
                program.GetFunction(666)(null);
            }
            catch (Exception e)
            {
                Console.WriteLine("Arrh intepretation terminated with fault: "+e.Message);
            }
        }
    }

    public class ArrhProgram
    {
        private Dictionary<int, Func<string>> data = new();
        private Dictionary<int, Func<string>> locals = new();
        private Dictionary<int, Func<string[], string>> functions = new();

        private Dictionary<string, Func<string[], string>> builtIn = new()
        {
            {
                "out", (s) =>
                {
                    Console.Write(s.Length > 0 ? s[0] : "");
                    return null;
                }
            }
        };

        public string GetData(int index) => data[index]();
        public void SetData(int index, string value) => data[index] = ValueFunc(value);

        public Func<string> ValueFunc(string val)
        {
            return () => val;
        }

        public Func<string> ValueReadFunc(int index)
        {
            return () => data[index % data.Count]();
        }

        public Func<string> FunctionCallFunc(int index, Func<string>[] parameters)
        {
            return () =>
            {
                var values = new List<string>();
                foreach (var p in parameters)
                {
                    values.Add(p());
                }

                return functions[index % functions.Count](values.ToArray());
            };
        }

        public Func<string> InternalCallFunc(string name, Func<string>[] parameters)
        {
            return () =>
            {
                var values = new List<string>();
                foreach (var p in parameters)
                {
                    values.Add(p());
                }

                return builtIn[name](values.ToArray());
            };
        }

        public Func<object[], string> FullFunc(List<Func<string>> lines)
        {
            return (o) =>
            {
                string output = null;
                foreach (var line in lines)
                {
                    output = line();
                }

                return output;
            };
        }

        public void SetFunction(int index, Func<string[], string> function) => functions[index] = function;

        public Func<string[], string> GetFunction(int index) => functions[index];

        public Func<string> GetLocal(int index) => locals[index];

        public Func<string> LocalReadFunc(int localIndex)
        {
            return () => locals[localIndex]();
        }

        public Func<string> LocalAssignFunc(int localAssIndex, Func<string> localAssValue)
        {
            return () =>
            {
                locals[localAssIndex] = localAssValue;
                return locals[localAssIndex]();
            };
        }
    }
}