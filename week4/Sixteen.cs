using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sixteen
{
    class EventManager
    {
        private Dictionary<string, List<Action<string[]>>> _subscriptions = new();

        public void Subscribe(string eventType, Action<string[]> handler) =>
            _subscriptions.TryGetValue(eventType, out var handlers)
                ? handlers.Add(handler)
                : _subscriptions[eventType] = new List<Action<string[]>> { handler };

        public void Publish(string[] _event) =>
            _subscriptions.TryGetValue(_event[0], out var handlers)?.ForEach(h => h(_event));
    }

    class DataStorage
    {
        private string _data = "";
        public DataStorage(EventManager eventManager)
        {
            eventManager.Subscribe("load", Load);
            eventManager.Subscribe("start", ProduceWords);
        }

        private void Load(string[] _event) =>
            _data = Regex.Replace(File.ReadAllText(_event[1]), "[^a-zA-Z]", " ").ToLower();

        private void ProduceWords(string[] _) =>
            _data.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList().ForEach(word => SixteenProg.EventBoard.Publish(new[] { "word", word }));
    }

    class StopWordFilter
    {
        private HashSet<string> _stopWords;
        public StopWordFilter(EventManager eventManager)
        {
            eventManager.Subscribe("load", Load);
            eventManager.Subscribe("word", IsStopWord);
        }

        private void Load(string[] _) =>
            _stopWords = File.ReadAllText("../stop_words.txt").Split(',').ToHashSet();

        private void IsStopWord(string[] _event)
        {
            if (!_stopWords.Contains(_event[1]) && _event[1].Length > 1)
                SixteenProg.EventBoard.Publish(new[] { "valid_word", _event[1] });
        }
    }

    class WordFrequencyCounter
    {
        private Dictionary<string, int> _wordFreqs = new();
        public WordFrequencyCounter(EventManager eventManager)
        {
            eventManager.Subscribe("valid_word", IncrementCount);
            eventManager.Subscribe("print", PrintFrequencies);
        }

        private void IncrementCount(string[] _event) =>
            _wordFreqs[_event[1]] = _wordFreqs.GetValueOrDefault(_event[1], 0) + 1;

        private void PrintFrequencies(string[] _) =>
            _wordFreqs.OrderByDescending(kvp => kvp.Value).Take(25)
                      .ToList().ForEach(kvp => Console.WriteLine($"{kvp.Key} - {kvp.Value}"));
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

        private void Stop(string[] _) => SixteenProg.EventBoard.Publish(new[] { "print" });
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
                _zWordCount++;
        }

        private void PrintZWordCount(string[] _) =>
            Console.WriteLine($"\nNumber of non-stop words containing 'z': {_zWordCount}");
    }

    class SixteenProg
    {
        public static EventManager EventBoard = new();

        static void Main(string[] args)
        {
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
