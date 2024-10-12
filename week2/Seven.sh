#!/bin/bash

stops=$(tr ',' '\n' < ../stop_words.txt; echo {a..z})
words=$(tr -cs '[:alpha:]' '\n' < "$1" | tr '[:upper:]' '[:lower:]' | grep -vwFf <(echo "$stops"))
echo "$words" | sort | uniq | while read -r word; do
    count=$(echo "$words" | grep -c "^$word$")
    echo "$count $word"
done | sort -rn | head -25 | while read -r count word; do
    echo "$word - $count"
done
