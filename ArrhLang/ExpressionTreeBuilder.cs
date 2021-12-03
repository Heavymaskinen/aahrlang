using System;
using System.Collections.Generic;

namespace ArrhLang
{
    public class ExpressionTreeBuilder
    {
        public List<EntryType> Parse(ScannedProgram program)
        {
            var entries = new List<EntryType>();
            foreach (var en in program.entries)
            {
                AddEntry(en, entries);
            }

            foreach (var fun in program.functions)
            {
                AddFunction(fun, entries);
            }

            foreach (var obj in program.objects)
            {
                AddObject(obj, entries);
            }

            return entries;
        }

        private void AddObject(ScannedProgram objectProgram, List<EntryType> baseEntries)
        {
            var objectEntries = Parse(objectProgram);
            var en = new ObjectEntryType { Entries = objectEntries, Index = objectProgram.Index};
            baseEntries.Add(en);
        }

        private void AddEntry(Entry en, List<EntryType> entries)
        {
            switch (en)
            {
                case EntryRoot root:
                    entries.Add(new ValueType { Index = root.Index, Value = root.Value.Replace("'", "") });
                    break;
                case ArrayEntry arr:
                    entries.Add(new ArrayType { Index = arr.Index, Value = arr.Value });
                    break;
            }
        }

        private void AddFunction(FunctionRoot fun, List<EntryType> entries)
        {
            var expressions = new List<Expression>();
            for (var i = 0; i < fun.TokenLines.Count; i++)
            {
                var line = fun.TokenLines[i];
                var exp = ParseTokenToExpression(line);
                if (exp is IfExpression)
                {
                }

                expressions.Add(exp);
            }

            var func = new FunctionType
                { Expressions = expressions, Index = fun.Index, Parameters = fun.ParameterNames };
            entries.Add(func);
        }

        private Expression ParseTokenToExpression(Token token)
        {
            Console.WriteLine("Parse token: " + token);
            return token switch
            {
                LocalVariable locVar => new LocalValueRead { Index = locVar.Index },
                ArrayEntryVariable arrVar => new ArrayValueRead() { Index = arrVar.Index, VarIndex = arrVar.varIndex },
                EntryVariable vari => new ValueRead { Index = vari.Index },
                Assignment assTok => CreateAssignment(assTok),
                Constant conTok => new Literal { Value = conTok.Value },
                FunctionCallStatement funTok => CreateFunctionCall(funTok),
                BuiltInFunctionStatement builFunTok => CreateBuiltinFunctionCall(builFunTok),
                OperatorStatement opTok => CreateOperatorExpression(opTok),
                Parameter parTok => new ParameterRead { Line = parTok.Line, Name = parTok.Name },
                Append appTok => CreateAppendExpression(appTok),
                IfStatement ifTok => CreateIfExpression(ifTok),
                ForStatement forTok => CreateForExpression(forTok),
                CompareStatement cmpTok => CreateBoolExpression(cmpTok),
                Reference refTok => CreateReferenceExpression(refTok),
                SectionClose => throw new Exception("Whitespace overflow"),
                _ => throw new Exception("Unknown: " + token.Line)
            };
        }

        private Expression CreateReferenceExpression(Reference refTok)
        {
            return new ReferenceExpression(refTok.Index);
        }

        private Expression CreateBoolExpression(CompareStatement cmpTok)
        {
            var left = ParseTokenToExpression(cmpTok._left);
            var right = ParseTokenToExpression(cmpTok._right);
            return new BoolExpression { Left = left, Right = right, Line = cmpTok.Line, Sign = cmpTok._sign };
        }

        private Expression CreateIfExpression(IfStatement ifTok)
        {
            var clause = ParseTokenToExpression(ifTok.clause);
            var body = new List<Expression>();
            foreach (var t in ifTok.body)
            {
                body.Add(ParseTokenToExpression(t));
            }

            return new IfExpression { Clause = clause, Expressions = body };
        }

        private Expression CreateForExpression(ForStatement forTok)
        {
            var clause = ParseTokenToExpression(forTok.clause);
            var body = new List<Expression>();
            var incrementor = ParseTokenToExpression(forTok.incrementor);
            foreach (var t in forTok.body)
            {
                body.Add(ParseTokenToExpression(t));
            }

            return new ForExpression { Clause = clause, Increment = incrementor, Expressions = body };
        }

        private AppendExpression CreateAppendExpression(Append appTok)
        {
            var leftExp = ParseTokenToExpression(appTok.left);
            var rightExp = ParseTokenToExpression(appTok.right);
            return new AppendExpression { Left = leftExp, Right = rightExp, Line = appTok.Line };
        }

        private Expression CreateOperatorExpression(OperatorStatement opTok)
        {
            var left = ParseTokenToExpression(opTok.Left);
            var right = ParseTokenToExpression(opTok.Right);
            return new Operator { Left = left, Right = right, Sign = opTok.Operator, Line = opTok.Line };
        }

        private Expression CreateFunctionCall(FunctionCallStatement funTok)
        {
            var paramExp = new List<Expression>();
            foreach (var par in funTok.ParameterValues)
            {
                paramExp.Add(ParseTokenToExpression(par));
            }

            return new FunctionCall
                { FunctionIndex = funTok.FunctionIndex, Line = funTok.Line, Parameters = paramExp };
        }

        private Expression CreateBuiltinFunctionCall(BuiltInFunctionStatement token)
        {
            var paramExp = new List<Expression>();
            foreach (var par in token.ParameterValues)
            {
                paramExp.Add(ParseTokenToExpression(par));
            }

            return new BuiltInFunctionCall
                { Name = token.name, Line = token.Line, Parameters = paramExp };
        }

        private Expression CreateAssignment(Assignment assTok)
        {
            Console.WriteLine("Assigment exp tree");
            var value = ParseTokenToExpression(assTok.Statement);
            if (assTok.Assignable.IsLocal)
            {
                return new LocalValueAssign { Index = assTok.Assignable.Index, Line = assTok.Line, Value = value };
            }

            if (assTok.Assignable.IsArray)
            {
                return new ArrayAppend { Index = assTok.Assignable.Index, Line = assTok.Line, Value = value };
            }

            return new ValueAssign { Index = assTok.Assignable.Index, Line = assTok.Line, Value = value };
        }
    }
}