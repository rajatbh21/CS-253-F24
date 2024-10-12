#!/bin/bash

# Function to check if a word is a stop word
is_stop_word() {
    local word=$1
    local stop_words=$(cat ../stop_words.txt)
    [[ $stop_words =~ (^|,)$word(,|$) ]] && return 0
    [[ $word =~ ^[a-z]$ ]] && return 0
    return 1
}

# Function to find a word in the frequency list
find_word() {
    local word=$1
    for i in "${!word_freqs[@]}"; do
        IFS=',' read -r w f <<< "${word_freqs[$i]}"
        if [ "$w" = "$word" ]; then
            echo $i
            return
        fi
    done
    echo -1
}

# Function to insert a word into the frequency list
insert_word() {
    local word=$1
    local freq=$2
    local index=$3
    
    word_freqs=("${word_freqs[@]:0:$index}" "$word,$freq" "${word_freqs[@]:$index}")
}

# Initialize empty array for word frequencies
declare -a word_freqs

# Process the input file
while IFS= read -r line || [ -n "$line" ]; do
    line=$(echo "$line" | tr '[:upper:]' '[:lower:]')
    word=""
    for (( i=0; i<${#line}; i++ )); do
        char="${line:$i:1}"
        if [[ $char =~ [[:alnum:]] ]]; then
            word+="$char"
        elif [ -n "$word" ]; then
            if ! is_stop_word "$word"; then
                index=$(find_word "$word")
                if [ $index -eq -1 ]; then
                    word_freqs+=("$word,1")
                else
                    IFS=',' read -r w f <<< "${word_freqs[$index]}"
                    ((f++))
                    word_freqs[$index]="$w,$f"
                    
                    # Reorder if necessary
                    for (( j=$index-1; j>=0; j-- )); do
                        IFS=',' read -r prev_w prev_f <<< "${word_freqs[$j]}"
                        if [ $f -gt $prev_f ]; then
                            word_freqs[$j+1]="${word_freqs[$j]}"
                            word_freqs[$j]="$w,$f"
                        else
                            break
                        fi
                    done
                fi
            fi
            word=""
        fi
    done
done < "$1"

# Print top 25 words
for i in "${word_freqs[@]:0:25}"; do
    IFS=',' read -r w f <<< "$i"
    echo "$w - $f"
done
