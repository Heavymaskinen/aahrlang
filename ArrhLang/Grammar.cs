using System.Collections.Generic;

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
        public string[] Parameters;
        public List<Expression> Expressions;
    }

    class Expression
    {
    }

    class IfExpression : Expression
    {
        public Expression Clause;
        public List<Expression> Expressions = new List<Expression>();
    }

    class BoolExpression : Expression
    {
        public Expression Left;
        public Expression Right;
        public string Sign;
    }

    class FunctionExpression : Expression
    {
        public List<Expression> Parameters;
    }

    class FunctionCall : FunctionExpression
    {
        public int FunctionIndex;
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

    class AssignExpression : Expression
    {
        public AssignableExpression Left;
        public Expression Right;
    }

    class ValueRead : Expression
    {
        public int Index;
    }

    class ParameterRead : Expression
    {
        public string Name;
    }

    class AssignableExpression : Expression { }

    class ValueAssign : AssignableExpression
    {
        public int Index;
        public Expression Value;
    }

    class LocalValueRead : Expression
    {
        public int Index;
    }

    class LocalValueAssign : AssignableExpression
    {
        public int Index;
        public Expression Value;
    }

    class Literal : Expression
    {
        public string Value;
    }
}