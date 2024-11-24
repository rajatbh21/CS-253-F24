using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

public class Thirty
{
    private static readonly BlockingCollection<string> wordSpace = new BlockingCollection<string>();
    private static readonly BlockingCollection<Dictionary<string, int>> freqSpace = new BlockingCollection<Dictionary<string, int>>();
    private static HashSet<string> stopwords;

    private class WordProcessor
    {
        public void Run()
        {
            var wordFreqs = new Dictionary<string, int>();
            while (true)
            {
                string word;
                try
                {
                    if (!wordSpace.TryTake(out word, TimeSpan.FromSeconds(1)))
                    {
                        break;
                    }
                    
                    if (!stopwords.Contains(word))
                    {
                        if (!wordFreqs.ContainsKey(word))
                            wordFreqs[word] = 0;
                        wordFreqs[word]++;
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
            freqSpace.Add(wordFreqs);
        }
    }

    public static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Please provide input file path");
            Environment.Exit(1);
        }

        try
        {
            // Load stopwords
            stopwords = new HashSet<string>(
                File.ReadAllText("../stop_words.txt")
                    .Split(',')
                    .Select(w => w.Trim())
            );

            
            string content = File.ReadAllText(args[0]).ToLower();
            var pattern = new Regex(@"[a-z]{2,}");
            foreach (Match match in pattern.Matches(content))
            {
                wordSpace.Add(match.Value);
            }
            wordSpace.CompleteAdding();

            
            var workers = new List<Thread>();
            for (int i = 0; i < 5; i++)
            {
                var processor = new WordProcessor();
                var worker = new Thread(processor.Run);
                workers.Add(worker);
                worker.Start();
            }

            
            foreach (var worker in workers)
            {
                worker.Join();
            }

           
            var wordFreqs = new Dictionary<string, int>();
            while (!freqSpace.IsCompleted)
            {
                Dictionary<string, int> freqs;
                if (freqSpace.TryTake(out freqs))
                {
                    foreach (var pair in freqs)
                    {
                        if (!wordFreqs.ContainsKey(pair.Key))
                            wordFreqs[pair.Key] = 0;
                        wordFreqs[pair.Key] += pair.Value;
                    }
                }
            }

          
            foreach (var pair in wordFreqs
                .OrderByDescending(x => x.Value)
                .Take(25))
            {
                Console.WriteLine($"{pair.Key} - {pair.Value}");
            }
        }
        catch (IOException e)
        {
            Console.Error.WriteLine($"Error reading file: {e.Message}");
            Environment.Exit(1);
        }
    }
}