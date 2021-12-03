using System;
using System.Collections.Generic;

namespace ArrhLang
{
    public class ScannedProgram
    {
        public List<Entry> entries = new();
        public List<ScannedProgram> objects = new();
        public List<FunctionRoot> functions = new();
        public Dictionary<string, string> constants = new();
        public string Index { get; set; }

        public EntryRoot DataValue(int index) => entries[index] as EntryRoot;
    }

    public class Scanner
    {
        private Stack<ScannedProgram> scanStack = new();

        private ScannedProgram Scanned => scanStack.Peek();
        
        public ScannedProgram Parse(string code)
        {
            scanStack.Push(new ScannedProgram());
            
            Scanned.constants.Add("MAIN", "666");
            var lines = code.Split("\n", StringSplitOptions.TrimEntries);
            started = false;
            for (var i = 0; i < lines.Length; i++)
            {
                if (ScanLine(lines, ref i))
                    continue;

                break;
            }
            Console.WriteLine("End of scan\n");

            return Scanned;
        }

        private static bool started = false;
        
        private bool ScanLine(string[] lines, ref int i)
        {
            var currentLine = lines[i];
            
            Console.WriteLine("Scan "+currentLine);
            if (currentLine.StartsWith(Parser.CommentToken) || string.IsNullOrEmpty(currentLine)) return true;
            if (currentLine.StartsWith(Parser.BlockCommentToken))
            {
                var commentLines = Utils.TakeUntilMatch(i, lines, Parser.BlockCommentToken);
                i += commentLines.Count;
                return true;
            }

            if (currentLine.StartsWith("def"))
            {
                if (started) throw new Exception("Wrong def");
                var parts = currentLine.Split(" ");
                Console.WriteLine("Constant "+parts[1]+" = "+parts[2]);
                Scanned.constants.Add(parts[1], parts[2]);
                return true;
            }

            currentLine = ReplaceConstants(currentLine);
            lines[i] = currentLine;

            if (currentLine.EndsWith("=> ["))
            {
                var parts = currentLine.Split("=>", StringSplitOptions.TrimEntries);
                var index = parts[0];
                Console.WriteLine("Start new object!");
                var objProgram = new ScannedProgram { Index = index };
                scanStack.Push(objProgram);
                return true;
            }

            if (scanStack.Count > 1 && currentLine == "]")
            {
                var finished = scanStack.Pop();
                Console.WriteLine("Finished object!");
                Scanned.objects.Add(finished);
                return true;
            }

            switch (currentLine)
            {
                case "[":  //Program start
                    started = true;
                    return true;
                case "]":  //Program end
                    if (!started) throw new Exception("Wrong end");
                    started = false;
                    return false;
            }

            if (currentLine.Contains("{"))
            {
                var funcLines = BuildFunctionLines(lines, i, out var filteredLines);

                i += funcLines.Count;
                Scanned.functions.Add(new FunctionRoot(filteredLines));
            }
            else
            {
                Scanned.entries.Add(EntryFactory.Create(currentLine));
            }

            return true;
        }

        private List<string> BuildFunctionLines(string[] lines, int i, out List<string> filteredLines)
        {
            var funcLines = Utils.TakeUntilMatch(i, lines, "}");
            filteredLines = new List<string>();
            for (var x = 0; x < funcLines.Count; x++)
            {
                var fline = funcLines[x].Trim();
                if (fline.StartsWith(Parser.CommentToken)) continue;
                if (fline.StartsWith(Parser.BlockCommentToken))
                {
                    x++;
                    var commentLines = Utils.TakeUntilMatch(x, funcLines.ToArray(), Parser.BlockCommentToken);
                    x += commentLines.Count + 1;
                }

                if (x < funcLines.Count)
                {
                    var approvedLine = funcLines[x].Trim();

                    approvedLine = ReplaceConstants(approvedLine);
                    filteredLines.Add(approvedLine);
                }
            }

            return funcLines;
        }

        private string ReplaceConstants(string line)
        {
            foreach (var set in Scanned.constants)
            {
                if (line.Contains(set.Key))
                {
                    line = line.Replace(set.Key, set.Value);
                    break;
                }
            }

            return line;
        }
    }

    public class FunctionRoot
    {
        public string Index;
        public string[] ParameterNames;
        public List<Token> TokenLines;

        public FunctionRoot(List<string> lines)
        {
            var first = lines[0];
            var parts = first.Split("=>", StringSplitOptions.TrimEntries);
            Index = parts[0];
            ParseParameterNames(parts);
            lines.RemoveAt(0);
            Console.WriteLine($"FuncDef[{Index}]({String.Join(",",ParameterNames)})");

            TokenLines = new List<Token>();
            for (var i=0;i<lines.Count;i++)
            {
                var line = lines[i];
                Token token;
                Console.WriteLine($"FuncLineScan[{Index}] "+line);
                token = StatementFactory.Create(line);

                if (token is IfStatement ifTok)
                {
                    i++;
                    while (i < lines.Count)
                    {
                        var inner = lines[i];
                        var innerTok = StatementFactory.Create(inner);
                        if (innerTok is SectionClose)
                        {
                            break;
                        }
                        
                        ifTok.body.Add(innerTok);
                        i++;
                    }
                }
                
                if (token is ForStatement forTok)
                {
                    i++;
                    while (i < lines.Count)
                    {
                        var inner = lines[i];
                        var innerTok = StatementFactory.Create(inner);
                        if (innerTok is SectionClose)
                        {
                            break;
                        }
                        
                        forTok.body.Add(innerTok);
                        i++;
                    }
                }

                Console.WriteLine("funcline token: "+token);
                TokenLines.Add(token);
            }
        }

        private void ParseParameterNames(string[] parts)
        {
            var inner = Utils.GetInner(parts[1], '(', ')');
            ParameterNames = inner.Split(",");
        }
    }

    
}