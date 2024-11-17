using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    // 28.2: Add line streaming function
    public static IEnumerable<string> Lines(string filePath)
    {
        using (var reader = new StreamReader(filePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                yield return line;
            }
        }
    }

    public static IEnumerable<char> Characters(string filePath)
    {
        using (var reader = new StreamReader(filePath))
        {
            while (!reader.EndOfStream)
            {
                yield return (char)reader.Read();
            }
        }
    }

    // 28.2: Added line-based word processing
    public static IEnumerable<string> AllWordsFromLines(string filePath)
    {
        foreach (var line in Lines(filePath))
        {
            var words = line.Split(new[] { ' ', '\t', '\n', '\r', '.', ',', ';', ':', '!', '?', '-', '/', '\\', '(', ')', '[', ']', '{', '}', '\'', '"', '*', '@', '#', '$', '%', '^', '&', '+', '=' },
                                 StringSplitOptions.RemoveEmptyEntries)
                          .Select(w => w.ToLower())
                          .Where(w => w.All(char.IsLetter));
            
            foreach (var word in words)
            {
                yield return word;
            }
        }
    }

    public static IEnumerable<string> AllWords(string filePath)
    {
        var startChar = true;
        var word = "";
        
        foreach (var c in Characters(filePath))
        {
            if (startChar)
            {
                if (char.IsLetter(c)) 
                {
                    word = char.ToLower(c).ToString();
                    startChar = false;
                }
            }
            else
            {
                if (char.IsLetter(c))
                {
                    word += char.ToLower(c);
                }
                else
                {
                    startChar = true;
                    if (!string.IsNullOrEmpty(word))
                        yield return word;
                }
            }
        }
        // Return last word if file doesn't end with non-letter
        if (!string.IsNullOrEmpty(word))
            yield return word;
    }

    public static IEnumerable<string> NonStopWords(string filePath)
    {
        var stopWords = new HashSet<string>();
        using (var reader = new StreamReader("../stop_words.txt"))
        {
            foreach (var word in reader.ReadToEnd().Split(','))
            {
                stopWords.Add(word.Trim());
            }
        }
        
        // 28.2: Allow choosing between character or line based processing
        var wordGenerator = args.Length > 1 && args[1] == "--use-lines" 
            ? AllWordsFromLines(filePath) 
            : AllWords(filePath);
            
        foreach (var w in wordGenerator)
        {
            if (!stopWords.Contains(w) && w.Length > 1)
            {
                yield return w;
            }
        }
    }

    public static IEnumerable<Dictionary<string, int>> CountAndSort(string filePath)
    {
        var frequencies = new Dictionary<string, int>();
        var i = 0;
        
        foreach (var w in NonStopWords(filePath))
        {
            frequencies[w] = frequencies.ContainsKey(w) ? frequencies[w] + 1 : 1;
            if (i % 5000 == 0)
            {
                yield return new Dictionary<string, int>(
                    frequencies.OrderByDescending(a => a.Value));
            }
            i++;
        }
        yield return new Dictionary<string, int>(
            frequencies.OrderByDescending(a => a.Value));
    }

    static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Please provide a file path");
            Console.WriteLine("Optional: add --use-lines to use line-based processing");
            return;
        }

        var i = 1;
        foreach (var frequencies in CountAndSort(args[0]))
        {
            // 28.2: Added indication of which processing method is being used
            Console.WriteLine($"Window {i++} ================================");
            Console.WriteLine($"Using {(args.Length > 1 && args[1] == "--use-lines" ? "line" : "character")} based processing");
            foreach (var kvp in frequencies.Take(25))
            {
                Console.WriteLine($"{kvp.Key} - {kvp.Value}");
            }
        }
    }
}