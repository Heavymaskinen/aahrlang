using System;

namespace ArrhLang
{
    public class EntryFactory
    {
        public static Entry Create(string line)
        {
            if (line.Contains("[") && line.EndsWith("]"))
            {
                return new ArrayEntry(line);
            }
            return new EntryRoot(line);
        }
    }

    public abstract class Entry
    {
        public string Index;
    }

    public class ArrayEntry : Entry
    {
        public string[] Value; 

        public ArrayEntry(string line)
        {
            Console.WriteLine("EntryArray Scan: "+line);
            
            var parts = line.Split("=>", StringSplitOptions.TrimEntries);
            Index = parts[0];
            var last = parts[1];
            last = Utils.GetInner(last, '[', ']');
            Value = last.Split(',', StringSplitOptions.TrimEntries);
        }
    }

    public class EntryRoot : Entry
    {
        
        public string Value;

        public EntryRoot(string line)
        {
            Console.WriteLine("EntryScan: "+line);
            var parts = line.Split("=>", StringSplitOptions.TrimEntries);
            Index = parts[0];
            Value = parts[1];
            Console.WriteLine($"Entry [{Index}] => {Value}");
        }
    }
}