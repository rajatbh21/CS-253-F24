const fs = require('fs');
const path = require('path');


const extract_words = (obj, path_to_file) => {
    obj.data = fs.readFileSync(path_to_file, 'utf8')
        .replace(/[^\w\s]/g, ' ')
        .toLowerCase()
        .split(/\s+/)
        .filter(word => word.length > 0);
};

const load_stop_words = (obj) => {
    const stopWordsPath = path.join(__dirname, '../stop_words.txt');
    obj.stop_words = fs.readFileSync(stopWordsPath, 'utf8')
        .split(',')
        .concat(Array.from('abcdefghijklmnopqrstuvwxyz'));
};

const increment_count = (obj, w) => {
    obj.freqs[w] = (obj.freqs[w] || 0) + 1;
};

// Prototype objects
const data_storage_obj = {
    data: [],
    // 13.3: 'this' used to refer to the object itself
    init: function(path_to_file) { 
        extract_words(this, path_to_file); 
    },
    // 13.3: 'this' used to access the object's own data
    words: function() { 
        return this.data; 
    }
};

const stop_words_obj = {
    stop_words: [],
    // 13.3: 'this' used to refer to the object itself
    init: function() { 
        load_stop_words(this); 
    },
    // 13.3: 'this' useed to access the object's own stop_words
    is_stop_word: function(word) { 
        return this.stop_words.includes(word); 
    }
};

const word_freqs_obj = {
    freqs: {},
    increment_count: function(w) { 
        increment_count(this, w); 
    },
    sorted: function() { 
        return Object.entries(this.freqs)
            .sort((a, b) => b[1] - a[1]); 
    },
    // 13.2:Method sorting freq. and prints
    top25: function() {
        const top_25 = this.sorted().slice(0, 25);
        top_25.forEach(([word, count]) => {
            console.log(`${word} - ${count}`);
        });
    }
};


function main() {
    const path_to_file = process.argv[2];
    
    data_storage_obj.init(path_to_file);
    stop_words_obj.init();
    
    for (const w of data_storage_obj.words()) {
        if (!stop_words_obj.is_stop_word(w)) {
            word_freqs_obj.increment_count(w);
        }
    }
    
    // 13.2: Call top25 method
    word_freqs_obj.top25();
}

main();