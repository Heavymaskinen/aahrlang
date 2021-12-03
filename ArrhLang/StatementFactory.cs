using System;
using System.Collections.Generic;

namespace ArrhLang
{
    class StatementFactory
    {
        public static Statement Create(string line)
        {
            line = line.Trim();
            Console.WriteLine("Fact: " + line);

            if (line.StartsWith("if"))
            {
                var first = line.Substring(2).Trim();
                var clause = Create(first);
                if (clause is not BoolStatement)
                {
                    throw new Exception("Invalid clause " + clause);
                }

                return new IfStatement(line, (BoolStatement)clause, new List<Statement>());
            }

            if (line.StartsWith("for"))
            {
                var defLine = Utils.GetInner(line, '(', ')');
                var parts = defLine.Split(';');
                var clause = Create(parts[0]);
                if (clause is not BoolStatement boolStatement)
                {
                    throw new Exception("Invalid clause " + clause);
                }

                var incrementor = Create(parts[1]);

                return new ForStatement(line, boolStatement, incrementor, new List<Statement>());
            }

            if (line.Contains("="))
            {
                return new Assignment(line);
            }

            var operators = new[] { "+", "-", "%", "/" };
            foreach (var op in operators)
            {
                if (line.Contains(op))
                {
                    return new OperatorStatement(line, op);
                }
            }

            var comparators = new[] { "<", ">", "<=", ">=", "==" };
            foreach (var cp in comparators)
            {
                if (line.Contains(cp))
                {
                    return new CompareStatement(line, cp);
                }
            }

            if (line.Contains("("))
            {
                if (line.StartsWith('['))
                {
                    var index = Utils.GetInner(line, '[', ']');
                    var pars = Utils.GetInner(line, '(', ')');
                    Console.WriteLine("Pars: " + pars + ", " + line);
                    return new FunctionCallStatement(line, index, pars);
                }

                var name = line.Split('(')[0];
                var parameters = Utils.GetInner(line, '(', ')');
                Console.WriteLine("Builtin Pars: " + parameters + ", " + line);
                return new BuiltInFunctionStatement(line, name, parameters);
            }

            if (line.Contains("^"))
            {
                var parts = line.Split("^", StringSplitOptions.TrimEntries);
                return new Append(line, Create(parts[0]), Create(parts[1]));
            }

            if (Boolean.TryParse(line, out bool tmpb))
            {
                return new BoolStatement(line);
            }

            if (line.StartsWith("[here]"))
            {
                var index = Utils.GetInner(line.Split("re]")[1], '[', ']');
                return new LocalVariable(line, index);
            }

            if (line.StartsWith("["))
            {
                var index = Utils.GetInner(line, '[', ']');
                if (line.IndexOf('[') != line.LastIndexOf('['))
                {
                    var splitPoint = line.IndexOf(']');
                    var remainder = line.Substring(splitPoint + 1);
                    var varIndex = Utils.GetInner(remainder, '[', ']');
                    return new ArrayEntryVariable(line, index, varIndex);
                }

                return new EntryVariable(line, index);
            }

            if (line.StartsWith("'") || int.TryParse(line, out int tmpi))
            {
                return new Constant(line, line.Replace("'", ""));
            }

            if (line.StartsWith("$"))
            {
                return new Parameter(line, line.Substring(1));
            }

            if (line.StartsWith("&"))
            {
                var index = Utils.GetInner(line, '[', ']');
                return new Reference(line, index);
            }

            if (line.Length == 0)
            {
                return new SectionClose(line);
            }

            Console.WriteLine("UNHANDLED: " + line);
            return new Statement(line);
        }
    }
}