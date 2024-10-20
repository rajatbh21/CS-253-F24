using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace EightProg
{
    class Eight
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

        // Recursive frequency counting function
        public static void CountFrequencies(List<string> words, Dictionary<string, int> frequency)
        {
            // Base case
            if (words.Count == 0)
                return;

            
            string word = words[0];
            if (frequency.ContainsKey(word))
            {
                frequency[word]++;
            }
            else
            {
                frequency.Add(word, 1);
            }

            
            CountFrequencies(words.Skip(1).ToList(), frequency);
        }

        
        public static Dictionary<string, int> Sort(Dictionary<string, int> frequency)
        {
            
            return frequency.OrderByDescending(a => a.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        
        public static void PrintFrequencies(List<KeyValuePair<string, int>> sortedFrequencies, int index)
        {
            
            if (index >= sortedFrequencies.Count || index >= 25)
                return;

            
            var kvp = sortedFrequencies[index];
            Console.WriteLine("{0} - {1}", kvp.Key, kvp.Value);

            
            PrintFrequencies(sortedFrequencies, index + 1);
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

            
            string data = ReadFile(args[0]);
            string normalizedData = FilterCharsAndNormalize(data);
            List<string> words = Scan(normalizedData);
            words = RemoveStopWords(words);

            
            var wordFreqs = new Dictionary<string, int>();
            CountFrequencies(words, wordFreqs);

            
            var sortedFreqs = Sort(wordFreqs).ToList();

            
            PrintFrequencies(sortedFreqs, 0);

            return 0;
        }
    }
}
