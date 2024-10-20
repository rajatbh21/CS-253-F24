using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace NineProg
{
    class Nine
    {
        private static HashSet<string> _stopWords;

        public static void ReadFile(string filePath, Action<string, Action<string, Action<List<string>, Action<List<string>, Action<Dictionary<string, int>, Action<Dictionary<string, int>, Action>>>>>> func)
        {
            var data = File.ReadAllText(filePath);
            func(data, Scan);
        }

        public static void FilterCharsAndNormalize(string data, Action<string, Action<List<string>, Action<List<string>, Action<Dictionary<string, int>, Action<Dictionary<string, int>, Action>>>>> func)
        {
            
            var regex = new Regex("[^a-zA-Z]", RegexOptions.Compiled);
            var normalizedData = regex.Replace(data, " ").ToLower();
            func(normalizedData, RemoveStopWords);
        }

        public static void Scan(string data, Action<List<string>, Action<List<string>, Action<Dictionary<string, int>, Action<Dictionary<string, int>, Action>>>> func)
        {
            var words = data.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
            func(words, Frequencies);
        }

        public static void RemoveStopWords(List<string> words, Action<List<string>, Action<Dictionary<string, int>, Action<Dictionary<string, int>, Action>>> func)
        {
           
            if (_stopWords == null)
            {
                _stopWords = new HashSet<string>(File.ReadAllText("../stop_words.txt").Split(","));
            }

            
            words.RemoveAll(word => _stopWords.Contains(word) || word.Length < 2);
            func(words, Sort);
        }

        public static void Frequencies(List<string> words, Action<Dictionary<string, int>, Action<Dictionary<string, int>, Action>> func)
        {
            var frequency = new Dictionary<string, int>();
            foreach (var word in words)
            {
                if (frequency.TryGetValue(word, out var count))
                {
                    frequency[word] = count + 1;
                }
                else
                {
                    frequency[word] = 1;
                }
            }
            func(frequency, PrintFrequencies);
        }

        public static void NoOp()
        {
            
        }


        public static void Sort(Dictionary<string, int> frequency, Action<Dictionary<string, int>, Action> func)
        {
          
            func(frequency.OrderByDescending(kvp => kvp.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value), NoOp);
        }

        public static void PrintFrequencies(Dictionary<string, int> frequency, Action func)
        {
            foreach (var kvp in frequency.Take(25))
            {
                Console.WriteLine("{0} - {1}", kvp.Key, kvp.Value);
            }
            func();
        }


        static int Main(string[] args)
        {   if (args.Length == 0)
            {
                Console.WriteLine("Enter input file");
                return 1;
            }

            
            if (!File.Exists(args[0]))
            {
                Console.WriteLine("No file found {0}", args[0]);
                return 1;
            }
            
            ReadFile(args[0], FilterCharsAndNormalize);

            return 0;
        }
    }
}

