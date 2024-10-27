using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Thirteen
{
    class ThirteenProg
    {
        static int Main(string[] args)
        {
            // Check if user input file path, else exit
            if (args.Length == 0)
            {
                Console.WriteLine("Please enter input text file");
                return 1;
            }

            // Check if file exists, else exit
            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Could not find file {0}", args[0]);
                return 1;
            }

            var dataStorageDict = new Dictionary<string, object>
            {
                ["data"] = "",
                ["Init"] =  (Action<Dictionary<string, object>, string>)((dict, filePath) =>
                {
                    dict["data"] = Regex.Replace(File.ReadAllText(filePath), "[^a-zA-Z]", " ").ToLower();
                    dict["words"] = ((string)dict["data"]).Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
                }),
                ["words"] = new List<string>()
            };

            var stopWordsDict = new Dictionary<string, object>
            {
                ["stopWords"] = new string[] {},
                ["Init"] = (Action<Dictionary<string, object>>)( dict => 
                {
                    dict["stopWords"] = File.ReadAllText("../stop_words.txt").Split(",").ToList();
                }),
                ["IsStopWord"] = (Func<string, Dictionary<string, object>, bool>)((word, dict) => ((List<string>)dict["stopWords"]).Contains(word) || word.Length < 2)
            };

            var wordFrequenciesDict = new Dictionary<string, object>
            {
                ["wordFrequencies"] = new Dictionary<string, int>(),
                ["IncrementCount"] = (Action<string, Dictionary<string, object>>)((word, dict) =>
                {
                    //->13.3 Use 'this' instead of dict parameter
                    var freqs = (Dictionary<string, int>)this["wordFrequencies"];
                    if (freqs.ContainsKey(word))
                    {
                        freqs[word]++;
                    }
                    else
                    {
                        freqs.Add(word, 1);
                    }
                }),
                ["Sorted"] = (Func<Dictionary<string, object>, Dictionary<string, int>>)( dict => 
                {
                    //->13.3 Using 'this' to access own properties
                    var freqs = (Dictionary<string, int>)this["wordFrequencies"];
                    return freqs.OrderByDescending(a => a.Value)
                               .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }),
                //->13.2 Add new top25 method
                ["top25"] = (Action<Dictionary<string, object>>)(dict =>
                {
                    //->13.3 
                    var sorted = ((Func<Dictionary<string, object>, Dictionary<string, int>>)this["Sorted"])(this);
                    foreach (var kvp in sorted.Take(25))
                    {
                        Console.WriteLine("{0} - {1}", kvp.Key, kvp.Value);
                    }
                })
            };

            ((Action<Dictionary<string, object>, string>)dataStorageDict["Init"])(dataStorageDict, args[0]);
            ((Action<Dictionary<string, object>>)stopWordsDict["Init"])(stopWordsDict);

            foreach (var word in (List<string>)dataStorageDict["words"])
            {
                if (!(((Func<string, Dictionary<string, object>, bool>)stopWordsDict["IsStopWord"])(word, stopWordsDict)))
                {
                    ((Action<string, Dictionary<string, object>>)wordFrequenciesDict["IncrementCount"])(word, wordFrequenciesDict);
                }
            }

            //->13.2 Replace direct printing with call top25 method
            ((Action<Dictionary<string, object>>)wordFrequenciesDict["top25"])(wordFrequenciesDict);
            
            return 0;
        }
    }
}