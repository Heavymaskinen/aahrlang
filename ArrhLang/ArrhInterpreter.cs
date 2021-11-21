using System;
using System.IO;

namespace ArrhLang
{
    public class ArrhInterpreter
    {
        public void RunFile(string filename, string[] args = null)
        {
            if (!File.Exists(filename))
            {
                throw new Exception("Can't do it, mate! " + filename);
            }

            try
            {
                var code = File.ReadAllText(filename);
                var parser = new Parser();
                var program = parser.ParseIt(code);
                program.GetFunction(666)(args);
                Console.WriteLine("");
            }
            catch (Exception e)
            {
                Console.WriteLine("Arrh intepretation terminated with fault: " + e.Message+"\n");
            }
        }
    }
}