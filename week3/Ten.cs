using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace TenProg
{
    class Ten
    {
        private class TFTheOne
        {
            private object value;

            public TFTheOne(object value)
            {
                this.value = value;
            }

            public TFTheOne Bind(Func<object, object> func)
            {
                value = func(value);
                return this;
            }

            public void PrintVal()
            {
                Console.WriteLine(value);
            }
        }

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
          
            words.RemoveAll(s => (stop_words.Contains(s) || s.Length < 2));

            return words;
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

        public static string Top25(Dictionary<string, int> frequency)
        {
            return string.Join("", frequency.Take(25).Select(kvp => kvp.Key + " - " + kvp.Value + "\n").ToArray());
        }

        static int Main(string[] args)
        {
            
            if (args.Length == 0)
            {
                Console.WriteLine("Enter input file");
                return 1;
            }

            
            if (!File.Exists(args[0]))
            {
                Console.WriteLine("No file found {0}", args[0]);
                return 1;
            }

            new TFTheOne(args[0])
                .Bind(o => ReadFile(o as string))
                .Bind(o => FilterCharsAndNormalize(o as string))
                .Bind(o => Scan(o as string))
                .Bind(o => RemoveStopWords(o as List<string>))
                .Bind(o => Frequencies(o as List<string>))
                .Bind(o => Sort(o as Dictionary<string, int>))
                .Bind(o => Top25(o as Dictionary<string, int>))
                .PrintVal();

            return 0;
        }

    }
}
