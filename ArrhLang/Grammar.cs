using System;
using System.Collections.Generic;

namespace ArrhLang
{
    public class EntryType
    {
        public string Index;
        
        public override string ToString()
        {
            return "EntryType@"+Index;
        } 
    }

    class ValueType : EntryType
    {
        public string Value;

        public override string ToString()
        {
            return "ValueType[" + Value + "]@"+Index;
        } 
    }
    
    class ArrayType : EntryType
    {
        public string[] Value;

        public override string ToString()
        {
            return "ArrayType[" + String.Join(',',Value) + "]@"+Index;
        } 
    }

    class FunctionType : EntryType
    {
        public string[] Parameters;
        public List<Expression> Expressions;
        
        public override string ToString()
        {
            return $"FunctionType[{Index}]({String.Join(", ",Parameters)})";
        } 
    }
    
    class ObjectEntryType : EntryType
    {
        public List<EntryType> Entries;
        
        public override string ToString()
        {
            return $"(Object)";
        } 
    }

    class Operator : Expression
    {
        public string Sign;
        public Expression Left;
        public Expression Right;
    }

    class Expression
    {
        public string Line;
    }

    class IfExpression : Expression
    {
        public Expression Clause;
        public List<Expression> Expressions = new();
    }

    class ForExpression : Expression
    {
        public Expression Clause;
        public Expression Increment;
        public List<Expression> Expressions = new();
    }

    class BoolExpression : Expression
    {
        public Expression Left;
        public Expression Right;
        public string Sign;
    }

    class ReferenceExpression : Expression
    {
        public string Index;

        public ReferenceExpression(string index)
        {
            Index = index;
        }
    }

    class FunctionExpression : Expression
    {
        public List<Expression> Parameters;
    }

    class FunctionCall : FunctionExpression
    {
        public string FunctionIndex;

        public override string ToString()
        {
            return $"CallFunc [{FunctionIndex}]({string.Join(", ",Parameters)})";
        }
    }

    class BuiltInFunctionCall : FunctionExpression
    {
        public string Name;
    }

    class SumExpression : Expression
    {
        public Expression Left;
        public Expression Right;
    }

    class AppendExpression : Expression
    {
        public Expression Left;
        public Expression Right;
    }

    class ValueRead : Expression
    {
        public string Index;
        public override string ToString()
        {
            return "Read[" + Index + "]";
        }
    }

    class ParameterRead : Expression
    {
        public string Name;

        public override string ToString()
        {
            return $"ReadParam[{Name}]";
        }
    }

    class AssignableExpression : Expression
    {
    }

    class ValueAssign : AssignableExpression
    {
        public string Index;
        public Expression Value;
    }

    class LocalValueRead : Expression
    {
        public string Index;
        public override string ToString()
        {
            return "ReadLocal[" + Index + "]";
        }
    }

    class LocalValueAssign : AssignableExpression
    {
        public string Index;
        public Expression Value;
    }

    class ArrayValueRead : Expression
    {
        public string Index;
        public string VarIndex;
        public override string ToString()
        {
            return $"ReadArray[{Index}][{VarIndex}]";
        }
    }
    
    class ArrayAppend : AssignableExpression
    {
        public string Index;
        public Expression Value;
        public override string ToString()
        {
            return $"AppendArray[{Index}][] = [{Value}]";
        }
    }

    class Literal : Expression
    {
        public string Value;
    }
}