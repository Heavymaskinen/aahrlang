using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ArrhLang
{
    public class FunctionCatalog
    {
        private ProgramType program;
         public Func<string> ValueFunc(string val)
        {
            return () => val;
        }

        public Func<string[]> ArrayValueFunc(string[] val)
        {
            return () => val;
        }

        public Func<string[]> ReferenceFunc(string index)
        {
            return () => program.GetArray(index)();
        }

        public Func<string> ScopedReadFunc(string name)
        {
            return () => program.GetFromScope(name)();
        }

        public Func<string> FunctionCallFunc(string index, Func<object>[] parameters)
        {
            return () =>
            {
                Console.WriteLine("call function " + index);
                var values = new List<string>();
                foreach (var p in parameters)
                {
                    values.Add(p().ToString());
                }

                return program.GetFunction(index)(values.ToArray());
            };
        }

        public Func<string> InternalCallFunc(string name, Func<object>[] parameters)
        {
            return () => program.GetBuiltIn(name)(parameters);
        }

        public Func<string> ForFunc(Func<string> clause, Func<string> increment, List<Func<string>> lines)
        {
            return () =>
            {
                var val = "";

                while (clause() == bool.TrueString)
                {
                    foreach (var line in lines)
                    {
                        val = line();
                    }

                    increment();
                }

                return val;
            };
        }

        public Func<string[], string> FullFunc(List<Func<string>> lines, string[] parameters)
        {
            return values =>
            {
                program.NewScope();
                
                if (values != null)
                {
                    for (var i = 0; i < values.Length; i++)
                    {
                        program.AddToScope(parameters[i].Trim(), values[i]);
                    }
                }

                string output = null;
                foreach (var line in lines)
                {
                    output = line();
                }

                program.CloseScope();

                return output;
            };
        }


        public FunctionCatalog(ProgramType program)
        {
            this.program = program;
        }

        public Func<string> LocalReadFunc(string localIndex)
        {
            Console.WriteLine("Read local " + localIndex);
            return () => program.GetLocal(localIndex)();
        }

        public Func<string> ArrReadFunc(string index, string varIndex)
        {
            Console.WriteLine("Read array " + index + ", " + varIndex);
            return () =>
            {
                if (program.HasObject(index))
                {
                    return program.GetObject(index).GetData(varIndex);
                }
                
                return program.GetArray(index)()[int.Parse(varIndex)];
            };
        }

        public Func<string> SumFunc(Func<string> left, Func<string> right)
        {
            return () =>
            {
                Console.WriteLine("Sum func");
                var leftVal = left();
                var rightVal = right();

                var sumResult = int.Parse(leftVal) + int.Parse(rightVal);
                return sumResult + "";
            };
        }

        private static Dictionary<string, Func<int, int, int>> Calculators = new()
        {
            { "+", (a, b) => a + b },
            { "%", (a, b) => a % b },
            { "/", (a, b) => a / b },
            { "-", (a, b) => a - b }
        };

        public Func<string> CalcFunc(Func<string> left, Func<string> right, string opSign)
        {
            return () =>
            {
                Console.WriteLine("Calc func " + opSign);
                if (!Calculators.ContainsKey(opSign)) throw new Exception("Invalid operator " + opSign);
                var leftVal = left();
                var rightVal = right();

                return Calculators[opSign](int.Parse(leftVal), int.Parse(rightVal)) + "";
            };
        }

        public Func<string> AppendFunc(Func<string> left, Func<string> right)
        {
            return () =>
            {
                Console.WriteLine("Append func");
                var leftVal = left();
                var rightVal = right();

                return leftVal + rightVal;
            };
        }

        public Func<string> LocalAssignFunc(string localAssIndex, Func<string> localAssValue)
        {
            return () =>
            {
                Console.WriteLine("Assign to local " + localAssIndex);
                program.SetLocal(localAssIndex, ValueFunc(localAssValue()));
                return program.GetLocalData(localAssIndex);
            };
        }

        internal Func<string> AssignFunc(string index, Func<string> val)
        {
            return () =>
            {
                Console.WriteLine("Assign to " + index);
                program.SetData(index, val());
                
                return program.GetData(index);
            };
        }

        internal Func<string> BoolFunc(Func<string> left, Func<string> right, string sign)
        {
            return () =>
            {
                var leftVal = left();
                var rightVal = right();

                if (sign == "==")
                {
                    var s = (leftVal == rightVal).ToString();
                    return s;
                }

                var leftNum = int.Parse(leftVal);
                var rightNum = int.Parse(rightVal);

                if (sign == "<")
                {
                    return (leftNum < rightNum).ToString();
                }

                if (sign == ">")
                {
                    var boolVal = (leftNum > rightNum).ToString();
                    return boolVal;
                }

                if (sign == "<=")
                {
                    return (leftNum <= rightNum).ToString();
                }

                if (sign == ">=")
                {
                    return (leftNum >= rightNum).ToString();
                }

                return "Weird sign:" + sign;
            };
        }

        internal Func<string> IfFunc(Func<string> clause, List<Func<string>> inner)
        {
            return () =>
            {
                string result = "";
                var cl = clause();
                Console.WriteLine("If Clause " + cl);
                var boolClause = bool.Parse(cl);
                if (boolClause)
                {
                    foreach (var stmt in inner)
                    {
                        result = stmt();
                    }
                }

                return result;
            };
        }

        public Func<string> ArrAppendFunc(string index, Func<string> value)
        {
            return () =>
            {
                Console.WriteLine($"Append {value()} to array at {index}");
                var actualValue = value();
                var arr = program.GetArrayData(index);
                var newArr = arr.Append(actualValue).ToArray();
                program.SetArrayData(index, newArr);

                return actualValue;
            };
        }

        public Func<string> ValueReadFunc(string readIndex)
        {
            return () => program.GetData(readIndex);
        }
    }
}