using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public class ThirtyTwo
{
    private static readonly string GROUP_AE = "abcde";
    private static readonly string GROUP_FJ = "fghij";
    private static readonly string GROUP_KO = "klmno";
    private static readonly string GROUP_PT = "pqrst";
    private static readonly string GROUP_UZ = "uvwxyz";

    private static HashSet<string> stopWords;

    static List<string> Partition(string dataStr, int nlines)
    {
        string[] lines = dataStr.Split('\n');
        var partitions = new List<string>();

        for (int i = 0; i < lines.Length; i += nlines)
        {
            int end = Math.Min(i + nlines, lines.Length);
            partitions.Add(string.Join("\n", lines.Skip(i).Take(end - i)));
        }
        return partitions;
    }

    static List<KeyValuePair<string, int>> SplitWords(string dataStr)
    {
        var pattern = new Regex(@"[\W_]+");
        string[] words = pattern.Replace(dataStr, " ")
                              .ToLower()
                              .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        return words.Where(w => !string.IsNullOrEmpty(w) && !stopWords.Contains(w))
                   .Select(w => new KeyValuePair<string, int>(w, 1))
                   .ToList();
    }

    static Dictionary<string, List<KeyValuePair<string, int>>> Regroup(List<List<KeyValuePair<string, int>>> pairsList)
    {
        var groupedWords = new Dictionary<string, List<KeyValuePair<string, int>>>
        {
            ["a-e"] = new List<KeyValuePair<string, int>>(),
            ["f-j"] = new List<KeyValuePair<string, int>>(),
            ["k-o"] = new List<KeyValuePair<string, int>>(),
            ["p-t"] = new List<KeyValuePair<string, int>>(),
            ["u-z"] = new List<KeyValuePair<string, int>>()
        };

        foreach (var pairs in pairsList)
        {
            foreach (var pair in pairs)
            {
                string word = pair.Key;
                if (string.IsNullOrEmpty(word)) continue;

                char firstChar = word[0];
                string group = GetWordGroup(firstChar);
                groupedWords[group].Add(pair);
            }
        }
        return groupedWords;
    }

    private static string GetWordGroup(char c)
    {
        if (GROUP_AE.IndexOf(c) >= 0) return "a-e";
        if (GROUP_FJ.IndexOf(c) >= 0) return "f-j";
        if (GROUP_KO.IndexOf(c) >= 0) return "k-o";
        if (GROUP_PT.IndexOf(c) >= 0) return "p-t";
        return "u-z";
    }

    static List<KeyValuePair<string, int>> ProcessGroup(List<KeyValuePair<string, int>> groupPairs)
    {
        return groupPairs.GroupBy(pair => pair.Key)
                        .Select(group => new KeyValuePair<string, int>(
                            group.Key,
                            group.Sum(pair => pair.Value)
                        ))
                        .ToList();
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
            stopWords = new HashSet<string>(
                File.ReadAllText("../stop_words.txt")
                    .Split(',')
                    .Select(w => w.Trim())
            );
            stopWords.UnionWith(Enumerable.Range('a', 26).Select(c => ((char)c).ToString()));

            string content = File.ReadAllText(args[0]);
            List<string> partitions = Partition(content, 200);

            var splits = partitions.Select(SplitWords).ToList();

            var groupedWords = Regroup(splits);

            var allWordFreqs = new List<KeyValuePair<string, int>>();
            foreach (var group in groupedWords)
            {
                allWordFreqs.AddRange(ProcessGroup(group.Value));
            }

            allWordFreqs.OrderByDescending(pair => pair.Value)
                       .Take(25)
                       .ToList()
                       .ForEach(pair => Console.WriteLine($"{pair.Key} - {pair.Value}"));

        }
        catch (IOException e)
        {
            Console.Error.WriteLine($"Error reading file: {e.Message}");
            Environment.Exit(1);
        }
    }
}