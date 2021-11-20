using System;
using System.Collections.Generic;

namespace ArrhLang
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Insufficient input");
                return;
            }
            
            new ArrhInterpreter().RunFile(args[0]);
        }
    }
}