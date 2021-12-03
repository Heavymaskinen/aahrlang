using System;
using System.Collections.Generic;

namespace ArrhLang
{
    public class Parser
    {
        public static string BlockCommentToken = "||";
        public static string CommentToken = ">>";
        private List<EntryType> entries = new();
        private Dictionary<string, string> constants = new() { { "MAIN", "666" } };

        public ArrhProgram ScanAndParse(string code)
        {
            var program = new Scanner().Parse(code);
            var builder = new ExpressionTreeBuilder();
            entries = builder.Parse(program);
            constants = program.constants;
            var creator = new FunctionCreator();
            foreach (var exp in entries)
            {
                Console.WriteLine("Exp:"+exp);
                creator.AddEntryFunctionsToProgram(exp);
            }

            return creator.Program;
        }
    }
}