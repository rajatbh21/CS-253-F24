using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SevenProg
{
    class Seven
    {
        static int Main(string[] args)
        {
            // Check if user input file path, else exit
            if (args.Length == 0)
            {
                Console.WriteLine("Please enter input text file");
                return 1;
            }

            // Check if file exists, else exit
            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Could not find file {0}", args[0]);
                return 1;
            }

            var stop_words = File.ReadAllText("../stop_words.txt").Split(",").ToList();
            Regex.Replace(File.ReadAllText(args[0]), "[^a-zA-Z]", " ").ToLower().Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList().RemoveAll(s => (stop_words.Contains(s) || s.Length < 2));
            Console.WriteLine(x);
            return 0;
        }
    }
}

