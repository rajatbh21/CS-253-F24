class WordFrequencyFramework {
    constructor() {
        this._loadEventHandlers = [];
        this._doWorkEventHandlers = [];
        this._endEventHandlers = [];
    }

    registerForLoadEvent(handler) {
        this._loadEventHandlers.push(handler);
    }

    registerForDoWorkEvent(handler) {
        this._doWorkEventHandlers.push(handler);
    }

    registerForEndEvent(handler) {
        this._endEventHandlers.push(handler);
    }

    run(pathToFile) {
        this._loadEventHandlers.forEach(handler => handler(pathToFile));
        this._doWorkEventHandlers.forEach(handler => handler());
        this._endEventHandlers.forEach(handler => handler());
    }
}

class DataStorage {
    constructor(wfApp, stopWordFilter) {
        this._stopWordFilter = stopWordFilter;
        this._data = '';
        this._wordEventHandlers = [];
        wfApp.registerForLoadEvent(this._load.bind(this));
        wfApp.registerForDoWorkEvent(this._produceWords.bind(this));
    }

    _load(pathToFile) {
        const fs = require('fs');
        this._data = fs.readFileSync(pathToFile, 'utf8');
        const pattern = /[\W_]+/g;
        this._data = this._data.replace(pattern, ' ').toLowerCase();
    }

    _produceWords() {
        const words = this._data.split(/\s+/);
        words.forEach(word => {
            if (!this._stopWordFilter.isStopWord(word)) {
                this._wordEventHandlers.forEach(handler => handler(word));
            }
        });
    }

    registerForWordEvent(handler) {
        this._wordEventHandlers.push(handler);
    }
}

class StopWordFilter {
    constructor(wfApp) {
        this._stopWords = [];
        wfApp.registerForLoadEvent(this._load.bind(this));
    }

    _load() {
        const fs = require('fs');
        this._stopWords = fs.readFileSync('../stop_words.txt', 'utf8').split(',');
        // Add single-letter words
        this._stopWords.push(...'abcdefghijklmnopqrstuvwxyz'.split(''));
    }

    isStopWord(word) {
        return this._stopWords.includes(word);
    }
}

class WordFrequencyCounter {
    constructor(wfApp, dataStorage) {
        this._wordFreqs = {};
        dataStorage.registerForWordEvent(this._incrementCount.bind(this));
        wfApp.registerForEndEvent(this._printFreqs.bind(this));
    }

    _incrementCount(word) {
        this._wordFreqs[word] = (this._wordFreqs[word] || 0) + 1;
    }

    _printFreqs() {
        const sortedWordFreqs = Object.entries(this._wordFreqs)
            .sort((a, b) => b[1] - a[1])
            .slice(0, 25);
        
        sortedWordFreqs.forEach(([word, count]) => {
            console.log(`${word} - ${count}`);
        });
    }
}

// For 'z'
class ZWordCounter {
    constructor(wfApp, dataStorage) {
        this._zWordCount = 0;
        dataStorage.registerForWordEvent(this._checkZWord.bind(this));
        wfApp.registerForEndEvent(this._printZWordCount.bind(this));
    }

    _checkZWord(word) {
        if (word.includes('z')) {
            this._zWordCount++;
        }
    }

    _printZWordCount() {
        console.log(`\nNumber of non-stop words with 'z': ${this._zWordCount}`);
    }
}

// Main 
const wfApp = new WordFrequencyFramework();
const stopWordFilter = new StopWordFilter(wfApp);
const dataStorage = new DataStorage(wfApp, stopWordFilter);
const wordFreqCounter = new WordFrequencyCounter(wfApp, dataStorage);
const zWordCounter = new ZWordCounter(wfApp, dataStorage);


const pathToFile = process.argv[2];
wfApp.run(pathToFile);