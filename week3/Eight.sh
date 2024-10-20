#!/bin/bash

RECURSION_LIMIT=5000  
STOPWORDS_FILE="../stop_words.txt"
MIN_WORD_LENGTH=3 

# Associative Array
declare -A stopwords
while IFS= read -r word; do
    stopwords["$word"]=1
done < "$STOPWORDS_FILE"


declare -A global_wordfreqs

count_words() {
    local words_chunk=("$@")
    local word

    
    if [ ${#words_chunk[@]} -eq 0 ]; then
        return
    fi

    for word in "${words_chunk[@]:0:$RECURSION_LIMIT}"; do

        word=$(echo "$word" | tr '[:upper:]' '[:lower:]' | tr -cd 'a-z')
        
     
        if [ ${#word} -lt $MIN_WORD_LENGTH ] || [ ${stopwords[$word]+_} ]; then
            continue
        fi

        ((global_wordfreqs[$word]++))
    done


    count_words "${words_chunk[@]:$RECURSION_LIMIT}"
}

process_file() {
    local filename="$1"

  
    mapfile -t words < <(cat "$filename" | tr '[:upper:]' '[:lower:]' | tr -sc 'a-z' '\n' | grep -E ".{$MIN_WORD_LENGTH,}") #mapfile may create issue, let's see

  
    count_words "${words[@]}"

   
    for word in "${!global_wordfreqs[@]}"; do
        echo "${global_wordfreqs[$word]} $word"
    done | sort -rn | head -n 25
}


if [ $# -eq 0 ]; then
    echo "Usage: $0 <filename>"
    exit 1
fi

process_file "$1"