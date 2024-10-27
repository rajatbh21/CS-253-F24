const fs = require("fs");
const path = require("path");

class DataStorage {
  constructor() {
    this.words = [];
  }

  //->13.3: 'this' to access words
  init(filePath) {
    try {
      const data = fs
        .readFileSync(filePath, "utf8")
        .replace(/[^a-zA-Z]/g, " ")
        .toLowerCase();
      this.words = data.split(/\s+/).filter((word) => word.length > 0);
      console.log(
        //"DataStorage count:",
        //this.words.length,
      ); //Debugging line
    } catch (error) {
     // console.error("Error reading or processing the input file:", error);
    }
  }
}

class StopWords {
  constructor() {
    this.stopWords = new Set();
  }

  //->13.3: Load stop words into 'this.stopWords'
  init() {
    try {
      const stopWordsContent = fs.readFileSync(
        path.join(__dirname, "../stop_words.txt"),
        "utf8",
      );
      this.stopWords = new Set(stopWordsContent.split(","));
      // console.log("StopWords initialized with count:", this.stopWords.size); // Debugging line
    } catch (error) {
      //  console.error("Error reading the stop words file:", error);
    }
  }

  isStopWord(word) {
    return this.stopWords.has(word) || word.length < 2;
  }
}

class WordFrequencies {
  constructor() {
    this.wordFrequencies = {};
  }

  //->13.3: Increment count using 'this' 
  incrementCount(word) {
    if (this.wordFrequencies[word]) {
      this.wordFrequencies[word]++;
    } else {
      this.wordFrequencies[word] = 1;
    }
  }

  //->13.2: top25 method 
  top25() {
    const sorted = Object.entries(this.wordFrequencies)
      .sort(([, a], [, b]) => b - a)
      .slice(0, 25);

    console.log("Top 25 words:"); // Debugging line
    sorted.forEach(([word, freq]) => console.log(`${word} - ${freq}`));
  }
}

function main(filePath) {
  if (!filePath) {
    console.log("Please enter input text file");
    return;
  }

  if (!fs.existsSync(filePath)) {
    console.log(`Could not find file ${filePath}`);
    return;
  }

  //->13.3
  const dataStorage = new DataStorage();
  const stopWords = new StopWords();
  const wordFrequencies = new WordFrequencies();

  dataStorage.init(filePath);
  stopWords.init();

  
  dataStorage.words.forEach((word) => {
    if (!stopWords.isStopWord(word)) {
      wordFrequencies.incrementCount(word);
    }
  });

  //->13.2: Call Top25
  wordFrequencies.top25();
}

//main call
main(process.argv[2]);
