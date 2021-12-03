using System;
using System.Collections.Generic;

namespace ArrhLang
{
    public class Utils
    {
        public static string GetInner(string input, char start, char end)
        {
            if (!input.Contains(start) || !input.Contains(end)) return input;
            
            var startIndex = input.IndexOf(start);
            var endindex = input.IndexOf(end) - startIndex;

            if (endindex - startIndex > 0)
            {
                var result = input.Substring(startIndex + 1, endindex - 1).Trim();
                return result;
            }
            
            Console.WriteLine($"Ignored: {input}, start {startIndex}, end {endindex}");

            return "";
        }
        

        public static T[] SelectFrom<T>(int index, T[] source)
        {
            var newList = new List<T>();
            for (var i = index; i < source.Length; i++)
            {
                newList.Add(source[i]);
            }

            return newList.ToArray();
        }
        
        public static List<string> TakeUntilMatch(string[] lines, string match)
        {
            var taken = new List<string>();
            
            foreach (var line in lines)
            {
                if (line.Contains(match)) break;
                
                taken.Add(line);
            }

            return taken;
        }
        
        public static List<string> TakeUntilMatch(int start, string[] lines, string match)
        {
            var taken = new List<string>();
            
            for (var i=start; i<lines.Length;i++)
            {
                var line = lines[i];
                if (line.Contains(match)) break;
                
                taken.Add(line);
            }

            return taken;
        }
    }
}