using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

public class SeventeenProg
{
    public static void Main(string[] args)
    {
        // Originally
        var controller = new WordFrequencyController(args[0]);
        controller.Run();
        
        // Reflection based inspect
        Console.WriteLine("\nEnter a class name to inspect (e.g. DataStorageManager): ");
        string className = Console.ReadLine();
        
        try
        {
            InspectClass(className);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Class not found: {className}");
        }
    }
    
    public static void InspectClass(string className)
    {
        // Load class - reflection 
        Type type = Type.GetType(className) ?? 
                   Assembly.GetExecutingAssembly().GetTypes()
                   .FirstOrDefault(t => t.Name == className);
                   
        if (type == null)
        {
            throw new Exception($"Class {className} not found");
        }
        
        Console.WriteLine($"\n=== Class Information for {className} ===");
        
       
        Console.WriteLine("\nFields:");
        foreach (var field in type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
        {
            Console.WriteLine($"- {field.FieldType.Name} {field.Name}");
        }
        
       
        Console.WriteLine("\nMethods:");
        foreach (var method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            Console.WriteLine($"- {method.Name}");
        }
        
       
        Console.WriteLine("\nSuperclasses:");
        Type baseType = type.BaseType;
        while (baseType != null)
        {
            Console.WriteLine($"- {baseType.Name}");
            baseType = baseType.BaseType;
        }
        
       
        Console.WriteLine("\nInterfaces:");
        foreach (var iface in type.GetInterfaces())
        {
            Console.WriteLine($"- {iface.Name}");
        }
    }
}

public abstract class TFExercise
{
    public virtual string GetInfo()
    {
        return this.GetType().Name;
    }
}

public class WordFrequencyController : TFExercise
{
    private readonly DataStorageManager _storageManager;
    private readonly StopWordManager _stopWordManager;
    private readonly WordFrequencyManager _wordFreqManager;
    
    public WordFrequencyController(string pathToFile)
    {
        _storageManager = new DataStorageManager(pathToFile);
        _stopWordManager = new StopWordManager();
        _wordFreqManager = new WordFrequencyManager();
    }
    
    public void Run()
    {
        try
        {
            // Get types using reflection
            Type storageType = _storageManager.GetType();
            Type stopWordType = _stopWordManager.GetType();
            Type wordFreqType = _wordFreqManager.GetType();
            
            // Get methods using reflection
            MethodInfo getWordsMethod = storageType.GetMethod("GetWords");
            MethodInfo isStopWordMethod = stopWordType.GetMethod("IsStopWord");
            MethodInfo incrementCountMethod = wordFreqType.GetMethod("IncrementCount");
            MethodInfo sortedMethod = wordFreqType.GetMethod("Sorted");
            
            // word freq.
            var words = (IEnumerable<string>)getWordsMethod.Invoke(_storageManager, null);
            
            foreach (string word in words)
            {
                bool isStopWord = (bool)isStopWordMethod.Invoke(_stopWordManager, new object[] { word });
                if (!isStopWord)
                {
                    incrementCountMethod.Invoke(_wordFreqManager, new object[] { word });
                }
            }
            
            var pairs = (IEnumerable<WordFrequencyPair>)sortedMethod.Invoke(_wordFreqManager, null);
            
            int numWordsPrinted = 0;
            foreach (var pair in pairs)
            {
                Console.WriteLine($"{pair.Word} - {pair.Frequency}");
                numWordsPrinted++;
                if (numWordsPrinted >= 25) break;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error during reflection: {e.Message}");
            Console.WriteLine(e.StackTrace);
        }
    }
}

public class DataStorageManager : TFExercise
{
    private readonly List<string> _words;
    
    public DataStorageManager(string pathToFile)
    {
        _words = new List<string>();
        
        string text = File.ReadAllText(pathToFile);
        var matches = Regex.Matches(text, @"\b\w+\b");
        foreach (Match match in matches)
        {
            _words.Add(match.Value.ToLower());
        }
    }
    
    public IEnumerable<string> GetWords()
    {
        return _words;
    }
    
    public override string GetInfo()
    {
        return $"{base.GetInfo()}: major data struct {_words.GetType().Name}";
    }
}

public class StopWordManager : TFExercise
{
    private readonly HashSet<string> _stopWords;
    
    public StopWordManager()
    {
        _stopWords = new HashSet<string>();
        
        string[] words = File.ReadAllText("../stop_words.txt").Split(',');
        foreach (string word in words)
        {
            _stopWords.Add(word.Trim());
        }
        
        // Add single letter words
        for (char c = 'a'; c <= 'z'; c++)
        {
            _stopWords.Add(c.ToString());
        }
    }
    
    public bool IsStopWord(string word)
    {
        return _stopWords.Contains(word);
    }
    
    public override string GetInfo()
    {
        return $"{base.GetInfo()}: major data struct {_stopWords.GetType().Name}";
    }
}

public class WordFrequencyManager : TFExercise
{
    private readonly Dictionary<string, int> _wordFreqs;
    
    public WordFrequencyManager()
    {
        _wordFreqs = new Dictionary<string, int>();
    }
    
    public void IncrementCount(string word)
    {
        if (_wordFreqs.ContainsKey(word))
        {
            _wordFreqs[word]++;
        }
        else
        {
            _wordFreqs[word] = 1;
        }
    }
    
    public IEnumerable<WordFrequencyPair> Sorted()
    {
        return _wordFreqs
            .Select(kvp => new WordFrequencyPair(kvp.Key, kvp.Value))
            .OrderByDescending(pair => pair.Frequency)
            .ToList();
    }
    
    public override string GetInfo()
    {
        return $"{base.GetInfo()}: major data struct {_wordFreqs.GetType().Name}";
    }
}

public class WordFrequencyPair : IComparable<WordFrequencyPair>
{
    public string Word { get; }
    public int Frequency { get; }
    
    public WordFrequencyPair(string word, int frequency)
    {
        Word = word;
        Frequency = frequency;
    }
    
    public int CompareTo(WordFrequencyPair other)
    {
        return this.Frequency.CompareTo(other.Frequency);
    }
}