import sys
import numpy as np

def main(filename):
    # Leet mapping 
    leet_map = {
        'A': '4', 'B': '8', 'E': '3', 'G': '6', 'I': '1',
        'O': '0', 'Q': '9', 'T': '7', 'Z': '2'
    }
    
    
    characters = np.array([' '] + list(open(filename).read()) + [' '])
    
    
    characters[~np.char.isalpha(characters)] = ' '
    characters = np.char.upper(characters)
    
    
    leet_characters = characters.copy()
    for letter, replacement in leet_map.items():
        leet_characters[leet_characters == letter] = replacement
    
    
    space_indices = np.where(characters == ' ')[0]
    doubled_indices = np.repeat(space_indices, 2)
    word_ranges = np.reshape(doubled_indices[1:-1], (-1, 2))
    
   
    word_ranges = word_ranges[np.where(word_ranges[:, 1] - word_ranges[:, 0] > 2)]
    
    
    words = np.array([
        ''.join(characters[start:end]).strip() 
        for start, end in word_ranges
    ])
    leet_words = np.array([
        ''.join(leet_characters[start:end]).strip()
        for start, end in word_ranges
    ])
    
    
    n = 2
    ngrams = np.array([' '.join(words[i:i + n]) for i in range(len(words) - n + 1)])
    leet_ngrams = np.array([' '.join(leet_words[i:i + n]) for i in range(len(leet_words) - n + 1)])
    

    unique_ngrams, counts = np.unique(ngrams, return_counts=True)
    leet_dict = dict(zip(ngrams, leet_ngrams))
    
    
    sorted_indices = np.argsort(counts)[::-1]
    top_5_ngrams = unique_ngrams[sorted_indices][:5]
    top_5_counts = counts[sorted_indices][:5]
    
    print("Top 5 2-grams:")
    for ngram, count in zip(top_5_ngrams, top_5_counts):
        leet_version = leet_dict[ngram]
        print(f"{ngram}: {leet_version}: {count}")


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("not enough arguments")
        sys.exit(1)
    
    main(sys.argv[1])
