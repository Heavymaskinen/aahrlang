using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;

namespace ArrhLang
{
    class FunctionCreator
    {
        private ProgramType program2;
        private FunctionCatalog functionCatalog => CurrentProgram.FunctionCatalog;
        private Stack<ProgramType> programStack;
        private ProgramType CurrentProgram => programStack.Peek();
        
        public FunctionCreator()
        {
            programStack = new();
            program2 = new ArrhProgram();
            programStack.Push(program2);
        }

        public ArrhProgram Program => (ArrhProgram)program2;

        public void AddEntryFunctionsToProgram(EntryType entry)
        {
            if (entry is ValueType valEntry)
            {
                CurrentProgram.SetData(entry.Index, valEntry.Value);
            }
            else if (entry is ArrayType arrEntry)
            {
                CurrentProgram.SetArrayData(arrEntry.Index, arrEntry.Value);
            }
            else if (entry is FunctionType funcEntry)
            {
                AddFunctionTypeToProgram(funcEntry);
            }
            else if (entry is ObjectEntryType objEntry)
            {
                var obj = new ObjectProgram();
                programStack.Push(obj);
                foreach (var oEntry in objEntry.Entries)
                {
                    AddEntryFunctionsToProgram(oEntry);
                }

                programStack.Pop();
                CurrentProgram.AddObject(objEntry.Index, obj);
            }
            else
            {
                throw new Exception(entry.GetType() + " makes no sense!");
            }
            
            Console.WriteLine("End function creation");
        }

        private void AddFunctionTypeToProgram(FunctionType entry)
        {
            var funcParts = BuildFunctionContent(entry.Expressions);
            Console.WriteLine("Add func "+entry);
            var func = functionCatalog.FullFunc(funcParts, entry.Parameters);
            CurrentProgram.SetFunction(entry.Index, func);
        }

        private List<Func<string>> BuildFunctionContent(List<Expression> statements)
        {
            var funcParts = new List<Func<string>>();
            foreach (var stmt in statements)
            {
                Console.WriteLine("stmt:"+stmt);
                
                var exFunc = ParseExpressionToFunction(stmt);
                funcParts.Add(exFunc);
            }

            return funcParts;
        }

        private Func<string> ParseExpressionToFunction(Expression stmt)
        {
            if (stmt is ValueRead read)
            {
                return Visit(read);
            }

            if (stmt is ArrayValueRead arrRead)
            {
                return Visit(arrRead);
            }
            
            if (stmt is LocalValueRead localRead)
            {
                return Visit(localRead);
            }
            
            if (stmt is ParameterRead param)
            {
                return Visit(param);
            }
            
            if (stmt is LocalValueAssign localAss)
            {
                return Visit(localAss);
            }
            
            if (stmt is ValueAssign ass)
            {
                return Visit(ass);
            }

            if (stmt is ArrayAppend app)
            {
                return Visit(app);
            }
            
            if (stmt is FunctionCall call)
            {
                return Visit(call);
            }
            
            if (stmt is BuiltInFunctionCall builtIn)
            {
                return Visit(builtIn);
            }
            
            if (stmt is Literal literal)
            {
                return Visit(literal);
            }
            
            if (stmt is SumExpression sum)
            {
                return Visit(sum);
            }
            
            if (stmt is AppendExpression append)
            {
                return Visit(append);
            }
            
            if (stmt is BoolExpression boolExp)
            {
                return Visit(boolExp);
            }
            
            if (stmt is IfExpression ifExp)
            {
                return Visit(ifExp);
            }
            
            if (stmt is ForExpression forExp)
            {
                return Visit(forExp);
            }

            if (stmt is Operator opExp)
            {
                return Visit(opExp);
            }

            if (stmt is ReferenceExpression)
            {
                throw new Exception("Reference not allowed here");
            }

            throw new Exception($"Expression {stmt} doesn't exist");
        }

        private Func<string[]> Visit(ReferenceExpression localAss)
        {
            return functionCatalog.ReferenceFunc(localAss.Index);
        }

        private Func<string> Visit(Operator opExp)
        {
            var left = ParseExpressionToFunction(opExp.Left);
            var right = ParseExpressionToFunction(opExp.Right);
            return functionCatalog.CalcFunc(left, right, opExp.Sign);
        }

        private Func<string> Visit(ValueRead read)
        {
            return functionCatalog.ValueReadFunc(read.Index);
        }

        private Func<string> Visit(ArrayValueRead arrRead)
        {
            return functionCatalog.ArrReadFunc(arrRead.Index, arrRead.VarIndex);
        }
    
        private Func<string> Visit(ArrayAppend append)
        {
            return functionCatalog.ArrAppendFunc(append.Index, ParseExpressionToFunction( append.Value));
        }

        private Func<string> Visit(LocalValueRead read)
        {
            return functionCatalog.LocalReadFunc(read.Index);
        }
        private Func<string> Visit(ParameterRead read)
        {
            return functionCatalog.ScopedReadFunc(read.Name);
        }

        private Func<string> Visit(ForExpression forExp)
        {
            var clause = ParseExpressionToFunction(forExp.Clause);
            var increment = ParseExpressionToFunction(forExp.Increment);
            var inner = new List<Func<string>>();
            foreach (var exp in forExp.Expressions)
            {
                inner.Add(ParseExpressionToFunction(exp));
            }

            var func = functionCatalog.ForFunc(clause, increment, inner);
            return func;
        }

        private Func<string> Visit(IfExpression ifExp)
        {
            var clause = ParseExpressionToFunction(ifExp.Clause);
            var inner = new List<Func<string>>();
            foreach (var exp in ifExp.Expressions)
            {
                inner.Add(ParseExpressionToFunction(exp));
            }

            var func = functionCatalog.IfFunc(clause, inner);
            return func;
        }

        private Func<string> Visit(BoolExpression boolExp)
        {
            var left = ParseExpressionToFunction(boolExp.Left);
            var right = ParseExpressionToFunction(boolExp.Right);
            var func = functionCatalog.BoolFunc(left, right, boolExp.Sign);
            return func;
        }

        private Func<string> Visit(AppendExpression append)
        {
            var left = ParseExpressionToFunction(append.Left);
            var right = ParseExpressionToFunction(append.Right);

            var func = functionCatalog.AppendFunc(left, right);
            return func;
        }

        private Func<string> Visit(SumExpression sum)
        {
            var left = ParseExpressionToFunction(sum.Left);
            var right = ParseExpressionToFunction(sum.Right);

            var func = functionCatalog.SumFunc(left, right);
            return func;
        }

        private Func<string> Visit(Literal literal)
        {
            var func = functionCatalog.ValueFunc(literal.Value);
            return func;
        }

        private Func<string> Visit(BuiltInFunctionCall builtIn)
        {
            var parFuncs = CreateParameterExpressions(builtIn);
            var func = functionCatalog.InternalCallFunc(builtIn.Name, parFuncs.ToArray());
            return func;
        }

        private Func<string> Visit(FunctionCall call)
        {
            var parFuncs = CreateParameterExpressions(call);
            var func = functionCatalog.FunctionCallFunc(call.FunctionIndex, parFuncs.ToArray());
            return func;
        }

        private Func<string> Visit(ValueAssign ass)
        {
            Func<string> func;
            var val = ParseExpressionToFunction(ass.Value);
            func = functionCatalog.AssignFunc(ass.Index, val);
            return func;
        }

        private Func<string> Visit(LocalValueAssign localAss)
        {
            var val = ParseExpressionToFunction(localAss.Value);
            return functionCatalog.LocalAssignFunc(localAss.Index, val);
        }

        private List<Func<object>> CreateParameterExpressions(FunctionExpression call)
        {
            var parFuncs = new List<Func<object>>();
            foreach (var param in call.Parameters)
            {
                if (param is ReferenceExpression refPar)
                {
                    parFuncs.Add(Visit(refPar));
                }
                else
                {
                    parFuncs.Add(ParseExpressionToFunction(param));
                }
            }

            return parFuncs;
        }
    }
}