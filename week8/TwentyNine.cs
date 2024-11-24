using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;

public abstract class ActiveWFObject
{
    protected BlockingCollection<object[]> queue;
    protected bool stopMe;
    protected Thread thread;

    public ActiveWFObject()
    {
        queue = new BlockingCollection<object[]>();
        stopMe = false;
        thread = new Thread(Run);
        thread.Start();
    }

    protected void Run()
    {
        while (!stopMe)
        {
            var message = queue.Take();
            Dispatch(message);
            if ((string)message[0] == "die")
                stopMe = true;
        }
    }

    protected abstract void Dispatch(object[] message);

    public void Send(object[] message)
    {
        queue.Add(message);
    }

    public void Join()
    {
        thread.Join();
    }
}

public class DataStorageManager : ActiveWFObject
{
    private string data = "";
    private StopWordManager stopWordManager;

    protected override void Dispatch(object[] message)
    {
        string command = (string)message[0];
        switch (command)
        {
            case "init":
                Init((string)message[1], (StopWordManager)message[2]);
                break;
            case "send_word_freqs":
                ProcessWords((WordFrequencyController)message[1]);
                break;
            default:
                stopWordManager.Send(message);
                break;
        }
    }

    private void Init(string pathToFile, StopWordManager swm)
    {
        stopWordManager = swm;
        data = File.ReadAllText(pathToFile);
        data = Regex.Replace(data, @"[\W_]+", " ").ToLower();
    }

    private void ProcessWords(WordFrequencyController recipient)
    {
        string[] words = data.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var word in words)
        {
            stopWordManager.Send(new object[] { "filter", word });
        }
        stopWordManager.Send(new object[] { "top25", recipient });
    }
}

public class StopWordManager : ActiveWFObject
{
    private HashSet<string> stopWords = new HashSet<string>();
    private WordFrequencyManager wordFreqsManager;

    protected override void Dispatch(object[] message)
    {
        string command = (string)message[0];
        switch (command)
        {
            case "init":
                Init((WordFrequencyManager)message[1]);
                break;
            case "filter":
                Filter((string)message[1]);
                break;
            default:
                wordFreqsManager.Send(message);
                break;
        }
    }

    private void Init(WordFrequencyManager wfm)
    {
        wordFreqsManager = wfm;
        var words = File.ReadAllText("../stop_words.txt").Split(',');
        foreach (var word in words)
            stopWords.Add(word);
        for (char c = 'a'; c <= 'z'; c++)
            stopWords.Add(c.ToString());
    }

    private void Filter(string word)
    {
        if (!stopWords.Contains(word))
            wordFreqsManager.Send(new object[] { "word", word });
    }
}

public class WordFrequencyManager : ActiveWFObject
{
    private Dictionary<string, int> wordFreqs = new Dictionary<string, int>();

    protected override void Dispatch(object[] message)
    {
        string command = (string)message[0];
        switch (command)
        {
            case "word":
                IncrementCount((string)message[1]);
                break;
            case "top25":
                Top25((WordFrequencyController)message[1]);
                break;
        }
    }

    private void IncrementCount(string word)
    {
        if (wordFreqs.ContainsKey(word))
            wordFreqs[word]++;
        else
            wordFreqs[word] = 1;
    }

    private void Top25(WordFrequencyController recipient)
    {
        var sortedFreqs = wordFreqs.OrderByDescending(kvp => kvp.Value)
                                  .Select(kvp => new KeyValuePair<string, int>(kvp.Key, kvp.Value))
                                  .ToList();
        recipient.Send(new object[] { "top25", sortedFreqs });
    }
}

public class WordFrequencyController : ActiveWFObject
{
    private DataStorageManager storageManager;

    protected override void Dispatch(object[] message)
    {
        string command = (string)message[0];
        switch (command)
        {
            case "run":
                Run((DataStorageManager)message[1]);
                break;
            case "top25":
                Display((List<KeyValuePair<string, int>>)message[1]);
                break;
            default:
                throw new Exception("Message not understood: " + command);
        }
    }

    private void Run(DataStorageManager sm)
    {
        storageManager = sm;
        storageManager.Send(new object[] { "send_word_freqs", this });
    }

    private void Display(List<KeyValuePair<string, int>> wordFreqs)
    {
        foreach (var pair in wordFreqs.Take(25))
        {
            Console.WriteLine($"{pair.Key} - {pair.Value}");
        }
        storageManager.Send(new object[] { "die" });
        stopMe = true;
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Please provide an input file path");
            return;
        }

        var wordFreqManager = new WordFrequencyManager();
        var stopWordManager = new StopWordManager();
        stopWordManager.Send(new object[] { "init", wordFreqManager });

        var storageManager = new DataStorageManager();
        storageManager.Send(new object[] { "init", args[0], stopWordManager });

        var wfController = new WordFrequencyController();
        wfController.Send(new object[] { "run", storageManager });

        // Wait for all active objects to finish
        wordFreqManager.Join();
        stopWordManager.Join();
        storageManager.Join();
        wfController.Join();
    }
}