using System;

class Four
{
    public static void Main(string[] args)
    {
        
        char[] stopWordsChars = new char[1024]; 
        int stopWordsLength = 0;
        int stopWordsCharIndex = 0;

        
        int stopWordsFileHandle = 0; 
        char currentChar;
        while ((currentChar = stopWordsFileHandle < 1024 ? "../stop_words.txt"[stopWordsFileHandle] : '\0') != '\0')
        {
            if (currentChar == ',')
            {
                stopWordsChars[stopWordsCharIndex] = '\0'; 
                stopWordsLength++;
                stopWordsCharIndex = 0;
            }
            else
            {
                stopWordsChars[stopWordsCharIndex++] = currentChar;
            }
            stopWordsFileHandle++;
        }

        
        for (char c = 'a'; c <= 'z'; c++)
        {
            stopWordsChars[stopWordsCharIndex++] = c;
            stopWordsChars[stopWordsCharIndex++] = '\0';
            stopWordsLength++;
        }

        
        char[] wordFreqsChars = new char[10240]; 
        int[][] wordFreqs = new int[1000][]; 
        for (int i = 0; i < wordFreqs.Length; i++)
        {
            wordFreqs[i] = new int[2];
        }
        int wordFreqsCount = 0;

        
        int fileHandle = 0; 
        char[] lineBuffer = new char[1024];
        int lineIndex = 0;
        int wordStart = -1;
        int globalCharIndex = 0;

        
        string inputFile = System.IO.File.ReadAllText(args[0]);

        while (fileHandle < inputFile.Length)
        {
            currentChar = inputFile[fileHandle];

            if (currentChar == '\n' || currentChar == '\r')
            {
                
                if (wordStart != -1)
                {
                    
                    bool isStopWord = false;
                    int stopWordIndex = 0;
                    while (stopWordIndex < stopWordsLength)
                    {
                        bool match = true;
                        int j = 0;
                        while (stopWordsChars[stopWordIndex + j] != '\0')
                        {
                            if (lineBuffer[wordStart + j] != stopWordsChars[stopWordIndex + j])
                            {
                                match = false;
                                break;
                            }
                            j++;
                        }

                        if (match && lineBuffer[wordStart + j] == '\0')
                        {
                            isStopWord = true;
                            break;
                        }

                       
                        while (stopWordsChars[stopWordIndex] != '\0')
                        {
                            stopWordIndex++;
                        }
                        stopWordIndex++;
                    }

                    if (!isStopWord)
                    {
                        
                        int wordFreqsIndex = wordFreqsCount * 100; 
                        int k = 0;
                        while (wordStart + k < lineIndex)
                        {
                            wordFreqsChars[wordFreqsIndex + k] = lineBuffer[wordStart + k];
                            k++;
                        }
                        wordFreqsChars[wordFreqsIndex + k] = '\0';

                        
                        bool found = false;
                        for (int i = 0; i < wordFreqsCount; i++)
                        {
                            int j = 0;
                            bool wordMatch = true;
                            while (true)
                            {
                                if (wordFreqsChars[wordFreqs[i][0] + j] != wordFreqsChars[wordFreqsIndex + j])
                                {
                                    wordMatch = false;
                                    break;
                                }
                                if (wordFreqsChars[wordFreqs[i][0] + j] == '\0')
                                    break;
                                j++;
                            }

                            if (wordMatch)
                            {
                                wordFreqs[i][1]++;
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            wordFreqs[wordFreqsCount][0] = wordFreqsIndex;
                            wordFreqs[wordFreqsCount][1] = 1;
                            wordFreqsCount++;
                        }
                    }
                }
                
                lineIndex = 0;
                wordStart = -1;
                fileHandle++;
                continue;
            }

            
            bool isAlphaNum = (currentChar >= 'a' && currentChar <= 'z') ||
                              (currentChar >= 'A' && currentChar <= 'Z') ||
                              (currentChar >= '0' && currentChar <= '9');

            if (wordStart == -1 && isAlphaNum)
            {
                
                wordStart = lineIndex;
            }

            if (wordStart != -1)
            {
               
                if (currentChar >= 'A' && currentChar <= 'Z')
                {
                    currentChar = (char)(currentChar + 32);
                }
                lineBuffer[lineIndex] = currentChar;
            }

            if (wordStart != -1 && !isAlphaNum)
            {
                
                bool isStopWord = false;
                int stopWordIndex = 0;
                while (stopWordIndex < stopWordsLength)
                {
                    bool match = true;
                    int j = 0;
                    while (stopWordsChars[stopWordIndex + j] != '\0')
                    {
                        if (lineBuffer[wordStart + j] != stopWordsChars[stopWordIndex + j])
                        {
                            match = false;
                            break;
                        }
                        j++;
                    }

                    if (match && lineBuffer[wordStart + j] == '\0')
                    {
                        isStopWord = true;
                        break;
                    }

                   
                    while (stopWordsChars[stopWordIndex] != '\0')
                    {
                        stopWordIndex++;
                    }
                    stopWordIndex++;
                }

                if (!isStopWord)
                {
                    
                    int wordFreqsIndex = wordFreqsCount * 100; // Giving space
                    int k = 0;
                    while (wordStart + k < lineIndex)
                    {
                        wordFreqsChars[wordFreqsIndex + k] = lineBuffer[wordStart + k];
                        k++;
                    }
                    wordFreqsChars[wordFreqsIndex + k] = '\0';

                   
                    bool found = false;
                    for (int i = 0; i < wordFreqsCount; i++)
                    {
                        int j = 0;
                        bool wordMatch = true;
                        while (true)
                        {
                            if (wordFreqsChars[wordFreqs[i][0] + j] != wordFreqsChars[wordFreqsIndex + j])
                            {
                                wordMatch = false;
                                break;
                            }
                            if (wordFreqsChars[wordFreqs[i][0] + j] == '\0')
                                break;
                            j++;
                        }

                        if (wordMatch)
                        {
                            wordFreqs[i][1]++;
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        wordFreqs[wordFreqsCount][0] = wordFreqsIndex;
                        wordFreqs[wordFreqsCount][1] = 1;
                        wordFreqsCount++;
                    }
                }

                wordStart = -1;
            }

            lineIndex++;
            fileHandle++;
            globalCharIndex++;
        }

        
        for (int i = 0; i < wordFreqsCount - 1; i++)
        {
            for (int j = 0; j < wordFreqsCount - i - 1; j++)
            {
                if (wordFreqs[j][1] < wordFreqs[j + 1][1])
                {
                    
                    int[] temp = wordFreqs[j];
                    wordFreqs[j] = wordFreqs[j + 1];
                    wordFreqs[j + 1] = temp;
                }
            }
        }

        
        int printLimit = wordFreqsCount < 25 ? wordFreqsCount : 25;
        for (int i = 0; i < printLimit; i++)
        {
           
            for (int j = wordFreqs[i][0]; wordFreqsChars[j] != '\0'; j++)
            {
                Console.Write(wordFreqsChars[j]);
            }
            Console.Write(" - ");
            Console.WriteLine(wordFreqs[i][1]);
        }
    }
}