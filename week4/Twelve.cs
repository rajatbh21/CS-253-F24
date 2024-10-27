using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Twelve
{
    class DataStorageManager
    {
        private string _data;

        public DataStorageManager()
        {
            _data = "";
        }

        private object Init(string filePath)
        {
            _data = Regex.Replace(File.ReadAllText(filePath), "[^a-zA-Z]", " ").ToLower();
            return null;
        }

        private List<string> Words()
        {
            return _data.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public object Dispatch(string[] message)
        {
            if (message[0].Equals("init"))
            {
                return Init(message[1]);
            }
            else if (message[0].Equals("words"))
            {
                return Words();
            }
            else
            {
                throw new ArgumentException("Message not understood: {0}", message[0]);
            }
        }
    }

    class StopWordManager
    {
        private List<string> _stopWords;

        public StopWordManager()
        {
            _stopWords = new List<string>();
        }

        private object Init()
        {
            _stopWords = File.ReadAllText("../stop_words.txt").Split(",").ToList();
            return null;
        } 

        private bool IsStopWord(string word)
        {
            return _stopWords.Contains(word) || word.Length < 2;
        }

        public object Dispatch(string[] message)
        {
            if (message[0].Equals("init"))
            {
                return Init();
            }
            else if (message[0].Equals("is_stop_word"))
            {
                return IsStopWord(message[1]);
            }
            else
            {
                throw new ArgumentException("Message not understood: {0}", message[0]);
            }
        }
    }

    class WordFrequencyManager
    {
        private Dictionary<string, int> _wordFrequencies;

        public WordFrequencyManager()
        {
            _wordFrequencies = new Dictionary<string, int>();
        }

        private object IncrementCount(string word)
        {
            if (_wordFrequencies.ContainsKey(word))
            {
                _wordFrequencies[word]++;
            }
            else
            {
                _wordFrequencies.Add(word, 1);
            }
            return null;
        }

        private Dictionary<string, int> Sorted()
        {
            return _wordFrequencies.OrderByDescending(a => a.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public object Dispatch(string[] message)
        {
            if (message[0].Equals("increment_count"))
            {
                return IncrementCount(message[1]);
            }
            else if (message[0].Equals("sorted"))
            {
                return Sorted();
            }
            else
            {
                throw new ArgumentException("Message not understood: {0}", message[0]);
            }
        }
    }

    class WordFrequencyController
    {
        private DataStorageManager _storageManager;
        private StopWordManager _stopWordManager;
        private WordFrequencyManager _wordFrequencyManager;

        public WordFrequencyController()
        {
            _storageManager = new DataStorageManager();
            _stopWordManager = new StopWordManager();
            _wordFrequencyManager = new WordFrequencyManager();
        }

        private object Init(string filePath)
        {
            _storageManager.Dispatch(new string[] { "init", filePath });
            _stopWordManager.Dispatch(new string[] { "init" });
            return null;
        }

        private object Run()
        {
            foreach (var w in (List<string>)_storageManager.Dispatch(new string[] { "words" }))
            {
                if (!(bool)_stopWordManager.Dispatch(new string[] { "is_stop_word", w }))
                {
                    _wordFrequencyManager.Dispatch(new string[] { "increment_count", w });
                }
            }

            var wordFrequencies = _wordFrequencyManager.Dispatch(new string[] { "sorted" });
            foreach (var kvp in ((Dictionary<string, int>)wordFrequencies).Take(25))
            {
                Console.WriteLine("{0} - {1}", kvp.Key, kvp.Value);
            }

            return null;
        }

        public object Dispatch(string[] message)
        {
            if (message[0].Equals("init"))
            {
                return Init(message[1]);
            }
            else if (message[0].Equals("run"))
            {
                return Run();
            }
            else
            {
                throw new ArgumentException("Message not understood: {0}", message[0]);
            }
        }
    }

    class TwelveProg
    {
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

            var wordFrequencyController = new WordFrequencyController();
            wordFrequencyController.Dispatch(new string[] { "init", args[0] });
            wordFrequencyController.Dispatch(new string[] { "run" });

            return 0;
        }
    }
}
