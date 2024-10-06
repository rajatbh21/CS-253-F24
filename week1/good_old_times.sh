#!/bin/bash

is_stop_word() { grep -qw "$1" "../stop_words.txt"; }

process_input_file() {
    local word_freq_file="word_freqs.txt"
    > "$word_freq_file"

    tr '[:upper:]' '[:lower:]' < "$1" | tr -cs '[:alnum:]' '\n' | 
    while read -r word; do
        if [[ ${#word} -ge 2 ]] && ! is_stop_word "$word"; then
            if grep -q "^$word " "$word_freq_file"; then
                sed -i "/^$word /s/ [0-9]*/ $(($(grep "^$word " "$word_freq_file" | awk '{print $2}') + 1))/" "$word_freq_file"
            else
                echo "$word 1" >> "$word_freq_file"
            fi
        fi
    done
}

[[ $# -ne 1 ]] && { echo "Usage: $0 <input_file>"; exit 1; }

process_input_file "$1"
sort -k2 -nr word_freqs.txt | head -n 25 | awk '{print $1 " - " $2}'