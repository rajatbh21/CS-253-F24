using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace WordFrequency
{
    class DataStorageManager
    {
        private string _data = "";

        public object Dispatch(string[] message) =>
            message[0] == "init" ? Init(message[1]) : (message[0] == "words" ? Words() : throw new ArgumentException("Message not understood"));

        private object Init(string filePath)
        {
            _data = Regex.Replace(File.ReadAllText(filePath), "[^a-zA-Z]", " ").ToLower();
            return null;
        }

        private List<string> Words() => _data.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    class StopWordManager
    {
        private HashSet<string> _stopWords = new();

        public object Dispatch(string[] message) =>
            message[0] == "init" ? Init() : (message[0] == "is_stop_word" ? IsStopWord(message[1]) : throw new ArgumentException("Message not understood"));

        private object Init()
        {
            _stopWords = File.ReadAllText("../stop_words.txt").Split(",").ToHashSet();
            return null;
        }

        private bool IsStopWord(string word) => _stopWords.Contains(word) || word.Length < 2;
    }

    class WordFrequencyManager
    {
        private Dictionary<string, int> _wordFreqs = new();

        public object Dispatch(string[] message) =>
            message[0] == "increment_count" ? IncrementCount(message[1]) : (message[0] == "sorted" ? Sorted() : throw new ArgumentException("Message not understood"));

        private object IncrementCount(string word)
        {
            _wordFreqs[word] = _wordFreqs.GetValueOrDefault(word, 0) + 1;
            return null;
        }

        private Dictionary<string, int> Sorted() => _wordFreqs.OrderByDescending(kvp => kvp.Value).Take(25).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    class WordFrequencyController
    {
        private readonly DataStorageManager _storageManager = new();
        private readonly StopWordManager _stopWordManager = new();
        private readonly WordFrequencyManager _wordFrequencyManager = new();

        public object Dispatch(string[] message) =>
            message[0] == "init" ? Init(message[1]) : (message[0] == "run" ? Run() : throw new ArgumentException("Message not understood"));

        private object Init(string filePath)
        {
            _storageManager.Dispatch(new[] { "init", filePath });
            _stopWordManager.Dispatch(new[] { "init" });
            return null;
        }

        private object Run()
        {
            foreach (var w in (List<string>)_storageManager.Dispatch(new[] { "words" }))
                if (!(bool)_stopWordManager.Dispatch(new[] { "is_stop_word", w }))
                    _wordFrequencyManager.Dispatch(new[] { "increment_count", w });

            foreach (var kvp in (Dictionary<string, int>)_wordFrequencyManager.Dispatch(new[] { "sorted" }))
                Console.WriteLine($"{kvp.Key} - {kvp.Value}");
            
            return null;
        }
    }

    class TwelveProg
    {
        static void Main(string[] args)
        {
            if (args.Length == 0 || !File.Exists(args[0]))
            {
                Console.WriteLine("Please provide a valid input file.");
                return;
            }

            var controller = new WordFrequencyController();
            controller.Dispatch(new[] { "init", args[0] });
            controller.Dispatch(new[] { "run" });
        }
    }
}
