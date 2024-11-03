using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Style25
{
    // 25.1: Split program - pure and IO
    public class TFQuarantine
    {
        private readonly List<Func<object, object>> _funcs = new List<Func<object, object>>();

        public TFQuarantine(Func<object, object> func)
        {
            _funcs.Add(func);
        }

        public TFQuarantine Bind(Func<object, object> func)
        {
            _funcs.Add(func);
            return this;
        }

        public void Execute()
        {
            object value = null;
            foreach (var func in _funcs)
            {
                value = func(value);
            }
            // 25.1: IO in sqnce, separated from pure functions
            Console.WriteLine(value);
        }
    }

    public class WordFrequencyAnalyzer
    {
        // 25.1: Pure func - No side effects
        private static object FilterCharsAndNormalize(object input)
        {
            var text = (string)input;
            var regex = new Regex("[^a-zA-Z]", RegexOptions.Compiled);
            return regex.Replace(text, " ").ToLower();
        }

        private static object ScanWords(object input)
        {
            var text = (string)input;
            return text.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        private static object RemoveStopWords(object input)
        {
            var words = (List<string>)input;
            // 25.1: Stop words load-> IO side
            return words.Where(w => w.Length >= 2 && !Program.StopWords.Contains(w)).ToList();
        }

        private static object ComputeFrequencies(object input)
        {
            var words = (List<string>)input;
            return words.GroupBy(w => w)
                       .ToDictionary(g => g.Key, g => g.Count());
        }

        private static object SortByFrequency(object input)
        {
            var freq = (Dictionary<string, int>)input;
            return freq.OrderByDescending(kvp => kvp.Value)
                      .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        private static object FormatTop25(object input)
        {
            var freq = (Dictionary<string, int>)input;
            return string.Join("\n", 
                freq.Take(25)
                    .Select(kvp => $"{kvp.Key} - {kvp.Value}"));
        }

        // 25.1: IO -> computation sequences
        public static class IO
        {
            public static string ReadInput(string path)
            {
                return File.ReadAllText(path);
            }

            public static HashSet<string> LoadStopWords(string path)
            {
                return new HashSet<string>(
                    File.ReadAllText(path).Split(","));
            }
        }

        public static void Process(string inputPath, string stopWordsPath)
        {
            // 25.1: Call all IO
            Program.StopWords = IO.LoadStopWords(stopWordsPath);
            var inputText = IO.ReadInput(inputPath);

            new TFQuarantine((_) => inputText)
                .Bind(FilterCharsAndNormalize)
                .Bind(ScanWords)
                .Bind(RemoveStopWords)
                .Bind(ComputeFrequencies)
                .Bind(SortByFrequency)
                .Bind(FormatTop25)
                .Execute();
        }
    }

    public class Program
    {
        // 25.1: Shared state for stop words loaded once
        public static HashSet<string> StopWords { get; set; }

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

            WordFrequencyAnalyzer.Process(args[0], "../stop_words.txt");
            return 0;
        }
    }
}