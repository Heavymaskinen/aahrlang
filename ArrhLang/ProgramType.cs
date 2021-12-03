using System;
using System.Collections.Generic;

namespace ArrhLang
{
    public abstract class ProgramType
    {
        protected Dictionary<string, Func<string>> data = new();
        protected Dictionary<string, Func<string[]>> arrayData = new();
        protected Dictionary<string, Func<string>> locals = new();
        protected Stack<Dictionary<string, string>> scopedStack = new();
        protected Dictionary<string, string> scoped = new();
        protected Dictionary<string, Func<string[], string>> functions = new();
        protected Dictionary<string, ProgramType> objects = new();
        public FunctionCatalog FunctionCatalog;
        
        protected Dictionary<string, Func<Func<object>[], string>> builtIn = new()
        {
            {
                "out",
                (s) =>
                {
                    Console.Write(s.Length > 0 ? s[0]() : "");
                    return null;
                }
            },
            {
                "size",
                s =>
                {
                    if (s.Length == 0)
                    {
                        throw new Exception("Not args for size!");
                    }

                    var val = s[0]();
                    Console.WriteLine("Get array size");
                    if (val is string[] valArr)
                    {
                        return valArr.Length.ToString();
                    }

                    throw new Exception("Wrong for size " + val);
                }
            }
        };

        public void AddObject(string index, ProgramType obj)
        {
            objects.Add(index, obj);
        }
        
        public string GetData(string index) => data[index]();
        public string GetData(int index) => data[index + ""]();
        public string[] GetArrayData(int index) => arrayData[index + ""]();
        public string[] GetArrayData(string index) => arrayData[index]();

        public Func<string> GetFromScope(string name) => () => scoped[name];
        public Func<string[]> GetArray(string index) => arrayData[index];
        public string GetLocalData(string index) => locals[index]();
        public Func<string> GetLocal(string index) => locals[index];
        public void SetData(string index, string value) => data[index] = () => value;
        public void SetArrayData(string index, string[] value) => arrayData[index] = () => value;
        public void SetFunction(string index, Func<string[], string> function) => functions[index] = function;

        public Func<string[], string> GetFunction(string index) => functions[index];
        public Func<string[], string> GetFunction(int index) => functions[index + ""];
        
        public Func<Func<object>[], string> GetBuiltIn(string name)
        {
            return builtIn[name];
        }
        
        protected ProgramType()
        {
            NewScope();
            FunctionCatalog = new FunctionCatalog(this);
        }

        public Dictionary<string, string> NewScope()
        {
            scoped = new();
            scopedStack.Push(scoped);
            return scoped;
        }

        public void CloseScope()
        {
            scoped = scopedStack.Pop();
        }

        public void AddToScope(string name, string value)
            => scoped.Add(name, value);

        public void SetLocal(string localAssIndex, Func<string> localAssValue)
        {
            locals[localAssIndex] = localAssValue;
        }

        public void SetEntry(string index, Func<string> val)
        {
            data[index] = val;
        }

        public ProgramType GetObject(string index)
        {
            return objects[index];
        }

        public bool HasObject(string index)
        {
            return objects.ContainsKey(index);
        }
    }
}