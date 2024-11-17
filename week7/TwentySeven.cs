using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

class Program
{
    
    static (List<string> data, Func<List<string>> formula) allWords = (new List<string>(), null);
    static (HashSet<string> data, Func<HashSet<string>> formula) stopWords = (new HashSet<string>(), null);
    
    static (List<string> data, Func<List<string>> formula) nonStopWords = 
        (new List<string>(), 
         () => allWords.data
             .Select(w => stopWords.data.Contains(w) ? "" : w)
             .ToList());
    
    static (HashSet<string> data, Func<HashSet<string>> formula) uniqueWords = 
        (new HashSet<string>(), 
         () => new HashSet<string>(
             nonStopWords.data.Where(w => w != "")));
    
    static (List<int> data, Func<List<int>> formula) counts = 
        (new List<int>(), 
         () => uniqueWords.data
             .Select(w => nonStopWords.data.Count(word => word == w))
             .ToList());
    
    static (List<(string, int)> data, Func<List<(string, int)>> formula) sortedData = 
        (new List<(string, int)>(), 
         () => uniqueWords.data
             .Zip(counts.data, (w, c) => (w, c))
             .OrderByDescending(x => x.Item2)
             .ToList());
    
    
    static readonly List<object> allColumns = new()
    {
        allWords, stopWords, nonStopWords, uniqueWords, counts, sortedData
    };

    // The active procedure, matching Python's update()
    static void Update()
    {
        foreach (dynamic column in allColumns)
        {
            if (column.formula != null)
            {
                column.data = column.formula();
            }
        }
    }

    static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Please provide a file path");
            return;
        }

        // Load the fixed data into first 2 columns
        allWords.data = Regex.Matches(
            File.ReadAllText(args[0]).ToLower(), 
            @"[a-z]{2,}")
            .Select(m => m.Value)
            .ToList();

        stopWords.data = new HashSet<string>(
            File.ReadAllText("../stop_words.txt").Split(','));

       
        Update();

        
        foreach (var (word, count) in sortedData.data.Take(25))
        {
            Console.WriteLine($"{word} - {count}");
        }

        //27.2 part
        Console.WriteLine("\nEnter additional files (or EXIT to quit):");
        while (true)
        {
            string input = Console.ReadLine();
            if (input.Equals("EXIT", StringComparison.OrdinalIgnoreCase))
                break;

            if (File.Exists(input))
            {
                var newWords = Regex.Matches(
                    File.ReadAllText(input).ToLower(), 
                    @"[a-z]{2,}")
                    .Select(m => m.Value)
                    .ToList();
                
                allWords.data.AddRange(newWords);
                Update();

                foreach (var (word, count) in sortedData.data.Take(25))
                {
                    Console.WriteLine($"{word} - {count}");
                }
            }
            else
            {
                Console.WriteLine("File not found");
            }
        }
    }
}