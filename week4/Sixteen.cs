using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sixteen
{
    class EventManager
    {
        private Dictionary<string, List<Action<string[]>>> _subscriptions = new Dictionary<string, List<Action<string[]>>>();

        public void Subscribe(string eventType, Action<string[]> handler)
        {
            List<Action<string[]>> handlers;
            if (_subscriptions.TryGetValue(eventType, out handlers))
            {
                handlers.Add(handler);
            }
            else
            {
                _subscriptions[eventType] = new List<Action<string[]>> { handler };
            }
        }

        public void Publish(string[] _event)
        {
            List<Action<string[]>> handlers;
            if (_subscriptions.TryGetValue(_event[0], out handlers))
            {
                foreach (var handler in handlers)
                {
                    handler(_event);
                }
            }
        }
    }

    class DataStorage
    {
        private string _data = "";

        public DataStorage(EventManager eventManager)
        {
            eventManager.Subscribe("load", Load);
            eventManager.Subscribe("start", ProduceWords);
        }

        private void Load(string[] _event)
        {
            _data = Regex.Replace(File.ReadAllText(_event[1]), "[^a-zA-Z]", " ").ToLower();
        }

        private void ProduceWords(string[] _)
        {
            var words = _data.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in words)
            {
                SixteenProg.EventBoard.Publish(new[] { "word", word });
            }
        }
    }

    class StopWordFilter
    {
        private HashSet<string> _stopWords;

        public StopWordFilter(EventManager eventManager)
        {
            eventManager.Subscribe("load", Load);
            eventManager.Subscribe("word", IsStopWord);
        }

        private void Load(string[] _)
        {
            _stopWords = new HashSet<string>(File.ReadAllText("../stop_words.txt").Split(','));
        }

        private void IsStopWord(string[] _event)
        {
            if (!_stopWords.Contains(_event[1]) && _event[1].Length > 1)
            {
                SixteenProg.EventBoard.Publish(new[] { "valid_word", _event[1] });
            }
        }
    }

    class WordFrequencyCounter
    {
        private Dictionary<string, int> _wordFreqs = new Dictionary<string, int>();

        public WordFrequencyCounter(EventManager eventManager)
        {
            eventManager.Subscribe("valid_word", IncrementCount);
            eventManager.Subscribe("print", PrintFrequencies);
        }

        private void IncrementCount(string[] _event)
        {
            string word = _event[1];
            if (!_wordFreqs.ContainsKey(word))
            {
                _wordFreqs[word] = 0;
            }
            _wordFreqs[word]++;
        }

        private void PrintFrequencies(string[] _)
        {
            var ordered = _wordFreqs.OrderByDescending(kvp => kvp.Value).Take(25);
            foreach (var pair in ordered)
            {
                Console.WriteLine("{0} - {1}", pair.Key, pair.Value);
            }
        }
    }

    class WordFrequencyApplication
    {
        public WordFrequencyApplication(EventManager eventManager)
        {
            eventManager.Subscribe("run", Run);
            eventManager.Subscribe("eof", Stop);
        }

        private void Run(string[] _event)
        {
            SixteenProg.EventBoard.Publish(new[] { "load", _event[1] });
            SixteenProg.EventBoard.Publish(new[] { "start" });
        }

        private void Stop(string[] _)
        {
            SixteenProg.EventBoard.Publish(new[] { "print" });
        }
    }

    class ZWordCounter
    {
        private int _zWordCount = 0;

        public ZWordCounter(EventManager eventManager)
        {
            eventManager.Subscribe("valid_word", CountZWords);
            eventManager.Subscribe("print_z_count", PrintZWordCount);
        }

        private void CountZWords(string[] _event)
        {
            if (_event[1].Contains("z"))
            {
                _zWordCount++;
            }
        }

        private void PrintZWordCount(string[] _)
        {
            Console.WriteLine("\nNumber of non-stop words containing 'z': {0}", _zWordCount);
        }
    }

    class SixteenProg
    {
        public static EventManager EventBoard = new EventManager();

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide an input file path as argument");
                return;
            }

            new DataStorage(EventBoard);
            new StopWordFilter(EventBoard);
            new WordFrequencyCounter(EventBoard);
            new WordFrequencyApplication(EventBoard);
            new ZWordCounter(EventBoard);

            EventBoard.Publish(new[] { "run", args[0] });
            EventBoard.Publish(new[] { "eof" });
            EventBoard.Publish(new[] { "print_z_count" });
        }
    }
}