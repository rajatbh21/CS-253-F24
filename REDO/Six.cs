using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
namespace SixProg
{
    class Six
    {
        public static string ReadFile(string filePath)
        {
            return File.ReadAllText(filePath);
        }
        public static string FilterCharsAndNormalize(string data)
        {
            
            return Regex.Replace(data, "[^a-zA-Z]", " ").ToLower();
        }
        public static List<string> Scan(string data)
        {
            return data.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
        }
        public static List<string> RemoveStopWords(List<string> words)
        {
            var stop_words = File.ReadAllText("../stop_words.txt").Split(",").ToList();
            
            var temp = new List<string>(words);
            
            temp.RemoveAll(s => (stop_words.Contains(s) || s.Length < 2));
            return temp;
        }
        public static Dictionary<string, int> Frequencies(List<string> words)
        {
            var frequency = new Dictionary<string, int>();
            foreach (string word in words)
            {
                if (frequency.ContainsKey(word))
                {
                    frequency[word]++;
                }
                else
                {
                    frequency.Add(word, 1);
                }
            }
            return frequency;
        }
        public static Dictionary<string, int> Sort(Dictionary<string, int> frequency)
        {
            
            return frequency.OrderByDescending(a => a.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
        public static void PrintWordFrequencies(Dictionary<string, int> sortedFrequencies)
        {
            foreach (var kvp in sortedFrequencies.Take(25))
            {
                Console.WriteLine("{0} - {1}", kvp.Key, kvp.Value);
            }
        }
        static int Main(string[] args)
        {
           
            if (args.Length == 0)
            {
                Console.WriteLine("Please enter input text file");
                return 1;
            }
            
            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Could not find file {0}", args[0]);
                return 1;
            }
            
            var sortedFrequencies = Sort(Frequencies(RemoveStopWords(Scan(FilterCharsAndNormalize(ReadFile(args[0]))))));
            PrintWordFrequencies(sortedFrequencies);
            return 0;
        }
    }
}