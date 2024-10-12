#!/bin/bash

#Shared mutable data
declare -a data
declare -a words
declare -A word_freqs

# Procedures
read_file() {
    local path_to_file="$1"
    mapfile -t data < "$path_to_file"
}

filter_chars_and_normalize() {
    local i
    for i in "${!data[@]}"; do
        data[i]=$(echo "${data[i]}" | tr '[:upper:]' '[:lower:]' | tr -c '[:alnum:]' ' ')
    done
}

scan() {
    local data_str
    data_str=$(printf '%s\n' "${data[@]}")
    readarray -t words <<< "$(echo "$data_str" | tr ' ' '\n')"
}

remove_stop_words() {
    local stop_words
    mapfile -t stop_words < <(tr ',' '\n' < ../stop_words.txt)
    stop_words+=({a..z})
    words=($(printf '%s\n' "${words[@]}" | grep -vwFf <(printf '%s\n' "${stop_words[@]}")))
}

frequencies() {
    local w
    for w in "${words[@]}"; do
        if [[ -v "word_freqs[$w]" ]]; then
            ((word_freqs[$w]++))
        else
            word_freqs[$w]=1
        fi
    done
}

sort_frequencies() {
    for w in "${!word_freqs[@]}"; do
        echo "${word_freqs[$w]} $w"
    done | sort -rn | head -n 25 | while read -r count word; do
        echo "$word - $count"
    done
}

# main
read_file "$1"
filter_chars_and_normalize
scan
remove_stop_words
frequencies
sort_frequencies
