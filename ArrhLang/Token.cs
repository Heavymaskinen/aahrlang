using System;
using System.Collections.Generic;

namespace ArrhLang
{
    public abstract class Token
    {
        public string Line;
    }

    class Assignment : Statement
    {
        public readonly Assignable Assignable;
        public readonly Statement Statement;

        public Assignment(string line):base(line)
        {
            var parts = line.Split('=', StringSplitOptions.TrimEntries);
            Assignable = new Assignable(parts[0]);
            Statement = StatementFactory.Create(parts[1]);
        }

        public override string ToString()
        {
            return "ASSIGN|"+Assignable + " = " + Statement;
        }
    }

    class Assignable : Token
    {
        public bool IsLocal;
        public string Index;
        public bool IsArray;

        public Assignable(string line)
        {
            IsLocal = line.StartsWith("[here]");
            IsArray = line.Contains("[]");
            Line = line;

            if (IsLocal)
            {
                var indexPart = line.Split("re]")[1];
                Index = Utils.GetInner(indexPart, '[', ']');
            }
            else
            {
                Index = Utils.GetInner(line, '[', ']');
            }
        }

        public override string ToString()
        {
            if (IsArray) return $"[{Index}][]";
            
            return (IsLocal ? $"[here][{Index}]" : $"[{Index}]");
        }
    }

    class IfStatement : Statement
    {
        public readonly BoolStatement clause;
        public readonly List<Statement> body;

        public IfStatement(string line, BoolStatement clause, List<Statement> body) : base(line)
        {
            this.clause = clause;
            this.body = body;
        }

        public override string ToString()
        {
            return $"IF {clause} (+{body.Count})";
        }
    }

    class ForStatement : Statement
    {
        public readonly BoolStatement clause;
        public readonly Statement incrementor;
        public readonly List<Statement> body;

        public ForStatement(string line, BoolStatement clause, Statement incrementor, List<Statement> body) : base(line)
        {
            this.clause = clause;
            this.incrementor = incrementor;
            this.body = body;
        }
    }

    class BoolStatement : Statement
    {
        public BoolStatement(string line) : base(line)
        {
        }
    }

    class CompareStatement : BoolStatement
    {
        public readonly Statement _left;
        public readonly Statement _right;
        public readonly string _sign;

        public CompareStatement(string line, string sign) : base(line)
        {
            _sign = sign;
            var parts = line.Split(sign);
            _left = StatementFactory.Create(parts[0]);
            _right = StatementFactory.Create(parts[1]);
        }

        public override string ToString()
        {
            return $"CMP({_left} {_sign} {_right})";
        }
    }

    class SectionClose : Statement
    {
        public SectionClose(string line) : base(line)
        {
        }

        public override string ToString()
        {
            return "ENDIF";
        }
    }

    class Statement : Token
    {
        public Statement(string line)
        {
            Line = line;
        }
    }

    class Constant : Statement
    {
        public readonly string Value;

        public Constant(string line, string value) : base(line)
        {
            Value = value.Replace("'", "");
        }

        public override string ToString()
        {
            return "C" + Value;
        }
    }

    abstract class VariableType : Statement
    {
        public readonly string Index;

        protected VariableType(string line, string index) : base(line)
        {
            Index = index;
        }
    }

    class EntryVariable : VariableType
    {
       public EntryVariable(string line, string index) : base(line, index)
        {
        }

        public override string ToString()
        {
            return $"Var[{Index}]";
        }
    }

    class LocalVariable : Statement
    {
        public readonly string Index;

        public LocalVariable(string line, string index) : base(line)
        {
            Index = index;
        }

        public override string ToString()
        {
            return "Local@" + Index;
        }
    }
    
    class ArrayEntryVariable : EntryVariable
    {
        public readonly string varIndex;

        public ArrayEntryVariable(string line, string index, string varIndex) : base(line, index)
        {
            this.varIndex = varIndex;
        }
        
        public override string ToString()
        {
            return "ArrayVar@" + Index;
        }
    }

    class Append : Statement
    {
        public readonly Statement left;
        public readonly Statement right;

        public Append(string line, Statement left, Statement right) : base(line)
        {
            this.left = left;
            this.right = right;
        }

        public override string ToString()
        {
            return $"{left} ^ {right}";
        }
    }

    class Parameter : Statement
    {
        public readonly string Name;

        public Parameter(string line, string name) : base(line)
        {
            Name = name;
        }

        public override string ToString()
        {
            return $"Param[{Name}]";
        }
    }
    
    class Reference : Statement
    {
        public readonly string Index;

        public Reference(string line, string index) : base(line)
        {
            Index = index;
        }

        public override string ToString()
        {
            return $"Ref[{Index}]";
        }
    }

    class BuiltInFunctionStatement : Statement
    {
        public readonly string name;
        public List<Statement> ParameterValues;
        public BuiltInFunctionStatement(string line, string name, string parameterValueStr) : base(line)
        {
            this.name = name;
            
            ParameterValues = new List<Statement>();

            if (string.IsNullOrEmpty(parameterValueStr)) return;

            var paramList = parameterValueStr.Split(",");
            foreach (var param in paramList)
            {
                var stmt = StatementFactory.Create(param);
                ParameterValues.Add(stmt);
            }
        }
    }

    class FunctionCallStatement : Statement
    {
        public string FunctionIndex;
        public List<Statement> ParameterValues;

        public FunctionCallStatement(string line, string index, string parameterValueStr) : base(line)
        {
            FunctionIndex = index;
            ParameterValues = new List<Statement>();

            if (string.IsNullOrEmpty(parameterValueStr)) return;

            var paramList = parameterValueStr.Split(",");
            foreach (var param in paramList)
            {
                var stmt = StatementFactory.Create(param);
                ParameterValues.Add(stmt);
            }
        }

        public override string ToString()
        {
            var pars = String.Join(",", ParameterValues);
            return $"CallFuncStmt[{FunctionIndex}]({pars})";
        }
    }

    class OperatorStatement : Statement
    {
        public Statement Left;
        public Statement Right;
        public string Operator;

        public OperatorStatement(string line, string op) : base(line)
        {
            Operator = op;
            var parts = line.Split(op);
            Left = StatementFactory.Create(parts[0]);
            Right = StatementFactory.Create(parts[1]);
        }

        public override string ToString()
        {
            return "OP:" + Left + " " + Operator + " " + Right;
        }
    }
}