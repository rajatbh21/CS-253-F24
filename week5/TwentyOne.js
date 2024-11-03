const fs = require("fs");
const path = require("path");

class DataStorage {
  constructor() {
    this.words = [];
  }

  init(filePath) {
    // 21.1: Check args
    if (typeof filePath !== "string" || filePath.length === 0) {
      console.log("Invalid file path provided.");
      return;
    }

    try {
      const data = fs
        .readFileSync(filePath, "utf8")
        .replace(/[^a-zA-Z]/g, " ")
        .toLowerCase();
      this.words = data.split(/\s+/).filter((word) => word.length > 0);
    } catch (error) {
      // 21.1: Handle error
      console.error("Error reading or processing the input file:", error.message);
      this.words = [];
    }
  }
}

class StopWords {
  constructor() {
    this.stopWords = new Set();
  }

  init() {
    try {
      const stopWordsContent = fs.readFileSync(
        path.join(__dirname, "../stop_words.txt"),
        "utf8",
      );
      this.stopWords = new Set(stopWordsContent.split(","));
      // 21.1: Stop words 
      this.stopWords = new Set([...this.stopWords, ..."abcdefghijklmnopqrstuvwxyz"]);
    } catch (error) {
      // 21.1: Handle error
      console.error("Error reading the stop words file:", error.message);
      this.stopWords = new Set();
    }
  }

  isStopWord(word) {
    // 21.1: Check args
    if (typeof word !== "string" || word.length === 0) {
      return true;
    }
    return this.stopWords.has(word) || word.length < 2;
  }
}

class WordFrequencies {
  constructor() {
    this.wordFrequencies = {};
  }

  incrementCount(word) {
    // 21.1: Check args
    if (typeof word !== "string" || word.length === 0) {
      console.log("Invalid word encountered in incrementCount.");
      return;
    }

    if (this.wordFrequencies[word]) {
      this.wordFrequencies[word]++;
    } else {
      this.wordFrequencies[word] = 1;
    }
  }

  top25() {
    // 21.1: Check args
    if (Object.keys(this.wordFrequencies).length === 0) {
      console.log("No words to display.");
      return;
    }

    const sorted = Object.entries(this.wordFrequencies)
      .sort(([, a], [, b]) => b - a)
      .slice(0, 25);

    console.log("Top 25 words:");
    sorted.forEach(([word, freq]) => console.log(`${word} - ${freq}`));
  }
}

function main(filePath) {
  // 21.1: Check args
  if (!filePath || typeof filePath !== "string") {
    console.log("Please enter a valid input text file.");
    return;
  }

  // 21.1: Input check - early return
  if (!fs.existsSync(filePath)) {
    console.log(`Could not find file ${filePath}`);
    return;
  }

  const dataStorage = new DataStorage();
  const stopWords = new StopWords();
  const wordFrequencies = new WordFrequencies();

  dataStorage.init(filePath);
  stopWords.init();

  // 21.1: State validation 
  if (dataStorage.words.length === 0) {
    console.log("No words were extracted from the file.");
    return;
  }

  dataStorage.words.forEach((word) => {
    if (!stopWords.isStopWord(word)) {
      wordFrequencies.incrementCount(word);
    }
  });

  wordFrequencies.top25();
}

// main call
main(process.argv[2]);