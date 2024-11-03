const fs = require("fs");
const path = require("path");

class DataStorage {
  constructor() {
    this.words = [];
  }

  init(filePath) {
    // 22.1: Check args
    if (typeof filePath !== "string") {
      throw new TypeError("DataStorage.init: filePath must be a string");
    }
    if (filePath.trim() === "") {
      throw new Error("DataStorage.init: filePath cannot be empty");
    }
    // 22.1: Check args
    if (!fs.existsSync(filePath)) {
      throw new Error(`DataStorage.init: file ${filePath} does not exist`);
    }

    try {
      const data = fs
        .readFileSync(filePath, "utf8")
        .replace(/[^a-zA-Z]/g, " ")
        .toLowerCase();

      // 22.1: Validate intermediate results - error checking
      if (typeof data !== "string") {
        throw new TypeError("DataStorage.init: file content must be convertible to string");
      }

      this.words = data.split(/\s+/).filter((word) => word.length > 0);

      // 22.1: Validate processed results - error checking
      if (!Array.isArray(this.words)) {
        throw new TypeError("DataStorage.init: processed words must be an array");
      }
      if (this.words.length === 0) {
        throw new Error("DataStorage.init: file has no valid words after processing");
      }
      // 22.1: Validate array contents - check args
      if (!this.words.every(word => typeof word === "string")) {
        throw new TypeError("DataStorage.init: all processed words must be strings");
      }
    } catch (error) {
      // 22.1: ERR msg and go on...
      console.error(`Error in DataStorage.init for file "${filePath}":`, error.message);
      throw error;
    }
  }

  getWords() {
    // 22.1: Validate state before returning - check args
    if (!Array.isArray(this.words)) {
      throw new TypeError("DataStorage.getWords: words array is corrupted");
    }
    return this.words;
  }
}

class StopWords {
  constructor() {
    this.stopWords = new Set();
  }

  init() {
    const stopWordsPath = path.join(__dirname, "../stop_words.txt");
    
    // 22.1: Check file
    if (!fs.existsSync(stopWordsPath)) {
      throw new Error(`StopWords.init: stop words file not found at ${stopWordsPath}`);
    }

    try {
      const stopWordsContent = fs.readFileSync(stopWordsPath, "utf8");

      // 22.1: check args- file type
      if (typeof stopWordsContent !== "string") {
        throw new TypeError("StopWords.init: stop words file content must be string");
      }
      if (stopWordsContent.trim() === "") {
        throw new Error("StopWords.init: stop words file is empty");
      }

      this.stopWords = new Set(stopWordsContent.split(","));
      
      const alphabet = Array.from("abcdefghijklmnopqrstuvwxyz");
      this.stopWords = new Set([...this.stopWords, ...alphabet]);

      // 22.1: error chck
      if (!(this.stopWords instanceof Set)) {
        throw new TypeError("StopWords.init: stopWords must be a Set");
      }
      if (this.stopWords.size === 0) {
        throw new Error("StopWords.init: no stop words found after processing");
      }
    } catch (error) {
      // 22.1: error msg
      console.error("Error in StopWords.init:", error.message);
      throw error;
    }
  }

  isStopWord(word) {
    // 22.1: chekc arg
    if (typeof word !== "string") {
      throw new TypeError("StopWords.isStopWord: word must be a string");
    }
    if (!(this.stopWords instanceof Set)) {
      throw new TypeError("StopWords.isStopWord: stopWords Set is corrupted");
    }

    if (word.length < 2) {
      return true;
    }
    return this.stopWords.has(word);
  }
}

class WordFrequencies {
  constructor() {
    this.wordFrequencies = {};
  }

  incrementCount(word) {
    // 22.1: check arg
    if (typeof word !== "string") {
      throw new TypeError("WordFrequencies.incrementCount: word must be a string");
    }
    if (word.trim() === "") {
      throw new Error("WordFrequencies.incrementCount: word cannot be empty");
    }
    
    // 22.1: prevent err
    if (typeof this.wordFrequencies !== "object") {
      throw new TypeError("WordFrequencies.incrementCount: wordFrequencies is corrupted");
    }

    this.wordFrequencies[word] = (this.wordFrequencies[word] || 0) + 1;
  }

  top25() {
    // 22.1: check arg
    if (typeof this.wordFrequencies !== "object") {
      throw new TypeError("WordFrequencies.top25: wordFrequencies is corrupted");
    }

    const uniqueWords = Object.keys(this.wordFrequencies);
    
    // 22.1: check if data there
    if (uniqueWords.length === 0) {
      throw new Error("WordFrequencies.top25: no words have been counted");
    }
    if (uniqueWords.length < 25) {
      throw new Error(`WordFrequencies.top25: insufficient unique words (found ${uniqueWords.length}, need 25)`);
    }

    try {
      const sorted = Object.entries(this.wordFrequencies)
        .sort(([, a], [, b]) => b - a)
        .slice(0, 25);

      // 22.1: check err
      if (!Array.isArray(sorted) || sorted.length !== 25) {
        throw new Error("WordFrequencies.top25: sorting operation failed");
      }

      sorted.forEach(([word, freq]) => {
        // 22.1: check o/p data
        if (typeof word !== "string" || typeof freq !== "number") {
          throw new TypeError("WordFrequencies.top25: corrupted frequency data");
        }
        console.log(`${word} - ${freq}`);
      });
    } catch (error) {
      // 22.1: err msg 
      console.error("Error in WordFrequencies.top25:", error.message);
      throw error;
    }
  }
}

function main(filePath) {
  // 22.1: check args
  if (!filePath) {
    throw new Error("main: input file path is required");
  }
  if (typeof filePath !== "string") {
    throw new TypeError("main: file path must be a string");
  }
  if (!fs.existsSync(filePath)) {
    throw new Error(`main: file not found: ${filePath}`);
  }

  try {
    const dataStorage = new DataStorage();
    const stopWords = new StopWords();
    const wordFrequencies = new WordFrequencies();

    // 22.1: check ERR
    if (!(dataStorage instanceof DataStorage)) {
      throw new TypeError("main: failed to create DataStorage instance");
    }
    if (!(stopWords instanceof StopWords)) {
      throw new TypeError("main: failed to create StopWords instance");
    }
    if (!(wordFrequencies instanceof WordFrequencies)) {
      throw new TypeError("main: failed to create WordFrequencies instance");
    }

    dataStorage.init(filePath);
    stopWords.init();

    const words = dataStorage.getWords();
    // 22.1: check ERR
    if (!Array.isArray(words)) {
      throw new TypeError("main: corrupted word list");
    }

    words.forEach((word) => {
      // 22.1: checK ERR
      if (typeof word !== "string") {
        throw new TypeError("main: non-string word encountered");
      }
      if (!stopWords.isStopWord(word)) {
        wordFrequencies.incrementCount(word);
      }
    });

    wordFrequencies.top25();

  } catch (error) {
    // 22.1: ERR check and stack trace
    console.error("Fatal error in main:", error.message);
    console.error(error.stack);
    throw error;
  }
}

// 22.1: error - topLevel
try {
  main(process.argv[2]);
} catch (error) {
  console.error("Program terminated due to error:", error.message);
  process.exit(1);
}