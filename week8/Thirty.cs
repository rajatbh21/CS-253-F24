using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

class Program
{
    // Two data spaces
    static BlockingCollection<string> wordSpace = new BlockingCollection<string>();
    static BlockingCollection<Dictionary<string, int>> freqSpace = new BlockingCollection<Dictionary<string, int>>();
    static HashSet<string> stopwords;

    static void ProcessWords()
    {
        var wordFreqs = new Dictionary<string, int>();
        foreach (string word in wordSpace.GetConsumingEnumerable())
        {
            if (!stopwords.Contains(word))
            {
                if (wordFreqs.ContainsKey(word))
                    wordFreqs[word]++;
                else
                    wordFreqs[word] = 1;
            }
        }
        freqSpace.Add(wordFreqs);
    }

    static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Please provide an input file path");
            return;
        }

        stopwords = new HashSet<string>(
            File.ReadAllText("../stop_words.txt")
                .Split(',')
                .Select(w => w.Trim())
        );

        string text = File.ReadAllText(args[0]).ToLower();
        var regex = new Regex(@"[a-z]{2,}");

        foreach (Match match in regex.Matches(text))
        {
            wordSpace.Add(match.Value);
        }
        wordSpace.CompleteAdding();

        var workers = new List<Task>();
        for (int i = 0; i < 5; i++)
        {
            workers.Add(Task.Run(() => ProcessWords()));
        }
        Task.WaitAll(workers.ToArray());
        freqSpace.CompleteAdding();

        var wordFreqs = new Dictionary<string, int>();
        foreach (var freqs in freqSpace.GetConsumingEnumerable())
        {
            foreach (var pair in freqs)
            {
                if (wordFreqs.ContainsKey(pair.Key))
                    wordFreqs[pair.Key] += pair.Value;
                else
                    wordFreqs[pair.Key] = pair.Value;
            }
        }

        foreach (var pair in wordFreqs
            .OrderByDescending(x => x.Value)
            .Take(25))
        {
            Console.WriteLine($"{pair.Key} - {pair.Value}");
        }
    }
}