using System;
using System.Collections.Generic;

namespace ArrhLang
{
    public class ArrhProgram
    {
        private Dictionary<int, Func<string>> data = new();
        private Dictionary<int, Func<string>> locals = new();
        private Stack<Dictionary<string, string>> scopedStack = new();
        private Dictionary<string, string> scoped = new();
        private Dictionary<int, Func<string[], string>> functions = new();
        

        private Dictionary<string, Func<string[], string>> builtIn = new()
        {
            {
                "out",
                (s) =>
         {
             Console.Write(s.Length > 0 ? s[0] : "");
             return null;
         }
            }
        };

        public string GetData(int index) => data[index]();
        public void SetData(int index, string value) => data[index] = ValueFunc(value);

        public ArrhProgram()
        {
            scopedStack.Push(scoped);
        }

        public Func<string> ValueFunc(string val)
        {
            return () => val;
        }

        public Func<string> ValueReadFunc(int index)
        {
            return () => data[index]();
        }

        public Func<string> ScopedReadFunc(string name)
        {
            return () => scoped[name];
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
                    string val = p();
                    values.Add(val);
                }

                return builtIn[name](values.ToArray());
            };
        }

        public Func<string[], string> FullFunc(List<Func<string>> lines, string[] parameters)
        {
            return (values) =>
            {
                scoped = new();
                scopedStack.Push(scoped);
                if (values != null)
                {
                    for (var i = 0; i < values.Length; i++)
                    {
                        scoped.Add(parameters[i].Trim(), values[i]);
                    }
                }

                string output = null;
                foreach (var line in lines)
                {
                    output = line();
                }

                scoped = scopedStack.Pop();

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

        public Func<string> SumFunc(Func<string> left, Func<string> right)
        {
            return () =>
            {
                var leftVal = left();
                var rightVal = right();

                return (int.Parse(leftVal) + int.Parse(rightVal)) + "";
            };
        }

        public Func<string> AppendFunc(Func<string> left, Func<string> right)
        {
            return () =>
            {
                var leftVal = left();
                var rightVal = right();

                return leftVal + rightVal;
            };
        }

        public Func<string> LocalAssignFunc(int localAssIndex, Func<string> localAssValue)
        {
            return () =>
            {
                locals[localAssIndex] = localAssValue;
                return locals[localAssIndex]();
            };
        }

        internal Func<string> AssignFunc(int index, Func<string> val)
        {
            return () =>
            {
                data[index] = val;
                return data[index]();
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
                    return (leftVal == rightVal).ToString();
                }

                var leftNum = int.Parse(leftVal);
                var rightNum = int.Parse(rightVal);

                if (sign == "<")
                {
                    return (leftNum < rightNum).ToString();
                }

                if (sign == ">")
                {
                    return (leftNum > rightNum).ToString();
                }

                if (sign == "<=")
                {
                    return (leftNum <= rightNum).ToString();
                }

                if (sign == ">=")
                {
                    return (leftNum >= rightNum).ToString();
                }

                return "";
            };
        }

        internal Func<string> IfFunc(Func<string> clause, List<Func<string>> inner)
        {
            return () =>
            {
                string result = "";
                var boolClause = bool.Parse(clause());
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
    }
}