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

            var trimArgs = new string[args.Length-1];
            for (var i=1;i<args.Length;i++)
            {
                trimArgs[i - 1] = args[i];
            }

            new ArrhInterpreter().RunFile(args[0], trimArgs);
        }
    }
}