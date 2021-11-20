using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Seashells
{
    class Program
    {
        static void Mains(string[] args)
        {
            Console.WindowHeight = Console.LargestWindowHeight;
            Console.WindowWidth = Console.LargestWindowWidth;
            Console.WindowLeft = 0;
            Console.WindowTop = 0;
            
            

            var lines = File.ReadAllLines("test.txt");
            var content = string.Join("\n", lines);

            Console.WriteLine(content);
            Console.SetCursorPosition(0,0);

            Console.ReadLine();
        }
    }

    class Editor
    {
        private string[] text;

        public Editor(string[] text)
        {
            this.text = text;
        }

        public void Insert(int line, int pos, string newText)
        {
            text[line] = text[line].Insert(pos, newText);
        }

        public string Get(int line)
        {
            return text[line];
        }
        
        
    }
}