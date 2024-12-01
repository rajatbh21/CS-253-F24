import sys
import numpy as np

def main(filename):
    leet_map = {
        'A': '4', 'B': '8', 'E': '3', 'G': '6', 'I': '1',
        'O': '0', 'Q': '9', 'T': '7', 'Z': '2'
    }
    
    # Load stop words
    try:
        with open("../stop_words.txt", "r") as f:
            stop_words = set(word.strip().upper() for word in f.read().strip().split(','))
    except FileNotFoundError:
        print("Error: stop_words.txt not found")
        sys.exit(1)
    
    
    try:
        characters = np.array([' '] + list(open(filename).read()) + [' '])
    except FileNotFoundError:
        print(f"Error: Input file '{filename}' not found")
        sys.exit(1)
    
    
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
    
    
    filtered_words = words[~np.isin(words, list(stop_words))]
    filtered_leet_words = leet_words[~np.isin(words, list(stop_words))]
    
    
    n = 2
    ngrams = np.array([' '.join(words[i:i + n]) for i in range(len(words) - n + 1)])
    leet_ngrams = np.array([' '.join(leet_words[i:i + n]) for i in range(len(leet_words) - n + 1)])
    
    filtered_ngrams = np.array([' '.join(filtered_words[i:i + n]) for i in range(len(filtered_words) - n + 1)])
    filtered_leet_ngrams = np.array([' '.join(filtered_leet_words[i:i + n]) for i in range(len(filtered_leet_words) - n + 1)])
    
    
    unique_ngrams, counts = np.unique(ngrams, return_counts=True)
    leet_dict = dict(zip(ngrams, leet_ngrams))
    
    filtered_unique_ngrams, filtered_counts = np.unique(filtered_ngrams, return_counts=True)
    filtered_leet_dict = dict(zip(filtered_ngrams, filtered_leet_ngrams))
    
    
    sorted_indices = np.argsort(counts)[::-1]
    top_5_ngrams = unique_ngrams[sorted_indices][:5]
    top_5_counts = counts[sorted_indices][:5]
    
    filtered_sorted_indices = np.argsort(filtered_counts)[::-1]
    filtered_top_5_ngrams = filtered_unique_ngrams[filtered_sorted_indices][:5]
    filtered_top_5_counts = filtered_counts[filtered_sorted_indices][:5]
    
    
    print("Top 5 2-grams (including stop words):")
    for ngram, count in zip(top_5_ngrams, top_5_counts):
        leet_version = leet_dict[ngram]
        print(f"{ngram}: {leet_version}: {count}")
    
    print("\nTop 5 2-grams (excluding stop words):")
    for ngram, count in zip(filtered_top_5_ngrams, filtered_top_5_counts):
        leet_version = filtered_leet_dict[ngram]
        print(f"{ngram}: {leet_version}: {count}")

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Error: Not enough arguments")
        sys.exit(1)
    
    main(sys.argv[1])
